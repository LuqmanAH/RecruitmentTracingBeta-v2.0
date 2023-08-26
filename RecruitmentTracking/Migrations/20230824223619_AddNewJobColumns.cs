using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecruitmentTracking.Migrations
{
    /// <inheritdoc />
    public partial class AddNewJobColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmploymentType",
                table: "Jobs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "JobDepartment",
                table: "Jobs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "JobMinEducation",
                table: "Jobs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmploymentType",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "JobDepartment",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "JobMinEducation",
                table: "Jobs");
        }
    }
}
