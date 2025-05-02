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
   public class QuestionService : IQuestionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public QuestionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Question>> GetAllAsync()
        {
            return await _unitOfWork.QuestionRepository.GetAsync([e => e.Choices]);
        }

        public async Task<Question?> GetByIdAsync(int id)
        {
            return await _unitOfWork.QuestionRepository.GetOneAsync([e=>e.Choices],expression: e => e.Id == id);
        }
        public async Task<IEnumerable<Question>> GetByExamIdAsync(int examId)
        {
            return await _unitOfWork.QuestionRepository.GetAsync(
                [ q => q.Choices],
                expression: q => q.ExamId == examId
            ) ?? new List<Question>();
        }
        public async Task CreateAsync(Question exam)
        {
            await _unitOfWork.QuestionRepository.CreateAsync(exam);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateAsync(Question question)
        {
            _unitOfWork.QuestionRepository.Edit(question);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var question = await GetByIdAsync(id);
            if (question != null)
            {
                _unitOfWork.QuestionRepository.Delete(question);
                await _unitOfWork.CompleteAsync();
            }
        }
       
    }
}
