namespace SorApi;

using TIndex = UInt32;

/// <summary>
/// 一筆委託內容.
/// </summary>
public class SinoPacSorOrder
{
    OrdTable Table_;
    string[] SorValues_;
    List<string[]> DealDetails_ = new List<string[]>();
    SorApi.Acc Acc_;

    /// 利用委託欄位 SorValues_ 取得此筆委託的帳號.
    void RegetAcc(SorApi.Accs accs)
    {
        if (accs == null)
            return;
        int fldCount = SorValues_.Length;
        if (Table_.IBrkNo >= fldCount)
            return;
        string acno = SorValues_[Table_.IBrkNo];
        if (Table_.IIvacNo < fldCount)
            acno += "-" + SorValues_[Table_.IIvacNo];
        if (Table_.ISubacNo < fldCount)
        {
            string subac = SorValues_[Table_.ISubacNo];
            if (!string.IsNullOrEmpty(subac))
                acno += "-" + subac;
        }
        accs.TryGetValue(acno, out Acc_);
    }

    /// <summary>
    /// 建構.
    /// </summary>
    public SinoPacSorOrder(OrdTable table, string[] values, SorApi.Accs accs)
    {
        Table_ = table;
        if (values == null)
            values = new string[table.SorTable.Fields.Count];
        SorValues_ = values;
        RegetAcc(accs);
    }

    /// <summary>
    /// 設定委託回補欄位內容.
    /// </summary>
    public void SetSorOrdFields(string[] values)
    {
        if (values != null)
            SorValues_ = values;
    }

    /// <summary>
    /// 委託書內容值清單
    /// </summary>
    public string[] Values { get { return SorValues_; } }

    /// <summary>
    /// 依欄位名稱取得委託書內容值
    /// </summary>
    public string this[string fieldName]
    {
        get
        {
            TIndex index = Table.SorTable.Fields.NameFieldIndex(fieldName);
            if (index == SorField.InvalidIndex || index >= SorValues_.Length)
                return null;
            return SorValues_[index];
        }
    }

    /// <summary>
    /// 回報更新委託內容, 如果是成交回報,則可能會加入成交明細表.
    /// </summary>
    public void SetRptFields(RptTable rptTable, string[] rptFlds, SorApi.Accs accs)
    {
        SorTable sorTable;
        SorField field;
        string value;
        TIndex index;

        if (rptFlds != null)
        {
            sorTable = rptTable.SorTable;
            for (TIndex i = 0; i < sorTable.Fields.Count; i++)
            {
                field = rptTable.SorTable.Fields.IndexField(i);
                value = rptFlds[i];
                index = Table_.SorTable.Fields.NameFieldIndex(field.Properties.Name);
                if (index < SorValues_.Length)
                    SorValues_[index] = value;
            }
        }

        TIndex[] ddsidxs = rptTable.GetDDSFromRpt(Table_);
        if (ddsidxs != null)
        {
            string[] dealValues = new string[ddsidxs.Length];
            int idds = 0;
            foreach (TIndex irpt in ddsidxs)
            {
                if (irpt < rptFlds.Length)
                    dealValues[idds] = rptFlds[irpt];
                ++idds;
            }
            AddDealDetail(dealValues);
        }
        if (Acc_ == null)
            RegetAcc(accs);
    }

    /// <summary>
    /// 增加一筆成交明細回補.
    /// </summary>
    public void AddDealDetail(string[] flds)
    {
        DealDetails_.Add(flds);
    }

    /// <summary>
    /// 取得此筆委託Key.
    /// </summary>
    public string OrgSorRID
    {
        get { return Table_.GetOrgSorRID(SorValues_); }
    }

    /// <summary>
    /// 取得此筆委託改單Key.
    /// </summary>
    public string AmendKey
    {
        get
        {
            return string.Format("{0}-{1}-{2}", OrdTable.GetValue(SorValues_, Table_.IBrkNo)
                                              , OrdTable.GetValue(SorValues_, Table_.IOrdNo)
                                              , OrgSorRID);
        }
    }

    /// <summary>
    /// 取得此筆委託的剩餘量.
    /// </summary>
    public string LeavesQty { get { return OrdTable.GetValue(SorValues_, Table_.ILeavesQty); } }

    /// <summary>
    /// 此委託所屬的委託表.
    /// </summary>
    public OrdTable Table { get { return Table_; } }

    /// <summary>
    /// 此委託所屬的可用帳號.
    /// </summary>
    public SorApi.Acc Acc { get { return Acc_; } }
}

/// <summary>
/// 委託管理表.
/// </summary>
public class OrdsTable
{
    /// <summary>
    /// 用 SorRID 索引 SorOrd.
    /// </summary>
    SortedList<string, int> SorOrds_ = new SortedList<string, int>();
    /// <summary>
    /// 依加入順序儲存的委託列表.
    /// </summary>
    List<SinoPacSorOrder> SorOrdsList_ = new List<SinoPacSorOrder>();

    /// <summary>
    /// 清除全部委託資料.
    /// </summary>
    public void Clear()
    {
        SorOrds_.Clear();
        SorOrdsList_.Clear();
    }

    /// <summary>
    /// 使用委託Key = OrdSorRID, 取得委託書.
    /// </summary>
    public SinoPacSorOrder SorOrdAtKey(string orgSorRID)
    {
        int listIndex;
        if (!SorOrds_.TryGetValue(orgSorRID, out listIndex))
            return null;
        SinoPacSorOrder ord = SorOrdsList_[listIndex];
        return ord;
    }
    /// <summary>
    /// 增加一筆新委託書 or 若已存在則更新.
    /// </summary>
    public SinoPacSorOrder AddSorOrd(OrdTable ordTable, string orgSorRID, string[] flds, SorApi.Accs accs)
    {
        SinoPacSorOrder ord;
        int listIndex;
        if (SorOrds_.TryGetValue(orgSorRID, out listIndex))
        {
            ord = SorOrdsList_[listIndex];
            ord.SetSorOrdFields(flds);
        }
        else
        {
            ord = new SinoPacSorOrder(ordTable, flds, accs);
            SorOrds_.Add(orgSorRID, SorOrdsList_.Count);
            SorOrdsList_.Add(ord);
        }
        return ord;
    }
    /// <summary>
    /// 增加or更新一筆新委託書, 傳回: true=新增, false=更新.
    /// </summary>
    public bool AddOrUpdateOrder(string orgSorRID, SinoPacSorOrder ord)
    {
        int listIndex;

        if (SorOrds_.TryGetValue(orgSorRID, out listIndex))
        {
            SorOrdsList_[listIndex] = ord;
            return false;
        }
        
        SorOrds_.Add(orgSorRID, listIndex = SorOrdsList_.Count);
        
        SorOrdsList_.Add(ord);
        
        return true;
    }
    /// <summary>
    /// 增加or更新一筆新委託書, 傳回: true=新增, false=更新.
    /// </summary>
    public bool AddSorOrd(SinoPacSorOrder ord)
    {
        return AddOrUpdateOrder(ord.OrgSorRID, ord);
    }
    
    /// <summary>
    /// 取得委託筆數.
    /// </summary>
    public int Count { get { return SorOrdsList_.Count; } }
    /// <summary>
    /// 取得委託列表.
    /// </summary>
    public List<SinoPacSorOrder> OrdsList { get { return SorOrdsList_; } }
}
