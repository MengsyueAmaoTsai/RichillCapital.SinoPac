using System.Runtime.InteropServices;

namespace RichillCapital.SinoPac.Sor;

public sealed partial class SorFields
{
    TImpl Impl_;
    internal SorFields(TImpl impl)
    {
        Impl_ = impl;
    }

    /// 用欄位名稱取得欄位,若不存在則傳回null.
    public SorField NameField(string fieldName) { return SorField.MakeSorField(CSorFields_NameField(ref Impl_, fieldName)); }

    /// 用欄位名稱取得欄位索引,若不存在則傳回 SorField.InvalidIndex
    public uint NameFieldIndex(string fieldName)
    {
        TImpl impl = CSorFields_NameField(ref Impl_, fieldName);
        return (impl.IsInvalid ? SorField.InvalidIndex : SorField.GetIndex(ref impl));
    }

    /// 用索引取得欄位,若不存在則傳回null.
    public SorField IndexField(uint fieldIndex) { return SorField.MakeSorField(CSorFields_IndexField(ref Impl_, fieldIndex)); }

    public uint Count { get { return CSorFields_Count(ref Impl_); } }
}

public sealed partial class SorFields
{

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorFields_NameField")]
    private static extern TImpl CSorFields_NameField(ref TImpl impl, string fieldName);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorFields_IndexField")]
    private static extern TImpl CSorFields_IndexField(ref TImpl impl, uint fieldIndex);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorFields_Count")]
    private static extern uint CSorFields_Count(ref TImpl impl);
}