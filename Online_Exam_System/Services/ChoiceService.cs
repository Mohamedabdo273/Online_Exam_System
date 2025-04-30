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
    public class ChoiceService : IChoiceService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ChoiceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IEnumerable<Choice>> GetAllAsync()
        {
            return await _unitOfWork.ChoiceRepository.GetAsync();
        }

        public async Task<Choice?> GetByIdAsync(int id)
        {
            return await _unitOfWork.ChoiceRepository.GetOneAsync(expression: e => e.Id == id);
        }

        public async Task CreateAsync(Choice choice)
        {
            await _unitOfWork.ChoiceRepository.CreateAsync(choice);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateAsync(Choice choice)
        {
            _unitOfWork.ChoiceRepository.Edit(choice);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var choice = await GetByIdAsync(id);
            if (choice != null)
            {
                _unitOfWork.ChoiceRepository.Delete(choice);
                await _unitOfWork.CompleteAsync();
            }
        }
        public async Task<IEnumerable<Choice>> GetChoicesByQuestionIdAsync(int questionId)
        {
            return await _unitOfWork.ChoiceRepository.GetChoicesByQuestionIdAsync(questionId);
        }
    }
}
