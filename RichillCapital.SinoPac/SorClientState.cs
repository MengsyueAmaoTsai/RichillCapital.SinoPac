namespace RichillCapital.SinoPac.Sor;

public enum SorClientState
{
    /// <summary>
    /// 切斷連線: 呼叫 Disconnect() 斷線.
    /// </summary>
    SorClientState_Disconnected = -1,
    /// <summary>
    /// 連線失敗: 網路層無法建立與SORS的連線.
    /// </summary>
    SorClientState_LinkFail = -2,
    /// <summary>
    /// 連線後斷線: 網路層斷線.
    /// </summary>
    SorClientState_LinkBroken = -3,
    /// <summary>
    /// 網路層可連線,但對方不是SORS服務.
    /// </summary>
    SorClientState_UnknownServer = -4,
    /// <summary>
    /// 登入失敗.
    /// </summary>
    SorClientState_SignonError = -5,
    /// <summary>
    /// 連線失敗: 主機拒絕連線訊息.
    /// </summary>
    SorClientState_ConnectError = -6,
    /// <summary>
    /// 連線中斷: 送出的 Heartbeat 主機沒有回覆.
    /// </summary>
    SorClientState_HeartbeatTimeout = -7,
    /// <summary>
    /// 建構後, 尚未進行連線.
    /// </summary>
    SorClientState_Idle = 0,
    /// <summary>
    /// 網路層連線中.
    /// </summary>
    SorClientState_Linking = 1,
    /// <summary>
    /// 已與SORS建立已連線: 已初步溝通完畢:已取得SORS服務端名稱.
    /// </summary>
    SorClientState_Connected = 2,
    /// <summary>
    /// 已登入券商系統, 可以進行下單或其他操作.
    /// </summary>
    SorClientState_ApReady = 3,
}
