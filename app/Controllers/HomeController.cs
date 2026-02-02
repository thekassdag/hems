using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HMS.Models;
using HMS.Data;
using Microsoft.EntityFrameworkCore;
using HMS.Services; // Added for FingerprintService
using System.Security.Cryptography; // Still needed for SHA256 in FingerprintService
using System.Text; // Still needed for Encoding in FingerprintService

namespace HMS.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    // GET: /Home/Info
    [HttpGet]
    [Route("/device-info")]
    public IActionResult Info()
    {
        var fingerprint = FingerprintService.Generate(HttpContext.Request);
        ViewBag.DeviceFingerprint = fingerprint;
        return View("/Views/Home/Info.cshtml");
    }

    [HttpPost("index")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartExam(long examCode)
    {
        if (examCode == 0)
        {
            ModelState.AddModelError(string.Empty, "Please enter an exam code.");
            return View("Index");
        }

        // Generate backend fingerprint
        var deviceFingerprint = FingerprintService.Generate(HttpContext.Request);

        // Check if the device is authorized (exists in LabMachines)
        var authorizedDevice = await _context.LabMachines
                                             .FirstOrDefaultAsync(lm => lm.DeviceId == deviceFingerprint);

        if (authorizedDevice == null)
        {
            ModelState.AddModelError(string.Empty, "Unauthorized device. Please use an authorized lab machine.");
            return View("Index");
        }

        var exam = await _context.Exams
                                 .FirstOrDefaultAsync(e => e.ExamCode == examCode && e.IsActive);

        if (exam == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid or inactive exam code.");
            return View("Index");
        }

        // 1. Check if the exam is expired
        if (exam.StartTime.HasValue)
        {
            var examEndTime = exam.StartTime.Value.AddMinutes(exam.DurationMinutes);
            if (DateTime.UtcNow > examEndTime)
            {
                ModelState.AddModelError(string.Empty, "This exam has expired.");
                return View("Index");
            }
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Exam is not started, please wait until they say start.");
            return View("Index");
        }

        // If all validations pass, redirect to the exam template
        return RedirectToAction("Index", "ExamRoom", new { id = exam.Id });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

