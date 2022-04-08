using System;

namespace SoundCharts.Explorer.MacOS.Views.SourceList.Objects;

internal sealed class SwitchObject : TreeObject
{
    private bool enabled;
    private Action<bool> onChanged;

    public SwitchObject(bool enabled, string label, Action<bool> onChanged)
    {
        this.enabled = enabled;
        this.onChanged = onChanged;

        this.Label = label;
    }

    public bool Enabled
    {
        get => this.enabled;
        set
        {
            if (this.enabled != value)
            {
                this.enabled = value;

                this.onChanged(this.enabled);
            }
        }
    }

    public string Label { get; }
}

