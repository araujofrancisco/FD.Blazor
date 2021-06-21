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

    public enum TextAlignment
    {
        Left = 0,
        Center = 1,
        Right = 2,
        End = 3,
        Start = 4
    }

    public enum VerticalAlignment
    {
        Bottom = 0,
        Baseline = 1,
        Middle = 2,
        Top = 3
    }

    public enum ColumnStyle
    {
        Both = 0,
        Header = 1,
        Data = 2
    }
}
