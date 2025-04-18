using Microsoft.AspNetCore.Mvc;
using NuGet.ContentModel;
using OnlineShop.Models;
using OnlineShop.Models.Db;
using System.Diagnostics;
namespace OnlineShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly OnlineShopContext _context;
        public HomeController(OnlineShopContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            var banners = _context.Banners.ToList();
            ViewData["banners"] = banners;

            var newProducts = _context.Products.OrderByDescending(x => x.Id).Take(8).ToList();
            ViewData["newProducts"] = newProducts;

            var bestSellingProducts = _context.BestSellingFinals.ToList();
            ViewData["bestSellingProducts"] = bestSellingProducts;
            
            return View();
        }
        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
