using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Models
{
    public class Question
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        public string Title { get; set; }

        public Exam? Exam { get; set; }
        public List<Choice> Choices { get; set; } = new List<Choice>(); 
        public ICollection<UserAnswer> userAnswers { get; set; }
    }

}
