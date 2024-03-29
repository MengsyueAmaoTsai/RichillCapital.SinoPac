using System.Runtime.InteropServices;
using RichillCapital.SharedKernel.Monads;
using RichillCapital.SinoPac.Sor.Events;

namespace RichillCapital.SinoPac.Sor;

public sealed partial class SorClient : IDisposable
{
    internal TImpl Impl_;

    SorClientDelegates Callbacks_ = new();

    private readonly QueryId _queryId = new();
    private readonly Accs _accountManager = new();
    private readonly TablesMgr _tableManager = new();

    public event EventHandler<SorStateChangedEvent>? StateChanged;

    public SorClient(bool isEventOnMessageLoop = false)
    {
        Callbacks_.OnUnknownMessageCode = HandleUnknownMessageCode;
        Callbacks_.OnConnect = HandleConnect;
        Callbacks_.OnApReady = HandleApReady;
        Callbacks_.OnTaskResult = HandleTaskResult;
        Callbacks_.OnRequestAck = HandleRequestAck;
        Callbacks_.OnReport = HandleReport;
        // Callbacks_.OnSorClientDeleteCallback = null;

        Impl_ = isEventOnMessageLoop ?
            CreateOnMessageLoop(ref Callbacks_, IntPtr.Zero) :
            Create(ref Callbacks_, IntPtr.Zero);
    }

    public SorClientState State => GetClientState(ref Impl_);

    public bool IsConnected => IsSessionConnected(ref Impl_);

    public void Dispose() => Delete(ref Impl_);

    public Result Connect(string userId, string password)
    {
        Connect(
            ref Impl_,
            SorApi.DefaultHost,
            "SorApiCS",
            SorApi.Version,
            SorApi.SystemId,
            userId,
            password);

        return Result.Success;
    }

    public Result Disconnect()
    {
        Disconnect(ref Impl_);

        return Result.Success;
    }

    public IReadOnlyCollection<Acc> GetAccounts() => _accountManager.Values.AsReadOnly();

    public void QueryAccountBalance(Acc sorAccount, string currencyCode = "NTX")
    {
        var taskId = "QBal";

        var parameters = new Dictionary<string, string>
        {
            { "bkno", sorAccount.BrokerageNumber},
            { "ivac", sorAccount.Number},
            { "MCODE", currencyCode }
        };

        if (sorAccount.IsSubAccount())
        {
            parameters.Add("subac", sorAccount.SubAccountNumber);
        }

        var sep = '\x01';
        var request = $"-----{_queryId.Next()}{sep}{taskId}{sep}{parameters
            .Select(p => $"{p.Key}={p.Value}")}";

