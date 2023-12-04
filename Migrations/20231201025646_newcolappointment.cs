using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace abys_agrivet_backend.Migrations
{
    /// <inheritdoc />
    public partial class newcolappointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "scheduleId",
                table: "appointment",
                type: "int",
                nullable: false,
                defaultValue: 0
                );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "scheduleId",
                table: "appointment");
        }
    }
}
