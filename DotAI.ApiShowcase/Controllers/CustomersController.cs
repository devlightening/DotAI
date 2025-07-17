using DotAI.ApiShowcase.Context;
using DotAI.ApiShowcase.Entites;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DotAI.ApiShowcase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ApiContext _context;

        public CustomersController(ApiContext context)
        {
            _context = context;
        }

        [HttpGet("GetCustomer")]
        public IActionResult GetCustomers()
        {
            var customers = _context.Customers.ToList();
            return Ok(customers);
        }

        [HttpGet("{id}")]
        public IActionResult GetCustomer(int id)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.CustomerId == id);
            if (customer == null)
            {
                return NotFound();
            }
            return Ok(customer);
        }

        [HttpPost]
        public IActionResult CreateCustomer(Customer customer)
        {
            if (customer == null)
            {
                return BadRequest("Customer cannot be null");
            }
            _context.Customers.Add(customer);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetCustomer), new { id = customer.CustomerId }, customer);
        }

        [HttpPut]
        public IActionResult UpdateCustomer(int id, Customer customer)
        {
            _context.Customers.Update(customer);
            _context.SaveChanges();
            return Ok("Müşteri Başarıyla Güncellendi.");

        }

        [HttpDelete("{id}")]
        public IActionResult DeleteCustomer(int id)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.CustomerId == id);
            if (customer == null)
            {
                return NotFound();
            }
            _context.Customers.Remove(customer);
            _context.SaveChanges();
            return NoContent();
        }

        [HttpGet("search")]
        public IActionResult SearchCustomers(string name, string surname)
        {
            var query = _context.Customers.AsQueryable();
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(c => c.CustomerName.Contains(name));
            }
            if (!string.IsNullOrEmpty(surname))
            {
                query = query.Where(c => c.CustomerSurname.Contains(surname));
            }
            var customers = query.ToList();
            return Ok(customers);
        }

        [HttpGet]
        public IActionResult CustomerList()
        {
            var customers = _context.Customers.ToList();
            return Ok(customers);

        }

        [HttpGet("balance")]
        public IActionResult ListCustomers()
        {
            var customers = _context.Customers
                .Where(c => c.CustomerBalance > 0)
                .OrderByDescending(c => c.CustomerBalance)
                .ToList();
            return Ok(customers);

        }

        [HttpGet("balance/{minBalance}")]
        public IActionResult MinBalanceList(decimal minBalance)
        {
            var customers = _context.Customers
                .Where(c => c.CustomerBalance >= minBalance)
                .OrderByDescending(c => c.CustomerBalance)
                .ToList();
            return Ok(customers);
        }

    }
}
