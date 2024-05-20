namespace ChainLib.Node.Controllers;
using ChainLib.Node.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

public class HomeController : Controller
{
    public IActionResult Index() => this.View();

    public IActionResult About()
    {
        this.ViewData["Message"] = "Your application description page.";

        return this.View();
    }

    public IActionResult Contact()
    {
        this.ViewData["Message"] = "Your contact page.";

        return this.View();
    }

    public IActionResult Privacy() => this.View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
}
