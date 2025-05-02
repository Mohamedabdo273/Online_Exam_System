using Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure.Services.Iservices
{
    public interface IUserExamService
    {
        Task<IEnumerable<UserExam>> GetAllAsync();
        Task<UserExam?> GetByIdAsync(int id);
        Task<bool> HasUserTakenExam(string userId, int examId);
        Task CreateAsync(UserExam userExam);
        Task UpdateAsync(UserExam userExam);
        Task DeleteAsync(int id);
    }
}
