using System.Runtime.InteropServices;

namespace RichillCapital.SinoPac.Sor;

public class SorTable
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
    /// <summary>
    /// 是否為無效表格.
    /// </summary>
    public bool IsInvalid { get { return Impl_.IsInvalid; } }

    /// <summary>
    /// 取得表格屬性列表
    /// </summary>
    public SorProperties Properties { get { return new SorProperties(CSorTable_Properties(ref Impl_)); } }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorTable_Properties")]
    private static extern TImpl CSorTable_Properties(ref TImpl impl);

    /// <summary>
    /// 取得表格的欄位列表.
    /// </summary>
    public SorFields Fields { get { return new SorFields(CSorTable_Fields(ref Impl_)); } }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorTable_Fields")]
    private static extern TImpl CSorTable_Fields(ref TImpl impl);

    /// <summary>
    /// 取得表格的資料筆數.
    /// </summary>
    public uint RecordsCount { get { return CSorTable_RecordsCount(ref Impl_); } }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorTable_RecordsCount")]
    private static extern uint CSorTable_RecordsCount(ref TImpl impl);

    /// <summary>
    /// 取得表格的某資料的某欄位(使用index)內容, 傳回 null 表示無該筆資料或欄位.
    /// </summary>
    public string RecordIndexField(uint recordIndex, uint fieldIndex) { return CSorTable_RecordIndexField(ref Impl_, recordIndex, fieldIndex); }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorTable_RecordIndexField_B")]
    [return: MarshalAs(UnmanagedType.AnsiBStr)]
    private static extern string CSorTable_RecordIndexField(ref TImpl impl, uint recordIndex, uint fieldIndex);

    /// <summary>
    /// 取得表格的某資料的某欄位(使用SorField)內容, 傳回 null 表示無該筆資料或欄位.
    /// </summary>
    public string RecordField(uint recordIndex, SorField field) { return (field == null ? null : CSorTable_RecordField(ref Impl_, recordIndex, ref field.Impl_)); }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorTable_RecordField_B")]
    [return: MarshalAs(UnmanagedType.AnsiBStr)]
    private static extern string CSorTable_RecordField(ref TImpl impl, uint recordIndex, ref TImpl fieldImpl);

    /// <summary>
    /// 取得此表格所屬的市場別屬性, 可能有多個市場別, 請使用 bit 判斷.
    /// </summary>
    public SorMktFlags MktFlag { get { return CSorTable_MktFlag(ref Impl_); } }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorTable_MktFlag")]
    private static extern SorMktFlags CSorTable_MktFlag(ref TImpl impl);
}
