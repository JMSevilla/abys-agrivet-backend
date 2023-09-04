using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace abys_agrivet_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddNewTableLobbies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "appointment_lobby",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    phoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    branch_id = table.Column<int>(type: "int", nullable: false),
                    service_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    petInfo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    appointmentSchedule = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    isWalkedIn = table.Column<int>(type: "int", nullable: false),
                    notify = table.Column<int>(type: "int", nullable: true),
                    reminderType = table.Column<int>(type: "int", nullable: false),
                    isSessionStarted = table.Column<int>(type: "int", nullable: true),
                    managersId = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appointment_lobby", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "appointment_lobby");
        }
    }
}
