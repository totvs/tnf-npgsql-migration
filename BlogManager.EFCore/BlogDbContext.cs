using BlogManager.Domain;

using Microsoft.EntityFrameworkCore;

using Tnf.EntityFrameworkCore;
using Tnf.Runtime.Session;

namespace BlogManager.EFCore
{
    public class BlogDbContext : TnfDbContext
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<BlogAuthor> BlogAuthors { get; set; }

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
            });

            modelBuilder.Entity<BlogPost>(builder =>
            {
                builder.HasKey(p => p.Id);

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
        }
    }
}
