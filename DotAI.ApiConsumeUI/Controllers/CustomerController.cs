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

        // Listeleme
        public async Task<IActionResult> CustomerList()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://localhost:7164/api/customers");
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<List<ResultCustomerDto>>(jsonData);
                return View(values);
            }
            return View();
        }

        // GET: Müşteri oluşturma formunu aç
        [HttpGet]
        public IActionResult CreateCustomer()
        {
            return View();
        }

        // POST: Yeni müşteri oluştur
        [HttpPost]
        public async Task<IActionResult> CreateCustomer(CreateCustomerDto createCustomerDto)
        {
            var client = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(createCustomerDto);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://localhost:7164/api/customers", content);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("CustomerList");
            }
            return View();
        }

        // GET: Güncelleme formu
        [HttpGet]
        public async Task<IActionResult> UpdateCustomer(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"https://localhost:7164/api/customers/{id}");
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var customer = JsonConvert.DeserializeObject<UpdateCustomerDto>(jsonData);
                return View(customer);
            }
            return NotFound();
        }

        // POST: Güncelleme işlemi
        [HttpPost]
        public async Task<IActionResult> UpdateCustomer(UpdateCustomerDto updateCustomerDto)
        {
            var client = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(updateCustomerDto);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"https://localhost:7164/api/customers/{updateCustomerDto.CustomerId}", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("CustomerList");
            }

            return View(updateCustomerDto);
        }

        // POST: Müşteri sil
        [HttpPost]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.DeleteAsync($"https://localhost:7164/api/customers/{id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("CustomerList");
            }

            return BadRequest("Silme işlemi başarısız.");
        }
    }
}
