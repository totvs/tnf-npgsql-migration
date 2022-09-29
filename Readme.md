# Migração de provider para banco de dados PostgreSql: Devart DotConnect para NpgSql

Devido a muitos problemas e atrasos enfrentados com fornecedor Devart, chegamos a conclusão que não iremos mais usar e encorajar os times a usar o provider DotConnect.

De agora em diante passamos a usar e recomendar o uso do provider de código aberto da Npgsql.

Neste repositório disponibilizamos uma aplicação que vai servir de exemplo de migração, e também o tutorial a ser seguido no processo.

## O processo de migração

Vamos entender o que é necessário para migrar do provider Devart para Npgsql.

> Atenção! Esse processo deve ser feito enquanto a sua aplicação ainda está no framework 3.1 (`netcoreapp3.1`).

O primeiro passo é alterar os registros dos DbContext da aplicação, mudando a chamada de `UsePostgreSql` para `UseNgpsql`.

```c#
services.AddTnfDbContext<BlogDbContext, PostgreSqlBlogDbContext>(conf =>
{
    //conf.DbContextOptions.UsePostgreSql(conf.ConnectionString);
    conf.DbContextOptions.UseNpgsql(conf.ConnectionString);
});
```

Porém não é tão simples assim. As migrações da sua aplicação são feitas especificamente para o provider da Devart. Isso não seria um problema se as migrações do passado nunca tivesse que ser executadas novamente, mas isso não é uma verdade.

Durante o desenvolvimento da aplicação os desenvolvedores costumam ter um ou mais bancos de dados da mesma aplicação em seu ambiente local, e para isso precisam poder executar todas as migrações da aplicação.

Outro caso é para aplicações multitenant com isolamento por schema. Nessas aplicações toda vez que um tenant novo é provisionado, é necessário criar o schema e rodar todas a migrações apontando para o tenant.

Por esses motivos temos a necessidade converter as migrações.

### Como funcionam as migrações no EntityFramework Core

Antes de começarmos a explicar como será o processo de conversão das migrações, vamos investir um tempo para entender como as migrações do EF Core funcionam. Assim ficará mais fácil entender o porque de cada parte do processo.

Quanto executamos o primeiro comando `Add-Migration` para um DbContext, o EF Core cria três arquivos. Um aquivo com o nome da migração, um outro arquivo com nome migração mais o sufixo `.Designer`, e um arquivo com o nome do DbContext mais o sufixo `ModelSnapshot`.

Exemplo:
```
-Migrations
    -20220825184314_CreateDatabase.cs
    -20220825184314_CreateDatabase.Designer.cs
    -MyDbContextModelSnapshot.cs
```

No exemplo, o arquivo `20220825184314_CreateDatabase.cs` contém o código que é usado para gerar o script SQL de migração, o arquivo `20220825184314_CreateDatabase.Designer.cs` contém o snapshot com o estado da model no momento que a migração foi criada, e o arquivo `MyDbContextModelSnapshot.cs` contém o snapshot com o último estado da model, ou seja o estado da model quando o a última migration foi adicionada, que nesse caso é a `20220825184314_CreateDatabase`. Se olharmos a definição da model nos arquivos `20220825184314_CreateDatabase.Designer.cs` e `MyDbContextModelSnapshot.cs`, veremos que a definição é a mesma.

Se nesse exemplo executarmos novamente o comando `Add-Migration`, vamos acabar com o seguinte resultado:

```
-Migrations
    -20220825184314_CreateDatabase.cs
    -20220825184314_CreateDatabase.Designer.cs
    -20220825191457_AddingNewTables.cs
    -20220825191457_AddingNewTables.Designer.cs
    -MyDbContextModelSnapshot.cs
```

Agora temos mais dois arquivos, `20220825191457_AddingNewTables.cs` e `20220825191457_AddingNewTables.Designer.cs`. O `20220825191457_AddingNewTables.cs` com o código para gerar o script SQL de migração e `20220825191457_AddingNewTables.Designer.cs` com o estado da model. Mas o arquivo `MyDbContextModelSnapshot.cs` também foi alterado. Agora ele tem a mesma definição que o arquivo `20220825191457_AddingNewTables.Designer.cs`.

> Mas como e EF Core sabe o que mudou entre um comando `Add-Migration` e outro?

Ele faz isso comparando o ModelSnapshot, que no nosso exemplo fica no arquivo `MyDbContextModelSnapshot.cs`, com a model construída a partir do DbContext.

Quando executamos o comando `Add-Migration` o EF Core instância o DbContext em questão e constrói a sua model através do método `OnModelCreating`. Com essa model construída, ele compara com a model do ModelSnapshot, com a diferença ele gera o código da migração e depois atualiza o ModelSnapshot.

> E a model do arquivo `.Designer.cs` pra que serve?

Ela serve para reverter o ModelSnapshot para uma migration anterior.

Exemplo, no nosso caso, se executarmos o comando `Remove-Migration`, vamos voltar para esta situação.

```
-Migrations
    -20220825184314_CreateDatabase.cs
    -20220825184314_CreateDatabase.Designer.cs
    -MyDbContextModelSnapshot.cs
```

Nesse caso, o ModelSnapshot que é o arquivo `MyDbContextModelSnapshot.cs` volta ter a mesma definição de model que o arquivo `20220825184314_CreateDatabase.Designer.cs`.

Lembrando, os códigos das migrações, model da migração e ModelSnapshot são gerados especificamente para o provider sendo usando. Por isso não é possível usar migrações geradas com o provider do SqlServer num provider de PostgreSQL ou Oracle. Da mesma forma, não é possível usar as migrações geradas pelo provider DotConnect no provider da Npgsql, pois cada provider tem customizações e annotations especificas que ele usa ao gerar o script SQL da migração.

### Regerando as migrações com o provider Npgsql

Como não podemos usar as migrações geradas pelo DotConnect com o novo provider, então temos que convertê-las para migrações compatíveis com o Npgsql. Para isso nossa estratégia será regerar as migrações usando o novo provider.

> Mas como fazer isso sem alterar a model do DbContext que a aplicação já está usando?

