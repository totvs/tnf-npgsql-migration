﻿// <auto-generated />
using System;
using BlogManager.EFCore.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace BlogManager.EFCore.PostgreSql.Migrations
{
    [DbContext(typeof(PostgreSqlBlogDbContext))]
    partial class PostgreSqlBlogDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.26")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("BlogManager.Domain.Author", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                    b.Property<DateTime>("Birthdate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<short>("Ranking")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("smallint")
                        .HasDefaultValue((short)-1);

                    b.HasKey("Id");

                    b.ToTable("Authors");
                });

            modelBuilder.Entity("BlogManager.Domain.AuthorMetrics", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                    b.Property<long>("AuthorId")
                        .HasColumnType("bigint");

                    b.Property<decimal>("AveragePostsPerMonth")
                        .HasColumnName("AvgPostsPerMonth")
                        .HasColumnType("numeric");

                    b.Property<decimal>("AverageWordsPerPost")
                        .HasColumnName("AvgWordsPerPost")
                        .HasColumnType("numeric");

                    b.Property<float>("StarRating")
                        .HasColumnType("real");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.ToTable("AuthorMetrics");
                });

            modelBuilder.Entity("BlogManager.Domain.Blog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                    b.Property<int>("Category")
                        .HasColumnType("integer");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Blogs");
                });

            modelBuilder.Entity("BlogManager.Domain.BlogAuthor", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                    b.Property<long>("AuthorId")
                        .HasColumnType("bigint");

                    b.Property<int>("BlogId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.HasIndex("BlogId");

                    b.ToTable("BlogAuthors");
                });

            modelBuilder.Entity("BlogManager.Domain.BlogPost", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("gen_random_uuid()");

                    b.Property<long>("AuthorId")
                        .HasColumnType("bigint");

                    b.Property<int>("BlogId")
                        .HasColumnType("integer");

                    b.Property<string>("Content")
                        .HasColumnType("text");

                    b.Property<bool?>("IsPublic")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(true);

                    b.Property<DateTimeOffset>("PublishDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<TimeSpan?>("ReadTime")
                        .HasColumnType("time without time zone");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.HasIndex("BlogId");

                    b.ToTable("BlogPosts");
                });

            modelBuilder.Entity("BlogManager.Domain.BlogPostMetrics", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                    b.Property<decimal>("AverageViewCountPerDay")
                        .HasColumnName("AvgViewCountPerDay")
                        .HasColumnType("numeric");

                    b.Property<Guid>("PostId")
                        .HasColumnType("uuid");

                    b.Property<long>("ViewCount")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("PostId");

                    b.ToTable("BlogPostMetrics");
                });

            modelBuilder.Entity("BlogManager.Domain.AuthorMetrics", b =>
                {
                    b.HasOne("BlogManager.Domain.Author", "Author")
                        .WithMany("AuthorMetrics")
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BlogManager.Domain.BlogAuthor", b =>
                {
                    b.HasOne("BlogManager.Domain.Author", "Author")
                        .WithMany("BlogAuthor")
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BlogManager.Domain.Blog", "Blog")
                        .WithMany("BlogAuthor")
                        .HasForeignKey("BlogId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BlogManager.Domain.BlogPost", b =>
                {
                    b.HasOne("BlogManager.Domain.Author", "Author")
                        .WithMany("BlogPost")
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BlogManager.Domain.Blog", "Blog")
                        .WithMany("BlogPost")
                        .HasForeignKey("BlogId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BlogManager.Domain.BlogPostMetrics", b =>
                {
                    b.HasOne("BlogManager.Domain.BlogPost", "Post")
                        .WithMany("BlogPostMetrics")
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
