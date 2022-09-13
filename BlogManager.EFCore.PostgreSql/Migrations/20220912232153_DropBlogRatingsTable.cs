using Microsoft.EntityFrameworkCore.Migrations;

namespace BlogManager.EFCore.PostgreSql.Migrations
{
    public partial class DropBlogRatingsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlogRatings");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlogRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    BlogId = table.Column<int>(type: "int", nullable: false),
                    StarRating = table.Column<decimal>(type: "decimal", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlogRatings_Blogs_BlogId",
                        column: x => x.BlogId,
                        principalTable: "Blogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlogRatings_BlogId",
                table: "BlogRatings",
                column: "BlogId");
        }
    }
}
