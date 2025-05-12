using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_64131491.Data;
using QLBH_64131491.Models.Model_64131491;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using QLBH_64131491.Models.ViewModels;

namespace QLBH_64131491.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                TempData["Error"] = "Email và mật khẩu không được để trống";
                return View();
            }

            // So sánh mật khẩu plain text
            var taiKhoan = await _context.TaiKhoans
                .FirstOrDefaultAsync(t => t.Email == email && t.MatKhau == password);

            if (taiKhoan == null)
            {
                TempData["Error"] = "Email hoặc mật khẩu không đúng";
                return View();
            }

            if (!taiKhoan.IsActive)
            {
                TempData["Error"] = "Tài khoản đã bị khóa";
                return View();
            }

            // Lấy MaKH từ KhachHangs
            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(kh => kh.Email == email);
            if (khachHang != null)
            {
                HttpContext.Session.SetString("UserId", khachHang.MaKH);
            }
            else
            {
                HttpContext.Session.SetString("UserId", taiKhoan.Email); // fallback cho admin
            }
            HttpContext.Session.SetString("IsAdmin", taiKhoan.RoleAdmin.ToString());

            // Bổ sung lưu MaNV vào session nếu là admin hoặc có liên kết nhân viên
            var nhanVien = await _context.NhanViens.FirstOrDefaultAsync(nv => nv.Email == email);
            if (nhanVien != null)
            {
                HttpContext.Session.SetString("MaNV", nhanVien.MaNV);
            }
            else
            {
                HttpContext.Session.Remove("MaNV");
            }

            if (taiKhoan.RoleAdmin)
            {
                TempData["Success"] = "Đăng nhập thành công!";
                return RedirectToAction("Index", "AdminHome");
            }
            else
            {
                TempData["Success"] = "Đăng nhập thành công!";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _context.TaiKhoans.AnyAsync(t => t.Email == model.Email))
                {
                    TempData["Toast_Danger"] = "Email đã tồn tại";
                    return View(model);
                }

                var taiKhoan = new TaiKhoan_64131491
                {
                    Email = model.Email,
                    MatKhau = model.MatKhau,
                    RoleAdmin = false,
                    IsActive = true,
                    IsVerified = false
                };

                var khachHang = new KhachHang_64131491
                {
                    MaKH = "KH" + (await _context.KhachHangs.CountAsync() + 1).ToString("D3"),
                    HoTen = model.HoTen,
                    Email = model.Email,
                    SoDienThoai = model.SoDienThoai,
                    DiaChi = model.DiaChi
                };

                _context.TaiKhoans.Add(taiKhoan);
                _context.KhachHangs.Add(khachHang);
                await _context.SaveChangesAsync();

                TempData["Toast_Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            TempData["Toast_Danger"] = "Vui lòng kiểm tra lại thông tin đăng ký.";
            return View(model);
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            
            // Sử dụng key Info thay vì Success để có màu khác, tránh nhầm với thông báo đăng nhập
            TempData["Info"] = "Đã đăng xuất thành công!";
            
            return RedirectToAction("Login", "Account");
        }

        // GET: Account/Profile
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            var khachHang = await _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .FirstOrDefaultAsync(k => k.MaKH == userId);

            if (khachHang == null)
            {
                return NotFound();
            }

            return View(khachHang);
        }

        // POST: Account/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile([Bind("MaKH,HoTen,SoDienThoai,DiaChi")] KhachHang_64131491 khachHang)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(khachHang);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KhachHangExists(khachHang.MaKH))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Profile));
            }
            return View("Profile", khachHang);
        }

        // POST: Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            var taiKhoan = await _context.TaiKhoans.FindAsync(userId);
            if (taiKhoan == null)
            {
                TempData["Error"] = "Không tìm thấy tài khoản!";
                return RedirectToAction("Profile");
            }

            // So sánh mật khẩu plain text
            if (taiKhoan.MatKhau != currentPassword)
            {
                TempData["Error"] = "Mật khẩu hiện tại không đúng";
                return RedirectToAction("Profile");
            }

            taiKhoan.MatKhau = newPassword;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đổi mật khẩu thành công!";
            return RedirectToAction(nameof(Profile));
        }

        private bool KhachHangExists(string id)
        {
            return _context.KhachHangs.Any(e => e.MaKH == id);
        }
    }
} 