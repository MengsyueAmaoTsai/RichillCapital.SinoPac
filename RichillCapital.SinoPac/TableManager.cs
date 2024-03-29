namespace RichillCapital.SinoPac.Sor;

#region Sor [委託表格/回報表格] 管理


/// [委託表格/回報表格] 管理.
public class TableManager
{
    SortedList<string, RptTable> RptTables_ = new SortedList<string, RptTable>();
    SortedList<string, OrdTable> OrdTables_ = new SortedList<string, OrdTable>();

    public void Clear()
    {
        RptTables_.Clear();
        OrdTables_.Clear();
    }

    /// 解析登入結果表, 取得:
    /// 1. 改單要求表: 有"IsDel"屬性或 "REQ:" 開頭且有 "Qty" 欄位
    /// 2. 委託表 "ORD:"
    /// 3. 回報表 "RPT:" 當回報欄位屬性有 DDS=xxx 時, 則表示該回報欄位對應到[成交明細]的xxx欄位.
    /// 4. 成交明細表 "DDS:"

    public void ParseSignInResult(TaskResult sgnResult)
    {
        uint tcount = sgnResult.TableCount;
        List<SorTable> ddsTables = new List<SorTable>();
        List<ReqKillTable> reqKillTables = new List<ReqKillTable>();
        SorProperties prop;
        string tableName;
        for (uint L = 0; L < tcount; ++L)
        {
            SorTable table = sgnResult.GetTableByIndex(L);
            prop = table.Properties;
            tableName = prop.Name;
            string tableType = tableName.Substring(0, 4);
            tableName = tableName.Substring(4);
            if (tableType == "REQ:")
            {
                if (prop.Get("IsNew") == "Y")
                    continue;
                // 改單要求.
                SorFields fields = table.Fields;
                string tableID = prop.Get("ID");
                if (!string.IsNullOrEmpty(tableID))
                {
                    string propIsDel = prop.Get("IsDel");
                    bool isDelTable = (!string.IsNullOrEmpty(propIsDel) && propIsDel[0] == 'Y');
                    uint idxFldQty = fields.NameFieldIndex("Qty");
                    uint idxFldBidQty = fields.NameFieldIndex("BidQty");
                    uint idxFldOfferQty = fields.NameFieldIndex("OfferQty");
                    if (isDelTable || idxFldQty != SorField.InvalidIndex || (idxFldBidQty != SorField.InvalidIndex && idxFldOfferQty != SorField.InvalidIndex))
                    {
                        uint idxFldAmendKey = fields.NameFieldIndex("OrdID");
                        if (idxFldAmendKey == SorField.InvalidIndex)
                            idxFldAmendKey = fields.NameFieldIndex("AmendKey");
                        uint idxFldOrgSorRID = fields.NameFieldIndex("OrgSorRID");
                        if (idxFldAmendKey != SorField.InvalidIndex || idxFldOrgSorRID != SorField.InvalidIndex)
                        {
                            if (idxFldBidQty != SorField.InvalidIndex && idxFldOfferQty != SorField.InvalidIndex)
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
        foreach (SorTable ddst in ddsTables)
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
            SorMarketFlag mktflags = ordTab.SorTable.MktFlag;
            foreach (ReqKillTable reqk in reqKillTables)
                if ((uint)(mktflags & reqk.MktFlag) != 0)
                {
                    ordTab.ReqKillTable_ = reqk;
                    break;
                }
        }
    }


    /// 用委託表名稱,取得委託表.

    public OrdTable OrdTable(string tableName)
    {
        OrdTable ordTable;
        OrdTables_.TryGetValue(tableName, out ordTable);
        return ordTable;
    }

    /// 用回報表名稱,取得回報表.

    public RptTable RptTable(string tableName)
    {
        RptTable rptTable;
        RptTables_.TryGetValue(tableName, out rptTable);
        return rptTable;
    }
}
#endregion
