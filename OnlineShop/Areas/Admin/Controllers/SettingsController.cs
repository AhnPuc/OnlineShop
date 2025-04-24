using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Models.Db;

namespace OnlineShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class SettingsController : Controller
    {
        private readonly OnlineShopContext _context;

        public SettingsController(OnlineShopContext context)
        {
            _context = context;
        }

        

        // GET: Admin/Settings/Edit/5
        public async Task<IActionResult> Edit()         // Chỉnh sửa cài đặt
        {
            var setting = await _context.Settings.FirstAsync();     // Lấy cài đặt đầu tiên trong cơ sở dữ liệu
            if (setting == null)        // Kiểm tra xem cài đặt có tồn tại không
            {
                return NotFound();          // Nếu không tồn tại thì trả về NotFound
            }
            return View(setting);           // Trả về view chỉnh sửa cài đặt
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, 
            [Bind("Id,Shipping,Title,Address,Email,Phone,CopyRight,Instagram,FaceBook,GooglePlus,Youtube,Twitter,Logo")] 
            Setting setting, IFormFile? newLogo)        // Chỉnh sửa cài đặt
        {
            if (id != setting.Id)       // Kiểm tra xem ID có khớp không
            {
                return NotFound();      // Nếu không khớp thì trả về NotFound
            }

            if (ModelState.IsValid)     // Kiểm tra tính hợp lệ của dữ liệu
            {
                try
                {
                    if (newLogo != null)        // Kiểm tra xem có file hình ảnh mới không
                    {
                        string d = Directory.GetCurrentDirectory();     // Lấy đường dẫn hiện tại
                        string path = d + "\\wwwroot\\images\\" + setting.Logo;     // Đường dẫn đến thư mục lưu hình ảnh

                        if (System.IO.File.Exists(path))            // Kiểm tra xem file hình ảnh có tồn tại không
                        {
                            System.IO.File.Delete(path);            // Nếu tồn tại thì xóa file hình ảnh
                        }

                        setting.Logo = Guid.NewGuid() + Path.GetExtension(newLogo.FileName);        // Tạo tên file ngẫu nhiên
                        path = d + "\\wwwroot\\images\\" + setting.Logo;        // Đường dẫn đến thư mục lưu hình ảnh

                        using (var stream = new FileStream(path, FileMode.Create))      // Tạo file stream
                        {
                            newLogo.CopyTo(stream);         // Lưu file hình ảnh vào thư mục
                        }

                        _context.Update(setting);           // Cập nhật cài đặt vào cơ sở dữ liệu
                        await _context.SaveChangesAsync();      // Lưu thay đổi

                        TempData["message"] = "Setting saved";          // Thông báo đã lưu cài đặt
                    }
                }
                catch (DbUpdateConcurrencyException)        // Kiểm tra xem có lỗi không
                {
                    if (!SettingExists(setting.Id))         // Kiểm tra xem cài đặt có tồn tại không
                    {
                        return NotFound();          // Nếu không tồn tại thì trả về NotFound
                    }
                    else
                    {
                        throw;          // Ném lỗi ra ngoài
                    }
                }
            }
            return Redirect($"/Admin/Settings/Edit");           // Quay lại trang chỉnh sửa cài đặt
        }

        private bool SettingExists(int id)          // Kiểm tra xem cài đặt có tồn tại không
        {
            return _context.Settings.Any(e => e.Id == id);          // Kiểm tra xem có cài đặt nào có ID giống với ID truyền vào không
        }
    }
}
