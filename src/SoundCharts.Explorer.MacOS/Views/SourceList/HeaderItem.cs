using System.Collections.Generic;
using System.Linq;

namespace SoundCharts.Explorer.MacOS.Views.SourceList
{
    internal sealed class HeaderItem : SourceListItem
    {
        public HeaderItem(string title, IEnumerable<SourceListItem> children)
            : base(title)
        {
            this.Children = children.ToArray();
        }

        public SourceListItem[] Children { get; }
    }
}
