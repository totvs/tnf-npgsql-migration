using System;
using System.Collections.Generic;
using System.Text;

using BlogManager.EFCore;
using BlogManager.EFCore.PostgreSql;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class PostgreSqlServiceCollectionExtensions
    {
        public static IServiceCollection AddPostgreSqlEFCore(this IServiceCollection services)
        {
            services.AddTnfDbContext<BlogDbContext, PostgreSqlBlogDbContext>(conf =>
            {
                conf.DbContextOptions.UseNpgsql(conf.ConnectionString);
            });

            return services;
        }
    }
}
