using System;
using System.Collections.Generic;
using ReClassNET.Extensions;

namespace ReClassNET.Util
{
	public class IntPtrComparer : IComparer<IntPtr>
	{
		public static IntPtrComparer Instance { get; } = new IntPtrComparer();

		public int Compare(IntPtr x, IntPtr y)
		{
			return IntPtrExtension.CompareTo(x, y); // x.CompareTo(y) would bind to the signed IntPtr.CompareTo.
		}
	}
}
