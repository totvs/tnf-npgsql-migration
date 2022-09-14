using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

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
