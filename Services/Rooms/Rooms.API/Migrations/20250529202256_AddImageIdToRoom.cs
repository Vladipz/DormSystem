using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rooms.API.Migrations
{
    /// <inheritdoc />
    public partial class AddImageIdToRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ImageId",
                table: "Rooms",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "Rooms");
        }
    }
}
