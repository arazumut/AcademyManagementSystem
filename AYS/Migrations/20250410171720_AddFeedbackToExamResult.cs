using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AYS.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedbackToExamResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Comments",
                table: "ExamResults",
                newName: "Feedback");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Feedback",
                table: "ExamResults",
                newName: "Comments");
        }
    }
}
