using System.Runtime.InteropServices;

namespace RichillCapital.SinoPac.Sor;

public sealed partial class SorClient : IDisposable
{
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorClient_Create_OnMessageLoop")]
    private static extern TImpl CreateOnMessageLoop(
        ref SorClientDelegates evHandler,
        IntPtr userData);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorClient_Create")]
    private static extern TImpl Create(
        ref SorClientDelegates evHandler,
        IntPtr userData);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorClient_Connect")]
    private static extern void Connect(
        ref TImpl cli,
        string host,
        string clientName,
        string version,
        string systemId,
        string userId,
        string password);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorClient_Disconnect")]
    private static extern void Disconnect(ref TImpl cli);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorClient_Delete")]
    private static extern void Delete(ref TImpl cli);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorClient_State")]
    private static extern SorClientState GetClientState(ref TImpl cli);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorClient_IsSessionConnected")]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool IsSessionConnected(ref TImpl cli);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorClient_SgnResult")]
    private static extern TImpl GetSignInResult(ref TImpl cli);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorClient_SendSorRequest")]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool SendRequest(
        ref TImpl cli,
        uint messageCode,
        string request,
        uint requestLength);
}