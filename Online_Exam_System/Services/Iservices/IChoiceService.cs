using Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure.Services.Iservices
{
     public interface IChoiceService
    {
        Task<IEnumerable<Choice>> GetAllAsync(int questionID);
        Task<Choice?> GetByIdAsync(int id);
        Task CreateAsync(Choice choice);
        Task UpdateAsync(Choice choice);
        Task DeleteAsync(int id);
        Task<IEnumerable<Choice>> GetChoicesByQuestionIdAsync(int questionId);
    }
}
