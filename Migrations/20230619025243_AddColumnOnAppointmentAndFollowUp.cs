using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace abys_agrivet_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnOnAppointmentAndFollowUp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "managersId",
                table: "follow_up_appointment",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "managersId",
                table: "appointment",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "managersId",
                table: "follow_up_appointment");

            migrationBuilder.DropColumn(
                name: "managersId",
                table: "appointment");
        }
    }
}
