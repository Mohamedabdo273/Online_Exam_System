using infrastructure.Services.Iservices;
using infrastructures.UnitOfWork;
using Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure.Services
{
    public class ExamService : IExamService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExamService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Exam>> GetAllAsync()
        {
            return await _unitOfWork.ExamRepository.GetAsync();
        }

        public async Task<Exam?> GetByIdAsync(int id)
        {
            return await _unitOfWork.ExamRepository.GetOneAsync(expression: e => e.Id == id);
        }

        public async Task CreateAsync(Exam exam)
        {
            await _unitOfWork.ExamRepository.CreateAsync(exam);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateAsync(Exam exam)
        {
            _unitOfWork.ExamRepository.Edit(exam);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var exam = await GetByIdAsync(id);
            if (exam != null)
            {
                _unitOfWork.ExamRepository.Delete(exam);
                await _unitOfWork.CompleteAsync();
            }
        }
    }
}
