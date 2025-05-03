using Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure.Services.Iservices
{
    public interface IQuestionService
    {
        Task<IEnumerable<Question>> GetAllAsync();
        Task<Question?> GetByIdAsync(int id);
        Task<IEnumerable<Question>> GetByExamIdAsync(int examId); // Add this

        Task CreateAsync(Question exam);
        Task UpdateAsync(Question exam);
        Task DeleteAsync(int id);

    }
}
