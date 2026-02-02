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
    public class LabMachinesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LabMachinesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: dashboard/labmachines
        [HttpGet("")]
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            var totalLabMachines = await _context.LabMachines.CountAsync();
            var labMachines = await _context.LabMachines
                                    .OrderByDescending(lm => lm.CreatedAt)
                                    .Skip((pageNumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();

            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalLabMachines / pageSize);
            ViewBag.HasPreviousPage = pageNumber > 1;
            ViewBag.HasNextPage = pageNumber < ViewBag.TotalPages;

            return View("/Views/Dashboard/LabMachines/Index.cshtml", labMachines);
        }

        // GET: dashboard/labmachines/details/5
        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var labMachine = await _context.LabMachines
                .FirstOrDefaultAsync(m => m.Id == id);
            if (labMachine == null)
            {
                return NotFound();
            }

            return View("/Views/Dashboard/LabMachines/Details.cshtml", labMachine);
        }

        // GET: dashboard/labmachines/create
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View("/Views/Dashboard/LabMachines/Create.cshtml");
        }

        // POST: dashboard/labmachines/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LabCode,MachineName,DeviceId,Status")] LabMachine labMachine)
        {
            if (ModelState.IsValid)
            {
                _context.Add(labMachine);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View("/Views/Dashboard/LabMachines/Create.cshtml", labMachine);
        }

        // GET: dashboard/labmachines/edit/5
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var labMachine = await _context.LabMachines.FindAsync(id);
            if (labMachine == null)
            {
                return NotFound();
            }
            return View("/Views/Dashboard/LabMachines/Edit.cshtml", labMachine);
        }

        // POST: dashboard/labmachines/edit/5
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,LabCode,MachineName,DeviceId,Status")] LabMachine labMachine)
        {
            if (id != labMachine.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingLabMachine = await _context.LabMachines.FindAsync(id);
                    if (existingLabMachine == null)
                    {
                        return NotFound();
                    }

                    existingLabMachine.LabCode = labMachine.LabCode;
                    existingLabMachine.MachineName = labMachine.MachineName;
                    existingLabMachine.DeviceId = labMachine.DeviceId;
                    existingLabMachine.Status = labMachine.Status;
                    // existingLabMachine.UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); // Removed as database handles this

                    _context.Update(existingLabMachine);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LabMachineExists(labMachine.Id))
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
            return View("/Views/Dashboard/LabMachines/Edit.cshtml", labMachine);
        }

        // GET: dashboard/labmachines/delete/5
        [Authorize(Roles = "SuperAdmin")] // Only SuperAdmin can delete
        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var labMachine = await _context.LabMachines
                .FirstOrDefaultAsync(m => m.Id == id);
            if (labMachine == null)
            {
                return NotFound();
            }

            return View("/Views/Dashboard/LabMachines/Delete.cshtml", labMachine);
        }

        // POST: dashboard/labmachines/delete/5
        [Authorize(Roles = "SuperAdmin")] // Only SuperAdmin can delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var labMachine = await _context.LabMachines.FindAsync(id);
            if (labMachine != null)
            {
                _context.LabMachines.Remove(labMachine);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool LabMachineExists(long id)
        {
            return _context.LabMachines.Any(e => e.Id == id);
        }
    }
}
