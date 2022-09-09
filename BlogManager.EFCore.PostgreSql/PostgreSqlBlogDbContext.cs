using System;

using Microsoft.EntityFrameworkCore;

using Tnf.Runtime.Session;

namespace BlogManager.EFCore.PostgreSql
{
    public class PostgreSqlBlogDbContext : BlogDbContext
    {
        public PostgreSqlBlogDbContext(DbContextOptions<BlogDbContext> options, ITnfSession session)
            : base(options, session)
        {
        }
    }
}
