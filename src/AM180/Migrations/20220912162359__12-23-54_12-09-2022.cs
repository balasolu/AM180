using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AM180.Migrations
{
    public partial class _122354_12092022 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Expiration",
                table: "Tokens",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Hash",
                table: "Tokens",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Tokens",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_UserId",
                table: "Tokens",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tokens_Users_UserId",
                table: "Tokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tokens_Users_UserId",
                table: "Tokens");

            migrationBuilder.DropIndex(
                name: "IX_Tokens_UserId",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "Expiration",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "Hash",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Tokens");
        }
    }
}
