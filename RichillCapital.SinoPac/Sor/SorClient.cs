using System.Runtime.InteropServices;
using RichillCapital.SharedKernel;
using RichillCapital.SharedKernel.Monads;
using RichillCapital.SinoPac.Sor.Events;
using RichillCapital.SinoPac.Sor.Models;

namespace RichillCapital.SinoPac.Sor;

public sealed partial class SorClient : IDisposable
{
    internal TImpl Impl_;

    SorClientDelegates Callbacks_ = new();

    private readonly QueryId _queryId = new();
    private readonly AccountManager _accountManager = new();
    private readonly TableManager _tableManager = new();

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

    public Result<IReadOnlyCollection<SorAccount>> GetAccounts()
    {
        if (State != SorClientState.SorClientState_ApReady)
        {
            return Error
                .Invalid("Cannot get accounts when not connected to server.")
                .ToResult<IReadOnlyCollection<SorAccount>>();
        }

        return _accountManager.Values
            .AsReadOnly()
            .ToResult<IReadOnlyCollection<SorAccount>>();
    }

    public Result QueryAccountBalance(SorAccount sorAccount, string currencyCode = "NTX")
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

        return SendRequest(0x80, request);
    }

    public Result QueryAccountPositions(SorAccount sorAccount, bool isSummary = true)
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

        return SendRequest(0x80, request);
    }

    private TaskResult GetSignInResult() => new(GetSignInResult(ref Impl_));

    public Result SendRequest(uint messageCode, string request)
    {

        if (State != SorClientState.SorClientState_ApReady)
        {
            return Error
                .Invalid("Cannot send request when not connected to server.")
                .ToResult();
        }

        if (!SendRequest(ref Impl_, messageCode, request, (uint)request.Length))
        {
            return Error
                .Invalid("Failed to send request.")
                .ToResult();
        }

        return Result.Success;
    }

    void HandleUnknownMessageCode(ref TImpl sender, IntPtr userData, uint msgCode, IntPtr pkptr, uint pksz)
    {
        Console.WriteLine("\n[OnSorUnknownMsgCodeCallback]");
    }

    void HandleConnect(ref TImpl sender, IntPtr userData, string errorMessage)
    {
        Console.WriteLine("\n[OnSorConnectCallback]");
        Console.WriteLine($"State = {State}");


        // 如果errorMessage是空的 表示連線成功
        // 如果errorMessage不是空的 表示連線失敗

        if (string.IsNullOrEmpty(errorMessage))
        {
            Console.WriteLine("Connected");
            return;
        }

        // 帳號不存在
        if (errorMessage.Contains("ORA-20003"))
        {
            Console.WriteLine("Customer not exists.");
        }

        // 密碼錯誤
        if (errorMessage.Contains("ORA-20004"))
        {
            Console.WriteLine("Invalid password.");
        }
    }

    void HandleApReady(ref TImpl sender, IntPtr userData)
    {
        Console.WriteLine("\n[OnSorApReadyCallback]");
        Console.WriteLine($"State = {State}");

        var signInResult = GetSignInResult();

        LoadAccounts(signInResult);
        SetRateLimit(signInResult);
        LoadTables(signInResult);
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

    private void LoadAccounts(TaskResult signInResult)
    {
        var headTable = signInResult.GetTableByName("head");
        var modTable = signInResult.GetTableByName("mod");
        var accountsTable = signInResult.GetTableByName("Accs");
        var recordsTable = signInResult.GetTableByName("records");

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

    private void SetRateLimit(TaskResult signInResult)
    {
        Console.WriteLine("SetRateLimit");
    }

    private void LoadTables(TaskResult signInResult) =>
        _tableManager.ParseSignInResult(signInResult);

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
    private Result RecoverExecutions() => SendRequest(0x83, "-----1" + "\x01" + "D");
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