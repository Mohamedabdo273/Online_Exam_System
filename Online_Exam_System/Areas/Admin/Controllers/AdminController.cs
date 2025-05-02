using infrastructure.Services.Iservices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Models;
using System.Net;

namespace OnlineExamSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="Admin")]
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
        
        public IActionResult CreateExam()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateExam(Exam exam)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var allErrors = ModelState.Values.SelectMany(v => v.Errors);
                    foreach (var error in allErrors)
                    {
                        Console.WriteLine(error.ErrorMessage); // مؤقتًا للعرض فقط
                    }

                    return View(exam);
                }


                await _examService.CreateAsync(exam);
                return RedirectToAction("GetAllExams");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating exam");
                ModelState.AddModelError("", "An error occurred while creating the exam.");
                return View(exam);
            }
        }
        [HttpGet]
        public async Task<IActionResult> EditExam(int id)
        {
            try
            {
                var exam = await _examService.GetByIdAsync(id);
                if (exam == null)
                {
                    TempData["ErrorMessage"] = "Exam not found.";
                    return RedirectToAction("GetAllExams");
                }
                return View(exam);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exam for edit");
                TempData["ErrorMessage"] = "An error occurred while retrieving the exam.";
                return RedirectToAction("GetAllExams");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditExam(int id, Exam exam)
        {
            try
            {
                if (id != exam.Id)
                {
                    TempData["ErrorMessage"] = "Exam ID mismatch.";
                    return RedirectToAction("GetAllExams");
                }

                if (!ModelState.IsValid)
                {
                    return View(exam);
                }

                var existingExam = await _examService.GetByIdAsync(id);
                if (existingExam == null)
                {
                    TempData["ErrorMessage"] = "Exam not found.";
                    return RedirectToAction("GetAllExams");
                }

                // Update only the properties you want to change
                existingExam.Title = exam.Title;
                existingExam.Description = exam.Description;
                existingExam.DurationInMinutes = exam.DurationInMinutes;

                await _examService.UpdateAsync(existingExam); // Update the existing entity

                TempData["SuccessMessage"] = "Exam updated successfully!";
                return RedirectToAction("GetAllExams");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating exam");
                ModelState.AddModelError("", "The record you attempted to edit was modified by another user. Please try again.");
                return View(exam);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating exam");
                ModelState.AddModelError("", "An error occurred while updating the exam. Please try again.");
                return View(exam);
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
                var questions = await _questionService.GetByExamIdAsync(examId);
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
        [HttpGet]
        public IActionResult CreateQuestion(int examId)
        {
            var question = new Question
            {
                ExamId = examId,
                Choices = new List<Choice>()
            };
            return View(question);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQuestion(Question question, int correctChoiceIndex)
        {
            try
            {
                if (string.IsNullOrEmpty(question.Title))
                {
                    ModelState.AddModelError("", "The question title cannot be empty.");
                    return View(question);
                }
                question.Choices.Clear();
                for (int i = 0; i < 4; i++)
                {
                    var choiceText = HttpContext.Request.Form[$"Choices[{i}].Text"];
                    if (!string.IsNullOrEmpty(choiceText))
                    {
                        var choice = new Choice
                        {
                            Text = choiceText,
                            IsCorrect = (i == correctChoiceIndex)
                        };
                        question.Choices.Add(choice);
                    }
                }
                if (!question.Choices.Any())
                {
                    ModelState.AddModelError("", "You must provide at least one choice.");
                    return View(question);
                }

                if (correctChoiceIndex < 0 || correctChoiceIndex >= question.Choices.Count)
                {
                    ModelState.AddModelError("", "Invalid correct choice index.");
                    return View(question);
                }
                await _questionService.CreateAsync(question);

                return RedirectToAction("GetQuestions", new { examId = question.ExamId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating question");
                ModelState.AddModelError("", "An error occurred while creating the question.");
                return View(question);
            }
        }
        [HttpGet]
        public async Task<IActionResult> EditQuestion(int id)
        {
            try
            {
                var question = await _questionService.GetByIdAsync(id);
                if (question == null)
                {
                    TempData["ErrorMessage"] = "Question not found.";
                    return RedirectToAction("GetQuestions", new { examId = question?.ExamId });
                }

                // Ensure we have at least 4 choices for the form
                while (question.Choices.Count < 4)
                {
                    question.Choices.Add(new Choice());
                }

                return View(question);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting question for edit");
                TempData["ErrorMessage"] = "An error occurred while retrieving the question.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQuestion(Question question, int correctChoiceIndex)
        {
            try
            {
                if (string.IsNullOrEmpty(question.Title))
                {
                    ModelState.AddModelError("", "The question title cannot be empty.");
                    return View(question);
                }
                question.Choices = new List<Choice>();
                for (int i = 0; i < 4; i++)
                {
                    var choiceText = HttpContext.Request.Form[$"Choices[{i}].Text"];
                    if (!string.IsNullOrEmpty(choiceText))
                    {
                        question.Choices.Add(new Choice
                        {
                            Text = choiceText,
                            IsCorrect = (i == correctChoiceIndex)
                        });
                    }
                }

                if (!question.Choices.Any())
                {
                    ModelState.AddModelError("", "You must provide at least one choice.");
                    return View(question);
                }

                if (correctChoiceIndex < 0 || correctChoiceIndex >= question.Choices.Count)
                {
                    ModelState.AddModelError("", "Invalid correct choice index.");
                    return View(question);
                }

                await _questionService.UpdateAsync(question);
                TempData["SuccessMessage"] = "Question updated successfully!";
                return RedirectToAction("GetQuestions", new { examId = question.ExamId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating question");
                ModelState.AddModelError("", "An error occurred while updating the question.");
                return View(question);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            try
            {
                await _questionService.DeleteAsync(id);
                return RedirectToAction("GetQuestions");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Question");
                ModelState.AddModelError("", "An error occurred while deleting the Question.");
                return BadRequest(ModelState);
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
                    query = query.Where(u => u.Email.Contains(search) || u.FullName.Contains(search));
                }

                var totalUsers = await query.CountAsync();

                // First get the user data without roles
                var users = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Then get roles for each user
                var userViewModels = new List<dynamic>();
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userViewModels.Add(new
                    {
                        user.Id,
                        user.FullName,
                        user.UserName,
                        user.Email,
                        Roles = roles
                    });
                }

                ViewBag.SearchTerm = search;
                ViewBag.PageNumber = pageNumber;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalCount = totalUsers;

                return View(userViewModels);
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
                    user.UserName,
                    user.FullName,
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