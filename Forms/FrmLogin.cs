using MySqlConnector;
using OnceUponABite.Data;
using System.Data;
using System.Security.Cryptography;

namespace OnceUponABite.Forms;

public class FrmLogin : Form
{
    private const int LarguraJanela = 380;
    private const int X = 30;
    private const int W = 320;

    private readonly Label lblTitulo = new()
    {
        Text = "Funcionário",
        Font = new Font("Georgia", 18, FontStyle.Bold),
        ForeColor = Ui.Brown,
        AutoSize = false,
        TextAlign = ContentAlignment.MiddleCenter,
        Location = new Point(0, 15),
        Size = new Size(LarguraJanela, 40)
    };

    private readonly Label lblNome = Ui.Label("Nome", X, 0, W);
    private readonly TextBox txtNome = Ui.TextBox(X, 0, W);
    private readonly Label lblUsuario = Ui.Label("Usuário", X, 0, W);
    private readonly TextBox txtUsuario = Ui.TextBox(X, 0, W);
    private readonly Label lblSenha = Ui.Label("Senha", X, 0, W);
    private readonly TextBox txtSenha = Ui.TextBox(X, 0, W);
    private readonly Label lblConfirmar = Ui.Label("Confirmar senha", X, 0, W);
    private readonly TextBox txtConfirmar = Ui.TextBox(X, 0, W);

    private readonly Button btnAcao = Ui.Button("Entrar", X, 0, W);
    private readonly LinkLabel lnkModo = new()
    {
        AutoSize = false,
        TextAlign = ContentAlignment.MiddleCenter,
        Size = new Size(W, 25),
        Left = X
    };

    private bool modoCadastro;

    /// <summary>Nome do funcionário autenticado (preenchido após login com sucesso).</summary>
    public string FuncionarioNome { get; private set; } = string.Empty;

    public FrmLogin()
    {
        Ui.Prepare(this, "Login", LarguraJanela, 400);

        // Ui.Prepare define um tamanho mínimo de 850x600, que deixaria a janela grande
        MinimumSize = Size.Empty;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        txtSenha.UseSystemPasswordChar = true;
        txtConfirmar.UseSystemPasswordChar = true;
        txtUsuario.MaxLength = 50;
        txtNome.MaxLength = 100;
        btnAcao.Height = 38;

        Controls.AddRange(new Control[]
        {
            lblTitulo, lblNome, txtNome, lblUsuario, txtUsuario,
            lblSenha, txtSenha, lblConfirmar, txtConfirmar, btnAcao, lnkModo
        });

        btnAcao.Click += (_, _) => Safe(modoCadastro ? Cadastrar : Entrar);
        lnkModo.LinkClicked += (_, _) =>
        {
            modoCadastro = !modoCadastro;
            AplicarModo();
        };

        AcceptButton = btnAcao;

        AplicarModo();
    }

    // ---------------------------------------------------------------
    // Layout / modo
    // ---------------------------------------------------------------

    private void AplicarModo()
    {
        lblNome.Visible = txtNome.Visible = modoCadastro;
        lblConfirmar.Visible = txtConfirmar.Visible = modoCadastro;

        int y = 70;

        if (modoCadastro)
            Place(lblNome, txtNome, ref y);

        Place(lblUsuario, txtUsuario, ref y);
        Place(lblSenha, txtSenha, ref y);

        if (modoCadastro)
            Place(lblConfirmar, txtConfirmar, ref y);

        btnAcao.Top = y + 5;
        lnkModo.Top = btnAcao.Bottom + 10;

        lblTitulo.Text = modoCadastro ? "Novo funcionário" : "Funcionário";
        btnAcao.Text = modoCadastro ? "Cadastrar" : "Entrar";
        lnkModo.Text = modoCadastro
            ? "Já tenho conta — voltar ao login"
            : "Não tem conta? Cadastre-se";

        ClientSize = new Size(LarguraJanela, lnkModo.Bottom + 25);

        txtSenha.Clear();
        txtConfirmar.Clear();
        (modoCadastro ? txtNome : txtUsuario).Focus();
    }

