using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

using Tnf.Runtime.Session;

namespace BlogManager.EFCore.PostgreSql
{
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
}
