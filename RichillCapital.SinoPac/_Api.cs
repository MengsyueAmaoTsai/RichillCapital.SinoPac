using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace SorApi;

using TUserData = IntPtr;
using TMsgCode = UInt32;
using TPkPtr = IntPtr;
using TPkSz = UInt32;
using TIndex = UInt32;

struct TImpl
{
  IntPtr Impl_;

  public bool IsInvalid { get { return Impl_ == IntPtr.Zero; } }
}

delegate void OnSorUnknownMsgCodeCallbackDelegate(ref TImpl sender, TUserData userdata, TMsgCode msgCode, TPkPtr pkptr, TPkSz pksz);
delegate void OnSorConnectCallbackDelegate(ref TImpl sender, TUserData userdata, string errmsg);
delegate void OnSorApReadyCallbackDelegate(ref TImpl sender, TUserData userdata);
delegate void OnSorTaskResultCallbackDelegate(ref TImpl sender, TUserData userdata, ref TImpl taskResult);
delegate void OnSorChgPassResultCallbackDelegate(ref TImpl sender, TUserData userdata, string user, string result);
delegate void OnSorRequestAckCallbackDelegate(ref TImpl sender, TUserData userdata, TMsgCode msgCode, string result);
delegate void OnSorReportCallbackDelegate(ref TImpl sender, TUserData userdata, string result);
delegate void OnSorClientDeleteCallbackDelegate(ref TImpl sender, TUserData userdata);

struct CSorClientCallbacks
{
  public OnSorUnknownMsgCodeCallbackDelegate OnSorUnknownMsgCodeCallback;
  public OnSorConnectCallbackDelegate OnSorConnectCallback;
  public OnSorApReadyCallbackDelegate OnSorApReadyCallback;
  public OnSorTaskResultCallbackDelegate OnSorTaskResultCallback;
  public OnSorChgPassResultCallbackDelegate OnSorChgPassResultCallback;
  public OnSorRequestAckCallbackDelegate OnSorRequestAckCallback;
  public OnSorReportCallbackDelegate OnSorReportCallback;
  public OnSorClientDeleteCallbackDelegate OnSorClientDeleteCallback;
}

/// 當收到[不明訊息]時的通知.
public delegate void OnSorUnknownMsgCodeEvent(SorClient sender, TMsgCode msgCode, TPkPtr pkptr, TPkSz pksz);
/// SORS連線訊息通知, if(errmsg.empty()) 表示成功, 此時可呼叫 sender.ServerName() 取得主機名稱.
public delegate void OnSorConnectEvent(SorClient sender, string errmsg);
/// SORS已備妥,可以下單或執行特定作業.
public delegate void OnSorApReadyEvent(SorClient sender);
/// 一般作業結果通知.
public delegate void OnSorTaskResultEvent(SorClient sender, SorTaskResult taskResult);
/// 改密碼結果, if(result.empty()) 改密碼成功! else result=失敗訊息.
public delegate void OnSorChgPassResultEvent(SorClient sender, string user, string result);
/// 下單回覆.
public delegate void OnSorRequestAckEvent(SorClient sender, TMsgCode msgCode, string result);
/// 委託回補, 委託主動回報, 成交回報.
public delegate void OnSorReportEvent(SorClient sender, string result);
/// 當 sender 要被殺死前的通知.
public delegate void OnSorClientDeleteEvent(SorClient sender);

/// <summary>
/// SorClient現在狀態
/// </summary>
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

/// <summary>
/// 與 SORS 溝通用的連線物件
/// </summary>
public class SorClient : IDisposable
{
  internal TImpl Impl_;
  CSorClientCallbacks Callbacks_ = new CSorClientCallbacks();

