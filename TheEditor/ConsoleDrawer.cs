using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace TheEditor
{
    /// <summary>Gravidade de uma mensagem do console.</summary>
    public enum LogLevel
    {
        Info,
        Success,
        Warning,
        Error
    }

    /// <summary>
    /// Gaveta (drawer) na base da janela que mostra o log de ações, o "terminal" do editor.
    /// Recolhida, mostra só a barra de título; ao abrir, desliza para cima. Mensagens que chegam com a
    /// gaveta fechada aparecem como um contador na barra, e erros abrem a gaveta sozinhos.
    /// </summary>
    public class ConsoleDrawer : Panel
    {
        private const int HeaderHeight = 32;
        private const int BodyHeight = 240;
        private const int MaxLines = 600;

        private static readonly Color HeaderColor = Color.FromArgb(0, 150, 215);
        private static readonly Color BodyColor = Color.FromArgb(30, 30, 30);

        private readonly Panel header = new Panel();
        private readonly Label chevron = new Label();
        private readonly Label title = new Label();
        private readonly Label badge = new Label();
        private readonly Label hint = new Label();
        private readonly Button clearButton = new Button();
        private readonly RichTextBox output = new RichTextBox();
        private readonly System.Windows.Forms.Timer animation = new System.Windows.Forms.Timer();

        private int targetHeight = HeaderHeight;
        private int unread;

        /// <summary>Indica se a gaveta está aberta (ou abrindo).</summary>
        public bool IsOpen { get; private set; }

        public ConsoleDrawer()
        {
            Dock = DockStyle.Bottom;
            Height = HeaderHeight;
            BackColor = BodyColor;
            DoubleBuffered = true;

            BuildOutput();
            BuildHeader();

            animation.Interval = 15;
            animation.Tick += (sender, e) => Animate();
        }

        // ------------------------------------------------------------------ construção

        private void BuildOutput()
        {
            output.Dock = DockStyle.Fill;
            output.BorderStyle = BorderStyle.None;
            output.ReadOnly = true;
            output.BackColor = BodyColor;
            output.ForeColor = Color.Gainsboro;
            output.Font = new Font("Consolas", 9.75F);
            output.WordWrap = false;
            output.DetectUrls = false;
            output.ScrollBars = RichTextBoxScrollBars.Both;
            Controls.Add(output);
        }

        private void BuildHeader()
        {
            header.Dock = DockStyle.Top;
            header.Height = HeaderHeight;
            header.BackColor = HeaderColor;
            header.Cursor = Cursors.Hand;

            chevron.AutoSize = false;
            chevron.Size = new Size(28, HeaderHeight);
            chevron.TextAlign = ContentAlignment.MiddleCenter;
            chevron.ForeColor = Color.White;
            chevron.Font = new Font("Segoe UI", 9F);

            title.AutoSize = true;
            title.Text = "Console de Ações";
            title.ForeColor = Color.White;
            title.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);

            badge.AutoSize = true;
            badge.ForeColor = Color.Gold;
            badge.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            hint.AutoSize = true;
            hint.Text = "Ctrl+J";
            hint.ForeColor = Color.FromArgb(200, 235, 250);
            hint.Font = new Font("Segoe UI", 8.5F);

            clearButton.Text = "Limpar";
            clearButton.FlatStyle = FlatStyle.Flat;
            clearButton.FlatAppearance.BorderColor = Color.White;
            clearButton.ForeColor = Color.White;
            clearButton.BackColor = HeaderColor;
            clearButton.Cursor = Cursors.Default;
            clearButton.Size = new Size(64, 22);
            clearButton.Font = new Font("Segoe UI", 8.5F);
            clearButton.Click += (sender, e) => Clear();

            header.Controls.Add(chevron);
            header.Controls.Add(title);
            header.Controls.Add(badge);
            header.Controls.Add(hint);
            header.Controls.Add(clearButton);
            Controls.Add(header);

            foreach (Control clickable in new Control[] { header, chevron, title, badge, hint })
            {
                clickable.Click += (sender, e) => Toggle();
            }

            header.Resize += (sender, e) => LayoutHeader();
            title.SizeChanged += (sender, e) => LayoutHeader();
            badge.SizeChanged += (sender, e) => LayoutHeader();
            UpdateHeader();
            LayoutHeader();
        }

        private void LayoutHeader()
        {
            chevron.Location = new Point(4, 0);
            title.Location = new Point(chevron.Right, (HeaderHeight - title.Height) / 2);
            badge.Location = new Point(title.Right + 10, (HeaderHeight - badge.Height) / 2);
            clearButton.Location = new Point(header.Width - clearButton.Width - 8, (HeaderHeight - clearButton.Height) / 2);
            hint.Location = new Point(clearButton.Left - hint.Width - 12, (HeaderHeight - hint.Height) / 2);
        }

        private void UpdateHeader()
        {
            chevron.Text = IsOpen ? "▼" : "▲";
            badge.Text = (!IsOpen && unread > 0) ? "● " + unread + (unread == 1 ? " nova" : " novas") : "";
        }

        // ------------------------------------------------------------------ abrir e fechar

        /// <summary>Abre a gaveta se estiver fechada, ou fecha se estiver aberta.</summary>
        public void Toggle()
        {
            if (IsOpen) CloseDrawer(); else OpenDrawer();
        }

        /// <summary>Desliza a gaveta para cima e marca as mensagens como lidas.</summary>
        public void OpenDrawer()
        {
            IsOpen = true;
            unread = 0;
            targetHeight = HeaderHeight + BodyHeight;
            UpdateHeader();
            animation.Start();
        }

        /// <summary>Recolhe a gaveta, deixando só a barra de título.</summary>
        public void CloseDrawer()
        {
            IsOpen = false;
            targetHeight = HeaderHeight;
            UpdateHeader();
            animation.Start();
        }

        /// <summary>
        /// Rola até a última linha. Só funciona com a janela já visível: chamar ScrollToCaret
        /// no construtor do form (controle ainda sem pai) derruba o processo com SEHException.
        /// </summary>
        private void ScrollToEnd()
        {
            if (!output.IsHandleCreated || !output.Visible)
            {
                return;
            }

            // WM_VSCROLL / SB_BOTTOM: ScrollToCaret do RichTextBox lança SEHException neste layout
            SendMessage(output.Handle, 0x0115, (IntPtr)7, IntPtr.Zero);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private void Animate()
        {
            int difference = targetHeight - Height;
            if (Math.Abs(difference) <= 2)
            {
                Height = targetHeight;
                animation.Stop();
                ScrollToEnd();
                return;
            }

            int step = (int)Math.Round(difference * 0.3);
            if (step == 0) step = Math.Sign(difference);
            Height += step;
        }

        // ------------------------------------------------------------------ mensagens

        /// <summary>Escreve uma linha com hora, no estilo de um terminal. Pode ser chamado de qualquer thread.</summary>
        public void Log(string message, LogLevel level = LogLevel.Info)
        {
            Append(message, level, true);
        }

        /// <summary>Escreve um bloco de texto como está, sem hora (por exemplo, uma tabela de resultados).</summary>
        public void Write(string text, LogLevel level = LogLevel.Info)
        {
            Append(text, level, false);
        }

        /// <summary>Apaga todas as mensagens.</summary>
        public void Clear()
        {
            output.Clear();
            unread = 0;
            UpdateHeader();
        }

        private void Append(string message, LogLevel level, bool timestamp)
        {
            if (message == null) return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => Append(message, level, timestamp)));
                return;
            }

            foreach (string line in message.Replace("\r\n", "\n").Split('\n'))
            {
                if (timestamp)
                {
                    AppendColored(DateTime.Now.ToString("HH:mm:ss") + "  ", Color.DimGray);
                }
                AppendColored(line + "\n", ColorFor(level));
            }

            TrimOldLines();
            ScrollToEnd();

            if (level == LogLevel.Error)
            {
                OpenDrawer();
            }
            else if (!IsOpen)
            {
                unread++;
                UpdateHeader();
                LayoutHeader();
            }
        }

        private void AppendColored(string text, Color color)
        {
            output.SelectionStart = output.TextLength;
            output.SelectionLength = 0;
            output.SelectionColor = color;
            output.AppendText(text);
        }

        private void TrimOldLines()
        {
            if (output.Lines.Length <= MaxLines) return;

            int cut = output.GetFirstCharIndexFromLine(output.Lines.Length - MaxLines);
            output.Select(0, cut);
            output.SelectedText = "";
        }

        private static Color ColorFor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Success: return Color.LightGreen;
                case LogLevel.Warning: return Color.Khaki;
                case LogLevel.Error: return Color.Salmon;
                default: return Color.Gainsboro;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                animation.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Redireciona Console.WriteLine para a gaveta. Assim as mensagens que o código já escreve
    /// ("Starting image rotation...") passam a aparecer na tela, que antes iam para lugar nenhum
    /// (um aplicativo WinForms não tem janela de console).
    /// </summary>
    public sealed class ConsoleDrawerWriter : TextWriter
    {
        private readonly ConsoleDrawer drawer;
        private readonly StringBuilder line = new StringBuilder();

        public ConsoleDrawerWriter(ConsoleDrawer drawer)
        {
            this.drawer = drawer;
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        public override void Write(char value)
        {
            if (value == '\n')
            {
                Flush();
            }
            else if (value != '\r')
            {
                line.Append(value);
            }
        }

        public override void Write(string value)
        {
            if (value == null) return;
            foreach (char c in value) Write(c);
        }

        public override void WriteLine(string value)
        {
            Write(value);
            Flush();
        }

        public override void Flush()
        {
            if (line.Length == 0) return;
            drawer.Log(line.ToString());
            line.Clear();
        }
    }
}
