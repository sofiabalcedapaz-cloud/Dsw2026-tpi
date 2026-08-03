using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dsw2026Tpi.Data.Migrations.Domain
{
    /// <inheritdoc />
    public partial class AddPatientsAppointments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_APPOINTMENTS_AvailabilitySlotId",
                table: "APPOINTMENTS");

            migrationBuilder.CreateIndex(
                name: "IX_APPOINTMENTS_AvailabilitySlotId",
                table: "APPOINTMENTS",
                column: "AvailabilitySlotId",
                unique: true,
                filter: "[Status] = 'BOOKED'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_APPOINTMENTS_AvailabilitySlotId",
                table: "APPOINTMENTS");

            migrationBuilder.CreateIndex(
                name: "IX_APPOINTMENTS_AvailabilitySlotId",
                table: "APPOINTMENTS",
                column: "AvailabilitySlotId",
                unique: true);
        }
    }
}
