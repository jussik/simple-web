using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleWeb.Models;

namespace SimpleWeb.Controllers
{
    [Route("api/[controller]")]
    public class ProductsController : Controller
    {
        private readonly Db db;

        public ProductsController(Db db)
        {
            this.db = db;
        }

        // GET api/values
        [HttpGet]
        public async Task<IEnumerable<Product>> Get()
        {
            return await db.Products.AsNoTracking().ToListAsync();
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<Product> Get(Guid id)
        {
            return await db.Products.AsNoTracking()
                .SingleOrDefaultAsync(p => p.Id == id);
        }

        // POST api/values
        [HttpPost]
        public async Task<Product> Post([FromBody] Product product)
        {
            db.Products.Add(product);
            await db.SaveChangesAsync();
            return product;
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public async Task<Product> Put(Guid id, [FromBody] Product product)
        {
            if(product.Id != default(Guid) && product.Id != id)
                throw new NotSupportedException("Id mismatch");
            db.Products.Attach(product);
            await db.SaveChangesAsync();
            return product;
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public async Task<NoContentResult> Delete(Guid id)
        {
            var prod = new Product{Id = id};
            db.Products.Attach(prod);
            db.Products.Remove(prod);
            await db.SaveChangesAsync();
            return NoContent();
        }
    }
}
