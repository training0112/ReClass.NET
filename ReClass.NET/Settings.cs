using System.Drawing;
using System.Text;
using ReClassNET.Util;

namespace ReClassNET
{
	public class Settings
	{
		// Application Settings

		public string LastProcess { get; set; } = string.Empty;

		public bool StayOnTop { get; set; } = false;

		public bool RunAsAdmin { get; set; } = false;

		public bool RandomizeWindowTitle { get; set; } = false;

		// Node Drawing Settings

		public bool ShowNodeAddress { get; set; } = true;

		public bool ShowNodeOffset { get; set; } = true;

		public bool ShowNodeText { get; set; } = true;

		public bool HighlightChangedValues { get; set; } = true;

		public Encoding RawDataEncoding { get; set; } = Encoding.GetEncoding(1252); /* Windows-1252 */

		// Comment Drawing Settings

		public bool ShowCommentFloat { get; set; } = true;

		public bool ShowCommentInteger { get; set; } = true;

		public bool ShowCommentPointer { get; set; } = true;

		public bool ShowCommentRtti { get; set; } = true;

		public bool ShowCommentSymbol { get; set; } = true;

		public bool ShowCommentString { get; set; } = true;

		public bool ShowCommentPluginInfo { get; set; } = true;

		// Colors

		public Color BackgroundColor { get; set; } = Color.FromArgb(30, 30, 30);

		public Color SelectedColor { get; set; } = Color.FromArgb(45, 45, 48);

		public Color HiddenColor { get; set; } = Color.FromArgb(60, 60, 60);

		public Color OffsetColor { get; set; } = Color.FromArgb(255, 99, 99);

		public Color AddressColor { get; set; } = Color.FromArgb(78, 201, 176);

		public Color HexColor { get; set; } = Color.FromArgb(220, 220, 220);

		public Color TypeColor { get; set; } = Color.FromArgb(86, 156, 214);

		public Color NameColor { get; set; } = Color.FromArgb(214, 157, 133);

		public Color ValueColor { get; set; } = Color.FromArgb(181, 206, 168);

		public Color IndexColor { get; set; } = Color.FromArgb(220, 220, 170);

		public Color CommentColor { get; set; } = Color.FromArgb(87, 166, 74);

		public Color TextColor { get; set; } = Color.FromArgb(206, 145, 120);

		public Color VTableColor { get; set; } = Color.FromArgb(78, 201, 176);

		public Color PluginColor { get; set; } = Color.FromArgb(197, 134, 192);

		public CustomDataMap CustomData { get; } = new CustomDataMap();

		public void ApplyDarkTheme()
		{
			BackgroundColor = Color.FromArgb(30, 30, 30);
			SelectedColor = Color.FromArgb(45, 45, 48);
			HiddenColor = Color.FromArgb(60, 60, 60);
			OffsetColor = Color.FromArgb(255, 99, 99);
			AddressColor = Color.FromArgb(78, 201, 176);
			HexColor = Color.FromArgb(220, 220, 220);
			TypeColor = Color.FromArgb(86, 156, 214);
			NameColor = Color.FromArgb(214, 157, 133);
			ValueColor = Color.FromArgb(181, 206, 168);
			IndexColor = Color.FromArgb(220, 220, 170);
			CommentColor = Color.FromArgb(87, 166, 74);
			TextColor = Color.FromArgb(206, 145, 120);
			VTableColor = Color.FromArgb(78, 201, 176);
			PluginColor = Color.FromArgb(197, 134, 192);
		}

		public Settings Clone() => MemberwiseClone() as Settings;
	}
}
