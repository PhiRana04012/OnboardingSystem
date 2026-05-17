using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnboardingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddJobTitles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "JobTitleID",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "JobTitles",
                columns: table => new
                {
                    JobTitleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__JobTitle__C64C6E0DB3EE15E9", x => x.JobTitleID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_JobTitleID",
                table: "Users",
                column: "JobTitleID");

            migrationBuilder.CreateIndex(
                name: "UQ__JobTitle__A1D5E8A64E51E3C0",
                table: "JobTitles",
                column: "Title",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_JobTitles",
                table: "Users",
                column: "JobTitleID",
                principalTable: "JobTitles",
                principalColumn: "JobTitleID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_JobTitles",
                table: "Users");

            migrationBuilder.DropTable(
                name: "JobTitles");

            migrationBuilder.DropIndex(
                name: "IX_Users_JobTitleID",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "JobTitleID",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "JobTitle",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
