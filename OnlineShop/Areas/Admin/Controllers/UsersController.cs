using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Models.Db;

namespace OnlineShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class UsersController : Controller
    {
        private readonly OnlineShopContext _context;

        public UsersController(OnlineShopContext context)
        {
            _context = context;
        }

        // GET: Admin/Users
        public async Task<IActionResult> Index()        // Danh sách người dùng
        {
            return View(await _context.Users.ToListAsync());        // Danh sách tất cả người dùng
        }

        // GET: Admin/Users/Create
        public IActionResult Create()       // Tạo mới người dùng
        {
            return View();      // Tạo mới người dùng
        }

        // POST: Admin/Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Email,FullName,Password,IsAdmin,RegisterDate,RecoveryCode")] OnlineShop.Models.Db.User user)      // Tạo mới người dùng
        {
            if (ModelState.IsValid)     // Kiểm tra tính hợp lệ của dữ liệu
            {
                _context.Add(user);     // Thêm người dùng vào cơ sở dữ liệu
                await _context.SaveChangesAsync();      // Lưu thay đổi
                return RedirectToAction(nameof(Index));         // Quay lại danh sách người dùng
            }
            return View(user);
        }

        // GET: Admin/Users/Edit/5
        public async Task<IActionResult> Edit(int? id)      // Chỉnh sửa người dùng
        {
            if (id == null)     // Kiểm tra xem ID có null không
            {
                return NotFound();      // Nếu null thì trả về NotFound
            }

            var user = await _context.Users.FindAsync(id);          // Tìm người dùng theo ID
            if (user == null)           // Nếu không tìm thấy người dùng
            {
                return NotFound();          // Nếu không tìm thấy người dùng
            }
            return View(user);          // Trả về view chỉnh sửa người dùng
        }

        // POST: Admin/Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Email,FullName,Password,IsAdmin,RegisterDate,RecoveryCode")] OnlineShop.Models.Db.User user)            // Chỉnh sửa người dùng
        {
            if (id != user.Id)          // Kiểm tra xem ID có khớp không
            {
                return NotFound();      // Nếu không khớp thì trả về NotFound
            }

            if (ModelState.IsValid)     // Kiểm tra tính hợp lệ của dữ liệu
            {
                try
                {
                    _context.Update(user);          // Cập nhật người dùng
                    await _context.SaveChangesAsync();          // Lưu thay đổi
                }
                catch (DbUpdateConcurrencyException)            // Kiểm tra xem có lỗi không
                {
                    if (!UserExists(user.Id))           // Kiểm tra xem người dùng có tồn tại không
                    {
                        return NotFound();          // Nếu không tồn tại thì trả về NotFound
                    }
                    else
                    {
                        throw;          // Ném lỗi ra ngoài
                    }
                }
                return RedirectToAction(nameof(Index));         // Quay lại danh sách người dùng
            }
            return View(user);              // Nếu không hợp lệ, quay lại trang chỉnh sửa người dùng
        }

        // GET: Admin/Users/Delete/5
        public async Task<IActionResult> Delete(int? id)            // Xóa người dùng
        {
            if (id == null)             // Kiểm tra xem ID có null không
            {
                return NotFound();          // Nếu null thì trả về NotFound
            }

            var user = await _context.Users     
                .FirstOrDefaultAsync(m => m.Id == id);          // Tìm người dùng theo ID
            if (user == null)           // Nếu không tìm thấy người dùng
            {
                return NotFound();          // Nếu không tìm thấy người dùng
            }

            return View(user);          // Trả về view xóa người dùng
        }

        // POST: Admin/Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)            // Xóa người dùng
        {
            var user = await _context.Users.FindAsync(id);          // Tìm người dùng theo ID
            if (user != null)           // Nếu tìm thấy người dùng
            {
                _context.Users.Remove(user);            // Xóa người dùng khỏi cơ sở dữ liệu
            }

            await _context.SaveChangesAsync();          // Lưu thay đổi
            return RedirectToAction(nameof(Index));         // Quay lại danh sách người dùng
        }

        private bool UserExists(int id)         // Kiểm tra xem người dùng có tồn tại không
        {   
            return _context.Users.Any(e => e.Id == id);         // Kiểm tra xem có người dùng nào có ID giống với ID truyền vào không
        }

        
    }
}
