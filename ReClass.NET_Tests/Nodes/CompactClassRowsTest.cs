using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using ReClassNET;
using ReClassNET.Controls;
using ReClassNET.Memory;
using ReClassNET.Nodes;
using ReClassNET.UI;
using ReClassNET.Util;
using Xunit;

namespace ReClass.NET_Tests.Nodes
{
	public class CompactClassRowsTest : IDisposable
	{
		private const int RowHeight = 16;
		private static readonly IntPtr PointerTarget = (IntPtr)0x10000;
		private static readonly IntPtr BaseAddress = (IntPtr)0x20000;

		private readonly Bitmap bitmap = new Bitmap(1600, 2000);
		private readonly Graphics graphics;
		private readonly Font font = new Font("Courier New", RowHeight, GraphicsUnit.Pixel);

		public CompactClassRowsTest()
		{
			graphics = Graphics.FromImage(bitmap);
		}

		public void Dispose()
		{
			graphics.Dispose();
			font.Dispose();
			bitmap.Dispose();
		}

		private DrawContext CreateContext(ClassNode root, bool compact, Rectangle? clientArea = null)
		{
			var memory = new MemoryBuffer { Size = root.MemorySize };
			foreach (var pointer in root.Nodes.OfType<PointerNode>())
			{
				BitConverter.GetBytes((long)PointerTarget).CopyTo(memory.RawData, pointer.Offset);
			}

			return new DrawContext
			{
				Settings = new Settings { CompactClassRows = compact },
				Graphics = graphics,
				Font = new FontEx { Font = font, Width = 10, Height = RowHeight },
				IconProvider = new IconProvider(),
				Memory = memory,
				CurrentTime = DateTime.UtcNow,
				ClientArea = clientArea ?? new Rectangle(0, 0, bitmap.Width, bitmap.Height),
				HotSpots = new List<HotSpot>(),
				Address = BaseAddress,
				Level = 0
			};
		}

		private static int DrawRows(ClassNode root, DrawContext context)
		{
			context.HotSpots.Clear();

			var height = root.Draw(context, 0, 0).Height;

			Assert.Equal(height, root.CalculateDrawnHeight(context));

			return height / RowHeight;
		}

		private static ClassNode CreateClass(string name, params BaseNode[] nodes)
		{
			var node = ClassNode.Create();
			node.Name = name;
			node.AddNodes(nodes);
			return node;
		}

		private static (PointerNode Pointer, ClassInstanceNode Instance) PointerTo(ClassNode target)
		{
			var instance = new ClassInstanceNode();
			instance.ChangeInnerNode(target);

			var pointer = new PointerNode();
			pointer.ChangeInnerNode(instance);

			return (pointer, instance);
		}

		private static ClassInstanceNode InstanceOf(ClassNode target)
		{
			var instance = new ClassInstanceNode();
			instance.ChangeInnerNode(target);
			return instance;
		}

		private static HotSpot SpotOf(DrawContext context, BaseNode node, HotSpotType type) =>
			context.HotSpots.Single(h => h.Node == node && h.Type == type);

		[Fact]
		public void SelfReferencingClassStopsAtOneLevelPerExpansion()
		{
			var root = CreateClass("Root", new Int32Node());
			var (pointer, _) = PointerTo(root);
			root.AddNode(pointer);

			var context = CreateContext(root, true);

			// Class, Int32, Ptr.
			Assert.Equal(3, DrawRows(root, context));

			pointer.ToggleLevelOpen(1);
			Assert.Equal(5, DrawRows(root, context));

			pointer.ToggleLevelOpen(2);
			Assert.Equal(7, DrawRows(root, context));

			pointer.ToggleLevelOpen(1);
			Assert.Equal(3, DrawRows(root, context));
		}

		[Fact]
		public void CompactPointerRowDrawsChildrenDirectly()
		{
			var target = CreateClass("Target", new Int32Node(), new Int32Node());
			var (pointer, instance) = PointerTo(target);
			var root = CreateClass("Root", pointer);

			var context = CreateContext(root, true);

			Assert.Equal(2, DrawRows(root, context));

			instance.SetLevelOpen(1, true);

			Assert.Equal(4, DrawRows(root, context));
		}

		[Fact]
		public void OldModeKeepsTheThreeRows()
		{
			var target = CreateClass("Target", new Int32Node(), new Int32Node());
			var (pointer, instance) = PointerTo(target);
			var root = CreateClass("Root", pointer);

			var context = CreateContext(root, false);

			// Class, Ptr, Instance.
			Assert.Equal(3, DrawRows(root, context));

			instance.SetLevelOpen(1, true);

			// Class, Ptr, Instance, Class, Int32, Int32.
			Assert.Equal(6, DrawRows(root, context));

			// The pointer has its own open state in this mode.
			pointer.ToggleLevelOpen(1);

			Assert.Equal(2, DrawRows(root, context));
			Assert.True(instance.IsLevelOpen(1));
		}

