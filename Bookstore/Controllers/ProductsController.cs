using Bookstore.data;
using Bookstore.model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductsController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // =====================
        // 📦 Get All Products (Paginated)
        // =====================
        [HttpGet]
        public IActionResult GetProducts(int page = 1, int limit = 10)
        {
            var totalItems = _context.Products.Count();
            var products = _context.Products
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();

            return Ok(new
            {
                total = totalItems,
                page,
                limit,
                data = products
            });
        }

        [HttpGet("search")]
        public IActionResult SearchProducts(string? q, string? sort = "name", int? minPrice = null, int? maxPrice = null)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(q))
                query = query.Where(p => p.Name.Contains(q) || p.Description.Contains(q));

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name" => query.OrderBy(p => p.Name),
                _ => query
            };

            return Ok(query.ToList());
        }

        // =====================
        // ➕ Add New Product
        // =====================
        [HttpPost]
        public async Task<IActionResult> AddProduct([FromForm] Product product, IFormFile image)
        {
            if (product == null || image == null)
                return BadRequest(new { message = "Invalid product or image." });

            try
            {
                if (string.IsNullOrEmpty(_env.WebRootPath))
                    return StatusCode(500, new { message = "Web root path is not set." });

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var uploadFolder = Path.Combine(_env.WebRootPath, "uploads");

                Directory.CreateDirectory(uploadFolder);

                var filePath = Path.Combine(uploadFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                product.Image = $"/uploads/{fileName}";
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error saving product", error = ex.Message });
            }
        }

        // =====================
        // ❌ Delete Product
        // =====================
        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var prod = _context.Products.Find(id);
            if (prod == null)
                return NotFound();

            _context.Products.Remove(prod);
            _context.SaveChanges();
            return Ok();
        }
    }
}
