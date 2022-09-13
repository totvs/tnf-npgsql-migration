using Microsoft.EntityFrameworkCore.Migrations;

namespace BlogManager.EFCore.PostgreSql.Migrations
{
    public partial class ResetingBlogRatings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"BlogRatings\" SET \"StarRating\" = 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