		[Fact]
		public void CompactInstanceRowOmitsTheClassRow()
		{
			var target = CreateClass("Target", new Int32Node(), new Int32Node());
			var instance = InstanceOf(target);
			var root = CreateClass("Root", instance);

			var compact = CreateContext(root, true);
			Assert.Equal(2, DrawRows(root, compact));

			instance.ToggleLevelOpen(1);

			Assert.Equal(4, DrawRows(root, compact));

			// Class, Instance, Class, Int32, Int32.
			Assert.Equal(5, DrawRows(root, CreateContext(root, false)));
		}

		[Fact]
		public void OtherPointersStayTheSame()
		{
			var toInt = new PointerNode();
			toInt.ChangeInnerNode(new Int32Node());
			var toVoid = new PointerNode();
			var root = CreateClass("Root", toInt, toVoid);

			// Class, Ptr, Int32, Ptr.
			Assert.Equal(4, DrawRows(root, CreateContext(root, true)));
			Assert.Equal(4, DrawRows(root, CreateContext(root, false)));

			toInt.ToggleLevelOpen(1);

			Assert.Equal(3, DrawRows(root, CreateContext(root, true)));
			Assert.Equal(3, DrawRows(root, CreateContext(root, false)));
		}

		[Fact]
		public void WrappedPointerInArrayIsCompact()
		{
			var target = CreateClass("Target", new Int32Node());
			var (pointer, instance) = PointerTo(target);
			var array = new ArrayNode();
			array.ChangeInnerNode(pointer);
			array.ToggleLevelOpen(1);
			var root = CreateClass("Root", array);

			var context = CreateContext(root, true);

			// Class, Array, Ptr.
			Assert.Equal(3, DrawRows(root, context));

			Assert.Equal(1, context.HotSpots.Count(h => h.Node == instance && h.Type == HotSpotType.ChangeClassType));
			Assert.DoesNotContain(context.HotSpots, h => h.Node == pointer && h.Id == HotSpot.NameId);

			pointer.ToggleLevelOpen(1);

			Assert.Equal(4, DrawRows(root, context));
		}

		[Theory]
		[InlineData(true, false, 0)]
		[InlineData(true, true, 0)]
		[InlineData(false, false, 0)]
		[InlineData(false, true, 0)]
		[InlineData(true, true, 40)]
		[InlineData(false, true, 40)]
		public void DrawHeightMatchesCalculatedHeight(bool compact, bool open, int clientHeight)
		{
			var inner = CreateClass("Inner", new Int32Node(), new Int32Node());
			var (innerPointer, innerPointerInstance) = PointerTo(inner);
			var target = CreateClass("Target", new Int32Node(), innerPointer);
			var (pointer, instance) = PointerTo(target);
			var inline = InstanceOf(inner);
			var root = CreateClass("Root", pointer, inline, new Int32Node());

			if (open)
			{
				instance.SetLevelOpen(1, true);
				inline.SetLevelOpen(1, true);
				innerPointerInstance.SetLevelOpen(2, true);
			}

			// A small client area sends rows through the culling path of ClassNode.
			var area = clientHeight > 0 ? new Rectangle(0, 0, bitmap.Width, clientHeight) : (Rectangle?)null;

			var context = CreateContext(root, compact, area);

			var expected = (compact, open) switch
			{
				(true, false) => 4,  // Class, Ptr, Instance, Int32
				(true, true) => 10,  // Class, Ptr, [Int32, Ptr, [Int32, Int32]], Instance, [Int32, Int32], Int32
				(false, false) => 5, // Class, Ptr, Instance, Instance, Int32
				(false, true) => 15  // Every wrapper adds its own Instance and Class row.
			};

			Assert.Equal(expected, DrawRows(root, context));
		}

		[Fact]
		public void CompactRowHotSpotsBelongToTheRightNodes()
		{
			var target = CreateClass("Target", new Int32Node());
			var (pointer, instance) = PointerTo(target);
			var root = CreateClass("Root", pointer);
			var context = CreateContext(root, true);

			DrawRows(root, context);

			var typeSpot = SpotOf(context, pointer, HotSpotType.ChangeWrappedType);
			var classSpot = SpotOf(context, instance, HotSpotType.ChangeClassType);
			var nameSpot = context.HotSpots.Single(h => h.Node == target && h.Id == HotSpot.NameId && h.Rect.Y == RowHeight);
			var openSpot = SpotOf(context, pointer, HotSpotType.OpenClose);
			var selectSpot = SpotOf(context, pointer, HotSpotType.Select);

			Assert.Equal(RowHeight, typeSpot.Rect.Y);
			Assert.Equal(RowHeight, classSpot.Rect.Y);
			Assert.Equal(RowHeight, openSpot.Rect.Y);
			Assert.Equal(RowHeight, selectSpot.Rect.Y);
			Assert.True(typeSpot.Rect.Right <= nameSpot.Rect.Left);
			Assert.True(nameSpot.Rect.Right <= classSpot.Rect.Left);

			Assert.Equal(HotSpotType.Edit, nameSpot.Type);
			Assert.Equal("Target", nameSpot.Text);

			// The same path MainForm uses after the class selection.
			Assert.False(pointer.CanChangeInnerNodeTo(target));
			Assert.True(instance.CanChangeInnerNodeTo(target));
			Assert.Same(pointer, instance.GetRootWrapperNode());
			Assert.False(instance.GetRootWrapperNode().ShouldPerformCycleCheckForInnerNode());

			nameSpot.Text = "Renamed";
			nameSpot.Node.Update(nameSpot);

			Assert.Equal("Renamed", target.Name);
		}

