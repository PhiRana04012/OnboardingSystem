using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnboardingSystem.Migrations
{
    public partial class AddModuleDepartments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModuleDepartments",
                columns: table => new
                {
                    ModuleID = table.Column<int>(type: "int", nullable: false),
                    DepartmentID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleDepartments", x => new { x.ModuleID, x.DepartmentID });
                    table.ForeignKey(
                        name: "FK_ModuleDepartments_Departments",
                        column: x => x.DepartmentID,
                        principalTable: "Departments",
                        principalColumn: "DepartmentID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModuleDepartments_Modules",
                        column: x => x.ModuleID,
                        principalTable: "Modules",
                        principalColumn: "ModuleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModuleDepartments_DepartmentID",
                table: "ModuleDepartments",
                column: "DepartmentID");

            migrationBuilder.Sql(@"
                INSERT INTO ModuleDepartments (ModuleID, DepartmentID)
                SELECT ModuleID, DepartmentID
                FROM Modules
                WHERE DepartmentID IS NOT NULL
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModuleDepartments");
        }
    }
}
