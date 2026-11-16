using OnceUponABite.Data;
using System.Data;
using System.Text.Json;

namespace OnceUponABite.Forms;

public class FrmPedidos : Form
{
    private readonly DataGridView gridPedidos = Ui.Grid(20, 55, 990, 270);
    private readonly DataGridView gridItens = Ui.Grid(20, 410, 990, 220);
    private readonly ComboBox cboStatus = new()
    {
        Location = new Point(130, 355),
        Size = new Size(220, 27),
        DropDownStyle = ComboBoxStyle.DropDownList
    };
    private readonly Label lblPedido = new()
    {
        Location = new Point(20, 330),
        Size = new Size(100, 25),
        Text = "Pedido: —",
        Font = new Font("Segoe UI", 10, FontStyle.Bold)
    };

    // Item do JSON já validado, pronto para gravar
    private record ItemPedido(string Nome, int Quantidade, decimal Preco, decimal Subtotal);

    public FrmPedidos()
    {
        Ui.Prepare(this, "Pedidos");
        cboStatus.Items.AddRange(new object[]
        {
            "Pendente", "Em preparo", "Pronto",
            "Saiu para entrega", "Entregue", "Cancelado"
        });

        Controls.Add(Ui.Label("Pedidos cadastrados", 20, 20, 250));

        var btnLerJson = Ui.Button("Ler arquivo JSON", 860, 15, 150);
        btnLerJson.Click += (_, _) => Safe(LerArquivoJson);
        Controls.Add(btnLerJson);

        Controls.Add(gridPedidos);
        Controls.Add(lblPedido);
        Controls.Add(Ui.Label("Novo status", 130, 330, 100));
        Controls.Add(cboStatus);

        var btnStatus = Ui.Button("Alterar status", 375, 350, 130);
        btnStatus.Click += (_, _) => Safe(UpdateStatus);
        Controls.Add(btnStatus);

        var btnExcluir = Ui.Button("Excluir pedido", 520, 350, 130);
        btnExcluir.Click += (_, _) => Safe(Delete);
        Controls.Add(btnExcluir);

        var btnAtualizar = Ui.Button("Atualizar lista", 665, 350, 130);
        btnAtualizar.Click += (_, _) => Safe(LoadPedidos);
        Controls.Add(btnAtualizar);

        var btnSair = Ui.Button("Sair", 810, 350, 130);
        btnSair.Click += (_, _) => Safe(Close);
        Controls.Add(btnSair);

        Controls.Add(Ui.Label("Itens do pedido selecionado", 20, 385, 300));

        gridPedidos.CellClick += (_, e) => Safe(() => SelectPedido(e.RowIndex));
        Controls.Add(gridItens);

        LoadPedidos();
    }

    private void Safe(Action action)
    {
        try { action(); }
        catch (Exception ex) { Ui.Error(ex); }
    }

    private int? SelectedPedidoId()
    {
        if (gridPedidos.CurrentRow == null) return null;
        var value = gridPedidos.CurrentRow.Cells["id_pedido"].Value;
        return value == null ? null : Convert.ToInt32(value);
    }

    private void LoadPedidos()
    {
        gridPedidos.DataSource = Db.Query(@"
            SELECT
                p.id_pedido,
                c.nome AS cliente,
                e.nome AS entregador,
                p.data_pedido,
                p.status,
                p.valor_total,
                p.observacao
            FROM pedidos p
            LEFT JOIN clientes c ON c.id_cliente = p.id_cliente
            LEFT JOIN entregadores e ON e.id_entregador = p.id_entregador
            ORDER BY p.data_pedido DESC");

        if (gridPedidos.Columns.Contains("valor_total"))
            gridPedidos.Columns["valor_total"].DefaultCellStyle.Format = "C2";

        gridItens.DataSource = null;
        lblPedido.Text = "Pedido: —";
    }

    private void SelectPedido(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= gridPedidos.Rows.Count) return;

        var row = gridPedidos.Rows[rowIndex];
        int id = Convert.ToInt32(row.Cells["id_pedido"].Value);

        lblPedido.Text = $"Pedido: {id}";
        cboStatus.Text = Convert.ToString(row.Cells["status"].Value);

        gridItens.DataSource = Db.Query(@"
            SELECT
                nome_produto,
                quantidade,
                preco_unitario,
                valor_total
            FROM pedido_itens
            WHERE id_pedido = @id
            ORDER BY id_item",
            Db.P("@id", id));

        if (gridItens.Columns.Contains("preco_unitario"))
            gridItens.Columns["preco_unitario"].DefaultCellStyle.Format = "C2";
        if (gridItens.Columns.Contains("valor_total"))
            gridItens.Columns["valor_total"].DefaultCellStyle.Format = "C2";
    }

