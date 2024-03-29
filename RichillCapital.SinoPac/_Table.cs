using System;
using System.Collections.Generic;

namespace SorApi;

using TIndex = UInt32;

#region Sor [委託表格/回報表格] 管理
/// <summary>
/// 各類SOR回報表(RPT,ORD,DDS).
/// </summary>
public class RptTableBase : IComparable
{
    SorApi.SorTable Table_;
    SorApi.SorMktFlags MktFlag_;

    /// <summary>
    /// 建構
    /// </summary>
    public RptTableBase(SorApi.SorTable table)
    {
        Table_ = table;
        MktFlag_ = table.MktFlag;
    }

    /// <summary>
    /// 取得指定欄位索引的字串, 若idx超過flds大小則傳回 null.
    /// </summary>
    public static string GetValue(string[] flds, TIndex idx)
    {
        return (idx < flds.Length ? flds[idx] : null);
    }

    /// <summary>
    /// 取得 SorTable 來源.
    /// </summary>
    public SorApi.SorTable SorTable { get { return Table_; } }

    /// <summary>
    /// 市場旗標.
    /// </summary>
    public SorApi.SorMktFlags MktFlag { get { return MktFlag_; } }


    int IComparable.CompareTo(object obj)
    {
        RptTableBase r = obj as RptTableBase;
        if (r != null)
            return this.Table_.Properties.Name.CompareTo(r.Table_.Properties.Name);
        return this.Table_.Properties.Name.CompareTo(r.ToString());
    }
}

/// <summary>
/// SOR回報表(RPT,ORD).
/// </summary>
public class KeyedRptTable : RptTableBase
{
    /// <summary>
    /// OrgSorRID 在 SOR欄位的索引.
    /// </summary>
    TIndex IOrgSorRID_;
    /// <summary>
    /// 建構.
    /// </summary>
    public KeyedRptTable(SorApi.SorTable table)
        : base(table)
    {
        IOrgSorRID_ = table.Fields.NameFieldIndex("OrgSorRID");
    }

    /// <summary>
    /// 取得 OrgSorRID, 作為刪改的依據.
    /// </summary>
    public string GetOrgSorRID(string[] flds)
    {
        return GetValue(flds, IOrgSorRID_);
    }
}

/// <summary>
/// 成交明細表.
/// </summary>
public class DDSTable : RptTableBase
{
    TIndex FieldsCount_;
    /// <summary>
    /// 建構.
    /// </summary>
    public DDSTable(SorApi.SorTable table)
        : base(table)
    {
        FieldsCount_ = table.Fields.Count;
    }
    /// <summary>
    /// 成交明細欄位數量.
    /// </summary>
    public TIndex FieldsCount { get { return FieldsCount_; } }
}

/// <summary>
/// SOR回報表(RPT, ORD).
/// </summary>
public class RptTable : KeyedRptTable
{
    SortedList<string, TIndex> DDSFldNames_ = new SortedList<string, TIndex>();
    SortedList<OrdTable, TIndex[]> DDSFromRpt_ = new SortedList<OrdTable, TIndex[]>();

