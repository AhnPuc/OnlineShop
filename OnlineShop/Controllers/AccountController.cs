using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Models.Db;
using OnlineShop.Models.ViewModels;
using System.Net.Mail;
using System.Security.Claims;
using System.Text.RegularExpressions;


namespace OnlineShop.Controllers
{
    public class AccountController : Controller
    {
        private OnlineShopContext _context;

        public AccountController(OnlineShopContext context)
        {
            _context = context;
        }
        public IActionResult Register()     // Đăng ký người dùng
        {
            return View();              // Trả về view đăng ký
        }

        [HttpPost]
        public IActionResult Register(User user)        // Đăng ký người dùng
        {
            user.RegisterDate = DateTime.Now;           // Lưu ngày đăng ký
            user.IsAdmin = false;                       // Lưu vai trò người dùng
            user.Email = user.Email?.Trim();            // Lưu email người dùng
            user.Password = user.Password?.Trim();      // Lưu mật khẩu người dùng
            user.FullName = user.FullName?.Trim();      // Lưu tên người dùng
            user.RecoveryCode = 0;

            if (!ModelState.IsValid)                    // Kiểm tra tính hợp lệ của dữ liệu
            {
                return View(user);                      // Trả về view đăng ký với dữ liệu đã nhập
            }

            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");    // Biểu thức chính quy để kiểm tra định dạng email
            Match match = regex.Match(user.Email);                          // Kiểm tra định dạng email
            if (!match.Success)             // Nếu email không hợp lệ
            {
                ModelState.AddModelError("Email", "Email is not valid");    // Thêm lỗi vào ModelState
                return View(user);              // Trả về view đăng ký với dữ liệu đã nhập
            }

            var prevUser = _context.Users.Any(x => x.Email == user.Email);      // Kiểm tra xem email đã tồn tại trong cơ sở dữ liệu chưa
            if (prevUser == true)                                               // Nếu email đã tồn tại
            {
                ModelState.AddModelError("Email", "Email exists");          // Thêm lỗi vào ModelState
                return View(user);                                          // Trả về view đăng ký với dữ liệu đã nhập
            }

            _context.Users.Add(user);                                       // Thêm người dùng vào cơ sở dữ liệu
            _context.SaveChanges();                                             // Lưu thay đổi

            return RedirectToAction("login");                               // Chuyển hướng đến trang đăng nhập
        }

        public IActionResult Login()        // Đăng nhập                                   
        {
            return View();                                                  // Trả về view đăng nhập
        }

        [HttpPost]      // Đăng nhập
        public IActionResult Login(LoginViewModel user)     // Đăng nhập
        {
            if (!ModelState.IsValid)                                    // Kiểm tra tính hợp lệ của dữ liệu
            {
                return View(user);                                      // Trả về view đăng nhập với dữ liệu đã nhập
            }

            var foundUser = _context.Users.FirstOrDefault(x => x.Email == user.Email.Trim() && x.Password == user.Password.Trim());     // Tìm người dùng trong cơ sở dữ liệu

            if (foundUser == null)                                      // Nếu không tìm thấy người dùng
            {
                ModelState.AddModelError("Email", "Email or Password is not valid");        // Thêm lỗi vào ModelState
                return View(user);                                                  // Trả về view đăng nhập với dữ liệu đã nhập
            }

            var claims = new List<Claim>();         // Tạo danh sách các claim cho người dùng
            claims.Add(new Claim(ClaimTypes.NameIdentifier, foundUser.Id.ToString()));      // Thêm claim Id người dùng
            claims.Add(new Claim(ClaimTypes.Name, foundUser.FullName));     // Thêm claim tên người dùng
            claims.Add(new Claim(ClaimTypes.Email, foundUser.Email));       // Thêm claim email người dùng


            // Thêm kiểm tra null trước khi truy cập
            if (foundUser.IsAdmin == true)          // Nếu người dùng là admin
            {
                claims.Add(new Claim(ClaimTypes.Role, "admin"));    // Thêm claim vai trò admin
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, "user"));     // Thêm claim vai trò user
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);   // Tạo đối tượng ClaimsIdentity từ danh sách các claim

