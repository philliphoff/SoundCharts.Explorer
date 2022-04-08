using System.Collections.Immutable;

namespace SoundCharts.Explorer.MacOS.Views.SourceList.Model;

internal abstract class ExplorerItem
{
    public virtual IImmutableList<ExplorerItem> GetChildren()
    {
        return ImmutableList<ExplorerItem>.Empty;
    }

    public abstract TreeObject GetObject();
}
