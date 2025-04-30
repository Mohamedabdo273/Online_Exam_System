using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            :base(options)
        {           
        }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Choice> Choices { get; set; }
        public DbSet<UserExam> UserExams { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Exam>()
               .HasMany(e => e.Questions)
               .WithOne(q => q.Exam)
               .HasForeignKey(q => q.ExamId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Question>()
                .HasMany(q => q.Choices)
                .WithOne(c => c.Question)
                .HasForeignKey(c => c.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

           

            modelBuilder.Entity<UserExam>()
    .HasMany(ue => ue.UserAnswers)
    .WithOne(ua => ua.UserExam)
    .HasForeignKey(ua => ua.UserExamId)
    .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<UserAnswer>()
    .HasOne(ua => ua.Question)
    .WithMany(q => q.userAnswers)
    .HasForeignKey(ua => ua.QuestionId)
    .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UserAnswer>()
   .HasOne(ua => ua.UserExam)
   .WithMany(q => q.UserAnswers)
   .HasForeignKey(ua => ua.QuestionId)
   .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<UserAnswer>()
  .HasOne(ua => ua.SelectedChoice)
  .WithMany(q => q.UserAnswers)
  .HasForeignKey(ua => ua.QuestionId)
  .OnDelete(DeleteBehavior.Restrict);// أو .NoAction




        }
    }
}
