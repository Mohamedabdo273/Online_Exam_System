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
   public class UserAnswerService : IUserAnswerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserAnswerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<UserAnswer>> GetAllAsync()
        {
            return await _unitOfWork.UserAnswerRepository.GetAsync();
        }

        public async Task<UserAnswer?> GetByIdAsync(int id)
        {
            return await _unitOfWork.UserAnswerRepository.GetOneAsync(expression: e => e.Id == id);
        }

        public async Task CreateAsync(UserAnswer userAnswer)
        {
            await _unitOfWork.UserAnswerRepository.CreateAsync(userAnswer);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateAsync(UserAnswer userAnswer)
        {
            _unitOfWork.UserAnswerRepository.Edit(userAnswer);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var userExam = await GetByIdAsync(id);
            if (userExam != null)
            {
                _unitOfWork.UserAnswerRepository.Delete(userExam);
                await _unitOfWork.CompleteAsync();
            }
        }
    }
}
