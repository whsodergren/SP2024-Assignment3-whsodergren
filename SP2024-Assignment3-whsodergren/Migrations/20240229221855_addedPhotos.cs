using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SP2024_Assignment3_whsodergren.Migrations
{
    /// <inheritdoc />
    public partial class addedPhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "MovieImage",
                table: "Movie",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ActorImage",
                table: "Actor",
                type: "varbinary(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MovieImage",
                table: "Movie");

            migrationBuilder.DropColumn(
                name: "ActorImage",
                table: "Actor");
        }
    }
}
