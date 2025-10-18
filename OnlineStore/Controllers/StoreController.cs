using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineStore.Data;
using OnlineStore.Models;

namespace OnlineStore.Controllers
{
    public class StoreController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly int _pageSize = 8;
        public StoreController(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        public async Task<IActionResult> Index(int pageIndex, string? search, string? brand, string? category, string? sort)
        {
            IQueryable<Product> query = _applicationDbContext.Products;

            // search functionality
            if (!string.IsNullOrEmpty(search))
            {
                search.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));
            }

            // filter functionality
            if (!string.IsNullOrEmpty(brand))
            {
                query = query.Where(p => p.Brand.ToLower().Contains(brand.ToLower()));
            }
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category.ToLower().Contains(category.ToLower()));
            }

            // sort functionality
            if (sort == "price-asc")
            {
                query = query.OrderBy(p => p.Price);
            }
            else if (sort == "price-desc")
            {
                query = query.OrderByDescending(p => p.Price);
            }
            else
            {
                // newest products first
                query = query.OrderByDescending(p => p.CreatedAt);
            }
            
            if (pageIndex < 1) pageIndex = 1;
            decimal count = await query.CountAsync();
            int numberOfPages = (int)Math.Ceiling(count / _pageSize);
            query = query.Skip((pageIndex - 1) * _pageSize).Take(_pageSize);

            var products = await query.ToListAsync();

            ViewBag.Products = products;
            ViewBag.PageIndex = pageIndex;
            ViewBag.NumberOfPages = numberOfPages;

            var storeSearchModel = new StoreSearchModel()
            {
                Search = search,
                Brand = brand,
                Category = category,
                Sort = sort
            };
            return View(storeSearchModel);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var product = await _applicationDbContext.Products.FindAsync(id);
            if (product == null)
            {
                return RedirectToAction("Index", "Store");
            }
            return View(product);
        }
    }
}
