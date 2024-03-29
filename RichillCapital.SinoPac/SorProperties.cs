using System.Runtime.InteropServices;

namespace RichillCapital.SinoPac.Sor;

public sealed partial class SorProperties
{
    private TImpl Impl_;

    internal SorProperties(TImpl impl) => Impl_ = impl;

    public string GetValue(string name) => GetValue(ref Impl_, name);

    public string Name => GetName(ref Impl_);

    public string DisplayText => GetDisplayText(ref Impl_);

    public string Description => GetDescription(ref Impl_);

    /// 取得屬性集合的顯示字串, 不含名稱屬性, 使用 0x01 分隔.
    public override string ToString() => ToString(ref Impl_);
}

public sealed partial class SorProperties
{
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorProperties_Get_B")]
    [return: MarshalAs(UnmanagedType.AnsiBStr)]
    private static extern String GetValue(ref TImpl impl, string name);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorProperties_Name_B")]
    [return: MarshalAs(UnmanagedType.AnsiBStr)]
    private static extern String GetName(ref TImpl impl);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorProperties_DisplayText_B")]
    [return: MarshalAs(UnmanagedType.AnsiBStr)]
    private static extern String GetDisplayText(ref TImpl impl);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorProperties_Description_B")]
    [return: MarshalAs(UnmanagedType.AnsiBStr)]
    private static extern String GetDescription(ref TImpl impl);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorProperties_ToString_B")]
    [return: MarshalAs(UnmanagedType.AnsiBStr)]
    private static extern String ToString(ref TImpl impl);
}