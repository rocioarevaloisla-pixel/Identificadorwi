using System.Net.Http;
using System.Text.Json;
using Identificador.Services;

namespace Identificador.Views;

[QueryProperty(nameof(Usuario), "usuario")]
public partial class HomePage : ContentPage
{
    private string _usuario = string.Empty;
    // Esto nos ayuda a esperar la opcion que elija el usuario en el action sheet
    private TaskCompletionSource<string>? _actionSheetTcs;

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

    // Muestra el overlay de alerta en vez del dialogo gris del sistema
    private void ShowAlert(string title, string message)
    {
        AlertTitle.Text = title;
        AlertMessage.Text = message;
        AlertOverlay.IsVisible = true;
    }

    // Muestra el overlay para elegir entre camara o galeria
    private Task<string> ShowActionSheetAsync()
    {
        _actionSheetTcs = new TaskCompletionSource<string>();
        ActionSheetOverlay.IsVisible = true;
        return _actionSheetTcs.Task;
    }

    private void OnCerrarAlert(object? sender, EventArgs e)
    {
        AlertOverlay.IsVisible = false;
    }

    private void OnTomarFotoAction(object? sender, EventArgs e)
    {
        ActionSheetOverlay.IsVisible = false;
        _actionSheetTcs?.TrySetResult("Tomar foto");
    }

    private void OnElegirGaleriaAction(object? sender, EventArgs e)
    {
        ActionSheetOverlay.IsVisible = false;
        _actionSheetTcs?.TrySetResult("Elegir de galería");
    }

    private void OnCancelarAction(object? sender, EventArgs e)
    {
        ActionSheetOverlay.IsVisible = false;
        _actionSheetTcs?.TrySetResult("Cancelar");
    }

    // Cuando el usuario toca "Identificar Planta" empieza todo el proceso
    private async void OnIdentificarPlanta(object? sender, EventArgs e)
    {
        // Le preguntamos si quiere usar la camara o subir una foto de la galeria
        var action = await ShowActionSheetAsync();
        if (action is null or "Cancelar") return;

        // Desactivamos el boton y mostramos el indicador de carga
        IdentificarBtn.IsEnabled = false;
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;

        try
        {
            FileResult? foto = null;

            if (action == "Tomar foto")
            {
                // Verificamos que el telefono tenga camara
                if (!MediaPicker.Default.IsCaptureSupported)
                {
                    ShowAlert("Cámara no disponible", "Este dispositivo no soporta captura de fotos");
                    return;
                }

                // Pedir permiso de camara antes de abrir la app de fotos
                var status = await Permissions.RequestAsync<Permissions.Camera>();

                // Si el usuario no nos dio permiso, le avisamos y salimos
                if (status != PermissionStatus.Granted)
                {
                    ShowAlert("Permiso denegado", "Sin permiso de cámara no podemos tomar fotos");
                    return;
                }

                foto = await MediaPicker.CapturePhotoAsync();
            }
            else if (action == "Elegir de galería")
            {
                var fotos = await MediaPicker.PickPhotosAsync();
                foto = fotos?.FirstOrDefault();
            }

            // Si no se selecciono ninguna foto, terminamos
            if (foto is null)
            {
                ShowAlert("Sin foto", "No se seleccionó ninguna foto. Intentá de nuevo.");
                return;
            }

            // Leemos la foto como bytes para enviarla a la API
            using var stream = await foto.OpenReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var bytes = ms.ToArray();
            var mimeType = foto.ContentType ?? "image/jpeg";

            // Llamamos al servicio de PlantNet para identificar la planta
            var service = Handler!.MauiContext!.Services.GetRequiredService<PlantNetService>();
            var resultadoJson = await service.IdentifyPlantAsync(bytes, foto.FileName, mimeType);

            // Parseamos la respuesta JSON
            using var doc = JsonDocument.Parse(resultadoJson);
            var results = doc.RootElement.GetProperty("results");

            // Si no encontro ninguna planta, se lo decimos al usuario
            if (results.GetArrayLength() == 0)
            {
                ShowAlert("Sin resultados", "No se pudo identificar la planta. Probá con otra foto.");
                return;
            }

            // Extraemos los datos de la mejor coincidencia
            var best = results[0].GetProperty("species");
            var scientificName = best.GetProperty("scientificNameWithoutAuthor").GetString();
            var commonNames = best.TryGetProperty("commonNames", out var cn)
                ? string.Join(", ", cn.EnumerateArray().Select(c => c.GetString()))
                : "Sin nombre común";
            var score = results[0].GetProperty("score").GetDouble();

            // Mostramos los resultados en la tarjeta de la pantalla principal
            ResultadoNombreCientifico.Text = scientificName;
            ResultadoNombreComun.Text = commonNames;
            ResultadoScore.Text = $"Coincidencia: {score:P1}";
            ResultadoFrame.IsVisible = true;

            // Y mostramos el overlay de exito con estilo
            SuccessOverlay.IsVisible = true;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            ShowAlert("Planta no identificada", "No se reconoció ninguna planta. Probá con otra foto.");
        }
        catch (Exception ex)
        {
            ShowAlert("Error", $"Ocurrió un error: {ex.Message}");
        }
        finally
        {
            // Restauramos todo al estado inicial
            IdentificarBtn.IsEnabled = true;
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private void OnCerrarSuccess(object? sender, EventArgs e)
    {
        SuccessOverlay.IsVisible = false;
    }

    private async void OnSobreNosotros(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("SobreNosotros");
    }

    private async void OnCerrarSesion(object? sender, EventArgs e)
    {
        // Volvemos a la pantalla de login
        await Shell.Current.GoToAsync("//LoginPage");
    }
}
