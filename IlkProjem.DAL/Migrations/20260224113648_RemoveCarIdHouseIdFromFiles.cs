using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IlkProjem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCarIdHouseIdFromFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Cars_CarId",
                table: "Files");

            migrationBuilder.DropForeignKey(
                name: "FK_Files_Houses_HouseId",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_CarId",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_HouseId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "CarId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "HouseId",
                table: "Files");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CarId",
                table: "Files",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HouseId",
                table: "Files",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Files_CarId",
                table: "Files",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_HouseId",
                table: "Files",
                column: "HouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Cars_CarId",
                table: "Files",
                column: "CarId",
                principalTable: "Cars",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Houses_HouseId",
                table: "Files",
                column: "HouseId",
                principalTable: "Houses",
                principalColumn: "Id");
        }
    }
}
