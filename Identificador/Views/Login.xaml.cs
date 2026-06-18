namespace Identificador.Views;

public partial class Login : ContentPage
{
	private const string UsuarioValido = "wiwi";
	private const string PasswordValido = "1234";
	private const string UsuarioCliente = "cliente";

	public Login()
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		UsuarioEntry.Text = string.Empty;
		PasswordEntry.Text = string.Empty;
		LoginBtn.IsEnabled = true;
		LoadingIndicator.IsRunning = false;
		LoadingIndicator.IsVisible = false;
	}

	private void OnTogglePassword(object? sender, EventArgs e)
	{
		PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
		TogglePasswordBtn.Text = PasswordEntry.IsPassword ? "👁" : "👁‍🗨";
	}

	private async void OnLoginClicked(object? sender, EventArgs e)
	{
		string usuario = UsuarioEntry.Text;
		string password = PasswordEntry.Text;

		if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
		{
			await DisplayAlertAsync("Error", "Completa todos los campos", "OK");
			return;
		}

		bool esValido = (usuario == UsuarioValido || usuario == UsuarioCliente) && password == PasswordValido;

		if (!esValido)
		{
			await DisplayAlertAsync("Error", "Usuario o contraseña incorrectos", "OK");
			return;
		}

		LoginBtn.IsEnabled = false;
		LoadingIndicator.IsRunning = true;
		LoadingIndicator.IsVisible = true;

		await Task.Delay(800);

		await Shell.Current.GoToAsync($"HomePage?usuario={usuario}");
	}
}