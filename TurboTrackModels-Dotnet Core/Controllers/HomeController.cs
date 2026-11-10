using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TurboTrackModels_Dotnet_Core.Models;

namespace TurboTrackModels_Dotnet_Core.Controllers;

public class HomeController(TTMDbContext context) : Controller
{
    //Create an instance of the database to use in the controller
    //Read only since we are only displaying data in this controller, not modifying it

    public IActionResult Index()
    {
        //Get the most recent models and pass them to the view
        var cars = context.Products.OrderByDescending(m => m.ProductId);
        return View(cars.ToList());
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