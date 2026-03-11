using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IlkProjem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToServiceLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ServiceLog",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ServiceLog");
        }
    }
}
