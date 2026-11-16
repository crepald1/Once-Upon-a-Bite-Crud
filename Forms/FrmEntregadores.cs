using OnceUponABite.Data;

namespace OnceUponABite.Forms;

public class FrmEntregadores : Form
{
    private readonly DataGridView grid = Ui.Grid(20, 255, 990, 385);
    private readonly TextBox txtId = Ui.TextBox(20, 45, 70);
    private readonly TextBox txtNome = Ui.TextBox(130, 45, 250);
    private readonly TextBox txtTelefone = Ui.TextBox(420, 45, 180);
    private readonly TextBox txtVeiculo = Ui.TextBox(640, 45, 180);
    private readonly TextBox txtPlaca = Ui.TextBox(130, 110, 150);
    private readonly ComboBox cboDisponibilidade = new()
    {
        Location = new Point(420, 110), Size = new Size(180, 27),
        DropDownStyle = ComboBoxStyle.DropDownList
    };

    public FrmEntregadores()
    {
        Ui.Prepare(this, "Entregadores");
        txtId.ReadOnly = true;
        cboDisponibilidade.Items.AddRange(new object[] { "Disponível", "Indisponível" });
        cboDisponibilidade.SelectedIndex = 0;

        AddField("ID", txtId, 20, 20);
        AddField("Nome", txtNome, 130, 20);
        AddField("Telefone", txtTelefone, 420, 20);
        AddField("Tipo de veículo", txtVeiculo, 640, 20);
        AddField("Placa", txtPlaca, 130, 85);
        Controls.Add(Ui.Label("Disponibilidade", 420, 85, 180));
        Controls.Add(cboDisponibilidade);

        AddButton("Inserir", 20, 175, Insert);
        AddButton("Alterar", 145, 175, Update);
        AddButton("Excluir", 270, 175, Delete);
        AddButton("Limpar", 395, 175, Clear);
        AddButton("Atualizar lista", 520, 175, LoadData);
        AddButton("Sair", 645, 175, Close);

        grid.CellClick += (_, e) => SelectRow(e.RowIndex);
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
        var b = Ui.Button(text, x, y);
        b.Click += (_, _) => Safe(action);
        Controls.Add(b);
    }

    private void Safe(Action action)
    {
        try { action(); }
        catch (Exception ex) { Ui.Error(ex); }
    }

    private void LoadData()
    {
        grid.DataSource = Db.Query(@"SELECT id_entregador, nome, telefone, tipo_veiculo,
            placa, disponibilidade, data_cadastro FROM entregadores ORDER BY nome");
    }

    private void Insert()
    {
        if (string.IsNullOrWhiteSpace(txtNome.Text) || string.IsNullOrWhiteSpace(txtTelefone.Text))
        {
            MessageBox.Show("Informe nome e telefone.");
            return;
        }

        Db.Execute(@"INSERT INTO entregadores
            (nome, telefone, tipo_veiculo, placa, disponibilidade)
            VALUES (@nome,@telefone,@veiculo,@placa,@disp)",
            Db.P("@nome", txtNome.Text.Trim()),
            Db.P("@telefone", txtTelefone.Text.Trim()),
            Db.P("@veiculo", txtVeiculo.Text.Trim()),
            Db.P("@placa", txtPlaca.Text.Trim()),
            Db.P("@disp", cboDisponibilidade.Text));

        MessageBox.Show("Entregador cadastrado com sucesso!");
        Clear();
        LoadData();
    }

    private void Atualizar()
    {
        if (!int.TryParse(txtId.Text, out int id))
        {
            MessageBox.Show("Selecione um entregador na lista.");
            return;
        }

        Db.Execute(@"UPDATE entregadores SET nome=@nome, telefone=@telefone,
            tipo_veiculo=@veiculo, placa=@placa, disponibilidade=@disp
            WHERE id_entregador=@id",
            Db.P("@nome", txtNome.Text.Trim()),
            Db.P("@telefone", txtTelefone.Text.Trim()),
            Db.P("@veiculo", txtVeiculo.Text.Trim()),
            Db.P("@placa", txtPlaca.Text.Trim()),
            Db.P("@disp", cboDisponibilidade.Text),
            Db.P("@id", id));

        MessageBox.Show("Entregador alterado com sucesso!");
        Clear();
        LoadData();
    }

    private void Delete()
    {
        if (!int.TryParse(txtId.Text, out int id))
        {
            MessageBox.Show("Selecione um entregador na lista.");
            return;
        }
        if (!Ui.Confirm("Deseja excluir este entregador? Os pedidos serão mantidos sem entregador vinculado."))
            return;

        Db.Execute("DELETE FROM entregadores WHERE id_entregador=@id", Db.P("@id", id));
        MessageBox.Show("Entregador excluído com sucesso!");
        Clear();
        LoadData();
    }

    private void SelectRow(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= grid.Rows.Count) return;
        var r = grid.Rows[rowIndex];
        txtId.Text = Convert.ToString(r.Cells["id_entregador"].Value);
        txtNome.Text = Convert.ToString(r.Cells["nome"].Value);
        txtTelefone.Text = Convert.ToString(r.Cells["telefone"].Value);
        txtVeiculo.Text = Convert.ToString(r.Cells["tipo_veiculo"].Value);
        txtPlaca.Text = Convert.ToString(r.Cells["placa"].Value);
        cboDisponibilidade.Text = Convert.ToString(r.Cells["disponibilidade"].Value);
    }

    private void Clear()
    {
        txtId.Clear();
        txtNome.Clear();
        txtTelefone.Clear();
        txtVeiculo.Clear();
        txtPlaca.Clear();
        cboDisponibilidade.SelectedIndex = 0;
    }
}
