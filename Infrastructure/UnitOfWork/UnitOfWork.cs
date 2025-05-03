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
        private IDbContextTransaction _transaction;

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
       
        public async Task CommitTransactionAsync()
        {
            await _transaction.CommitAsync();
            _transaction = null;
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                _transaction = null;
            }
        }

        public async Task CompleteAsync()
        {
            await _context.SaveChangesAsync();
        }
        public int Complete() => _context.SaveChanges();
        public Task<IDbContextTransaction> BeginTransactionAsync() => _context.Database.BeginTransactionAsync();
        public void Dispose() => _context.Dispose();
    }
}
