using System;

namespace FD.Blazor.Core
{
    [Flags]
    public enum TableStyle
    {
        Dark = 1,
        Striped = 2,
        Bordered = 4,
        Borderless = 8,
        Hover = 16,
        Sm = 32,
    }

    public enum SortDirection
    {
        Ascending = 1,
        Descending = 2,
        None = 0
    }
}
