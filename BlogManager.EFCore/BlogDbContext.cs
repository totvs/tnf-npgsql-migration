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
