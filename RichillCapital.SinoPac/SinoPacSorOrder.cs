using RichillCapital.SinoPac.Sor.Models;

namespace RichillCapital.SinoPac.Sor;

public class SinoPacSorOrder
{
    OrdTable Table_;
    string[] SorValues_;
    List<string[]> DealDetails_ = new List<string[]>();
    SorAccount Acc_;

    /// 利用委託欄位 SorValues_ 取得此筆委託的帳號.
    void RegetAcc(AccountManager accs)
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
    public SinoPacSorOrder(OrdTable table, string[] values, AccountManager accs)
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
            uint index = Table.SorTable.Fields.NameFieldIndex(fieldName);
            if (index == SorField.InvalidIndex || index >= SorValues_.Length)
                return null;
            return SorValues_[index];
        }
    }

    /// <summary>
    /// 回報更新委託內容, 如果是成交回報,則可能會加入成交明細表.
    /// </summary>
    public void SetRptFields(RptTable rptTable, string[] rptFlds, AccountManager accs)
    {
        SorTable sorTable;
        SorField field;
        string value;
        uint index;

        if (rptFlds != null)
        {
            sorTable = rptTable.SorTable;
            for (uint i = 0; i < sorTable.Fields.Count; i++)
            {
                field = rptTable.SorTable.Fields.IndexField(i);
                value = rptFlds[i];
                index = Table_.SorTable.Fields.NameFieldIndex(field.Properties.Name);
                if (index < SorValues_.Length)
                    SorValues_[index] = value;
            }
        }

        uint[] ddsidxs = rptTable.GetDDSFromRpt(Table_);
        if (ddsidxs != null)
        {
            string[] dealValues = new string[ddsidxs.Length];
            int idds = 0;
            foreach (uint irpt in ddsidxs)
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
    public SorAccount Acc { get { return Acc_; } }
}
