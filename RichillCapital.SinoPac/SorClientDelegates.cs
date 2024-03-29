namespace RichillCapital.SinoPac.Sor;

struct SorClientDelegates
{
    public OnSorUnknownMsgCodeCallbackDelegate OnSorUnknownMsgCodeCallback;
    public OnSorConnectCallbackDelegate OnSorConnectCallback;
    public OnSorApReadyCallbackDelegate OnSorApReadyCallback;
    public OnSorTaskResultCallbackDelegate OnSorTaskResultCallback;
    public OnSorChgPassResultCallbackDelegate OnSorChgPassResultCallback;
    public OnSorRequestAckCallbackDelegate OnSorRequestAckCallback;
    public OnSorReportCallbackDelegate OnSorReportCallback;
    public OnSorClientDeleteCallbackDelegate OnSorClientDeleteCallback;
}


delegate void OnSorUnknownMsgCodeCallbackDelegate(ref TImpl sender, IntPtr userdata, uint msgCode, IntPtr pkptr, uint pksz);
delegate void OnSorConnectCallbackDelegate(ref TImpl sender, IntPtr userdata, string errmsg);
delegate void OnSorApReadyCallbackDelegate(ref TImpl sender, IntPtr userdata);
delegate void OnSorTaskResultCallbackDelegate(ref TImpl sender, IntPtr userdata, ref TImpl taskResult);
delegate void OnSorChgPassResultCallbackDelegate(ref TImpl sender, IntPtr userdata, string user, string result);
delegate void OnSorRequestAckCallbackDelegate(ref TImpl sender, IntPtr userdata, uint msgCode, string result);
delegate void OnSorReportCallbackDelegate(ref TImpl sender, IntPtr userdata, string result);
delegate void OnSorClientDeleteCallbackDelegate(ref TImpl sender, IntPtr userdata);


/// 當收到[不明訊息]時的通知.
public delegate void OnSorUnknownMsgCodeEvent(SorClient sender, uint msgCode, IntPtr pkptr, uint pksz);
/// SORS連線訊息通知, if(errmsg.empty()) 表示成功, 此時可呼叫 sender.ServerName() 取得主機名稱.
public delegate void OnSorConnectEvent(SorClient sender, string errmsg);
/// SORS已備妥,可以下單或執行特定作業.
public delegate void OnSorApReadyEvent(SorClient sender);
/// 一般作業結果通知.
public delegate void OnSorTaskResultEvent(SorClient sender, SorTaskResult taskResult);
/// 改密碼結果, if(result.empty()) 改密碼成功! else result=失敗訊息.
public delegate void OnSorChgPassResultEvent(SorClient sender, string user, string result);
/// 下單回覆.
public delegate void OnSorRequestAckEvent(SorClient sender, uint msgCode, string result);
/// 委託回補, 委託主動回報, 成交回報.
public delegate void OnSorReportEvent(SorClient sender, string result);
/// 當 sender 要被殺死前的通知.
public delegate void OnSorClientDeleteEvent(SorClient sender);