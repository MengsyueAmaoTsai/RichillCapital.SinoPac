namespace RichillCapital.SinoPac.Sor;
#region Sor [委託表格/回報表格] 管理

public class DDSTable : RptTableBase
{
    uint FieldsCount_;

    public DDSTable(SorTable table)
        : base(table)
    {
        FieldsCount_ = table.Fields.Count;
    }

    /// 成交明細欄位數量.
    public uint FieldsCount { get { return FieldsCount_; } }
}
#endregion
