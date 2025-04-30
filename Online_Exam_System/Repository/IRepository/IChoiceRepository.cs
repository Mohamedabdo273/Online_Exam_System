using Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure.Repository.IRepository
{
    public interface IChoiceRepository : IRepository<Choice>
    {
        Task<IEnumerable<Choice>> GetChoicesByQuestionIdAsync(int questionId);

    }
}
