using OnceUponABite.Data;

namespace OnceUponABite.Forms;

public class FrmProdutos : Form
{
    private readonly DataGridView grid = Ui.Grid(20, 235, 990, 405);
    private readonly TextBox txtId = Ui.TextBox(20, 45, 70);
    private readonly TextBox txtNome = Ui.TextBox(130, 45, 280);
    private readonly TextBox txtDescricao = Ui.TextBox(450, 45, 300);
    private readonly NumericUpDown nudPreco = new()
    {
        Location = new Point(130, 115), Size = new Size(150, 27),
        DecimalPlaces = 2, Maximum = 999999, Minimum = 0,
        ThousandsSeparator = true
    };
    private readonly NumericUpDown nudEstoque = new()
    {
        Location = new Point(450, 115), Size = new Size(120, 27),
        Maximum = 1000000, Minimum = 0
    };

    public FrmProdutos()
    {
        Ui.Prepare(this, "Produtos");
        txtId.ReadOnly = true;
        AddField("ID", txtId, 20, 20);
        AddField("Cookie", txtNome, 130, 20);
        AddField("Descrição", txtDescricao, 450, 20);
        Controls.Add(Ui.Label("Preço (R$)", 130, 90, 150));
        Controls.Add(nudPreco);
        Controls.Add(Ui.Label("Estoque", 450, 90, 120));
        Controls.Add(nudEstoque);

        AddButton("Salvar alteração", 20, 170, Update);
        AddButton("Excluir", 165, 170, Delete);
        AddButton("Limpar", 290, 170, Clear);
        AddButton("Atualizar lista", 415, 170, LoadData);
        AddButton("Sair", 540, 170, Close);

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
        var b = Ui.Button(text, x, y, text == "Salvar alteração" ? 135 : 115);
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
        grid.DataSource = Db.Query(
            "SELECT id_produto, nome, descricao, preco, estoque, data_cadastro FROM produtos ORDER BY nome");
        if (grid.Columns.Contains("preco"))
            grid.Columns["preco"].DefaultCellStyle.Format = "C2";
    }

    private void Atualizar()
    {
        if (!int.TryParse(txtId.Text, out int id))
        {
            MessageBox.Show("Selecione um produto na lista.");
            return;
        }

        if (string.IsNullOrWhiteSpace(txtNome.Text))
        {
            MessageBox.Show("O nome do cookie é obrigatório.");
            return;
        }

        Db.Execute(@"UPDATE produtos SET nome=@nome, descricao=@descricao,
            preco=@preco, estoque=@estoque WHERE id_produto=@id",
            Db.P("@nome", txtNome.Text.Trim()),
            Db.P("@descricao", txtDescricao.Text.Trim()),
            Db.P("@preco", nudPreco.Value),
            Db.P("@estoque", (int)nudEstoque.Value),
            Db.P("@id", id));

        MessageBox.Show("Preço e estoque atualizados com sucesso!");
        Clear();
        LoadData();
    }

    private void Delete()
    {
        if (!int.TryParse(txtId.Text, out int id))
        {
            MessageBox.Show("Selecione um produto na lista.");
            return;
        }
        if (!Ui.Confirm("Deseja excluir este cookie do catálogo? O histórico dos pedidos será preservado."))
            return;

        Db.Execute("DELETE FROM produtos WHERE id_produto=@id", Db.P("@id", id));
        MessageBox.Show("Produto excluído com sucesso!");
        Clear();
        LoadData();
    }

    private void SelectRow(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= grid.Rows.Count) return;
        var r = grid.Rows[rowIndex];
        txtId.Text = Convert.ToString(r.Cells["id_produto"].Value);
        txtNome.Text = Convert.ToString(r.Cells["nome"].Value);
        txtDescricao.Text = Convert.ToString(r.Cells["descricao"].Value);
        nudPreco.Value = Math.Max(0, Convert.ToDecimal(r.Cells["preco"].Value));
        nudEstoque.Value = Math.Max(0, Convert.ToDecimal(r.Cells["estoque"].Value));
    }

    private void Clear()
    {
        txtId.Clear();
        txtNome.Clear();
        txtDescricao.Clear();
        nudPreco.Value = 0;
        nudEstoque.Value = 0;
    }
}
