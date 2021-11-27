using AppKit;

namespace SoundCharts.Explorer.MacOS
{
	static class MainClass
	{
		static void Main (string [] args)
		{
			NSApplication.Init ();
			NSApplication.Main (args);
		}
	}
}
