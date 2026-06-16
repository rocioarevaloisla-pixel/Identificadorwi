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
}
