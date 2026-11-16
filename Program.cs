using OnceUponABite.Forms;

namespace OnceUponABite;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        using (var login = new FrmLogin())
        {
            // Se o login for fechado ou cancelado, o sistema não abre
            if (login.ShowDialog() != DialogResult.OK)
                return;
        }

        Application.Run(new FrmPrincipal());
    }
}