A estratégia envolve o uso dos comandos `Update-Database`, `Scaffold-DbContext` e `Add-Migration`.

Digamos que temos a seguinte lista de migrations para regerar:

```
-InitialModel
-AddingUserTable
-RemovingUserPassColumn
```

Primeiro criamos um banco de dados temporário. Depois configuramos o DbContext que queremos regerar a migrações para se conectar nesse banco de dados.

Então executamos o comando `Update-Database InitialModel -Context MyDbContext`.

Esse comando vai executar as migrações até a migração `InitialModel`, que é a primeira.

Com isso deixamos o banco de dados no estado de modelagem da primeira migração.

Logo em seguida executamos o comando `Scaffold-DbContext "{connString}" Npgsql.EntityFrameworkCore.PostgreSQL -Context NewMyDbContext` com a ConnectionString apontando para o mesmo banco de dados temporário, indicando que vamos usar o provider do Ngpsql.

Esse comando vai ler as tabelas e relacionamentos e gerar uma model baseado nessa leitura. Com essa model ele vai gerar um DbContext com nome `NewMyDbContext`, conforme indicamos no parâmetro `-Context` do comando. 

Ou seja, agora temos um DbContext que representa o estado da model após a primeira migração.

Logo após executamos o comando `Add-Migration InitialModel -Context NewMyDbContext`. Só que dessa vez, vamos apontar para o `NewMyDbContext`.

Esse comando vai gerar uma migração para o novo DbContext, dessa vez usando o provider da Npgsql.

Em seguida executamos o comando `Update-Database AddingUserTable -Context MyDbContext`.

Agora deixamos o banco de dados no estado de modelagem da migração `AddingUserTable`.

Depois executamos o comando `Scaffold-DbContext` da mesma forma. Agora teremos um DbContext que representa do estado da model após a migração `AddingUserTable`.

Executamos o commando `Add-Migration AddingUserTable -Context NewMyDbContext` e geramos a migração para este estado.

Depois executamos o mesmo processo para a migração `RemovingUserPassColumn`.

Contudo, esse processo não vai gerar um código que pode ser portado diretamente para o DbContext original da aplicação, pois sempre temos algumas diferenças entre os nomes no banco de dados e os nomes na model do DbContext, nome de relacionamento, mapeamentos de alguns tipos e etc. Ainda, o processo é bastante trabalhoso de se fazer manualmente e envolve muita repetição, por isso desenvolvemos uma ferramenta e um pacote para auxiliar.

## A ferramenta

Para ajudar nesse processo de migração desenvolvemos um pacote e uma ferramenta (dotnet tool).

### Tnf.EntityFrameworkCore.Migration.Tool

`Tnf.EntityFrameworkCore.Migration.Tool` foi desenvolvido como uma ferramenta dotnet tool que está disponível no feed do TNF;

A ferramenta vai auxiliar no processo de `Update-Database`, `Scaffold-DbContext` e `Add-Migration` que descrevemos acima.

Entes de instalar a ferramenta, é necessário instalar a dotnet tool EF Core na versão 3.1.26.

Para isso executamos o comando `dotnet tool install dotnet-ef -g --version 3.1.26`.

Para instalar a ferramenta, executamos o comando `dotnet tool install tnf.entityframeworkcore.migration.tool -g --add-source https://www.myget.org/F/tnf/api/v3/index.json`.

> Note que no final do comando adicionamos o source https://www.myget.org/F/tnf/api/v3/index.json, que é feed do TNF.

```
C:>tnf-ef-migration -h
Description:
  Migrates the provided DbContext to work on the Npgsql Provider

Usage:
  Tnf.EntityFrameworkCore.Migration.Tool [options]

Options:
  --connection-string <connection-string> (REQUIRED)                  The connectionString for the temporary Database.
  --context <context> (REQUIRED)                                      The target DbContext to migrate
  --context-project <context-project> (REQUIRED)                      The project name of the target DbContext
  --context-assembly <context-assembly> (REQUIRED)                    The assembly of the target DbContext
  --scaffold-project <scaffold-project> (REQUIRED)                    The project for the scaffold DbContext
  --scaffold-project-dir <scaffold-project-dir> (REQUIRED)            The directory for the project of the scaffold DbContext
  --scaffold-context-assembly <scaffold-context-assembly> (REQUIRED)  The assembly for the scaffold DbContext
  --scaffold-schema <scaffold-schema>                                 One of the schema used on the DbContext (This options can be set multiple times)
  --scaffold-table <scaffold-table>                                   One of the tables used on the DbContext (This options can be set multiple times)
  --scaffold-from-migration <scaffold-from-migration>                 Specifies the first migration to scaffold the new DbContext
  --scaffold-to-migration <scaffold-to-migration>                     Specifies the last migration to scaffold the new DbContext
  --working-dir <working-dir>                                         The working directory to execute the dotnet-ef commands [default: C:]
  --verbose                                                           [default: False]
  --version                                                           Show version information
  -?, -h, --help                                                      Show help and usage information
```

### Tnf.EntityFrameworkCore.Migration.Design

O `Tnf.EntityFrameworkCore.Migration.Design` deve ser usado no projeto onde ser vai criar os DbContext de migração. E tem as seguintes funções

* Ajuda no mapeamento dos nomes de campos e tabela que estão no banco de dados, para os os nomes na model do DbContext;
* Ajuda no mapeamento de alguns tipos de dados no banco dados que acabam vindo diferente da model original;
* Fornece a opção algumas opções que são usadas durante o processo de migração.

Veremos mais detalhes do uso tando da ferramenta quando do pacote no tutorial a seguir.

## Tutorial

Agora vamos fazer exemplo pratico de uso da ferramenta e pacote com uma aplicação.

> Use a branch `master` desse repositório como ponto de inicio para seguir o tutorial.

Este é o model da aplicação:

