using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace News_Portal_1._1.Migrations
{
    /// <inheritdoc />
    public partial class mig12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DislikeCount",
                table: "ForumReplies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DislikedUsers",
                table: "ForumReplies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LikeCount",
                table: "ForumReplies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LikedUsers",
                table: "ForumReplies",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DislikeCount",
                table: "ForumReplies");

            migrationBuilder.DropColumn(
                name: "DislikedUsers",
                table: "ForumReplies");

            migrationBuilder.DropColumn(
                name: "LikeCount",
                table: "ForumReplies");

            migrationBuilder.DropColumn(
                name: "LikedUsers",
                table: "ForumReplies");
        }
    }
}
