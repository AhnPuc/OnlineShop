using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Models.Db;

namespace OnlineShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class ProductsController : Controller
    {
        private readonly OnlineShopContext _context;

        public ProductsController(OnlineShopContext context)
        {
            _context = context;
        }

        // GET: Admin/Products
        public async Task<IActionResult> Index()        // Danh sách sản phẩm
        {
            return View(await _context.Products.ToListAsync());     // Danh sách tất cả các sản phẩm
        }
        
        public IActionResult DeleteGallery(int id)      // Xóa hình ảnh trong thư viện
        {
            var gallery = _context.ProductGaleries.FirstOrDefault(x => x.Id == id);     // Tìm hình ảnh theo ID
            if (gallery == null)        // Nếu không tìm thấy hình ảnh
            {
                return NotFound();      // Trả về NotFound
            }
            string d = Directory.GetCurrentDirectory();         // Lấy đường dẫn hiện tại
            string fn = d + "\\wwwroot\\images\\banners\\" + gallery.ImageName;         // Đường dẫn đến thư mục lưu hình ảnh

            if (System.IO.File.Exists(fn))          // Kiểm tra xem file hình ảnh có tồn tại không
            {
                System.IO.File.Delete(fn);      // Nếu tồn tại thì xóa file hình ảnh
            }
            _context.Remove(gallery);           // Xóa hình ảnh khỏi cơ sở dữ liệu
            _context.SaveChanges();             // Lưu thay đổi

            return Redirect("edit/" + gallery.ProductId);           // Quay lại trang chỉnh sửa sản phẩm
        }

        // GET: Admin/Products/Create
        public IActionResult Create()           // Tạo mới sản phẩm
        {
            return View();          // Tạo mới sản phẩm
        }

        // POST: Admin/Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,FullDesc,Price,Discount,ImageName,Qty,Tags,VideoUrl")] Product product, IFormFile MainImage, IFormFile[]? GalleryImages)        // Tạo mới sản phẩm
        {
            if (ModelState.IsValid)         // Kiểm tra tính hợp lệ của model
            {
                if (MainImage != null)      // Kiểm tra xem có file hình ảnh chính không
                {
                    product.ImageName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(MainImage.FileName);        // Tạo tên file ngẫu nhiên
                    string fn;      // Lấy đường dẫn hiện tại
                    fn = Directory.GetCurrentDirectory();       // Đường dẫn đến thư mục hiện tại
                    string ImagePath = fn + "\\wwwroot\\images\\banners\\" + product.ImageName;     // Đường dẫn đến thư mục lưu hình ảnh chính

                    using (var stream = new FileStream(ImagePath, FileMode.Create))         // Tạo file stream
                    {
                        MainImage.CopyTo(stream);       // Lưu file hình ảnh chính vào thư mục
                    }
                }
                _context.Add(product);      // Thêm sản phẩm vào cơ sở dữ liệu
                await _context.SaveChangesAsync();      // Lưu thay đổi
                if (GalleryImages != null)          // Kiểm tra xem có hình ảnh trong thư viện không
                {
                    foreach (var item in GalleryImages)         // Duyệt qua từng hình ảnh trong thư viện
                    {
                        var newgallery = new ProductGalery();       // Tạo mới hình ảnh trong thư viện
                        newgallery.ProductId = product.Id;          // Gán ID sản phẩm cho hình ảnh

                        newgallery.ImageName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(item.FileName);      // Tạo tên file ngẫu nhiên
                        string fn;      // Lấy đường dẫn hiện tại
                        fn = Directory.GetCurrentDirectory();       // Đường dẫn đến thư mục hiện tại
                        string ImagePath = fn + "\\wwwroot\\images\\banners\\" + newgallery.ImageName;      // Đường dẫn đến thư mục lưu hình ảnh trong thư viện

                        using (var stream = new FileStream(ImagePath, FileMode.Create))     // Tạo file stream
                        {
                            item.CopyTo(stream);        // Lưu file hình ảnh vào thư mục
                        }
                        _context.ProductGaleries.Add(newgallery);       // Thêm hình ảnh vào cơ sở dữ liệu
                    }
                }
                await _context.SaveChangesAsync();      // Lưu thay đổi

                return RedirectToAction(nameof(Index));     //  Quay lại danh sách sản phẩm
            }
            return View(product);           // Nếu không hợp lệ, quay lại trang tạo mới sản phẩm
        }

        // GET: Admin/Products/Edit/5
        public async Task<IActionResult> Edit(int? id)          // Chỉnh sửa sản phẩm
        {
            if (id == null)         // Kiểm tra xem ID có null không
            {
                return NotFound();      // Nếu null thì trả về NotFound
            }

            var product = await _context.Products.FindAsync(id);        // Tìm sản phẩm theo ID
            if (product == null)        // Nếu không tìm thấy sản phẩm
            {
                return NotFound();      // Nếu không tìm thấy sản phẩm
            }
            ViewData["gallery"] = _context.ProductGaleries.Where(x => x.ProductId == product.Id).ToList();      // Lấy danh sách hình ảnh của sản phẩm
            return View(product);       // Trả về view chỉnh sửa sản phẩm
        }

        // POST: Admin/Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,FullDesc,Price,Discount,ImageName,Qty,Tags,VideoUrl")] Product product, IFormFile? MainImage, IFormFile[]? GalleryImages)     // Chỉnh sửa sản phẩm
        {
            if (id != product.Id)       // Kiểm tra xem ID có khác với ID của sản phẩm không
            {
                return NotFound();      // Nếu khác thì trả về NotFound
            }

            if (ModelState.IsValid)     // Kiểm tra tính hợp lệ của dữ liệu
            {
                try
                {
                    if (MainImage != null)          // Kiểm tra xem có file hình ảnh chính không
                    {
                        string d = Directory.GetCurrentDirectory();         // Lấy đường dẫn hiện tại
                        string fn = d + "\\wwwroot\\images\\banners\\" + product.ImageName;     // Đường dẫn đến thư mục lưu hình ảnh chính

                        if (System.IO.File.Exists(fn))      // Kiểm tra xem file hình ảnh có tồn tại không
                        {
                            System.IO.File.Delete(fn);      // Nếu tồn tại thì xóa file hình ảnh
                        }    

                        using (var stream = new FileStream(fn, FileMode.Create))        // Tạo file stream
                        {
                            MainImage.CopyTo(stream);           // Lưu file hình ảnh chính vào thư mục
                        }    
                    }
                    if (GalleryImages != null)          // Kiểm tra xem có hình ảnh trong thư viện không
                    {
                        foreach (var item in GalleryImages)         // Duyệt qua từng hình ảnh trong thư viện
                        {
                            var imageName = Guid.NewGuid().ToString() + Path.GetExtension(item.FileName);       // Tạo tên file ngẫu nhiên

                            string d = Directory.GetCurrentDirectory();         // Lấy đường dẫn hiện tại
                            string fn = d + "\\wwwroot\\images\\banners\\" + imageName;         // Đường dẫn đến thư mục lưu hình ảnh trong thư viện

                            using (var stream = new FileStream(fn, FileMode.Create))            // Tạo file stream
                            {
                                item.CopyTo(stream);        // Lưu file hình ảnh vào thư mục
                            }
                            var galleryItem = new ProductGalery();          //  Tạo mới hình ảnh trong thư viện
                            galleryItem.ImageName = imageName;          // Gán tên file hình ảnh
                            galleryItem.ProductId = product.Id;         // Gán ID sản phẩm cho hình ảnh
                            _context.ProductGaleries.Add(galleryItem);          // Thêm hình ảnh vào cơ sở dữ liệu
                        }    
                    }

                    _context.Update(product);           // Cập nhật sản phẩm vào cơ sở dữ liệu
                    await _context.SaveChangesAsync();          // Lưu thay đổi
                }
                catch (DbUpdateConcurrencyException)        // Kiểm tra xem có lỗi không
                {
                    if (!ProductExists(product.Id))     // Kiểm tra xem sản phẩm có tồn tại không
                    {
                        return NotFound();      // Nếu không tồn tại thì trả về NotFound
                    }
                    else
                    {
                        throw;          // Ném lỗi ra ngoài
                    }
                }
                return RedirectToAction(nameof(Index));         // Quay lại danh sách sản phẩm
            }
            return View(product);           // Nếu không hợp lệ, quay lại trang chỉnh sửa sản phẩm
        }

        // GET: Admin/Products/Delete/5
        public async Task<IActionResult> Delete(int? id)        // Xóa sản phẩm
        {
            if (id == null)         // Kiểm tra xem ID có null không
            {
                return NotFound();          // Nếu null thì trả về NotFound
            }

            var product = await _context.Products           
                .FirstOrDefaultAsync(m => m.Id == id);          // Tìm sản phẩm theo ID
            if (product == null)            // Nếu không tìm thấy sản phẩm
            {
                return NotFound();          // Nếu không tìm thấy sản phẩm
            }

            return View(product);           // Trả về view xóa sản phẩm
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)            // Xóa sản phẩm
        {
            var product = await _context.Products.FindAsync(id);            // Tìm sản phẩm theo ID
            if (product != null)            // Nếu tìm thấy sản phẩm
            {
                string d = Directory.GetCurrentDirectory();         // Lấy đường dẫn hiện tại
                string fn = d + "\\wwwroot\\images\\banners\\";         // Đường dẫn đến thư mục lưu hình ảnh
                string mainImagePath = fn + product.ImageName;          // Đường dẫn đến thư mục lưu hình ảnh chính

                if (System.IO.Path.Exists(mainImagePath))       // Kiểm tra xem file hình ảnh chính có tồn tại không
                {
                    System.IO.File.Delete(mainImagePath);       // Nếu tồn tại thì xóa file hình ảnh chính
                }
                
                var galleries = _context.ProductGaleries.Where(x => x.ProductId == id).ToList();            // Tìm tất cả hình ảnh trong thư viện theo ID sản phẩm
                if (galleries != null)          // Nếu tìm thấy hình ảnh trong thư viện
                {
                    foreach (var item in galleries)         // Duyệt qua từng hình ảnh trong thư viện
                    {
                        string galleryImagePath = fn + item.ImageName;          // Đường dẫn đến thư mục lưu hình ảnh trong thư viện

                        if (System.IO.Path.Exists(galleryImagePath))        // Kiểm tra xem file hình ảnh có tồn tại không
                        {
                            System.IO.File.Delete(galleryImagePath);        // Nếu tồn tại thì xóa file hình ảnh
                        }
                    }
                    _context.ProductGaleries.RemoveRange(galleries);        // Xóa tất cả hình ảnh trong thư viện khỏi cơ sở dữ liệu
                }
                _context.Products.Remove(product);              // Xóa sản phẩm khỏi cơ sở dữ liệu
            }

            await _context.SaveChangesAsync();          // Lưu thay đổi
            return RedirectToAction(nameof(Index));         // Quay lại danh sách sản phẩm
        }

        private bool ProductExists(int id)          // Kiểm tra xem sản phẩm có tồn tại không
        {
            return _context.Products.Any(e => e.Id == id);          // Kiểm tra xem có sản phẩm nào có ID giống với ID truyền vào không
        }
    }
}
