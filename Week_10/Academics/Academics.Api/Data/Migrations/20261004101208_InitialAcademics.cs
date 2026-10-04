using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Academics.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialAcademics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    RollNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    InternalNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EnrolledOn = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.CheckConstraint("CK_Students_Age", "Age BETWEEN 5 AND 100");
                    table.CheckConstraint("CK_Students_Score", "Score BETWEEN 0 AND 100");
                });

            migrationBuilder.CreateTable(
                name: "Teachers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Designation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teachers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Credits = table.Column<int>(type: "int", nullable: false),
                    TeacherId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                    table.CheckConstraint("CK_Courses_Credits", "Credits BETWEEN 1 AND 10");
                    table.ForeignKey(
                        name: "FK_Courses_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Enrollments",
                columns: table => new
                {
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    EnrolledOn = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollments", x => new { x.StudentId, x.CourseId });
                    table.ForeignKey(
                        name: "FK_Enrollments_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Enrollments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Courses",
                columns: new[] { "Id", "Code", "Credits", "TeacherId", "Title" },
                values: new object[] { 3, "CS101", 3, null, "Programming in C#" });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "Id", "Age", "Email", "EnrolledOn", "InternalNotes", "Name", "RollNumber", "Score" },
                values: new object[,]
                {
                    { 1, 20, "asha.kumar@school.example", new DateOnly(2026, 6, 1), "", "Asha Kumar", "R001", 82 },
                    { 2, 21, "rohit.menon@school.example", new DateOnly(2026, 6, 1), "", "Rohit Menon", "R002", 64 },
                    { 3, 19, "divya.nair@school.example", new DateOnly(2026, 6, 15), "Scholarship review pending", "Divya Nair", "R003", 91 }
                });

            migrationBuilder.InsertData(
                table: "Teachers",
                columns: new[] { "Id", "Designation", "Email", "Name" },
                values: new object[,]
                {
                    { 1, "Mathematics", "meera.iyer@school.example", "Meera Iyer" },
                    { 2, "Physics", "arjun.rao@school.example", "Arjun Rao" }
                });

            migrationBuilder.InsertData(
                table: "Courses",
                columns: new[] { "Id", "Code", "Credits", "TeacherId", "Title" },
                values: new object[,]
                {
                    { 1, "MATH101", 4, 1, "Calculus I" },
                    { 2, "PHYS101", 4, 2, "Mechanics" }
                });

            migrationBuilder.InsertData(
                table: "Enrollments",
                columns: new[] { "CourseId", "StudentId", "EnrolledOn" },
                values: new object[,]
                {
                    { 3, 1, new DateOnly(2026, 6, 2) },
                    { 1, 1, new DateOnly(2026, 6, 2) },
                    { 2, 2, new DateOnly(2026, 6, 3) },
                    { 1, 3, new DateOnly(2026, 6, 16) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_Code",
                table: "Courses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_TeacherId",
                table: "Courses",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_CourseId",
                table: "Enrollments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_Email",
                table: "Students",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_RollNumber",
                table: "Students",
                column: "RollNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_Email",
                table: "Teachers",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Enrollments");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Teachers");
        }
    }
}
