using Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure.Services.Iservices
{
    public interface IUserAnswerService
    {
        Task<IEnumerable<UserAnswer>> GetAllAsync();
        Task<UserAnswer?> GetByIdAsync(int id);
        Task CreateAsync(UserAnswer userAnswer);
        Task UpdateAsync(UserAnswer userAnswer);
        Task DeleteAsync(int id);
    }
}
