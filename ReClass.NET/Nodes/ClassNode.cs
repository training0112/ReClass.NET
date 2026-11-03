using System;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using ReClassNET.Controls;
using ReClassNET.UI;

namespace ReClassNET.Nodes
{
	public delegate void ClassCreatedEventHandler(ClassNode node);

	public class ClassNode : BaseContainerNode
	{
		public static event ClassCreatedEventHandler ClassCreated;

#if RECLASSNET64
		public static IntPtr DefaultAddress { get; } = (IntPtr)0x140000000;
		public static string DefaultAddressFormula { get; } = "140000000";
#else
		public static IntPtr DefaultAddress { get; } = (IntPtr)0x400000;
		public static string DefaultAddressFormula { get; } = "400000";
#endif

		public override int MemorySize => Nodes.Sum(n => n.MemorySize);

		protected override bool ShouldCompensateSizeChanges => true;

		public Guid Uuid { get; set; }

		public string AddressFormula { get; set; } = DefaultAddressFormula;

		public event NodeEventHandler NodesChanged;

		internal ClassNode(bool notifyClassCreated)
		{
			Contract.Ensures(AddressFormula != null);

			LevelsOpen.DefaultValue = true;

			Uuid = Guid.NewGuid();

			if (notifyClassCreated)
			{
				ClassCreated?.Invoke(this);
			}
		}

		public static ClassNode Create()
		{
			Contract.Ensures(Contract.Result<ClassNode>() != null);

			return new ClassNode(true);
		}

		public override void GetUserInterfaceInfo(out string name, out Image icon)
		{
			throw new InvalidOperationException($"The '{nameof(ClassNode)}' node should not be accessible from the ui.");
		}

		public override bool CanHandleChildNode(BaseNode node)
		{
			switch (node)
			{
				case null:
				case ClassNode _:
				case VirtualMethodNode _:
					return false;
			}

			return true;
		}

		public override void Initialize()
		{
			AddBytes(IntPtr.Size);
		}

		public override Size Draw(DrawContext context, int x, int y)
		{
			AddSelection(context, 0, y, context.Font.Height);

			var origX = x;
			var origY = y;

			x = AddOpenCloseIcon(context, x, y);

			var tx = x;

			x = AddIcon(context, x, y, context.IconProvider.Class, HotSpot.NoneId, HotSpotType.None);
			x = AddText(context, x, y, context.Settings.OffsetColor, 0, AddressFormula) + context.Font.Width;

			x = AddText(context, x, y, context.Settings.TypeColor, HotSpot.NoneId, "Class") + context.Font.Width;
			x = AddText(context, x, y, context.Settings.NameColor, HotSpot.NameId, Name) + context.Font.Width;
			x = AddText(context, x, y, context.Settings.ValueColor, HotSpot.NoneId, $"[{MemorySize}]") + context.Font.Width;
			x = AddComment(context, x, y);

			y += context.Font.Height;

			var size = new Size(x - origX, y - origY);

			if (LevelsOpen[context.Level])
			{
				var childrenSize = DrawChildren(context, tx, y);

				size.Width = Math.Max(size.Width, childrenSize.Width + tx - origX);
				size.Height += childrenSize.Height;
			}

			return size;
		}

		/// <summary>Draws the child nodes one level deeper than <paramref name="context"/>, without the row of the class itself.</summary>
		/// <param name="context">The drawing context of the class.</param>
		/// <param name="x">The x coordinate of the child rows.</param>
		/// <param name="y">The y coordinate of the first child row.</param>
		/// <returns>The pixel size the child rows occupy.</returns>
		internal Size DrawChildren(DrawContext context, int x, int y)
		{
			var size = Size.Empty;

			var innerContext = context.Clone();
			innerContext.Level++;
			foreach (var node in Nodes)
			{
				Size AggregateNodeSizes(Size baseSize, Size newSize)
				{
					return new Size(Math.Max(baseSize.Width, newSize.Width), baseSize.Height + newSize.Height);
				}

				// Draw the node if it is in the visible area.
				if (context.ClientArea.Contains(x, y))
				{
					var innerSize = node.Draw(innerContext, x, y);

					size = AggregateNodeSizes(size, innerSize);

					y += innerSize.Height;
				}
				else
				{
					// Otherwise calculate the height...
					var calculatedHeight = node.CalculateDrawnHeight(innerContext);

					// and check if the node area overlaps with the visible area...
					if (new Rectangle(x, y, 9999999, calculatedHeight).IntersectsWith(context.ClientArea))
					{
						// then draw the node...
						var innerSize = node.Draw(innerContext, x, y);

						size = AggregateNodeSizes(size, innerSize);

						y += innerSize.Height;
					}
					else
					{
						// or skip drawing and just use the calculated height.
						size = AggregateNodeSizes(size, new Size(0, calculatedHeight));

						y += calculatedHeight;
					}
				}
			}

			return size;
		}

		/// <summary>Draws the name of the class in angle brackets. The name can be edited in place.</summary>
		/// <param name="context">The drawing context.</param>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <returns>The new x coordinate after drawing the text.</returns>
		internal int AddBracketedName(DrawContext context, int x, int y)
		{
			x = AddText(context, x, y, context.Settings.ValueColor, HotSpot.NoneId, "<");
			x = AddText(context, x, y, context.Settings.ValueColor, HotSpot.NameId, Name);
			return AddText(context, x, y, context.Settings.ValueColor, HotSpot.NoneId, ">");
		}

		public override int CalculateDrawnHeight(DrawContext context)
		{
			if (IsHidden)
			{
				return HiddenHeight;
			}

			var height = context.Font.Height;
			if (LevelsOpen[context.Level])
			{
				height += CalculateChildrenHeight(context);
			}
			return height;
		}

		/// <summary>Calculates the height <see cref="DrawChildren"/> returns.</summary>
		/// <param name="context">The drawing context of the class.</param>
		/// <returns>The calculated height.</returns>
		internal int CalculateChildrenHeight(DrawContext context)
		{
			var nv = context.Clone();
			nv.Level++;
			return Nodes.Sum(n => n.CalculateDrawnHeight(nv));
		}

		public override void Update(HotSpot spot)
		{
			base.Update(spot);

			if (spot.Id == 0)
			{
				AddressFormula = spot.Text;
			}
		}

		protected internal override void ChildHasChanged(BaseNode child)
		{
			NodesChanged?.Invoke(this);
		}
	}
}
