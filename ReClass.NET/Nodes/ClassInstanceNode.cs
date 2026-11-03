using System;
using System.Drawing;
using ReClassNET.Controls;
using ReClassNET.UI;

namespace ReClassNET.Nodes
{
	public class ClassInstanceNode : BaseClassWrapperNode
	{
		public override int MemorySize => InnerNode.MemorySize;

		protected override bool PerformCycleCheck => true;

		public override void GetUserInterfaceInfo(out string name, out Image icon)
		{
			name = "Class Instance";
			icon = Properties.Resources.B16x16_Button_Class_Instance;
		}

		public override Size Draw(DrawContext context, int x, int y)
		{
			if (IsHidden && !IsWrapped)
			{
				return DrawHidden(context, x, y);
			}

			var origX = x;
			var origY = y;

			AddSelection(context, x, y, context.Font.Height);

			x = AddOpenCloseIcon(context, x, y);
			x = AddIcon(context, x, y, context.IconProvider.Class, HotSpot.NoneId, HotSpotType.None);

			var tx = x;
			x = AddAddressOffset(context, x, y);

			x = AddText(context, x, y, context.Settings.TypeColor, HotSpot.NoneId, "Instance") + context.Font.Width;
			if (!IsWrapped)
			{
				x = AddText(context, x, y, context.Settings.NameColor, HotSpot.NameId, Name) + context.Font.Width;
			}
			var isCompact = IsDrawnCompact(context);
			if (isCompact)
			{
				x = AddClassInfo(context, x, y, context.IconProvider.Change);
			}
			else
			{
				x = AddText(context, x, y, context.Settings.ValueColor, HotSpot.NoneId, $"<{InnerNode.Name}>") + context.Font.Width;
				x = AddIcon(context, x, y, context.IconProvider.Change, 4, HotSpotType.ChangeClassType) + context.Font.Width;
			}

			x = AddComment(context, x, y);

			DrawInvalidMemoryIndicatorIcon(context, y);
			AddContextDropDownIcon(context, y);
			AddDeleteIcon(context, y);

			y += context.Font.Height;

			var size = new Size(x - origX, y - origY);

			if (LevelsOpen[context.Level])
			{
				var innerSize = isCompact ? DrawClassChildren(context, tx, y) : InnerNode.Draw(CreateInnerContext(context), tx, y);
				size.Width = Math.Max(size.Width, innerSize.Width + tx - origX);
				size.Height += innerSize.Height;
			}

			return size;
		}

		public override int CalculateDrawnHeight(DrawContext context)
		{
			if (IsHidden && !IsWrapped)
			{
				return HiddenHeight;
			}

			var height = context.Font.Height;
			if (LevelsOpen[context.Level])
			{
				height += IsDrawnCompact(context) ? CalculateClassChildrenHeight(context) : InnerNode.CalculateDrawnHeight(context);
			}
			return height;
		}

		/// <summary>A wrapped instance stays a row of its own unless the wrapper draws it, see <see cref="PointerNode"/>.</summary>
		private bool IsDrawnCompact(DrawContext context) => context.Settings.CompactClassRows && !IsWrapped && InnerNode is ClassNode;

		private DrawContext CreateInnerContext(DrawContext context)
		{
			var innerContext = context.Clone();
			innerContext.Address = context.Address + Offset;
			innerContext.Memory = context.Memory.Clone();
			innerContext.Memory.Offset += Offset;
			return innerContext;
		}

		/// <summary>Draws the class name, the icon to change the class and the class size on the row of an other node.</summary>
		/// <param name="context">The drawing context.</param>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="changeClassIcon">The icon to change the class. Its <see cref="HotSpot"/> belongs to this node because only this node can hold a class.</param>
		/// <returns>The new x coordinate after drawing.</returns>
		internal int AddClassInfo(DrawContext context, int x, int y, Image changeClassIcon)
		{
			var classNode = (ClassNode)InnerNode;

			x = classNode.AddBracketedName(context, x, y) + context.Font.Width;
			x = AddIcon(context, x, y, changeClassIcon, 4, HotSpotType.ChangeClassType) + context.Font.Width;
			return AddText(context, x, y, context.Settings.ValueColor, HotSpot.NoneId, $"[{classNode.MemorySize}]") + context.Font.Width;
		}

		/// <summary>Draws the child rows of the class without the row of the class itself.</summary>
		/// <param name="context">The drawing context of this node.</param>
		/// <param name="x">The x coordinate of the child rows.</param>
		/// <param name="y">The y coordinate of the first child row.</param>
		/// <returns>The pixel size the child rows occupy.</returns>
		internal Size DrawClassChildren(DrawContext context, int x, int y) =>
			((ClassNode)InnerNode).DrawChildren(CreateInnerContext(context), x, y);

		/// <summary>Calculates the height <see cref="DrawClassChildren"/> returns.</summary>
		/// <param name="context">The drawing context of this node.</param>
		/// <returns>The calculated height.</returns>
		internal int CalculateClassChildrenHeight(DrawContext context) =>
			((ClassNode)InnerNode).CalculateChildrenHeight(context);
	}
}
