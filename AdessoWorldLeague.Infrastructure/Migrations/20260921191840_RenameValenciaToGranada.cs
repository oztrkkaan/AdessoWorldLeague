using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdessoWorldLeague.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameValenciaToGranada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Teams",
                keyColumn: "Id",
                keyValue: 32,
                column: "Name",
                value: "Adesso Granada");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Teams",
                keyColumn: "Id",
                keyValue: 32,
                column: "Name",
                value: "Adesso Valencia");
        }
    }
}