    private void UpdateStatus()
    {
        int? id = SelectedPedidoId();
        if (id == null)
        {
            MessageBox.Show("Selecione um pedido.");
            return;
        }
        if (string.IsNullOrWhiteSpace(cboStatus.Text))
        {
            MessageBox.Show("Selecione o novo status.");
            return;
        }

        Db.Execute("UPDATE pedidos SET status=@status WHERE id_pedido=@id",
            Db.P("@status", cboStatus.Text),
            Db.P("@id", id.Value));

        MessageBox.Show("Status do pedido atualizado!");
        LoadPedidos();
    }

    private void InitializeComponent()
    {

    }

    private void Delete()
    {
        int? id = SelectedPedidoId();
        if (id == null)
        {
            MessageBox.Show("Selecione um pedido.");
            return;
        }
        if (!Ui.Confirm($"Deseja excluir o pedido {id}? Os itens desse pedido também serão excluídos."))
            return;

        Db.Execute("DELETE FROM pedidos WHERE id_pedido=@id", Db.P("@id", id.Value));
        MessageBox.Show("Pedido excluído com sucesso!");
        LoadPedidos();
    }

    // ---------------------------------------------------------------
    // Importação do JSON de pedido (insere o pedido e dá baixa no estoque)
    // ---------------------------------------------------------------

