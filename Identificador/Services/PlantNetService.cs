using System.Net.Http.Headers;
using System.Text.Json;

namespace Identificador.Services;

/// <summary>
/// Servicio para la identificación de plantas a través de la API de PlantNet.
/// Envía una imagen y devuelve los resultados de identificación en formato JSON.
/// </summary>
public class PlantNetService
{
    private readonly HttpClient _http;
    private const string ApiKey = "2b10YFuQrd3ZT1Yaxalm7hGOe";
    private const string BaseUrl = "https://my-api.plantnet.org/v2/identify/all";

    public PlantNetService(HttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Identifica una planta a partir de una imagen usando la API de PlantNet.
    /// </summary>
    /// <param name="imageBytes">Arreglo de bytes de la imagen.</param>
    /// <param name="filename">Nombre del archivo de la imagen.</param>
    /// <param name="contentType">Tipo MIME de la imagen (por defecto image/jpeg).</param>
    /// <returns>Respuesta JSON de PlantNet con los resultados de la identificación.</returns>
    public async Task<string> IdentifyPlantAsync(byte[] imageBytes, string filename, string contentType = "image/jpeg")
    {
        var url = $"{BaseUrl}?api-key={ApiKey}&lang=es";
        using var content = new MultipartFormDataContent();
        var imageContent = new ByteArrayContent(imageBytes);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(imageContent, "images", filename);

        var response = await _http.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error {(int)response.StatusCode}: {body}", null, response.StatusCode);
        }

        return await response.Content.ReadAsStringAsync();
    }
}
