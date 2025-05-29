using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rooms.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomPhotosCollection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "Rooms");

            migrationBuilder.AddColumn<string>(
                name: "PhotoIds",
                table: "Rooms",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoIds",
                table: "Rooms");

            migrationBuilder.AddColumn<Guid>(
                name: "ImageId",
                table: "Rooms",
                type: "TEXT",
                nullable: true);
        }
    }
}