  #region SorClient Callbacks 轉 C# event
  void OnSorUnknownMsgCodeCallback(ref TImpl sender, TUserData userdata, TMsgCode msgCode, TPkPtr pkptr, TPkSz pksz)
  {
     if (OnSorUnknownMsgCodeEvent != null)
        OnSorUnknownMsgCodeEvent(this, msgCode, pkptr, pksz);
  }
  void OnSorConnectCallback(ref TImpl sender, TUserData userdata, string errmsg)
  {
     if (OnSorConnectEvent != null)
        OnSorConnectEvent(this, errmsg);
  }
  void OnSorApReadyCallback(ref TImpl sender, TUserData userdata)
  {
     if (OnSorApReadyEvent != null)
        OnSorApReadyEvent(this);
  }
  void OnSorTaskResultCallback(ref TImpl sender, TUserData userdata, ref TImpl taskResult)
  {
     if (OnSorTaskResultEvent != null)
        OnSorTaskResultEvent(this, new SorTaskResult(taskResult));
  }
  void OnSorChgPassResultCallback(ref TImpl sender, TUserData userdata, string user, string result)
  {
     if (OnSorChgPassResultEvent != null)
        OnSorChgPassResultEvent(this, user, result);
  }
  void OnSorRequestAckCallback(ref TImpl sender, TUserData userdata, TMsgCode msgCode, string result)
  {
     if (OnSorRequestAckEvent != null)
        OnSorRequestAckEvent(this, msgCode, result);
  }
  void OnSorReportCallback(ref TImpl sender, TUserData userdata, string result)
  {
     if (OnSorReportEvent != null)
        OnSorReportEvent(this, result);
  }
  void OnSorClientDeleteCallback(ref TImpl sender, TUserData userdata)
  {
     if (OnSorClientDeleteEvent != null)
        OnSorClientDeleteEvent(this);
  }
  #endregion

  #region 建構 & 解構
  /// 使用 MessageLoop 事件通知, 建構 CSorClient, evHandler 會被複製一份在 CSorClient 裡面.
  [DllImport("SorApi.dll", EntryPoint = "CSorClient_Create_OnMessageLoop")]
  private static extern TImpl CSorClient_Create_OnMessageLoop(ref CSorClientCallbacks evHandler, TUserData userdata);
  /// 使用 Thread 事件通知, 建構 CSorClient, evHandler 會被複製一份在 CSorClient 裡面.
  [DllImport("SorApi.dll", EntryPoint = "CSorClient_Create")]
  private static extern TImpl CSorClient_Create(ref CSorClientCallbacks evHandler, TUserData userdata);
  /// <summary>
  /// 建構.
  /// </summary>
  /// <param name="isEventOnMessageLoop">
  ///   true=在MessageLoop觸發事件, false=事件觸發可能在任一Thread,
  ///   只有 OnSorClientDeleteEvent事件 不受這個限制, 此事件一律都在呼叫 Dispose()的那個Thread, 且在返回前觸發.
  ///  </param>
  public SorClient(bool isEventOnMessageLoop)
  {
     Callbacks_.OnSorUnknownMsgCodeCallback = OnSorUnknownMsgCodeCallback;
     Callbacks_.OnSorConnectCallback = OnSorConnectCallback;
     Callbacks_.OnSorApReadyCallback = OnSorApReadyCallback;
     Callbacks_.OnSorTaskResultCallback = OnSorTaskResultCallback;
     Callbacks_.OnSorChgPassResultCallback = OnSorChgPassResultCallback;
     Callbacks_.OnSorRequestAckCallback = OnSorRequestAckCallback;
     Callbacks_.OnSorReportCallback = OnSorReportCallback;
     Callbacks_.OnSorClientDeleteCallback = null;// OnSorClientDeleteCallback;
     Impl_ = isEventOnMessageLoop ? CSorClient_Create_OnMessageLoop(ref Callbacks_, IntPtr.Zero) : CSorClient_Create(ref Callbacks_, IntPtr.Zero);
  }

  /// 解構 CSorClient.
  [DllImport("SorApi.dll", EntryPoint = "CSorClient_Delete")]
  private static extern void CSorClient_Delete(ref TImpl cli);
  /// <summary>
  ///  解構.
  /// </summary>
  public void Dispose()
  {
     CSorClient_Delete(ref Impl_);
  }
  #endregion

