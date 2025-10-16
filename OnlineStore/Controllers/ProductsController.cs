using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineStore.Data;
using OnlineStore.Models;

namespace OnlineStore.Controllers
{
    [Route("/Admin/[controller]/{action=Index}/{id?}")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly int _pageSize = 5;

        public ProductsController(ApplicationDbContext dbContext, IWebHostEnvironment environment)
        {
            _context = dbContext;
            _environment = environment;
        }

        public async Task<IActionResult> Index(int pageIndex, string? search, string? column, string? orderBy)
        {
            IQueryable<Product> query = _context.Products;

            if (!string.IsNullOrEmpty(search))
            {
                var searchTerm = search.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(searchTerm) ||
                                        p.Brand.ToLower().Contains(searchTerm));
            }

            string[] validColumns = {"Name", "Brand", "Category", "Price", "CreatedAt"};
            string[] validOrderBy = {"asc", "desc"};

            if (!validColumns.Contains(column)) column = "Brand";
            if (!validOrderBy.Contains(orderBy)) orderBy = "asc";

            query = ApplySorting(query, column, orderBy);

            if (pageIndex < 1) pageIndex = 1;

            decimal count = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(count / _pageSize);
            query = query.Skip((pageIndex - 1) * _pageSize).Take(_pageSize);

            var products = await query.ToListAsync();

            ViewData["PageIndex"] = pageIndex;
            ViewData["TotalPages"] = totalPages;
            ViewData["Search"] = search ?? "";
            ViewData["Column"] = column;
            ViewData["OrderBy"] = orderBy;
            return View(products);
        }

        private IQueryable<Product> ApplySorting(IQueryable<Product> query, string column, string orderBy)
        {
            return column switch
            {
                "Name" => orderBy == "asc" ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
                "Brand" => orderBy == "asc" ? query.OrderBy(p => p.Brand) : query.OrderByDescending(p => p.Brand),
                "Category" => orderBy == "asc" ? query.OrderBy(p => p.Category) : query.OrderByDescending(p => p.Category),
                "Price" => orderBy == "asc" ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price),
                "CreatedAt" => orderBy == "asc" ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderBy(p => p.Name)
            };
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductDTO productDto)
        {
            if (productDto.ImageFile == null)
                ModelState.AddModelError("ImageFile", "The image is required");

            if (!ModelState.IsValid)
                return View(productDto);

            // save the image file
            string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            newFileName += Path.GetExtension(productDto.ImageFile.FileName);

            string imageFullPath = _environment.WebRootPath + "/products/" + newFileName;

            using (var stream = System.IO.File.Create(imageFullPath))
            {
                await productDto.ImageFile.CopyToAsync(stream);
            }

            // save the product record to the database
            Product product = new Product
            {
                Name = productDto.Name,
                Brand = productDto.Brand,
                Category = productDto.Category,
                Price = productDto.Price,
                Description = productDto.Description,
                ImageFileName = newFileName,
                CreatedAt = DateTime.Now
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Products");
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return RedirectToAction("Index", "Products");

            // map product to productDTO
            var productDto = new ProductDTO
            {
                Name = product.Name,
                Brand = product.Brand,
                Category = product.Category,
                Price = product.Price,
                Description = product.Description
            };

            ViewData["ProductId"] = id;
            ViewData["ImageFileName"] = product.ImageFileName;
            ViewData["CreatedAt"] = product.CreatedAt.ToString("yyyy-MM-dd");
            return View(productDto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, ProductDTO productDTO)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return RedirectToAction("Index", "Products");

            if (!ModelState.IsValid)
            {
                ViewData["ProductId"] = id;
                ViewData["ImageFileName"] = product.ImageFileName;
                ViewData["CreatedAt"] = product.CreatedAt.ToString("yyyy-MM-dd");
                return View(productDTO);
            }

            // update the image file if we have a new image 
            string newFileName = product.ImageFileName;
            if (productDTO.ImageFile != null)
            {
                newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                newFileName += Path.GetExtension(productDTO.ImageFile.FileName);
                string imageFullPath = _environment.WebRootPath + "/products/" + newFileName;

                using (var stream = System.IO.File.Create(imageFullPath))
                {
                    await productDTO.ImageFile.CopyToAsync(stream);
                }

                // delete the old image file
                string oldImageFullPath = _environment.WebRootPath + "/products/" + product.ImageFileName;
                if (System.IO.File.Exists(oldImageFullPath))
                {
                    System.IO.File.Delete(oldImageFullPath);
                }
            }

            // map productDTO to product
            product.Name = productDTO.Name;
            product.Brand = productDTO.Brand;
            product.Category = productDTO.Category;
            product.Price = productDTO.Price;
            product.Description = productDTO.Description;
            product.ImageFileName = newFileName;

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Products");
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return RedirectToAction("Index", "Products");

            string imageFullPath = _environment.WebRootPath + "/products/" + product.ImageFileName;
            if (System.IO.File.Exists(imageFullPath))
            {
                System.IO.File.Delete(imageFullPath);
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Products");
        }
    }
}
