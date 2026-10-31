using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TheEditor
{
    // Tema escuro aplicado em código sobre os controles criados pelo Designer.
    public partial class MainWindow
    {
        private static readonly Color WindowColor = Color.FromArgb(30, 31, 36);
        private static readonly Color PanelColor = Color.FromArgb(37, 38, 45);
        private static readonly Color CanvasColor = Color.FromArgb(22, 23, 27);
        private static readonly Color ButtonColor = Color.FromArgb(52, 54, 64);
        private static readonly Color ButtonHoverColor = Color.FromArgb(66, 69, 82);
        private static readonly Color AccentColor = Color.FromArgb(0, 151, 215);
        private static readonly Color TextColor = Color.FromArgb(232, 232, 236);

        // textos em português para os botões criados pelo Designer
        private static readonly Dictionary<string, string> ButtonTexts = new Dictionary<string, string>
        {
            { "Reset Image to Default", "Restaurar original" },
            { "Rotate Image 90 Degrees", "Girar 90°" },
            { "Checar Equalização", "Equalização" },
            { "Mirror H", "Espelhar ↔" },
            { "Mirror V", "Espelhar ↕" },
            { "Apply Reduce Noise", "Reduzir ruído" },
        };

        private void ApplyTheme()
        {
            Font = new Font("Segoe UI", 9F);
            BackColor = WindowColor;
            ForeColor = TextColor;

            StyleControls(Controls);
            button5.Text = "Bordas (Roberts)";

            imageCanvas.BackColor = CanvasColor;
            imageCanvas.BorderStyle = BorderStyle.None;

            MenuTop.Renderer = new ToolStripProfessionalRenderer(new DarkColorTable());
            MenuTop.BackColor = PanelColor;
            MenuTop.ForeColor = TextColor;
            foreach (ToolStripItem item in MenuTop.Items)
            {
                item.ForeColor = TextColor;
            }

            ToolTip tips = new ToolTip();
            tips.SetToolTip(button1, "Gira a imagem 90° no sentido horário");
            tips.SetToolTip(button2, "Volta para a imagem original");
            tips.SetToolTip(button3, "Espelha de cima para baixo");
            tips.SetToolTip(button4, "Espelha da esquerda para a direita");
            tips.SetToolTip(button5, "Detecção de bordas pelo gradiente cruzado de Roberts");
            tips.SetToolTip(CheckEqualization, "Mostra no console o histograma e a equalização (16 níveis)");
            tips.SetToolTip(trackBar2, "Quanto mais à direita, mais a imagem vai para preto e branco");
        }

        private void StyleControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                if (control == imageCanvas || control == consoleDrawer)
                {
                    continue;
                }

                if (control is MenuStrip)
                {
                    continue;
                }

                if (control is Button button)
                {
                    StyleButton(button);
                }
                else if (control is Label label)
                {
                    label.BorderStyle = BorderStyle.None;
                    label.BackColor = Color.Transparent;
                    label.Font = new Font(Font, FontStyle.Bold);
                    label.ForeColor = TextColor;
                }
                else if (control is TextBox box)
                {
                    box.BorderStyle = BorderStyle.None;
                    box.BackColor = PanelColor;
                    box.ForeColor = TextColor;
                    box.Font = new Font(Font, FontStyle.Bold);
                }
                else if (control is TrackBar)
                {
                    control.BackColor = PanelColor;
                }
                else
                {
                    control.BackColor = PanelColor;
                    control.ForeColor = TextColor;
                }

                StyleControls(control.Controls);
            }
        }

        private void StyleButton(Button button)
        {
            string translated;
            if (ButtonTexts.TryGetValue(button.Text.Replace("\r\n", " ").Replace("\n", " "), out translated))
            {
                button.Text = translated;
            }

            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = ButtonHoverColor;
            button.FlatAppearance.MouseDownBackColor = AccentColor;
            button.BackColor = ButtonColor;
            button.ForeColor = TextColor;
            button.UseVisualStyleBackColor = false;
            button.Cursor = Cursors.Hand;
        }

        /// <summary>Mostra uma dica no canvas enquanto não há imagem carregada.</summary>
        private void DrawCanvasPlaceholder(Graphics g, Rectangle area)
        {
            const string text = "Arquivo > Importar imagem\npara começar";
            using (Font font = new Font("Segoe UI", 14F))
            using (Brush brush = new SolidBrush(Color.FromArgb(110, 112, 124)))
            using (StringFormat format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.DrawString(text, font, brush, area, format);
            }
        }

        private class DarkColorTable : ProfessionalColorTable
        {
            public override Color MenuItemSelected { get { return Color.FromArgb(60, 63, 76); } }
            public override Color MenuItemSelectedGradientBegin { get { return Color.FromArgb(60, 63, 76); } }
            public override Color MenuItemSelectedGradientEnd { get { return Color.FromArgb(60, 63, 76); } }
            public override Color MenuItemBorder { get { return Color.FromArgb(60, 63, 76); } }
            public override Color MenuBorder { get { return Color.FromArgb(60, 63, 76); } }
            public override Color MenuItemPressedGradientBegin { get { return Color.FromArgb(37, 38, 45); } }
            public override Color MenuItemPressedGradientEnd { get { return Color.FromArgb(37, 38, 45); } }
            public override Color ToolStripDropDownBackground { get { return Color.FromArgb(37, 38, 45); } }
            public override Color ImageMarginGradientBegin { get { return Color.FromArgb(37, 38, 45); } }
            public override Color ImageMarginGradientMiddle { get { return Color.FromArgb(37, 38, 45); } }
            public override Color ImageMarginGradientEnd { get { return Color.FromArgb(37, 38, 45); } }
            public override Color MenuStripGradientBegin { get { return Color.FromArgb(37, 38, 45); } }
            public override Color MenuStripGradientEnd { get { return Color.FromArgb(37, 38, 45); } }
        }
    }
}