    private void LerArquivoJson()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Selecione o arquivo JSON do pedido",
            Filter = "Arquivos JSON (*.json)|*.json",
            CheckFileExists = true
        };

        if (dialog.ShowDialog() != DialogResult.OK)
            return;

        using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(dialog.FileName));
        ImportarPedido(doc.RootElement);
    }

    private static string Texto(JsonElement el, string campo)
    {
        if (!el.TryGetProperty(campo, out JsonElement v))
            return string.Empty;

        return v.ValueKind switch
        {
            JsonValueKind.Null => string.Empty,
            JsonValueKind.String => v.GetString() ?? string.Empty,
            _ => v.GetRawText()
        };
    }

    private static decimal Numero(JsonElement el, string campo)
    {
        return el.TryGetProperty(campo, out JsonElement v) && v.ValueKind == JsonValueKind.Number
            ? v.GetDecimal()
            : 0m;
    }

    private void ImportarPedido(JsonElement raiz)
    {
        if (raiz.ValueKind != JsonValueKind.Object
            || !raiz.TryGetProperty("itens", out JsonElement itensJson)
            || itensJson.ValueKind != JsonValueKind.Array
            || itensJson.GetArrayLength() == 0)
        {
            MessageBox.Show("JSON de pedido inválido: o pedido não tem itens.");
            return;
        }

        // 1) Lê os itens do JSON
        var itens = new List<ItemPedido>();

        foreach (JsonElement it in itensJson.EnumerateArray())
        {
            string nome = Texto(it, "nome").Trim();
            int quantidade = (int)Numero(it, "quantidade");
            decimal preco = Numero(it, "preco_unitario");
            decimal subtotal = Numero(it, "subtotal");

            if (nome == "" || quantidade <= 0)
            {
                MessageBox.Show("Item inválido no JSON (nome vazio ou quantidade menor que 1).");
                return;
            }

            if (subtotal == 0)
                subtotal = preco * quantidade;

            itens.Add(new ItemPedido(nome, quantidade, preco, subtotal));
        }

        // 2) Valida o estoque ANTES de gravar qualquer coisa.
        //    O id do JSON é texto ("cookie-pink-dream"), então o produto é
        //    localizado pelo nome. Se o mesmo cookie aparecer em mais de
        //    uma linha, as quantidades são somadas.
        var baixas = new List<(int IdProduto, string Nome, int Quantidade)>();

        var porProduto = itens
            .GroupBy(i => i.Nome, StringComparer.OrdinalIgnoreCase)
            .Select(g => (Nome: g.Key, Quantidade: g.Sum(i => i.Quantidade)));

        foreach (var (nome, quantidade) in porProduto)
        {
            DataTable prod = Db.Query(
                "SELECT id_produto, estoque FROM produtos WHERE nome = @nome OR nome = CONCAT('Cookie ', @nome) LIMIT 1",
                Db.P("@nome", nome));

            if (prod.Rows.Count == 0)
            {
                MessageBox.Show($"Produto não encontrado no catálogo:\n{nome}\n\nNada foi gravado.");
                return;
            }

            int idProduto = Convert.ToInt32(prod.Rows[0]["id_produto"]);
            int estoque = Convert.ToInt32(prod.Rows[0]["estoque"]);

            if (estoque < quantidade)
            {
                MessageBox.Show(
                    $"Estoque insuficiente para \"{nome}\".\nEm estoque: {estoque} | Pedido: {quantidade}\n\nNada foi gravado.");
                return;
            }

            baixas.Add((idProduto, nome, quantidade));
        }

        decimal total = Numero(raiz, "valor_total");
        if (total == 0)
            total = itens.Sum(i => i.Subtotal);

        string status = Texto(raiz, "status");
        if (string.IsNullOrWhiteSpace(status))
            status = "Pendente";

        // A data vem em UTC ("...Z"); converte para o horário local
        DateTime dataPedido = DateTimeOffset.TryParse(Texto(raiz, "data_pedido"), out DateTimeOffset dto)
            ? dto.LocalDateTime
            : DateTime.Now;

        // 3) INSERT em pedidos (id_pedido é gerado pelo banco).
        //    O SELECT LAST_INSERT_ID() vai no mesmo comando para
        //    garantir que é o id deste insert.
        DataTable novo = Db.Query(@"
            INSERT INTO pedidos (data_pedido, status, valor_total, observacao)
            VALUES (@data, @status, @total, @obs);
            SELECT LAST_INSERT_ID();",
            Db.P("@data", dataPedido),
            Db.P("@status", status),
            Db.P("@total", total),
            Db.P("@obs", Texto(raiz, "observacao")));

        int idPedido = Convert.ToInt32(novo.Rows[0][0]);

        // 4) INSERT dos itens do pedido
        //    A coluna "subtotal" é GERADA pelo MySQL (quantidade x preco_unitario),
        //    então ela NÃO entra no INSERT.
        foreach (ItemPedido item in itens)
        {
            Db.Execute(@"
                INSERT INTO pedido_itens
                    (id_pedido, nome_produto, quantidade, preco_unitario)
                VALUES
                    (@pedido, @nome, @qtd, @preco)",
                Db.P("@pedido", idPedido),
                Db.P("@nome", item.Nome),
                Db.P("@qtd", item.Quantidade),
                Db.P("@preco", item.Preco));
        }

        // 5) UPDATE em produtos: baixa o estoque
        foreach (var baixa in baixas)
        {
            Db.Execute(@"
                UPDATE produtos
                SET estoque = estoque - @qtd
                WHERE id_produto = @id",
                Db.P("@qtd", baixa.Quantidade),
                Db.P("@id", baixa.IdProduto));
        }

        MessageBox.Show(
            $"Pedido {idPedido} importado com sucesso!\n" +
            $"{itens.Count} item(ns) | Total: {total:C2}\n" +
            "Estoque dos produtos atualizado.");

        LoadPedidos();
    }
}