  #region Events
  /// 當收到[不明訊息]時的通知.
  public OnSorUnknownMsgCodeEvent OnSorUnknownMsgCodeEvent;
  /// SORS連線訊息通知, if(errmsg.empty()) 表示成功, 此時可呼叫 sender.ServerName() 取得主機名稱.
  public OnSorConnectEvent OnSorConnectEvent;
  /// SORS已備妥,可以下單或執行特定作業.
  public OnSorApReadyEvent OnSorApReadyEvent;
  /// 一般作業結果通知.
  public OnSorTaskResultEvent OnSorTaskResultEvent;
  /// 改密碼結果, if(result.empty()) 改密碼成功! else result=失敗訊息.
  public OnSorChgPassResultEvent OnSorChgPassResultEvent;
  /// 下單回覆.
  public OnSorRequestAckEvent OnSorRequestAckEvent;
  /// 委託回補, 委託主動回報, 成交回報.
  public OnSorReportEvent OnSorReportEvent;
  /// 當 sender 要被殺死前的通知.
  public OnSorClientDeleteEvent OnSorClientDeleteEvent;
  #endregion

  #region 連線登入 & 切斷連線
  /// <summary>
  /// 建立與SORS的連線並登入.
  /// </summary>
  /// <param name="connParam">連線參數, 格式: "host:port", host = SORS 主機 ip 或 domain name</param>
  /// <param name="sysid">登入的系統名稱,由券商指定</param>
  /// <param name="user">使用者ID</param>
  /// <param name="pass">使用者密碼</param>
  public void Connect(string connParam, string sysid, string user, string pass)
  {
     CSorClient_Connect(ref Impl_, connParam, "SorApiCS", "1.0.0.0", sysid, user, pass);
  }
  [DllImport("SorApi.dll", EntryPoint = "CSorClient_Connect")]
  private static extern void CSorClient_Connect(ref TImpl cli, string connParam, string cliApName, string cliApVer, string sysid, string user, string pass);

  /// <summary>
  /// 切斷連線.
  /// </summary>
  public void Disconnect()
  {
     CSorClient_Disconnect(ref Impl_);
  }
  [DllImport("SorApi.dll", EntryPoint = "CSorClient_Disconnect")]
  private static extern void CSorClient_Disconnect(ref TImpl cli);
  #endregion

  #region Get State & Result
  /// <summary>
  /// 取得現在的狀態.
  /// </summary>
  public SorClientState State { get { return CSorClient_State(ref Impl_); } }
  [DllImport("SorApi.dll", EntryPoint = "CSorClient_State")]
  private static extern SorClientState CSorClient_State(ref TImpl cli);

  /// <summary>
  /// 是否已與SORS建立連線, 取得SORS服務端名稱 (包含已登入 or 登入失敗).
  /// </summary>
  public bool IsSessionConnected { get { return CSorClient_IsSessionConnected(ref Impl_); } }
  [DllImport("SorApi.dll", EntryPoint = "CSorClient_IsSessionConnected")]
  [return: MarshalAs(UnmanagedType.I1)]
  private static extern bool CSorClient_IsSessionConnected(ref TImpl cli);

  /// <summary>
  /// 取得登入結果.
  /// </summary>
  public SorTaskResult SgnResult { get { return new SorTaskResult(CSorClient_SgnResult(ref Impl_)); } }
  [DllImport("SorApi.dll", EntryPoint = "CSorClient_SgnResult")]
  private static extern TImpl CSorClient_SgnResult(ref TImpl cli);
  #endregion

