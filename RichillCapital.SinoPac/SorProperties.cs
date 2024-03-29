using System.Runtime.InteropServices;

namespace RichillCapital.SinoPac.Sor;

public class SorProperties
{
    TImpl Impl_;
    internal SorProperties(TImpl impl)
    {
        Impl_ = impl;
    }

    /// <summary>
    /// 取得指定的屬性
    /// </summary>
    /// <param name="propName">屬性名稱</param>
    public string Get(string propName) { return CSorProperties_Get(ref Impl_, propName); }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorProperties_Get_B")]
    [return: MarshalAs(UnmanagedType.AnsiBStr)]
    private static extern String CSorProperties_Get(ref TImpl impl, string name);

    /// <summary>
    /// 取得名稱屬性.
    /// </summary>
    public string Name { get { return CSorProperties_Name(ref Impl_); } }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorProperties_Name_B")]
    [return: MarshalAs(UnmanagedType.AnsiBStr)]
    private static extern String CSorProperties_Name(ref TImpl impl);

    /// <summary>
    /// 取得顯示字串
    /// </summary>
    public string DisplayText { get { return CSorProperties_DisplayText(ref Impl_); } }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorProperties_DisplayText_B")]
    [return: MarshalAs(UnmanagedType.AnsiBStr)]
    private static extern String CSorProperties_DisplayText(ref TImpl impl);

    /// <summary>
    /// 取得描述字串
    /// </summary>
    public string Description { get { return CSorProperties_Description(ref Impl_); } }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorProperties_Description_B")]
    [return: MarshalAs(UnmanagedType.AnsiBStr)]
    private static extern String CSorProperties_Description(ref TImpl impl);

    /// <summary>
    /// 取得屬性集合的顯示字串, 不含名稱屬性, 使用 0x01 分隔.
    /// </summary>
    public override string ToString() { return CSorProperties_ToString(ref Impl_); }
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorProperties_ToString_B")]
    [return: MarshalAs(UnmanagedType.AnsiBStr)]
    private static extern String CSorProperties_ToString(ref TImpl impl);
}
