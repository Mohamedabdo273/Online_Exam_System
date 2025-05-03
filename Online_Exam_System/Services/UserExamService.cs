using infrastructure.Services.Iservices;
using infrastructures.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace infrastructure.Services
{
    public class UserExamService : IUserExamService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserExamService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<UserExam>> GetAllAsync()
        {
            return await _unitOfWork.UserExamRepository.GetAsync([e=>e.UserAnswers]);
        }

        public async Task<UserExam?> GetByIdAsync(int id)
        {
            return await _unitOfWork.UserExamRepository.GetOneAsync([e => e.UserAnswers],expression: e => e.Id == id);
        }

        public async Task CreateAsync(UserExam userExam)
        {

            try
            {
                await _unitOfWork.UserExamRepository.CreateAsync(userExam);
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("DB ERROR: " + ex.InnerException?.Message ?? ex.Message);
                throw;
            }

        }
        public async Task UpdateAsync(UserExam userExam)
        {
            _unitOfWork.UserExamRepository.Edit(userExam);
            await _unitOfWork.CompleteAsync();
        }
        public async Task<bool> HasUserTakenExam(string userId, int examId)
        {
            var exam = await _unitOfWork.UserExamRepository.GetAsync(
                expression: e => e.UserId == userId && e.ExamId == examId);
            return exam != null;
        }
        public async Task DeleteAsync(int id)
        {
            var userExam = await GetByIdAsync(id);
            if (userExam != null)
            {
                _unitOfWork.UserExamRepository.Delete(userExam);
                await _unitOfWork.CompleteAsync();
            }
        }
    }
}
