using ECommerce_Application.Mappings;
using ECommerce_Application.Services.Cart_Service;
using ECommerce_Application.ViewModels;
using ECommerceApplication.Data;
using ECommerceApplication.Models;
using ECommerceApplication.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace ECommerce_Application.Controllers
{
    [Authorize(Roles = Roles.Customer)]
    public class CheckoutController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly ICartService _cartService;

        public CheckoutController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IConfiguration config, ICartService cartService)
        {
            _userManager = userManager;
            _context = context;
            _config = config;
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var cart = await _cartService.GetCartWithDetailsAsync(user.Id);
            if (cart is null || !cart.Items.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }
            return View(new CheckoutVM
            {
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                Phone = user.PhoneNumber,
                Cart = CartMapper.BuildViewModel(cart)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutVM checkoutvm)
        {
            var userId = _userManager.GetUserId(User);
            var cart = await _cartService.GetCartWithDetailsAsync(userId);
            if (cart is null || !cart.Items.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }
            checkoutvm.Cart = CartMapper.BuildViewModel(cart);
            if (!ModelState.IsValid) return View("Index", checkoutvm);
            var cartVm = checkoutvm.Cart;

            // Create Order
            var order = new Order
            {
                CustomerId = userId,
                ShippingAddress = checkoutvm.FormattedAddress,
                SubTotal = cartVm.SubTotal,
                DiscountAmount = cartVm.DiscountAmount,
                Total = cartVm.Total,
                PromotionId = cart.AppliedPromotionId,
                Status = OrderStatus.Pending
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Add items to Order
            foreach (var item in cartVm.Items)
            {
                _context.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    VendorId = (await _context.Products
                                   .Where(p => p.Id == item.ProductId)
                                   .Select(p => p.VendorId)
                                   .FirstAsync()),
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                });
            }
            await _context.SaveChangesAsync();

            // Create Stripe Checkout Session
            var domain = $"{Request.Scheme}://{Request.Host}";
            var promo = cart.AppliedPromotion;
            var lineItems = cartVm.Items.Select(item =>
            {
                decimal unitPrice = item.UnitPrice;
                if (promo is not null)
                {
                    bool applies = promo.CategoryId is null || promo.CategoryId == item.CategoryId;
                    if (applies)
                        unitPrice = Math.Round(unitPrice * (1 - promo.DiscountPercentage / 100m), 2);
                }
                return new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        UnitAmount = (long)(unitPrice * 100),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.ProductName
                        }
                    },
                    Quantity = item.Quantity
                };
            }).ToList();

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = $"{domain}/Checkout/Success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{domain}/Checkout/Cancel?orderId={order.Id}",
                Metadata = new Dictionary<string, string> { { "orderId", order.Id.ToString() } }
            };

            var service = new SessionService();
            Session session;
            try
            {
                session = await service.CreateAsync(options);
            }
            catch (StripeException ex)
            {
                // Stripe keys not configured — still show success for dev/demo
                TempData["Info"] = $"Stripe not configured ({ex.Message}). Order #{order.Id} saved as Pending.";
                return RedirectToAction("Success", new { orderId = order.Id, demo = true });
            }

            order.StripeSessionId = session.Id;
            await _context.SaveChangesAsync();

            return Redirect(session.Url);
        }

        public async Task<IActionResult> Success(string? session_id, int? orderId, bool demo = false)
        {
            Order? order;

            if (demo && orderId.HasValue)
            {
                order = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == orderId);
                if (order is null) return NotFound();
                await FulfillOrderAsync(order.Id);
            }
            else
            {
                if (string.IsNullOrEmpty(session_id)) return RedirectToAction("Index", "Home");

                var service = new SessionService();
                Session session;
                try { session = await service.GetAsync(session_id); }
                catch { return RedirectToAction("Index", "Home"); }

                if (session.PaymentStatus != "paid") return RedirectToAction("Index", "Cart");

                if (!session.Metadata.TryGetValue("orderId", out var orderIdStr) ||
                    !int.TryParse(orderIdStr, out int oid))
                    return RedirectToAction("Index", "Home");

                order = await _context.Orders.FindAsync(oid);
                if (order is null) return NotFound();

                if (order.Status == OrderStatus.Pending)
                {
                    order.StripeSessionId = session.Id;
                    order.StripePaymentIntentId = session.PaymentIntentId;
                    await FulfillOrderAsync(order.Id);
                }
            }

            return View(order);
        }

        // GET /Checkout/Cancel
        public async Task<IActionResult> Cancel(int orderId)
        {
            var userId = _userManager.GetUserId(User)!;
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == userId);

            if (order?.Status == OrderStatus.Pending)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }

            TempData["Info"] = "Payment cancelled. Your cart is still saved.";
            return RedirectToAction("Index", "Cart");
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Webhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var webhookSecret = _config["Stripe:WebhookSecret"];

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json, Request.Headers["Stripe-Signature"], webhookSecret);

                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Session;
                    if (session?.Metadata.TryGetValue("orderId", out var idStr) == true &&
                        int.TryParse(idStr, out int oid))
                    {
                        var order = await _context.Orders.FindAsync(oid);
                        if (order?.Status == OrderStatus.Pending)
                        {
                            order.StripeSessionId = session.Id;
                            order.StripePaymentIntentId = session.PaymentIntentId;
                            await FulfillOrderAsync(oid);
                        }
                    }
                }
                return Ok();
            }
            catch (StripeException) { return BadRequest(); }
        }
        private async Task FulfillOrderAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Promotion)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order is null || order.Status != OrderStatus.Pending) return;

            order.Status = OrderStatus.Processing;

            // Deduct stock
            foreach (var item in order.OrderItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product is not null)
                    product.StockQuantity = Math.Max(0, product.StockQuantity - item.Quantity);
            }

            // Increment promo usage
            if (order.Promotion is not null)
                order.Promotion.UsageCount++;

            // Clear cart
            var userId = order.CustomerId;
            var cart = await _context.Carts.Include(c => c.Items)
                             .FirstOrDefaultAsync(c => c.CustomerId == userId);
            if (cart is not null)
            {
                _context.CartItems.RemoveRange(cart.Items);
                cart.AppliedPromotionId = null;
                cart.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

    }
}
