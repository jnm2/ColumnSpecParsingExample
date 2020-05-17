using System;

namespace ColumnSpecParsingExample
{
    public readonly struct ColumnSort
    {
        public ColumnSort(string fieldName, bool ascendingOrder)
        {
            FieldName = fieldName ?? throw new ArgumentNullException(nameof(fieldName));
            AscendingOrder = ascendingOrder;
        }

        public string FieldName { get; }
        public bool AscendingOrder { get; }
    }

}
