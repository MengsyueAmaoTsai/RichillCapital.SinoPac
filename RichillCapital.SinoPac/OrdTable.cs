using RichillCapital.SinoPac.Sor.Models;

namespace RichillCapital.SinoPac.Sor;
#region Sor [委託表格/回報表格] 管理


/// 委託表格內容.
public class OrdTable : KeyedRptTable
{
    uint IBrkNo_;
    uint IIvacNo_;
    uint ISubacNo_;
    uint IOrdNo_;
    uint ILeavesQty_;
    internal ExecutionTable DDSTable_;
    internal ReqKillTable ReqKillTable_;

    public OrdTable(SorTable table)
        : base(table)
    {
        SorFields fields = table.Fields;
        IBrkNo_ = fields.NameFieldIndex("BrkNo");
        IIvacNo_ = fields.NameFieldIndex("IvacNo");
        ISubacNo_ = fields.NameFieldIndex("SubacNo");
        IOrdNo_ = fields.NameFieldIndex("OrdNo");
        ILeavesQty_ = fields.NameFieldIndex("LeavesQty");
    }
    public uint IBrkNo { get { return IBrkNo_; } }
    public uint IIvacNo { get { return IIvacNo_; } }
    public uint ISubacNo { get { return ISubacNo_; } }
    public uint IOrdNo { get { return IOrdNo_; } }
    public uint ILeavesQty { get { return ILeavesQty_; } }
    public ReqKillTable ReqKillTable { get { return ReqKillTable_; } }

    internal void CreateDDSTable(SorTable ddst)
    {
        DDSTable_ = new ExecutionTable(ddst);
    }


    /// 建立要送出的下單訊息內容字串, 不含 ACK ReqSeqNo.
    /// acc = 用哪個帳號的憑證簽章.

    public static string MakeRequestString(string[] flds, string tableID, SorAccount acc, uint iDigSgn, out string errMsg)
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
#endregion
