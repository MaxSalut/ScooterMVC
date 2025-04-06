using Microsoft.EntityFrameworkCore.Migrations;

namespace ScooterInfrastructure.Migrations
{
    public partial class AddApplicationUserIdToRider : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Rider",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rider_ApplicationUserId",
                table: "Rider",
                column: "ApplicationUserId",
                unique: true,
                filter: "[ApplicationUserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Rider_AspNetUsers_ApplicationUserId",
                table: "Rider",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rider_AspNetUsers_ApplicationUserId",
                table: "Rider");

            migrationBuilder.DropIndex(
                name: "IX_Rider_ApplicationUserId",
                table: "Rider");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Rider");
        }
    }
}