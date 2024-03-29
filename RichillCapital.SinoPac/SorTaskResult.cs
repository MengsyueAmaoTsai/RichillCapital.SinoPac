using System.Runtime.InteropServices;

namespace RichillCapital.SinoPac.Sor;

public class SorTaskResult
{
    TImpl Impl_;
    internal SorTaskResult(TImpl impl)
    {
        Impl_ = impl;
    }

    /// <summary>
    /// 取得此Task的識別碼.
    /// </summary>
    public string WorkID { get { return CSorTaskResult_WorkID(ref Impl_); } }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorTaskResult_WorkID_B")]
    [return: MarshalAs(UnmanagedType.AnsiBStr)]
    private static extern string CSorTaskResult_WorkID(ref TImpl impl);

    /// <summary>
    /// 取得原始結果字串.
    /// </summary>
    public string OrigResult { get { return CSorTaskResult_OrigResult(ref Impl_); } }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorTaskResult_OrigResult_B")]
    [return: MarshalAs(UnmanagedType.AnsiBStr)]
    private static extern string CSorTaskResult_OrigResult(ref TImpl impl);

    /// <summary>
    /// 使用 tableName 取得結果資料表.
    /// </summary>
    public SorTable NameTable(string tableName) { return new SorTable(CSorTaskResult_NameTable(ref Impl_, tableName)); }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorTaskResult_NameTable")]
    private static extern TImpl CSorTaskResult_NameTable(ref TImpl impl, string tableName);

    /// <summary>
    /// 使用 index 取得結果資料表.
    /// </summary>
    public SorTable IndexTable(uint index) { return new SorTable(CSorTaskResult_IndexTable(ref Impl_, index)); }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorTaskResult_IndexTable")]
    private static extern TImpl CSorTaskResult_IndexTable(ref TImpl impl, uint index);

    /// <summary>
    /// 取得資料表數量.
    /// </summary>
    public uint TablesCount { get { return CSorTaskResult_TableCount(ref Impl_); } }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorTaskResult_TableCount")]
    private static extern uint CSorTaskResult_TableCount(ref TImpl impl);
}
