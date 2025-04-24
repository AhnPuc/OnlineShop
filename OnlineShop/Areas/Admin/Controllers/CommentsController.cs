using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Models.Db;

namespace OnlineShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class CommentsController : Controller
    {
        private readonly OnlineShopContext _context;

        public CommentsController(OnlineShopContext context)
        {
            _context = context;
        }

        // GET: Admin/Comments
        public async Task<IActionResult> Index()        // Danh sách bình luận
        {
            return View(await _context.Comments.ToListAsync());     // Danh sách tất cả các bình luận
        }


        // GET: Admin/Comments/Create
        public IActionResult Create()       // Tạo mới bình luận
        {
            return View();      // Tạo mới bình luận
        }

        // POST: Admin/Comments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Email,CommentText,ProductId,CreateDate")] Comment comment)       // Tạo mới bình luận
        {
            if (ModelState.IsValid)     // Kiểm tra tính hợp lệ của model
            {
                _context.Add(comment);      // Thêm bình luận vào cơ sở dữ liệu
                await _context.SaveChangesAsync();      // Lưu thay đổi
                return RedirectToAction(nameof(Index));     // Quay lại danh sách bình luận
            }
            return View(comment);       // Nếu không hợp lệ, quay lại trang tạo bình luận
        }

        // GET: Admin/Comments/Edit/5
        public async Task<IActionResult> Edit(int? id)      // Chỉnh sửa bình luận
        {
            if (id == null)     // Kiểm tra xem ID có null không
            {
                return NotFound();  // Nếu null thì trả về NotFound
            }

            var comment = await _context.Comments.FindAsync(id);        // Tìm bình luận theo ID
            if (comment == null)        // Kiểm tra xem bình luận có tồn tại không
            {
                return NotFound();      // Nếu không tồn tại thì trả về NotFound
            }
            return View(comment);       // Trả về view chỉnh sửa bình luận
        }

        // POST: Admin/Comments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,CommentText,ProductId,CreateDate")] Comment comment)     //  Chỉnh sửa bình luận
        {
            if (id != comment.Id)       // Kiểm tra xem ID có khớp với bình luận không
            {
                return NotFound();  // Nếu không khớp thì trả về NotFound
            }

            if (ModelState.IsValid)     // Kiểm tra tính hợp lệ của model
            {
                try
                {
                    _context.Update(comment);       // Cập nhật bình luận vào cơ sở dữ liệu
                    await _context.SaveChangesAsync();      // Lưu thay đổi
                }
                catch (DbUpdateConcurrencyException)        // Kiểm tra xem có lỗi không
                {
                    if (!CommentExists(comment.Id))     // Kiểm tra xem bình luận có tồn tại không
                    {
                        return NotFound();      // Nếu không tồn tại thì trả về NotFound
                    }
                    else
                    {
                        throw;          // Ném lỗi ra ngoài
                    }
                }
                return RedirectToAction(nameof(Index));     //  Quay lại danh sách bình luận
            }
            return View(comment);       // Nếu không hợp lệ, quay lại trang chỉnh sửa bình luận
        }

        // GET: Admin/Comments/Delete/5
        public async Task<IActionResult> Delete(int? id)        // Xóa bình luận
        {
            if (id == null)     // Kiểm tra xem ID có null không
            {
                return NotFound();      // Nếu null thì trả về NotFound
            }

            var comment = await _context.Comments       // Tìm bình luận theo ID
                .FirstOrDefaultAsync(m => m.Id == id);      // Lấy bình luận đầu tiên
            if (comment == null)        // Kiểm tra xem bình luận có tồn tại không
            {
                return NotFound();      // Nếu không tồn tại thì trả về NotFound
            }

            return View(comment);       // Trả về view xóa bình luận
        }

        // POST: Admin/Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)        // Xóa bình luận
        {
            var comment = await _context.Comments.FindAsync(id);        // Tìm bình luận theo ID
            if (comment != null)        // Kiểm tra xem bình luận có tồn tại không
            {
                _context.Comments.Remove(comment);      // Xóa bình luận khỏi cơ sở dữ liệu
            }

            await _context.SaveChangesAsync();      // Lưu thay đổi
            return RedirectToAction(nameof(Index));     // Quay lại danh sách bình luận
        }

        private bool CommentExists(int id)      // Kiểm tra xem bình luận có tồn tại không
        {
            return _context.Comments.Any(e => e.Id == id);      // Kiểm tra xem có bình luận nào có ID giống với ID truyền vào không
        }
    }
}
