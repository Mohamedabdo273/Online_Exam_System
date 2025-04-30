using infrastructure.Services.Iservices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Models;
using System.Net;

namespace OnlineExamSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminController : Controller
    {
        private readonly IExamService _examService;
        private readonly IQuestionService _questionService;
        private readonly IChoiceService _choiceService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IExamService examService,
            IQuestionService questionService,
            IChoiceService choiceService,
            UserManager<ApplicationUser> userManager,
            ILogger<AdminController> logger)
        {
            _examService = examService;
            _questionService = questionService;
            _choiceService = choiceService;
            _userManager = userManager;
            _logger = logger;
        }

        // ========== Exams ==========
        public async Task<IActionResult> GetAllExams()
        {
            try
            {
                var exams = await _examService.GetAllAsync();
                return View(exams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all exams");
                ModelState.AddModelError("", "An error occurred while retrieving exams.");
                return View(new List<Exam>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateExam(Exam exam)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _examService.CreateAsync(exam);
                return RedirectToAction("GetAllExams");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating exam");
                ModelState.AddModelError("", "An error occurred while creating the exam.");
                return BadRequest(ModelState);
            }
        }

        public async Task<IActionResult> EditExam(int id)
        {
            try
            {
                var exam = await _examService.GetByIdAsync(id);
                if (exam == null)
                {
                    ModelState.AddModelError("", "Exam not found.");
                    return NotFound();
                }
                return View(exam);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exam for edit");
                ModelState.AddModelError("", "An error occurred while retrieving the exam.");
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditExam(Exam exam)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _examService.UpdateAsync(exam);
                return RedirectToAction("GetAllExams");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating exam");
                ModelState.AddModelError("", "An error occurred while updating the exam.");
                return BadRequest(ModelState);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteExam(int id)
        {
            try
            {
                await _examService.DeleteAsync(id);
                return RedirectToAction("GetAllExams");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting exam");
                ModelState.AddModelError("", "An error occurred while deleting the exam.");
                return BadRequest(ModelState);
            }
        }

        public async Task<IActionResult> ExamDetails(int id)
        {
            try
            {
                var exam = await _examService.GetByIdAsync(id);
                if (exam == null)
                {
                    ModelState.AddModelError("", "Exam not found.");
                    return NotFound();
                }
                return View(exam);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exam details");
                ModelState.AddModelError("", "An error occurred while retrieving exam details.");
                return NotFound();
            }
        }

        // ========== Questions ==========
        public async Task<IActionResult> GetQuestions(int examId)
        {
            try
            {
                var questions = await _questionService.GetByIdAsync(examId);
                ViewBag.ExamId = examId;
                return View(questions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting questions for exam {ExamId}", examId);
                ModelState.AddModelError("", "An error occurred while retrieving questions.");
                ViewBag.ExamId = examId;
                return View(new List<Question>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQuestion(Question question)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _questionService.CreateAsync(question);
                return RedirectToAction("GetQuestions", new { examId = question.ExamId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating question");
                ModelState.AddModelError("", "An error occurred while creating the question.");
                return BadRequest(ModelState);
            }
        }

        public async Task<IActionResult> EditQuestion(int id)
        {
            try
            {
                var question = await _questionService.GetByIdAsync(id);
                if (question == null)
                {
                    ModelState.AddModelError("", "Question not found.");
                    return NotFound();
                }
                return View(question);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting question for edit");
                ModelState.AddModelError("", "An error occurred while retrieving the question.");
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQuestion(Question question)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _questionService.UpdateAsync(question);
                return RedirectToAction("GetQuestions", new { examId = question.ExamId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating question");
                ModelState.AddModelError("", "An error occurred while updating the question.");
                return BadRequest(ModelState);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            try
            {
                var question = await _questionService.GetByIdAsync(id);
                if (question == null)
                {
                    ModelState.AddModelError("", "Question not found.");
                    return NotFound();
                }

                await _questionService.DeleteAsync(id);
                return RedirectToAction("GetQuestions", new { examId = question.ExamId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting question");
                ModelState.AddModelError("", "An error occurred while deleting the question.");
                return BadRequest(ModelState);
            }
        }

        // ========== Choices ==========
        public async Task<IActionResult> GetChoices(int questionId)
        {
            try
            {
                var choices = await _choiceService.GetByIdAsync(questionId);
                ViewBag.QuestionId = questionId;
                return View(choices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting choices for question {QuestionId}", questionId);
                ModelState.AddModelError("", "An error occurred while retrieving choices.");
                ViewBag.QuestionId = questionId;
                return View(new List<Choice>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateChoice(Choice choice)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _choiceService.CreateAsync(choice);
                return RedirectToAction("GetChoices", new { questionId = choice.QuestionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating choice");
                ModelState.AddModelError("", "An error occurred while creating the choice.");
                return BadRequest(ModelState);
            }
        }

        public async Task<IActionResult> EditChoice(int id)
        {
            try
            {
                var choice = await _choiceService.GetByIdAsync(id);
                if (choice == null)
                {
                    ModelState.AddModelError("", "Choice not found.");
                    return NotFound();
                }
                return View(choice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting choice for edit");
                ModelState.AddModelError("", "An error occurred while retrieving the choice.");
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditChoice(Choice choice)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _choiceService.UpdateAsync(choice);
                return RedirectToAction("GetChoices", new { questionId = choice.QuestionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating choice");
                ModelState.AddModelError("", "An error occurred while updating the choice.");
                return BadRequest(ModelState);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteChoice(int id)
        {
            try
            {
                var choice = await _choiceService.GetByIdAsync(id);
                if (choice == null)
                {
                    ModelState.AddModelError("", "Choice not found.");
                    return NotFound();
                }

                await _choiceService.DeleteAsync(id);
                return RedirectToAction("GetChoices", new { questionId = choice.QuestionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting choice");
                ModelState.AddModelError("", "An error occurred while deleting the choice.");
                return BadRequest(ModelState);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetCorrectChoice(int choiceId)
        {
            try
            {
                var choice = await _choiceService.GetByIdAsync(choiceId);
                if (choice == null)
                {
                    return NotFound();
                }

                // Get all choices for the question
                var choices = await _choiceService.GetChoicesByQuestionIdAsync(choice.QuestionId);
                if (choices == null || !choices.Any())
                {
                    return NotFound("No choices found for this question.");
                }

                // Reset all choices to IsCorrect = false
                foreach (var c in choices)
                {
                    c.IsCorrect = false;
                }

                // Set the selected choice to IsCorrect = true
                var selectedChoice = choices.FirstOrDefault(c => c.Id == choiceId);
                if (selectedChoice != null)
                {
                    selectedChoice.IsCorrect = true;
                }

                // Update all choices in bulk (if supported) or one-by-one
                foreach (var c in choices)
                {
                    await _choiceService.UpdateAsync(c);
                }

                return Ok("Correct choice set successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting correct choice");
                return StatusCode(500, "An internal server error occurred.");
            }
        }


        // ========== Users ==========
        public async Task<IActionResult> GetAllUsers(string? search, int pageNumber = 1)
        {
            try
            {
                int pageSize = 15;
                var query = _userManager.Users.AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(u => u.Email.Contains(search) || u.UserName.Contains(search));
                }

                var totalUsers = await query.CountAsync();

                var users = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var result = new List<object>();
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    result.Add(new
                    {
                        user.Id,
                        user.UserName,
                        user.Email,
                        Roles = roles
                    });
                }

                ViewBag.SearchTerm = search;
                ViewBag.PageNumber = pageNumber;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalCount = totalUsers;

                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                ModelState.AddModelError("", "An error occurred while retrieving users.");
                return View(new List<object>());
            }
        }

        public async Task<IActionResult> UserDetails(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    ModelState.AddModelError("", "User not found.");
                    return NotFound();
                }

                var roles = await _userManager.GetRolesAsync(user);
                var result = new
                {
                    user.Id,
                    user.Email,
                    Roles = roles
                };
                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user details for {UserId}", id);
                ModelState.AddModelError("", "An error occurred while retrieving user details.");
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    ModelState.AddModelError("", "User not found.");
                    return NotFound();
                }

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return BadRequest(ModelState);
                }

                return RedirectToAction("GetAllUsers");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                ModelState.AddModelError("", "An error occurred while deleting the user.");
                return BadRequest(ModelState);
            }
        }
    }
}