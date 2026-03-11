using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IlkProjem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class StampLegacyAuditData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Eski kayıtları 'System_Legacy' etiketi ile damgala
            migrationBuilder.Sql("""
                UPDATE "Customers"
                SET "CreatedBy" = 'System_Legacy',
                    "CreatedAt" = NOW(),
                    "CreatedByUserId" = 0,
                    "IsActive" = true,
                    "IsDeleted" = false
                WHERE "CreatedBy" IS NULL;
            """);

            migrationBuilder.Sql("""
                UPDATE "Cars"
                SET "CreatedBy" = 'System_Legacy',
                    "CreatedAt" = NOW(),
                    "CreatedByUserId" = 0,
                    "IsActive" = true,
                    "IsDeleted" = false
                WHERE "CreatedBy" IS NULL;
            """);

            migrationBuilder.Sql("""
                UPDATE "Houses"
                SET "CreatedBy" = 'System_Legacy',
                    "CreatedAt" = NOW(),
                    "CreatedByUserId" = 0,
                    "IsActive" = true,
                    "IsDeleted" = false
                WHERE "CreatedBy" IS NULL;
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Geri alma: damgalanan kayıtları eski haline döndür
            migrationBuilder.Sql("""
                UPDATE "Customers"
                SET "CreatedBy" = NULL, "CreatedByUserId" = NULL
                WHERE "CreatedBy" = 'System_Legacy';
            """);

            migrationBuilder.Sql("""
                UPDATE "Cars"
                SET "CreatedBy" = NULL, "CreatedByUserId" = NULL
                WHERE "CreatedBy" = 'System_Legacy';
            """);

            migrationBuilder.Sql("""
                UPDATE "Houses"
                SET "CreatedBy" = NULL, "CreatedByUserId" = NULL
                WHERE "CreatedBy" = 'System_Legacy';
            """);
        }
    }
}
