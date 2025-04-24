using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Models.Db;

namespace OnlineShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class BannersController : Controller
    {
        private readonly OnlineShopContext _context;

        public BannersController(OnlineShopContext context)
        {
            _context = context;
        }

        // GET: Admin/Banners
        public async Task<IActionResult> Index()      // Danh sách banner  
        {
            return View(await _context.Banners.ToListAsync());      // Danh sách tất cả các banner
        }
        

        // GET: Admin/Banners/Create
        public IActionResult Create()       // Tạo mới banner
        {
            return View();      // Tạo mới banner
        }

        // POST: Admin/Banners/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,SubTitle,ImageName,Priority,Link,Position")] Banner banner, IFormFile ImageFile)        // Tạo mới banner
        {
            if (ModelState.IsValid)     // Kiểm tra tính hợp lệ của model
            {
                /*SAVE IMAGE*/
                if (ImageFile != null)      // Kiểm tra xem có file hình ảnh không
                {
                    banner.ImageName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(ImageFile.FileName);     // Tạo tên file ngẫu nhiên
                    string fn;      // Lấy đường dẫn hiện tại
                    fn = Directory.GetCurrentDirectory();       // Đường dẫn đến thư mục hiện tại
                    string ImagePath = fn + "\\wwwroot\\images\\banners\\" + banner.ImageName;      // Đường dẫn đến thư mục lưu hình ảnh       

                    using (var stream = new FileStream(ImagePath, FileMode.Create))     // Tạo file stream
                    {
                        ImageFile.CopyTo(stream);       // Lưu file hình ảnh vào thư mục
                    }      
                } 
                    
                _context.Add(banner);       // Thêm banner vào cơ sở dữ liệu
                await _context.SaveChangesAsync();      // Lưu thay đổi
                return RedirectToAction(nameof(Index));     // Quay lại danh sách banner    
            }
            return View(banner);        // Trả về view tạo mới banner
        }

        // GET: Admin/Banners/Edit/5
        public async Task<IActionResult> Edit(int? id)      // Chỉnh sửa banner
        {
            if (id == null)     // Kiểm tra ID có null không
            {
                return NotFound();      // Nếu null thì trả về NotFound
            }

            var banner = await _context.Banners.FindAsync(id);      // Tìm banner theo ID
            if (banner == null)     // Kiểm tra banner có null không
            {
                return NotFound();      // Nếu null thì trả về NotFound
            }
            return View(banner);        // Trả về view chỉnh sửa banner
        }

        // POST: Admin/Banners/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,SubTitle,ImageName,Priority,Link,Position")] Banner banner, IFormFile? ImageFile)     // Chỉnh sửa banner
        {
            if (id != banner.Id)        // Kiểm tra ID có null không
            {
                return NotFound();      // Nếu null thì trả về NotFound
            }

            if (ModelState.IsValid)     // Kiểm tra tính hợp lệ của model
            {
                try
                {
                    if (ImageFile != null)      // Kiểm tra xem có file hình ảnh không
                    {
                        string org_fn;      // Lấy đường dẫn hiện tại
                        org_fn = Directory.GetCurrentDirectory() + "/wwwroot/images/banners/" + banner.ImageName;       // Đường dẫn đến thư mục lưu hình ảnh

                        if (System.IO.File.Exists(org_fn))  // Kiểm tra xem file hình ảnh có tồn tại không
                        {
                            System.IO.File.Delete(org_fn);      // Nếu tồn tại thì xóa file hình ảnh
                        }

                        banner.ImageName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);       // Tạo tên file ngẫu nhiên

                        string ImagePath;       // Lấy đường dẫn hiện tại
                        ImagePath = Directory.GetCurrentDirectory() + "/wwwroot/images/banners/" + banner.ImageName;    //  Đường dẫn đến thư mục lưu hình ảnh
                        using (var stream = new FileStream(ImagePath,FileMode.Create))      // Tạo file stream
                        {
                            ImageFile.CopyTo(stream);       // Lưu file hình ảnh vào thư mục
                        }    
                    }    
                    _context.Update(banner);        // Cập nhật banner vào cơ sở dữ liệu
                    await _context.SaveChangesAsync();      // Lưu thay đổi
                }
                catch (DbUpdateConcurrencyException)    // Kiểm tra xem có lỗi không
                {
                    if (!BannerExists(banner.Id))   // Kiểm tra banner có tồn tại không
                    {
                        return NotFound();      // Nếu không tồn tại thì trả về NotFound
                    }
                    else
                    {
                        throw;      // Ném lỗi ra ngoài
                    }
                }
                return RedirectToAction(nameof(Index));     // Quay lại danh sách banner
            }
            return View(banner);        // Trả về view chỉnh sửa banner
        }

        // GET: Admin/Banners/Delete/5
        public async Task<IActionResult> Delete(int? id)        // Xóa banner
        {
            if (id == null)     // Kiểm tra ID có null không
            {
                return NotFound();      // Nếu null thì trả về NotFound
            }

            var banner = await _context.Banners     // Lấy danh sách banner
                .FirstOrDefaultAsync(m => m.Id == id);      // Tìm banner theo ID
            if (banner == null)     // Kiểm tra banner có null không
            {
                return NotFound();      // Nếu null thì trả về NotFound
            }

            return View(banner);        // Trả về view xóa banner
        }

        // POST: Admin/Banners/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)        // Xóa banner
        {
            var banner = await _context.Banners.FindAsync(id);      // Tìm banner theo ID
            if (banner != null)     // Kiểm tra banner có null không
            {
                string org_fn;      // Lấy đường dẫn hiện tại
                org_fn = Directory.GetCurrentDirectory() + "/wwwroot/images/banners/" + banner.ImageName;       // Đường dẫn đến thư mục lưu hình ảnh

                if (System.IO.File.Exists(org_fn))      // Kiểm tra xem file hình ảnh có tồn tại không
                {
                    System.IO.File.Delete(org_fn);      // Nếu tồn tại thì xóa file hình ảnh
                }


                _context.Banners.Remove(banner);        // Xóa banner khỏi cơ sở dữ liệu
            }

            await _context.SaveChangesAsync();      // Lưu thay đổi
            return RedirectToAction(nameof(Index));     //  Quay lại danh sách banner
        }

        private bool BannerExists(int id)       // Kiểm tra banner có tồn tại không
        {
            return _context.Banners.Any(e => e.Id == id);       // Kiểm tra xem có banner nào có ID giống với ID truyền vào không
        }
    }
}
