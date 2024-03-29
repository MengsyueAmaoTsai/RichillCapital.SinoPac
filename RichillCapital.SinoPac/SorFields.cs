using System.Runtime.InteropServices;

namespace RichillCapital.SinoPac.Sor;

/// <summary>
/// SORS [資料表] 的 [欄位列表].
/// </summary>
public class SorFields
{
    TImpl Impl_;
    internal SorFields(TImpl impl)
    {
        Impl_ = impl;
    }

    /// <summary>
    /// 用欄位名稱取得欄位,若不存在則傳回null.
    /// </summary>
    public SorField NameField(string fieldName) { return SorField.MakeSorField(CSorFields_NameField(ref Impl_, fieldName)); }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorFields_NameField")]
    private static extern TImpl CSorFields_NameField(ref TImpl impl, string fieldName);

    /// <summary>
    /// 用欄位名稱取得欄位索引,若不存在則傳回 SorField.InvalidIndex
    /// </summary>
    public uint NameFieldIndex(string fieldName)
    {
        TImpl impl = CSorFields_NameField(ref Impl_, fieldName);
        return (impl.IsInvalid ? SorField.InvalidIndex : SorField.CSorField_Index(ref impl));
    }

    /// <summary>
    /// 用索引取得欄位,若不存在則傳回null.
    /// </summary>
    public SorField IndexField(uint fieldIndex) { return SorField.MakeSorField(CSorFields_IndexField(ref Impl_, fieldIndex)); }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorFields_IndexField")]
    private static extern TImpl CSorFields_IndexField(ref TImpl impl, uint fieldIndex);

    /// <summary>
    /// 取得欄位數量.
    /// </summary>
    public uint Count { get { return CSorFields_Count(ref Impl_); } }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorFields_Count")]
    private static extern uint CSorFields_Count(ref TImpl impl);
}
