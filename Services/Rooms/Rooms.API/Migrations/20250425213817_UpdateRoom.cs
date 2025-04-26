using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rooms.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "MaintenanceTickets",
                newName: "IsResolved");

            migrationBuilder.RenameColumn(
                name: "ReportedBy",
                table: "MaintenanceTickets",
                newName: "Title");

            migrationBuilder.AlterColumn<Guid>(
                name: "BlockId",
                table: "Rooms",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "Purpose",
                table: "Rooms",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RoomType",
                table: "Rooms",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResolvedAt",
                table: "MaintenanceTickets",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Buildings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Address = table.Column<string>(type: "TEXT", nullable: false),
                    FloorsCount = table.Column<int>(type: "INTEGER", nullable: false),
                    YearBuilt = table.Column<int>(type: "INTEGER", nullable: false),
                    AdministratorContact = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buildings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_BlockId",
                table: "Rooms",
                column: "BlockId");

            migrationBuilder.CreateIndex(
                name: "IX_Places_RoomId",
                table: "Places",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceTickets_RoomId",
                table: "MaintenanceTickets",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Floors_BuildingId",
                table: "Floors",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_FloorId",
                table: "Blocks",
                column: "FloorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Blocks_Floors_FloorId",
                table: "Blocks",
                column: "FloorId",
                principalTable: "Floors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Floors_Buildings_BuildingId",
                table: "Floors",
                column: "BuildingId",
                principalTable: "Buildings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceTickets_Rooms_RoomId",
                table: "MaintenanceTickets",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Places_Rooms_RoomId",
                table: "Places",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Blocks_BlockId",
                table: "Rooms",
                column: "BlockId",
                principalTable: "Blocks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blocks_Floors_FloorId",
                table: "Blocks");

            migrationBuilder.DropForeignKey(
                name: "FK_Floors_Buildings_BuildingId",
                table: "Floors");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceTickets_Rooms_RoomId",
                table: "MaintenanceTickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Places_Rooms_RoomId",
                table: "Places");

            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Blocks_BlockId",
                table: "Rooms");

            migrationBuilder.DropTable(
                name: "Buildings");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_BlockId",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Places_RoomId",
                table: "Places");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceTickets_RoomId",
                table: "MaintenanceTickets");

            migrationBuilder.DropIndex(
                name: "IX_Floors_BuildingId",
                table: "Floors");

            migrationBuilder.DropIndex(
                name: "IX_Blocks_FloorId",
                table: "Blocks");

            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "RoomType",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "ResolvedAt",
                table: "MaintenanceTickets");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "MaintenanceTickets",
                newName: "ReportedBy");

            migrationBuilder.RenameColumn(
                name: "IsResolved",
                table: "MaintenanceTickets",
                newName: "Status");

            migrationBuilder.AlterColumn<Guid>(
                name: "BlockId",
                table: "Rooms",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
