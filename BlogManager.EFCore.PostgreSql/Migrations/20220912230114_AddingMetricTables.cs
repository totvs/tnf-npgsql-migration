using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BlogManager.EFCore.PostgreSql.Migrations
{
    public partial class AddingMetricTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthorMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    AvgWordsPerPost = table.Column<decimal>(nullable: false),
                    AvgPostsPerMonth = table.Column<decimal>(nullable: false),
                    StarRating = table.Column<float>(nullable: false),
                    AuthorId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthorMetrics_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlogPostMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    ViewCount = table.Column<long>(nullable: false),
                    AvgViewCountPerDay = table.Column<decimal>(nullable: false),
                    PostId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogPostMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlogPostMetrics_BlogPosts_PostId",
                        column: x => x.PostId,
                        principalTable: "BlogPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthorMetrics_AuthorId",
                table: "AuthorMetrics",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPostMetrics_PostId",
                table: "BlogPostMetrics",
                column: "PostId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthorMetrics");

            migrationBuilder.DropTable(
                name: "BlogPostMetrics");
        }
    }
}
