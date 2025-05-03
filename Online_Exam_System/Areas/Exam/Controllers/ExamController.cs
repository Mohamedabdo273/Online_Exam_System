using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using infrastructure.Services.Iservices;
using Models.Models;
using NuGet.ContentModel;
using static System.Formats.Asn1.AsnWriter;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace YourNamespace.Areas.Exam.Controllers
{
    [Area("Exam")]
    [Authorize(Roles = "User")]
    public class ExamController : Controller
    {
        private readonly IExamService _examService;
        private readonly IQuestionService _questionService;
        private readonly IChoiceService _choiceService;
        private readonly IUserExamService _userExamService;
        private readonly IUserAnswerService _userAnswerService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExamController(
            IExamService examService,
            IQuestionService questionService,
            IChoiceService choiceService,
            IUserExamService userExamService,
            IUserAnswerService userAnswerService,
            UserManager<ApplicationUser> userManager)
        {
            _examService = examService;
            _questionService = questionService;
            _choiceService = choiceService;
            _userExamService = userExamService;
            _userAnswerService = userAnswerService;
            this._userManager = userManager;
        }

        // GET: Exam/Exam
        public async Task<IActionResult> Index()
        {
            var exams = await _examService.GetAllAsync();
            return View(exams);
        }

        // GET: Exam/Exam/TakeExam/5
        public async Task<IActionResult> TakeExam(int id)
        {

            var exam = await _examService.GetByIdAsync(id);
            if (exam == null)
            {
                return NotFound();
            }
            var UserId = _userManager.GetUserId(User);
            var questions = await _questionService.GetByExamIdAsync(id);
            if (!questions?.Any() ?? true)
            {
                TempData["ErrorMessage"] = "No questions available for this exam.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ExamId = exam.Id;
            ViewBag.ExamTitle = exam.Title;
            ViewBag.Duration = exam.DurationInMinutes;
            ViewBag.UserId = UserId;

            return View(questions.OrderBy(q => q.Id).ToList());
        }
        [HttpGet]
        public async Task<IActionResult> GetExamsData()
        {
            try
            {
                var exams = await _examService.GetAllAsync();
                var examData = exams.Select(e => new
                {
                    id = e.Id,
                    title = e.Title,
                    durationInMinutes = e.DurationInMinutes,
                    questionsCount = e.Questions?.Count ?? 0
                }).ToList();

                return Json(examData);
            }
            catch (Exception ex)
            {

                return StatusCode(500, "Error loading exams data");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitExam(
            [FromForm] int examId,
            [FromForm] string userId,
            [FromForm] string questionAnswers)
        {
            try
            {
                // Input validation
                if (examId <= 0) return BadRequest("Invalid exam ID");

                var userExists = await _userManager.FindByIdAsync(userId);
                if (userExists == null) return BadRequest("User not found");

                if (string.IsNullOrWhiteSpace(questionAnswers)) return BadRequest("No answers submitted");

                Dictionary<int, int> answers;
                try
                {
                    answers = JsonSerializer.Deserialize<Dictionary<int, int>>(questionAnswers)
                        ?? throw new JsonException("Deserialization returned null");
                }
                catch (JsonException ex)
                {
                    return BadRequest($"Invalid answer format: {ex.Message}");
                }

                var exam = await _examService.GetByIdAsync(examId);
                if (exam == null) return NotFound("Exam not found");

                // Get all questions with their choices
                var questionsEnumerable = await _questionService.GetByExamIdAsync(examId);
                if (questionsEnumerable == null || !questionsEnumerable.Any())
                {
                    return BadRequest("No questions found for this exam");
                }

                // Convert IEnumerable to List
                var questions = questionsEnumerable.ToList();

                var (correctAnswers, userAnswers) = ProcessAnswers(answers, questions);
                var totalQuestions = questions.Count;
                var percentageScore = totalQuestions > 0
                    ? Math.Round((correctAnswers / (double)totalQuestions) * 100, 2)
                    : 0;

                // Create UserExam
                var userExam = new UserExam
                {
                    ExamId = examId,
                    UserId = userId,
                    TakenAt = DateTime.UtcNow,
                    Score = percentageScore,
                    Passed = percentageScore >= 60,
                    UserAnswers = new List<UserAnswer>() // Initialize the list
                };

                try
                {
                    // Save the UserExam first to get its ID
                    await _userExamService.CreateAsync(userExam);

                    // Now that we have the UserExam ID, set it for each answer
                    foreach (var answer in userAnswers)
                    {
                        answer.UserExamId = userExam.Id;
                        // Add the answer to the UserExam's UserAnswers collection
                        userExam.UserAnswers.Add(answer);
                        // Save each answer
                        await _userAnswerService.CreateAsync(answer);
                    }

                    return Json(new
                    {
                        redirectUrl = Url.Action("ExamResult", new { userExamId = userExam.Id }),
                        status = "success"
                    });
                }
                catch (Exception ex)
                {
                    // If there's an error during saving, try to clean up
                    if (userExam.Id > 0)
                    {
                        try
                        {
                            await _userExamService.DeleteAsync(userExam.Id);
                        }
                        catch { /* Ignore cleanup errors */ }
                    }
                    throw; // Re-throw the original exception
                }
            }
            catch (Exception ex)
            {
                // Log the error
                return StatusCode(500, new
                {
                    message = "An error occurred while submitting the exam",
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }
        private (int correctAnswers, List<UserAnswer> userAnswers) ProcessAnswers(
            Dictionary<int, int> submittedAnswers,
            ICollection<Question> questions)
        {
            int correctCount = 0;
            var answers = new List<UserAnswer>();

            if (questions == null || !questions.Any())
            {
                return (correctCount, answers);
            }

            foreach (var question in questions)
            {
                if (!submittedAnswers.TryGetValue(question.Id, out var selectedChoiceId))
                {
                    continue;
                }

                // Find the selected choice in the question's choices
                var selectedChoice = question.Choices?
                    .FirstOrDefault(c => c.Id == selectedChoiceId);

                if (selectedChoice == null)
                {
                    continue;
                }

                // Get the correct choice for this question
                var correctChoice = question.Choices?
                    .FirstOrDefault(c => c.IsCorrect);

                // Check if the selected choice is the correct one
                var isCorrect = correctChoice != null && selectedChoice.Id == correctChoice.Id;
                if (isCorrect) correctCount++;

                // Create the user answer with proper relationships
                var userAnswer = new UserAnswer
                {
                    QuestionId = question.Id,
                    SelectedChoiceId = selectedChoice.Id,
                    IsCorrect = isCorrect
                };

                answers.Add(userAnswer);
            }

            return (correctCount, answers);
        }
        [HttpGet]
        public async Task<IActionResult> ExamResult(int userExamId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var userExam = (await _userExamService.GetAllAsync())
                .FirstOrDefault(ue => ue.Id == userExamId && ue.UserId == userId);

            if (userExam == null)
            {
                return NotFound("Exam results not found");
            }

            // Get the exam details
            var exam = await _examService.GetByIdAsync(userExam.ExamId) ?? new Models.Models.Exam { Title = "Unknown Exam" };

            // Get all user answers for this exam
            var userAnswers = (await _userAnswerService.GetAllAsync())
                .Where(ua => ua.UserExamId == userExamId)
                .ToList();

            // Calculate correct answers
            var correctAnswers = userAnswers.Count(a => a.IsCorrect);
            var totalQuestions = exam.Questions?.Count ?? 0;

            ViewBag.ExamTitle = exam.Title;
            ViewBag.TotalQuestions = totalQuestions;
            ViewBag.CorrectAnswers = correctAnswers;
            ViewBag.Score = userExam.Score;
            ViewBag.Passed = userExam.Passed;
            ViewBag.ExamId = userExam.ExamId;

            return View();
        }
        // GET: Exam/Exam/UserResults
        public async Task<IActionResult> UserResults()
        {
            var userId = User.Identity.Name;
            var userExams = (await _userExamService.GetAllAsync())
                .Where(ue => ue.UserId == userId)
                .OrderByDescending(ue => ue.TakenAt)
                .ToList();

            return View(userExams);
        }

        // GET: Exam/Exam/ExamDetails/5
        public async Task<IActionResult> ExamDetails(int id)
        {
            var userId = User.Identity.Name;
            var userExam = (await _userExamService.GetAllAsync())
                .FirstOrDefault(ue => ue.Id == id && ue.UserId == userId);

            if (userExam == null)
            {
                return NotFound();
            }

            var userAnswers = (await _userAnswerService.GetAllAsync())
                .Where(ua => ua.UserExamId == userExam.Id)
                .ToList();

            var resultDetails = userAnswers.Select(ua => new
            {
                Question = ua.Question?.Title,
                SelectedChoice = ua.SelectedChoice?.Text,
                IsCorrect = ua.IsCorrect,
                CorrectChoice = ua.Question?.Choices?.FirstOrDefault(c => c.IsCorrect)?.Text
            });

            ViewBag.ExamTitle = userExam.Exam?.Title;
            ViewBag.Score = userExam.Score;
            ViewBag.Passed = userExam.Passed;
            ViewBag.TakenAt = userExam.TakenAt;
            ViewBag.ResultDetails = resultDetails;

            return View();
        }
    }
}