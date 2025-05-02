using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Online_Exam_System.Models;

namespace Online_Exam_System.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        if (User.IsInRole("Admin"))
        {
            return RedirectToAction("GetAllExams", "Admin", new { area = "Admin" });
        }
        else
        {
            return RedirectToAction("Index", "Exam", new { area = "Exam" });
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
