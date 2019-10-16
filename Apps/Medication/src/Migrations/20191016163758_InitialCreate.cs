using Microsoft.EntityFrameworkCore.Migrations;

namespace Medication.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "trace_seq",
                minValue: 1L,
                maxValue: 999999L,
                cyclic: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "trace_seq");
        }
    }
}
