using Foundation;

namespace SoundCharts.Explorer.MacOS.Views.SourceList;

internal abstract class TreeObject : NSObject
{
    public TreeObject(bool isExpandable = false)
    {
        this.IsExpandable = isExpandable;
    }

    public bool IsExpandable { get; }
}