    private static void Place(Label label, TextBox box, ref int y)
    {
        label.Top = y;
        box.Top = y + 25;
        y += 60;
    }

    // ---------------------------------------------------------------
    // Ações
    // ---------------------------------------------------------------

    private void Safe(Action action)
    {
        try
        {
            action();
        }
        catch (MySqlException ex)
        {
            MessageBox.Show(
                "Erro no banco de dados:\n" + ex.Message,
                "Erro MySQL",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            Ui.Error(ex);
        }
    }

    private void Entrar()
    {
        string usuario = txtUsuario.Text.Trim();

        if (usuario == "" || txtSenha.Text == "")
        {
            MessageBox.Show("Informe usuário e senha.");
            return;
        }

        DataTable t = Db.Query(
            "SELECT nome, senha_hash FROM funcionarios WHERE usuario = @usuario LIMIT 1",
            Db.P("@usuario", usuario));

        // Mesma mensagem para usuário inexistente e senha errada
        if (t.Rows.Count == 0 ||
            !VerificarSenha(txtSenha.Text, Convert.ToString(t.Rows[0]["senha_hash"]) ?? ""))
        {
            MessageBox.Show("Usuário ou senha inválidos.", "Login",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtSenha.Clear();
            txtSenha.Focus();
            return;
        }

        FuncionarioNome = Convert.ToString(t.Rows[0]["nome"]) ?? "";
        DialogResult = DialogResult.OK;
        Close();
    }

    private void Cadastrar()
    {
        string nome = txtNome.Text.Trim();
        string usuario = txtUsuario.Text.Trim();

        if (nome == "")
        {
            MessageBox.Show("Informe o nome.");
            txtNome.Focus();
            return;
        }

        if (usuario.Length < 3 || usuario.Contains(' '))
        {
            MessageBox.Show("O usuário precisa ter pelo menos 3 caracteres e não pode ter espaços.");
            txtUsuario.Focus();
            return;
        }

        if (txtSenha.Text.Length < 6)
        {
            MessageBox.Show("A senha precisa ter pelo menos 6 caracteres.");
            txtSenha.Focus();
            return;
        }

        if (txtSenha.Text != txtConfirmar.Text)
        {
            MessageBox.Show("As senhas não conferem.");
            txtConfirmar.Focus();
            return;
        }

        DataTable existe = Db.Query(
            "SELECT 1 FROM funcionarios WHERE usuario = @usuario LIMIT 1",
            Db.P("@usuario", usuario));

        if (existe.Rows.Count > 0)
        {
            MessageBox.Show("Este usuário já está em uso. Escolha outro.");
            txtUsuario.Focus();
            return;
        }

        Db.Execute(
            "INSERT INTO funcionarios (nome, usuario, senha_hash) VALUES (@nome, @usuario, @hash)",
            Db.P("@nome", nome),
            Db.P("@usuario", usuario),
            Db.P("@hash", GerarHash(txtSenha.Text)));

        MessageBox.Show("Funcionário cadastrado com sucesso! Faça login para continuar.");

        modoCadastro = false;
        AplicarModo();
        txtSenha.Focus();
    }

    // ---------------------------------------------------------------
    // Hash de senha (PBKDF2 + salt) — a senha nunca é salva em texto puro
    // Formato salvo: iteracoes.salt(base64).hash(base64)
    // ---------------------------------------------------------------

    private const int Iteracoes = 100_000;

    private static string GerarHash(string senha)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            senha, salt, Iteracoes, HashAlgorithmName.SHA256, 32);

        return $"{Iteracoes}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    private static bool VerificarSenha(string senha, string armazenado)
    {
        string[] partes = armazenado.Split('.');
        if (partes.Length != 3 || !int.TryParse(partes[0], out int iteracoes))
            return false;

        byte[] salt = Convert.FromBase64String(partes[1]);
        byte[] esperado = Convert.FromBase64String(partes[2]);
        byte[] calculado = Rfc2898DeriveBytes.Pbkdf2(
            senha, salt, iteracoes, HashAlgorithmName.SHA256, esperado.Length);

        return CryptographicOperations.FixedTimeEquals(calculado, esperado);
    }
}
