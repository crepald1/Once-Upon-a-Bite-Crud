namespace OnceUponABite.Forms;

public class FrmPrincipal : Form
{
    // Painel com todo o conteúdo do menu, mantido sempre centralizado na tela
    private readonly Panel conteudo = new()
    {
        Size = new Size(760, 500),
        BackColor = Color.Transparent
    };

    public FrmPrincipal()
    {
        Ui.Prepare(this, "Sistema de gerenciamento", 760, 500);

        // Abre ocupando a tela inteira
        WindowState = FormWindowState.Maximized;

        var title = new Label
        {
            Text = "Once Upon A Bite",
            Font = new Font("Georgia", 27, FontStyle.Bold),
            ForeColor = Ui.Brown,
            AutoSize = true,
            Location = new Point(230, 45)
        };
        conteudo.Controls.Add(title);

        var subtitle = new Label
        {
            Text = "Sistema de gerenciamento de pedidos e cookies",
            AutoSize = true,
            Location = new Point(245, 105),
            Font = new Font("Segoe UI", 11)
        };
        conteudo.Controls.Add(subtitle);

        AddMenuButton("Clientes", 140, 190, () => new FrmClientes().ShowDialog(this));
        AddMenuButton("Entregadores", 430, 190, () => new FrmEntregadores().ShowDialog(this));
        AddMenuButton("Produtos", 140, 260, () => new FrmProdutos().ShowDialog(this));
        AddMenuButton("Pedidos", 430, 260, () => new FrmPedidos().ShowDialog(this));
        AddMenuButton("Sair", 285, 330, Close);

        var footer = new Label
        {
            Text = "Once Upon A Bite • Gestão da loja",
            AutoSize = true,
            ForeColor = Color.DimGray,
            Location = new Point(320, 400)
        };
        conteudo.Controls.Add(footer);

        Controls.Add(conteudo);

        // Recentraliza o conteúdo sempre que a janela mudar de tamanho
        Resize += (_, _) => CentralizarConteudo();
        Load += (_, _) => CentralizarConteudo();
    }

    private void CentralizarConteudo()
    {
        conteudo.Left = Math.Max(0, (ClientSize.Width - conteudo.Width) / 2);
        conteudo.Top = Math.Max(0, (ClientSize.Height - conteudo.Height) / 2);
    }

    private void AddMenuButton(string text, int x, int y, Action action)
    {
        var button = Ui.Button(text, x, y, 250);
        button.Height = 48;
        button.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        button.Click += (_, _) => action();
        conteudo.Controls.Add(button);
    }
}
