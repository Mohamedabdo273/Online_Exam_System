using infrastructure.Data;
using infrastructure.Repository.IRepository;
using Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure.Repository
{
    public class UserExamRepository : Repository<UserExam>, IUserExamRepository
    {
        public UserExamRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
