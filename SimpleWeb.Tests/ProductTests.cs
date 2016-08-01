using Xunit;
using SimpleWeb.Controllers;
using SimpleWeb.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;

namespace ClassLibrary
{
    public class ProductTests
    {
        private readonly DbContextOptions<Db> dbOpts;

        public ProductTests()
        {
             var sp = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();
            
            var builder = new DbContextOptionsBuilder<Db>()
                .UseInMemoryDatabase()
                .UseInternalServiceProvider(sp);
            
            dbOpts = builder.Options;
        }

        [Fact]
        public void TestEmptyGet()
        {
            using(var db = new Db(dbOpts))
            {
                var ctrl = new ProductsController(db);
                Assert.Empty(ctrl.Get().Result);
            }
        }

        [Fact]
        public void TestGetMultiple()
        {
            using(var db = new Db(dbOpts))
            {
                db.Products.Add(new Product{Name="Test 1"});
                db.Products.Add(new Product{Name="Test 2"});
                db.SaveChanges();
            }

            using(var db = new Db(dbOpts))
            {
                var ctrl = new ProductsController(db);
                Assert.Equal(
                    new[]{"Test 1","Test 2"},
                    ctrl.Get().Result.Select(p => p.Name).OrderBy(s => s));
            }
        }

        [Fact]
        public void TestPost()
        {
            Guid newId;
            using(var db = new Db(dbOpts))
            {
                var ctrl = new ProductsController(db);
                newId = ctrl.Post(new Product{Name="Test"}).Result.Id;
            }

            using(var db = new Db(dbOpts))
            {
                Assert.NotNull(db.Products.SingleOrDefault(p => p.Id == newId));
            }
        }
    }
}
