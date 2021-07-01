using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyAPI.Context;
using MyAPI.Helpers;
using MyAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly DataContext _context;

        public ProductsController(DataContext dataContext)
        {
            _context = dataContext;

            _context.Database.EnsureCreated();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts([FromQuery]ProductQueryParametrs queryParametrs)
        {
            IQueryable<Product> products = _context.Products;

            if(queryParametrs.MinPrice!=null && queryParametrs.MaxPrice != null)
            {
                products = products.Where(p => p.Price >= queryParametrs.MinPrice && p.Price <= queryParametrs.MaxPrice);
            }

            if (!string.IsNullOrEmpty(queryParametrs.SearchTerm))
            {
                products = products.Where(p => p.Sku.Contains(queryParametrs.SearchTerm) || 
                    p.Name.ToLower().Contains(queryParametrs.SearchTerm.ToLower()));
            }

            if (!string.IsNullOrEmpty(queryParametrs.Sku))
            {
                products = products.Where(p => p.Sku == queryParametrs.Sku);
            }

            if (!string.IsNullOrEmpty(queryParametrs.Name))
            {
                products = products.Where(p => p.Name.ToLower().Contains(queryParametrs.Name.ToLower()));
            }
            if (!string.IsNullOrEmpty(queryParametrs.SortBy))
            {
                if (typeof(Product).GetProperty(queryParametrs.SortBy) != null)
                {
                    products = products.OrderByCustom(queryParametrs.SortBy, queryParametrs.SortOrder);
                }
            }

            products = products
                .Skip(queryParametrs.Size * (queryParametrs.Page - 1))
                .Take(queryParametrs.Size);

            return Ok(await products.ToArrayAsync());
        }

        [HttpGet, Route("/products/{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if(product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        
    }
}
