using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_64131491.Data;
using QLBH_64131491.Models.Model_64131491;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using System.Collections.Generic;

namespace QLBH_64131491.Controllers
{
    public class SmartphoneController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SmartphoneController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Smartphone
        public async Task<IActionResult> Index()
        {
            var smartphones = await _context.Smartphones
                .Include(s => s.LoaiSanPham)
                .Where(s => s.IsActive)
                .ToListAsync();
            ViewBag.Brands = await _context.Smartphones.Where(s => s.IsActive).Select(s => s.Brand).Distinct().ToListAsync();
            return View(smartphones);
        }

        // GET: Smartphone/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var smartphone = await _context.Smartphones
                .Include(s => s.LoaiSanPham)
                .Include(s => s.ProductImages)
                .Include(s => s.ProductReviews.Where(r => r.IsApproved && !r.IsHidden))
                .FirstOrDefaultAsync(s => s.Id == id);

            if (smartphone == null)
            {
                return NotFound();
            }

            return View(smartphone);
        }

        // GET: Smartphone/Create
        [HttpGet]
        [Route("Smartphone/Create")]
        public IActionResult Create()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] GET Create called");
                var loaiSanPhams = _context.LoaiSanPhams.ToList();
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Found {loaiSanPhams.Count} LoaiSanPhams");
                ViewBag.LoaiSanPhams = loaiSanPhams;
                return View();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] GET Create Exception: {ex}");
                return View("Error");
            }
        }

        // POST: Smartphone/Create
        [HttpPost]
        [Route("Smartphone/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Brand,Model,Price,DiscountPrice,Description,Color,StorageGB,RAM,Processor,Camera,Battery,ScreenSize,OS,ReleaseDate,InStock,Quantity,Featured,IsNew,MaLSP")] Smartphone_64131491 smartphone, IFormFile images)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] POST Create called");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Model: Brand={smartphone.Brand}, Model={smartphone.Model}, Price={smartphone.Price}, MaLSP={smartphone.MaLSP}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Additional: DiscountPrice={smartphone.DiscountPrice}, Description={(smartphone.Description?.Substring(0, Math.Min(20, smartphone.Description?.Length ?? 0)) ?? "null")}...");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Checkboxes: InStock={smartphone.InStock}, Featured={smartphone.Featured}, IsNew={smartphone.IsNew}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Image: {(images != null ? $"Name={images.FileName}, Size={images.Length}" : "null")}");

                // Tải danh sách loại sản phẩm cho view
                ViewBag.LoaiSanPhams = _context.LoaiSanPhams.ToList();

                // Loại bỏ lỗi "Id không được để trống" vì ID sẽ được tự động tạo
                if (ModelState.ContainsKey("Id"))
                {
                    ModelState.Remove("Id");
                }

                // Kiểm tra hình ảnh
                if (images == null || images.Length == 0)
                {
                    ModelState.AddModelError("images", "Vui lòng chọn hình ảnh sản phẩm.");
                    TempData["FormError"] = "Vui lòng chọn hình ảnh sản phẩm.";
                    return View(smartphone);
                }

                // Kiểm tra các trường bắt buộc khác
                if (string.IsNullOrEmpty(smartphone.Brand))
                {
                    ModelState.AddModelError("Brand", "Vui lòng nhập tên thương hiệu.");
                    TempData["FormError"] = "Vui lòng nhập tên thương hiệu.";
                    return View(smartphone);
                }

                if (string.IsNullOrEmpty(smartphone.Model))
                {
                    ModelState.AddModelError("Model", "Vui lòng nhập tên Model.");
                    TempData["FormError"] = "Vui lòng nhập tên Model.";
                    return View(smartphone);
                }

                if (smartphone.Price <= 0)
                {
                    ModelState.AddModelError("Price", "Giá phải lớn hơn 0.");
                    TempData["FormError"] = "Giá phải lớn hơn 0.";
                    return View(smartphone);
                }

                if (string.IsNullOrEmpty(smartphone.MaLSP))
                {
                    ModelState.AddModelError("MaLSP", "Vui lòng chọn loại sản phẩm.");
                    TempData["FormError"] = "Vui lòng chọn loại sản phẩm.";
                    return View(smartphone);
                }

                if (!ModelState.IsValid)
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] ModelState is invalid");
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] ModelState Error: {error.ErrorMessage}");
                    }
                    
                    // Kiểm tra key nào trong ModelState gây lỗi
                    var errors = new List<string>();
                    foreach (var key in ModelState.Keys)
                    {
                        var entry = ModelState[key];
                        if (entry.Errors.Count > 0)
                        {
                            errors.Add(string.Join(", ", entry.Errors.Select(e => e.ErrorMessage)));
                            System.Diagnostics.Debug.WriteLine($"[DEBUG] Invalid key: {key}, Errors: {string.Join(", ", entry.Errors.Select(e => e.ErrorMessage))}");
                        }
                    }
                    
                    if (errors.Any())
                    {
                        TempData["FormError"] = $"Có lỗi khi nhập dữ liệu: {string.Join("; ", errors)}";
                    }
                    
                    return View(smartphone);
                }

                // Sinh mã sản phẩm tự động dạng SMxxx
                var lastId = await _context.Smartphones
                    .OrderByDescending(s => s.Id)
                    .Select(s => s.Id)
                    .FirstOrDefaultAsync();
                int nextNumber = 1;
                if (!string.IsNullOrEmpty(lastId) && lastId.StartsWith("SM"))
                {
                    if (int.TryParse(lastId.Substring(2), out nextNumber))
                    {
                        nextNumber++;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] Could not parse number from ID: {lastId}");
                    }
                }
                string newId = $"SM{nextNumber:D3}";
                smartphone.Id = newId;
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Generated new ID: {newId}");

                // Xử lý upload ảnh
                var wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var saveFolder = Path.Combine(wwwRootPath, "images", "smartphones");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Save folder path: {saveFolder}");
                
                if (!Directory.Exists(saveFolder))
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Creating directory: {saveFolder}");
                    Directory.CreateDirectory(saveFolder);
                }

                var fileName = Path.GetFileNameWithoutExtension(images.FileName);
                var ext = Path.GetExtension(images.FileName);
                var uniqueName = $"{fileName}_{Guid.NewGuid().ToString().Substring(0, 8)}{ext}";
                var filePath = Path.Combine(saveFolder, uniqueName);

                System.Diagnostics.Debug.WriteLine($"[DEBUG] Saving image to: {filePath}");

                try
                {
                    // Lưu file vào thư mục của dự án
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await images.CopyToAsync(stream);
                    }
                    System.Diagnostics.Debug.WriteLine("[DEBUG] Image saved successfully");

                    // Tạo bản sao trong thư mục gốc chạy lệnh dotnet watch run
                    var projectRoot = Directory.GetCurrentDirectory();
                    // Lên 1 cấp để đến thư mục gốc dự án (nơi chạy lệnh dotnet watch run)
                    var rootDirectory = Directory.GetParent(projectRoot)?.FullName ?? projectRoot;
                    
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Project root directory: {projectRoot}");
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Root directory for dotnet watch run: {rootDirectory}");
                    
                    var backupFilePath = Path.Combine(rootDirectory, uniqueName);
                    System.IO.File.Copy(filePath, backupFilePath, true);
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Image copy created at: {backupFilePath}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR] Image save failed: {ex.Message}");
                    TempData["FormError"] = $"Lỗi khi lưu hình ảnh: {ex.Message}";
                    return View(smartphone);
                }

                smartphone.ImageUrl = $"/images/smartphones/{uniqueName}";
                smartphone.IsActive = true;

                // Khởi tạo các collection
                smartphone.ProductImages = new List<ProductImage_64131491>();
                smartphone.ProductReviews = new List<ProductReview_64131491>();
                smartphone.OrderDetails = new List<OrderDetail_64131491>();
                smartphone.CartItems = new List<Cart_64131491>();

                System.Diagnostics.Debug.WriteLine("[DEBUG] Adding smartphone to context");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Smartphone to save: ID={smartphone.Id}, Brand={smartphone.Brand}, Model={smartphone.Model}");
                try
                {
                    _context.Add(smartphone);
                    await _context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine("[DEBUG] Smartphone saved to database");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR] Error saving smartphone: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"[ERROR] Inner exception: {ex.InnerException?.Message}");
                    TempData["FormError"] = $"Lỗi khi lưu sản phẩm: {ex.Message}";
                    return View(smartphone);
                }

                // Lưu vào ProductImages
                var productImage = new ProductImage_64131491
                {
                    SmartphoneId = smartphone.Id,
                    ImageUrl = smartphone.ImageUrl,
                    IsPrimary = true
                };
                System.Diagnostics.Debug.WriteLine("[DEBUG] Adding product image to context");
                try
                {
                    _context.ProductImages.Add(productImage);
                    await _context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine("[DEBUG] Product image saved to database");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR] Error saving product image: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"[ERROR] Inner exception: {ex.InnerException?.Message}");
                    TempData["FormError"] = $"Lỗi khi lưu thông tin hình ảnh: {ex.Message}";
                    return View(smartphone);
                }

                System.Diagnostics.Debug.WriteLine($"[DEBUG] Product saved successfully with ID: {smartphone.Id}");
                TempData["Toast_Success"] = "Thêm sản phẩm thành công!";
                System.Diagnostics.Debug.WriteLine("[DEBUG] Setting success toast and redirecting");
                return RedirectToAction("AdminIndex");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] POST Create Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                System.Diagnostics.Debug.WriteLine($"[ERROR] Inner exception: {ex.InnerException?.Message}");
                ModelState.AddModelError("", "Có lỗi xảy ra khi lưu sản phẩm: " + ex.Message);
                TempData["FormError"] = $"Có lỗi xảy ra: {ex.Message}";
                ViewBag.LoaiSanPhams = _context.LoaiSanPhams.ToList();
                return View(smartphone);
            }
        }

        // GET: Smartphone/Edit/5
        [HttpGet]
        [Route("Smartphone/Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var smartphone = await _context.Smartphones.FindAsync(id);
            if (smartphone == null)
            {
                return NotFound();
            }
            ViewBag.LoaiSanPhams = _context.LoaiSanPhams.ToList();
            return View(smartphone);
        }

        // POST: Smartphone/Edit/5
        [HttpPost]
        [Route("Smartphone/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Brand,Model,Price,DiscountPrice,Description,ImageUrl,Color,StorageGB,RAM,Processor,Camera,Battery,ScreenSize,OS,ReleaseDate,InStock,Quantity,Featured,IsNew,IsActive,MaLSP")] Smartphone_64131491 smartphone, IFormFile image)
        {
            if (id != smartphone.Id)
            {
                return NotFound();
            }

            ViewBag.LoaiSanPhams = _context.LoaiSanPhams.ToList();

            // Loại bỏ các lỗi validation liên quan đến hình ảnh
            ModelState.Remove("image");

            // Kiểm tra các điều kiện bắt buộc khác
            if (string.IsNullOrEmpty(smartphone.Brand))
            {
                ModelState.AddModelError("Brand", "Thương hiệu không được để trống");
            }
            
            if (string.IsNullOrEmpty(smartphone.Model))
            {
                ModelState.AddModelError("Model", "Model không được để trống");
            }
            
            if (smartphone.Price <= 0)
            {
                ModelState.AddModelError("Price", "Giá phải lớn hơn 0");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý upload ảnh nếu có
                    if (image != null && image.Length > 0)
                    {
                        // Xóa ảnh cũ (nếu có)
                        if (!string.IsNullOrEmpty(smartphone.ImageUrl))
                        {
                            try
                            {
                                string oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", smartphone.ImageUrl.TrimStart('/'));
                                if (System.IO.File.Exists(oldImagePath))
                                {
                                    System.IO.File.Delete(oldImagePath);
                                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Deleted old image: {oldImagePath}");
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"[WARNING] Could not delete old image: {ex.Message}");
                            }
                        }

                        // Lưu ảnh mới
                        var wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        var saveFolder = Path.Combine(wwwRootPath, "images", "smartphones");
                        
                        if (!Directory.Exists(saveFolder))
                        {
                            Directory.CreateDirectory(saveFolder);
                        }

                        var fileName = Path.GetFileNameWithoutExtension(image.FileName);
                        var ext = Path.GetExtension(image.FileName);
                        var uniqueName = $"{fileName}_{Guid.NewGuid().ToString().Substring(0, 8)}{ext}";
                        var filePath = Path.Combine(saveFolder, uniqueName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        // Tạo bản sao trong thư mục gốc chạy lệnh dotnet watch run
                        var projectRoot = Directory.GetCurrentDirectory();
                        var rootDirectory = Directory.GetParent(projectRoot)?.FullName ?? projectRoot;
                        var backupFilePath = Path.Combine(rootDirectory, uniqueName);
                        System.IO.File.Copy(filePath, backupFilePath, true);

                        // Cập nhật URL ảnh mới
                        smartphone.ImageUrl = $"/images/smartphones/{uniqueName}";
                        
                        // Cập nhật ProductImage nếu có
                        var productImage = await _context.ProductImages
                            .FirstOrDefaultAsync(p => p.SmartphoneId == smartphone.Id && p.IsPrimary);
                        
                        if (productImage != null)
                        {
                            productImage.ImageUrl = smartphone.ImageUrl;
                            _context.Update(productImage);
                        }
                    }

                    _context.Update(smartphone);
                    await _context.SaveChangesAsync();
                    TempData["Toast_Success"] = "Cập nhật sản phẩm thành công!";
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Smartphone updated successfully: {smartphone.Id}");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!SmartphoneExists(smartphone.Id))
                    {
                        System.Diagnostics.Debug.WriteLine($"[ERROR] Smartphone not found: {smartphone.Id}");
                        TempData["Toast_Error"] = "Không tìm thấy sản phẩm để cập nhật!";
                        return NotFound();
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[ERROR] Concurrency error: {ex.Message}");
                        TempData["Toast_Error"] = $"Lỗi khi cập nhật sản phẩm: {ex.Message}";
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR] Error updating smartphone: {ex.Message}");
                    TempData["Toast_Error"] = $"Lỗi khi cập nhật sản phẩm: {ex.Message}";
                    return View(smartphone);
                }
                return RedirectToAction("AdminIndex");
            }
            return View(smartphone);
        }

        // GET: Smartphone/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var smartphone = await _context.Smartphones
                .Include(s => s.LoaiSanPham)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (smartphone == null)
            {
                return NotFound();
            }

            return View(smartphone);
        }

        // POST: Smartphone/DeleteConfirmed/5
        [HttpPost]
        [ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Deleting smartphone with ID: {id}");
            
            try
            {
                // Tìm sản phẩm cần xóa
                var smartphone = await _context.Smartphones
                    .Include(s => s.ProductImages)
                    .Include(s => s.ProductReviews)
                    .Include(s => s.CartItems)
                    .Include(s => s.OrderDetails)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (smartphone != null)
                {
                    // Xóa các ảnh sản phẩm liên quan trong cơ sở dữ liệu
                    if (smartphone.ProductImages != null && smartphone.ProductImages.Any())
                    {
                        _context.ProductImages.RemoveRange(smartphone.ProductImages);
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] Removed {smartphone.ProductImages.Count} product images");
                    }

                    // Xóa các đánh giá sản phẩm liên quan
                    if (smartphone.ProductReviews != null && smartphone.ProductReviews.Any())
                    {
                        _context.ProductReviews.RemoveRange(smartphone.ProductReviews);
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] Removed {smartphone.ProductReviews.Count} product reviews");
                    }

                    // Xóa các mục giỏ hàng liên quan
                    if (smartphone.CartItems != null && smartphone.CartItems.Any())
                    {
                        _context.Cart.RemoveRange(smartphone.CartItems);
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] Removed {smartphone.CartItems.Count} cart items");
                    }
                    
                    // Xem xét có đơn hàng liên quan không
                    if (smartphone.OrderDetails != null && smartphone.OrderDetails.Any())
                    {
                        // Cảnh báo: Sản phẩm đã có trong đơn hàng
                        System.Diagnostics.Debug.WriteLine($"[WARNING] Smartphone has {smartphone.OrderDetails.Count} order details, marking as inactive instead");
                        TempData["Toast_Warning"] = $"Sản phẩm đã có trong {smartphone.OrderDetails.Count} đơn hàng nên không thể xóa hoàn toàn. Đã ẩn sản phẩm.";
                        
                        // Đánh dấu không hoạt động thay vì xóa
                        smartphone.IsActive = false;
                        await _context.SaveChangesAsync();
                        return RedirectToAction("AdminIndex");
                    }

                    // Xóa file ảnh sản phẩm từ thư mục wwwroot nếu có
                    if (!string.IsNullOrEmpty(smartphone.ImageUrl))
                    {
                        try
                        {
                            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", smartphone.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(filePath))
                            {
                                System.IO.File.Delete(filePath);
                                System.Diagnostics.Debug.WriteLine($"[DEBUG] Deleted image file: {filePath}");
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[WARNING] Could not delete image file: {ex.Message}");
                        }
                    }

                    // Xóa sản phẩm khỏi cơ sở dữ liệu
                    _context.Smartphones.Remove(smartphone);
                    await _context.SaveChangesAsync();

                    TempData["Toast_Success"] = "Xóa sản phẩm thành công!";
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Smartphone completely removed from database: {id}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Smartphone not found: {id}");
                    TempData["Toast_Error"] = "Không tìm thấy sản phẩm để xóa!";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Error deleting smartphone: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                TempData["Toast_Error"] = $"Lỗi khi xóa sản phẩm: {ex.Message}";
            }
            
            return RedirectToAction("AdminIndex");
        }

        private bool SmartphoneExists(string id)
        {
            return _context.Smartphones.Any(e => e.Id == id);
        }

        // GET: Smartphone/Search
        public async Task<IActionResult> Search(string searchString, string brand, decimal? minPrice, decimal? maxPrice)
        {
            var smartphones = _context.Smartphones
                .Include(s => s.LoaiSanPham)
                .Where(s => s.IsActive);

            if (!string.IsNullOrEmpty(searchString))
            {
                smartphones = smartphones.Where(s => s.Model.Contains(searchString) || s.Brand.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(brand))
            {
                smartphones = smartphones.Where(s => s.Brand == brand);
            }

            if (minPrice.HasValue)
            {
                smartphones = smartphones.Where(s => s.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                smartphones = smartphones.Where(s => s.Price <= maxPrice.Value);
            }

            ViewBag.Brands = await _context.Smartphones.Select(s => s.Brand).Distinct().ToListAsync();
            ViewBag.SearchString = searchString;
            ViewBag.SelectedBrand = brand;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            return View("Index", await smartphones.ToListAsync());
        }

        // GET: Smartphone/AdminIndex
        public async Task<IActionResult> AdminIndex()
        {
            var smartphones = await _context.Smartphones
                .Include(s => s.LoaiSanPham)
                .ToListAsync();
            return View("AdminIndex", smartphones);
        }

        // GET: Smartphone/AdminDetails/5
        public async Task<IActionResult> AdminDetails(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var smartphone = await _context.Smartphones
                .Include(s => s.LoaiSanPham)
                .Include(s => s.ProductImages)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (smartphone == null)
            {
                return NotFound();
            }

            return View("AdminDetails", smartphone);
        }
    }
} 