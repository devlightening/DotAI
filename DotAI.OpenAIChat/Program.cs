using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

class Program
{
    private static IConfiguration _config = null!;
    private static HttpClient _httpClient = new();

    static async Task Main(string[] args)
    {
        if (!LoadConfiguration())
            return;

        var apiKey = _config["GeminiApiKey"]!;
        _httpClient.DefaultRequestHeaders.Add("X-goog-api-key", apiKey);

        ShowWelcomeMessage();

        var chatHistory = new List<(string Sender, string Message)>();

        while (true)
        {
            var prompt = ReadUserInput();
            if (IsExitCommand(prompt))
                break;

            chatHistory.Add(("Siz", prompt));

            var response = await SendRequestAsync(prompt);
            if (response.isSuccess)
            {
                chatHistory.Add(("Ahsen", response.answer!));
                DisplayChatHistory(chatHistory);
            }
            else
            {
                ShowError(response.errorMessage ?? "Bilinmeyen hata oluştu.");
            }
        }

        Console.WriteLine("Görüşmek üzere!");
    }

    private static bool LoadConfiguration()
    {
        try
        {
            _config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var apiKey = _config["GeminiApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                ShowError("HATA: appsettings.json içinde 'GeminiApiKey' bulunamadı.\nLütfen API anahtarınızı appsettings.json dosyasına ekleyin.");
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            ShowError($"Yapılandırma dosyası yüklenirken hata oluştu: {ex.Message}");
            return false;
        }
    }

    private static void ShowWelcomeMessage()
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Merhaba Minikkuşummmm");
        Console.WriteLine("Çıkmak için 'çık' veya 'exit' yazabilirsiniz.\n");
        Console.ResetColor();
    }

    private static string ReadUserInput()
    {
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.Write("Siz: ");
        Console.ResetColor();
        return Console.ReadLine() ?? string.Empty;
    }

    private static bool IsExitCommand(string input)
    {
        var trimmedInput = input.Trim().ToLower();
        return string.IsNullOrWhiteSpace(trimmedInput) || trimmedInput == "çık" || trimmedInput == "exit";
    }

    private static async Task<(bool isSuccess, string? answer, string? errorMessage)> SendRequestAsync(string prompt)
    {
        var endpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(endpoint, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return (false, null, $"API Hatası: {response.StatusCode}");
            }

            var result = JsonSerializer.Deserialize<JsonElement>(responseString);
            var answer = result
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return (true, answer, null);
        }
        catch (Exception ex)
        {
            return (false, null, $"İstek sırasında hata oluştu: {ex.Message}");
        }
    }

    private static void DisplayChatHistory(List<(string Sender, string Message)> chatHistory)
    {
        Console.Clear();
        ShowWelcomeMessage();

        foreach (var (Sender, Message) in chatHistory)
        {
            if (Sender == "Siz")
            {
                Console.ForegroundColor = ConsoleColor.Red; // Kırmızı
                Console.Write("Siz: ");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Black; // Siyah
                Console.Write("Ahsen: ");
            }
            Console.ResetColor();
            Console.WriteLine(Message + "\n");
        }
    }

    private static void ShowError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}
