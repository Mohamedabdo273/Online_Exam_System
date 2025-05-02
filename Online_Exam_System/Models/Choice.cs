using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Models
{
    public class Choice
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string? Text { get; set; }
        public bool IsCorrect { get; set; }
        [ValidateNever]
        public Question? Question { get; set; }
        [ValidateNever]
        public ICollection<UserAnswer> UserAnswers { get; set; }
    }

}
