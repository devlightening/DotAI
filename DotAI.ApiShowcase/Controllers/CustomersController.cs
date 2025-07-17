using DotAI.ApiShowcase.Context;
using DotAI.ApiShowcase.Entites;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Async için

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

        // Tüm müşterileri getir (async)
        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await _context.Customers.ToListAsync();
            return Ok(customers);
        }

        // Tek müşteri getir (id ile)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer(int id)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == id);
            if (customer == null)
            {
                return NotFound();
            }
            return Ok(customer);
        }

        // Yeni müşteri oluştur
        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] Customer customer)
        {
            if (customer == null)
            {
                return BadRequest("Müşteri bilgileri eksik.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // CreatedAtAction ile yeni müşterinin detay endpointine yönlendir
            return CreatedAtAction(nameof(GetCustomer), new { id = customer.CustomerId }, customer);
        }

        // Müşteri güncelle (id ve body'den)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] Customer customer)
        {
            if (id != customer.CustomerId)
            {
                return BadRequest("ID uyuşmazlığı.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == id);
            if (existingCustomer == null)
            {
                return NotFound();
            }

            // Güncelle
            existingCustomer.CustomerName = customer.CustomerName;
            existingCustomer.CustomerSurname = customer.CustomerSurname;
            existingCustomer.CustomerBalance = customer.CustomerBalance;

            await _context.SaveChangesAsync();

            return Ok("Müşteri başarıyla güncellendi.");
        }

        // Müşteri sil
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == id);
            if (customer == null)
            {
                return NotFound();
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // İsim ve soyisim ile arama
        [HttpGet("search")]
        public async Task<IActionResult> SearchCustomers(string name, string surname)
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

            var customers = await query.ToListAsync();
            return Ok(customers);
        }

        // Pozitif bakiye olan müşteriler, büyükten küçüğe sırala
        [HttpGet("balance")]
        public async Task<IActionResult> ListCustomersWithBalance()
        {
            var customers = await _context.Customers
                .Where(c => c.CustomerBalance > 0)
                .OrderByDescending(c => c.CustomerBalance)
                .ToListAsync();

            return Ok(customers);
        }

        // Minimum bakiye filtreli liste
        [HttpGet("balance/{minBalance}")]
        public async Task<IActionResult> MinBalanceList(decimal minBalance)
        {
            var customers = await _context.Customers
                .Where(c => c.CustomerBalance >= minBalance)
                .OrderByDescending(c => c.CustomerBalance)
                .ToListAsync();

            return Ok(customers);
        }
    }
}
