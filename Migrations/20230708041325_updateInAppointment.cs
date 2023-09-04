using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace abys_agrivet_backend.Migrations
{
    /// <inheritdoc />
    public partial class updateInAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "archive_indicator",
                table: "appointment",
                type: "datetime",
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "archive_indicator",
                table: "appointment"
            );
        }
    }
}
