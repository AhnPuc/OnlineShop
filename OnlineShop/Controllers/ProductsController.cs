using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Models.Db;
using System.Text.RegularExpressions;

namespace OnlineShop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly OnlineShopContext _context;

        public ProductsController(OnlineShopContext context)
        {
            _context = context;
        }

        public IActionResult Index()        // Trang danh sách sản phẩm
        {
            List<Product> products = _context.Products.OrderByDescending(x => x.Id).ToList();   // Lấy danh sách sản phẩm từ
                                                                                                // cơ sở dữ liệu
            return View(products);                                  // Trả về view danh sách sản phẩm
        }

        public IActionResult SearchProducts(string SearchText)      // Tìm kiếm sản phẩm
        {
            var products = _context.Products            // Lấy danh sách sản phẩm từ cơ sở dữ liệu
                .Where(x =>
                EF.Functions.Like(x.Title, "%" + SearchText + "%") ||       // Tìm kiếm theo tiêu đề
                EF.Functions.Like(x.Tags, "%" + SearchText + "%")           // Tìm kiếm theo thẻ
                )
                .OrderBy(x => x.Title)              // Sắp xếp theo tiêu đề
                .ToList();
            return View("Index", products);         // Trả về view danh sách sản phẩm
        }

        public IActionResult ProductDetails(int id)     // Chi tiết sản phẩm
        {
            Product? product = _context.Products.FirstOrDefault(x => x.Id == id);       // Lấy sản phẩm theo id
            if (product == null)            
            {
                return NotFound();      // Nếu không tìm thấy sản phẩm
            }
            ViewData["gallery"] = _context.ProductGaleries.Where(x => x.ProductId == id).ToList();      // Lấy danh sách hình ảnh của
                                                                                                        // sản phẩm

            ViewData["NewProducts"] = _context.Products.Where(x => x.Id != id).                 
                                        Take(6).OrderByDescending(x => x.Id).ToList();  // Lấy 6 sản phẩm mới nhất

            ViewData["comments"] = _context.Comments.Where(x => x.ProductId == id).
                                        OrderByDescending(x => x.CreateDate).ToList();  // Lấy danh sách bình luận của sản phẩm

            if (!string.IsNullOrEmpty(product.Tags))
            {
                
                var productTags = product.Tags.Split(',').Select(t => t.Trim().ToLower()).ToList();     // Chia tách
                                                                                                        // các thẻ của sản phẩm

                var recommendedProducts = _context.Products
                    .Where(p => p.Id != id && !string.IsNullOrEmpty(p.Tags))        //  Lọc các sản phẩm khác
                    .AsEnumerable() //  Chuyển đổi sang IEnumerable để sử dụng LINQ to Objects
                    .Select(p => new        
                    {
                        Product = p,            //  Lưu sản phẩm
                        MatchedTags = p.Tags.Split(',')     //  Chia tách các thẻ của sản phẩm
                            .Select(t => t.Trim().ToLower())        //  Chuyển đổi các thẻ thành chữ thường
                            .Intersect(productTags).Count()     //  Đếm số thẻ trùng khớp
                    })
                    .Where(x => x.MatchedTags > 0)  // Lọc các sản phẩm có thẻ trùng khớp
                    .OrderByDescending(x => x.MatchedTags) // Sắp xếp theo số lượng thẻ trùng khớp
                    .ThenByDescending(x => x.Product.Id) // Sắp xếp theo id sản phẩm
                    .Take(4)        // Lấy 4 sản phẩm
                    .Select(x => x.Product)     // Chọn sản phẩm
                    .ToList();      //  Trả về danh sách sản phẩm

                ViewData["RecommendedProducts"] = recommendedProducts;      // Lưu danh sách sản phẩm gợi ý vào ViewData
            }
            return View(product);       // Trả về view chi tiết sản phẩm
        }

        [HttpPost]
        public IActionResult SubmitComment(string name, string email, string comment, int productID)        // Gửi bình luận
        {
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(comment) && productID != 0)            // Kiểm tra thông tin bình luận
            {
                Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");    // Biểu thức chính quy để kiểm tra định dạng email
                Match match = regex.Match(email);   // Kiểm tra định dạng email
                if (!match.Success)
                {       
                    TempData["ErrorMessage"] = "Email is not valid";        // Nếu email không hợp lệ
                    return Redirect("/Products/ProductDetails/" + productID);   // Trả về trang chi tiết sản phẩm
                }
                
                Comment newComment = new Comment();     // Tạo mới bình luận
                newComment.Name = name;                 // Lưu tên người bình luận
                newComment.Email = email;               // Lưu email người bình luận
                newComment.CommentText = comment;       // Lưu nội dung bình luận
                newComment.ProductId = productID;       // Lưu id sản phẩm
                newComment.CreateDate = DateTime.Now;       // Lưu ngày tạo bình luận

                _context.Comments.Add(newComment);      // Thêm bình luận vào cơ sở dữ liệu
                _context.SaveChanges();                     // Lưu thay đổi

                TempData["SuccessMessage"] = "Your comment submited success fully";         // Thông báo thành công
                return Redirect("/Products/ProductDetails/" + productID);       // Trả về trang chi tiết sản phẩm
            }
            else
            {       // Nếu thông tin không hợp lệ
                TempData["ErrorMessage"] = "Please complete your information";      // Thông báo yêu cầu hoàn thành thông tin
                return Redirect("/Products/ProductDetails/" + productID);       // Trả về trang chi tiết sản phẩm
            }    
        }
    }
}
