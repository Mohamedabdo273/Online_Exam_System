using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Models
{
    public class Question
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        [Required]
        public string? Title { get; set; }
        [ValidateNever]
        public Exam? Exam { get; set; }
        [ValidateNever]
        public List<Choice> Choices { get; set; } = new List<Choice>();
        [ValidateNever]
        public ICollection<UserAnswer>? userAnswers { get; set; }
    }

}
