using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnboardingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddHeader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HeadUserID",
                table: "Departments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_HeadUserID",
                table: "Departments",
                column: "HeadUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Users_Head",
                table: "Departments",
                column: "HeadUserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Users_Head",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_HeadUserID",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "HeadUserID",
                table: "Departments");
        }
    }
}
