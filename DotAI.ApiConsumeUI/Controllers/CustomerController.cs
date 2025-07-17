using DotAI.ApiConsumeUI.Dtos;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace DotAI.ApiConsumeUI.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CustomerController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> CustomerList()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://localhost:7291/api/customers");
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<List<ResultCustomerDto>>(jsonData);
                return View(values);
            }
            return View();
        }

        public async Task<IActionResult> CreateCustomer(CreateCustomerDto createCustomerDto)
        {
            var client = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(createCustomerDto);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://localhost:7291/api/customers", content);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("CustomerList");
            }
            return View();


        }

        // GET: Customers/UpdateCustomer/5  --> Güncelleme formunu açmak için
        [HttpGet]
        public async Task<IActionResult> UpdateCustomer(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"https://localhost:7291/api/customers/{id}");
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var customer = JsonConvert.DeserializeObject<ResultCustomerDto>(jsonData);
                return View(customer);
            }
            return NotFound();
        }

        // POST: Customers/UpdateCustomer/5  --> Formdan gelen güncellemeyi API'ye gönder
        [HttpPost]
        public async Task<IActionResult> UpdateCustomer(int id, ResultCustomerDto model)
        {
            if (id != model.CustomerId)
            {
                return BadRequest("ID mismatch");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(model);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"https://localhost:7291/api/customers/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(CustomerList));
            }

            ModelState.AddModelError(string.Empty, "Güncelleme sırasında hata oluştu.");
            return View(model);
        }
    }
}