```C#
namespace BlogManager.Domain
{
    public class Author
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public DateTime Birthdate { get; set; }
        public short Ranking { get; set; }
    }

    public class AuthorMetrics
    {
        public int Id { get; set; }
        public decimal AverageWordsPerPost { get; set; }
        public decimal AveragePostsPerMonth { get; set; }
        public float StarRating { get; set; }

        public long AuthorId { get; set; }
        public Author Author { get; set; }
    }

    public class Blog
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public BlogCategory Category { get; set; }
    }

    public enum BlogCategory
    {
        Tecnology,
        Cooking,
        Travel
    }

    public class BlogAuthor
    {
        public int Id { get; set; }

        public long AuthorId { get; set; }
        public Author Author { get; set; }

        public int BlogId { get; set; }
        public Blog Blog { get; set; }
    }

    public class BlogPost
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTimeOffset PublishDate { get; set; }
        public TimeSpan? ReadTime { get; set; }
        public bool IsPublic { get; set; }

        public int BlogId { get; set; }
        public Blog Blog { get; set; }

        public long AuthorId { get; set; }
        public Author Author { get; set; }
    }

    public class BlogPostMetrics
    {
        public int Id { get; set; }
        public long ViewCount { get; set; }
        public decimal AverageViewCountPerDay { get; set; }

        public Guid PostId { get; set; }
        public BlogPost Post { get; set; }
    }

    public class BlogRating
    {
        public int Id { get; set; }
        public decimal StarRating { get; set; }

        public int BlogId { get; set; }
    }
}

namespace BlogManager.EFCore
{
    public class BlogDbContext : TnfDbContext
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<BlogAuthor> BlogAuthors { get; set; }

        public DbSet<AuthorMetrics> AuthorMetrics { get; set; }
        public DbSet<BlogPostMetrics> BlogPostMetrics { get; set; }

        public BlogDbContext(DbContextOptions<BlogDbContext> options, ITnfSession session)
            : base(options, session)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Blog>(builder =>
            {
                builder.HasKey(p => p.Id);
            });

            modelBuilder.Entity<Author>(builder =>
            {
                builder.HasKey(p => p.Id);

                builder.Property(p => p.Ranking)
                    .HasDefaultValue(-1);
            });

            modelBuilder.Entity<BlogPost>(builder =>
            {
                builder.HasKey(p => p.Id);

                builder.Property(p => p.IsPublic)
                    .HasDefaultValue(true);

                builder.HasOne(p => p.Blog)
                    .WithMany()
                    .HasForeignKey(p => p.BlogId);

                builder.HasOne(p => p.Author)
                    .WithMany()
                    .HasForeignKey(p => p.AuthorId);
            });

            modelBuilder.Entity<BlogAuthor>(builder =>
            {
                builder.HasKey(p => p.Id);

                builder.HasOne(p => p.Blog)
                    .WithMany()
                    .HasForeignKey(p => p.BlogId);

                builder.HasOne(p => p.Author)
                    .WithMany()
                    .HasForeignKey(p => p.AuthorId);
            });

            modelBuilder.Entity<AuthorMetrics>(builder =>
            {
                builder.HasKey(p => p.Id);

                builder.HasOne(p => p.Author)
                    .WithMany()
                    .HasForeignKey(p => p.AuthorId);

                builder.Property(p => p.AverageWordsPerPost)
                    .HasColumnName("AvgWordsPerPost");

                builder.Property(p => p.AveragePostsPerMonth)
                    .HasColumnName("AvgPostsPerMonth");
            });

            modelBuilder.Entity<BlogPostMetrics>(builder =>
            {
                builder.HasKey(p => p.Id);

                builder.HasOne(p => p.Post)
                    .WithMany()
                    .HasForeignKey(p => p.PostId);

                builder.Property(p => p.AverageViewCountPerDay)
                    .HasColumnName("AvgViewCountPerDay");
            });
        }
    }
}
```

No projeto `BlogManager.EFCore.PostgreSql` temos o DbContext `PostgreSqlBlogDbContext` que é um derivado do `BlogDbContext`. O `PostgreSqlBlogDbContext` tem as seguintes migrações:

```
-20220912225920_InitialModel.cs
-20220912230114_AddingMetricTables.cs
-20220912232120_AddingBlogRatings.cs
-20220912232132_ResetingBlogRatings.cs
-20220912232153_DropBlogRatingsTable.cs
```

Essas migrações foram criadas com provider da Devart.

Antes de iniciarmos o processo de migração, vamos gerar script completo de migração com as migrações originais.

Para fazer isso, vamos executar o comando `Script-Migration -Output DotConnectMigrations.sql`

```powershell
PM> Script-Migration -Output DotConnectMigrations.sql
Build started...
Build succeeded.
```

Vamos guardar esse script para poder comparar com script após a migração para Npgsql.

Agora vamos adicionar um projeto do tipo `Console App` na nossa solution. Como o projeto e EF Core para PostgreSQL da nossa aplicação se chama `BlogManager.EFCore.PostgreSql`, vamos chamar esse novo projeto de `BlogManager.EFCore.PostgreSql.Recover`, e vamos selecionar o Framework .NET Core 3.1 para ele.

No projeto `BlogManager.EFCore.PostgreSql.Recover` vamos fazer referencia ao projeto `BlogManager.EFCore.PostgreSql`.

Em seguida vamos adicionar o pacote `Tnf.EntityFrameworkCore.Migration.Design` executando o comando `Install-Package Tnf.EntityFrameworkCore.Migration.Design`.

```powershell
PM> Install-Package Tnf.EntityFrameworkCore.Migration.Design
Restoring packages for C:\Temp\BlogManager\BlogManager.EFCore.PostgreSql.Recover\BlogManager.EFCore.PostgreSql.Recover.csproj...
Installing NuGet package Tnf.EntityFrameworkCore.Migration.Design 3.11.0.25203.
//...
Time Elapsed: 00:00:03.1602137
```

Vamos criar uma pasta com o nome `DesignTime` e adicionar uma classe de nome `DesignTimeServices`.

A definição da classe deve ser a seguinte:

```C#
public class DesignTimeServices : IDesignTimeServices
{
    public void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddTnfEFCoreProviderMigration(builder =>
        {
            builder.ConfigureTnfDbContext<BlogDbContext, PostgreSqlBlogDbContext>(
                "Host=localhost;Port=5432;Database=BlogManager_Recover;User ID=postgres;password=admin");
        });
    }
}
```

