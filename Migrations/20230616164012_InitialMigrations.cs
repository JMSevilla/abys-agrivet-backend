using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace abys_agrivet_backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           

            migrationBuilder.CreateTable(
                name: "follow_up_appointment",
                columns: table => new
                {
                    followupId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id = table.Column<int>(type: "int", nullable: false),
                    customerPickServices = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    petInformation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    branch_id = table.Column<int>(type: "int", nullable: false),
                    customerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    followupServices = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    followupDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    notificationType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    diagnosis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    treatment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    start = table.Column<DateTime>(type: "datetime2", nullable: false),
                    end = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_follow_up_appointment", x => x.followupId);
                    table.ForeignKey(
                        name: "FK_Appointment_Id",
                        column: x => x.id,
                        principalTable: "appointment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.DropTable(
                name: "follow_up_appointment");
        }
    }
}
