using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Models.Db;

namespace OnlineShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class OrdersController : Controller
    {
        private readonly OnlineShopContext _context;

        public OrdersController(OnlineShopContext context)
        {
            _context = context;
        }

        // GET: Admin/Orders
        public async Task<IActionResult> Index()        // Danh sách đơn hàng
        {
            return View(await _context.Orders.OrderByDescending(x => x.Id).ToListAsync());      // Danh sách tất cả các đơn hàng
        }

        // GET: Admin/Orders/Create
        public IActionResult Create()       // Tạo mới đơn hàng
        {
            return View();      //  Tạo mới đơn hàng
        }

        // POST: Admin/Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,FirstName,LastName,CompanyName,Country,Address,City,Email,Phone,Comment,CouponCode,CouponDiscount,Shipping,SubTotal,Total,CreateDate,TransId,Status")] Order order)        // Tạo mới đơn hàng
        {
            if (ModelState.IsValid)         // Kiểm tra tính hợp lệ của dữ liệu
            {
                _context.Add(order);        // Thêm đơn hàng vào cơ sở dữ liệu
                await _context.SaveChangesAsync();      // Lưu thay đổi
                return RedirectToAction(nameof(Index));     // Quay lại danh sách đơn hàng
            }
            return View(order);         // Nếu không hợp lệ, quay lại trang tạo đơn hàng
        }

        // GET: Admin/Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)      // Chỉnh sửa đơn hàng
        {
            if (id == null)         // Kiểm tra xem ID có null không
            {
                return NotFound();      // Nếu null thì trả về NotFound
            }

            var order = await _context.Orders.FindAsync(id);        // Tìm đơn hàng theo ID
            if (order == null)      // Nếu không tìm thấy đơn hàng
            {
                return NotFound();              // Nếu không tìm thấy đơn hàng
            }
            ViewData["OrderDetails"] = _context.OrderDetails.       
                                        Where(x => x.OrderId == id).ToList();       // Lấy danh sách chi tiết đơn hàng theo ID
            return View(order);
        }

        // POST: Admin/Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,FirstName,LastName,CompanyName,Country,Address,City,Email,Phone,Comment,CouponCode,CouponDiscount,Shipping,SubTotal,Total,CreateDate,TransId,Status")] Order order)      // Chỉnh sửa đơn hàng
        {
            ViewData["OrderDetails"] = _context.OrderDetails.
                                        Where(x => x.OrderId == id).ToList();       // Lấy danh sách chi tiết đơn hàng theo ID
            if (id != order.Id)     // Kiểm tra xem ID có khớp với đơn hàng không
            {
                return NotFound();      //  Nếu không khớp thì trả về NotFound
            }

            if (ModelState.IsValid)     // Kiểm tra tính hợp lệ của dữ liệu
            {
                try
                {
                    _context.Update(order);     // Cập nhật đơn hàng vào cơ sở dữ liệu
                    await _context.SaveChangesAsync();          // Lưu thay đổi
                }
                catch (DbUpdateConcurrencyException)        // Kiểm tra xem có lỗi không
                {
                    if (!OrderExists(order.Id))         // Kiểm tra xem đơn hàng có tồn tại không
                    {
                        return NotFound();      // Nếu không tồn tại thì trả về NotFound
                    }
                    else
                    {
                        throw;          // Ném lỗi ra ngoài
                    }
                }
                return RedirectToAction(nameof(Index));     // Quay lại danh sách đơn hàng
            }
            return View(order);         // Nếu không hợp lệ, quay lại trang chỉnh sửa đơn hàng
        }

        // GET: Admin/Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)            // Xóa đơn hàng
        {
            if (id == null)             // Kiểm tra xem ID có null không
            {
                return NotFound();          // Nếu null thì trả về NotFound
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(m => m.Id == id);          // Tìm đơn hàng theo ID
            if (order == null)          // Nếu không tìm thấy đơn hàng
            {
                return NotFound();          // Nếu không tìm thấy đơn hàng
            }

            return View(order);         // Trả về view xóa đơn hàng
        }

        // POST: Admin/Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)            // Xóa đơn hàng
        {
            var order = await _context.Orders.FindAsync(id);            // Tìm đơn hàng theo ID
            if (order != null)          // Nếu tìm thấy đơn hàng    
            {
                _context.Orders.Remove(order);          // Xóa đơn hàng khỏi cơ sở dữ liệu
            }

            var orderDetails = _context.OrderDetails.
                                        Where(x => x.OrderId == id).ToList();       // Lấy danh sách chi tiết đơn hàng theo ID
            _context.OrderDetails.RemoveRange(orderDetails);            // Xóa danh sách chi tiết đơn hàng khỏi cơ sở dữ liệu

            await _context.SaveChangesAsync();          // Lưu thay đổi
            return RedirectToAction(nameof(Index));         // Quay lại danh sách đơn hàng
        }

        private bool OrderExists(int id)            // Kiểm tra xem đơn hàng có tồn tại không
        {
            return _context.Orders.Any(e => e.Id == id);            // Kiểm tra xem có đơn hàng nào có ID giống với ID truyền vào không
        }
    }
}
