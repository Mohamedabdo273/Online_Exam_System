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
            // Get the existing question with its choices
            var existingQuestion = await _unitOfWork.QuestionRepository
                .GetOneAsync([ e => e.Choices],
                           expression: e => e.Id == question.Id);

            if (existingQuestion == null)
            {
                throw new Exception("Question not found");
            }
            existingQuestion.Title = question.Title;
            var choicesToRemove = existingQuestion.Choices.ToList();
            for (int i = 0; i < choicesToRemove.Count; i++)
            {
                _unitOfWork.ChoiceRepository.Delete(choicesToRemove[i]);
            }
            foreach (var choice in question.Choices)
            {
                existingQuestion.Choices.Add(new Choice
                {
                    Text = choice.Text,
                    IsCorrect = choice.IsCorrect,
                    QuestionId = question.Id
                });
            }

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
