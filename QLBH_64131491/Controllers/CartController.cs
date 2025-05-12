using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_64131491.Data;
using QLBH_64131491.Models.Model_64131491;
using System.Linq;
using System.Threading.Tasks;

namespace QLBH_64131491.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Cart
        public async Task<IActionResult> Index()
        {
            var currentUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(currentUserId))
                return RedirectToAction("Login", "Account");

            var cartItems = await _context.Cart
                .Include(c => c.Smartphone)
                .Where(c => c.MaKH == currentUserId)
                .ToListAsync();

            return View(cartItems);
        }

        // POST: Cart/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(string smartphoneId, int quantity)
        {
            if (string.IsNullOrEmpty(smartphoneId) || quantity <= 0)
            {
                Console.WriteLine($"[AddToCart] BadRequest: smartphoneId='{smartphoneId}', quantity={quantity}");
                return BadRequest("Thiếu mã sản phẩm hoặc số lượng không hợp lệ");
            }

            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("[AddToCart] BadRequest: Chưa đăng nhập hoặc session hết hạn");
                return StatusCode(401, "Bạn chưa đăng nhập");
            }

            try
            {
                var existingCartItem = await _context.Cart
                    .FirstOrDefaultAsync(c => c.MaKH == userId && c.SmartphoneId == smartphoneId);

                if (existingCartItem != null)
                {
                    existingCartItem.Quantity += quantity;
                }
                else
                {
                    var newCartItem = new Cart_64131491
                    {
                        MaKH = userId,
                        SmartphoneId = smartphoneId,
                        Quantity = quantity,
                        DateAdded = System.DateTime.Now
                    };
                    _context.Cart.Add(newCartItem);
                }

                await _context.SaveChangesAsync();
                var cartCount = await _context.Cart.Where(c => c.MaKH == userId).SumAsync(c => c.Quantity);
                Console.WriteLine($"[AddToCart] Success: userId={userId}, smartphoneId={smartphoneId}, cartCount={cartCount}");
                return Json(new { success = true, cartCount });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AddToCart] Exception: {ex.Message}");
                return StatusCode(500, "Lỗi server khi thêm vào giỏ hàng");
            }
        }

        // POST: Cart/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartId, int quantity)
        {
            var cartItem = await _context.Cart.FindAsync(cartId);
            if (cartItem == null)
            {
                return NotFound();
            }

            if (quantity <= 0)
            {
                _context.Cart.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity = quantity;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            var cartItem = await _context.Cart.FindAsync(id);
            if (cartItem == null)
            {
                return NotFound();
            }

            _context.Cart.Remove(cartItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Cart/Checkout
        public async Task<IActionResult> Checkout()
        {
            var currentUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(currentUserId))
                return RedirectToAction("Login", "Account");

            var cartItems = await _context.Cart
                .Include(c => c.Smartphone)
                .Where(c => c.MaKH == currentUserId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            // Lấy thông tin khách hàng
            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(kh => kh.MaKH == currentUserId);

            var order = new Order_64131491
            {
                OrderId = "ORD" + System.DateTime.Now.ToString("yyyyMMddHHmmss"),
                MaKH = currentUserId,
                OrderDate = System.DateTime.Now,
                IsProcessing = true,
                TotalAmount = cartItems.Sum(c => c.Smartphone.Price * c.Quantity),
                FinalAmount = cartItems.Sum(c => (c.Smartphone.DiscountPrice ?? c.Smartphone.Price) * c.Quantity),
                KhachHang = khachHang,
                ShippingAddress = khachHang?.DiaChi
            };

            return View(order);
        }

        // POST: Cart/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout([Bind("ShippingAddress,PaymentMethod,Notes")] Order_64131491 order)
        {
            var currentUserId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(currentUserId))
                return RedirectToAction("Login", "Account");

            var cartItems = await _context.Cart
                .Include(c => c.Smartphone)
                .Where(c => c.MaKH == currentUserId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction(nameof(Index));
            }

            // Tự động sinh OrderId và gán các trường cần thiết
            order.OrderId = "ORD" + System.DateTime.Now.ToString("yyyyMMddHHmmssfff");
            order.MaKH = currentUserId;
            order.OrderDate = System.DateTime.Now;
            order.IsProcessing = true;
            order.IsPaid = false;
            order.IsShipped = false;
            order.IsCompleted = false;
            order.IsCancelled = false;
            order.TotalAmount = cartItems.Sum(c => c.Smartphone.Price * c.Quantity);
            order.FinalAmount = cartItems.Sum(c => (c.Smartphone.DiscountPrice ?? c.Smartphone.Price) * c.Quantity);
            order.Discount = (order.TotalAmount ?? 0) - (order.FinalAmount ?? 0);

            if (string.IsNullOrWhiteSpace(order.ShippingAddress) || string.IsNullOrWhiteSpace(order.PaymentMethod))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ địa chỉ giao hàng và chọn phương thức thanh toán.";
                return View(order);
            }

            _context.Orders.Add(order);

            foreach (var cartItem in cartItems)
            {
                var orderDetail = new OrderDetail_64131491
                {
                    OrderId = order.OrderId,
                    SmartphoneId = cartItem.SmartphoneId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.Smartphone.Price,
                    Discount = cartItem.Smartphone.Price - (cartItem.Smartphone.DiscountPrice ?? cartItem.Smartphone.Price)
                };
                _context.OrderDetails.Add(orderDetail);

                // Update product quantity
                cartItem.Smartphone.Quantity -= cartItem.Quantity;
            }

            // Clear the cart
            _context.Cart.RemoveRange(cartItems);

            await _context.SaveChangesAsync();
            TempData["Success"] = "Đặt hàng thành công! Cảm ơn bạn đã mua sắm.";
            return RedirectToAction("Details", "Order", new { id = order.OrderId });
        }

        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            var userId = HttpContext.Session.GetString("UserId");
            int cartCount = 0;
            if (!string.IsNullOrEmpty(userId))
            {
                cartCount = await _context.Cart.Where(c => c.MaKH == userId).SumAsync(c => c.Quantity);
            }
            return Json(new { cartCount });
        }
    }
} 