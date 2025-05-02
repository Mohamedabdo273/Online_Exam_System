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

namespace YourNamespace.Areas.Exam.Controllers
{
    [Area("Exam")]
    [Authorize(Roles ="User")]
    public class ExamController : Controller
    {
        private readonly IExamService _examService;    
        private readonly IQuestionService _questionService;
        private readonly IChoiceService _choiceService;
        private readonly IUserExamService _userExamService;
        private readonly IUserAnswerService _userAnswerService;

        public ExamController(
            IExamService examService,
            IQuestionService questionService,
            IChoiceService choiceService,
            IUserExamService userExamService,
            IUserAnswerService userAnswerService)
        {
            _examService = examService;
            _questionService = questionService;
            _choiceService = choiceService;
            _userExamService = userExamService;
            _userAnswerService = userAnswerService;
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
            if (await _userExamService.HasUserTakenExam(User.Identity.Name, id))
            {
                TempData["ErrorMessage"] = "You have already taken this exam.";
                return RedirectToAction(nameof(Index));
            }
            var exam = await _examService.GetByIdAsync(id);
            if (exam == null)
            {
                return NotFound();
            }

            var questions = await _questionService.GetByExamIdAsync(id);
            if (!questions?.Any() ?? true)
            {
                TempData["ErrorMessage"] = "No questions available for this exam.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ExamId = exam.Id;
            ViewBag.ExamTitle = exam.Title;
            ViewBag.Duration = exam.DurationInMinutes;
            ViewBag.UserId = User.Identity.Name;

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
                
                // Validate inputs
                if (examId <= 0 || string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(new { message = "Invalid exam or user information" });
                }

                // Deserialize answers
                var answers = JsonSerializer.Deserialize<Dictionary<int, int>>(questionAnswers)
                    ?? new Dictionary<int, int>();

                if (!answers.Any())
                {
                    return BadRequest(new { message = "No answers submitted" });
                }

                // Get exam with questions and choices
                var exam = await _examService.GetByIdAsync(examId);
                if (exam == null)
                {
                    return NotFound(new { message = "Exam not found" });
                }

                // Verify user
                if (User.Identity?.Name != userId)
                {
                    return Unauthorized(new { message = "User mismatch" });
                }

                // Check for existing attempt
                if (await _userExamService.HasUserTakenExam(userId, examId))
                {
                    return Conflict(new { message = "You have already taken this exam" });
                }

                // Process answers
                var (correctAnswers, userAnswers) = ProcessAnswers(answers, exam.Questions);

                // Calculate score
                var totalQuestions = exam.Questions.Count;
                var percentageScore = totalQuestions > 0
                    ? Math.Round((correctAnswers / (double)totalQuestions) * 100, 2)
                    : 0;

                // Create user exam record
                var userExam = new UserExam
                {
                    ExamId = examId,
                    UserId = userId,
                    TakenAt = DateTime.UtcNow,
                    Score = percentageScore,
                    Passed = percentageScore >= 60,
                    UserAnswers = userAnswers
                };

                await _userExamService.CreateAsync(userExam);

                return Json(new
                {
                    redirectUrl = Url.Action(
        nameof(ExamResult),
        new
        {
            examTitle = exam.Title,
            totalQuestions = totalQuestions,
            correctAnswers = correctAnswers,
            score = percentageScore,
            passed = userExam.Passed,
            examId = exam.Id
        }),
                    status = "success"
                });
            }
            catch (JsonException jsonEx)
            {
                return BadRequest(new { message = "Invalid answer format" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while processing your exam",
                    detailedError = ex.Message
                });
            }
        }

        private (int correctAnswers, List<UserAnswer> userAnswers) ProcessAnswers(
            Dictionary<int, int> submittedAnswers,
            ICollection<Question> questions)
        {
            int correctCount = 0;
            var answers = new List<UserAnswer>();

            foreach (var question in questions)
            {
                if (!submittedAnswers.TryGetValue(question.Id, out var selectedChoiceId))
                    continue;

                var isCorrect = question.Choices?
                    .Any(c => c.Id == selectedChoiceId && c.IsCorrect) ?? false;

                if (isCorrect) correctCount++;

                answers.Add(new UserAnswer
                {
                    QuestionId = question.Id,
                    SelectedChoiceId = selectedChoiceId,
                    IsCorrect = isCorrect
                });
            }

            return (correctCount, answers);
        }
        public async Task<IActionResult> ExamResult(int examId)
        {
            var userId = User.Identity.Name;

            // Get the most recent attempt
            var userExam = (await _userExamService.GetAllAsync())
                .Where(ue => ue.UserId == userId && ue.ExamId == examId);

            if (userExam == null)
            {
                ViewBag.ErrorMessage = "No exam results found. Please complete the exam first.";
                return View("Error");
            }

            return View(userExam);
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