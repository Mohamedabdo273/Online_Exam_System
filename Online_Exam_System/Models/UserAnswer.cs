using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Models.Models
{
    public class UserAnswer
    {
        [Key]
        public int Id { get; set; }

        public int UserExamId { get; set; }

        public int QuestionId { get; set; }

        public int SelectedChoiceId { get; set; }

        public bool IsCorrect { get; set; }

        [ForeignKey("UserExamId")]
        public UserExam? UserExam { get; set; }

        [ForeignKey("QuestionId")]
        public Question? Question { get; set; }

        [ForeignKey("SelectedChoiceId")]
        public Choice? SelectedChoice { get; set; }
    }
}