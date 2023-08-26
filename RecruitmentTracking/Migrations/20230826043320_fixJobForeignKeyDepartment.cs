using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecruitmentTracking.Migrations
{
    /// <inheritdoc />
    public partial class fixJobForeignKeyDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Departments_DeptId",
                table: "Jobs");

            migrationBuilder.RenameColumn(
                name: "candidateCount",
                table: "Jobs",
                newName: "CandidateCount");

            migrationBuilder.RenameColumn(
                name: "DeptId",
                table: "Jobs",
                newName: "DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Jobs_DeptId",
                table: "Jobs",
                newName: "IX_Jobs_DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Departments_DepartmentId",
                table: "Jobs",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Departments_DepartmentId",
                table: "Jobs");

            migrationBuilder.RenameColumn(
                name: "CandidateCount",
                table: "Jobs",
                newName: "candidateCount");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                table: "Jobs",
                newName: "DeptId");

            migrationBuilder.RenameIndex(
                name: "IX_Jobs_DepartmentId",
                table: "Jobs",
                newName: "IX_Jobs_DeptId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Departments_DeptId",
                table: "Jobs",
                column: "DeptId",
                principalTable: "Departments",
                principalColumn: "DepartmentId");
        }
    }
}
