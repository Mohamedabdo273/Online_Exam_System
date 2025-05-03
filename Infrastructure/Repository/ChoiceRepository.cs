using infrastructure.Data;
using infrastructure.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure.Repository
{
    public class ChoiceRepository : Repository<Choice>, IChoiceRepository
    {
        private readonly ApplicationDbContext dbContext;

        public ChoiceRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<IEnumerable<Choice>> GetChoicesByQuestionIdAsync(int questionId)
        {
            return await dbContext.Choices
                .Where(c => c.QuestionId == questionId)
                .ToListAsync();
        }
    }
}
