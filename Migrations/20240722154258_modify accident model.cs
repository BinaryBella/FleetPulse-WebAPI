using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetPulse_BackEndDevelopment.Migrations
{
    public partial class modifyaccidentmodel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccidentUsers");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Accidents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Accidents_UserId",
                table: "Accidents",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accidents_Users_UserId",
                table: "Accidents",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accidents_Users_UserId",
                table: "Accidents");

            migrationBuilder.DropIndex(
                name: "IX_Accidents_UserId",
                table: "Accidents");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Accidents");

            migrationBuilder.CreateTable(
                name: "AccidentUsers",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AccidentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccidentUsers", x => new { x.UserId, x.AccidentId });
                    table.ForeignKey(
                        name: "FK_AccidentUsers_Accidents_AccidentId",
                        column: x => x.AccidentId,
                        principalTable: "Accidents",
                        principalColumn: "AccidentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccidentUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccidentUsers_AccidentId",
                table: "AccidentUsers",
                column: "AccidentId");
        }
    }
}
