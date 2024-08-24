using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetPulse_BackEndDevelopment.Migrations
{
    public partial class modifytrip : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TripUsers");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Trips",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Trips_UserId",
                table: "Trips",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_Users_UserId",
                table: "Trips",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trips_Users_UserId",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_Trips_UserId",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Trips");

            migrationBuilder.CreateTable(
                name: "TripUsers",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TripId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripUsers", x => new { x.UserId, x.TripId });
                    table.ForeignKey(
                        name: "FK_TripUsers_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "TripId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TripUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TripUsers_TripId",
                table: "TripUsers",
                column: "TripId");
        }
    }
}