A interface `IDesignTimeServices` implementada pela classe é uma interface definida pelo EF Core, e ela permite registar e substituir services que serão usadas nas operações de design time, tipo `Add-Migration`, `Update-Database`, `Scaffold-Context` e ect. Já o método de extensão `AddTnfEFCoreProviderMigration` é definido no pacote `Tnf.EntityFrameworkCore.Migration.Design` e ele faz o registro e substituição de services que precisamos para fazer essa migração. Dentro da lambda dele também registramos os DbContext da aplicação que vamos migrar. O Registro é necessário porque durante o processo de Scaffold usamos a model do DbContext da aplicação para dar o nome correto as propriedades do DbContext gerado e não apenas no nome da coluna, como originalmente é feito.

O primeiro parâmetro do método `AddTnfEFCoreProviderMigration` é a string de conexão do banco de dados que vamos usar para regerar as migrações. No exemplo estamos apontando para o banco `BlogManager_Recover`, estão vamos criar um banco de dados com esse nome no PostgreSql local. Não precisamos fazer nada além de criar o banco com esse nome.

Agora vamos criar uma pasta chamada `Properties` Adicionar um arquivo `AssemblyInfo.cs`.

Dentro do arquivo vamos por o seguinte conteúdo:

```c#
using Microsoft.EntityFrameworkCore.Design;

[assembly: DesignTimeServicesReference("BlogManager.EFCore.PostgreSql.Recover.DesignTime.DesignTimeServices, BlogManager.EFCore.PostgreSql.Recover")]
```

Assim mesmo, sem namespace ou classe.

O atributo `DesignTimeServicesReference` vai indicar ao EF Core qual a classe que vamos usar para carregar as services de design time. Nele vai uma `string` com o nome completo do tipo e o nome do assembly onde o tipo se encontra, no nosso caso `BlogManager.EFCore.PostgreSql.Recover.DesignTime.DesignTimeServices, BlogManager.EFCore.PostgreSql.Recover`.

Já estamos quase prontos para rodar a ferramenta. Primeiro precisamos ajustar a factory do nosso `PostgreSqlBlogDbContext`.

```c#
namespace BlogManager.EFCore.PostgreSql
{
    public class PostgreSqlBlogDbContextFactory : IDesignTimeDbContextFactory<PostgreSqlBlogDbContext>
    {
        public PostgreSqlBlogDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<BlogDbContext>();

            var connectionString = "Server=127.0.0.1;Port=5432;Database=BlogManager_Recover;User ID=postgres;password=admin;Unicode=true;";
            builder.UsePostgreSql(connectionString);
            PostgreSqlLicense.Validate(connectionString);

            return new PostgreSqlBlogDbContext(builder.Options, NullTnfSession.Instance);
        }
    }
}
```

Aqui nós mudamos a string de conexão para conectar no banco de dados temporário.

Para o processo de migração é importante deixar a factory de DbContext o mais simples possível. Caso contrário a ferramenta do TNF pode não conseguir carregar os assemblies envolvidos. Após a migração você pode voltar ao código original.

Agora vamos rodar a ferramenta. Mas primeiro temos que instala-la.

Vamos instalar do ferramenta dotnet-ef primeiro com o comando `dotnet tool install dotnet-ef -g --version 3.1.26` em uma janela de cmd.

```
C:\Temp\BlogManager>dotnet tool install dotnet-ef -g --version 3.1.26
Você pode invocar a ferramenta usando o comando a seguir: dotnet-ef
A ferramenta 'dotnet-ef' (versão '3.1.26') foi instalada com êxito.
```

Depois instalamos a ferramenta do TNF com o comando `dotnet tool install tnf.entityframeworkcore.migration.tool -g --add-source https://www.myget.org/F/tnf/api/v3/index.json`, também em uma janela cmd.

```
C:\Temp\BlogManager>dotnet tool install tnf.entityframeworkcore.migration.tool -g --add-source https://www.myget.org/F/tnf/api/v3/index.json
Você pode invocar a ferramenta usando o comando a seguir: tnf-ef-migration
A ferramenta 'tnf.entityframeworkcore.migration.tool' (versão '3.11.0.25203') foi instalada com êxito.
```

Com a ferramenta instalada, abrimos um cmd e navegamos até pasta da solution. No meu caso fica em `C:\Temp\BlogManager>`.

Agora vamos executar a ferramenta

```powershell
C:\Temp\BlogManager>tnf-ef-migration --connection-string "Host=localhost;Port=5432;Database=BlogManager_Recover;User ID=postgres;password=admin" --context PostgreSqlBlogDbContext --context-project BlogManager.EFCore.PostgreSql --context-assembly BlogManager.EFCore.PostgreSql.Recover\bin\Debug\netcoreapp3.1\BlogManager.EFCore.PostgreSql.dll --scaffold-project BlogManager.EFCore.PostgreSql.Recover --scaffold-project-dir BlogManager.EFCore.PostgreSql.Recover --scaffold-context-assembly BlogManager.EFCore.PostgreSql.Recover\bin\Debug\netcoreapp3.1\BlogManager.EFCore.PostgreSql.Recover.dll
```

Vamos passar os parâmetros um por um.

* `--connection-string`: A string de conexão com o banco temporário;
* `--context`: O DbContext que queremos regerar as migrações. Lembrando - esse deve ser o DbContext derivado que tem uma factory, não o DbContext base;
* `--context-project`: O projeto que o DbContext se encontra;
* `--context-assembly`: O assembly que o DbContext se encontra. Nesse caso apontamos para o assembly dentro da pasta `bin` do projeto `BlogManager.EFCore.PostgreSql.Recover`, já que as operações serão feitas lá;
* `--scaffold-project`: O projeto onde vamos colocar o nosso novo DbContext que vai ser gerado pela ferramenta;
* `--scaffold-project-dir`: A pasta do projeto onde vamos colocar o novo DbContext;
* `--scaffold-context-assembly`: O assembly onde o novo DbContext vai ficar.

