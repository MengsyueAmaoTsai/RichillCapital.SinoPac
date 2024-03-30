using System.Runtime.InteropServices;

namespace RichillCapital.SinoPac.Sor;

/// <summary>
/// 傳送下單要求前先檢查流量管制, 被管制的要求, 就放在 Queue 裡面, 等解除管制時自動送出.
/// </summary>
class SorFlowCtrlSender : IDisposable
{
    TImpl Impl_;
    /// <summary>
    /// 建構 SorFlowCtrlSender, owner死掉前必須先呼叫 this.Dispose().
    /// </summary>
    /// <param name="owner"></param>
    public SorFlowCtrlSender(SorClient owner)
    {
        Impl_ = CSorFlowCtrlSender_Create(ref owner.Client);
    }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorFlowCtrlSender_Create")]
    private static extern TImpl CSorFlowCtrlSender_Create(ref TImpl gcliImpl);

    /// <summary>
    /// 刪除 SorFlowCtrlSender 及相關資源.
    /// </summary>
    public void Dispose()
    {
        CSorFlowCtrlSender_Delete(ref Impl_);
    }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorFlowCtrlSender_Delete")]
    private static extern void CSorFlowCtrlSender_Delete(ref TImpl impl);

    /// <summary>
    /// 設定相關流量管制參數.
    /// </summary>
    public void SetFlowCtrl(UInt32 rate, UInt32 rateMS) { CSorFlowCtrlSender_SetFlowCtrl(ref Impl_, rate, rateMS); }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorFlowCtrlSender_SetFlowCtrl")]
    private static extern void CSorFlowCtrlSender_SetFlowCtrl(ref TImpl impl, UInt32 rate, UInt32 rateMS);

    /// <summary>
    /// 取得現在排隊的數量.
    /// </summary>
    public UInt32 PendingCount { get { return CSorFlowCtrlSender_PendingCount(ref Impl_); } }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorFlowCtrlSender_PendingCount")]
    private static extern UInt32 CSorFlowCtrlSender_PendingCount(ref TImpl impl);

    /// <summary>
    /// Ack Parser, 自動處理 SetAckTime() 相關事項
    /// </summary>
    public void AckParser(uint msgCode, string acks) { CSorFlowCtrlSender_AckParser(ref Impl_, msgCode, acks); }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorFlowCtrlSender_AckParser")]
    private static extern void CSorFlowCtrlSender_AckParser(ref TImpl impl, uint msgCode, string acks);

    /// <summary>
    /// 送出一筆下單要求, 若無法立即送出, 則會放到Queue之中自動傳送.
    /// reqmsg 需保留 5 bytes header.
    /// 傳回 true=已立即送出, false=已放到Queue之中等候送出.
    /// </summary>
    public bool SendSorRequest(string reqmsg) { return CSorFlowCtrlSender_SendSorRequest(ref Impl_, reqmsg, (uint)reqmsg.Length); }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorFlowCtrlSender_SendSorRequest")]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool CSorFlowCtrlSender_SendSorRequest(ref TImpl impl, string reqmsg, uint reqmsgLen);

    /// <summary>
    /// 送出一批下單要求, 傳回[立即送出]的筆數, 其餘要求會放到Queue之中自動傳送.
    /// reqmsg 需保留 5 bytes header, 每筆下單要求之間用 '\n' 分隔, 最後一筆不用加 '\n'
    /// </summary>
    public UInt32 SendSorRequests(string reqmsg) { return CSorFlowCtrlSender_SendSorRequests(ref Impl_, reqmsg, (uint)reqmsg.Length); }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorFlowCtrlSender_SendSorRequests")]
    private static extern UInt32 CSorFlowCtrlSender_SendSorRequests(ref TImpl impl, string reqmsg, uint reqmsgLen);
}
