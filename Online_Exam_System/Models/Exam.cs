using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Models.Models
{
    public class Exam
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? Title { get; set; }
        [Required]
        public string? Description { get; set; }
        public string? DurationInMinutes { get; set; }
        [ValidateNever]
        [JsonIgnore]       
        public ICollection<Question> Questions { get; set; }
        [ValidateNever]
        [JsonIgnore]
        public ICollection<UserExam> UserExams { get; set; }
    }

}
