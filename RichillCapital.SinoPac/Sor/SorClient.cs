using System.Runtime.InteropServices;

namespace RichillCapital.SinoPac.Sor;

public sealed partial class SorClient
{
    public void Connect()
    {
    }

    public void Disconnect()
    {
    }

    public IEnumerable<object> GetAccounts()
    {
        return [];
    }

    public void QueryPositions()
    {
    }

    public void QueryBalanceInfo()
    {
    }

    public void PlaceOrder()
    {
    }

    public void ModifyOrderPrice()
    {
    }

    public void ModifyOrderQuantity()
    {
    }

    private void ValidateConnectionState()
    {
    }
}

public sealed partial class SorClient
{
    [DllImport("SorApi.dll", EntryPoint = "CSorClient_State")]
    private static extern SorClientState CSorClient_State(ref TImpl cli);

}

struct TImpl
{
  IntPtr Impl_;

  public readonly bool IsInvalid => Impl_ == IntPtr.Zero;
}
