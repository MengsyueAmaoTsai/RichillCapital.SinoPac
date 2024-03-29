using System.Runtime.InteropServices;

namespace RichillCapital.SinoPac.Sor;

public sealed partial class SorProperties
{
    TImpl Impl_;

    internal SorProperties(TImpl impl)
    {
        Impl_ = impl;
    }

    /// 取得指定的屬性
    public string Get(string propertyName) { return CSorProperties_Get(ref Impl_, propertyName); }

    public string Name => CSorProperties_Name(ref Impl_);

    /// 取得顯示字串
    public string DisplayText => CSorProperties_DisplayText(ref Impl_);

    /// 取得描述字串
    public string Description => CSorProperties_Description(ref Impl_);

    /// 取得屬性集合的顯示字串, 不含名稱屬性, 使用 0x01 分隔.
    public override string ToString() => CSorProperties_ToString(ref Impl_);
}

public sealed partial class SorProperties
{
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorProperties_Get_B")]
    [return: MarshalAs(UnmanagedType.AnsiBStr)]
    private static extern String CSorProperties_Get(ref TImpl impl, string name);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorProperties_Name_B")]
    [return: MarshalAs(UnmanagedType.AnsiBStr)]
    private static extern String CSorProperties_Name(ref TImpl impl);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorProperties_DisplayText_B")]
    [return: MarshalAs(UnmanagedType.AnsiBStr)]
    private static extern String CSorProperties_DisplayText(ref TImpl impl);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorProperties_Description_B")]
    [return: MarshalAs(UnmanagedType.AnsiBStr)]
    private static extern String CSorProperties_Description(ref TImpl impl);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorProperties_ToString_B")]
    [return: MarshalAs(UnmanagedType.AnsiBStr)]
    private static extern String CSorProperties_ToString(ref TImpl impl);
}