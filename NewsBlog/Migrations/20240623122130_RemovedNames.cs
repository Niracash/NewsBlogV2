using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsBlog.Migrations
{
    /// <inheritdoc />
    public partial class RemovedNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorFirstName",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "AuthorLastName",
                table: "Posts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorFirstName",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthorLastName",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
