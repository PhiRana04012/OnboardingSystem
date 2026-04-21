using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnboardingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddChecklists2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChecklistItems",
                columns: table => new
                {
                    ChecklistItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuleId = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistItems", x => x.ChecklistItemId);
                    table.ForeignKey(
                        name: "FK_ChecklistItems_Modules",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "ModuleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserChecklistItems",
                columns: table => new
                {
                    UserChecklistItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ChecklistItemId = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserChecklistItems", x => x.UserChecklistItemId);
                    table.ForeignKey(
                        name: "FK_UserChecklistItems_ChecklistItems",
                        column: x => x.ChecklistItemId,
                        principalTable: "ChecklistItems",
                        principalColumn: "ChecklistItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserChecklistItems_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistItems_ModuleId",
                table: "ChecklistItems",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserChecklistItems_ChecklistItemId",
                table: "UserChecklistItems",
                column: "ChecklistItemId");

            migrationBuilder.CreateIndex(
                name: "IX_UserChecklistItems_UserId_ChecklistItemId",
                table: "UserChecklistItems",
                columns: new[] { "UserId", "ChecklistItemId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserChecklistItems");

            migrationBuilder.DropTable(
                name: "ChecklistItems");
        }
    }
}