    /// <summary>
    /// 建構, 登入結果的 "ORD:" 處理完畢後.
    /// </summary>
    public RptTable(SorApi.SorTable table)
        : base(table)
    {
        SorApi.SorFields fields = table.Fields;
        TIndex fcount = fields.Count;
        for (TIndex L = 0; L < fcount; ++L)
        {
            SorApi.SorField fld = fields.IndexField(L);
            SorApi.SorProperties prop = fld.Properties;
            string ddsFld = prop.Get("DDS");
            if (!string.IsNullOrEmpty(ddsFld))
                DDSFldNames_[ddsFld] = L;
        }
    }
    /// <summary>
    /// 傳回陣列的用法: ddsValues[ddsFieldIndex] = rptFlds[ddsidxs[ddsFieldIndex]]
    /// </summary>
    internal TIndex[] GetDDSFromRpt(OrdTable ordTable)
    {
        if (ordTable.DDSTable_ == null || DDSFldNames_.Count <= 0)
            return null;
        TIndex ddsFldCount = ordTable.DDSTable_.FieldsCount;
        if (ddsFldCount <= 0)
            return null;
        TIndex[] ddsidxs;
        if (DDSFromRpt_.TryGetValue(ordTable, out ddsidxs))
            return ddsidxs;
        DDSFromRpt_[ordTable] = ddsidxs = new TIndex[ddsFldCount];
        for (TIndex i = 0; i < ddsFldCount; ++i)
            ddsidxs[i] = SorApi.SorField.InvalidIndex;
        int L = 0;
        SorApi.SorFields ddsFields = ordTable.DDSTable_.SorTable.Fields;
        foreach (string ddsName in DDSFldNames_.Keys)
        {
            TIndex fldIndex = ddsFields.NameFieldIndex(ddsName);
            if (fldIndex < ddsFldCount)
                ddsidxs[fldIndex] = DDSFldNames_.Values[L];
            ++L;
        }
        return ddsidxs;
    }
}

/// <summary>
/// 刪單要求表.
/// </summary>
public class ReqKillTable
{
    SorApi.SorMktFlags MktFlag_;
    TIndex IdxFldAmendKey_;
    TIndex IdxFldOrgSorRID_;
    TIndex IdxDigSgn_;
    string TableID_;
    string[] ReqFlds_;

    /// <summary>
    /// 建構.
    /// </summary>
    private ReqKillTable(SorApi.SorTable reqTable, TIndex idxFldAmendKey, TIndex idxFldOrgSorRID, string tableID)
    {
        MktFlag_ = reqTable.MktFlag;
        IdxFldAmendKey_ = idxFldAmendKey;
        IdxFldOrgSorRID_ = idxFldOrgSorRID;
        IdxDigSgn_ = reqTable.Fields.NameFieldIndex("DigSgn");
        TableID_ = tableID;
        ReqFlds_ = new string[reqTable.Fields.Count];
    }

    /// <summary>
    /// 建構.
    /// </summary>
    public ReqKillTable(SorApi.SorTable reqTable, TIndex idxFldQty, TIndex idxFldAmendKey, TIndex idxFldOrgSorRID, string tableID)
        : this(reqTable, idxFldAmendKey, idxFldOrgSorRID, tableID)
    {
        if (idxFldQty < reqTable.Fields.Count)
            ReqFlds_[idxFldQty] = "0";
    }

    /// <summary>
    /// 建構.
    /// </summary>
    public ReqKillTable(SorApi.SorTable reqTable, TIndex idxFldBidQty, TIndex idxFldOfferQty, TIndex idxFldAmendKey, TIndex idxFldOrgSorRID, string tableID)
        : this(reqTable, idxFldAmendKey, idxFldOrgSorRID, tableID)
    {
        if (idxFldBidQty < reqTable.Fields.Count)
            ReqFlds_[idxFldBidQty] = "0";
        if (idxFldOfferQty < reqTable.Fields.Count)
            ReqFlds_[idxFldOfferQty] = "0";
    }

    /// <summary>
    /// 此刪單表可操作的市場.
    /// </summary>
    public SorApi.SorMktFlags MktFlag { get { return MktFlag_; } }

    /// <summary>
    /// 建立委託刪單要求字串.
    /// </summary>
    public string MakeKillReqStr(SinoPacSorOrder ord, out string errMsg)
    {
        if (IdxFldAmendKey_ != SorApi.SorField.InvalidIndex)
            ReqFlds_[IdxFldAmendKey_] = ord.AmendKey;
        if (IdxFldOrgSorRID_ != SorApi.SorField.InvalidIndex)
            ReqFlds_[IdxFldOrgSorRID_] = ord.OrgSorRID;
        return OrdTable.MakeRequestString(ReqFlds_, TableID_, ord.Acc, IdxDigSgn_, out errMsg);
    }
}

