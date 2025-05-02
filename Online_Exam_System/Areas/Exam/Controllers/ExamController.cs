using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using infrastructure.Services.Iservices;
using Models.Models;

namespace YourNamespace.Areas.Exam.Controllers
{
    [Area("Exam")]
    [Authorize]
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
        // POST: Exam/Exam/SubmitExam
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitExam(int examId, string userId,
    [FromForm] Dictionary<int, int> questionAnswers)
        {
            try
            {
                if (questionAnswers == null || !questionAnswers.Any())
                {
                    TempData["ErrorMessage"] = "No answers submitted.";
                    return RedirectToAction(nameof(TakeExam), new { id = examId });
                }

                var exam = await _examService.GetByIdAsync(examId);
                if (exam == null)
                {
                    return NotFound("Exam not found.");
                }

                if (User.Identity?.Name != userId)
                {
                    return Unauthorized("User mismatch.");
                }

                // Check if user already took this exam
                var existingExam = (await _userExamService.GetAllAsync())
                    .FirstOrDefault(ue => ue.UserId == userId && ue.ExamId == examId);
                if (existingExam != null)
                {
                    TempData["ErrorMessage"] = "You have already taken this exam.";
                    return RedirectToAction(nameof(Index));
                }

                var userExam = new UserExam
                {
                    ExamId = examId,
                    UserId = userId,
                    TakenAt = DateTime.UtcNow,
                    UserAnswers = new List<UserAnswer>()
                };

                int correctAnswers = 0;
                var totalQuestions = exam.Questions?.Count ?? 0;

                foreach (var answer in questionAnswers)
                {
                    var question = await _questionService.GetByIdAsync(answer.Key);
                    if (question == null) continue;

                    var correctChoice = question.Choices?.FirstOrDefault(c => c.IsCorrect);
                    bool isCorrect = correctChoice != null && answer.Value == correctChoice.Id;

                    if (isCorrect) correctAnswers++;

                    userExam.UserAnswers.Add(new UserAnswer
                    {
                        QuestionId = answer.Key,
                        SelectedChoiceId = answer.Value,
                        IsCorrect = isCorrect
                    });
                }

                double percentageScore = totalQuestions > 0 ? (correctAnswers / (double)totalQuestions) * 100 : 0;
                userExam.Score = percentageScore;
                userExam.Passed = percentageScore >= 60;

                await _userExamService.CreateAsync(userExam);

                // Prepare result data for the view
                var resultData = new
                {
                    ExamTitle = exam.Title,
                    TotalQuestions = totalQuestions,
                    CorrectAnswers = correctAnswers,
                    Score = percentageScore,
                    Passed = userExam.Passed,
                    ExamId = exam.Id
                };

                return Json(new { redirectUrl = Url.Action(nameof(ExamResult), resultData) });
            }
            catch (Exception ex)
            {
           
                return StatusCode(500, new { message = "An unexpected error occurred while processing your exam." });
            }
        }

        // GET: Exam/Exam/ExamResult
        public IActionResult ExamResult(string examTitle, int totalQuestions,
            int correctAnswers, double score, bool passed, int examId)
        {
            ViewBag.ExamTitle = examTitle;
            ViewBag.TotalQuestions = totalQuestions;
            ViewBag.CorrectAnswers = correctAnswers;
            ViewBag.Score = score;
            ViewBag.Passed = passed;
            ViewBag.ExamId = examId;

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