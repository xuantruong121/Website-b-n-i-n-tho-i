using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_64131491.Data;
using QLBH_64131491.Models.Model_64131491;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace QLBH_64131491.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Order
        public async Task<IActionResult> Index()
        {
            var currentUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(currentUserId))
                return RedirectToAction("Login", "Account");

            var orders = await _context.Orders
                .Include(o => o.KhachHang)
                .Include(o => o.NhanVien)
                .Where(o => o.MaKH == currentUserId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: Order/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.KhachHang)
                .Include(o => o.NhanVien)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Smartphone)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Order/Admin
        [HttpGet]
        public async Task<IActionResult> Admin()
        {
            var orders = await _context.Orders
                .Include(o => o.KhachHang)
                .Include(o => o.NhanVien)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // POST: Order/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(string id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            // Validate status transition
            if (order.IsCancelled)
            {
                TempData["Error"] = "Không thể thay đổi trạng thái của đơn hàng đã hủy";
                return RedirectToAction(nameof(Admin));
            }

            // Set the new status
            switch (status.ToLower())
            {
                case "paid":
                    if (!order.IsProcessing && !order.IsShipped)
                    {
                        TempData["Error"] = "Chỉ có thể thanh toán đơn hàng đang xử lý hoặc đang giao";
                        return RedirectToAction(nameof(Admin));
                    }
                    order.IsPaid = true;
                    break;

                case "shipped":
                    if (!order.IsProcessing)
                    {
                        TempData["Error"] = "Chỉ có thể giao hàng khi đơn hàng đang xử lý";
                        return RedirectToAction(nameof(Admin));
                    }
                    order.IsShipped = true;
                    order.IsProcessing = false;
                    // Gán nhân viên xử lý
                    var maNV = HttpContext.Session.GetString("MaNV");
                    if (!string.IsNullOrEmpty(maNV))
                        order.MaNVXuLy = maNV;
                    break;

                case "processing":
                    if (order.IsCompleted || order.IsShipped)
                    {
                        TempData["Error"] = "Không thể chuyển đơn hàng đã hoàn thành hoặc đang giao về trạng thái xử lý";
                        return RedirectToAction(nameof(Admin));
                    }
                    order.IsProcessing = true;
                    order.IsShipped = false;
                    order.IsCompleted = false;
                    break;

                case "completed":
                    if (!order.IsShipped)
                    {
                        TempData["Error"] = "Chỉ có thể hoàn thành đơn hàng đang giao";
                        return RedirectToAction(nameof(Admin));
                    }
                    order.IsCompleted = true;
                    order.IsShipped = false;
                    order.IsProcessing = false;
                    order.IsPaid = true;
                    break;

                case "cancelled":
                    if (order.IsCompleted)
                    {
                        TempData["Error"] = "Không thể hủy đơn hàng đã hoàn thành";
                        return RedirectToAction(nameof(Admin));
                    }
                    order.IsCancelled = true;
                    order.IsProcessing = false;
                    order.IsShipped = false;
                    order.IsCompleted = false;
                    break;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật trạng thái đơn hàng thành công";
            return RedirectToAction(nameof(Admin));
        }

        // GET: Order/Statistics
        public async Task<IActionResult> Statistics()
        {
            // Lấy toàn bộ đơn hàng, không loại bỏ đơn đã hủy
            var orders = await _context.Orders.ToListAsync();

            var orderDetails = await _context.OrderDetails
                .Include(od => od.Smartphone)
                .Where(od => od.SmartphoneId != null && od.Smartphone != null)
                .ToListAsync();

            var statistics = new OrderStatistics_64131491
            {
                TotalOrders = orders.Count,
                TotalRevenue = orders.Where(o => o.IsCompleted).Sum(o => o.FinalAmount ?? 0),
                PendingOrders = orders.Count(o => o.IsProcessing && !o.IsCompleted && !o.IsCancelled),
                ShippedOrders = orders.Count(o => o.IsShipped && !o.IsCompleted && !o.IsCancelled),
                CompletedOrders = orders.Count(o => o.IsCompleted),
                CancelledOrders = orders.Count(o => o.IsCancelled),
                CashPaymentCount = orders.Count(o => {
                    var pm = (o.PaymentMethod ?? "").Trim().ToLower();
                    return pm == "cod" || pm == "tiền mặt" || pm == "cash";
                }),
                BankTransferCount = orders.Count(o => {
                    var pm = (o.PaymentMethod ?? "").Trim().ToLower();
                    return pm == "bank" || pm == "chuyển khoản";
                }),
                CreditCardCount = orders.Count(o => {
                    var pm = (o.PaymentMethod ?? "").Trim().ToLower();
                    return pm == "thẻ tín dụng" || pm == "credit";
                }),
                MonthlyRevenue = orders
                    .Where(o => o.IsCompleted)
                    .GroupBy(o => o.OrderDate.ToString("MM/yyyy"))
                    .OrderBy(g => g.Key)
                    .ToDictionary(g => g.Key, g => g.Sum(o => o.FinalAmount ?? 0)),
                TopSellingProducts = orderDetails
                    .GroupBy(od => od.SmartphoneId)
                    .Select(g => new OrderStatistics_64131491.TopSellingProduct
                    {
                        Brand = g.First().Smartphone.Brand,
                        Model = g.First().Smartphone.Model,
                        TotalSold = g.Sum(od => od.Quantity),
                        TotalRevenue = g.Sum(od => od.UnitPrice * od.Quantity)
                    })
                    .OrderByDescending(x => x.TotalSold)
                    .Take(5)
                    .ToList()
            };

            return View(statistics);
        }

        // GET: Order/AdminDetails/5
        public async Task<IActionResult> AdminDetails(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.KhachHang)
                .Include(o => o.NhanVien)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Smartphone)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Order/CancelOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(string orderId, string cancelReason)
        {
            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(cancelReason))
            {
                TempData["Error"] = "Vui lòng nhập lý do hủy đơn hàng";
                return RedirectToAction(nameof(Details), new { id = orderId });
            }

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound();
            }

            // Kiểm tra trạng thái đơn hàng
            if (order.IsCompleted)
            {
                TempData["Error"] = "Không thể hủy đơn hàng đã hoàn thành";
                return RedirectToAction(nameof(Details), new { id = orderId });
            }

            if (order.IsCancelled)
            {
                TempData["Error"] = "Đơn hàng đã được hủy trước đó";
                return RedirectToAction(nameof(Details), new { id = orderId });
            }

            // Cập nhật trạng thái đơn hàng
            order.IsCancelled = true;
            order.IsProcessing = false;
            order.IsShipped = false;
            order.IsCompleted = false;
            order.Notes = $"Lý do hủy: {cancelReason}";

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Hủy đơn hàng thành công";
            }
            catch (Exception)
            {
                TempData["Error"] = "Có lỗi xảy ra khi hủy đơn hàng";
            }

            return RedirectToAction(nameof(Details), new { id = orderId });
        }
    }
} 