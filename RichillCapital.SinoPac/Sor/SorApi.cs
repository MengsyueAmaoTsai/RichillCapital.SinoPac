internal static class SorApi
{
    internal const string Version = "1.0.0.0";
    internal const string DefaultHost = "210.59.255.56:6003";
    internal const string SystemId = "SINOPAC";
    
    internal static class Dll
    {
        private const string Directory = "./Libs/SorApi";

        internal const string SorClient = $"{Directory}/SorApi.dll";
        internal const string Certificate = $"{Directory}/SinoPacSorApiCA.dll";
        internal const string Kernel32 = "kernel32.dll";
    }
}