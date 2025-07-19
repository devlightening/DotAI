using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text;

class Program
{
    public static async Task Main(string[] args)
    {
        // 1. appsettings.json'dan API key'i oku
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // Projenin çalışma dizinini alır
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var token = configuration["huggingfaceApiKey"];

        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("❌ API anahtarı bulunamadı. Lütfen appsettings.json dosyasını kontrol edin.");
            return;
        }

        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        Console.Write("🎨 Oluşturulacak görsel için bir betimleme girin (prompt): ");
        var userPrompt = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(userPrompt))
        {
            Console.WriteLine("❌ Lütfen geçerli bir prompt girin.");
            return;
        }

        var requestData = new
        {
            prompt = userPrompt,
            response_format = "base64",
            model = "black-forest-labs/FLUX.1-schnell"
        };

        var json = JsonSerializer.Serialize(requestData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync("https://router.huggingface.co/together/v1/images/generations", content);

        if (response.IsSuccessStatusCode)
        {
            var responseJson = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseJson);
            var base64 = doc.RootElement.GetProperty("data")[0].GetProperty("b64_json").GetString();
            var imageBytes = Convert.FromBase64String(base64);

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Images");
            Directory.CreateDirectory(folderPath);

            var fileName = $"image_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            var filePath = Path.Combine(folderPath, fileName);

            await File.WriteAllBytesAsync(filePath, imageBytes);
            Console.WriteLine($"✅ Görsel oluşturuldu ve kaydedildi: {filePath}");
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine("❌ Hata: " + error);
        }
    }
}
