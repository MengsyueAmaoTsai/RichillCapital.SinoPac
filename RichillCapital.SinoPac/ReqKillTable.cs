namespace RichillCapital.SinoPac.Sor;
#region Sor [委託表格/回報表格] 管理


/// 刪單要求表.
public class ReqKillTable
{
    SorMarketFlag MktFlag_;
    uint IdxFldAmendKey_;
    uint IdxFldOrgSorRID_;
    uint IdxDigSgn_;
    string TableID_;
    string[] ReqFlds_;


    /// 建構.

    private ReqKillTable(SorTable reqTable, uint idxFldAmendKey, uint idxFldOrgSorRID, string tableID)
    {
        MktFlag_ = reqTable.MktFlag;
        IdxFldAmendKey_ = idxFldAmendKey;
        IdxFldOrgSorRID_ = idxFldOrgSorRID;
        IdxDigSgn_ = reqTable.Fields.NameFieldIndex("DigSgn");
        TableID_ = tableID;
        ReqFlds_ = new string[reqTable.Fields.Count];
    }


    /// 建構.

    public ReqKillTable(SorTable reqTable, uint idxFldQty, uint idxFldAmendKey, uint idxFldOrgSorRID, string tableID)
        : this(reqTable, idxFldAmendKey, idxFldOrgSorRID, tableID)
    {
        if (idxFldQty < reqTable.Fields.Count)
            ReqFlds_[idxFldQty] = "0";
    }


    /// 建構.

    public ReqKillTable(SorTable reqTable, uint idxFldBidQty, uint idxFldOfferQty, uint idxFldAmendKey, uint idxFldOrgSorRID, string tableID)
        : this(reqTable, idxFldAmendKey, idxFldOrgSorRID, tableID)
    {
        if (idxFldBidQty < reqTable.Fields.Count)
            ReqFlds_[idxFldBidQty] = "0";
        if (idxFldOfferQty < reqTable.Fields.Count)
            ReqFlds_[idxFldOfferQty] = "0";
    }


    /// 此刪單表可操作的市場.

    public SorMarketFlag MktFlag { get { return MktFlag_; } }


    /// 建立委託刪單要求字串.

    public string MakeKillReqStr(SorOrder ord, out string errMsg)
    {
        if (IdxFldAmendKey_ != SorField.InvalidIndex)
            ReqFlds_[IdxFldAmendKey_] = ord.AmendKey;
        if (IdxFldOrgSorRID_ != SorField.InvalidIndex)
            ReqFlds_[IdxFldOrgSorRID_] = ord.OrgSorRID;
        return OrdTable.MakeRequestString(ReqFlds_, TableID_, ord.Account, IdxDigSgn_, out errMsg);
    }
}
#endregion