		[Fact]
		public void InlineInstanceStillPerformsTheCycleCheck()
		{
			var target = CreateClass("Target", new Int32Node());
			var instance = InstanceOf(target);
			var root = CreateClass("Root", instance);
			var context = CreateContext(root, true);

			DrawRows(root, context);

			Assert.Equal(RowHeight, SpotOf(context, instance, HotSpotType.ChangeClassType).Rect.Y);
			Assert.True(instance.GetRootWrapperNode().ShouldPerformCycleCheckForInnerNode());
		}

		[Fact]
		public void ArrowAndKeyboardPathsUseTheInstanceState()
		{
			var target = CreateClass("Target", new Int32Node());
			var (pointer, instance) = PointerTo(target);
			var root = CreateClass("Root", pointer);
			var context = CreateContext(root, true);

			Assert.Equal(2, DrawRows(root, context));

			// Arrow click and double click both call Node.ToggleLevelOpen(Level).
			var arrow = SpotOf(context, pointer, HotSpotType.OpenClose);
			arrow.Node.ToggleLevelOpen(arrow.Level);

			Assert.True(instance.IsLevelOpen(1));
			Assert.Equal(3, DrawRows(root, context));

			var select = SpotOf(context, pointer, HotSpotType.Select);
			select.Node.ToggleLevelOpen(select.Level);

			Assert.False(instance.IsLevelOpen(1));
			Assert.Equal(2, DrawRows(root, context));

			// Right and Left keys call Node.SetLevelOpen(Level, bool).
			select = SpotOf(context, pointer, HotSpotType.Select);
			select.Node.SetLevelOpen(select.Level, true);

			Assert.Equal(3, DrawRows(root, context));

			select = SpotOf(context, pointer, HotSpotType.Select);
			select.Node.SetLevelOpen(select.Level, false);

			Assert.Equal(2, DrawRows(root, context));
		}

		[Fact]
		public void ChangingTheInnerNodeEndsTheCompactRow()
		{
			var target = CreateClass("Target", new Int32Node());
			var (pointer, instance) = PointerTo(target);
			var root = CreateClass("Root", pointer);
			var context = CreateContext(root, true);

			DrawRows(root, context);

			pointer.ChangeInnerNode(new Int32Node());
			pointer.ToggleLevelOpen(1);

			Assert.False(instance.IsLevelOpen(1));
			Assert.Equal(2, DrawRows(root, context));
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void ChildAddressesAreTheSameInBothModes(bool compact)
		{
			var target = CreateClass("Target", new Int32Node(), new Int32Node());
			var (pointer, pointerInstance) = PointerTo(target);
			var inlineChild = new Int32Node();
			var inlineClass = CreateClass("Inline", new Int32Node(), inlineChild);
			var inline = InstanceOf(inlineClass);
			var root = CreateClass("Root", new Int32Node(), pointer, inline);

			pointerInstance.SetLevelOpen(1, true);
			inline.SetLevelOpen(1, true);

			var context = CreateContext(root, compact);

			DrawRows(root, context);

			var pointerChild = target.Nodes[1];
			Assert.Equal(PointerTarget + pointerChild.Offset, SpotOf(context, pointerChild, HotSpotType.Select).Address);
			Assert.Equal(BaseAddress + inline.Offset + inlineChild.Offset, SpotOf(context, inlineChild, HotSpotType.Select).Address);
		}

		[Fact]
		public void SettingDefaultsToOnAndRoundTrips()
		{
			Assert.True(new Settings().CompactClassRows);

			var path = Path.GetTempFileName();
			try
			{
				SettingsSerializer.Save(new Settings { CompactClassRows = false }, path);
				Assert.False(SettingsSerializer.Load(path).CompactClassRows);

				SettingsSerializer.Save(new Settings { CompactClassRows = true }, path);
				Assert.True(SettingsSerializer.Load(path).CompactClassRows);

				// A file written before this setting existed.
				File.WriteAllText(path, "<Settings><Display><ShowNodeOffset>false</ShowNodeOffset></Display></Settings>");
				var old = SettingsSerializer.Load(path);

				Assert.False(old.ShowNodeOffset);
				Assert.True(old.CompactClassRows);
			}
			finally
			{
				File.Delete(path);
			}
		}
	}
}
