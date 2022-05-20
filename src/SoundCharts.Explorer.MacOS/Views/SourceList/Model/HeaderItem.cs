using System;
using System.Collections.Immutable;
using SoundCharts.Explorer.MacOS.Views.SourceList.Objects;

namespace SoundCharts.Explorer.MacOS.Views.SourceList.Model;

internal class HeaderItem : ExplorerItem
{
    private readonly Func<IImmutableList<ExplorerItem>>? childrenFactory;
    private readonly string title;

    public HeaderItem(string title, Func<IImmutableList<ExplorerItem>>? childrenFactory = default)
    {
        this.childrenFactory = childrenFactory;
        this.title = title;
    }

    public override IImmutableList<ExplorerItem> GetChildren()
    {
        return this.childrenFactory is not null
            ? this.childrenFactory()
            : ImmutableList<ExplorerItem>.Empty;
    }

    public override TreeObject GetObject()
    {
        return new HeaderObject(this.title);
    }
}
