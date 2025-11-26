using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnboardingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddRimsIntegrationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add ExternalID column to Departments table
            migrationBuilder.AddColumn<string>(
                name: "ExternalID",
                table: "Departments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            // Add JobTitle column to Users table
            migrationBuilder.AddColumn<string>(
                name: "JobTitle",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            // Add RimsLastSyncDate column to Users table
            migrationBuilder.AddColumn<DateTime>(
                name: "RimsLastSyncDate",
                table: "Users",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove ExternalID column from Departments table
            migrationBuilder.DropColumn(
                name: "ExternalID",
                table: "Departments");

            // Remove JobTitle column from Users table
            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "Users");

            // Remove RimsLastSyncDate column from Users table
            migrationBuilder.DropColumn(
                name: "RimsLastSyncDate",
                table: "Users");
        }
    }
}
