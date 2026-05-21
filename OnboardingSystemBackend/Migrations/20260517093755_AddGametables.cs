using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnboardingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddGametables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Achievements",
                columns: table => new
                {
                    AchievementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IconName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ConditionKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievements", x => x.AchievementId);
                });

            migrationBuilder.CreateTable(
                name: "FaqEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Question = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Общее"),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaqEntries", x => x.Id);
                });

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

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Roles__8AFACE3A04F18E8C", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "ActionLog",
                columns: table => new
                {
                    LogID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ActionLo__5E5499A8B7BEDF7C", x => x.LogID);
                });

            migrationBuilder.CreateTable(
                name: "AnswerOptions",
                columns: table => new
                {
                    AnswerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionID = table.Column<int>(type: "int", nullable: false),
                    AnswerText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AnswerOp__D482502408F6A591", x => x.AnswerID);
                });

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
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    DepartmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ExternalID = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HeadUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Departme__B2079BCD767D3C89", x => x.DepartmentID);
                });

            migrationBuilder.CreateTable(
                name: "Modules",
                columns: table => new
                {
                    ModuleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DepartmentID = table.Column<int>(type: "int", nullable: true),
                    PassingScore = table.Column<int>(type: "int", nullable: false, defaultValue: 80),
                    MaxAttempts = table.Column<int>(type: "int", nullable: false, defaultValue: 3)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Modules__2B747787B4D47241", x => x.ModuleID);
                    table.ForeignKey(
                        name: "FK_Modules_Departments",
                        column: x => x.DepartmentID,
                        principalTable: "Departments",
                        principalColumn: "DepartmentID");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalID = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DepartmentID = table.Column<int>(type: "int", nullable: false),
                    MentorID = table.Column<int>(type: "int", nullable: true),
                    HireDate = table.Column<DateOnly>(type: "date", nullable: false),
                    OnboardingStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Не начат"),
                    JobTitleID = table.Column<int>(type: "int", nullable: true),
                    TelegramTag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RimsLastSyncDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalXP = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__1788CCAC41930BCF", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_Users_Departments",
                        column: x => x.DepartmentID,
                        principalTable: "Departments",
                        principalColumn: "DepartmentID");
                    table.ForeignKey(
                        name: "FK_Users_JobTitles",
                        column: x => x.JobTitleID,
                        principalTable: "JobTitles",
                        principalColumn: "JobTitleID");
                    table.ForeignKey(
                        name: "FK_Users_Mentor",
                        column: x => x.MentorID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

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

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    QuestionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuleID = table.Column<int>(type: "int", nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Question__0DC06F8C277F87A3", x => x.QuestionID);
                    table.ForeignKey(
                        name: "FK_Questions_Modules",
                        column: x => x.ModuleID,
                        principalTable: "Modules",
                        principalColumn: "ModuleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetTokens",
                columns: table => new
                {
                    TokenId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetTokens", x => x.TokenId);
                    table.ForeignKey(
                        name: "FK_PasswordResetTokens_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestAttempts",
                columns: table => new
                {
                    AttemptID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ModuleID = table.Column<int>(type: "int", nullable: false),
                    AttemptDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    AttemptNumber = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    IsPassed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TestAtte__891A68868F130401", x => x.AttemptID);
                    table.ForeignKey(
                        name: "FK_TestAttempts_Modules",
                        column: x => x.ModuleID,
                        principalTable: "Modules",
                        principalColumn: "ModuleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TestAttempts_Users",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAchievements",
                columns: table => new
                {
                    UserAchievementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AchievementId = table.Column<int>(type: "int", nullable: false),
                    AwardedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAchievements", x => x.UserAchievementId);
                    table.ForeignKey(
                        name: "FK_UserAchievements_Achievements",
                        column: x => x.AchievementId,
                        principalTable: "Achievements",
                        principalColumn: "AchievementId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAchievements_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserID",
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

            migrationBuilder.CreateTable(
                name: "UserModuleProgress",
                columns: table => new
                {
                    ProgressID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    ModuleID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Не начат"),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserModu__BAE29C85994BA69E", x => x.ProgressID);
                    table.ForeignKey(
                        name: "FK_UserModuleProgress_Modules",
                        column: x => x.ModuleID,
                        principalTable: "Modules",
                        principalColumn: "ModuleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserModuleProgress_Users",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false),
                    RoleID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserRole__AF27604F7D17785F", x => new { x.UserID, x.RoleID });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles",
                        column: x => x.RoleID,
                        principalTable: "Roles",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "XpGrantLogs",
                columns: table => new
                {
                    XpGrantId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ModuleId = table.Column<int>(type: "int", nullable: true),
                    BaseXp = table.Column<int>(type: "int", nullable: false),
                    Multiplier = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    FinalXp = table.Column<int>(type: "int", nullable: false),
                    ReasonSummary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GrantedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XpGrantLogs", x => x.XpGrantId);
                    table.ForeignKey(
                        name: "FK_XpGrantLogs_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_ConditionKey",
                table: "Achievements",
                column: "ConditionKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActionLog_UserID",
                table: "ActionLog",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_AnswerOptions_QuestionID",
                table: "AnswerOptions",
                column: "QuestionID");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistItems_ModuleId",
                table: "ChecklistItems",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_HeadUserID",
                table: "Departments",
                column: "HeadUserID");

            migrationBuilder.CreateIndex(
                name: "UQ__Departme__737584F630BC325D",
                table: "Departments",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__JobTitle__A1D5E8A64E51E3C0",
                table: "JobTitles",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModuleDepartments_DepartmentID",
                table: "ModuleDepartments",
                column: "DepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_DepartmentID",
                table: "Modules",
                column: "DepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_Token",
                table: "PasswordResetTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_UserId",
                table: "PasswordResetTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_ModuleID",
                table: "Questions",
                column: "ModuleID");

            migrationBuilder.CreateIndex(
                name: "UQ__Roles__8A2B616047850BFA",
                table: "Roles",
                column: "RoleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestAttempts_ModuleID",
                table: "TestAttempts",
                column: "ModuleID");

            migrationBuilder.CreateIndex(
                name: "IX_TestAttempts_UserID",
                table: "TestAttempts",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_AchievementId",
                table: "UserAchievements",
                column: "AchievementId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_UserId_AchievementId",
                table: "UserAchievements",
                columns: new[] { "UserId", "AchievementId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserChecklistItems_ChecklistItemId",
                table: "UserChecklistItems",
                column: "ChecklistItemId");

            migrationBuilder.CreateIndex(
                name: "IX_UserChecklistItems_UserId_ChecklistItemId",
                table: "UserChecklistItems",
                columns: new[] { "UserId", "ChecklistItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserModuleProgress_ModuleID",
                table: "UserModuleProgress",
                column: "ModuleID");

            migrationBuilder.CreateIndex(
                name: "UQ__UserModu__753F8BD5DAF2E324",
                table: "UserModuleProgress",
                columns: new[] { "UserID", "ModuleID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleID",
                table: "UserRoles",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DepartmentID",
                table: "Users",
                column: "DepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_JobTitleID",
                table: "Users",
                column: "JobTitleID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_MentorID",
                table: "Users",
                column: "MentorID");

            migrationBuilder.CreateIndex(
                name: "UQ__Users__A9D10534CAA8D9F8",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_XpGrantLogs_UserId_GrantedAt",
                table: "XpGrantLogs",
                columns: new[] { "UserId", "GrantedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_ActionLog_Users",
                table: "ActionLog",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_AnswerOptions_Questions",
                table: "AnswerOptions",
                column: "QuestionID",
                principalTable: "Questions",
                principalColumn: "QuestionID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChecklistItems_Modules",
                table: "ChecklistItems",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "ModuleID",
                onDelete: ReferentialAction.Cascade);

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

            migrationBuilder.DropTable(
                name: "ActionLog");

            migrationBuilder.DropTable(
                name: "AnswerOptions");

            migrationBuilder.DropTable(
                name: "FaqEntries");

            migrationBuilder.DropTable(
                name: "ModuleDepartments");

            migrationBuilder.DropTable(
                name: "PasswordResetTokens");

            migrationBuilder.DropTable(
                name: "TestAttempts");

            migrationBuilder.DropTable(
                name: "UserAchievements");

            migrationBuilder.DropTable(
                name: "UserChecklistItems");

            migrationBuilder.DropTable(
                name: "UserModuleProgress");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "XpGrantLogs");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "Achievements");

            migrationBuilder.DropTable(
                name: "ChecklistItems");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Modules");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "JobTitles");
        }
    }
}