  #region Send Request
  /// <summary>
  /// 送出SORS要求訊息.
  /// reqCtx裡面不可有中文,因為在取 reqCtx.Length 時, .NET裡面一個中文字長度=1, 但ASCII(big5)中文字長度=2
  /// </summary>
  /// <param name="msgCode">0x81=下單要求, 0x83=回補要求, 0x84=無流量管制時的無ACK下單要求</param>
  /// <param name="reqCtx">要送出的下單要求內容,前5碼必須保留給header</param>
  /// <returns>true成功送出,false=無法送出(例如:0x80查詢超過流量上限)</returns>
  public bool SendSorRequest(TMsgCode msgCode, string reqCtx)
  {
     return CSorClient_SendSorRequest(ref Impl_, msgCode, reqCtx, (TPkSz)reqCtx.Length);
  }
  [DllImport("SorApi.dll", EntryPoint = "CSorClient_SendSorRequest")]
  [return: MarshalAs(UnmanagedType.I1)]
  private static extern bool CSorClient_SendSorRequest(ref TImpl cli, TMsgCode msgCode, string reqCtx, TPkSz reqLen);

  /// <summary>
  /// <summary>
  /// 改密碼, 必須先建立連線才能改密碼: State >= ConnectState.Connected || SignonError.
  /// </summary>
  /// <returns>若 State >= ConnectState.Connected 則傳回 true, 否則傳回 false 無法進行改密碼操作</returns>
  public bool ChgPass(string user, string oldpass, string newpass)
  {
     return CSorClient_ChgPass(ref Impl_, user, oldpass, newpass);
  }
  [DllImport("SorApi.dll", EntryPoint = "CSorClient_ChgPass")]
  [return: MarshalAs(UnmanagedType.I1)]
  private static extern bool CSorClient_ChgPass(ref TImpl cli, string user, string oldpass, string newpass);
  #endregion
}

/// <summary>
/// SORS 作業結果分析.
/// </summary>
public class SorTaskResult
{
  TImpl Impl_;
  internal SorTaskResult(TImpl impl)
  {
     Impl_ = impl;
  }

  /// <summary>
  /// 取得此Task的識別碼.
  /// </summary>
  public string WorkID { get { return CSorTaskResult_WorkID(ref Impl_); } }
  [DllImport("SorApi.dll", EntryPoint = "CSorTaskResult_WorkID_B")]
  [return: MarshalAs(UnmanagedType.AnsiBStr)]
  private static extern string CSorTaskResult_WorkID(ref TImpl impl);

  /// <summary>
  /// 取得原始結果字串.
  /// </summary>
  public string OrigResult { get { return CSorTaskResult_OrigResult(ref Impl_); } }
  [DllImport("SorApi.dll", EntryPoint = "CSorTaskResult_OrigResult_B")]
  [return: MarshalAs(UnmanagedType.AnsiBStr)]
  private static extern string CSorTaskResult_OrigResult(ref TImpl impl);

  /// <summary>
  /// 使用 tableName 取得結果資料表.
  /// </summary>
  public SorTable NameTable(string tableName) { return new SorTable(CSorTaskResult_NameTable(ref Impl_, tableName)); }
  [DllImport("SorApi.dll", EntryPoint = "CSorTaskResult_NameTable")]
  private static extern TImpl CSorTaskResult_NameTable(ref TImpl impl, string tableName);

  /// <summary>
  /// 使用 index 取得結果資料表.
  /// </summary>
  public SorTable IndexTable(TIndex index) { return new SorTable(CSorTaskResult_IndexTable(ref Impl_, index)); }
  [DllImport("SorApi.dll", EntryPoint = "CSorTaskResult_IndexTable")]
  private static extern TImpl CSorTaskResult_IndexTable(ref TImpl impl, TIndex index);

  /// <summary>
  /// 取得資料表數量.
  /// </summary>
  public TIndex TablesCount { get { return CSorTaskResult_TableCount(ref Impl_); } }
  [DllImport("SorApi.dll", EntryPoint = "CSorTaskResult_TableCount")]
  private static extern TIndex CSorTaskResult_TableCount(ref TImpl impl);
}

/// <summary>
/// 可交易市場別.
/// </summary>
public enum SorMktFlags
{
  /// 無可交易市場.
  None = 0,
  /// 台灣證券.
  TwStk = 1,
  /// 台灣期權.
  TwFuo = 2,
  /// 國外證券.
  FrStk = 4,
  /// 國外期權.
  FrFuo = 8,
  /// 台灣期權報價.
  TwfQuot = 0x10,
  /// 大陸期權.
  CnFuo = 0x20,
}

