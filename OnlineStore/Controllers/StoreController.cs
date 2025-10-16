using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> Index(int pageIndex)
        {
            IQueryable<Product> query = _applicationDbContext.Products;
            query.OrderByDescending(p => p.CreatedAt);
            
            if (pageIndex < 1) pageIndex = 1;
            decimal count = await query.CountAsync();
            int numberOfPages = (int)Math.Ceiling(count / _pageSize);
            query = query.Skip((pageIndex - 1) * _pageSize).Take(_pageSize);

            var products = await query.ToListAsync();

            ViewBag.Products = products;
            ViewBag.PageIndex = pageIndex;
            ViewBag.NumberOfPages = numberOfPages;
            return View();
        }
    }
}
