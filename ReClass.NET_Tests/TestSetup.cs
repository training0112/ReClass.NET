using System.Runtime.CompilerServices;
using System.Text;

namespace ReClass.NET_Tests
{
	internal static class TestSetup
	{
		/// <summary>Program.Main registers the code pages (e.g. Windows-1252) in the real application.</summary>
		[ModuleInitializer]
		internal static void Initialize()
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		}
	}
}
