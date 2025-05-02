using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Models.Models
{
    public class UserAnswer
    {
        public int Id { get; set; }
        public int UserExamId { get; set; }
        public int QuestionId { get; set; }
        public int SelectedChoiceId { get; set; }
        public bool IsCorrect { get; set; }
        [ValidateNever]
        public UserExam? UserExam { get; set; }
        [ValidateNever]
        public Question? Question { get; set; }
        [ValidateNever]
        public Choice? SelectedChoice { get; set; }
    }
}