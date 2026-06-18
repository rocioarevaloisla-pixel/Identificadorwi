namespace Identificador.Views;

public partial class SobreNosotros : ContentPage
{
    public SobreNosotros()
    {
        InitializeComponent();
    }

    private async void OnVolver(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
