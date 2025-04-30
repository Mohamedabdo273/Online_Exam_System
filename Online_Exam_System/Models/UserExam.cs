using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Models
{
    public class UserExam
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int ExamId { get; set; }
        public double Score { get; set; }
        public bool Passed { get; set; }
        public DateTime TakenAt { get; set; }

        public ApplicationUser User { get; set; }
        public Exam Exam { get; set; }
        public ICollection<UserAnswer> UserAnswers { get; set; }
    }

}
