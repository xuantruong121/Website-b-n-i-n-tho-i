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

            // Reload smartphone data to ensure all properties are properly loaded
            foreach (var item in cartItems)
            {
                if (item.Smartphone != null)
                {
                    // Explicitly load the full smartphone data from the database
                    item.Smartphone = await _context.Smartphones.FindAsync(item.SmartphoneId);
                }
            }

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
                // Fetch the smartphone with full details to ensure prices are correct
                var smartphone = await _context.Smartphones
                    .FirstOrDefaultAsync(s => s.Id == smartphoneId);
                
                if (smartphone == null)
                {
                    return NotFound("Sản phẩm không tồn tại");
                }
                
                if (quantity > smartphone.Quantity)
                {
                    return BadRequest($"Số lượng yêu cầu vượt quá số lượng trong kho ({smartphone.Quantity})");
                }
                
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
            var cartItem = await _context.Cart
                .Include(c => c.Smartphone)
                .FirstOrDefaultAsync(c => c.CartId == cartId);

            if (cartItem == null)
            {
                return NotFound();
            }

            // Reload smartphone data to ensure we have current inventory information
            var smartphone = await _context.Smartphones.FindAsync(cartItem.SmartphoneId);
            if (smartphone == null)
            {
                return NotFound("Sản phẩm không tồn tại");
            }

            // Check if the requested quantity is available
            if (quantity > smartphone.Quantity)
            {
                TempData["Error"] = $"Số lượng yêu cầu ({quantity}) vượt quá số lượng trong kho ({smartphone.Quantity})";
                return RedirectToAction(nameof(Index));
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

            // Reload smartphone data to ensure all properties are properly loaded
            foreach (var item in cartItems)
            {
                if (item.Smartphone != null)
                {
                    // Explicitly load the full smartphone data from the database
                    item.Smartphone = await _context.Smartphones.FindAsync(item.SmartphoneId);
                }
            }

            // Lấy thông tin khách hàng
            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(kh => kh.MaKH == currentUserId);

            // Calculate correct amounts based on the fixed pricing logic
            decimal totalOriginalAmount = 0;
            decimal totalFinalAmount = 0;
            
            foreach (var item in cartItems)
            {
                var originalPrice = item.Smartphone.Price;
                decimal finalPrice;
                
                // Only use discount price if it is valid (greater than 0 and less than original price)
                if (item.Smartphone.DiscountPrice.HasValue && 
                    item.Smartphone.DiscountPrice.Value > 0 && 
                    item.Smartphone.DiscountPrice.Value < originalPrice)
                {
                    finalPrice = item.Smartphone.DiscountPrice.Value;
                }
                else
                {
                    finalPrice = originalPrice;
                }
                
                totalOriginalAmount += originalPrice * item.Quantity;
                totalFinalAmount += finalPrice * item.Quantity;
            }

            var order = new Order_64131491
            {
                OrderId = "ORD" + System.DateTime.Now.ToString("yyyyMMddHHmmss"),
                MaKH = currentUserId,
                OrderDate = System.DateTime.Now,
                IsProcessing = true,
                TotalAmount = totalOriginalAmount,
                FinalAmount = totalFinalAmount,
                Discount = totalOriginalAmount - totalFinalAmount,
                KhachHang = khachHang,
                ShippingAddress = khachHang?.DiaChi
            };

            // Pass cart items to the view
            ViewBag.CartItems = cartItems;

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
            
            // Calculate total amounts correctly
            decimal totalOriginalAmount = 0;
            decimal totalFinalAmount = 0;
            
            foreach (var item in cartItems)
            {
                var originalPrice = item.Smartphone.Price;
                decimal finalPrice;
                
                // Only use discount price if it is valid (greater than 0 and less than original price)
                if (item.Smartphone.DiscountPrice.HasValue && 
                    item.Smartphone.DiscountPrice.Value > 0 && 
                    item.Smartphone.DiscountPrice.Value < originalPrice)
                {
                    finalPrice = item.Smartphone.DiscountPrice.Value;
                }
                else
                {
                    finalPrice = originalPrice;
                }
                
                totalOriginalAmount += originalPrice * item.Quantity;
                totalFinalAmount += finalPrice * item.Quantity;
            }
            
            // Assign nullable decimal values
            order.TotalAmount = totalOriginalAmount;
            order.FinalAmount = totalFinalAmount;
            order.Discount = totalOriginalAmount - totalFinalAmount;

            if (string.IsNullOrWhiteSpace(order.ShippingAddress) || string.IsNullOrWhiteSpace(order.PaymentMethod))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ địa chỉ giao hàng và chọn phương thức thanh toán.";
                return View(order);
            }

            _context.Orders.Add(order);

            foreach (var cartItem in cartItems)
            {
                // Ensure we have the latest smartphone data
                var smartphone = await _context.Smartphones.FindAsync(cartItem.SmartphoneId);
                if (smartphone == null) continue;
                
                // Calculate discount amount safely
                decimal originalPrice = smartphone.Price;
                decimal discountedPrice;
                
                // Only use discount price if it is valid (greater than 0 and less than original price)
                if (smartphone.DiscountPrice.HasValue && 
                    smartphone.DiscountPrice.Value > 0 && 
                    smartphone.DiscountPrice.Value < originalPrice)
                {
                    discountedPrice = smartphone.DiscountPrice.Value;
                }
                else
                {
                    discountedPrice = originalPrice;
                }
                
                decimal discountAmount = originalPrice - discountedPrice;
                
                var orderDetail = new OrderDetail_64131491
                {
                    OrderId = order.OrderId,
                    SmartphoneId = cartItem.SmartphoneId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = originalPrice,
                    Discount = discountAmount
                };
                _context.OrderDetails.Add(orderDetail);

                // Update product quantity
                smartphone.Quantity -= cartItem.Quantity;
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