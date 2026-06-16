using Identificador.Views;

namespace Identificador
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("HomePage", typeof(HomePage));
        }
    }
}
