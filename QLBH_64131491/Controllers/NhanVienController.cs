using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_64131491.Data;
using QLBH_64131491.Models.Model_64131491;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QLBH_64131491.Controllers
{
    public class NhanVienController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NhanVienController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: NhanVien/Index
        public async Task<IActionResult> Index()
        {
            var nhanViens = await _context.NhanViens.ToListAsync();
            return View(nhanViens);
        }

        // GET: NhanVien/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
                return NotFound();
            var nv = await _context.NhanViens.FirstOrDefaultAsync(n => n.MaNV == id);
            if (nv == null)
                return NotFound();
            return View(nv);
        }

        // GET: NhanVien/Create
        public IActionResult Create()
        {
            // Khởi tạo mặc định các giá trị
            var model = new NhanVien_64131491 
            { 
                QuyenSuDung = "Nhân viên bán hàng" 
            };
            return View(model);
        }

        // POST: NhanVien/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaNV,HoTen,SoDienThoai,Email,QuyenSuDung")] NhanVien_64131491 nv, string MatKhau, bool RoleAdmin, bool IsActive)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Kiểm tra Email đã tồn tại trong hệ thống chưa
                    var existingEmail = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.Email == nv.Email);
                    if (existingEmail != null)
                    {
                        ModelState.AddModelError("Email", "Email này đã tồn tại trong hệ thống!");
                        return View(nv);
                    }

                    // Kiểm tra mã nhân viên đã tồn tại chưa
                    var existingNV = await _context.NhanViens.FirstOrDefaultAsync(n => n.MaNV == nv.MaNV);
                    if (existingNV != null)
                    {
                        ModelState.AddModelError("MaNV", "Mã nhân viên đã tồn tại!");
                        return View(nv);
                    }

                    // Kiểm tra mật khẩu
                    if (string.IsNullOrEmpty(MatKhau) || MatKhau.Length < 6)
                    {
                        ModelState.AddModelError("MatKhau", "Mật khẩu phải có ít nhất 6 ký tự!");
                        return View(nv);
                    }

                    // Tạo tài khoản mới
                    var taiKhoan = new TaiKhoan_64131491
                    {
                        Email = nv.Email,
                        MatKhau = MatKhau,
                        RoleAdmin = RoleAdmin,
                        IsActive = IsActive,
                        IsVerified = true
                    };

                    // Thêm tài khoản vào DB
                    _context.TaiKhoans.Add(taiKhoan);
                    
                    // Thêm nhân viên vào DB
                    _context.NhanViens.Add(nv);
                    
                    // Lưu thay đổi
                    await _context.SaveChangesAsync();
                    
                    // Thông báo thành công
                    TempData["Toast_Success"] = "Thêm nhân viên mới thành công!";
                    return RedirectToAction(nameof(Index));
                }
                return View(nv);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi thêm nhân viên: " + ex.Message);
                TempData["Toast_Error"] = "Lỗi khi thêm nhân viên: " + ex.Message;
                return View(nv);
            }
        }

        // GET: NhanVien/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
                return NotFound();
            var nv = await _context.NhanViens.FindAsync(id);
            if (nv == null)
                return NotFound();
            return View(nv);
        }

        // POST: NhanVien/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaNV,HoTen,SoDienThoai,Email,QuyenSuDung")] NhanVien_64131491 nv)
        {
            if (id != nv.MaNV)
                return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nv);
                    await _context.SaveChangesAsync();
                    TempData["Toast_Success"] = "Cập nhật thông tin nhân viên thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.NhanViens.Any(e => e.MaNV == id))
                        return NotFound();
                    else
                        throw;
                }
            }
            return View(nv);
        }

        // GET: NhanVien/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
                return NotFound();
            var nv = await _context.NhanViens.FindAsync(id);
            if (nv == null)
                return NotFound();
            return View(nv);
        }

        // POST: NhanVien/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var nv = await _context.NhanViens.FindAsync(id);
            if (nv != null)
            {
                // Lưu email để xóa tài khoản sau
                string email = nv.Email;
                
                // Xóa nhân viên
                _context.NhanViens.Remove(nv);
                await _context.SaveChangesAsync();
                
                // Kiểm tra và xóa tài khoản nếu không còn liên kết
                var taiKhoan = await _context.TaiKhoans.FindAsync(email);
                var khachHangCount = await _context.KhachHangs.CountAsync(k => k.Email == email);
                
                if (taiKhoan != null && khachHangCount == 0)
                {
                    _context.TaiKhoans.Remove(taiKhoan);
                    await _context.SaveChangesAsync();
                }
                
                TempData["Toast_Success"] = "Xóa nhân viên thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
} 