/// <summary>
/// 委託表格內容.
/// </summary>
public class OrdTable : KeyedRptTable
{
    TIndex IBrkNo_;
    TIndex IIvacNo_;
    TIndex ISubacNo_;
    TIndex IOrdNo_;
    TIndex ILeavesQty_;
    internal DDSTable DDSTable_;
    internal ReqKillTable ReqKillTable_;

    public OrdTable(SorApi.SorTable table)
        : base(table)
    {
        SorApi.SorFields fields = table.Fields;
        IBrkNo_ = fields.NameFieldIndex("BrkNo");
        IIvacNo_ = fields.NameFieldIndex("IvacNo");
        ISubacNo_ = fields.NameFieldIndex("SubacNo");
        IOrdNo_ = fields.NameFieldIndex("OrdNo");
        ILeavesQty_ = fields.NameFieldIndex("LeavesQty");
    }
    public TIndex IBrkNo { get { return IBrkNo_; } }
    public TIndex IIvacNo { get { return IIvacNo_; } }
    public TIndex ISubacNo { get { return ISubacNo_; } }
    public TIndex IOrdNo { get { return IOrdNo_; } }
    public TIndex ILeavesQty { get { return ILeavesQty_; } }
    public ReqKillTable ReqKillTable { get { return ReqKillTable_; } }

    internal void CreateDDSTable(SorApi.SorTable ddst)
    {
        DDSTable_ = new DDSTable(ddst);
    }

    /// <summary>
    /// 建立要送出的下單訊息內容字串, 不含 ACK ReqSeqNo.
    /// acc = 用哪個帳號的憑證簽章.
    /// </summary>
    public static string MakeRequestString(string[] flds, string tableID, SorApi.Acc acc, TIndex iDigSgn, out string errMsg)
    {
        errMsg = null;
        string reqmsg1 = string.Empty;
        int L = 0;
        int iDigSgnAtMsgPos = -1;
        foreach (string f in flds)
        {
            if (L > 0)
                reqmsg1 += "\x01";
            if (iDigSgn == L)
                iDigSgnAtMsgPos = reqmsg1.Length;
            ++L;
            if (string.IsNullOrEmpty(f))
                continue;
            reqmsg1 += f;
        }
        if (acc != null && iDigSgnAtMsgPos >= 0)
        {
            if (!acc.MakeDigSgn(ref reqmsg1, iDigSgnAtMsgPos))
            {
                errMsg = reqmsg1;
                return null;
            }
        }
        return string.Format("\x01{0}\n{1}", tableID, reqmsg1);
    }
}

/// <summary>
/// [委託表格/回報表格] 管理.
/// </summary>
public class TablesMgr
{
    SortedList<string, RptTable> RptTables_ = new SortedList<string, RptTable>();
    SortedList<string, OrdTable> OrdTables_ = new SortedList<string, OrdTable>();

