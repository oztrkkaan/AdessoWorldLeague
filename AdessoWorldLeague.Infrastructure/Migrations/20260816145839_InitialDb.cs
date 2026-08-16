using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AdessoWorldLeague.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Draws",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatorFullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Draws", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teams_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DrawGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    DrawId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrawGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrawGroups_Draws_DrawId",
                        column: x => x.DrawId,
                        principalTable: "Draws",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DrawTeamAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DrawGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeamId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrawTeamAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrawTeamAssignments_DrawGroups_DrawGroupId",
                        column: x => x.DrawGroupId,
                        principalTable: "DrawGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DrawTeamAssignments_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Türkiye" },
                    { 2, "Almanya" },
                    { 3, "Belçika" },
                    { 4, "Fransa" },
                    { 5, "Hollanda" },
                    { 6, "Portekiz" },
                    { 7, "İtalya" },
                    { 8, "İspanya" }
                });

            migrationBuilder.InsertData(
                table: "Teams",
                columns: new[] { "Id", "CountryId", "Name" },
                values: new object[,]
                {
                    { 1, 1, "Adesso İstanbul" },
                    { 2, 1, "Adesso Ankara" },
                    { 3, 1, "Adesso İzmir" },
                    { 4, 1, "Adesso Antalya" },
                    { 5, 2, "Adesso Berlin" },
                    { 6, 2, "Adesso Frankfurt" },
                    { 7, 2, "Adesso Münih" },
                    { 8, 2, "Adesso Dortmund" },
                    { 9, 3, "Adesso Brüksel" },
                    { 10, 3, "Adesso Brugge" },
                    { 11, 3, "Adesso Anvers" },
                    { 12, 3, "Adesso Gent" },
                    { 13, 4, "Adesso Paris" },
                    { 14, 4, "Adesso Marsilya" },
                    { 15, 4, "Adesso Nice" },
                    { 16, 4, "Adesso Lyon" },
                    { 17, 5, "Adesso Amsterdam" },
                    { 18, 5, "Adesso Rotterdam" },
                    { 19, 5, "Adesso Lahey" },
                    { 20, 5, "Adesso Eindhoven" },
                    { 21, 6, "Adesso Lisbon" },
                    { 22, 6, "Adesso Porto" },
                    { 23, 6, "Adesso Braga" },
                    { 24, 6, "Adesso Coimbra" },
                    { 25, 7, "Adesso Roma" },
                    { 26, 7, "Adesso Milano" },
                    { 27, 7, "Adesso Venedik" },
                    { 28, 7, "Adesso Napoli" },
                    { 29, 8, "Adesso Madrid" },
                    { 30, 8, "Adesso Barselona" },
                    { 31, 8, "Adesso Sevilla" },
                    { 32, 8, "Adesso Valencia" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DrawGroups_DrawId_GroupName",
                table: "DrawGroups",
                columns: new[] { "DrawId", "GroupName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DrawTeamAssignments_DrawGroupId",
                table: "DrawTeamAssignments",
                column: "DrawGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_DrawTeamAssignments_TeamId",
                table: "DrawTeamAssignments",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_CountryId",
                table: "Teams",
                column: "CountryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DrawTeamAssignments");

            migrationBuilder.DropTable(
                name: "DrawGroups");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "Draws");

            migrationBuilder.DropTable(
                name: "Countries");
        }
    }
}
