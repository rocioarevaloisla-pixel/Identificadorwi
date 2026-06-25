using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Identificador.Services;

/// <summary>
/// Servicio de autenticación con Firebase Identity Toolkit.
/// Permite iniciar sesión, registrarse y actualizar el perfil del usuario.
/// </summary>
public class FirebaseAuthService
{
    private readonly HttpClient _httpClient;
    private const string ApiKey = "AIzaSyCM54KIRL2SFGpV5fo6zreiAwzvH3imgNY";

    public FirebaseAuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Inicia sesión con email y contraseña usando la API de Firebase.
    /// </summary>
    /// <param name="email">Correo electrónico del usuario.</param>
    /// <param name="password">Contraseña del usuario.</param>
    /// <returns>Resultado de la autenticación con los datos del usuario o un mensaje de error.</returns>
    public async Task<AuthResult> SignInWithEmailAsync(string email, string password)
    {
        var payload = new
        {
            email,
            password,
            returnSecureToken = true
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={ApiKey}",
            payload);

        var json = await response.Content.ReadFromJsonAsync<FirebaseSignInResponse>();

        if (!response.IsSuccessStatusCode)
        {
            var error = json?.Error?.Message switch
            {
                "EMAIL_NOT_FOUND" => "Email no registrado",
                "INVALID_PASSWORD" => "Contraseña incorrecta",
                "INVALID_LOGIN_CREDENTIALS" => "Email o contraseña incorrectos",
                "USER_DISABLED" => "Usuario deshabilitado",
                _ => json?.Error?.Message ?? "Error de autenticación"
            };
            return AuthResult.Failure(error);
        }

        return AuthResult.Success(json!.LocalId, json.Email, json.DisplayName ?? json.Email, json.IdToken);
    }

    /// <summary>
    /// Registra un nuevo usuario con email, contraseña y nombre visible.
    /// </summary>
    /// <param name="email">Correo electrónico del nuevo usuario.</param>
    /// <param name="password">Contraseña del nuevo usuario.</param>
    /// <param name="displayName">Nombre visible del perfil.</param>
    /// <returns>Resultado del registro con los datos del usuario o un mensaje de error.</returns>
    public async Task<AuthResult> SignUpWithEmailAsync(string email, string password, string displayName)
    {
        var payload = new
        {
            email,
            password,
            returnSecureToken = true
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={ApiKey}",
            payload);

        var json = await response.Content.ReadFromJsonAsync<FirebaseSignInResponse>();

        if (!response.IsSuccessStatusCode)
        {
            var error = json?.Error?.Message switch
            {
                "EMAIL_EXISTS" => "Este email ya está registrado",
                "WEAK_PASSWORD" => "La contraseña debe tener al menos 6 caracteres",
                _ => json?.Error?.Message ?? "Error al registrarse"
            };
            return AuthResult.Failure(error);
        }

        await UpdateProfileAsync(json!.IdToken, displayName);

        return AuthResult.Success(json.LocalId, json.Email, displayName, json.IdToken);
    }

    /// <summary>
    /// Actualiza el nombre visible del perfil en Firebase.
    /// </summary>
    /// <param name="idToken">Token de ID del usuario autenticado.</param>
    /// <param name="displayName">Nuevo nombre visible.</param>
    private async Task UpdateProfileAsync(string idToken, string displayName)
    {
        var payload = new
        {
            idToken,
            displayName,
            returnSecureToken = true
        };

        await _httpClient.PostAsJsonAsync(
            $"https://identitytoolkit.googleapis.com/v1/accounts:update?key={ApiKey}",
            payload);
    }
}

/// <summary>
/// Resultado de una operación de autenticación.
/// Contiene el estado (éxito/fallo), datos del usuario y mensaje de error.
/// </summary>
public class AuthResult
{
    public bool IsSuccess { get; private set; }
    public string UserId { get; private set; }
    public string Email { get; private set; }
    public string DisplayName { get; private set; }
    public string Token { get; private set; }
    public string ErrorMessage { get; private set; }

    public static AuthResult Success(string userId, string email, string displayName, string token) =>
        new() { IsSuccess = true, UserId = userId, Email = email, DisplayName = displayName, Token = token };

    public static AuthResult Failure(string error) =>
        new() { IsSuccess = false, ErrorMessage = error };
}

/// <summary>
/// Respuesta de la API de Firebase al iniciar sesión o registrarse.
/// Incluye los campos devueltos por el endpoint de Identity Toolkit.
/// </summary>
public class FirebaseSignInResponse
{
    [JsonPropertyName("localId")]
    public string LocalId { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("idToken")]
    public string IdToken { get; set; }

    [JsonPropertyName("error")]
    public FirebaseError? Error { get; set; }
}

/// <summary>
/// Error devuelto por Firebase en la respuesta de autenticación.
/// </summary>
public class FirebaseError
{
    [JsonPropertyName("message")]
    public string Message { get; set; }
}
