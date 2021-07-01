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

            if(ModelState.IsValid)

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

        [HttpPost]
        public async Task<ActionResult<Product>> AddProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct([FromRoute]int id, [FromBody] Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }
            _context.Entry(product).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (_context.Products.Find(id) == null)
                {
                    return NotFound();
                }
                throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Product>> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return product;
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<ActionResult<Product>> DeleteProducts([FromQuery]int[] ids)
        {
            var products = new List<Product>();
            foreach(var id in ids)
            {
                var product = await _context.Products.FindAsync(id);

                if (product == null)
                {
                    return NotFound();
                }
                products.Add(product);
            }
            

            _context.Products.RemoveRange(products);
            await _context.SaveChangesAsync();

            return Ok(products);
        }

    }
}
