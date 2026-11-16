using System.Drawing;

namespace OnceUponABite.Forms;

internal static class Ui
{
    public static readonly Color Brown = Color.FromArgb(104, 64, 45);
    public static readonly Color Cream = Color.FromArgb(255, 248, 238);

    public static void Prepare(Form form, string title, int width = 1050, int height = 700)
    {
        form.Text = $"Once Upon A Bite | {title}";
        form.StartPosition = FormStartPosition.CenterScreen;
        form.Size = new Size(width, height);
        form.MinimumSize = new Size(850, 600);
        form.BackColor = Cream;
        form.Font = new Font("Segoe UI", 9F);
    }

    public static Label Label(string text, int x, int y, int width = 120) =>
        new() { Text = text, Location = new Point(x, y), Size = new Size(width, 25), TextAlign = ContentAlignment.MiddleLeft };

    public static TextBox TextBox(int x, int y, int width = 220) =>
        new() { Location = new Point(x, y), Size = new Size(width, 27) };

    public static Button Button(string text, int x, int y, int width = 115) =>
        new()
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(width, 34),
            BackColor = Brown,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };

    public static DataGridView Grid(int x, int y, int width, int height) =>
        new()
        {
            Location = new Point(x, y),
            Size = new Size(width, height),
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            RowHeadersVisible = false
        };

    public static void Error(Exception ex) =>
        MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);

    public static bool Confirm(string message) =>
        MessageBox.Show(message, "Confirmação", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
}