/// <summary>
/// SORS資料表型別.
/// </summary>
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
  [DllImport("SorApi.dll", EntryPoint = "CSorTable_Properties")]
  private static extern TImpl CSorTable_Properties(ref TImpl impl);

  /// <summary>
  /// 取得表格的欄位列表.
  /// </summary>
  public SorFields Fields { get { return new SorFields(CSorTable_Fields(ref Impl_)); } }
  [DllImport("SorApi.dll", EntryPoint = "CSorTable_Fields")]
  private static extern TImpl CSorTable_Fields(ref TImpl impl);

  /// <summary>
  /// 取得表格的資料筆數.
  /// </summary>
  public TIndex RecordsCount { get { return CSorTable_RecordsCount(ref Impl_); } }
  [DllImport("SorApi.dll", EntryPoint = "CSorTable_RecordsCount")]
  private static extern TIndex CSorTable_RecordsCount(ref TImpl impl);

  /// <summary>
  /// 取得表格的某資料的某欄位(使用index)內容, 傳回 null 表示無該筆資料或欄位.
  /// </summary>
  public string RecordIndexField(TIndex recordIndex, TIndex fieldIndex) { return CSorTable_RecordIndexField(ref Impl_, recordIndex, fieldIndex); }
  [DllImport("SorApi.dll", EntryPoint = "CSorTable_RecordIndexField_B")]
  [return: MarshalAs(UnmanagedType.AnsiBStr)]
  private static extern string CSorTable_RecordIndexField(ref TImpl impl, TIndex recordIndex, TIndex fieldIndex);

  /// <summary>
  /// 取得表格的某資料的某欄位(使用SorField)內容, 傳回 null 表示無該筆資料或欄位.
  /// </summary>
  public string RecordField(TIndex recordIndex, SorField field) { return (field == null ? null : CSorTable_RecordField(ref Impl_, recordIndex, ref field.Impl_)); }
  [DllImport("SorApi.dll", EntryPoint = "CSorTable_RecordField_B")]
  [return: MarshalAs(UnmanagedType.AnsiBStr)]
  private static extern string CSorTable_RecordField(ref TImpl impl, TIndex recordIndex, ref TImpl fieldImpl);

  /// <summary>
  /// 取得此表格所屬的市場別屬性, 可能有多個市場別, 請使用 bit 判斷.
  /// </summary>
  public SorMktFlags MktFlag { get { return CSorTable_MktFlag(ref Impl_); } }
  [DllImport("SorApi.dll", EntryPoint = "CSorTable_MktFlag")]
  private static extern SorMktFlags CSorTable_MktFlag(ref TImpl impl);
}

/// <summary>
/// SORS回覆的 [資料表]、[欄位] 都會包含屬性內容.
/// </summary>
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
  [DllImport("SorApi.dll", EntryPoint = "CSorProperties_Get_B")]
  [return: MarshalAs(UnmanagedType.AnsiBStr)]
  private static extern String CSorProperties_Get(ref TImpl impl, string name);

  /// <summary>
  /// 取得名稱屬性.
  /// </summary>
  public string Name { get { return CSorProperties_Name(ref Impl_); } }
  [DllImport("SorApi.dll", EntryPoint = "CSorProperties_Name_B")]
  [return: MarshalAs(UnmanagedType.AnsiBStr)]
  private static extern String CSorProperties_Name(ref TImpl impl);

  /// <summary>
  /// 取得顯示字串
  /// </summary>
  public string DisplayText { get { return CSorProperties_DisplayText(ref Impl_); } }
  [DllImport("SorApi.dll", EntryPoint = "CSorProperties_DisplayText_B")]
  [return: MarshalAs(UnmanagedType.AnsiBStr)]
  private static extern String CSorProperties_DisplayText(ref TImpl impl);

  /// <summary>
  /// 取得描述字串
  /// </summary>
  public string Description { get { return CSorProperties_Description(ref Impl_); } }
  [DllImport("SorApi.dll", EntryPoint = "CSorProperties_Description_B")]
  [return: MarshalAs(UnmanagedType.AnsiBStr)]
  private static extern String CSorProperties_Description(ref TImpl impl);

  /// <summary>
  /// 取得屬性集合的顯示字串, 不含名稱屬性, 使用 0x01 分隔.
  /// </summary>
  public override string ToString() { return CSorProperties_ToString(ref Impl_); }
  [DllImport("SorApi.dll", EntryPoint = "CSorProperties_ToString_B")]
  [return: MarshalAs(UnmanagedType.AnsiBStr)]
  private static extern String CSorProperties_ToString(ref TImpl impl);
}

