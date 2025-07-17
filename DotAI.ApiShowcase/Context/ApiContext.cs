using DotAI.ApiShowcase.Entites;
using Microsoft.EntityFrameworkCore;

namespace DotAI.ApiShowcase.Context
{
    public class ApiContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=DESKTOP-4UQ0AMN\\SQLEXPRESS01;initial catalog=ApiAIDb;integrated security=true;trustservercertificate=true");
        }

        public DbSet<Customer> Customers { get; set; }
    }
}