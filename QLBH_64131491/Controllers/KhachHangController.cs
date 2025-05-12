using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_64131491.Data;
using QLBH_64131491.Models.Model_64131491;
using System.Linq;
using System.Threading.Tasks;

namespace QLBH_64131491.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KhachHangController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: KhachHang/Index
        public async Task<IActionResult> Index()
        {
            var khachHangs = await _context.KhachHangs.Include(k => k.TaiKhoan).ToListAsync();
            return View(khachHangs);
        }

        // GET: KhachHang/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
                return NotFound();
            var khachHang = await _context.KhachHangs.Include(k => k.TaiKhoan).FirstOrDefaultAsync(k => k.MaKH == id);
            if (khachHang == null)
                return NotFound();
            return View(khachHang);
        }

        // GET: KhachHang/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
                return NotFound();
            var khachHang = await _context.KhachHangs.Include(k => k.TaiKhoan).FirstOrDefaultAsync(k => k.MaKH == id);
            if (khachHang == null)
                return NotFound();
            return View(khachHang);
        }

        // POST: KhachHang/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaKH,HoTen,SoDienThoai,DiaChi,Email")] KhachHang_64131491 khachHang)
        {
            if (id != khachHang.MaKH)
                return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(khachHang);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.KhachHangs.Any(e => e.MaKH == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(khachHang);
        }

        // POST: KhachHang/Lock/5
        [HttpPost]
        public async Task<IActionResult> Lock(string id)
        {
            var khachHang = await _context.KhachHangs.Include(k => k.TaiKhoan).FirstOrDefaultAsync(k => k.MaKH == id);
            if (khachHang == null || khachHang.TaiKhoan == null)
                return NotFound();
            khachHang.TaiKhoan.IsActive = false;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: KhachHang/Unlock/5
        [HttpPost]
        public async Task<IActionResult> Unlock(string id)
        {
            var khachHang = await _context.KhachHangs.Include(k => k.TaiKhoan).FirstOrDefaultAsync(k => k.MaKH == id);
            if (khachHang == null || khachHang.TaiKhoan == null)
                return NotFound();
            khachHang.TaiKhoan.IsActive = true;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
} 