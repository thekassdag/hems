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
    public class TopicsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TopicsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: dashboard/topics
        [HttpGet("")]
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            var totalTopics = await _context.Topics.CountAsync();
            var topics = await _context.Topics
                                    .OrderByDescending(t => t.CreatedAt)
                                    .Skip((pageNumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();

            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalTopics / pageSize);
            ViewBag.HasPreviousPage = pageNumber > 1;
            ViewBag.HasNextPage = pageNumber < ViewBag.TotalPages;

            return View("/Views/Dashboard/Topics/Index.cshtml", topics);
        }

        // GET: dashboard/topics/details/5
        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var topic = await _context.Topics
                .FirstOrDefaultAsync(m => m.Id == id);
            if (topic == null)
            {
                return NotFound();
            }

            return View("/Views/Dashboard/Topics/Details.cshtml", topic);
        }

        // GET: dashboard/topics/create
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View("/Views/Dashboard/Topics/Create.cshtml");
        }

        // POST: dashboard/topics/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description")] Topic topic)
        {
            if (ModelState.IsValid)
            {
                topic.CreatedAt = DateTime.UtcNow;
                topic.UpdatedAt = DateTime.UtcNow;
                _context.Add(topic);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View("/Views/Dashboard/Topics/Create.cshtml", topic);
        }

        // GET: dashboard/topics/edit/5
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var topic = await _context.Topics.FindAsync(id);
            if (topic == null)
            {
                return NotFound();
            }
            return View("/Views/Dashboard/Topics/Edit.cshtml", topic);
        }

        // POST: dashboard/topics/edit/5
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Title,Description")] Topic topic)
        {
            if (id != topic.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingTopic = await _context.Topics.FindAsync(id);
                    if (existingTopic == null)
                    {
                        return NotFound();
                    }

                    existingTopic.Title = topic.Title;
                    existingTopic.Description = topic.Description;
                    existingTopic.UpdatedAt = DateTime.UtcNow;

                    _context.Update(existingTopic);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TopicExists(topic.Id))
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
            return View("/Views/Dashboard/Topics/Edit.cshtml", topic);
        }

        // GET: dashboard/topics/delete/5
        [Authorize(Roles = "SuperAdmin")] // Only SuperAdmin can delete
        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var topic = await _context.Topics
                .FirstOrDefaultAsync(m => m.Id == id);
            if (topic == null)
            {
                return NotFound();
            }

            return View("/Views/Dashboard/Topics/Delete.cshtml", topic);
        }

        // POST: dashboard/topics/delete/5
        [Authorize(Roles = "SuperAdmin")] // Only SuperAdmin can delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var topic = await _context.Topics.FindAsync(id);
            if (topic != null)
            {
                _context.Topics.Remove(topic);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TopicExists(long id)
        {
            return _context.Topics.Any(e => e.Id == id);
        }
    }
}
