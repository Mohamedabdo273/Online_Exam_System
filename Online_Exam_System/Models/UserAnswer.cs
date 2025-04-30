using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Models
{
    public class UserAnswer
    {
        public int Id { get; set; }
        public int UserExamId { get; set; }
        public int QuestionId { get; set; }
        public int SelectedChoiceId { get; set; }

        public UserExam UserExam { get; set; }
        public Question Question { get; set; }
        public Choice SelectedChoice { get; set; }
    }

}
