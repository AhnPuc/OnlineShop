using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Models.Db;

namespace OnlineShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class CouponsController : Controller
    {
        private readonly OnlineShopContext _context;

        public CouponsController(OnlineShopContext context)
        {
            _context = context;
        }

        // GET: Admin/Coupons
        public async Task<IActionResult> Index()        // Danh sách mã giảm giá
        {
            return View(await _context.Coupons.ToListAsync());      // Danh sách tất cả các mã giảm giá
        }

        // GET: Admin/Coupons/Create
        public IActionResult Create()       // Tạo mới mã giảm giá
        {
            return View();      // Tạo mới mã giảm giá
        }

        // POST: Admin/Coupons/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Code,Discount")] Coupon coupon)       // Tạo mới mã giảm giá
        {
            if (ModelState.IsValid)     // Kiểm tra tính hợp lệ của dữ liệu
            {
                _context.Add(coupon);       // Thêm mã giảm giá vào cơ sở dữ liệu
                await _context.SaveChangesAsync();      // Lưu thay đổi
                return RedirectToAction(nameof(Index));     // Quay lại danh sách mã giảm giá
            }
            return View(coupon);        // Nếu không hợp lệ, quay lại trang tạo mã giảm giá
        }

        // GET: Admin/Coupons/Edit/5
        public async Task<IActionResult> Edit(int? id)      // Chỉnh sửa mã giảm giá
        {
            if (id == null)     // Kiểm tra xem ID có null không
            {
                return NotFound();      // Nếu null thì trả về NotFound
            }

            var coupon = await _context.Coupons.FindAsync(id);      // Tìm mã giảm giá theo ID
            if (coupon == null)     // Kiểm tra xem mã giảm giá có tồn tại không
            {
                return NotFound();      // Nếu không tồn tại thì trả về NotFound
            }
            return View(coupon);        // Trả về view chỉnh sửa mã giảm giá
        }

        // POST: Admin/Coupons/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Code,Discount")] Coupon coupon)     // Chỉnh sửa mã giảm giá
        {
            if (id != coupon.Id)        // Kiểm tra xem ID có khớp với mã giảm giá không
            {
                return NotFound();      // Nếu không khớp thì trả về NotFound
            }

            if (ModelState.IsValid)     // Kiểm tra tính hợp lệ của dữ liệu
            {
                try
                {
                    _context.Update(coupon);        // Cập nhật mã giảm giá vào cơ sở dữ liệu
                    await _context.SaveChangesAsync();      // Lưu thay đổi
                }
                catch (DbUpdateConcurrencyException)        // Kiểm tra xem có lỗi không
                {
                    if (!CouponExists(coupon.Id))       // Kiểm tra xem mã giảm giá có tồn tại không
                    {
                        return NotFound();      // Nếu không tồn tại thì trả về NotFound
                    }
                    else
                    {
                        throw;      // Ném lỗi ra ngoài
                    }
                }
                return RedirectToAction(nameof(Index));     // Quay lại danh sách mã giảm giá
            }
            return View(coupon);        // Nếu không hợp lệ, quay lại trang chỉnh sửa mã giảm giá
        }

        // GET: Admin/Coupons/Delete/5
        public async Task<IActionResult> Delete(int? id)        // Xóa mã giảm giá
        {
            if (id == null)     // Kiểm tra ID có null không
            {
                return NotFound();      // Nếu null thì trả về NotFound
            }

            var coupon = await _context.Coupons                 // Lấy danh sách mã giảm giá
                .FirstOrDefaultAsync(m => m.Id == id);      // Tìm mã giảm giá theo ID
            if (coupon == null)     // Kiểm tra mã giảm giá có null không
            {
                return NotFound();      // Nếu null thì trả về NotFound
            }

            return View(coupon);        // Trả về view xóa mã giảm giá
        }

        // POST: Admin/Coupons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)        // Xóa mã giảm giá
        {
            var coupon = await _context.Coupons.FindAsync(id);      // Tìm mã giảm giá theo ID
            if (coupon != null)     // Kiểm tra mã giảm giá có null không
            {   
                _context.Coupons.Remove(coupon);        // Xóa mã giảm giá khỏi cơ sở dữ liệu
            }

            await _context.SaveChangesAsync();      // Lưu thay đổi
            return RedirectToAction(nameof(Index));     // Quay lại danh sách mã giảm giá
        }

        private bool CouponExists(int id)       // Kiểm tra mã giảm giá có tồn tại không
        {
            return _context.Coupons.Any(e => e.Id == id);       //  Kiểm tra xem có mã giảm giá nào có ID giống với ID truyền vào không
        }
    }
}
