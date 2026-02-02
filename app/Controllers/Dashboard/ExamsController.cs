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

namespace HMS.Controllers.Dashboard
{
    [Area("Dashboard")]
    [Route("dashboard/[controller]")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class ExamsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExamsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: dashboard/exams
        [HttpGet("")]
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            var totalExams = await _context.Exams.CountAsync();
            var exams = await _context.Exams
                                    .Include(e => e.User) // Include the User navigation property
                                    .Include(e => e.ExamTopics)!
                                        .ThenInclude(et => et.Topic)
                                        // .ThenInclude(et => et.User) // Instructor for the topic
                                    .OrderByDescending(e => e.CreatedAt)
                                    .Skip((pageNumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();

            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalExams / pageSize);
            ViewBag.HasPreviousPage = pageNumber > 1;
            ViewBag.HasNextPage = pageNumber < ViewBag.TotalPages;

            return View("/Views/Dashboard/Exams/Index.cshtml", exams);
        }

        // GET: dashboard/exams/details/5
        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var exam = await _context.Exams
                .Include(e => e.User)
                .Include(e => e.ExamTopics)!
                    .ThenInclude(et => et.Topic)
                .Include(e => e.ExamTopics)!
                    .ThenInclude(et => et.User) // Instructor for the topic
                .FirstOrDefaultAsync(m => m.Id == id);
            if (exam == null)
            {
                return NotFound();
            }

            return View("/Views/Dashboard/Exams/Details.cshtml", exam);
        }

        // GET: dashboard/exams/create
        [HttpGet("create")]
        public IActionResult Create()
        {
            // You might want to pass a list of users (coordinators) to the view
            ViewBag.Users = _context.Users.Where(u => u.Role == UserRole.Admin || u.Role == UserRole.SuperAdmin || u.Role == UserRole.Instructor).ToList();
            return View("/Views/Dashboard/Exams/Create.cshtml");
        }

        // POST: dashboard/exams/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,DurationMinutes,UserId,IsActive,Batch")] Exam exam)
        {
            if (ModelState.IsValid)
            {
                exam.CreatedAt = DateTime.UtcNow;
                exam.UpdatedAt = DateTime.UtcNow;
                _context.Add(exam);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Users = _context.Users.Where(u => u.Role == UserRole.Admin || u.Role == UserRole.SuperAdmin || u.Role == UserRole.Instructor).ToList();
            return View("/Views/Dashboard/Exams/Create.cshtml", exam);
        }

        // POST: dashboard/exams/StartExamNow/5
        [HttpPost("StartExamNow/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartExamNow(long id)
        {
            var exam = await _context.Exams.FindAsync(id);
            if (exam == null)
            {
                return NotFound();
            }

            exam.StartTime = DateTime.UtcNow;
            exam.UpdatedAt = DateTime.UtcNow; // Update UpdatedAt as well
            _context.Update(exam);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: dashboard/exams/edit/5
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var exam = await _context.Exams.FindAsync(id);
            if (exam == null)
            {
                return NotFound();
            }
            ViewBag.Users = _context.Users.Where(u => u.Role == UserRole.Admin || u.Role == UserRole.SuperAdmin || u.Role == UserRole.Instructor).ToList();
            return View("/Views/Dashboard/Exams/Edit.cshtml", exam);
        }

        // POST: dashboard/exams/edit/5
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Title,Description,DurationMinutes,StartTime,UserId,IsActive,Batch")] Exam exam)
        {
            if (id != exam.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingExam = await _context.Exams.FindAsync(id);
                    if (existingExam == null)
                    {
                        return NotFound();
                    }

                    existingExam.Title = exam.Title;
                    existingExam.Description = exam.Description;
                    existingExam.DurationMinutes = exam.DurationMinutes;
                    existingExam.StartTime = exam.StartTime;
                    existingExam.UserId = exam.UserId;
                    existingExam.IsActive = exam.IsActive;
                    existingExam.Batch = exam.Batch;
                    existingExam.UpdatedAt = DateTime.UtcNow;

                    _context.Update(existingExam);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExamExists(exam.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Users = _context.Users.Where(u => u.Role == UserRole.Admin || u.Role == UserRole.SuperAdmin || u.Role == UserRole.Instructor).ToList();
            return View("/Views/Dashboard/Exams/Edit.cshtml", exam);
        }

        // GET: dashboard/exams/delete/5
        [Authorize(Roles = "SuperAdmin")] // Only SuperAdmin can delete
        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var exam = await _context.Exams
                .Include(e => e.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (exam == null)
            {
                return NotFound();
            }

            return View("/Views/Dashboard/Exams/Delete.cshtml", exam);
        }

        // POST: dashboard/exams/delete/5
        [Authorize(Roles = "SuperAdmin")] // Only SuperAdmin can delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var exam = await _context.Exams.FindAsync(id);
            if (exam != null)
            {
                _context.Exams.Remove(exam);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ExamExists(long id)
        {
            return _context.Exams.Any(e => e.Id == id);
        }
    }
}
