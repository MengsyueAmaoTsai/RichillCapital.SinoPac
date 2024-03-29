using System.Runtime.InteropServices;

namespace RichillCapital.SinoPac.Sor;

internal sealed partial class TaskResult
{
    TImpl Impl_;

    internal TaskResult(TImpl impl)
    {
        Impl_ = impl;
    }

    public string Id => GetTaskId(ref Impl_);

    public string OrigResult => CSorTaskResult_OrigResult(ref Impl_);

    public uint TableCount => GetTableCount(ref Impl_);

    public SorTable GetTableByName(string tableName) => new(GetTableByName(ref Impl_, tableName));

    /// 使用 index 取得結果資料表.
    public SorTable GetTableByIndex(uint index) => new(GetTableByIndex(ref Impl_, index));

}

internal sealed partial class TaskResult
{
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorTaskResult_WorkID_B")]
    [return: MarshalAs(UnmanagedType.AnsiBStr)]
    private static extern string GetTaskId(ref TImpl impl);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorTaskResult_OrigResult_B")]
    [return: MarshalAs(UnmanagedType.AnsiBStr)]
    private static extern string CSorTaskResult_OrigResult(ref TImpl impl);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorTaskResult_NameTable")]
    private static extern TImpl GetTableByName(ref TImpl impl, string tableName);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorTaskResult_IndexTable")]
    private static extern TImpl GetTableByIndex(ref TImpl impl, uint index);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorTaskResult_TableCount")]
    private static extern uint GetTableCount(ref TImpl impl);
}