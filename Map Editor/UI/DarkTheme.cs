using System;
using System.Drawing;
using System.Windows.Forms;

namespace Map_Editor.UI
{
    /// <summary>
    /// Modern Dark Theme Manager for Crystal Map Editor
    /// Provides a consistent dark color scheme throughout the application
    /// </summary>
    public static class DarkTheme
    {
        // Primary color palette for dark theme
        public static readonly Color PrimaryBackground = Color.FromArgb(30, 30, 30);      // Main background
        public static readonly Color SecondaryBackground = Color.FromArgb(45, 45, 48);    // Secondary panels
        public static readonly Color TertiaryBackground = Color.FromArgb(37, 37, 38);     // Tertiary elements
        
        public static readonly Color PrimaryText = Color.FromArgb(241, 241, 241);         // Main text
        public static readonly Color SecondaryText = Color.FromArgb(204, 204, 204);       // Secondary text
        public static readonly Color DisabledText = Color.FromArgb(109, 109, 109);        // Disabled text
        
        public static readonly Color AccentBlue = Color.FromArgb(0, 122, 204);            // Accent color
        public static readonly Color AccentBlueHover = Color.FromArgb(28, 151, 234);      // Hover accent
        public static readonly Color AccentBluePressed = Color.FromArgb(0, 84, 153);      // Pressed accent
        
        public static readonly Color BorderColor = Color.FromArgb(63, 63, 70);            // Borders
        public static readonly Color BorderColorFocused = Color.FromArgb(0, 122, 204);    // Focused borders
        
        public static readonly Color ButtonBackground = Color.FromArgb(62, 62, 66);       // Button background
        public static readonly Color ButtonHover = Color.FromArgb(80, 80, 80);           // Button hover
        public static readonly Color ButtonPressed = Color.FromArgb(40, 40, 40);         // Button pressed
        
        public static readonly Color GridLines = Color.FromArgb(60, 60, 60);             // Grid lines
        public static readonly Color SelectionColor = Color.FromArgb(51, 153, 255);      // Selection color

        /// <summary>
        /// Applies dark theme to a Form and all its child controls
        /// </summary>
        public static void ApplyToForm(Form form)
        {
            if (form == null) return;

            form.BackColor = PrimaryBackground;
            form.ForeColor = PrimaryText;

            ApplyToControls(form.Controls);
        }

        /// <summary>
        /// Applies dark theme to a collection of controls recursively
        /// </summary>
        public static void ApplyToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                ApplyToControl(control);
                
