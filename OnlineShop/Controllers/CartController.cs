using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OnlineShop.Models.Db;
using OnlineShop.Models;
using OnlineShop.Models.ViewModels;
using PayPal.Api;
using System.Security.Claims;


namespace OnlineShop.Controllers
{
    public class CartController : Controller
    {
        private OnlineShopContext _context;                     // Để truy cập vào cơ sở dữ liệu
                                                                // thông qua Entity Framework Core

        private IHttpContextAccessor _httpContextAccessor;      // Để truy cập vào HttpContext
                                                                // và lấy thông tin từ cookie

        public IConfiguration _configuration;                   // Để lấy thông tin cấu hình từ appsettings.json
        public CartController(OnlineShopContext context, IHttpContextAccessor httpContextAccessor, IConfiguration iconfiguration)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = iconfiguration;
            _context = context;
        }

        // Hiển thị trang giỏ hàng với danh sách sản phẩm trong giỏ hàng
        public IActionResult Index()            // Hiển thị giỏ hàng    
        {
            var result = GetProductsinCart();       // Lấy danh sách sản phẩm trong giỏ hàng
            return View(result);                //  Trả về danh sách sản phẩm trong giỏ hàng
        }

        // Xóa cookie chứa thông tin giỏ hàng và chuyển hướng về trang giỏ hàng.
        public IActionResult ClearCart()        // Xóa giỏ hàng
        {
            Response.Cookies.Delete("Cart");        // Xóa cookie giỏ hàng
            return Redirect("/");                   // Chuyển hướng về trang giỏ hàng
        }

        // Cập nhật sản phẩm trong giỏ hàng
        [HttpPost]
        public IActionResult UpdateCart([FromBody] CartViewModel request)       // Cập nhật sản phẩm trong giỏ hàng
        {
            // Kiểm tra sản phẩm tồn tại
            var product = _context.Products.FirstOrDefault(x => x.Id == request.ProductId);     // Lấy sản phẩm trong cơ sở dữ liệu
            if (product == null)                        
            {
                return NotFound();                              // Trả về không tìm thấy sản phẩm
            }

            // Kiểm tra số lượng đặt hàng không vượt quá tồn kho
            if (product.Qty < request.Count)            
            {
                // Nếu có thì hiển thị thông báo hết số lượng sản phẩm
                return BadRequest();
            }    

            // Truy xuất danh sách Sản phẩm (Product) trong Giỏ (Cart) 
            var cartItems = GetCartItems();

            var foundProductInCart = cartItems.FirstOrDefault(x => x.ProductId == request.ProductId);   // Tìm sản phẩm trong giỏ hàng

            // // Xử lý thêm mới hoặc cập nhật số lượng sản phẩm
            if (foundProductInCart == null)
            {
                // Thêm sản phẩm mới vào giỏ
                var newCartItem = new CartViewModel() { };      // Tạo một đối tượng giỏ hàng mới
                newCartItem.ProductId = request.ProductId;          // Lấy ID sản phẩm
                newCartItem.Count = request.Count;                  // Lấy số lượng sản phẩm

                cartItems.Add(newCartItem);                         // Thêm sản phẩm mới vào giỏ hàng
            }
            else
            {
                
                // Kiểm tra số lượng sản phầm trong giỏ hàng
                if (request.Count > 0)
                {
                    // Nếu lớn hơn 0, điều này có nghĩa là người dùng muốn cập nhật số lượng trong giỏ hàng (Cart)
                    foundProductInCart.Count = request.Count;
                }
                else
                {
                    // nếu không nó sẽ bị loại trừ từ giỏ hàng (Cart)
                    cartItems.Remove(foundProductInCart);
                }
            }

            var json = JsonConvert.SerializeObject(cartItems);      // Chuyển đổi danh sách giỏ hàng thành chuỗi JSON

            CookieOptions option = new CookieOptions();             // Tạo tùy chọn cookie
            option.Expires = DateTime.Now.AddDays(7);               // Đặt thời gian hết hạn của cookie là 7 ngày
            Response.Cookies.Append("Cart", json, option);          // Lưu giỏ hàng vào cookie

            // Trả về tổng số sản phẩm trong giỏ
            var result = cartItems.Sum(x => x.Count);

            return new JsonResult(result);
        }

