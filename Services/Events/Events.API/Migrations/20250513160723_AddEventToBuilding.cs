using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Events.API.Migrations
{
    /// <inheritdoc />
    public partial class AddEventToBuilding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BuildingId",
                table: "Events",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RoomId",
                table: "Events",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuildingId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "Events");
        }
    }
}