> Todos os caminhos passados por parâmetro são relativos ao local de onde a ferramenta está sendo executada, no exemplo `C:\Temp\BlogManager`.

Após a execução da ferramenta, podemos ver que temos a pasta `Scaffold_PostgreSqlBlogDbContext` no projeto `BlogManager.EFCore.PostgreSql.Recover`. Nela teremos todas as entidades da model e um DbContext com o nome `ProviderMigration_PostgreSqlBlogDbContext`.

```
Scaffold_PostgreSqlBlogDbContext
  -Migrations
  -Author.cs
  -AuthorMetrics.cs
  -Blog.cs
  -BlogAuthor.cs
  -BlogPost.cs
  -BlogPostMetrics.cs
  -BlogRatings.cs
  -ProviderMigration_PostgreSqlBlogDbContext.cs
```

Também temos uma entidade que não estava na model original. A entidade `BlogRatings` existia na model original em versões anteriores, mas a ultima migração removeu ela. Como a ferramenta apensa sobrescreve o conteúdo da pasta `Scaffold_PostgreSqlBlogDbContext` sem apagar nada, a entidade permanece na pasta. Porém se formos olhar dentro da classe `ProviderMigration_PostgreSqlBlogDbContext`, ela não vai estar lá.

Esse é o DbContext gerado pela ferramenta:

```c#
namespace ProviderMigration.BlogManager.EFCore.PostgreSql
{
    public partial class ProviderMigration_PostgreSqlBlogDbContext : DbContext
    {
        public ProviderMigration_PostgreSqlBlogDbContext()
        {
        }

        public ProviderMigration_PostgreSqlBlogDbContext(DbContextOptions<ProviderMigration_PostgreSqlBlogDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Author> Author { get; set; }
        public virtual DbSet<AuthorMetrics> AuthorMetrics { get; set; }
        public virtual DbSet<Blog> Blog { get; set; }
        public virtual DbSet<BlogAuthor> BlogAuthor { get; set; }
        public virtual DbSet<BlogPost> BlogPost { get; set; }
        public virtual DbSet<BlogPostMetrics> BlogPostMetrics { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
    #warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=BlogManager_Recover;User ID=postgres;password=admin");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Author>(entity =>
            {
                entity.ToTable("Authors");

                entity.Property(e => e.Id).UseSerialColumn();

                entity.Property(e => e.Ranking).HasDefaultValueSql("'-1'::integer");
            });

            modelBuilder.Entity<AuthorMetrics>(entity =>
            {
                entity.HasIndex(e => e.AuthorId);

                entity.Property(e => e.Id).UseSerialColumn();

                entity.Property(e => e.AveragePostsPerMonth)
                    .HasColumnName("AvgPostsPerMonth")
                    .HasColumnType("numeric");

                entity.Property(e => e.AverageWordsPerPost)
                    .HasColumnName("AvgWordsPerPost")
                    .HasColumnType("numeric");

                entity.HasOne(d => d.Author)
                    .WithMany(p => p.AuthorMetrics)
                    .HasForeignKey(d => d.AuthorId);
            });

            modelBuilder.Entity<Blog>(entity =>
            {
                entity.ToTable("Blogs");

                entity.Property(e => e.Id).UseSerialColumn();
            });

            modelBuilder.Entity<BlogAuthor>(entity =>
            {
                entity.ToTable("BlogAuthors");

                entity.HasIndex(e => e.AuthorId);

                entity.HasIndex(e => e.BlogId);

                entity.Property(e => e.Id).UseSerialColumn();

                entity.HasOne(d => d.Author)
                    .WithMany(p => p.BlogAuthor)
                    .HasForeignKey(d => d.AuthorId);

                entity.HasOne(d => d.Blog)
                    .WithMany(p => p.BlogAuthor)
                    .HasForeignKey(d => d.BlogId);
            });

            modelBuilder.Entity<BlogPost>(entity =>
            {
                entity.ToTable("BlogPosts");

                entity.HasIndex(e => e.AuthorId);

                entity.HasIndex(e => e.BlogId);

                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e.IsPublic)
                    .IsRequired()
                    .HasDefaultValueSql("true");

                entity.Property(e => e.ReadTime).HasColumnType("time without time zone");

                entity.HasOne(d => d.Author)
                    .WithMany(p => p.BlogPost)
                    .HasForeignKey(d => d.AuthorId);

                entity.HasOne(d => d.Blog)
                    .WithMany(p => p.BlogPost)
                    .HasForeignKey(d => d.BlogId);
            });

            modelBuilder.Entity<BlogPostMetrics>(entity =>
            {
                entity.HasIndex(e => e.PostId);

                entity.Property(e => e.Id).UseSerialColumn();

                entity.Property(e => e.AverageViewCountPerDay)
                    .HasColumnName("AvgViewCountPerDay")
                    .HasColumnType("numeric");

                entity.HasOne(d => d.Post)
                    .WithMany(p => p.BlogPostMetrics)
                    .HasForeignKey(d => d.PostId);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
```

Dá para notar também que esse novo DbContext não está no mesmo namespace que o DbContext original, ele está no namespace `ProviderMigration.BlogManager.EFCore.PostgreSql`. Não é só o DbContext, as entidades da model também foram colocadas no mesmo namespace. Isso é feito para evitar conflito com as classes do modelo original.

Agora que temos o novo DbContext, o nosso objectivo não é substituir o DbContext da nossa aplicação por ele, mas sim usar partes dele ajustar o DbContext original o poder rodar as migrações com o provider da Npgsql.

Vamos começão copiando o conteúdo do método `OnModelCreating`. Mas não vamos copiar para o `OnModelCreating` do `BlogDbContext`, vamos copiar para o `OnModelCreating` do `PostgreSqlBlogDbContext`, pois esse é o DbContext especifico para PostgreSql.

Após a copia vamos ter alguns erros de compilação porque o mapeamento das relações entre as entidades está diferente, pois no modelo original não temos a navegação bidirecional. Nesse caso basta apagar os mapeamentos que estão com erro, pois ele já são registrados no `OnModelCreating` do `BlogDbContext`.

