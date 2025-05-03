using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rooms.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenanceTicketProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssignedToId",
                table: "MaintenanceTickets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "MaintenanceTickets",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ReporterById",
                table: "MaintenanceTickets",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedToId",
                table: "MaintenanceTickets");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "MaintenanceTickets");

            migrationBuilder.DropColumn(
                name: "ReporterById",
                table: "MaintenanceTickets");
        }
    }
}
