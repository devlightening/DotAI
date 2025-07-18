using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

class Program
{
    static async Task Main(string[] args)
    {
        // Konsol başlığı ve renkli hoş geldin mesajı
        Console.Title = "AssemblyAI Speech-to-Text";
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine("=== AssemblyAI Speech-to-Text Console Uygulaması ===\n");
        Console.ResetColor();

        // 1. API anahtarını appsettings.json'dan oku
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        var apiKey = config["AssemblyAIApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("HATA: appsettings.json içinde 'AssemblyAIApiKey' bulunamadı.");
            Console.ResetColor();
            return;
        }

        // 2. Ses dosyasının yolunu belirtin
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.Write("Lütfen Analiz Edilecek Ses Dosyasının Tam Yolunu Girin (örn: C:\\Users\\hy971\\Desktop\şiir.mp3): ");
        Console.ResetColor();
        string audioFilePath = Console.ReadLine()?.Trim() ?? "audio1.mp3";
        if (!File.Exists(audioFilePath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Ses dosyası bulunamadı: {audioFilePath}");
            Console.ResetColor();
            return;
        }

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("authorization", apiKey);

        // 3. Dosyayı AssemblyAI'ya yükle
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine("\n[1/3] Ses dosyası yükleniyor...");
        Console.ResetColor();
        string uploadUrl;
        using (var fileStream = File.OpenRead(audioFilePath))
        using (var fileContent = new StreamContent(fileStream))
        {
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            using var response = await httpClient.PostAsync("https://api.assemblyai.com/v2/upload", fileContent);
            response.EnsureSuccessStatusCode();
            var jsonDoc = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(jsonDoc);
            uploadUrl = json.RootElement.GetProperty("upload_url").GetString()!;
        }

        // 4. Transkripsiyon isteği gönder
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine("[2/3] Transkripsiyon başlatılıyor...");
        Console.ResetColor();
        var requestData = new
        {
            audio_url = uploadUrl,
            language_code = "tr", // Türkçe dil kodu
            speech_model = "universal"
        };
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestData),
            Encoding.UTF8,
            "application/json");

        using var transcriptResponse = await httpClient.PostAsync("https://api.assemblyai.com/v2/transcript", jsonContent);
        var transcriptResponseBody = await transcriptResponse.Content.ReadAsStringAsync();
        var transcriptData = JsonSerializer.Deserialize<JsonElement>(transcriptResponseBody);

        if (!transcriptData.TryGetProperty("id", out JsonElement idElement))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Transkript ID alınamadı.");
            Console.ResetColor();
            return;
        }

        string transcriptId = idElement.GetString()!;
        string pollingEndpoint = $"https://api.assemblyai.com/v2/transcript/{transcriptId}";

        // 5. Sonuç tamamlanana kadar bekle
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine("[3/3] Ses dosyası işleniyor, lütfen bekleyiniz...");
        Console.ResetColor();
        int dotCount = 0;
        while (true)
        {
            using var pollingResponse = await httpClient.GetAsync(pollingEndpoint);
            var pollingResponseBody = await pollingResponse.Content.ReadAsStringAsync();
            var transcriptionResult = JsonSerializer.Deserialize<JsonElement>(pollingResponseBody);

            if (!transcriptionResult.TryGetProperty("status", out JsonElement statusElement))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Transkripsiyon durumu alınamadı.");
                Console.ResetColor();
                return;
            }

            string status = statusElement.GetString()!;

            if (status == "completed")
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n=== Transkript Başarılı ===\n");
                if (transcriptionResult.TryGetProperty("text", out JsonElement textElement))
                {
                    string transcriptText = textElement.GetString() ?? string.Empty;
                    Console.WriteLine(transcriptText);
                }
                else
                {
                    Console.WriteLine("Transkript metni alınamadı.");
                }
                Console.ResetColor();
                break;
            }
            else if (status == "error")
            {
                string errorMessage = transcriptionResult.TryGetProperty("error", out JsonElement errorElement)
                    ? errorElement.GetString() ?? "Bilinmeyen hata"
                    : "Bilinmeyen hata";
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Transkripsiyon başarısız: {errorMessage}");
                Console.ResetColor();
                break;
            }
            else
            {
                // Kullanıcıya bekleme animasyonu göster
                Console.Write(".");
                dotCount++;
                if (dotCount % 10 == 0) Console.WriteLine();
                await Task.Delay(1000); // 1 saniye bekle ve tekrar dene
            }
        }
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n\nProgramı kapatmak için bir tuşa basın...");
        Console.ResetColor();
        Console.ReadKey();
    }
}