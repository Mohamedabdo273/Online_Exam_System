using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Models
{
    public class Exam
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string DurationInMinutes { get; set; }
  
        public ICollection<Question> Questions { get; set; }
        public ICollection<UserExam> UserExams { get; set; }
    }

}
