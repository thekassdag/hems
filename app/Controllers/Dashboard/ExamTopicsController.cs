using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HMS.Data;
using HMS.Models;
using HMS.Models.Enums;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System;
using System.Security.Claims;
using MySqlConnector;
using HMS.Services;

namespace HMS.Controllers.Dashboard
{
    [Area("Dashboard")]
    [Route("dashboard/exams/{examId}/[controller]")] // Nested route for ExamTopics
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class ExamTopicsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public ExamTopicsController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: dashboard/exams/{examId}/examtopics/add
        [HttpGet("add")]
        public IActionResult Add(int examId)
        {
            ViewBag.ExamId = examId;
            ViewBag.Topics = _context.Topics.ToList();
            ViewBag.Instructors = _context.Users.Where(u => u.Role == UserRole.Instructor).ToList();
            return View("/Views/Dashboard/Exams/AddTopic.cshtml", new ExamTopic { ExamId = examId });
        }

        // POST: dashboard/exams/{examId}/examtopics/add
        [HttpPost("add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int examId, [Bind("ExamId,TopicId,TotalQuestions,UserId,Deadline")] ExamTopic examTopic)
        {
            if (examId != examTopic.ExamId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check for duplicate topic and instructor in the same exam
                    bool alreadyExists = await _context.ExamTopics.AnyAsync(et => et.ExamId == examTopic.ExamId && et.TopicId == examTopic.TopicId && et.UserId == examTopic.UserId);
                    if (alreadyExists)
                    {
                        ModelState.AddModelError("TopicId", "This topic has already been assigned to this instructor for this exam.");
                        TempData["ErrorMessage"] = "This topic has already been assigned to this instructor for this exam.";
                    }
                    else
                    {
                        examTopic.CreatedAt = DateTime.UtcNow;
                        examTopic.UpdatedAt = DateTime.UtcNow;
                        _context.Add(examTopic);
                        await _context.SaveChangesAsync();

                        // Fetch the newly created ExamTopic with related Exam, Topic, and User for email notification
                        var newExamTopic = await _context.ExamTopics
                                                        .Include(et => et.Exam)
                                                        .Include(et => et.Topic)
                                                        .Include(et => et.User)
                                                        .FirstOrDefaultAsync(et => et.Id == examTopic.Id);

                        if (newExamTopic != null)
                        {
                            var instructorEmail = newExamTopic.User?.Email;
                            if (!string.IsNullOrEmpty(instructorEmail))
                            {
                                var subject = $"New Exam Topic Assignment: {newExamTopic.Exam?.Title} - {newExamTopic.Topic?.Title}";
                                var body = $"Dear {newExamTopic.User?.FullName},<br><br>" +
                                           $"You have been assigned a new exam topic '{newExamTopic.Topic?.Title}' for exam '{newExamTopic.Exam?.Title}'.<br>" +
                                           $"Total Questions: {newExamTopic.TotalQuestions}<br>" +
                                           $"Deadline: {newExamTopic.Deadline.ToShortDateString()}<br>";

                                var questionsUrl = $"{Request.Scheme}://{Request.Host}/dashboard/exams/{newExamTopic.ExamId}/questions";

                                body += $"<br>Please review the assignment: <a href='{questionsUrl}'>View Exam Questions</a><br><br>" +
                                        "Regards,<br>HMS Team";

                                await _emailService.Send(instructorEmail, subject, body);
                            }
                        }
                        return RedirectToAction("Details", "Exams", new { id = examId }); // Redirect back to Exam Details
                    }
                }
                catch (DbUpdateException ex) when (ex.InnerException is MySqlException mysqlEx && mysqlEx.Number == 1062) // 1062 is for duplicate entry
                {
                    ModelState.AddModelError(string.Empty, "This topic has already been assigned to this instructor for this exam.");
                    TempData["ErrorMessage"] = "This topic has already been assigned to this instructor for this exam.";
                }
            }
            ViewBag.ExamId = examId;
            ViewBag.Topics = _context.Topics.ToList();
            ViewBag.Instructors = _context.Users.Where(u => u.Role == UserRole.Instructor).ToList();
            return View("/Views/Dashboard/Exams/AddTopic.cshtml", examTopic);
        }

        // GET: dashboard/exams/{examId}/examtopics/{id}/edit
        [HttpGet("{id}/edit")]
        public async Task<IActionResult> Edit(int examId, int id)
        {
            var examTopic = await _context.ExamTopics.FindAsync(id);
            if (examTopic == null || examTopic.ExamId != examId)
            {
                return NotFound();
            }

            ViewBag.ExamId = examId;
            ViewBag.Topics = _context.Topics.ToList();
            ViewBag.Instructors = _context.Users.Where(u => u.Role == UserRole.Instructor).ToList();
            return View("/Views/Dashboard/Exams/EditTopic.cshtml", examTopic);
        }

        // POST: dashboard/exams/{examId}/examtopics/{id}/edit
        [HttpPost("{id}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int examId, int id, [Bind("Id,ExamId,TopicId,TotalQuestions,UserId,Deadline")] ExamTopic examTopic)
        {
            if (id != examTopic.Id || examId != examTopic.ExamId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Fetch the original ExamTopic to compare changes
                var originalExamTopic = await _context.ExamTopics
                                                    .Include(et => et.Exam)
                                                    .Include(et => et.Topic)
                                                    .Include(et => et.User)
                                                    .AsNoTracking() // Important to not track this entity
                                                    .FirstOrDefaultAsync(et => et.Id == examTopic.Id);

                if (originalExamTopic == null)
                {
                    return NotFound();
                }

                try
                {
                    // Check for duplicate topic and instructor in the same exam
                    bool alreadyExists = await _context.ExamTopics.AnyAsync(et => et.ExamId == examTopic.ExamId && et.TopicId == examTopic.TopicId && et.UserId == examTopic.UserId && et.Id != examTopic.Id);
                    if (alreadyExists)
                    {
                        ModelState.AddModelError("TopicId", "This topic has already been assigned to this instructor for this exam.");
                        TempData["ErrorMessage"] = "This topic has already been assigned to this instructor for this exam.";
                    }
                    else
                    {
                        var existingExamTopic = await _context.ExamTopics.FindAsync(examTopic.Id);
                        if (existingExamTopic == null)
                        {
                            return NotFound();
                        }

                        // Store old values for comparison
                        var oldTotalQuestions = existingExamTopic.TotalQuestions;
                        var oldDeadline = existingExamTopic.Deadline;

                        existingExamTopic.TopicId = examTopic.TopicId;
                        existingExamTopic.TotalQuestions = examTopic.TotalQuestions;
                        existingExamTopic.UserId = examTopic.UserId;
                        existingExamTopic.Deadline = examTopic.Deadline;
                        existingExamTopic.UpdatedAt = DateTime.UtcNow;

                        _context.Update(existingExamTopic);
                        await _context.SaveChangesAsync();

                        // Send email notification if changes occurred
                        if (oldTotalQuestions != existingExamTopic.TotalQuestions || oldDeadline != existingExamTopic.Deadline)
                        {
                            var instructorEmail = originalExamTopic.User?.Email;
                            if (!string.IsNullOrEmpty(instructorEmail))
                            {
                                var subject = $"Exam Topic Update: {originalExamTopic.Exam?.Title} - {originalExamTopic.Topic?.Title}";
                                var body = $"Dear {originalExamTopic.User?.FullName},<br><br>" +
                                           $"The exam topic '{originalExamTopic.Topic?.Title}' for exam '{originalExamTopic.Exam?.Title}' has been updated:<br>";

                                if (oldTotalQuestions != existingExamTopic.TotalQuestions)
                                {
                                    body += $"Total Questions changed from {oldTotalQuestions} to {existingExamTopic.TotalQuestions}.<br>";
                                }
                                if (oldDeadline != existingExamTopic.Deadline)
                                {
                                    body += $"Deadline changed from {oldDeadline.ToShortDateString()} to {existingExamTopic.Deadline.ToShortDateString()}.<br>";
                                }

                                var questionsUrl = $"{Request.Scheme}://{Request.Host}/dashboard/exams/{originalExamTopic.ExamId}/questions";
                                // The user specified dashboard/exams/{id}/questions. I need to construct this.
                                // var questionsUrl = Url.Action("Questions", "Exams", new { id = originalExamTopic.ExamId }, Request.Scheme, Request.Host.Value);

                                body += $"<br>Please review the changes: <a href='{questionsUrl}'>View Exam Questions</a><br><br>" +
                                        "Regards,<br>HMS Team";

                                await _emailService.Send(instructorEmail, subject, body);
                            }
                        }
                        return RedirectToAction("Details", "Exams", new { id = examId }); // Redirect back to Exam Details
                    }
                }
                catch (DbUpdateException ex) when (ex.InnerException is MySqlException mysqlEx && mysqlEx.Number == 1062) // 1062 is for duplicate entry
                {
                    ModelState.AddModelError(string.Empty, "This topic has already been assigned to this instructor for this exam.");
                    TempData["ErrorMessage"] = "This topic has already been assigned to this instructor for this exam.";
                }
            }
            ViewBag.ExamId = examId;
            ViewBag.Topics = _context.Topics.ToList();
            ViewBag.Instructors = _context.Users.Where(u => u.Role == UserRole.Instructor).ToList();
            return View("/Views/Dashboard/Exams/EditTopic.cshtml", examTopic);
        }

        // GET: dashboard/exams/{examId}/examtopics/{id}/details
        [HttpGet("{id}/details")]
        public async Task<IActionResult> Details(int examId, int id)
        {
            var examTopic = await _context.ExamTopics
                .Include(et => et.Exam)
                .Include(et => et.Topic)
                .Include(et => et.User)
                .FirstOrDefaultAsync(m => m.Id == id && m.ExamId == examId);
            if (examTopic == null)
            {
                return NotFound();
            }

            ViewBag.ExamId = examId;
            return View("/Views/Dashboard/Exams/DetailsTopic.cshtml", examTopic);
        }

        // GET: dashboard/exams/{examId}/examtopics/{id}/delete
        [Authorize(Roles = "SuperAdmin")] // Only SuperAdmin can delete ExamTopic
        [HttpGet("{id}/delete")]
        public async Task<IActionResult> Delete(int examId, int id)
        {
            var examTopic = await _context.ExamTopics
                .Include(et => et.Exam)
                .Include(et => et.Topic)
                .Include(et => et.User)
                .FirstOrDefaultAsync(m => m.Id == id && m.ExamId == examId);
            if (examTopic == null)
            {
                return NotFound();
            }

            ViewBag.ExamId = examId;
            return View("/Views/Dashboard/Exams/DeleteTopic.cshtml", examTopic);
        }

        // POST: dashboard/exams/{examId}/examtopics/{id}/delete
        [Authorize(Roles = "SuperAdmin")] // Only SuperAdmin can delete ExamTopic
        [HttpPost("{id}/delete"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int examId, int id)
        {
            var examTopic = await _context.ExamTopics.FindAsync(id);
            if (examTopic != null && examTopic.ExamId == examId)
            {
                _context.ExamTopics.Remove(examTopic);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Details", "Exams", new { id = examId }); // Redirect back to Exam Details
        }

        private bool ExamTopicExists(int id)
        {
            return _context.ExamTopics.Any(e => e.Id == id);
        }
    }
}
