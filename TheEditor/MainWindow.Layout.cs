using System;
using System.Drawing;
using System.Windows.Forms;

namespace TheEditor
{
    // Layout da janela feito em código: a janela passa a ser redimensionável, a área da imagem ocupa o
    // espaço que sobra, e o console antigo (coluna da direita) vira uma gaveta recolhível na base.
    public partial class MainWindow
    {
        private ConsoleDrawer consoleDrawer;

        private const int ToolsColumnWidth = 340;

        private void ConfigureLayout()
        {
            SuspendLayout();

            // a janela deixa de ter tamanho fixo
            AutoSize = false;
            AutoScroll = false;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimumSize = new Size(900, 600);
            ClientSize = new Size(1180, 720);
            StartPosition = FormStartPosition.CenterScreen;

            // área principal: ferramentas à esquerda (largura fixa) e a imagem ocupando o resto
            tableLayoutPanel4.SuspendLayout();
            tableLayoutPanel4.Controls.Remove(tableLayoutPanel1);   // console antigo, substituído pela gaveta
            tableLayoutPanel4.AutoSize = false;
            tableLayoutPanel4.Dock = DockStyle.Fill;
            tableLayoutPanel4.ColumnStyles.Clear();
            tableLayoutPanel4.RowStyles.Clear();
            tableLayoutPanel4.ColumnCount = 2;
            tableLayoutPanel4.RowCount = 1;
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, ToolsColumnWidth));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            tableLayoutPanel6.AutoSize = false;
            tableLayoutPanel6.Dock = DockStyle.Fill;
            foreach (Control group in new Control[] { tableLayoutPanel5, tableLayoutPanel7, tableLayoutPanel9, tableLayoutPanel3 })
            {
                group.Dock = DockStyle.Fill;   // cada grupo ocupa a própria linha, sem sobra entre eles
            }

            // grupos de ferramentas compactos, em alturas fixas, em vez de dividir o espaço sobrando em porcentagens
            tableLayoutPanel6.RowStyles.Clear();
            tableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
            tableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));
            tableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Absolute, 82F));
            tableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Absolute, 159F));
            tableLayoutPanel6.RowCount = 5;
            tableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            imageCanvas.AutoSize = false;
            imageCanvas.Dock = DockStyle.Fill;
            imageCanvas.Margin = new Padding(6);
            tableLayoutPanel4.ResumeLayout(true);

            ResumeLayout(true);
        }

        private void SetupConsoleDrawer()
        {
            consoleDrawer = new ConsoleDrawer();

            // adicionada depois da área principal: o WinForms encaixa primeiro a gaveta (base) e dá o resto à área principal
            Controls.Add(consoleDrawer);

            // tudo que o código escreve com Console.WriteLine aparece na gaveta
            Console.SetOut(new ConsoleDrawerWriter(consoleDrawer));

            // menu Exibir > Console de Ações (Ctrl+J), como o painel do VS Code
            ToolStripMenuItem viewMenu = new ToolStripMenuItem("Exibir");
            ToolStripMenuItem consoleItem = new ToolStripMenuItem("Console de Ações");
            consoleItem.ShortcutKeys = Keys.Control | Keys.J;
            consoleItem.Click += (sender, e) => consoleDrawer.Toggle();
            viewMenu.DropDownItems.Add(consoleItem);
            MenuTop.Items.Add(viewMenu);

            consoleDrawer.Log("Editor pronto. Use Arquivo > Importar imagem para começar.");
        }

        /// <summary>Confere se há uma imagem carregada; se não houver, avisa na gaveta em vez de falhar.</summary>
        private bool RequireImage()
        {
            if (imageHandle != null)
            {
                return true;
            }

            consoleDrawer.Log("Importe uma imagem antes (Arquivo > Importar imagem).", LogLevel.Warning);
            consoleDrawer.OpenDrawer();
            return false;
        }
    }
}
