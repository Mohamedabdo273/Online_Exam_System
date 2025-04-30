using infrastructure.Data;
using infrastructure.Repository;
using infrastructure.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading.Tasks;

namespace infrastructures.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            ChoiceRepository = new ChoiceRepository(_context);
            ExamRepository = new ExamRepository(_context);
            QuestionRepository = new QuestionRepository(_context);
            UserAnswerRepository = new UserAnswerRepository(_context);
            UserExamRepository = new UserExamRepository(_context);
        }

        public IChoiceRepository ChoiceRepository { get; set; }
        public IExamRepository ExamRepository { get; set; }
        public IQuestionRepository QuestionRepository { get; set; }
        public IUserAnswerRepository UserAnswerRepository { get; set; }
        public IUserExamRepository UserExamRepository { get; set; }

        public int Complete() => _context.SaveChanges();
        public Task CompleteAsync() => _context.SaveChangesAsync();
        public Task<IDbContextTransaction> BeginTransactionAsync() => _context.Database.BeginTransactionAsync();
        public void Dispose() => _context.Dispose();
    }
}
