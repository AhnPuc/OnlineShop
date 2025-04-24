using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Models.Db;
using System.Security.Claims;

namespace OnlineShop.Areas.User.Controllers
{
    [Authorize]
    [Area("User")]
    public class HomeController : Controller
    {
        private readonly OnlineShopContext _context;

        public HomeController(OnlineShopContext context)
        {
            _context = context;
        }

        public IActionResult Index()        // Trang chính người dùng
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));     // Lấy ID người dùng từ Claims
            var user = _context.Users.FirstOrDefault(x => x.Id == userId);      // Tìm người dùng theo ID
            return View(user);      // Trả về view trang chủ của người dùng
        }
    }
}
