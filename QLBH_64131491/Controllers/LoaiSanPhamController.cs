using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_64131491.Data;
using QLBH_64131491.Models.Model_64131491;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace QLBH_64131491.Controllers
{
    public class LoaiSanPhamController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoaiSanPhamController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: LoaiSanPham/Index
        public async Task<IActionResult> Index()
        {
            var loais = await _context.LoaiSanPhams.ToListAsync();
            return View(loais);
        }

        // GET: LoaiSanPham/Create
        public IActionResult Create()
        {
            return View(new LoaiSanPham_64131491 { Smartphones = new List<Smartphone_64131491>() });
        }

        // POST: LoaiSanPham/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaLSP,TenLSP")] LoaiSanPham_64131491 loai)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Khởi tạo collection để tránh lỗi null reference
                    loai.Smartphones = new List<Smartphone_64131491>();
                    
                    // Kiểm tra mã loại sản phẩm đã tồn tại chưa
                    var existingLoai = await _context.LoaiSanPhams.FirstOrDefaultAsync(l => l.MaLSP == loai.MaLSP);
                    if (existingLoai != null)
                    {
                        ModelState.AddModelError("MaLSP", "Mã loại sản phẩm đã tồn tại!");
                        return View(loai);
                    }

                    _context.Add(loai);
                    await _context.SaveChangesAsync();
                    TempData["Toast_Success"] = "Thêm loại sản phẩm thành công!";
                    return RedirectToAction("AdminIndex", "Smartphone");
                }
                return View(loai);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi thêm loại sản phẩm: " + ex.Message);
                TempData["Toast_Error"] = "Lỗi khi thêm loại sản phẩm: " + ex.Message;
                return View(loai);
            }
        }

        // GET: LoaiSanPham/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
                return NotFound();
            var loai = await _context.LoaiSanPhams.FindAsync(id);
            if (loai == null)
                return NotFound();
            return View(loai);
        }

        // POST: LoaiSanPham/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("MaLSP,TenLSP")] LoaiSanPham_64131491 loai)
        {
            if (id != loai.MaLSP)
                return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(loai);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.LoaiSanPhams.Any(e => e.MaLSP == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(loai);
        }

        // GET: LoaiSanPham/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
                return NotFound();
            var loai = await _context.LoaiSanPhams.FindAsync(id);
            if (loai == null)
                return NotFound();
            return View(loai);
        }

        // POST: LoaiSanPham/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var loai = await _context.LoaiSanPhams.FindAsync(id);
            if (loai != null)
            {
                // Kiểm tra ràng buộc sản phẩm trước khi xóa
                bool hasProducts = _context.Smartphones.Any(s => s.MaLSP == id && s.IsActive);
                if (hasProducts)
                {
                    ModelState.AddModelError("", "Không thể xóa loại sản phẩm đang có sản phẩm!");
                    return View(loai);
                }
                _context.LoaiSanPhams.Remove(loai);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
} 