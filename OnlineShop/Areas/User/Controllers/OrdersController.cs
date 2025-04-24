using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Models.Db;

namespace OnlineShop.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly OnlineShopContext _context;

        public OrdersController(OnlineShopContext context)
        {
            _context = context;
        }

        // GET: User/Orders
        public async Task<IActionResult> Index()        // Danh sách đơn hàng
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));     // Lấy ID người dùng từ Claims
            var result = await _context.Orders.Where(x => x.UserId == userId).OrderByDescending(x => x.Id).ToListAsync();       // Danh sách đơn hàng của người dùng
            return View(result);
        }

        // GET: User/Orders/Details/5
        public async Task<IActionResult> Details(int? id)       // Chi tiết đơn hàng
        {
            if (id == null)     // Kiểm tra xem ID có null không
            {
                return NotFound();      // Nếu null thì trả về NotFound
            }

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));     // Lấy ID người dùng từ Claims

            var order = await _context.Orders
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);         // Tìm đơn hàng theo ID và ID người dùng
            if (order == null)      // Nếu không tìm thấy đơn hàng
            {
                return NotFound();      // Hiển thị không tìm thấy đơn hàng
            }

            ViewData["OrderDetails"] = await _context.OrderDetails.
                                        Where(x => x.OrderId == id).ToListAsync();      // Lấy danh sách chi tiết đơn hàng theo ID

            return View(order);     // Trả về view chi tiết đơn hàng
        }   


        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}
