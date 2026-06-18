using System.Net.Http.Headers;
using System.Text.Json;

namespace Identificador.Services;

public class PlantNetService
{
    private readonly HttpClient _http;
    private const string ApiKey = "AQUI_TU_API_KEY";
    private const string BaseUrl = "https://my-api.plantnet.org/v2/identify/all";

    public PlantNetService(HttpClient http)
    {
        _http = http;
    }

    public async Task<string> IdentifyPlantAsync(byte[] imageBytes, string filename)
    {
        var url = $"{BaseUrl}?api-key={ApiKey}";
        using var content = new MultipartFormDataContent();
        var imageContent = new ByteArrayContent(imageBytes);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(imageContent, "images", filename);

        var response = await _http.PostAsync(url, content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
