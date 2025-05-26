using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Guide.Data.Migrations
{
    /// <inheritdoc />
    public partial class initialCommentsv2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "TourComments",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "TourComments");
        }
    }
}
