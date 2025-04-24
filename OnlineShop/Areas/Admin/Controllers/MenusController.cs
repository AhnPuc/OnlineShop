using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Models.Db;

namespace OnlineShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class MenusController : Controller
    {
        private readonly OnlineShopContext _context;

        public MenusController(OnlineShopContext context)
        {
            _context = context;
        }

        // GET: Admin/Menus
        public async Task<IActionResult> Index()        // Danh sách menu
        {
            return View(await _context.Menus.ToListAsync());        // Danh sách tất cả các menu
        }


        // GET: Admin/Menus/Create
        public IActionResult Create()       // Tạo mới menu
        {
            return View();      // Tạo mới menu
        }

        // POST: Admin/Menus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MenuTitle,Link,Type")] Menu menu)     // Tạo mới menu
        {
            if (ModelState.IsValid)     // Kiểm tra tính hợp lệ của dữ liệu
            {
                _context.Add(menu);     // Thêm menu vào cơ sở dữ liệu
                await _context.SaveChangesAsync();      // Lưu thay đổi
                return RedirectToAction(nameof(Index));     // Quay lại danh sách menu
            }
            return View(menu);      // Nếu không hợp lệ, quay lại trang tạo menu
        }

        // GET: Admin/Menus/Edit/5
        public async Task<IActionResult> Edit(int? id)      // Chỉnh sửa menu
        {
            if (id == null)     // Kiểm tra xe ID có null không
            {
                return NotFound();      // Nếu null thì trả về NotFound
            }

            var menu = await _context.Menus.FindAsync(id);      // Tìm menu theo ID
            if (menu == null)       // Nếu không tìm thấy menu
            {
                return NotFound();      // Nếu không tìm thấy menu
            }
            return View(menu);      // Trả về view chỉnh sửa menu
        }

        // POST: Admin/Menus/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MenuTitle,Link,Type")] Menu menu)       // Chỉnh sửa menu
        {
            if (id != menu.Id)      // Kiểm tra xe ID có khớp với ID của menu không
            {
                return NotFound();      // Nếu không khớp thì trả về NotFound
            }

            if (ModelState.IsValid)     // Kiểm tra tính hợp lệ của dữ liệu
            {
                try
                {
                    _context.Update(menu);      // Cập nhật menu vào cơ sở dữ liệu
                    await _context.SaveChangesAsync();      // Lưu thay đổi
                }
                catch (DbUpdateConcurrencyException)        // Kiểm tra xem có lỗi không
                {
                    if (!MenuExists(menu.Id))       // Kiểm tra xem menu có tồn tại không
                    {
                        return NotFound();      // Nếu không tồn tại thì trả về NotFound
                    }
                    else
                    {
                        throw;      // Ném lỗi ra ngoài
                    }   
                }
                return RedirectToAction(nameof(Index));     // Quay lại danh sách menu
            }
            return View(menu);      // Nếu không hợp lệ, quay lại trang chỉnh sửa menu
        }

        // GET: Admin/Menus/Delete/5
        public async Task<IActionResult> Delete(int? id)        // Xóa menu
        {
            if (id == null)     // Kiểm tra xem ID có null không
            {
                return NotFound();      // Nếu null thì trả về NotFound
            }

            var menu = await _context.Menus     // Tìm menu theo ID
                .FirstOrDefaultAsync(m => m.Id == id);      // Lấy menu đầu tiên
            if (menu == null)       // Nếu không tìm thấy menu
            {
                return NotFound();      // Nếu không tìm thấy menu
            }

            return View(menu);          // Trả về view xóa menu
        }

        // POST: Admin/Menus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)        // Xóa menu
        {
            var menu = await _context.Menus.FindAsync(id);      //  Tìm menu theo ID
            if (menu != null)       // Nếu tìm thấy menu
            {
                _context.Menus.Remove(menu);        // Xóa menu khỏi cơ sở dữ liệu
            }

            await _context.SaveChangesAsync();      // Lưu thay đổi
            return RedirectToAction(nameof(Index));     // Quay lại danh sách menu
        }

        private bool MenuExists(int id)     // Kiểm tra xem menu có tồn tại không
        {
            return _context.Menus.Any(e => e.Id == id);     // Kiểm tra xem có menu nào có ID giống với ID truyền vào không
        }
    }
}
