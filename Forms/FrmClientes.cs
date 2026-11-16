using OnceUponABite.Data;
using MySqlConnector;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace OnceUponABite.Forms;

public class FrmClientes : Form
{
    private readonly DataGridView grid = Ui.Grid(20, 300, 990, 340);

    private readonly TextBox txtId = Ui.TextBox(20, 45, 70);
    private readonly TextBox txtNome = Ui.TextBox(130, 45, 250);
    private readonly TextBox txtTelefone = Ui.TextBox(420, 45, 180);
    private readonly TextBox txtEmail = Ui.TextBox(640, 45, 300);
    private readonly TextBox txtEndereco = Ui.TextBox(130, 105, 300);
    private readonly TextBox txtNumero = Ui.TextBox(470, 105, 90);
    private readonly TextBox txtComplemento = Ui.TextBox(600, 105, 200);
    private readonly TextBox txtBairro = Ui.TextBox(130, 165, 220);
    private readonly TextBox txtCidade = Ui.TextBox(390, 165, 250);
    private readonly TextBox txtSenha = Ui.TextBox(680, 165, 200);

    // Mostra o resultado da leitura dos arquivos JSON (cadastro / login)
    private readonly Label lblResultado = Ui.Label("", 20, 268, 990);

    public FrmClientes()
    {
        Ui.Prepare(this, "Clientes");

        txtId.ReadOnly = true;
        txtSenha.UseSystemPasswordChar = true;

        AddField("ID", txtId, 20, 20);
        AddField("Nome", txtNome, 130, 20);
        AddField("Telefone", txtTelefone, 420, 20);
        AddField("E-mail", txtEmail, 640, 20);
        AddField("Endereço", txtEndereco, 130, 80);
        AddField("Número", txtNumero, 470, 80);
        AddField("Complemento", txtComplemento, 600, 80);
        AddField("Bairro", txtBairro, 130, 140);
        AddField("Cidade", txtCidade, 390, 140);
        AddField("Senha", txtSenha, 680, 140);

        AddButton("Inserir", 20, 225, Insert);
        AddButton("Alterar", 145, 225, Atualizar);
        AddButton("Excluir", 270, 225, Delete);
        AddButton("Limpar", 395, 225, Clear);
        AddButton("Atualizar lista", 520, 225, LoadData);
        AddButton("Ler arquivo JSON", 645, 225, LerArquivoJson);
        AddButton("Sair", 770, 225, Close);

        lblResultado.AutoSize = false;
        lblResultado.Width = 990;
        lblResultado.Font = new Font(lblResultado.Font, FontStyle.Bold);
        Controls.Add(lblResultado);

        grid.CellClick += (_, e) =>
        {
            if (e.RowIndex >= 0)
                SelectRow(e.RowIndex);
        };

        Controls.Add(grid);

        LoadData();
    }

    private void AddField(string label, TextBox box, int x, int y)
    {
        Controls.Add(Ui.Label(label, x, y, box.Width));
        Controls.Add(box);
    }

    private void AddButton(string text, int x, int y, Action action)
    {
        var button = Ui.Button(text, x, y);
        button.Click += (_, _) => Safe(action);
        Controls.Add(button);
    }

