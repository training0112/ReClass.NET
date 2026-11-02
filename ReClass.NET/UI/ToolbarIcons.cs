using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using ReClassNET.Nodes;

namespace ReClassNET.UI
{
	/// <summary>Draws the main toolbar icons at their final size, so they stay sharp instead of being scaled from 16x16 bitmaps.</summary>
	internal static class ToolbarIcons
	{
		private const int LogicalHeight = 24;
		private const int LogicalIconSize = 20;

		private static readonly Color HexColor = Color.FromArgb(70, 70, 70);
		private static readonly Color IntColor = Color.FromArgb(198, 40, 40);
		private static readonly Color UIntColor = Color.FromArgb(21, 101, 192);
		private static readonly Color MiscColor = Color.FromArgb(0, 121, 107);
		private static readonly Color FloatColor = Color.FromArgb(216, 90, 0);
		private static readonly Color VectorColor = Color.FromArgb(46, 125, 50);
		private static readonly Color TextColor = Color.FromArgb(123, 31, 162);
		private static readonly Color ReferenceColor = Color.FromArgb(55, 71, 79);
		private static readonly Color ClassColor = Color.FromArgb(109, 76, 65);
		private static readonly Color FunctionColor = Color.FromArgb(57, 73, 171);

		private static readonly Dictionary<Type, (string Top, string Bottom, Color Color)> nodeLabels = new Dictionary<Type, (string, string, Color)>
		{
			[typeof(Hex64Node)] = ("HEX", "64", HexColor),
			[typeof(Hex32Node)] = ("HEX", "32", HexColor),
			[typeof(Hex16Node)] = ("HEX", "16", HexColor),
			[typeof(Hex8Node)] = ("HEX", "8", HexColor),
			[typeof(NIntNode)] = ("INT", "N", IntColor),
			[typeof(Int64Node)] = ("INT", "64", IntColor),
			[typeof(Int32Node)] = ("INT", "32", IntColor),
			[typeof(Int16Node)] = ("INT", "16", IntColor),
			[typeof(Int8Node)] = ("INT", "8", IntColor),
			[typeof(NUIntNode)] = ("UINT", "N", UIntColor),
			[typeof(UInt64Node)] = ("UINT", "64", UIntColor),
			[typeof(UInt32Node)] = ("UINT", "32", UIntColor),
			[typeof(UInt16Node)] = ("UINT", "16", UIntColor),
			[typeof(UInt8Node)] = ("UINT", "8", UIntColor),
			[typeof(BoolNode)] = ("BOOL", null, MiscColor),
			[typeof(BitFieldNode)] = ("BITS", null, MiscColor),
			[typeof(EnumNode)] = ("ENUM", null, MiscColor),
			[typeof(FloatNode)] = ("FLT", null, FloatColor),
			[typeof(DoubleNode)] = ("DBL", null, FloatColor),
			[typeof(Vector4Node)] = ("VEC", "4", VectorColor),
			[typeof(Vector3Node)] = ("VEC", "3", VectorColor),
			[typeof(Vector2Node)] = ("VEC", "2", VectorColor),
			[typeof(Matrix4x4Node)] = ("MAT", "4x4", VectorColor),
			[typeof(Matrix3x4Node)] = ("MAT", "3x4", VectorColor),
			[typeof(Matrix3x3Node)] = ("MAT", "3x3", VectorColor),
			[typeof(Utf8TextNode)] = ("TXT", null, TextColor),
			[typeof(Utf8TextPtrNode)] = ("TXT", "PTR", TextColor),
			[typeof(Utf16TextNode)] = ("UTXT", null, TextColor),
			[typeof(Utf16TextPtrNode)] = ("UTXT", "PTR", TextColor),
			[typeof(PointerNode)] = ("PTR", null, ReferenceColor),
			[typeof(ArrayNode)] = ("ARY", null, ReferenceColor),
			[typeof(UnionNode)] = ("UNION", null, ReferenceColor),
			[typeof(ClassInstanceNode)] = ("CLS", "INST", ClassColor),
			[typeof(VirtualMethodTableNode)] = ("VTBL", null, FunctionColor),
			[typeof(FunctionNode)] = ("FN", null, FunctionColor),
			[typeof(FunctionPtrNode)] = ("FN", "PTR", FunctionColor)
		};