                // Recursively apply to child controls
                if (control.HasChildren)
                {
                    ApplyToControls(control.Controls);
                }
            }
        }

        /// <summary>
        /// Applies dark theme to a specific control
        /// </summary>
        public static void ApplyToControl(Control control)
        {
            if (control == null) return;

            switch (control)
            {
                case Form form:
                    ApplyToForm(form);
                    break;

                case TabControl tabControl:
                    ApplyToTabControl(tabControl);
                    break;

                case TabPage tabPage:
                    ApplyToTabPage(tabPage);
                    break;

                case Button button:
                    ApplyToButton(button);
                    break;

                case Panel panel:
                    ApplyToPanel(panel);
                    break;

                case GroupBox groupBox:
                    ApplyToGroupBox(groupBox);
                    break;

                case ListBox listBox:
                    ApplyToListBox(listBox);
                    break;

                case ListView listView:
                    ApplyToListView(listView);
                    break;

                case TreeView treeView:
                    ApplyToTreeView(treeView);
                    break;

                case TextBox textBox:
                    ApplyToTextBox(textBox);
                    break;

                case ComboBox comboBox:
                    ApplyToComboBox(comboBox);
                    break;

                case Label label:
                    ApplyToLabel(label);
                    break;

                case ToolStrip toolStrip:
                    ApplyToToolStrip(toolStrip);
                    break;

                case MenuStrip menuStrip:
                    ApplyToMenuStrip(menuStrip);
                    break;

                case StatusStrip statusStrip:
                    ApplyToStatusStrip(statusStrip);
                    break;

                case SplitContainer splitContainer:
                    ApplyToSplitContainer(splitContainer);
                    break;

                case TableLayoutPanel tableLayout:
                    ApplyToTableLayoutPanel(tableLayout);
                    break;

                default:
                    // Apply basic styling to unknown controls
                    control.BackColor = SecondaryBackground;
                    control.ForeColor = PrimaryText;
                    break;
            }
        }

        private static void ApplyToTabControl(TabControl tabControl)
        {
            tabControl.BackColor = PrimaryBackground;
            tabControl.ForeColor = PrimaryText;
        }

        private static void ApplyToTabPage(TabPage tabPage)
        {
            tabPage.BackColor = SecondaryBackground;
            tabPage.ForeColor = PrimaryText;
            tabPage.UseVisualStyleBackColor = false;
        }

        private static void ApplyToButton(Button button)
        {
            button.BackColor = ButtonBackground;
            button.ForeColor = PrimaryText;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = BorderColor;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.MouseOverBackColor = ButtonHover;
            button.FlatAppearance.MouseDownBackColor = ButtonPressed;
            button.UseVisualStyleBackColor = false;
        }

        private static void ApplyToPanel(Panel panel)
        {
            // Keep MapPanel transparent for DirectX rendering
            if (panel.Name == "MapPanel")
            {
                panel.BackColor = Color.Transparent;
            }
            else
            {
                panel.BackColor = SecondaryBackground;
                panel.ForeColor = PrimaryText;
            }
        }

        private static void ApplyToGroupBox(GroupBox groupBox)
        {
            groupBox.BackColor = SecondaryBackground;
            groupBox.ForeColor = PrimaryText;
        }

        private static void ApplyToListBox(ListBox listBox)
        {
            listBox.BackColor = TertiaryBackground;
            listBox.ForeColor = PrimaryText;
            listBox.BorderStyle = BorderStyle.FixedSingle;
        }

        private static void ApplyToListView(ListView listView)
        {
            listView.BackColor = TertiaryBackground;
            listView.ForeColor = PrimaryText;
            listView.BorderStyle = BorderStyle.FixedSingle;
        }

        private static void ApplyToTreeView(TreeView treeView)
        {
            treeView.BackColor = TertiaryBackground;
            treeView.ForeColor = PrimaryText;
            treeView.BorderStyle = BorderStyle.FixedSingle;
            treeView.LineColor = GridLines;
        }

        private static void ApplyToTextBox(TextBox textBox)
        {
            textBox.BackColor = TertiaryBackground;
            textBox.ForeColor = PrimaryText;
            textBox.BorderStyle = BorderStyle.FixedSingle;
        }

        private static void ApplyToComboBox(ComboBox comboBox)
        {
            comboBox.BackColor = TertiaryBackground;
            comboBox.ForeColor = PrimaryText;
            comboBox.FlatStyle = FlatStyle.Flat;
        }

        private static void ApplyToLabel(Label label)
        {
            // Keep transparent labels for overlays
            if (label.BackColor == Color.Transparent)
            {
                label.ForeColor = PrimaryText;
            }
            else
            {
                label.BackColor = SecondaryBackground;
                label.ForeColor = PrimaryText;
            }
        }

        private static void ApplyToToolStrip(ToolStrip toolStrip)
        {
            toolStrip.BackColor = PrimaryBackground;
            toolStrip.ForeColor = PrimaryText;
            toolStrip.Renderer = new DarkToolStripRenderer();
        }

        private static void ApplyToMenuStrip(MenuStrip menuStrip)
        {
            menuStrip.BackColor = PrimaryBackground;
            menuStrip.ForeColor = PrimaryText;
            menuStrip.Renderer = new DarkToolStripRenderer();
        }

        private static void ApplyToStatusStrip(StatusStrip statusStrip)
        {
            statusStrip.BackColor = PrimaryBackground;
            statusStrip.ForeColor = PrimaryText;
            statusStrip.Renderer = new DarkToolStripRenderer();
        }

        private static void ApplyToSplitContainer(SplitContainer splitContainer)
        {
            splitContainer.BackColor = BorderColor;
            splitContainer.Panel1.BackColor = SecondaryBackground;
            splitContainer.Panel2.BackColor = SecondaryBackground;
        }

        private static void ApplyToTableLayoutPanel(TableLayoutPanel tableLayout)
        {
            tableLayout.BackColor = SecondaryBackground;
            tableLayout.ForeColor = PrimaryText;
        }
    }

    /// <summary>
    /// Custom ToolStrip renderer for dark theme
    /// </summary>
    public class DarkToolStripRenderer : ToolStripProfessionalRenderer
    {
        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using (var brush = new SolidBrush(DarkTheme.PrimaryBackground))
            {
                e.Graphics.FillRectangle(brush, e.AffectedBounds);
            }
        }

        protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
        {
            var button = e.Item;
            var bounds = new Rectangle(Point.Empty, e.Item.Size);

            Color backgroundColor = DarkTheme.ButtonBackground;
            
            if (button.Pressed)
                backgroundColor = DarkTheme.ButtonPressed;
            else if (button.Selected)
                backgroundColor = DarkTheme.ButtonHover;

            using (var brush = new SolidBrush(backgroundColor))
            {
                e.Graphics.FillRectangle(brush, bounds);
            }

            if (button.Selected)
            {
                using (var pen = new Pen(DarkTheme.BorderColorFocused))
                {
                    bounds.Width -= 1;
                    bounds.Height -= 1;
                    e.Graphics.DrawRectangle(pen, bounds);
                }
            }
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = DarkTheme.PrimaryText;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            var bounds = new Rectangle(Point.Empty, e.Item.Size);
            
            if (e.Item.Selected)
            {
                using (var brush = new SolidBrush(DarkTheme.ButtonHover))
                {
                    e.Graphics.FillRectangle(brush, bounds);
                }
            }
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            var bounds = e.Item.Bounds;
            var startPoint = new Point(bounds.Left, bounds.Height / 2);
            var endPoint = new Point(bounds.Right, bounds.Height / 2);

            using (var pen = new Pen(DarkTheme.BorderColor))
            {
                e.Graphics.DrawLine(pen, startPoint, endPoint);
            }
        }
    }
}