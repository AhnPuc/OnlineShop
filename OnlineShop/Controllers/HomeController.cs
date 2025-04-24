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

        public IActionResult Index()        // Trang chính của ứng dụng
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("admin"))    // Kiểm tra xem người dùng đã đăng nhập
                                                                            // và có vai trò admin hay không
            {
                return RedirectToAction("Index", "Admin");              // Nếu có, chuyển hướng đến trang admin
            }

            var banners = _context.Banners.ToList();                    // Lấy danh sách banner từ cơ sở dữ liệu
            ViewData["banners"] = banners;                              // Lưu danh sách banner vào ViewData để sử dụng trong view

            var newProducts = _context.Products.OrderByDescending(x => x.Id).Take(8).ToList();  // Lấy 8 sản phẩm mới nhất
            ViewData["newProducts"] = newProducts;                      // Lưu danh sách sản phẩm mới vào ViewData

            var bestSellingProducts = _context.BestSellingFinals.ToList();      // Lấy danh sách sản phẩm bán chạy
            ViewData["bestSellingProducts"] = bestSellingProducts;          // Lưu danh sách sản phẩm bán chạy vào ViewData

            return View();                  // Trả về view chính
        }
        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]   
        public IActionResult Error()        // Trả về trang lỗi
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });    // Trả về view lỗi
        }
    }
}
