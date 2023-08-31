using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecruitmentTracking.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeptId",
                table: "Jobs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Department",
                columns: table => new
                {
                    DepartmentId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DepartmentName = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Department", x => x.DepartmentId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_DeptId",
                table: "Jobs",
                column: "DeptId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Department_DeptId",
                table: "Jobs",
                column: "DeptId",
                principalTable: "Department",
                principalColumn: "DepartmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Department_DeptId",
                table: "Jobs");

            migrationBuilder.DropTable(
                name: "Department");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_DeptId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "DeptId",
                table: "Jobs");
        }
    }
}
