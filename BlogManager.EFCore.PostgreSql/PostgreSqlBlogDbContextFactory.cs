using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

using Tnf.Drivers.DevartPostgreSQL;
using Tnf.Runtime.Session;

namespace BlogManager.EFCore.PostgreSql
{
    public class PostgreSqlBlogDbContextFactory : IDesignTimeDbContextFactory<PostgreSqlBlogDbContext>
    {
        public PostgreSqlBlogDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<BlogDbContext>();

            var connectionString = "Server=127.0.0.1;Port=5432;Database=BlogManager;User ID=postgres;password=admin;Unicode=true;";
            builder.UsePostgreSql(connectionString);
            PostgreSqlLicense.Validate(connectionString);

            return new PostgreSqlBlogDbContext(builder.Options, NullTnfSession.Instance);
        }
    }
}
