using infrastructure.Repository.IRepository;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastructures.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
       public IChoiceRepository ChoiceRepository { get; }
       public IExamRepository ExamRepository { get; }
       public IQuestionRepository QuestionRepository { get; }
       public IUserAnswerRepository UserAnswerRepository { get; }
       public IUserExamRepository UserExamRepository { get; }
        



        int Complete();
        Task CompleteAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