    private void Safe(Action action)
    {
        try
        {
            action();
        }
        catch (JsonException ex)
        {
            MessageBox.Show(
                "O arquivo não é um JSON válido:\n" + ex.Message,
                "Erro no JSON",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
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
            MessageBox.Show(
                "Ocorreu um erro:\n" + ex.Message,
                "Erro",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void LoadData()
    {
        grid.DataSource = null;

        DataTable table = Db.Query(@"
            SELECT
                id_cliente,
                nome,
                telefone,
                email,
                endereco,
                numero,
                complemento,
                bairro,
                cidade,
                senha,
                data_cadastro
            FROM clientes
            ORDER BY nome");

        grid.DataSource = table;
    }

    private void Insert()
    {
        if (string.IsNullOrWhiteSpace(txtNome.Text))
        {
            MessageBox.Show("Informe o nome do cliente.");
            txtNome.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(txtTelefone.Text))
        {
            MessageBox.Show("Informe o telefone do cliente.");
            txtTelefone.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(txtSenha.Text))
        {
            MessageBox.Show("Informe a senha do cliente.");
            txtSenha.Focus();
            return;
        }

        string sql = @"
            INSERT INTO clientes
            (
                nome,
                telefone,
                email,
                endereco,
                numero,
                complemento,
                bairro,
                cidade,
                senha
            )
            VALUES
            (
                @nome,
                @telefone,
                @email,
                @endereco,
                @numero,
                @complemento,
                @bairro,
                @cidade,
                @senha
            )";

        Db.Execute(
            sql,
            Db.P("@nome", txtNome.Text.Trim()),
            Db.P("@telefone", txtTelefone.Text.Trim()),
            Db.P("@email", txtEmail.Text.Trim()),
            Db.P("@endereco", txtEndereco.Text.Trim()),
            Db.P("@numero", txtNumero.Text.Trim()),
            Db.P("@complemento", txtComplemento.Text.Trim()),
            Db.P("@bairro", txtBairro.Text.Trim()),
            Db.P("@cidade", txtCidade.Text.Trim()),
            Db.P("@senha", txtSenha.Text)
        );

        MessageBox.Show("Cliente cadastrado com sucesso!");

        Clear();
        LoadData();
    }

    private void Atualizar()
    {
        if (!int.TryParse(txtId.Text, out int id))
        {
            MessageBox.Show("Selecione um cliente na lista.");
            return;
        }

        if (string.IsNullOrWhiteSpace(txtNome.Text))
        {
            MessageBox.Show("Informe o nome do cliente.");
            txtNome.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(txtTelefone.Text))
        {
            MessageBox.Show("Informe o telefone do cliente.");
            txtTelefone.Focus();
            return;
        }

        string sql = @"
            UPDATE clientes SET
                nome = @nome,
                telefone = @telefone,
                email = @email,
                endereco = @endereco,
                numero = @numero,
                complemento = @complemento,
                bairro = @bairro,
                cidade = @cidade,
                senha = @senha
            WHERE id_cliente = @id";

        int resultado = Db.Execute(
            sql,
            Db.P("@nome", txtNome.Text.Trim()),
            Db.P("@telefone", txtTelefone.Text.Trim()),
            Db.P("@email", txtEmail.Text.Trim()),
            Db.P("@endereco", txtEndereco.Text.Trim()),
            Db.P("@numero", txtNumero.Text.Trim()),
            Db.P("@complemento", txtComplemento.Text.Trim()),
            Db.P("@bairro", txtBairro.Text.Trim()),
            Db.P("@cidade", txtCidade.Text.Trim()),
            Db.P("@senha", txtSenha.Text),
            Db.P("@id", id)
        );

        if (resultado > 0)
            MessageBox.Show("Cliente alterado com sucesso!");
        else
            MessageBox.Show("Nenhum cliente foi alterado.");

        Clear();
        LoadData();
    }

    private void Delete()
    {
        if (!int.TryParse(txtId.Text, out int id))
        {
            MessageBox.Show("Selecione um cliente na lista.");
            return;
        }

        DialogResult confirmacao = MessageBox.Show(
            "Deseja realmente excluir este cliente?",
            "Confirmar exclusão",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (confirmacao != DialogResult.Yes)
            return;

        int resultado = Db.Execute(
            "DELETE FROM clientes WHERE id_cliente = @id",
            Db.P("@id", id)
        );

        if (resultado > 0)
            MessageBox.Show("Cliente excluído com sucesso!");
        else
            MessageBox.Show("Nenhum cliente foi encontrado para excluir.");

        Clear();
        LoadData();
    }

    // ---------------------------------------------------------------
    // Leitura de arquivos JSON (cadastro e login)
    // ---------------------------------------------------------------

    private void LerArquivoJson()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Selecione o arquivo JSON",
            Filter = "Arquivos JSON (*.json)|*.json",
            CheckFileExists = true
        };

        if (dialog.ShowDialog() != DialogResult.OK)
            return;

        string conteudo = File.ReadAllText(dialog.FileName);

        using JsonDocument doc = JsonDocument.Parse(conteudo);
        JsonElement raiz = doc.RootElement;

        if (raiz.ValueKind != JsonValueKind.Object)
        {
            MostrarResultado("JSON inválido: a raiz precisa ser um objeto.", false);
            return;
        }

        // Se tem "nome" é cadastro; se tem "data_login" é login
        if (raiz.TryGetProperty("nome", out _))
        {
            ImportarCadastro(raiz);
        }
        else if (raiz.TryGetProperty("data_login", out _))
        {
            string email = Texto(raiz, "email");
            bool existe = LoginExiste(email);

            MostrarResultado($"Login ({email}): {(existe ? "true" : "false")}", existe);
        }
        else
        {
            MostrarResultado("Formato de JSON não reconhecido.", false);
        }
    }

    private void MostrarResultado(string mensagem, bool sucesso)
    {
        lblResultado.ForeColor = sucesso ? Color.ForestGreen : Color.Firebrick;
        lblResultado.Text = mensagem;
    }

    private static string Texto(JsonElement el, string campo)
    {
        if (!el.TryGetProperty(campo, out JsonElement v))
            return string.Empty;

        return v.ValueKind switch
        {
            JsonValueKind.Null => string.Empty,
            JsonValueKind.String => v.GetString() ?? string.Empty,
            _ => v.GetRawText()   // número, bool etc. viram texto
        };
    }

    private void ImportarCadastro(JsonElement j)
    {
        string nome = Texto(j, "nome").Trim();
        string telefone = Texto(j, "telefone").Trim();
        string email = Texto(j, "email").Trim();

        if (nome == "" || telefone == "" || email == "")
        {
            MostrarResultado("O JSON de cadastro precisa ter nome, telefone e e-mail.", false);
            return;
        }

        if (!DateTime.TryParse(Texto(j, "data_cadastro"), out DateTime dataCadastro))
            dataCadastro = DateTime.Now;

        string sql = @"
            INSERT INTO clientes
                (nome, telefone, email, endereco, numero, complemento, bairro, cidade, senha, data_cadastro)
            VALUES
                (@nome, @telefone, @email, @endereco, @numero, @complemento, @bairro, @cidade, @senha, @data)";

        Db.Execute(
            sql,
            Db.P("@nome", nome),
            Db.P("@telefone", telefone),
            Db.P("@email", email),
            Db.P("@endereco", Texto(j, "endereco")),
            Db.P("@numero", Texto(j, "numero")),
            Db.P("@complemento", Texto(j, "complemento")),
            Db.P("@bairro", Texto(j, "bairro")),
            Db.P("@cidade", Texto(j, "cidade")),
            Db.P("@senha", Texto(j, "senha")),
            Db.P("@data", dataCadastro)
        );

        MostrarResultado($"Cadastro importado com sucesso: {nome} ({email})", true);
        LoadData();
    }

    private bool LoginExiste(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        DataTable t = Db.Query(
            "SELECT 1 FROM clientes WHERE email = @email LIMIT 1",
            Db.P("@email", email.Trim()));

        return t.Rows.Count > 0;
    }

    // ---------------------------------------------------------------

    private string GetCellValue(DataGridViewRow row, string column)
    {
        object value = row.Cells[column].Value;

        if (value == null || value == DBNull.Value)
            return string.Empty;

        return Convert.ToString(value) ?? string.Empty;
    }

    private void SelectRow(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= grid.Rows.Count)
            return;

        DataGridViewRow row = grid.Rows[rowIndex];

        if (row.IsNewRow)
            return;

        txtId.Text = GetCellValue(row, "id_cliente");
        txtNome.Text = GetCellValue(row, "nome");
        txtTelefone.Text = GetCellValue(row, "telefone");
        txtEmail.Text = GetCellValue(row, "email");
        txtEndereco.Text = GetCellValue(row, "endereco");
        txtNumero.Text = GetCellValue(row, "numero");
        txtComplemento.Text = GetCellValue(row, "complemento");
        txtBairro.Text = GetCellValue(row, "bairro");
        txtCidade.Text = GetCellValue(row, "cidade");
        txtSenha.Text = GetCellValue(row, "senha");
    }

    private void Clear()
    {
        txtId.Clear();
        txtNome.Clear();
        txtTelefone.Clear();
        txtEmail.Clear();
        txtEndereco.Clear();
        txtNumero.Clear();
        txtComplemento.Clear();
        txtBairro.Clear();
        txtCidade.Clear();
        txtSenha.Clear();

        grid.ClearSelection();
        txtNome.Focus();
    }
}