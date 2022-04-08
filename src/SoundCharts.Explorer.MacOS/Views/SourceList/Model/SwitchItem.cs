using System;
using SoundCharts.Explorer.MacOS.Views.SourceList.Objects;

namespace SoundCharts.Explorer.MacOS.Views.SourceList.Model;

internal sealed class SwitchItem : ExplorerItem
{
    public SwitchItem(bool enabled, string label, Action<bool> onChanged)
    {
        this.Enabled = enabled;
        this.Label = label;
        this.OnChanged = onChanged;
    }

    public bool Enabled { get; }

    public string Label { get; }

    public Action<bool> OnChanged { get; }

    public override TreeObject GetObject()
    {
        return new SwitchObject(this.Enabled, this.Label, this.OnChanged);
    }
}
