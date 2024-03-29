namespace RichillCapital.SinoPac.Sor;
#region Sor [委託表格/回報表格] 管理
/// <summary>
/// 各類SOR回報表(RPT,ORD,DDS).
/// </summary>
public class RptTableBase : IComparable
{
    SorTable Table_;
    SorMktFlags MktFlag_;

    /// <summary>
    /// 建構
    /// </summary>
    public RptTableBase(SorTable table)
    {
        Table_ = table;
        MktFlag_ = table.MktFlag;
    }

    /// <summary>
    /// 取得指定欄位索引的字串, 若idx超過flds大小則傳回 null.
    /// </summary>
    public static string GetValue(string[] flds, uint idx)
    {
        return (idx < flds.Length ? flds[idx] : null);
    }

    /// <summary>
    /// 取得 SorTable 來源.
    /// </summary>
    public SorTable SorTable { get { return Table_; } }

    /// <summary>
    /// 市場旗標.
    /// </summary>
    public SorMktFlags MktFlag { get { return MktFlag_; } }


    int IComparable.CompareTo(object obj)
    {
        RptTableBase r = obj as RptTableBase;
        if (r != null)
            return this.Table_.Properties.Name.CompareTo(r.Table_.Properties.Name);
        return this.Table_.Properties.Name.CompareTo(r.ToString());
    }
}
#endregion