        SendRequest(0x80, request);
    }

    public void QueryAccountPositions(Acc sorAccount, bool isSummary = true)
    {
        var taskId = "QINV";

        var parameters = new Dictionary<string, string>
        {
            { "Bkno", sorAccount.BrokerageNumber },
            { "Ivac", sorAccount.Number },
            { "QSUM", isSummary ? "Y" : "N" }
        };

        if (sorAccount.IsSubAccount())
        {
            parameters.Add("subac", sorAccount.SubAccountNumber);
        }

        var sep = '\x01';
        var request = $"-----{_queryId.Next()}{sep}{taskId}{sep}{parameters
            .Select(p => $"{p.Key}={p.Value}")}";

        SendRequest(0x80, request);
    }
    private SorTaskResult GetSignInResult() => new(GetSignInResult(ref Impl_));

    public bool SendRequest(uint messageCode, string request)
    {
        var isSuccess = SendRequest(ref Impl_, messageCode, request, (uint)request.Length);

        if (!isSuccess)
        {
            Console.WriteLine("SendSorRequest failed");
        }

        return isSuccess;
    }

    void HandleUnknownMessageCode(ref TImpl sender, IntPtr userData, uint msgCode, IntPtr pkptr, uint pksz)
    {
        Console.WriteLine("\n[OnSorUnknownMsgCodeCallback]");
    }

    void HandleConnect(ref TImpl sender, IntPtr userData, string errorMessage)
    {
        Console.WriteLine("\n[OnSorConnectCallback]");
        Console.WriteLine($"State = {State}");
    }

    void HandleApReady(ref TImpl sender, IntPtr userData)
    {
        Console.WriteLine("\n[OnSorApReadyCallback]");
        Console.WriteLine($"State = {State}");

        var result = GetSignInResult();

        LoadAccounts(result);
        SetRateLimit();
        LoadTables(result);
        RecoverExecutions();
    }

    void HandleTaskResult(ref TImpl sender, IntPtr userData, ref TImpl taskResult)
    {
        Console.WriteLine("\n[OnSorTaskResultCallback]");
    }

    void HandleRequestAck(ref TImpl sender, IntPtr userData, uint msgCode, string result)
    {
        Console.WriteLine("\n[OnSorRequestAckCallback]");
    }

    void HandleReport(ref TImpl sender, IntPtr userData, string result)
    {
        Console.WriteLine("\n[OnSorReportCallback]");
    }

    private void LoadAccounts(SorTaskResult signInResult)
    {
        var headTable = signInResult.NameTable("head");
        var modTable = signInResult.NameTable("mod");
        var accountsTable = signInResult.NameTable("Accs");
        var recordsTable = signInResult.NameTable("records");

        int.TryParse(
            modTable.RecordField(0, (headTable.IsInvalid ? modTable : headTable).Fields.NameField("sgnact")),
            out int signInAct);

        _accountManager.SorTableParser(
            accountsTable.IsInvalid ? recordsTable : accountsTable,
            SorApi.Dll.Certificate,
            signInAct);

        var accounts = _accountManager.Values;

        foreach (var acc in accounts)
        {
            Console.WriteLine($"{acc.Key}");
        }
    }

    private void SetRateLimit()
    {
        //             table = sgnResult.NameTable("FlowCtrl");
        //             SorFields fields = table.Fields;
        //             TIndex rate = 0;
        //             TIndex rateMS = 0;
        //             string fldRate = table.RecordField(0, fields.NameField("ORate"));
        //             string fldRateMS = table.RecordField(0, fields.NameField("ORateMS"));
        //             if (fldRate != null && fldRateMS != null)
        //             {
        //                 TIndex.TryParse(fldRate, out rate);
        //                 TIndex.TryParse(fldRateMS, out rateMS);
        //             }
        //             SorFlowSender_.SetFlowCtrl(rate, rateMS);
        //             if (rate <= 0 || rateMS <= 0)
        //                 Console.WriteLine("User : {0}, 無流量管制參數", User);
        //             else
        //                 Console.WriteLine("User : {0}, 流量管制參數: {0}筆 / 每{1}{2}"
        //                                                , User
        //                                                , rate
        //                                                , rateMS >= 1000 ? (rateMS / 1000.0) : rateMS
        //                                                , rateMS >= 1000 ? "秒" : "ms");
    }

    private void LoadTables(SorTaskResult signInResult)
    {
        _tableManager.ParseSgnResult(signInResult);
    }

    // 回補委託: 回補全部(「,D」 = 包含成交明細) (SendSorRequest() 必須保留前5碼).
    // "-----1"                 回補全部，不包含成交明細
    // "-----1" + "\x01" + "D"  補全部委託,含成交明細
    // "-----1" + "\x01" + "Dw" 回補全部有剩餘量，並包含成交明細
    // "-----1" + "\x01" + "M"  補有成交(或UserID相同)的委託,含成交明細
    // "-----1" + "\x01" + "m"  補有成交(或UserID相同)的委託,不含成交明細
    // "-----1" + "\x01" + "M0" 僅補有成交(不考慮UserID)的委託,含成交明細
    // "-----1" + "\x01" + "m0" 僅補有成交(不考慮UserID)的委託,不含成交明細]
    // "-----2" + "\x01" + "YYYYMMDDHHMMSS,D"  指定時間，包含成交明細
    // "-----2" + "\x01" + "YYYYMMDDHHMMSS,Dw" 指定時間有剩餘量，包含成交明細
    // "-----2" + "\x01" + "YYYYMMDDHHMMSS,m"  指定時間，僅回補有成交, 不包含成交明細
    // "-----2" + "\x01" + "YYYYMMDDHHMMSS,M"  指定時間，僅回補有成交, 並包含成交明細
    // "-----0"                 不回補,且不收任何回報
    // "-----0m"                不回補、且不收委託回報，僅收成交回報
    private void RecoverExecutions()
    {
        var isSuccess = SendRequest(0x83, "-----1" + "\x01" + "D");

        if (!isSuccess)
        {
            Console.WriteLine("RecoverExecutions failed");
        }
    }
}

public sealed partial class SorClient : IDisposable
{
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorClient_Create_OnMessageLoop")]
    private static extern TImpl CreateOnMessageLoop(
        ref SorClientDelegates evHandler,
        IntPtr userData);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorClient_Create")]
    private static extern TImpl Create(
        ref SorClientDelegates evHandler,
        IntPtr userData);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorClient_Connect")]
    private static extern void Connect(
        ref TImpl cli,
        string host,
        string clientName,
        string version,
        string systemId,
        string userId,
        string password);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorClient_Disconnect")]
    private static extern void Disconnect(ref TImpl cli);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorClient_Delete")]
    private static extern void Delete(ref TImpl cli);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorClient_State")]
    private static extern SorClientState GetClientState(ref TImpl cli);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorClient_IsSessionConnected")]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool IsSessionConnected(ref TImpl cli);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorClient_SgnResult")]
    private static extern TImpl GetSignInResult(ref TImpl cli);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorClient_SendSorRequest")]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool SendRequest(
        ref TImpl cli,
        uint messageCode,
        string request,
        uint requestLength);
}