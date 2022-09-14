using System;

using BlogManager.Domain;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Author>(entity =>
            {
                entity.ToTable("Authors");

                entity.Property(e => e.Id).UseSerialColumn();
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
            });

            modelBuilder.Entity<BlogPost>(entity =>
            {
                entity.ToTable("BlogPosts");

                entity.HasIndex(e => e.AuthorId);

                entity.HasIndex(e => e.BlogId);

                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e.ReadTime).HasColumnType("time without time zone");
            });

            modelBuilder.Entity<BlogPostMetrics>(entity =>
            {
                entity.HasIndex(e => e.PostId);

                entity.Property(e => e.Id).UseSerialColumn();

                entity.Property(e => e.AverageViewCountPerDay)
                    .HasColumnName("AvgViewCountPerDay")
                    .HasColumnType("numeric");
            });
        }
    }
}
