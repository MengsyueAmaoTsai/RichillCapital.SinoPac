using System.Runtime.InteropServices;

namespace RichillCapital.SinoPac.Sor;

/// <summary>
/// SORS資料表裡面的 [一個欄位] 型別.
/// </summary>
public class SorField
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

    /// <summary>
    /// 取得欄位屬性列表
    /// </summary>
    public SorProperties Properties { get { return new SorProperties(CSorField_Properties(ref Impl_)); } }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorField_Properties")]
    private static extern TImpl CSorField_Properties(ref TImpl impl);

    /// <summary>
    /// 取得欄位的索引, InvalidIndex 表示欄位有誤.
    /// </summary>
    public uint Index { get { return CSorField_Index(ref Impl_); } }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorField_Index")]
    internal static extern uint CSorField_Index(ref TImpl impl);

    /// <summary>
    /// 無效的欄位索引.
    /// </summary>
    public const uint InvalidIndex = 0xffffffff;
}
