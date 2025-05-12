// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Update cart badge
function updateCartBadge(count) {
    const badge = document.getElementById('cartCount');
    if (badge) {
        badge.textContent = count;
        badge.style.display = count > 0 ? 'inline-block' : 'none';
    }
}

// Get cart count
function getCartCount() {
    $.get('/Cart/GetCartCount', function(data) {
        if (data.cartCount !== undefined) {
            updateCartBadge(data.cartCount);
        }
    });
}

// Hiển thị toast Bootstrap động ở góc dưới phải
function showBootstrapToast(message, type = 'success') {
    const toastContainer = document.getElementById('toast-container');
    const toast = document.createElement('div');
    toast.className = `toast align-items-center text-bg-${type} border-0 show`;
    toast.setAttribute('role', 'alert');
    toast.setAttribute('aria-live', 'assertive');
    toast.setAttribute('aria-atomic', 'true');
    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">${message}</div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
    `;
    toastContainer.appendChild(toast);
    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 500);
    }, 5000);
}

// Add to cart functionality
function addToCart(smartphoneId) {
    const quantity = document.getElementById('quantity')?.value || 1;
    
    $.ajax({
        url: '/Cart/AddToCart',
        type: 'POST',
        data: {
            smartphoneId: smartphoneId,
            quantity: quantity,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (result) {
            if (result.success) {
                updateCartBadge(result.cartCount);
                showBootstrapToast('Đã thêm sản phẩm vào giỏ hàng!', 'success');
            }
        },
        error: function (error) {
            if (error.status === 401) {
                showBootstrapToast('Bạn cần đăng nhập để thêm sản phẩm vào giỏ hàng!', 'danger');
                window.location.href = '/Account/Login';
            } else {
                showBootstrapToast('Có lỗi xảy ra khi thêm sản phẩm vào giỏ hàng!', 'danger');
            }
        }
    });
}

// Update cart quantity
function updateQuantity(cartId, quantity) {
    $.ajax({
        url: '/Cart/UpdateQuantity',
        type: 'POST',
        data: {
            cartId: cartId,
            quantity: quantity,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (result) {
            location.reload();
        },
        error: function (error) {
            alert('Có lỗi xảy ra khi cập nhật số lượng!');
        }
    });
}

// Remove from cart
function removeFromCart(cartId) {
    if (confirm('Bạn có chắc chắn muốn xóa sản phẩm này khỏi giỏ hàng?')) {
        $.ajax({
            url: '/Cart/Remove',
            type: 'POST',
            data: {
                id: cartId,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (result) {
                location.reload();
            },
            error: function (error) {
                alert('Có lỗi xảy ra khi xóa sản phẩm khỏi giỏ hàng!');
            }
        });
    }
}

// Update order status (Admin only)
function updateOrderStatus(orderId, status) {
    $.ajax({
        url: '/Order/UpdateStatus',
        type: 'POST',
        data: {
            id: orderId,
            status: status,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (result) {
            location.reload();
        },
        error: function (error) {
            alert('Có lỗi xảy ra khi cập nhật trạng thái đơn hàng!');
        }
    });
}

// Search functionality
function searchProducts() {
    const searchString = document.getElementById('searchString').value;
    const brand = document.getElementById('brand').value;
    const minPrice = document.getElementById('minPrice').value;
    const maxPrice = document.getElementById('maxPrice').value;

    window.location.href = `/Smartphone/Search?searchString=${searchString}&brand=${brand}&minPrice=${minPrice}&maxPrice=${maxPrice}`;
}

// Password validation
function validatePassword() {
    const password = document.getElementById('password').value;
    const confirmPassword = document.getElementById('confirmPassword').value;

    if (password !== confirmPassword) {
        alert('Mật khẩu xác nhận không khớp!');
        return false;
    }
    return true;
}

// Initialize tooltips and get cart count on page load
document.addEventListener('DOMContentLoaded', function () {
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Get cart count when page loads
    getCartCount();
});