Agora vamos olhar dentro da pasta `Migrations` do novo DbContext. Vamos olhar o arquivo `ProviderMigration_PostgreSqlBlogDbContextModelSnapshot.cs`. Esse é o ModelSnapshot do `ProviderMigration_PostgreSqlBlogDbContext`. Ele também está em um namespace diferente então não podemos substituir o arquivo inteiro, ao invés, vamos apensa copiar os conteúdo do método `BuildModel` e substituir pelo conteúdo desse mesmo método no arquivo `PostgreSqlBlogDbContextModelSnapshot.cs`.

Após a substituição, temos que corrigir os nomes das entidades nesse arquivo, pois como falamos antes, as entidades da model `ProviderMigration_PostgreSqlBlogDbContext` tem um namespace diferente do original, e o conteúdo desse ModelSnapshot que acabamos de copiar se refere e essas entidades. No nosso caso precisamos fazer um replace de `ProviderMigration.BlogManager.EFCore.PostgreSql` por `BlogManager.Domain` para ajustar o namespace das entidades.

Agora já podemos fazer um teste da nossa migração. Sim, ainda não migramos as migrações em si, mas podemos fazer um teste para saber se a nossa model está migrada corretamente. 

Para saber isso, podemos usar o comando `Add-Migration`. Pois se não houver diferenças entre o `PostgreSqlBlogDbContextModelSnapshot` e a model do `PostgreSqlBlogDbContext`, a migration resultante deve ficar vazia. Caso haja alguma diferença, a migração resultante vai ter algumas operações, então teremos fazer ajustes no `PostgreSqlBlogDbContextModelSnapshot` ou no `PostgreSqlBlogDbContext`.

Caso haja necessidade de fazer ajustes, também teremos que fazer um `Remove-Migration` para remover essa última migração. Isso vai substituir a model do arquivo `PostgreSqlBlogDbContextModelSnapshot.cs` pela model do arquivo `20220912232153_DropBlogRatingsTable.Designer.cs`. Porém, esse não foi atualizado de acordo com as migrações do `ProviderMigration_PostgreSqlBlogDbContext` e vai desfazer o trabalho que já fizemos no arquivo `PostgreSqlBlogDbContextModelSnapshot.cs`. Então para prevenir isso, vamos substituir o conteúdo do método `BuildTargetModel` no arquivo `20220912232153_DropBlogRatingsTable.Designer.cs` com o conteúdo do mesmo método no arquivo `.Designer.cs` da migração `DropBlogRatingsTable` do DbContext `ProviderMigration_PostgreSqlBlogDbContext`. Nesse caso também precisamos corrigir o namespace das entidades da mesma forma que fizemos com o `PostgreSqlBlogDbContextModelSnapshot`.

Antes do `Add-Migration` também temos que ajustar a `PostgreSqlBlogDbContextFactory` para começar a usar o provider da Ngpsql.

Primeiro vamos adicionar a seguinte `PackageReference` no projeto `BlogManager.EFCore.PostgreSql`.

```xml
<ItemGroup>
  <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="3.1.18" />
</ItemGroup>
```

Agora vamos ajusta a classe `PostgreSqlBlogDbContextFactory`:

```c#
public class PostgreSqlBlogDbContextFactory : IDesignTimeDbContextFactory<PostgreSqlBlogDbContext>
{
    public PostgreSqlBlogDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<BlogDbContext>();

        var connectionString = "Host=127.0.0.1;Port=5432;Database=BlogManager_Recover;User ID=postgres;password=admin;";
        builder.UseNpgsql(connectionString);

        return new PostgreSqlBlogDbContext(builder.Options, NullTnfSession.Instance);
    }
}
```

Note que a string de conexão usada pelo Npgsql é diferente da anterior.

Pronto, agora estamos prontos para rodar um `Add-Migration`. Vamos executar o comando `Add-Migration Test`.

```powershell
PM> Add-Migration Test
Build started...
Build succeeded.
System.InvalidOperationException: DefaultValue cannot be set for 'Ranking' at the same time as DefaultValueSql. Remove one of these values.
   at Microsoft.EntityFrameworkCore.Metadata.Conventions.StoreGenerationConvention.Validate(IConventionProperty property)
   at Microsoft.EntityFrameworkCore.Metadata.Conventions.StoreGenerationConvention.ProcessModelFinalized(IConventionModelBuilder modelBuilder, IConventionContext`1 context)
   //...