            var principal = new ClaimsPrincipal(identity);  // Tạo đối tượng ClaimsPrincipal từ ClaimsIdentity

            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);      // Đăng nhập người dùng

            return Redirect("/");       // Chuyển hướng đến trang chủ
        }

        [Authorize]     // Chỉ cho phép người dùng đã đăng nhập truy cập

        public IActionResult Logout()               // Đăng xuất 
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);    // Đăng xuất người dùng
            return RedirectToAction("Login", "Account");    // Chuyển hướng đến trang đăng nhập
        }

        /// Đăng ký lại mật khẩu
        public IActionResult RecoveryPassword()     // Đăng ký lại mật khẩu
        {
            return View();  // Trả về view đăng ký lại mật khẩu
        }

        [HttpPost]
        public IActionResult RecoveryPassword(RecoveryPasswordViewModel recoveryPassword)   // Đăng ký lại mật khẩu 
        {
            if (!ModelState.IsValid)    //  Kiểm tra tính hợp lệ của dữ liệu
            {
                return View();      // Trả về view đăng ký lại mật khẩu
            }

            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");    // Biểu thức chính quy để kiểm tra định dạng email
            Match match = regex.Match(recoveryPassword.Email);      // Kiểm tra định dạng email
            if (!match.Success)     // Nếu email không hợp lệ
            {
                ModelState.AddModelError("Email", "Email is not valid");        // Thêm lỗi vào ModelState
                return View(recoveryPassword);          // Trả về view đăng ký lại mật khẩu
            }

            var foundUser = _context.Users.FirstOrDefault(x => x.Email == recoveryPassword.Email.Trim());       // Tìm người dùng trong cơ sở dữ liệu
            if (foundUser == null)      // Nếu không tìm thấy người dùng
            {
                ModelState.AddModelError("Email", "Email not exist");       // Thêm lỗi vào ModelState
                return View(recoveryPassword);          // Trả về view đăng ký lại mật khẩu
            }

            foundUser.RecoveryCode = new Random().Next(10000, 100000);          // Tạo mã khôi phục ngẫu nhiên
            _context.Users.Update(foundUser);           // Cập nhật người dùng trong cơ sở dữ liệu
            _context.SaveChanges();                 // Lưu thay đổi

            MailMessage mail = new MailMessage();       // Tạo đối tượng MailMessage
            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");       // Tạo đối tượng SmtpClient

            mail.From = new MailAddress("longde0912@gmail.com");        // Địa chỉ email người gửi
            mail.To.Add(foundUser.Email);                   // Địa chỉ email người nhận
            mail.Subject = "Recovery code";                 // Tiêu đề email
            mail.Body = "Your recovery code: " + foundUser.RecoveryCode;        // Nội dung email

            SmtpServer.Port = 587;          // Cổng SMTP
            SmtpServer.Credentials = new System.Net.NetworkCredential("longde0912@gmail.com", "qxgx qsji otla ixvb");   // Tài khoản và mật khẩu email người gửi
            SmtpServer.EnableSsl = true;        // Bật SSL

            SmtpServer.Send(mail);      // Gửi email

            return Redirect("/Account/ResetPassword?email=" + foundUser.Email);     // Chuyển hướng đến trang đặt lại mật khẩu
        }

        public IActionResult ResetPassword(string Email)    // Đặt lại mật khẩu
        {
            var resetPasswordModel = new ResetPasswordViewModel();      // Tạo đối tượng ResetPasswordViewModel
            resetPasswordModel.Email = Email;               // Lưu email người dùng

            return View(resetPasswordModel);                    // Trả về view đặt lại mật khẩu
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel resetPassword)        // Đặt lại mật khẩu
        {
            if (!ModelState.IsValid)        // Kiểm tra tính hợp lệ của dữ liệu
            {
                return View(resetPassword);     // Trả về view đặt lại mật khẩu
            }

            var foundUser = _context.Users.FirstOrDefault(x => x.Email == resetPassword.Email.Trim() && x.RecoveryCode == resetPassword.RecoveryCode);      // Tìm người dùng trong cơ sở dữ liệu
            if (foundUser == null)      // Nếu không tìm thấy người dùng
            {
                ModelState.AddModelError("RecoveryCode", "Email or Recovery code is not valid");        // Thêm lỗi vào ModelState
                return View(resetPassword);     // Trả về view đặt lại mật khẩu
            }

            foundUser.Password = resetPassword.NewPassword;     // Đặt lại mật khẩu người dùng
            _context.Users.Update(foundUser);       // Cập nhật người dùng trong cơ sở dữ liệu
            _context.SaveChanges();         // Lưu thay đổi

            return RedirectToAction("Login");       // Chuyển hướng đến trang đăng nhập
        }

    }
}
