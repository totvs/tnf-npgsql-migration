using System;

using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace BlogManager.EFCore.PostgreSql.Recover.DesignTime
{
    public class DesignTimeServices : IDesignTimeServices
    {
        public void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTnfEFCoreProviderMigration(builder =>
            {
                builder.ConfigureTnfDbContext<BlogDbContext, PostgreSqlBlogDbContext>(
                    "Host=localhost;Port=5432;Database=BlogManager_Recover;User ID=postgres;password=admin",
                    options =>
                    {
                    });
            });
        }
    }
}
