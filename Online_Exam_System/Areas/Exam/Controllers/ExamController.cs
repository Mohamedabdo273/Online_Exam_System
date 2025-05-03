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

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var existingUserExam = (await _userExamService.GetAllAsync())
                .FirstOrDefault(ue => ue.ExamId == id && ue.UserId == userId);

            if (existingUserExam != null)
            {
                TempData["ErrorMessage"] = "You have already taken this exam.";
                return RedirectToAction("ExamDetails", new { id = existingUserExam.Id });
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
            ViewBag.UserId = userId;

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

               
                var questionsEnumerable = await _questionService.GetByExamIdAsync(examId);
                if (questionsEnumerable == null || !questionsEnumerable.Any())
                {
                    return BadRequest("No questions found for this exam");
                }

                var questions = questionsEnumerable.ToList();

                var (correctAnswers, userAnswers) = ProcessAnswers(answers, questions);
                var totalQuestions = questions.Count;
                var percentageScore = totalQuestions > 0
                    ? Math.Round((correctAnswers / (double)totalQuestions) * 100, 2)
                    : 0;

                
                var userExam = new UserExam
                {
                    ExamId = examId,
                    UserId = userId,
                    TakenAt = DateTime.UtcNow,
                    Score = percentageScore,
                    Passed = percentageScore >= 60,
                    UserAnswers = new List<UserAnswer>() 
                };

                try
                {
                    
                    await _userExamService.CreateAsync(userExam);

                   
                    foreach (var answer in userAnswers)
                    {
                        answer.UserExamId = userExam.Id;
                        
                        userExam.UserAnswers.Add(answer);
                        
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
                    
                    if (userExam.Id > 0)
                    {
                        try
                        {
                            await _userExamService.DeleteAsync(userExam.Id);
                        }
                        catch { }
                    }
                    throw; 
                }
            }
            catch (Exception ex)
            {
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

                var selectedChoice = question.Choices?
                    .FirstOrDefault(c => c.Id == selectedChoiceId);

                if (selectedChoice == null)
                {
                    continue;
                }

             
                var correctChoice = question.Choices?
                    .FirstOrDefault(c => c.IsCorrect);

               
                var isCorrect = correctChoice != null && selectedChoice.Id == correctChoice.Id;
                if (isCorrect) correctCount++;

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

         
            var exam = await _examService.GetByIdAsync(userExam.ExamId) ?? new Models.Models.Exam { Title = "Unknown Exam" };

           
            var userAnswers = (await _userAnswerService.GetAllAsync())
                .Where(ua => ua.UserExamId == userExamId)
                .ToList();

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
            var userId = _userManager.GetUserId(User);
            var userExams = (await _userExamService.GetAllAsync())
                .Where(ue => ue.UserId == userId)
                .OrderByDescending(ue => ue.TakenAt)
                .ToList();

            return View(userExams);
        }

        // GET: Exam/Exam/ExamDetails/5
        public async Task<IActionResult> ExamDetails(int id)
        {
            var userId = _userManager.GetUserId(User);
            var userExam = (await _userExamService.GetAllAsync())
                .FirstOrDefault(ue => ue.Id == id && ue.UserId == userId);

            if (userExam == null)
            {
                return NotFound();
            }

           
            var exam = await _examService.GetByIdAsync(userExam.ExamId);
            if (exam == null)
            {
                return NotFound("Exam not found");
            }

            var userAnswers = (await _userAnswerService.GetAllAsync())
                .Where(ua => ua.UserExamId == userExam.Id)
                .ToList();

            var questions = await _questionService.GetByExamIdAsync(userExam.ExamId);

            var resultDetails = userAnswers.Select(ua =>
            {
                var question = questions.FirstOrDefault(q => q.Id == ua.QuestionId);
                var selectedChoice = question?.Choices?.FirstOrDefault(c => c.Id == ua.SelectedChoiceId);
                var correctChoice = question?.Choices?.FirstOrDefault(c => c.IsCorrect);

                return new
                {
                    QuestionId = ua.QuestionId,
                    QuestionText = question?.Title,
                    SelectedChoiceId = ua.SelectedChoiceId,
                    SelectedChoiceText = selectedChoice?.Text,
                    IsCorrect = ua.IsCorrect,
                    CorrectChoiceId = correctChoice?.Id,
                    CorrectChoiceText = correctChoice?.Text
                };
            }).ToList();

            ViewBag.ExamTitle = exam.Title;
            ViewBag.Score = userExam.Score;
            ViewBag.Passed = userExam.Passed;
            ViewBag.TakenAt = userExam.TakenAt;
            ViewBag.ResultDetails = resultDetails;

            return View();
        }
    }
}