/// <summary>
/// SORS [資料表] 的 [欄位列表].
/// </summary>
public class SorFields
{
  TImpl Impl_;
  internal SorFields(TImpl impl)
  {
     Impl_ = impl;
  }

  /// <summary>
  /// 用欄位名稱取得欄位,若不存在則傳回null.
  /// </summary>
  public SorField NameField(string fieldName) { return SorField.MakeSorField(CSorFields_NameField(ref Impl_, fieldName)); }
  [DllImport("SorApi.dll", EntryPoint = "CSorFields_NameField")]
  private static extern TImpl CSorFields_NameField(ref TImpl impl, string fieldName);

  /// <summary>
  /// 用欄位名稱取得欄位索引,若不存在則傳回 SorField.InvalidIndex
  /// </summary>
  public TIndex NameFieldIndex(string fieldName)
  {
     TImpl impl = CSorFields_NameField(ref Impl_, fieldName);
     return (impl.IsInvalid ? SorField.InvalidIndex : SorField.CSorField_Index(ref impl));
  }

  /// <summary>
  /// 用索引取得欄位,若不存在則傳回null.
  /// </summary>
  public SorField IndexField(TIndex fieldIndex) { return SorField.MakeSorField(CSorFields_IndexField(ref Impl_, fieldIndex)); }
  [DllImport("SorApi.dll", EntryPoint = "CSorFields_IndexField")]
  private static extern TImpl CSorFields_IndexField(ref TImpl impl, TIndex fieldIndex);

  /// <summary>
  /// 取得欄位數量.
  /// </summary>
  public TIndex Count { get { return CSorFields_Count(ref Impl_); } }
  [DllImport("SorApi.dll", EntryPoint = "CSorFields_Count")]
  private static extern TIndex CSorFields_Count(ref TImpl impl);
}

/// <summary>
/// SORS資料表裡面的 [一個欄位] 型別.
/// </summary>
public class SorField
{
  internal TImpl Impl_;
  internal SorField(TImpl impl)
  {
     Impl_ = impl;
  }
  internal static SorField MakeSorField(TImpl impl)
  {
     return impl.IsInvalid ? null : new SorField(impl);
  }

  /// <summary>
  /// 取得欄位屬性列表
  /// </summary>
  public SorProperties Properties { get { return new SorProperties(CSorField_Properties(ref Impl_)); } }
  [DllImport("SorApi.dll", EntryPoint = "CSorField_Properties")]
  private static extern TImpl CSorField_Properties(ref TImpl impl);

  /// <summary>
  /// 取得欄位的索引, InvalidIndex 表示欄位有誤.
  /// </summary>
  public TIndex Index { get { return CSorField_Index(ref Impl_); } }
  [DllImport("SorApi.dll", EntryPoint = "CSorField_Index")]
  internal static extern TIndex CSorField_Index(ref TImpl impl);

  /// <summary>
  /// 無效的欄位索引.
  /// </summary>
  public const TIndex InvalidIndex = 0xffffffff;
}

