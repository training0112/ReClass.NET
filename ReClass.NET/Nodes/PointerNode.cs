using System;
using System.Drawing;
using ReClassNET.Controls;
using ReClassNET.Memory;
using ReClassNET.UI;

namespace ReClassNET.Nodes
{
	public class PointerNode : BaseWrapperNode
	{
		private readonly MemoryBuffer memory = new MemoryBuffer();

		/// <summary>
		/// The instance drawn on the row of this pointer, as of the last draw. The row then opens and closes with the
		/// instance, whose levels default to closed. The open default of the pointer alone would make a class which
		/// points to itself recurse forever. It is kept here because <see cref="ToggleLevelOpen"/> has no draw context.
		/// </summary>
		private ClassInstanceNode compactInstance;

		private bool IsCompact => compactInstance != null && compactInstance == InnerNode;

		public override int MemorySize => IntPtr.Size;

		protected override bool PerformCycleCheck => false;

		public PointerNode()
		{
			LevelsOpen.DefaultValue = true;
		}

		public override void Initialize()
		{
			var node = new ClassInstanceNode();
			node.Initialize();
			((BaseContainerNode)node.InnerNode).AddBytes(16 * IntPtr.Size);

			ChangeInnerNode(node);
		}

		public override void GetUserInterfaceInfo(out string name, out Image icon)
		{
			name = "Pointer";
			icon = Properties.Resources.B16x16_Button_Pointer;
		}

		public override bool UseMemoryPreviewToolTip(HotSpot spot, out IntPtr address)
		{
			// TODO Should the preview be disabled if an inner node is set?

			address = spot.Memory.ReadIntPtr(Offset);

			return spot.Process.GetNamedAddress(address) != null;
		}

		public override bool CanChangeInnerNodeTo(BaseNode node) =>
			node switch
			{
				ClassNode _ => false,
				VirtualMethodNode _ => false,
				_ => true
			};

		internal override void ToggleLevelOpen(int level)
		{
			if (IsCompact)
			{
				compactInstance.ToggleLevelOpen(level);
			}
			else
			{
				base.ToggleLevelOpen(level);
			}
		}

		internal override void SetLevelOpen(int level, bool open)
		{
			if (IsCompact)
			{
				compactInstance.SetLevelOpen(level, open);
			}
			else
			{
				base.SetLevelOpen(level, open);
			}
		}

		internal override bool IsLevelOpen(int level) => IsCompact ? compactInstance.IsLevelOpen(level) : base.IsLevelOpen(level);

		private void UpdateCompactInstance(DrawContext context)
		{
			compactInstance = context.Settings.CompactClassRows && InnerNode is ClassInstanceNode { InnerNode: ClassNode } instance ? instance : null;
		}

		public override Size Draw(DrawContext context, int x, int y)
		{
			UpdateCompactInstance(context);

			if (IsHidden && !IsWrapped)
			{
				return DrawHidden(context, x, y);
			}

			var origX = x;
			var origY = y;

			AddSelection(context, x, y, context.Font.Height);

			if (InnerNode != null)
			{
				x = AddOpenCloseIcon(context, x, y);
			}
			else
			{
				x = AddIconPadding(context, x);
			}
			x = AddIcon(context, x, y, context.IconProvider.Pointer, HotSpot.NoneId, HotSpotType.None);

			var tx = x;
			x = AddAddressOffset(context, x, y);

			x = AddText(context, x, y, context.Settings.TypeColor, HotSpot.NoneId, "Ptr") + context.Font.Width;
			if (!IsWrapped)
			{
				x = AddText(context, x, y, context.Settings.NameColor, HotSpot.NameId, Name) + context.Font.Width;
			}
			if (InnerNode == null)
			{
				x = AddText(context, x, y, context.Settings.ValueColor, HotSpot.NoneId, "<void>") + context.Font.Width;
			}
			x = AddIcon(context, x, y, context.IconProvider.Change, 4, HotSpotType.ChangeWrappedType) + context.Font.Width;

			var ptr = context.Memory.ReadIntPtr(Offset);

			x = AddText(context, x, y, context.Settings.OffsetColor, HotSpot.NoneId, "->") + context.Font.Width;
			x = AddText(context, x, y, context.Settings.ValueColor, 0, "0x" + ptr.ToString(Constants.AddressHexFormat)) + context.Font.Width;

			if (IsCompact)
			{
				x = compactInstance.AddClassInfo(context, x, y, context.IconProvider.Class);
			}

			x = AddComment(context, x, y);

			DrawInvalidMemoryIndicatorIcon(context, y);
			AddContextDropDownIcon(context, y);
			AddDeleteIcon(context, y);

			y += context.Font.Height;

			var size = new Size(x - origX, y - origY);

			if (IsLevelOpen(context.Level) && InnerNode != null)
			{
				memory.Size = InnerNode.MemorySize;
				memory.UpdateFrom(context.Process, ptr);

				var innerContext = context.Clone();
				innerContext.Address = ptr;
				innerContext.Memory = memory;

				var innerSize = IsCompact ? compactInstance.DrawClassChildren(innerContext, tx, y) : InnerNode.Draw(innerContext, tx, y);

				size.Width = Math.Max(size.Width, innerSize.Width + tx - origX);
				size.Height += innerSize.Height;
			}

			return size;
		}

		public override int CalculateDrawnHeight(DrawContext context)
		{
			UpdateCompactInstance(context);

			if (IsHidden && !IsWrapped)
			{
				return HiddenHeight;
			}

			var height = context.Font.Height;
			if (IsLevelOpen(context.Level) && InnerNode != null)
			{
				height += IsCompact ? compactInstance.CalculateClassChildrenHeight(context) : InnerNode.CalculateDrawnHeight(context);
			}
			return height;
		}
	}
}
