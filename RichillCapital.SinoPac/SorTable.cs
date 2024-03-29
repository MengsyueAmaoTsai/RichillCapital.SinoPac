using System.Runtime.InteropServices;

namespace RichillCapital.SinoPac.Sor;

public sealed partial class SorTable
{
    TImpl Impl_;
    internal SorTable(TImpl impl)
    {
        Impl_ = impl;
    }
    public override string ToString()
    {
        return Properties.DisplayText;
    }

    /// 是否為無效表格.
    public bool IsInvalid => Impl_.IsInvalid;

    /// 取得表格屬性列表
    public SorProperties Properties => new SorProperties(CSorTable_Properties(ref Impl_));

    /// 取得表格的欄位列表.
    public SorFields Fields => new SorFields(CSorTable_Fields(ref Impl_));

    /// 取得表格的資料筆數.
    public uint RecordsCount => CSorTable_RecordsCount(ref Impl_);

    /// 取得表格的某資料的某欄位(使用index)內容, 傳回 null 表示無該筆資料或欄位.
    public string RecordIndexField(uint recordIndex, uint fieldIndex) =>
        CSorTable_RecordIndexField(ref Impl_, recordIndex, fieldIndex);

    /// 取得表格的某資料的某欄位(使用SorField)內容, 傳回 null 表示無該筆資料或欄位.
    public string RecordField(uint recordIndex, SorField field) =>
        field == null ? null : CSorTable_RecordField(ref Impl_, recordIndex, ref field.Impl_);

    /// 取得此表格所屬的市場別屬性, 可能有多個市場別, 請使用 bit 判斷.
    public SorMarketFlag MktFlag => CSorTable_MktFlag(ref Impl_);
}

public sealed partial class SorTable
{
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorTable_Properties")]
    private static extern TImpl CSorTable_Properties(ref TImpl impl);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorTable_Fields")]
    private static extern TImpl CSorTable_Fields(ref TImpl impl);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorTable_RecordsCount")]
    private static extern uint CSorTable_RecordsCount(ref TImpl impl);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorTable_RecordIndexField_B")]
    [return: MarshalAs(UnmanagedType.AnsiBStr)]
    private static extern string CSorTable_RecordIndexField(ref TImpl impl, uint recordIndex, uint fieldIndex);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorTable_RecordField_B")]
    [return: MarshalAs(UnmanagedType.AnsiBStr)]
    private static extern string CSorTable_RecordField(ref TImpl impl, uint recordIndex, ref TImpl fieldImpl);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorTable_MktFlag")]
    private static extern SorMarketFlag CSorTable_MktFlag(ref TImpl impl);
}