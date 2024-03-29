using System.Runtime.InteropServices;

namespace RichillCapital.SinoPac.Sor;

public class SorClient : IDisposable
{
    internal TImpl Impl_;
    CSorClientCallbacks Callbacks_ = new CSorClientCallbacks();

    #region SorClient Callbacks 轉 C# event
    void OnSorUnknownMsgCodeCallback(ref TImpl sender, IntPtr userdata, uint msgCode, IntPtr pkptr, uint pksz)
    {
        if (OnSorUnknownMsgCodeEvent != null)
            OnSorUnknownMsgCodeEvent(this, msgCode, pkptr, pksz);
    }
    void OnSorConnectCallback(ref TImpl sender, IntPtr userdata, string errmsg)
    {
        if (OnSorConnectEvent != null)
            OnSorConnectEvent(this, errmsg);
    }
    void OnSorApReadyCallback(ref TImpl sender, IntPtr userdata)
    {
        if (OnSorApReadyEvent != null)
            OnSorApReadyEvent(this);
    }
    void OnSorTaskResultCallback(ref TImpl sender, IntPtr userdata, ref TImpl taskResult)
    {
        if (OnSorTaskResultEvent != null)
            OnSorTaskResultEvent(this, new SorTaskResult(taskResult));
    }
    void OnSorChgPassResultCallback(ref TImpl sender, IntPtr userdata, string user, string result)
    {
        if (OnSorChgPassResultEvent != null)
            OnSorChgPassResultEvent(this, user, result);
    }
    void OnSorRequestAckCallback(ref TImpl sender, IntPtr userdata, uint msgCode, string result)
    {
        if (OnSorRequestAckEvent != null)
            OnSorRequestAckEvent(this, msgCode, result);
    }
    void OnSorReportCallback(ref TImpl sender, IntPtr userdata, string result)
    {
        if (OnSorReportEvent != null)
            OnSorReportEvent(this, result);
    }
    void OnSorClientDeleteCallback(ref TImpl sender, IntPtr userdata)
    {
        if (OnSorClientDeleteEvent != null)
            OnSorClientDeleteEvent(this);
    }
    #endregion

    #region 建構 & 解構
    /// 使用 MessageLoop 事件通知, 建構 CSorClient, evHandler 會被複製一份在 CSorClient 裡面.
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorClient_Create_OnMessageLoop")]
    private static extern TImpl CSorClient_Create_OnMessageLoop(ref CSorClientCallbacks evHandler, IntPtr userdata);
    /// 使用 Thread 事件通知, 建構 CSorClient, evHandler 會被複製一份在 CSorClient 裡面.
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorClient_Create")]
    private static extern TImpl CSorClient_Create(ref CSorClientCallbacks evHandler, IntPtr userdata);
    /// <summary>
    /// 建構.
    /// </summary>
    /// <param name="isEventOnMessageLoop">
    ///   true=在MessageLoop觸發事件, false=事件觸發可能在任一Thread,
    ///   只有 OnSorClientDeleteEvent事件 不受這個限制, 此事件一律都在呼叫 Dispose()的那個Thread, 且在返回前觸發.
    ///  </param>
    public SorClient(bool isEventOnMessageLoop = false)
    {
        Callbacks_.OnSorUnknownMsgCodeCallback = OnSorUnknownMsgCodeCallback;
        Callbacks_.OnSorConnectCallback = OnSorConnectCallback;
        Callbacks_.OnSorApReadyCallback = OnSorApReadyCallback;
        Callbacks_.OnSorTaskResultCallback = OnSorTaskResultCallback;
        Callbacks_.OnSorChgPassResultCallback = OnSorChgPassResultCallback;
        Callbacks_.OnSorRequestAckCallback = OnSorRequestAckCallback;
        Callbacks_.OnSorReportCallback = OnSorReportCallback;
        Callbacks_.OnSorClientDeleteCallback = null;// OnSorClientDeleteCallback;
        Impl_ = isEventOnMessageLoop ? CSorClient_Create_OnMessageLoop(ref Callbacks_, IntPtr.Zero) : CSorClient_Create(ref Callbacks_, IntPtr.Zero);
    }

    /// 解構 CSorClient.
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorClient_Delete")]
    private static extern void CSorClient_Delete(ref TImpl cli);
    /// <summary>
    ///  解構.
    /// </summary>
    public void Dispose()
    {
        CSorClient_Delete(ref Impl_);
    }
    #endregion

