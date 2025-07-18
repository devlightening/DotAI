using DotAI.RapidApi.ViewModels;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

var client = new HttpClient();
List<ApiSeriesViewModel> apiSeriesViewModels=new List<ApiSeriesViewModel>(); 
var request = new HttpRequestMessage
{
    Method = HttpMethod.Get,
    RequestUri = new Uri("https://imdb-top-100-movies.p.rapidapi.com/"),
    Headers =
    {
        { "x-rapidapi-key", "9c21022069mshcde4f8e1feac23cp1fdc25jsn894d1c8b42cd" },
        { "x-rapidapi-host", "imdb-top-100-movies.p.rapidapi.com" },
    },
};
using (var response = await client.SendAsync(request))
{
    response.EnsureSuccessStatusCode();
    var body = await response.Content.ReadAsStringAsync();
    apiSeriesViewModels = JsonConvert.DeserializeObject<List<ApiSeriesViewModel>>(body);
    foreach (var series in apiSeriesViewModels)
    {
        Console.WriteLine($"{series.title} | Rank: {series.rank} | Rating: {series.rating} | Year: {series.year}");
    }

}

Console.ReadLine();