namespace RichillCapital.SinoPac.Sor;
#region Sor [委託表格/回報表格] 管理


/// SOR回報表(RPT, ORD).
public class RptTable : KeyedRptTable
{
    SortedList<string, uint> DDSFldNames_ = new SortedList<string, uint>();
    SortedList<OrdTable, uint[]> DDSFromRpt_ = new SortedList<OrdTable, uint[]>();


    /// 建構, 登入結果的 "ORD:" 處理完畢後.

    public RptTable(SorTable table)
        : base(table)
    {
        SorFields fields = table.Fields;
        uint fcount = fields.Count;
        for (uint L = 0; L < fcount; ++L)
        {
            SorField fld = fields.IndexField(L);
            SorProperties prop = fld.Properties;
            string ddsFld = prop.Get("DDS");
            if (!string.IsNullOrEmpty(ddsFld))
                DDSFldNames_[ddsFld] = L;
        }
    }

    /// 傳回陣列的用法: ddsValues[ddsFieldIndex] = rptFlds[ddsidxs[ddsFieldIndex]]

    internal uint[] GetDDSFromRpt(OrdTable ordTable)
    {
        if (ordTable.DDSTable_ == null || DDSFldNames_.Count <= 0)
            return null;
        uint ddsFldCount = ordTable.DDSTable_.FieldsCount;
        if (ddsFldCount <= 0)
            return null;
        uint[] ddsidxs;
        if (DDSFromRpt_.TryGetValue(ordTable, out ddsidxs))
            return ddsidxs;
        DDSFromRpt_[ordTable] = ddsidxs = new uint[ddsFldCount];
        for (uint i = 0; i < ddsFldCount; ++i)
            ddsidxs[i] = SorField.InvalidIndex;
        int L = 0;
        SorFields ddsFields = ordTable.DDSTable_.SorTable.Fields;
        foreach (string ddsName in DDSFldNames_.Keys)
        {
            uint fldIndex = ddsFields.NameFieldIndex(ddsName);
            if (fldIndex < ddsFldCount)
                ddsidxs[fldIndex] = DDSFldNames_.Values[L];
            ++L;
        }
        return ddsidxs;
    }
}
#endregion
