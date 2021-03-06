using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleWeb.Models;
using SimpleWeb.Services;

namespace SimpleWeb.Controllers
{
    [Route("api/[controller]"), PubConsumer("products")]
    public class ProductsController : Controller
    {
        private readonly Db db;
        private readonly Publisher publisher;

        public ProductsController(Db db, Publisher publisher)
        {
            this.db = db;
            this.publisher = publisher;
        }

        [HttpGet]
        public async Task<IEnumerable<Product>> Get()
        {
            return await db.Products.AsNoTracking().ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<Product> Get(Guid id)
        {
            return await db.Products.AsNoTracking()
                .SingleOrDefaultAsync(p => p.Id == id);
        }

        [PubConsumer("ADD")]
        public async Task<Product> Add(Product product)
        {
            db.Products.Add(product);
            await db.SaveChangesAsync();
            return product;
        }

        [PubConsumer("REMOVE")]
        public async Task<Product> Remove(Product product)
        {
            db.Products.Attach(product);
            db.Products.Remove(product);
            await db.SaveChangesAsync();
            return new Product{Id=product.Id};
        }

        [HttpPost]
        public async Task<Product> Post([FromBody] Product product)
        {
            db.Products.Add(product);
            await db.SaveChangesAsync();
            return product;
        }

        [HttpPut("{id}")]
        public async Task<Product> Put(Guid id, [FromBody] Product product)
        {
            if(product.Id != default(Guid) && product.Id != id)
                throw new NotSupportedException("Id mismatch");
            db.Products.Attach(product);
            await db.SaveChangesAsync();
            return product;
        }

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
