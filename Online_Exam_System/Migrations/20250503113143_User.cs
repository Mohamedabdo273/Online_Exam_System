using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Online_Exam_System.Migrations
{
    /// <inheritdoc />
    public partial class User : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_Choices_QuestionId",
                table: "UserAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_UserExams_QuestionId",
                table: "UserAnswers");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_SelectedChoiceId",
                table: "UserAnswers",
                column: "SelectedChoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_UserExamId",
                table: "UserAnswers",
                column: "UserExamId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_Choices_SelectedChoiceId",
                table: "UserAnswers",
                column: "SelectedChoiceId",
                principalTable: "Choices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_UserExams_UserExamId",
                table: "UserAnswers",
                column: "UserExamId",
                principalTable: "UserExams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_Choices_SelectedChoiceId",
                table: "UserAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_UserExams_UserExamId",
                table: "UserAnswers");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_SelectedChoiceId",
                table: "UserAnswers");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_UserExamId",
                table: "UserAnswers");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_Choices_QuestionId",
                table: "UserAnswers",
                column: "QuestionId",
                principalTable: "Choices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_UserExams_QuestionId",
                table: "UserAnswers",
                column: "QuestionId",
                principalTable: "UserExams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
