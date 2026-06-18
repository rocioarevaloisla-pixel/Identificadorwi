using System.Text.Json;
using Identificador.Services;

namespace Identificador.Views;

[QueryProperty(nameof(Usuario), "usuario")]
public partial class HomePage : ContentPage
{
    private string _usuario = string.Empty;

    public string Usuario
    {
        get => _usuario;
        set
        {
            _usuario = value;
            BienvenidoLabel.Text = $"Bienvenido, {_usuario}";
        }
    }

    public HomePage()
    {
        InitializeComponent();
    }

    private async void OnIdentificarPlanta(object? sender, EventArgs e)
    {
        var action = await DisplayActionSheetAsync("Seleccionar foto", "Cancelar", null, "Tomar foto", "Elegir de galería");
        if (action is null or "Cancelar") return;

        FileResult? foto = action switch
        {
            "Tomar foto" => await MediaPicker.CapturePhotoAsync(),
            "Elegir de galería" => (await MediaPicker.PickPhotosAsync()).FirstOrDefault(),
            _ => null
        };

        if (foto is null) return;

        IdentificarBtn.IsEnabled = false;
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;

        try
        {
            using var stream = await foto.OpenReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var bytes = ms.ToArray();

            var service = Handler!.MauiContext!.Services.GetRequiredService<PlantNetService>();
            var resultadoJson = await service.IdentifyPlantAsync(bytes, foto.FileName);

            using var doc = JsonDocument.Parse(resultadoJson);
            var results = doc.RootElement.GetProperty("results");

            if (results.GetArrayLength() == 0)
            {
                await DisplayAlertAsync("Sin resultados", "No se pudo identificar la planta", "OK");
                return;
            }

            var best = results[0].GetProperty("species");
            var scientificName = best.GetProperty("scientificNameWithoutAuthor").GetString();
            var commonNames = best.TryGetProperty("commonNames", out var cn)
                ? string.Join(", ", cn.EnumerateArray().Select(c => c.GetString()))
                : "Sin nombre común";
            var score = results[0].GetProperty("score").GetDouble();

            var mensaje = $"Nombre científico: {scientificName}\n" +
                          $"Nombre común: {commonNames}\n" +
                          $"Coincidencia: {score:P1}";

            await DisplayAlertAsync("Planta identificada", mensaje, "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"No se pudo conectar con PlantNet: {ex.Message}", "OK");
        }
        finally
        {
            IdentificarBtn.IsEnabled = true;
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnCerrarSesion(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }
}
