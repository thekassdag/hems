using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HMS.Data;
using HMS.Models;
using HMS.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HMS.Controllers.Dashboard
{
    [Area("Dashboard")]
    [Route("dashboard/exams/{examId}/exam-topics/{topicId}/[controller]")]
    [Authorize(Roles = "Instructor")]
    public class QuestionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EncryptionService _encryptionService;

        public QuestionsController(ApplicationDbContext context, EncryptionService encryptionService)
        {
            _context = context;
            _encryptionService = encryptionService;
        }

        // GET
        [HttpGet("")]
        public async Task<IActionResult> Index(long examId, long topicId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return StatusCode(401, new { message = "User not found." });

            var examTopic = await _context.ExamTopics
                .Include(et => et.Topic)
                .Include(et => et.Exam)
                .FirstOrDefaultAsync(et =>
                    et.Id == topicId &&
                    et.ExamId == examId &&
                    et.UserId == userId
                );

            if (examTopic == null)
                return StatusCode(404, new { message = "ExamTopic not found." });

            var currentUser = await _context.Users.FindAsync(userId);

            ViewBag.ExamId = examId;
            ViewBag.TopicId = topicId;
            ViewBag.ExamTopic = examTopic;
            ViewBag.ExamTitle = examTopic.Exam?.Title;
            ViewBag.TopicTitle = examTopic.Topic?.Title;
            ViewBag.CurrentUserName = currentUser?.FullName;

            var questions = await _context.Questions
                .Where(q => q.ExamId == examId && q.ExamTopicId == examTopic.Id)
                .Include(q => q.Choices)
                .OrderByDescending(q => q.CreatedAt)
                .Select(q => new QuestionResponseDto
                {
                    Id = q.Id,
                    ExamId = q.ExamId,
                    ExamTopicId = q.ExamTopicId,
                    Content = _encryptionService.Decrypt(q.Content),
                    Marks = q.Marks,
                    Choices = q.Choices!
                        .Select(c => new ChoiceDto
                        {
                            Id = c.Id,
                            ChoiceText = _encryptionService.Decrypt(c.ChoiceText),
                            IsCorrect = c.IsCorrect
                        }).ToList()
                }).ToListAsync();

            ViewBag.ExistingQuestions = questions;
            ViewBag.TotalAllocatedQuestions = examTopic.TotalQuestions;
            ViewBag.CurrentSubmittedQuestions = questions.Count;
            ViewBag.RemainingQuestions = examTopic.TotalQuestions - questions.Count;

            return View("/Views/Dashboard/Questions/Index.cshtml");
        }

        // DTOs
        public class ChoiceDto
        {
            public long Id { get; set; }
            [Required]
            public string? ChoiceText { get; set; }
            public bool IsCorrect { get; set; }
        }

        public class CreateQuestionDto
        {
            [Required]
            public long ExamTopicId { get; set; }
            [Required]
            public string? Content { get; set; }
            [Required]
            public int Marks { get; set; }
            [Required]
            public List<ChoiceDto>? Choices { get; set; }
        }

        public class UpdateQuestionDto
        {
            [Required]
            public long QuestionId { get; set; }
            [Required]
            public long ExamTopicId { get; set; }
            [Required]
            public string? Content { get; set; }
            [Required]
            public int Marks { get; set; }
            [Required]
            public List<ChoiceDto>? Choices { get; set; }
        }

        public class QuestionResponseDto
        {
            public long Id { get; set; }
            public long ExamId { get; set; }
            public long ExamTopicId { get; set; }
            public string? Content { get; set; }
            public int Marks { get; set; }
            public List<ChoiceDto>? Choices { get; set; }
        }

        // CREATE
        [HttpPost("create")]
        public async Task<IActionResult> CreateQuestion(long examId, [FromBody] CreateQuestionDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return StatusCode(401, new { message = "User not found." });

            var examTopic = await _context.ExamTopics
                .FirstOrDefaultAsync(et =>
                    et.Id == request.ExamTopicId &&
                    et.ExamId == examId &&
                    et.UserId == userId
                );

            if (examTopic == null)
                return StatusCode(404, new { message = "ExamTopic not found." });

            var questionCount = await _context.Questions
                .CountAsync(q => q.ExamTopicId == request.ExamTopicId);

            if (questionCount >= examTopic.TotalQuestions)
                return StatusCode(403, new { message = "Limit reached." });

            var question = new Question
            {
                ExamId = examId,
                ExamTopicId = request.ExamTopicId,
                Content = _encryptionService.Encrypt(request.Content),
                Marks = request.Marks,
                Choices = new List<Choice>()
            };

            foreach (var c in request.Choices!)
            {
                question.Choices.Add(new Choice
                {
                    ChoiceText = _encryptionService.Encrypt(c.ChoiceText),
                    IsCorrect = c.IsCorrect
                });
            }

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetQuestion),
                new { examId = examId, questionId = question.Id },
                new QuestionResponseDto
                {
                    Id = question.Id,
                    ExamId = question.ExamId,
                    ExamTopicId = question.ExamTopicId,
                    Content = request.Content,
                    Marks = question.Marks,
                    Choices = question.Choices
                        .Select(c => new ChoiceDto { Id = c.Id, ChoiceText = _encryptionService.Decrypt(c.ChoiceText), IsCorrect = c.IsCorrect })
                        .ToList()
                });
        }

        // GET SINGLE
        [HttpGet("{questionId}")]
        public async Task<IActionResult> GetQuestion(long examId, long questionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return StatusCode(401, new { message = "User not found." });

            var question = await _context.Questions
                .Include(q => q.ExamTopic)
                .Include(q => q.Choices)
                .FirstOrDefaultAsync(q => q.Id == questionId && q.ExamId == examId);

            if (question == null)
                return StatusCode(404, new { message = "Question not found." });

            if (question.ExamTopic?.UserId != userId)
                return StatusCode(403, new { message = "Unauthorized." });

            return Ok(new QuestionResponseDto
            {
                Id = question.Id,
                ExamId = question.ExamId,
                ExamTopicId = question.ExamTopicId,
                Content = _encryptionService.Decrypt(question.Content),
                Marks = question.Marks,
                Choices = question.Choices
                    .Select(c => new ChoiceDto
                    {
                        Id = c.Id,
                        ChoiceText = _encryptionService.Decrypt(c.ChoiceText),
                        IsCorrect = c.IsCorrect
                    }).ToList()
            });
        }

        // UPDATE
        [HttpPut("{questionId}/edit")]
        public async Task<IActionResult> UpdateQuestion(long examId, long questionId, [FromBody] UpdateQuestionDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (questionId != request.QuestionId)
                return BadRequest(new { message = "ID mismatch." });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return StatusCode(401, new { message = "User not found." });

            var question = await _context.Questions
                .Include(q => q.ExamTopic)
                .Include(q => q.Choices)
                .FirstOrDefaultAsync(q => q.Id == questionId && q.ExamId == examId);

            if (question == null)
                return StatusCode(404, new { message = "Not found." });

            if (question.ExamTopic?.UserId != userId)
                return StatusCode(403, new { message = "Unauthorized." });

            var examTopic = await _context.ExamTopics
                .FirstOrDefaultAsync(et =>
                    et.Id == request.ExamTopicId &&
                    et.ExamId == examId &&
                    et.UserId == userId
                );

            if (examTopic == null)
                return StatusCode(404, new { message = "Target ExamTopic not found." });

            // Update base fields
            if (_encryptionService.Decrypt(question.Content) != request.Content)
                question.Content = _encryptionService.Encrypt(request.Content);

            if (question.Marks != request.Marks)
                question.Marks = request.Marks;

            if (question.ExamTopicId != request.ExamTopicId)
                question.ExamTopicId = request.ExamTopicId;

            // Choices update
            var incomingIds = request.Choices!.Where(c => c.Id != 0).Select(c => c.Id).ToHashSet();

            var toRemove = question.Choices!.Where(c => !incomingIds.Contains(c.Id)).ToList();
            _context.Choices.RemoveRange(toRemove);

            foreach (var c in request.Choices)
            {
                if (c.Id == 0)
                {
                    question.Choices.Add(new Choice
                    {
                        QuestionId = question.Id,
                        ChoiceText = _encryptionService.Encrypt(c.ChoiceText),
                        IsCorrect = c.IsCorrect
                    });
                }
                else
                {
                    var existing = question.Choices.First(x => x.Id == c.Id);
                    var decrypted = _encryptionService.Decrypt(existing.ChoiceText);

                    if (decrypted != c.ChoiceText)
                        existing.ChoiceText = _encryptionService.Encrypt(c.ChoiceText);

                    existing.IsCorrect = c.IsCorrect;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new QuestionResponseDto
            {
                Id = question.Id,
                ExamId = question.ExamId,
                ExamTopicId = question.ExamTopicId,
                Content = _encryptionService.Decrypt(question.Content),
                Marks = question.Marks,
                Choices = question.Choices
                    .Select(c => new ChoiceDto { Id = c.Id, ChoiceText = _encryptionService.Decrypt(c.ChoiceText), IsCorrect = c.IsCorrect })
                    .ToList()
            });
        }

        // DELETE
        [HttpDelete("{questionId}/delete")]
        public async Task<IActionResult> DeleteQuestion(long examId, long questionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return StatusCode(401, new { message = "User not found." });

            var question = await _context.Questions
                .Include(q => q.ExamTopic)
                .FirstOrDefaultAsync(q => q.Id == questionId && q.ExamId == examId);

            if (question == null)
                return StatusCode(404, new { message = "Not found." });

            if (question.ExamTopic?.UserId != userId)
                return StatusCode(403, new { message = "Unauthorized." });

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Deleted successfully." });
        }

        private bool QuestionExists(long id)
        {
            return _context.Questions.Any(e => e.Id == id);
        }
    }
}