        // Phương thức này đọc cookie "Cart" và chuyển đổi từ chuỗi JSON thành danh sách các đối tượng CartViewModel
        public List<CartViewModel> GetCartItems()       // Lấy danh sách sản phẩm trong giỏ hàng trống
        {
            List<CartViewModel> cartList = new List<CartViewModel>();

            var prevCartItemsString = Request.Cookies["Cart"];

            // Nếu nó không null, điều này có nghĩa là trong Giỏ hàng (Cart) có sản phẩm (Product),
            // Nếu không có thì trả về danh sách giỏ hàng (Cart) trống 
            if (!string.IsNullOrEmpty(prevCartItemsString))
            {
                cartList = JsonConvert.DeserializeObject<List<CartViewModel>>(prevCartItemsString);

            }
            return cartList;
        }

        // Phương thức này lấy chi tiết thông tin sản phẩm từ database dựa trên ID sản phẩm trong giỏ hàng
        // và tính toán giá trị của từng sản phẩm
        public List<ProductCartViewModel> GetProductsinCart()       // Lấy danh sách sản phẩm trong giỏ hàng
        {
            // Kiểm tra trong giỏ hàng có sản phẩm không 
            var cartItems = GetCartItems();
            if (!cartItems.Any())
            {
                // Nếu không có sản phảm trong giỏ hàng thì trả về không
                return null;
            }

            // Lấy sản phẩm từ database
            var cartItemProductIds = cartItems.Select(x => x.ProductId).ToList();

            var products = _context.Products
                .Where(p => cartItemProductIds.Contains(p.Id))
                .ToList();

            // Tạo danh sách sản phẩm trong giỏ hàng với thông tin đầy đủ
            List<ProductCartViewModel> result = new List<ProductCartViewModel>();
            foreach (var item in products)
            {
                var newItem = new ProductCartViewModel
                {
                    Id = item.Id,
                    ImageName = item.ImageName,
                    Price = item.Price - (item.Discount ?? 0),
                    Title = item.Title,
                    Count = cartItems.Single(x => x.ProductId == item.Id).Count,
                    RowSumPrice = (item.Price - (item.Discount ?? 0)) * cartItems.Single(x => x.ProductId == item.Id).Count
                };
                result.Add(newItem);
            }
            // Trả về danh sách sản phẩm trong giỏ hàng với thông tin đầy đủ
            return result;
        }

        // Trả về một PartialView (1 phần góc nhìn) hiển thị giỏ hàng nhỏ (thường hiển thị ở header).
        public IActionResult SmallCart()        // Hiển thị giỏ hàng nhỏ
        {
            // Hiển thị lại những sản phẩm được đặt trong giỏ hàng nhỏ
            var result = GetProductsinCart();
            return PartialView(result);
        }

        // Hiển thị trang thanh toán, yêu cầu người dùng đã đăng nhập.Lấy phí vận chuyển từ cài đặt hệ thống
        [Authorize]
        public IActionResult Checkout()     // Chuyển hướng đến trang thanh toán
        {
            // Lấy đơn hàng và shipping trong cơ sở dữ liệu
            var order = new Models.Db.Order();

            var shipping = _context.Settings.First().Shipping;

            // Kiểm tra shipping có tồn tại không
            if (shipping != null)
            {
                // Nếu có phí shipping thì thêm vào đơn hàng
                order.Shipping = shipping;
            }

            // Chuyển qua trang giao diện thanh toán
            ViewData["Products"] = GetProductsinCart();
            return View(order);
        }