		/// <summary>Creates the label icon of a node type.</summary>
		/// <returns>The icon or null if the type has no label (for example a plugin node).</returns>
		public static Image ForNodeType(Type nodeType)
		{
			if (!nodeLabels.TryGetValue(nodeType, out var label))
			{
				return null;
			}

			var height = DpiUtil.ScaleIntY(LogicalHeight);
			var fontSize = DpiUtil.ScaleIntY(11);

			using var font = new Font("Segoe UI", fontSize, FontStyle.Bold, GraphicsUnit.Pixel);

			int width;
			using (var bitmap = new Bitmap(1, 1))
			using (var graphics = Graphics.FromImage(bitmap))
			{
				var textWidth = graphics.MeasureString(label.Top, font, PointF.Empty, StringFormat.GenericTypographic).Width;
				if (label.Bottom != null)
				{
					textWidth = Math.Max(textWidth, graphics.MeasureString(label.Bottom, font, PointF.Empty, StringFormat.GenericTypographic).Width);
				}
				width = Math.Max(DpiUtil.ScaleIntX(LogicalIconSize), (int)Math.Ceiling(textWidth) + DpiUtil.ScaleIntX(4));
			}

			var image = new Bitmap(width, height);
			using (var graphics = Graphics.FromImage(image))
			using (var brush = new SolidBrush(label.Color))
			using (var format = new StringFormat(StringFormat.GenericTypographic) { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
			{
				graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

				if (label.Bottom == null)
				{
					graphics.DrawString(label.Top, font, brush, new RectangleF(0, 0, width, height), format);
				}
				else
				{
					var half = height / 2f;
					graphics.DrawString(label.Top, font, brush, new RectangleF(0, 0, width, half + DpiUtil.ScaleIntY(1)), format);
					graphics.DrawString(label.Bottom, font, brush, new RectangleF(0, half - DpiUtil.ScaleIntY(1), width, half + DpiUtil.ScaleIntY(1)), format);
				}
			}
			return image;
		}

		public static Image Search() => Draw(g =>
		{
			using var pen = new Pen(Color.FromArgb(21, 101, 192), 2.2f);
			using var handle = new Pen(Color.FromArgb(21, 101, 192), 3f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
			using var glass = new SolidBrush(Color.FromArgb(200, 227, 242, 253));
			g.FillEllipse(glass, 3f, 3f, 10.5f, 10.5f);
			g.DrawEllipse(pen, 3f, 3f, 10.5f, 10.5f);
			g.DrawLine(handle, 12.5f, 12.5f, 17f, 17f);
		});

		public static Image Folder() => Draw(g =>
		{
			using var outline = new Pen(Color.FromArgb(176, 122, 10), 1f);
			using var back = new SolidBrush(Color.FromArgb(230, 174, 40));
			using var front = new SolidBrush(Color.FromArgb(255, 208, 90));
			var backShape = new[] { new PointF(2.5f, 16.5f), new PointF(2.5f, 4.5f), new PointF(8f, 4.5f), new PointF(10f, 6.5f), new PointF(17.5f, 6.5f), new PointF(17.5f, 16.5f) };
			var frontShape = new[] { new PointF(2.5f, 16.5f), new PointF(5f, 9f), new PointF(19f, 9f), new PointF(17.5f, 16.5f) };
			g.FillPolygon(back, backShape);
			g.DrawPolygon(outline, backShape);
			g.FillPolygon(front, frontShape);
			g.DrawPolygon(outline, frontShape);
		});

		public static Image Save() => Draw(g =>
		{
			using var body = new SolidBrush(Color.FromArgb(21, 101, 192));
			using var paper = new SolidBrush(Color.White);
			using var slot = new SolidBrush(Color.FromArgb(207, 216, 220));
			using var dark = new SolidBrush(Color.FromArgb(13, 71, 161));
			g.FillPolygon(body, new[] { new PointF(2.5f, 2.5f), new PointF(15f, 2.5f), new PointF(17.5f, 5f), new PointF(17.5f, 17.5f), new PointF(2.5f, 17.5f) });
			g.FillRectangle(paper, 5.5f, 3f, 8f, 5.5f);
			g.FillRectangle(dark, 10.5f, 3.5f, 2f, 4f);
			g.FillRectangle(slot, 5f, 11f, 10f, 6.5f);
		});

		public static Image NewClass() => Draw(g =>
		{
			using var body = new SolidBrush(Color.FromArgb(255, 193, 7));
			using var outline = new Pen(Color.FromArgb(176, 122, 10), 1f);
			var block = new[] { new PointF(9f, 2f), new PointF(16f, 5.5f), new PointF(9f, 9f), new PointF(2f, 5.5f) };
			var left = new[] { new PointF(2f, 5.5f), new PointF(9f, 9f), new PointF(9f, 16f), new PointF(2f, 12.5f) };
			var right = new[] { new PointF(16f, 5.5f), new PointF(9f, 9f), new PointF(9f, 16f), new PointF(16f, 12.5f) };
			using var leftBrush = new SolidBrush(Color.FromArgb(240, 160, 0));
			using var rightBrush = new SolidBrush(Color.FromArgb(214, 137, 0));
			g.FillPolygon(body, block);
			g.FillPolygon(leftBrush, left);
			g.FillPolygon(rightBrush, right);
			g.DrawPolygon(outline, block);
			g.DrawPolygon(outline, left);
			g.DrawPolygon(outline, right);
			DrawPlus(g, 14.5f, 14.5f);
		});

		public static Image AddBytes() => Draw(g =>
		{
			DrawByteRows(g, 3);
			DrawPlus(g, 14.5f, 14.5f);
		});

		public static Image InsertBytes() => Draw(g =>
		{
			DrawByteRows(g, 3);
			using var arrow = new SolidBrush(Color.FromArgb(46, 125, 50));
			using var outline = new Pen(Color.White, 1f);
			var shape = new[] { new PointF(10.5f, 9f), new PointF(15f, 9f), new PointF(15f, 6f), new PointF(19.5f, 11.5f), new PointF(15f, 17f), new PointF(15f, 14f), new PointF(10.5f, 14f) };
			g.FillPolygon(arrow, shape);
			g.DrawPolygon(outline, shape);
		});

		private static void DrawByteRows(Graphics g, int rows)
		{
			using var brush = new SolidBrush(Color.FromArgb(84, 110, 122));
			for (var row = 0; row < rows; row++)
			{
				g.FillRectangle(brush, 2f, 3f + row * 5f, 12f, 3f);
			}
		}

		private static void DrawPlus(Graphics g, float x, float y)
		{
			using var background = new SolidBrush(Color.FromArgb(46, 125, 50));
			using var outline = new Pen(Color.White, 1f);
			using var plus = new Pen(Color.White, 1.6f);
			g.FillEllipse(background, x - 5f, y - 5f, 10f, 10f);
			g.DrawEllipse(outline, x - 5f, y - 5f, 10f, 10f);
			g.DrawLine(plus, x - 2.8f, y, x + 2.8f, y);
			g.DrawLine(plus, x, y - 2.8f, x, y + 2.8f);
		}

		private static Image Draw(Action<Graphics> paint)
		{
			var width = DpiUtil.ScaleIntX(LogicalIconSize);
			var height = DpiUtil.ScaleIntY(LogicalIconSize);

			var image = new Bitmap(width, height);
			using (var graphics = Graphics.FromImage(image))
			{
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
				graphics.ScaleTransform(width / (float)LogicalIconSize, height / (float)LogicalIconSize);

				paint(graphics);
			}
			return image;
		}
	}
}
