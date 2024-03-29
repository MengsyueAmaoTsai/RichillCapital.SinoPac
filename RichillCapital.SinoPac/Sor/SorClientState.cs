namespace RichillCapital.SinoPac.Sor;

public enum SorClientState
{
  /// 切斷連線: 呼叫 Disconnect() 斷線.
  SorClientState_Disconnected = -1,

  /// 連線失敗: 網路層無法建立與SORS的連線.
  SorClientState_LinkFail = -2,

  /// 連線後斷線: 網路層斷線.
  SorClientState_LinkBroken = -3,

  /// 網路層可連線,但對方不是SORS服務.
  SorClientState_UnknownServer = -4,
  
  /// 登入失敗.
  SorClientState_SignonError = -5,
  
  /// 連線失敗: 主機拒絕連線訊息.
  SorClientState_ConnectError = -6,
  
  /// 連線中斷: 送出的 Heartbeat 主機沒有回覆.
  SorClientState_HeartbeatTimeout = -7,
  
  /// 建構後, 尚未進行連線.
  SorClientState_Idle = 0,
  
  /// 網路層連線中.
  SorClientState_Linking = 1,
  
  /// 已與SORS建立已連線: 已初步溝通完畢:已取得SORS服務端名稱.
  SorClientState_Connected = 2,
  
  /// 已登入券商系統, 可以進行下單或其他操作.
  SorClientState_ApReady = 3,
}