    #region Events
    /// 當收到[不明訊息]時的通知.
    public OnSorUnknownMsgCodeEvent OnSorUnknownMsgCodeEvent;
    /// SORS連線訊息通知, if(errmsg.empty()) 表示成功, 此時可呼叫 sender.ServerName() 取得主機名稱.
    public OnSorConnectEvent OnSorConnectEvent;
    /// SORS已備妥,可以下單或執行特定作業.
    public OnSorApReadyEvent OnSorApReadyEvent;
    /// 一般作業結果通知.
    public OnSorTaskResultEvent OnSorTaskResultEvent;
    /// 改密碼結果, if(result.empty()) 改密碼成功! else result=失敗訊息.
    public OnSorChgPassResultEvent OnSorChgPassResultEvent;
    /// 下單回覆.
    public OnSorRequestAckEvent OnSorRequestAckEvent;
    /// 委託回補, 委託主動回報, 成交回報.
    public OnSorReportEvent OnSorReportEvent;
    /// 當 sender 要被殺死前的通知.
    public OnSorClientDeleteEvent OnSorClientDeleteEvent;
    #endregion

    #region 連線登入 & 切斷連線
    /// <summary>
    /// 建立與SORS的連線並登入.
    /// </summary>
    /// <param name="connParam">連線參數, 格式: "host:port", host = SORS 主機 ip 或 domain name</param>
    /// <param name="sysid">登入的系統名稱,由券商指定</param>
    /// <param name="user">使用者ID</param>
    /// <param name="pass">使用者密碼</param>
    public void Connect(string connParam, string sysid, string user, string pass)
    {
        CSorClient_Connect(ref Impl_, connParam, "SorApiCS", "1.0.0.0", sysid, user, pass);
    }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorClient_Connect")]
    private static extern void CSorClient_Connect(ref TImpl cli, string connParam, string cliApName, string cliApVer, string sysid, string user, string pass);

    /// <summary>
    /// 切斷連線.
    /// </summary>
    public void Disconnect()
    {
        CSorClient_Disconnect(ref Impl_);
    }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorClient_Disconnect")]
    private static extern void CSorClient_Disconnect(ref TImpl cli);
    #endregion

    #region Get State & Result
    /// <summary>
    /// 取得現在的狀態.
    /// </summary>
    public SorClientState State { get { return CSorClient_State(ref Impl_); } }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorClient_State")]
    private static extern SorClientState CSorClient_State(ref TImpl cli);

    /// <summary>
    /// 是否已與SORS建立連線, 取得SORS服務端名稱 (包含已登入 or 登入失敗).
    /// </summary>
    public bool IsSessionConnected { get { return CSorClient_IsSessionConnected(ref Impl_); } }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorClient_IsSessionConnected")]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool CSorClient_IsSessionConnected(ref TImpl cli);

    /// <summary>
    /// 取得登入結果.
    /// </summary>
    public SorTaskResult SgnResult { get { return new SorTaskResult(CSorClient_SgnResult(ref Impl_)); } }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorClient_SgnResult")]
    private static extern TImpl CSorClient_SgnResult(ref TImpl cli);
    #endregion

    #region Send Request
    /// <summary>
    /// 送出SORS要求訊息.
    /// reqCtx裡面不可有中文,因為在取 reqCtx.Length 時, .NET裡面一個中文字長度=1, 但ASCII(big5)中文字長度=2
    /// </summary>
    /// <param name="msgCode">0x81=下單要求, 0x83=回補要求, 0x84=無流量管制時的無ACK下單要求</param>
    /// <param name="reqCtx">要送出的下單要求內容,前5碼必須保留給header</param>
    /// <returns>true成功送出,false=無法送出(例如:0x80查詢超過流量上限)</returns>
    public bool SendSorRequest(uint msgCode, string reqCtx)
    {
        return CSorClient_SendSorRequest(ref Impl_, msgCode, reqCtx, (uint)reqCtx.Length);
    }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorClient_SendSorRequest")]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool CSorClient_SendSorRequest(ref TImpl cli, uint msgCode, string reqCtx, uint reqLen);

    /// <summary>
    /// <summary>
    /// 改密碼, 必須先建立連線才能改密碼: State >= ConnectState.Connected || SignonError.
    /// </summary>
    /// <returns>若 State >= ConnectState.Connected 則傳回 true, 否則傳回 false 無法進行改密碼操作</returns>
    public bool ChgPass(string user, string oldpass, string newpass)
    {
        return CSorClient_ChgPass(ref Impl_, user, oldpass, newpass);
    }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorClient_ChgPass")]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool CSorClient_ChgPass(ref TImpl cli, string user, string oldpass, string newpass);
    #endregion
}
