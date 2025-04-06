using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScooterInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRiderApplicationUserRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rider_AspNetUsers_ApplicationUserId",
                table: "Rider");

            migrationBuilder.AddForeignKey(
                name: "FK_Rider_AspNetUsers_ApplicationUserId",
                table: "Rider",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rider_AspNetUsers_ApplicationUserId",
                table: "Rider");

            migrationBuilder.AddForeignKey(
                name: "FK_Rider_AspNetUsers_ApplicationUserId",
                table: "Rider",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
