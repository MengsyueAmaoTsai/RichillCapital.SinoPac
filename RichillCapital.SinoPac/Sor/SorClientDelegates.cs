namespace RichillCapital.SinoPac.Sor;

struct SorClientDelegates
{
    public OnUnknownMessageCodeDelegate OnUnknownMessageCode;
    public OnConnectDelegate OnConnect;
    public OnApReadyDelegate OnApReady;
    public OnTaskResultDelegate OnTaskResult;
    public OnChangePasswordResultDelegate OnChangePasswordResult;
    public OnRequestAckDelegate OnRequestAck;
    public OnReportDelegate OnReport;
    public OnDeletedDelegate OnDeleted;
}

delegate void OnUnknownMessageCodeDelegate(ref TImpl sender, IntPtr userData, uint messageCode, IntPtr pkPtr, uint pkSz);

delegate void OnConnectDelegate(ref TImpl sender, IntPtr userData, string errorMessage);

delegate void OnApReadyDelegate(ref TImpl sender, IntPtr userData);

delegate void OnTaskResultDelegate(ref TImpl sender, IntPtr userData, ref TImpl taskResult);

delegate void OnChangePasswordResultDelegate(ref TImpl sender, IntPtr userData, string userId, string result);

delegate void OnRequestAckDelegate(ref TImpl sender, IntPtr userData, uint messageCode, string result);

delegate void OnReportDelegate(ref TImpl sender, IntPtr userData, string result);

delegate void OnDeletedDelegate(ref TImpl sender, IntPtr userData);
