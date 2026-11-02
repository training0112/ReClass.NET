using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace ReClassNET.UI
{
	public sealed class GlobalWindowManagerEventArgs : EventArgs
	{
		public Form Form { get; }

		public GlobalWindowManagerEventArgs(Form form)
		{
			Contract.Requires(form != null);

			Form = form;
		}
	}

	public static class GlobalWindowManager
	{
		private static readonly List<Form> windows = new List<Form>();

		public static Form TopWindow => windows.LastOrDefault();
		public static IEnumerable<Form> Windows => windows;

		public static event EventHandler<GlobalWindowManagerEventArgs> WindowAdded;
		public static event EventHandler<GlobalWindowManagerEventArgs> WindowRemoved;

		public static void AddWindow(Form form)
		{
			Contract.Requires(form != null);

			windows.Add(form);

			form.TopMost = Program.Settings.StayOnTop;

			ApplyDefaultFontToToolStrips(form);

			WindowAdded?.Invoke(null, new GlobalWindowManagerEventArgs(form));
		}

		public static void RemoveWindow(Form form)
		{
			Contract.Requires(form != null);

			if (windows.Remove(form))
			{
				WindowRemoved?.Invoke(null, new GlobalWindowManagerEventArgs(form));
			}
		}

		/// <summary>Tool strips use the system menu font and ignore Application.SetDefaultFont.</summary>
		private static void ApplyDefaultFontToToolStrips(Control control)
		{
			if (control is Form || control is UserControl)
			{
				foreach (var field in control.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
				{
					if (field.GetValue(control) is ToolStrip strip)
					{
						strip.Font = Control.DefaultFont;
					}
				}
			}

			foreach (Control child in control.Controls)
			{
				ApplyDefaultFontToToolStrips(child);
			}
		}
	}
}