    public void Clear()
    {
        RptTables_.Clear();
        OrdTables_.Clear();
    }
    /// <summary>
    /// 解析登入結果表, 取得:
    /// 1. 改單要求表: 有"IsDel"屬性或 "REQ:" 開頭且有 "Qty" 欄位
    /// 2. 委託表 "ORD:"
    /// 3. 回報表 "RPT:" 當回報欄位屬性有 DDS=xxx 時, 則表示該回報欄位對應到[成交明細]的xxx欄位.
    /// 4. 成交明細表 "DDS:"
    /// </summary>
    public void ParseSgnResult(SorApi.SorTaskResult sgnResult)
    {
        TIndex tcount = sgnResult.TablesCount;
        List<SorApi.SorTable> ddsTables = new List<SorApi.SorTable>();
        List<ReqKillTable> reqKillTables = new List<ReqKillTable>();
        SorApi.SorProperties prop;
        string tableName;
        for (TIndex L = 0; L < tcount; ++L)
        {
            SorApi.SorTable table = sgnResult.IndexTable(L);
            prop = table.Properties;
            tableName = prop.Name;
            string tableType = tableName.Substring(0, 4);
            tableName = tableName.Substring(4);
            if (tableType == "REQ:")
            {
                if (prop.Get("IsNew") == "Y")
                    continue;
                // 改單要求.
                SorApi.SorFields fields = table.Fields;
                string tableID = prop.Get("ID");
                if (!string.IsNullOrEmpty(tableID))
                {
                    string propIsDel = prop.Get("IsDel");
                    bool isDelTable = (!string.IsNullOrEmpty(propIsDel) && propIsDel[0] == 'Y');
                    TIndex idxFldQty = fields.NameFieldIndex("Qty");
                    TIndex idxFldBidQty = fields.NameFieldIndex("BidQty");
                    TIndex idxFldOfferQty = fields.NameFieldIndex("OfferQty");
                    if (isDelTable || idxFldQty != SorApi.SorField.InvalidIndex || (idxFldBidQty != SorApi.SorField.InvalidIndex && idxFldOfferQty != SorApi.SorField.InvalidIndex))
                    {
                        TIndex idxFldAmendKey = fields.NameFieldIndex("OrdID");
                        if (idxFldAmendKey == SorApi.SorField.InvalidIndex)
                            idxFldAmendKey = fields.NameFieldIndex("AmendKey");
                        TIndex idxFldOrgSorRID = fields.NameFieldIndex("OrgSorRID");
                        if (idxFldAmendKey != SorApi.SorField.InvalidIndex || idxFldOrgSorRID != SorApi.SorField.InvalidIndex)
                        {
                            if (idxFldBidQty != SorApi.SorField.InvalidIndex && idxFldOfferQty != SorApi.SorField.InvalidIndex)
                                reqKillTables.Add(new ReqKillTable(table, idxFldBidQty, idxFldOfferQty, idxFldAmendKey, idxFldOrgSorRID, tableID));
                            else
                                reqKillTables.Add(new ReqKillTable(table, idxFldQty, idxFldAmendKey, idxFldOrgSorRID, tableID));
                        }
                    }
                }
            }
            else
                if (tableType == "RPT:")
                    // 增加一個[回報表格], 必須等[全部委託表]建立好之後,
                    // 因為有些[回報欄位]並不存在於[委託表].
                    RptTables_.Add(tableName, new RptTable(table));
                else
                    if (tableType == "ORD:")
                        // 增加一個[委託表格].
                        OrdTables_.Add(tableName, new OrdTable(table));
                    else
                        if (tableType == "DDS:")
                            // 成交明細表, 因為屬於 "ORD:" 的一部份, 所以先保留, 等全部的 "ORD:" 都處理完後再填入.
                            ddsTables.Add(table);
        }
        // 解析 DDS: 欄位, 設定對應委託的[成交明細表]
        foreach (SorApi.SorTable ddst in ddsTables)
        {
            prop = ddst.Properties;
            tableName = prop.Name.Substring(4);
            OrdTable ordt = OrdTable(tableName);
            if (ordt != null)
                ordt.CreateDDSTable(ddst);
        }
        // 設定委託刪單要求表.
        foreach (OrdTable ordTab in OrdTables_.Values)
        {
            SorApi.SorMktFlags mktflags = ordTab.SorTable.MktFlag;
            foreach (ReqKillTable reqk in reqKillTables)
                if ((uint)(mktflags & reqk.MktFlag) != 0)
                {
                    ordTab.ReqKillTable_ = reqk;
                    break;
                }
        }
    }

    /// <summary>
    /// 用委託表名稱,取得委託表.
    /// </summary>
    public OrdTable OrdTable(string tableName)
    {
        OrdTable ordTable;
        OrdTables_.TryGetValue(tableName, out ordTable);
        return ordTable;
    }
    /// <summary>
    /// 用回報表名稱,取得回報表.
    /// </summary>
    public RptTable RptTable(string tableName)
    {
        RptTable rptTable;
        RptTables_.TryGetValue(tableName, out rptTable);
        return rptTable;
    }
}
#endregion
