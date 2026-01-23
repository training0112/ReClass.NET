using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace ReClassNET.UI
{
	public static class Theme
	{
		public static readonly Color Background = Color.FromArgb(30, 30, 30);
		public static readonly Color Foreground = Color.FromArgb(220, 220, 220);
		public static readonly Color ControlBackground = Color.FromArgb(37, 37, 38);
		public static readonly Color ControlBorder = Color.FromArgb(62, 62, 64);
		public static readonly Color MenuHoverBackground = Color.FromArgb(62, 62, 64);
		public static readonly Color MenuPressedBackground = Color.FromArgb(51, 51, 55);

		public static void ApplyDarkTheme(Control control)
		{
			if (control == null)
			{
				return;
			}

			ApplyColors(control);

			foreach (Control child in control.Controls)
			{
				ApplyDarkTheme(child);
			}
		}

		public static void ApplyDarkThemeToContextMenus(Form form)
		{
			if (form == null)
			{
				return;
			}

			var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			foreach (var field in form.GetType().GetFields(flags))
			{
				if (field.GetValue(form) is ToolStrip strip)
				{
					ApplyToolStrip(strip);
				}
			}
		}

		private static void ApplyColors(Control control)
		{
			switch (control)
			{
				case Form form:
					form.BackColor = Background;
					form.ForeColor = Foreground;
					break;
					case TabPage _:
					case Panel _:
					case GroupBox _:
					case SplitContainer _:
					control.BackColor = Background;
					control.ForeColor = Foreground;
					break;
				case TextBoxBase _:
					control.BackColor = ControlBackground;
					control.ForeColor = Foreground;
					break;
				case ButtonBase _:
					control.BackColor = ControlBackground;
					control.ForeColor = Foreground;
					break;
				case ListView _:
				case TreeView _:
					control.BackColor = ControlBackground;
					control.ForeColor = Foreground;
					break;
				case DataGridView grid:
					grid.BackgroundColor = Background;
					grid.ForeColor = Foreground;
					grid.EnableHeadersVisualStyles = false;
					grid.ColumnHeadersDefaultCellStyle.BackColor = ControlBackground;
					grid.ColumnHeadersDefaultCellStyle.ForeColor = Foreground;
					grid.DefaultCellStyle.BackColor = ControlBackground;
					grid.DefaultCellStyle.ForeColor = Foreground;
					grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(51, 51, 55);
					grid.DefaultCellStyle.SelectionForeColor = Foreground;
					grid.GridColor = ControlBorder;
					break;
				case ToolStrip strip:
					ApplyToolStrip(strip);
					break;
				default:
					control.BackColor = Background;
					control.ForeColor = Foreground;
					break;
			}
		}

		private static void ApplyToolStrip(ToolStrip strip)
		{
			strip.BackColor = Background;
			strip.ForeColor = Foreground;
			strip.Renderer = new DarkToolStripRenderer();

			if (strip is ToolStripDropDown dropDown)
			{
				dropDown.BackColor = Background;
				dropDown.ForeColor = Foreground;
			}

			foreach (ToolStripItem item in strip.Items)
			{
				item.BackColor = Background;
				item.ForeColor = Foreground;

				if (item is ToolStripDropDownItem dropDownItem)
				{
					ApplyToolStrip(dropDownItem.DropDown);
				}
			}
		}
	}

	internal sealed class DarkToolStripRenderer : ToolStripProfessionalRenderer
	{
		public DarkToolStripRenderer()
			: base(new DarkColorTable())
		{
		}

		protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
		{
			e.TextColor = Theme.Foreground;
			base.OnRenderItemText(e);
		}
	}

	internal sealed class DarkColorTable : ProfessionalColorTable
	{
		public override Color MenuBorder => Theme.ControlBorder;
		public override Color ToolStripBorder => Theme.ControlBorder;
		public override Color MenuItemBorder => Theme.ControlBorder;
		public override Color MenuItemSelected => Theme.MenuHoverBackground;
		public override Color MenuItemSelectedGradientBegin => Theme.MenuHoverBackground;
		public override Color MenuItemSelectedGradientEnd => Theme.MenuHoverBackground;
		public override Color MenuItemPressedGradientBegin => Theme.MenuPressedBackground;
		public override Color MenuItemPressedGradientMiddle => Theme.MenuPressedBackground;
		public override Color MenuItemPressedGradientEnd => Theme.MenuPressedBackground;
		public override Color ToolStripDropDownBackground => Theme.Background;
		public override Color ImageMarginGradientBegin => Theme.Background;
		public override Color ImageMarginGradientMiddle => Theme.Background;
		public override Color ImageMarginGradientEnd => Theme.Background;
	}
}