DefaultValue cannot be set for 'Ranking' at the same time as DefaultValueSql. Remove one of these values.
```

Opa! Deu um erro.

```
DefaultValue cannot be set for 'Ranking' at the same time as DefaultValueSql. Remove one of these values.
```

Esse erro parece ser por causa da chamada `HasDefaultValueSql` na propriedade `Ranking` da entidade `Author`, pois já temos uma chamada `HasDefaultValue` para essa propriedade no `OnModelCreating` da classe `BlogDbContext`.

```c#
modelBuilder.Entity<Author>(entity =>
{
    entity.ToTable("Authors");

    entity.Property(e => e.Id).UseSerialColumn();

    //entity.Property(e => e.Ranking).HasDefaultValueSql("'-1'::integer");
});
```

Podemos comentar essa linha na classe `PostgreSqlBlogDbContext`.

Pronto, agora podemos executar o `Add-Migration Test` novamente.

```powershell
PM> Add-Migration Test
Build started...
Build succeeded.
System.InvalidOperationException: DefaultValue cannot be set for 'IsPublic' at the same time as DefaultValueSql. Remove one of these values.
   at Microsoft.EntityFrameworkCore.Metadata.Conventions.StoreGenerationConvention.Validate(IConventionProperty property)
   at Microsoft.EntityFrameworkCore.Metadata.Conventions.StoreGenerationConvention.ProcessModelFinalized(IConventionModelBuilder modelBuilder, IConventionContext`1 context)
   //...
DefaultValue cannot be set for 'IsPublic' at the same time as DefaultValueSql. Remove one of these values.
```

Hmm! Outro erro.

```
DefaultValue cannot be set for 'IsPublic' at the same time as DefaultValueSql. Remove one of these values.
```

Dessa vez é a propriedade `IsPublic` da entidade `BlogPost`. Vamos fazer a mesma coisa. Vamos comentar a linha.

```c#
modelBuilder.Entity<BlogPost>(entity =>
{
    entity.ToTable("BlogPosts");

    entity.HasIndex(e => e.AuthorId);

    entity.HasIndex(e => e.BlogId);

    entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");

    // entity.Property(e => e.IsPublic)
    //     .IsRequired()
    //     .HasDefaultValueSql("true");

    entity.Property(e => e.ReadTime).HasColumnType("time without time zone");
});
```

Vamos tentar o `Add-Migration Test` novamente.

```powershell
PM> Add-Migration Test
Build started...
Build succeeded.
To undo this action, use Remove-Migration.
```

Agora não tivemos erro, mas a migration tem operações.

```c#
public partial class Test : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<bool>(
            name: "IsPublic",
            table: "BlogPosts",
            nullable: false,
            defaultValue: true,
            oldClrType: typeof(bool),
            oldType: "boolean",
            oldDefaultValueSql: "true");

        migrationBuilder.AlterColumn<short>(
            name: "Ranking",
            table: "Authors",
            nullable: false,
            defaultValue: (short)-1,
            oldClrType: typeof(short),
            oldType: "smallint",
            oldDefaultValueSql: "'-1'::integer");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<bool>(
            name: "IsPublic",
            table: "BlogPosts",
            type: "boolean",
            nullable: false,
            defaultValueSql: "true",
            oldClrType: typeof(bool),
            oldDefaultValue: true);

        migrationBuilder.AlterColumn<short>(
            name: "Ranking",
            table: "Authors",
            type: "smallint",
            nullable: false,
            defaultValueSql: "'-1'::integer",
            oldClrType: typeof(short),
            oldDefaultValue: (short)-1);
    }
}
```

Pelo visto são as mesmas propriedades que estavam gerando os erros anteriormente. E o que está gerando a operação para a propriedade `IsPublic` é que no ModelSnapshot temos um `DefaultValueSql` com o valor de `"true"`, mas na model é um `DefaultValue` com `true`. Para propriedade `Ranking` a situação é similar, onde temos no ModelSnapshot um `DefaultValueSql` de `"'-1'::integer"`, mas na model um `DefaultValue` de `(short)-1`.

Para corrigir isso, vamos ajustar os default values no ModelSnapshot.

Mas antes de fazer isso, temos que remover essa migração indesejada executando um `Remove-Migration`. E aqui é onde se paga o passo que fizemos de também ajustar o arquivo `20220913215043_DropBlogRatingsTable.Designer.cs`, pois agora o model dele vai virar o nosso ModelSnapshot.

```powershell
PM> Remove-Migration
Build started...
Build succeeded.
Removing migration '20220915144836_Test'.
Reverting model snapshot.
Done.
```

Agora que removemos a migração, vamos ajustar o ModelSnapshot no arquivo `PostgreSqlBlogDbContextModelSnapshot.cs`.

```c#
modelBuilder.Entity("BlogManager.Domain.Author", b =>
    {
        //...

        b.Property<short>("Ranking")
            .ValueGeneratedOnAdd()
            .HasColumnType("smallint")
            //.HasDefaultValueSql("'-1'::integer")
            .HasDefaultValue((short)-1);

        //...
    });
//.....
modelBuilder.Entity("BlogManager.Domain.BlogPost", b =>
    {
        //...
        b.Property<bool>("IsPublic")
            .ValueGeneratedOnAdd()
            .HasColumnType("boolean")
            //.HasDefaultValueSql("true")
            .HasDefaultValue(true);

        //....
    });
```

Para evitar problemas quando executarmos um `Remove-Migration`, vamos fazer o mesmo no arquivo `20220913215043_DropBlogRatingsTable.Designer.cs`.

Agora executamos o `Add-Migration Test`:

```powershell
PM> Add-Migration Test
Build started...
Build succeeded.
To undo this action, use Remove-Migration.
```

E o resultado:

```c#
public partial class Test : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {

    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
}
```

Ótimo! Isso significa que a model do `PostgreSqlBlogDbContext` e o ModelSnapshot estão de acordo.

Vamos remover esse migração pois não precisamos dela.

```powershell
PM> Remove-Migration
Build started...
Build succeeded.
Removing migration '20220915180914_Test'.
Reverting model snapshot.
Done.
```

Agora falta migrar a migrações em si.

Vamos começar pela primeira, a `InitialModel`.

Basta substituir o conteúdo dos métodos `Up` e `Down` do arquivo `20220912225920_InitialModel.cs` pelo conteúdo da mesma migração na pasta `Scaffold_PostgreSqlBlogDbContext/Migrations` no projeto `BlogManager.EFCore.PostgreSql.Recover`.

Também temos que substituir o conteúdo do método `BuildTargetModel` do arquivo `20220912225920_InitialModel.Designer.cs` pelo equivalente que está no projeto `BlogManager.EFCore.PostgreSql.Recover`. E não podemos esquecer que temos que fazer a correção do namespace da entidades de `ProviderMigration.BlogManager.EFCore.PostgreSql` para `BlogManager.Domain`, e o ajuste dos defaults das propriedades `Ranking` e `IsPublic` com fizemos no arquivo `20220912232153_DropBlogRatingsTable.Designer.cs`.

Agora basta fazer o mesmo para as migrações `AddingMetricTables`, `AddingBlogRatings`, `ResetingBlogRatings`, `DropBlogRatingsTable`. Atenção para a `ResetingBlogRatings`, pois a que está no projeto `BlogManager.EFCore.PostgreSql.Recover` estará vazia. Nesse caso preservamos a original.

Agora para validar que as migrações rodam. Vamos criar um banco de dados novo e vamos chamar ele de `BlogManager_Migrated`.

Alteramos a PostgreSqlBlogDbContextFactory para conectar nesse banco de dados.

```c#
public class PostgreSqlBlogDbContextFactory : IDesignTimeDbContextFactory<PostgreSqlBlogDbContext>
{
    public PostgreSqlBlogDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<BlogDbContext>();

        var connectionString = "Host=127.0.0.1;Port=5432;Database=BlogManager_Migrated;User ID=postgres;password=admin;";
        builder.UseNpgsql(connectionString);

        return new PostgreSqlBlogDbContext(builder.Options, NullTnfSession.Instance);
    }
}
```

E executamos o comando `Update-Database`.

```powershell
PM> Update-Database
Build started...
Build succeeded.
Applying migration '20220912225920_InitialModel'.
Applying migration '20220912230114_AddingMetricTables'.
Applying migration '20220912232120_AddingBlogRatings'.
Applying migration '20220912232132_ResetingBlogRatings'.
Applying migration '20220912232153_DropBlogRatingsTable'.
Done.
```

Também podemos executar um `Script-Migration` e comparar com o script anterior.

```powershell
PM> Script-Migration -Output NpgsqlMigrations.sql
Build started...
Build succeeded.
```

Podemos ver que os scripts são bem parecidos. Temos apenas algumas alterações nos nomes dos tipos, onde um usa o alias e outro está usando o nome próprio, e uma declara a constraint da PK explicitamente enquanto o outro deixa de forma implícita.

Agora já podemos mudar o drive no resto da aplicação.

Vamos até o arquivo `PostgreSqlServiceCollectionExtensions.cs` mudar o provider usado pela aplicação.

```c#
public static class PostgreSqlServiceCollectionExtensions
{
    public static IServiceCollection AddPostgreSqlEFCore(this IServiceCollection services)
    {
        services.AddTnfDbContext<BlogDbContext, PostgreSqlBlogDbContext>(conf =>
        {
            //conf.DbContextOptions.UsePostgreSql(conf.ConnectionString);
            conf.DbContextOptions.UseNpgsql(conf.ConnectionString);
        });

        return services;
    }
}
```

Também precisamos remove a chamada para ativação do drive da Devart na classe `Startup`.

```c#
public class Startup
{
    //...

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddTnfAspNetCore(tnf =>
        {
            tnf.DefaultConnectionString(Configuration.GetConnectionString("PostgreSql"));

            // tnf.EnableDevartPostgreSQLDriver();
        });

        services.AddPostgreSqlEFCore();
        services.AddHostedService<MigratorService>();

        services.AddControllers();
    }
}
```

E precisamos mudar a string de conexão nos arquivos `appsettings.json` e `appsettings.Development.json` para ficar no formato aceito pelo Npgsql.

```c#
"ConnectionStrings": {
    //"PostgreSql": "Server=127.0.0.1;Port=5432;Database=BlogManager;User ID=postgres;password=admin;Unicode=true;"
    "PostgreSql": "Host=127.0.0.1;Port=5432;Database=BlogManager;User ID=postgres;password=admin;"
},
```

Pronto!! Aplicação migrada para o Ngpsql para EntityFramework Core.

Se você quiser ver a nossa versão da aplicação migrada para compara com a sua, olhe a branch `Completed`.

## Pontos de atenção

### Navigation properties

Caso você tenha uma sua model navigation properties com nomes diferentes das tabelas, a model gerada pela ferramenta vai gerar essas navigations com os nomes das tabelas. Então ao copiar o conteúdo do `OnModelCreating` do DbContext gerado pela ferramenta para o DbContext da aplicação, podem ocorrer alguns erros no mapeamento das navigations. Para resolver basta ajustar os nomes ou remover os mapeamentos duplicados ou que não são necessários.

### Default Values

Como vimos durante o tutorial, ao fazer a engenharia reversa do banco de dados para gerar o DbContext, algumas coisas vão ficar duplicadas. Os valores padrões das colunas são uma delas, porém, nesse caso, isso gera erro no EF Core, pois ao restaurar o DbContext vem com o valor padrão como `DefaultValueSql`, que é diferente do que normalmente fazemos nas nossas models.

Em boa parte dos casos o valor padrão vai precisar de um ajuste como fizemos no tutorial.

### Indexes nomeados

Os índices das tabelas vão aparecer na model do DbContext gerado pela ferramenta. Esses índices vão ter nomes. Não esqueça de trazer esses índices para a model do DbContext da aplicação.

### Tabelas e colunas renomeadas

Se sua aplicação teve tabelas ou colunas renomeadas, o processo executado pela ferramenta não consegue identificar isso. Por exemplo, se a tabela `BlogPostMetrics` for renomeada para `PostMetrics`, a ferramenta vai criar um comando `drop table` para `BlogPostMetrics` e um `create table` para `PostMetrics`.

Se para as suas migrações for crucial que seja feito a renomeação da tabela, você tera que ajustar a migração manualmente.

### Múltiplos DbContexts

Se sua aplicação contem múltiplos DbContexts, será necessário mais cuidados ao executar a ferramenta, pois se nenhum parâmetro extra for passado, quando a ferramenta roda, ela lê todas as tabelas e adiciona no DbContext criado. Se você estiver usando o mesmo banco de dados temporário para todos DbContexts, quando for fazer o segundo DbContext, ele vai acabar pegando as tabelas do primeiro também.

Logo, aconselhamos criar um banco de dados temporário para cada DbContext.

Ou então passar os nomes das tabelas que pertencem ao DbContext. Basta passar o parâmetro `--scaffold-table` uma vez para cada tabela. Ou, passar via código na hora que está configurando o DbContext na chamada `ConfigureTnfDbContext`.

```c#
public void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
{
    serviceCollection.AddTnfEFCoreProviderMigration(builder =>
    {
        builder.ConfigureTnfDbContext<BlogDbContext, PostgreSqlBlogDbContext>(
            "Host=localhost;Port=5432;Database=BlogManager_Recover;User ID=postgres;password=admin",
            options =>
            {
                options.Tables = new List<string>
                {
                    "Table1",
                    "Table2",
                    "Table3"
                };
            });
    });
}
```

Nesse caso é necessário colocar todas as tabelas que pertencem e já pertenceram ao DbContext. Isso é necessário porque mesmo que a tabela não exista mais no DbContext, nós temos que gerar a migração que cria e depois remove ela.
