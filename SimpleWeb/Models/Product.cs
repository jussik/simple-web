using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace SimpleWeb.Models {
    public class Product {
        public Guid Id {get;set;}
        [Required]
        public string Name {get;set;}
    }

    public class Db : DbContext {
        public DbSet<Product> Products {get;set;}

        public Db() {  }

        public Db(DbContextOptions<Db> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);

            builder.HasPostgresExtension("uuid-ossp");
        }
    }
}