        // Áp dụng mã giảm giá vào đơn hàng khi thanh toán
        [Authorize]
        [HttpPost]
        public IActionResult ApplyCouponCode([FromForm] string couponCode)      // Xử lý mã giảm giá
        {
            // Lấy dữ liệu đơn hàng và phiếu giảm giá trong cơ sở dữ liệu
            var order = new Models.Db.Order();

            var coupon = _context.Coupons.FirstOrDefault(c => c.Code == couponCode);

            
            if (coupon != null)                                         // Kiểm tra phiếu giảm giá có trong cơ sở dữ liệu không
            {
                                                                        // Nếu có phiếu giảm giá trong cơ sở dữ liệu 
                order.CouponCode = coupon.Code;                         // Thêm mã phiếu giảm giá vào đơn hàng trong cơ sở dữ liệu
                order.CouponDiscount = coupon.Discount;                 // Thêm phí giảm giá vào đơn hàng trong cơ sở dữ liệu
            }
            else                                                        // Nếu không có phiếu giảm giá trong cơ sở dữ liệu
            {
                ViewData["Products"] = GetProductsinCart();         // Lấy sản phẩm trong giỏ hàng
                TempData["message"] = "Coupon not exist";           // Thông báo không có phí giảm giá
                return View("Checkout", order);                     // Hiển thị trang giao diện thanh toán Checkout
            }

            
            var shipping = _context.Settings.First().Shipping;      // Lấy shipping trong cơ sở dữ liệu
            if (shipping != null)                                   // Kiểm tra shipping có tồn tại trong cơ sở dữ liệu không
            {
                                                                    // Nếu có phí shipping
                order.Shipping = shipping;                          // Thêm phí shipping vào đơn hàng trong cơ sở dữ liệu
            }

            
            ViewData["Products"] = GetProductsinCart();             // Lấy sản phẩm trong giỏ hàng
            return View("Checkout", order);                         // HIển thị lại trang giao diện thanh toán Checkout
        }

        
        [Authorize]                     // Chức năng này chỉ dành cho người dùng đã đăng nhập dưới dạng khách hàng
        [HttpPost]                      // Tạo dữ liệu và xử lý yêu cầu HTTP POST 
        public IActionResult Checkout(Models.Db.Order order)        // Xử lý việc người dùng xác nhận thanh toán
        {
            
            if (!ModelState.IsValid)                                // Kiểm tra trạng thái cơ sở dữ liệu
            { 
                                                                    // Nếu trạng thái không hợp lệ
                ViewData["Products"] = GetProductsinCart();         // lấy danh sách sản phẩm trong giỏ hàng
                return View(order);
            }

            if (!string.IsNullOrEmpty(order.CouponCode))            // Kiểm tra có mã phiếu giảm giá không
            {
                                                                    // Nếu có mã phiếu giảm giá
                var coupon = _context.Coupons.FirstOrDefault(c => c.Code == order.CouponCode);      // Lấy phiếu giảm giá từ cơ sở dữ liệu

                if (coupon != null)                                 // Kiểm tra có phiếu giảm giá trong cơ sở dữ liệu không
                {
                                                                    // Nếu có phiếu giảm giá trong cơ sở dữ liệu
                    order.CouponCode = coupon.Code;                         // Thêm mã phiếu giảm giá vào đơn hàng
                    order.CouponDiscount = coupon.Discount;                 // Thêm phí giảm giá vào đơn hàng
                }
                else                                                // Nếu không có phiếu giảm giá trong cơ sở dữ liệu
                {
                    
                    TempData["message"] = "Coupon not exist";               // Thông báo không có phí giảm giá
                    TempData["Products"] = GetProductsinCart();             // Lấy sản phẩm trong giỏ hàng
                    return View(order);
                }
            }

            var products = GetProductsinCart();                         // Lấy sản phẩm trong giỏ hàng

            order.Shipping = _context.Settings.First().Shipping;                            // Thêm Shipping vào đơn hàng
            order.CreateDate = DateTime.Now;                                                // Thêm ngày tạo đơn hàng
            order.SubTotal = products.Sum(x => x.RowSumPrice);                              // Thêm tiền từng sản phẩm vào đơn hàng
            order.Total = (order.SubTotal + order.Shipping ?? 0);                           // Thêm tổng tiền vào đơn hàng
            order.UserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));       // Thêm ID người dùng vào đơn hàng


            if (order.CouponDiscount != null)                           // Kiểm tra phí giảm giá có trong cơ sở dữ liệu không
            {
                order.Total -= order.CouponDiscount;                    // Nếu có phí giảm giá thì trừ vào tổng tiền
            }

            _context.Orders.Add(order);                                 // Cập nhật đơn hàng trong cơ sở dữ liệu
            _context.SaveChanges();                                     // Lưu trạng thái đơn hàng trong cơ sở dữ liệu

            
            List<OrderDetail> orderDetails = new List<OrderDetail>();           // Lấy chi tiết sản phẩm từ cơ sở dữ liệu

            
            foreach (var item in products)                                  // Lấy từng sản phẩm trong giỏ hàng
            {
                
                OrderDetail orderDetailItem = new OrderDetail()                 // Lấy chi tiết sản phẩm từ cơ sở dữ liệu
                {
                    Count = item.Count,                                         // Thêm số lượng sản phẩm trong giỏ hàng vào
                                                                                // chi tiết sản phẩm trong cơ sở dữ liệu

                    ProductTitle = item.Title,                                  // Thêm tiêu đề sản phẩm trong giỏ hàng vào 
                                                                                // chi tiết sản phẩm trong cơ sở dữ liệu

                    ProductPrice = (decimal)item.Price,                         // Thêm giá sản phẩm trong giỏ hàng vào
                                                                                // chi tiết sản phẩm trong cơ sở dữ liệu
                                                                                
                    OrderId = order.Id,                                         // Thêm ID đơn hàng vào
                                                                                // chi tiết sản phẩm trong cơ sở dữ liệu

                    ProductId = item.Id                                         // Thêm ID sản phẩm vào
                                                                                // chi tiết sản phẩm trong cơ sở dữ liệu
                };

                
                orderDetails.Add(orderDetailItem);                          // Thêm chi tiết đơn hàng vào cơ sở dữ liệu
            }

            
            _context.OrderDetails.AddRange(orderDetails);                       // Cập nhật chi tiết đơn hàng trong cơ sở dữ liệu 
            _context.SaveChanges();                                             // Lưu trạng thái chi tiết đơn hàng cơ sở dữ liệu

            
            return Redirect("/Cart/RedirectToPayPal?orderId=" + order.Id);          // Hiển thị trang giao diện Paypal
        }

        // Phương thức này tạo đối tượng thanh toán PayPal và chuyển hướng người dùng đến trang thanh toán của PayPal
        public ActionResult RedirectToPayPal(int orderId)       // Chuyển hướng đến trang thanh toán của PayPal
        {
            
            var order = _context.Orders.Find(orderId);
            if (order == null)              // Kiểm tra đơn hàng có tồn tại trong cơ sở dữ liệu không
            {                                   // Nếu không có đơn hàng trong cơ sở dữ liệu
                return View("PaymentFailed");           // Hiển thị trang giao diện Thanh toán thất bại
            }

            var orderDetails = _context.OrderDetails.Where(x => x.OrderId == orderId).ToList();     // Lấy chi tiết đơn hàng trong cơ sở dữ liệu

            var clientId = _configuration.GetValue<string>("PayPal:Key");                   // Lấy key của Paypal
            var clientSecret = _configuration.GetValue<string>("PayPal:Secret");            // Lấy Secret của Paypal
            var mode = _configuration.GetValue<string>("PayPal:mode");                          // Lấy mode của Paypal
            var apiContext = PaypalConfiguration.GetAPIContext(clientId, clientSecret, mode);           // Lấy cấu hình API của Paypal

            try
            {
                string baseURI = $"{Request.Scheme}://{Request.Host}/Cart/PaypalReturn?";   // URL chuyển hướng đến
                                                                                            // trang xác nhận thanh toán
                var guid = Guid.NewGuid().ToString();                                   // Tạo GUID ngẫu nhiên

                var payment = new Payment       // Tạo đối tượng thanh toán cho Paypal
                {
                    intent = "sale",
                    payer = new Payer { payment_method = "paypal" },
                    transactions = new List<Transaction>                // Tạo giao dịch thanh toán cho Paypal
                    {
                        new Transaction                                 
                        {
                            description = $"Order {order.Id}",              // Mô tả đơn hàng
                            invoice_number = Guid.NewGuid().ToString(),     // Mã hóa đơn
                            amount = new Amount                         // Tạo số lượng thanh toán hóa đơn cho Paypal
                            {
                                currency = "USD",                                   // Tiền tệ
                                total = order.Total?.ToString("F"),                 //  Tổng tiền
                            },

                            item_list = new ItemList                    // danh sách chi tiết đơn hàng cho Paypal
                            {
                                items = orderDetails.Select(p => new Item       // Tạo danh sách sản phẩm trong giỏ hàng
                                {
                                    name = p.ProductTitle,          // Tên sản phẩm
                                    currency = "USD",               // Tiền tệ
                                    price = p.ProductPrice.ToString("F"),   // Giá sản phẩm
                                    quantity = p.Count.ToString(),      // Số lượng sản phẩm
                                    sku = p.ProductId.ToString(),       // Mã sản phẩm
                                }).ToList(),                            // Chuyển đổi danh sách sản phẩm thành danh sách
                                                                            // các đối tượng Item trong Paypal
                            },
                        }
                    },
                    redirect_urls = new RedirectUrls        // URL chuyển hướng đến trang xác nhận thanh toán
                    {
                        cancel_url = $"{baseURI}&Cancel=true",          // URL hủy bỏ thanh toán
                        return_url = $"{baseURI}&orderId={order.Id}"    //  URL trở về sau khi thanh toán
                    }
                };

                payment.transactions[0].item_list.items.Add(new Item    // Thêm đơn hàng vào giao dịch trong Paypal
                {
                    name = "Shipping cost",                     // Tên sản phẩm
                    currency = "USD",                           // Tiền tệ
                    price = order.Shipping?.ToString("F"),      // Giá sản phẩm
                    quantity = "1",                             // Số lượng sản phẩm
                    sku = "1",                                      // Mã sản phẩm
                });

                var createdPayment = payment.Create(apiContext);        // Tạo giao dịch thanh toán chứa
                                                                        // thông tin xác thực Paypal về clientID, clientSecret
                var approvalUrl = createdPayment.links.FirstOrDefault(l => l.rel.ToLower() == "approval_url")?.href;    // URL mà người dùng cần được
                                                                                                                        // chuyển hướng đến để
                                                                                                                        // xác nhận thanh toán trên
                                                                                                                        // trang của PayPal.

                _httpContextAccessor.HttpContext.Session.SetString("payment", createdPayment.id);   // Lưu ID của giao dịch PayPal vào
                                                                                                    // Session của ứng dụng
                return Redirect(approvalUrl);                   // Hiển thị trang giao diện Paypal
            }
            catch (Exception)               // Nếu có lỗi xảy ra trong quá trình tạo giao dịch thanh toán
            {
                return View("PaymentFailed");           // Hiển thị trang giao diện Thanh toán thất bại
            }
        }

        public ActionResult PaypalReturn(int orderId, string PayerID)       // Xử lý khi người dùng quay lại từ trang thanh toán của PayPal
        {
            var order = _context.Orders.Find(orderId);          // Lấy đơn hàng trong cơ sở dữ liệu
            if (order == null)                                  // Kiểm tra đơn hàng có tồn tại trong cơ sở dữ liệu không
            {                                                   // Nếu không có đơn hàng trong cơ sở dữ liệu
                return View("PaymentFailed");               // Hiển thị trang giao diện Thanh toán thất bại
            }

            var clientId = _configuration.GetValue<string>("PayPal:Key");               // Lấy key của Paypal
            var clientSecret = _configuration.GetValue<string>("PayPal:Secret");        // Lấy secret của Paypal
            var mode = _configuration.GetValue<string>("PayPal:mode");                  // Lấy mode của Paypal
            var apiContext = PaypalConfiguration.GetAPIContext(clientId, clientSecret, mode);       // Lấy cấu hình API của Paypal

            try
            {
                var paymentId = _httpContextAccessor.HttpContext.Session.GetString("payment");      // Lấy ID giao dịch trong Session
                var paymentExecution = new PaymentExecution { payer_id = PayerID };             // Tạo đối tượng thanh toán
                var payment = new Payment { id = paymentId };                                          // Tạo đối tượng thanh toán

                var executedPayment = payment.Execute(apiContext, paymentExecution);            // Thực hiện giao dịch thanh toán

                if (executedPayment.state.ToLower() != "approved")                              // Kiểm tra trạng thái giao dịch thanh toán
                {                                                                               // Nếu không được phê duyệt
                    return View("PaymentFailed");                                                   // Hiển thị trang giao diện Thanh toán thất bại
                }

                Response.Cookies.Delete("Cart");                                                // Xóa cookie giỏ hàng

                order.TransId = executedPayment.transactions[0].related_resources[0].sale.id;       // Lưu ID giao dịch vào đơn hàng
                order.Status = executedPayment.state.ToLower();                                     // Lưu trạng thái giao dịch vào đơn hàng

                var orderDetails = _context.OrderDetails.Where(x => x.OrderId == orderId).ToList();     // Lấy chi tiết đơn hàng
                                                                                                        // trong cơ sở dữ liệu

                var productsIds = orderDetails.Select(x => x.ProductId);                            // Lấy ID sản phẩm trong giỏ hàng
                                                                                                    // trong cơ sở dữ liệu

                var products = _context.Products.Where(x => productsIds.Contains(x.Id)).ToList();       // Lấy sản phẩm trong giỏ hàng
                                                                                                        // trong cơ sở dữ liệu

                foreach (var item in products)                                                  // Lấy từng sản phẩm trong giỏ hàng
                {
                    item.Qty = item.Qty - orderDetails.FirstOrDefault(x => x.ProductId == item.Id).Count;   // Cập nhật số lượng
                                                                                                            // sản phẩm trong cơ sở dữ liệu
                }
                _context.Products.UpdateRange(products);                                        // Cập nhật sản phẩm
                                                                                                // trong cơ sở dữ liệu

                _context.SaveChanges();                                                         // Lưu trạng thái đơn hàng
                                                                                                // trong cơ sở dữ liệu

                ViewData["orderId"] = order.Id;                                             //  Lưu ID đơn hàng vào ViewData
                return View("PaymentSuccess");                                              // Hiển thị trang giao diện
                                                                                            // Thanh toán thành công
            }
            catch (Exception)                                                               // Nếu có lỗi xảy ra trong
                                                                                            // quá trình thanh toán
            {
                return View("PaymentFailed");                                               // Hiển thị trang giao diện
                                                                                            // Thanh toán thất bại

            }
        }
    }
}
