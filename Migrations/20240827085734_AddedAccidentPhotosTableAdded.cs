using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FleetPulse_BackEndDevelopment.Migrations
{
    public partial class AddedAccidentPhotosTableAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccidentPhoto_Accidents_AccidentId",
                table: "AccidentPhoto");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AccidentPhoto",
                table: "AccidentPhoto");

            migrationBuilder.RenameTable(
                name: "AccidentPhoto",
                newName: "AccidentPhotos");

            migrationBuilder.RenameIndex(
                name: "IX_AccidentPhoto_AccidentId",
                table: "AccidentPhotos",
                newName: "IX_AccidentPhotos_AccidentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AccidentPhotos",
                table: "AccidentPhotos",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AccidentPhotos_Accidents_AccidentId",
                table: "AccidentPhotos",
                column: "AccidentId",
                principalTable: "Accidents",
                principalColumn: "AccidentId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccidentPhotos_Accidents_AccidentId",
                table: "AccidentPhotos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AccidentPhotos",
                table: "AccidentPhotos");

            migrationBuilder.RenameTable(
                name: "AccidentPhotos",
                newName: "AccidentPhoto");

            migrationBuilder.RenameIndex(
                name: "IX_AccidentPhotos_AccidentId",
                table: "AccidentPhoto",
                newName: "IX_AccidentPhoto_AccidentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AccidentPhoto",
                table: "AccidentPhoto",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AccidentPhoto_Accidents_AccidentId",
                table: "AccidentPhoto",
                column: "AccidentId",
                principalTable: "Accidents",
                principalColumn: "AccidentId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