/// <summary>
/// 傳送下單要求前先檢查流量管制, 被管制的要求, 就放在 Queue 裡面, 等解除管制時自動送出.
/// </summary>
class SorFlowCtrlSender : IDisposable
{
  TImpl Impl_;
  /// <summary>
  /// 建構 SorFlowCtrlSender, owner死掉前必須先呼叫 this.Dispose().
  /// </summary>
  /// <param name="owner"></param>
  public SorFlowCtrlSender(SorClient owner)
  {
     Impl_ = CSorFlowCtrlSender_Create(ref owner.Impl_);
  }
  [DllImport("SorApi.dll", EntryPoint = "CSorFlowCtrlSender_Create")]
  private static extern TImpl CSorFlowCtrlSender_Create(ref TImpl gcliImpl);

  /// <summary>
  /// 刪除 SorFlowCtrlSender 及相關資源.
  /// </summary>
  public void Dispose()
  {
     CSorFlowCtrlSender_Delete(ref Impl_);
  }
  [DllImport("SorApi.dll", EntryPoint = "CSorFlowCtrlSender_Delete")]
  private static extern void CSorFlowCtrlSender_Delete(ref TImpl impl);

  /// <summary>
  /// 設定相關流量管制參數.
  /// </summary>
  public void SetFlowCtrl(UInt32 rate, UInt32 rateMS) { CSorFlowCtrlSender_SetFlowCtrl(ref Impl_, rate, rateMS); }
  [DllImport("SorApi.dll", EntryPoint = "CSorFlowCtrlSender_SetFlowCtrl")]
  private static extern void CSorFlowCtrlSender_SetFlowCtrl(ref TImpl impl, UInt32 rate, UInt32 rateMS);

  /// <summary>
  /// 取得現在排隊的數量.
  /// </summary>
  public UInt32 PendingCount { get { return CSorFlowCtrlSender_PendingCount(ref Impl_); } }
  [DllImport("SorApi.dll", EntryPoint = "CSorFlowCtrlSender_PendingCount")]
  private static extern UInt32 CSorFlowCtrlSender_PendingCount(ref TImpl impl);

  /// <summary>
  /// Ack Parser, 自動處理 SetAckTime() 相關事項
  /// </summary>
  public void AckParser(TMsgCode msgCode, string acks) { CSorFlowCtrlSender_AckParser(ref Impl_, msgCode, acks); }
  [DllImport("SorApi.dll", EntryPoint = "CSorFlowCtrlSender_AckParser")]
  private static extern void CSorFlowCtrlSender_AckParser(ref TImpl impl, TMsgCode msgCode, string acks);

  /// <summary>
  /// 送出一筆下單要求, 若無法立即送出, 則會放到Queue之中自動傳送.
  /// reqmsg 需保留 5 bytes header.
  /// 傳回 true=已立即送出, false=已放到Queue之中等候送出.
  /// </summary>
  public bool SendSorRequest(string reqmsg) { return CSorFlowCtrlSender_SendSorRequest(ref Impl_, reqmsg, (TPkSz)reqmsg.Length); }
  [DllImport("SorApi.dll", EntryPoint = "CSorFlowCtrlSender_SendSorRequest")]
  [return: MarshalAs(UnmanagedType.I1)]
  private static extern bool CSorFlowCtrlSender_SendSorRequest(ref TImpl impl, string reqmsg, TPkSz reqmsgLen);

  /// <summary>
  /// 送出一批下單要求, 傳回[立即送出]的筆數, 其餘要求會放到Queue之中自動傳送.
  /// reqmsg 需保留 5 bytes header, 每筆下單要求之間用 '\n' 分隔, 最後一筆不用加 '\n'
  /// </summary>
  public UInt32 SendSorRequests(string reqmsg) { return CSorFlowCtrlSender_SendSorRequests(ref Impl_, reqmsg, (TPkSz)reqmsg.Length); }
  [DllImport("SorApi.dll", EntryPoint = "CSorFlowCtrlSender_SendSorRequests")]
  private static extern UInt32 CSorFlowCtrlSender_SendSorRequests(ref TImpl impl, string reqmsg, TPkSz reqmsgLen);
}
