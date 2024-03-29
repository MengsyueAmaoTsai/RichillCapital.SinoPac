using System.Runtime.InteropServices;

namespace RichillCapital.SinoPac.Sor;

public sealed partial class SorField
{
    internal TImpl Impl_;

    internal SorField(TImpl impl)
    {
        Impl_ = impl;
    }

    internal static SorField MakeSorField(TImpl impl)
    {
        return impl.IsInvalid ? null : new SorField(impl);
    }

    /// 取得欄位屬性列表
    public SorProperties Properties => new SorProperties(CSorField_Properties(ref Impl_));

    /// 取得欄位的索引, InvalidIndex 表示欄位有誤.
    public uint Index => GetIndex(ref Impl_);

    /// 無效的欄位索引.
    public const uint InvalidIndex = 0xffffffff;
}

public sealed partial class SorField
{
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorField_Properties")]
    private static extern TImpl CSorField_Properties(ref TImpl impl);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorField_Index")]
    internal static extern uint GetIndex(ref TImpl impl);
}