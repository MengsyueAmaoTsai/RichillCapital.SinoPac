namespace RichillCapital.SinoPac.Sor;
#region Sor [委託表格/回報表格] 管理

/// <summary>
/// SOR回報表(RPT,ORD).
/// </summary>
public class KeyedRptTable : ReportTableBase
{
    /// <summary>
    /// OrgSorRID 在 SOR欄位的索引.
    /// </summary>
    uint IOrgSorRID_;
    /// <summary>
    /// 建構.
    /// </summary>
    public KeyedRptTable(SorTable table)
        : base(table)
    {
        IOrgSorRID_ = table.Fields.GetIndexByName("OrgSorRID").Value;
    }

    /// <summary>
    /// 取得 OrgSorRID, 作為刪改的依據.
    /// </summary>
    public string GetOrgSorRID(string[] flds)
    {
        return GetValue(flds, IOrgSorRID_);
    }
}
#endregion
