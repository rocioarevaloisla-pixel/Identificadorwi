using Identificador.Services;

namespace Identificador.Views;

public partial class Login : ContentPage
{
	private readonly FirebaseAuthService _authService;
	private bool _esRegistro;

	public Login()
	{
		InitializeComponent();
		_authService = new FirebaseAuthService(new HttpClient { Timeout = TimeSpan.FromSeconds(30) });
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		UsuarioEntry.Text = string.Empty;
		PasswordEntry.Text = string.Empty;
		NombreEntry.Text = string.Empty;
		LoginBtn.IsEnabled = true;
		LoadingIndicator.IsRunning = false;
		LoadingIndicator.IsVisible = false;
		_esRegistro = false;
		MostrarModoLogin();
	}

	private void OnTogglePassword(object? sender, EventArgs e)
	{
		PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
		TogglePasswordBtn.Text = PasswordEntry.IsPassword ? "👁" : "👁‍🗨";
	}

	private void OnToggleMode(object? sender, EventArgs e)
	{
		_esRegistro = !_esRegistro;
		if (_esRegistro)
			MostrarModoRegistro();
		else
			MostrarModoLogin();
	}

	private void MostrarModoLogin()
	{
		TituloLabel.Text = "Inicia sesión";
		LoginBtn.Text = "Ingresar";
		NombreLabel.IsVisible = false;
		NombreBorder.IsVisible = false;
		ToggleModeBtn.Text = "¿No tienes cuenta? Regístrate";
	}

	private void MostrarModoRegistro()
	{
		TituloLabel.Text = "Crear cuenta";
		LoginBtn.Text = "Registrarse";
		NombreLabel.IsVisible = true;
		NombreBorder.IsVisible = true;
		ToggleModeBtn.Text = "¿Ya tienes cuenta? Inicia sesión";
	}

	private async void OnLoginClicked(object? sender, EventArgs e)
	{
		string email = UsuarioEntry.Text?.Trim();
		string password = PasswordEntry.Text;

		if (_esRegistro)
		{
			await RegistrarAsync(email, password);
			return;
		}

		if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
		{
			await DisplayAlertAsync("Error", "Completa todos los campos", "OK");
			return;
		}

		LoginBtn.IsEnabled = false;
		LoadingIndicator.IsRunning = true;
		LoadingIndicator.IsVisible = true;

		var result = await _authService.SignInWithEmailAsync(email, password);

		if (!result.IsSuccess)
		{
			await DisplayAlertAsync("Error", result.ErrorMessage, "OK");
			LoginBtn.IsEnabled = true;
			LoadingIndicator.IsRunning = false;
			LoadingIndicator.IsVisible = false;
			return;
		}

		await Shell.Current.GoToAsync($"HomePage?usuario={Uri.EscapeDataString(result.DisplayName)}");
	}

	private async Task RegistrarAsync(string email, string password)
	{
		string nombre = NombreEntry.Text?.Trim();

		if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(nombre))
		{
			await DisplayAlertAsync("Error", "Completa todos los campos", "OK");
			return;
		}

		LoginBtn.IsEnabled = false;
		LoadingIndicator.IsRunning = true;
		LoadingIndicator.IsVisible = true;

		var result = await _authService.SignUpWithEmailAsync(email, password, nombre);

		if (!result.IsSuccess)
		{
			await DisplayAlertAsync("Error", result.ErrorMessage, "OK");
			LoginBtn.IsEnabled = true;
			LoadingIndicator.IsRunning = false;
			LoadingIndicator.IsVisible = false;
			return;
		}

		await DisplayAlertAsync("Cuenta creada", $"Bienvenido {nombre}, ahora puedes iniciar sesión.", "OK");
		_esRegistro = false;
		MostrarModoLogin();
		LoginBtn.IsEnabled = true;
		LoadingIndicator.IsRunning = false;
		LoadingIndicator.IsVisible = false;
	}
}