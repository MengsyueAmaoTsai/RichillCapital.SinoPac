using System.Runtime.InteropServices;

namespace SorApi;

using TIndex = UInt32;

/// <summary>
/// 簽章輔助功能.
/// </summary>
public class DigSgnHandler : IDisposable
{
  [DllImport("kernel32.dll")]
  static extern IntPtr LoadLibrary(string dllToLoad);
  [DllImport("kernel32.dll")]
  static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);
  [DllImport("kernel32.dll")]
  static extern bool FreeLibrary(IntPtr hModule);

  /// LoadCert() 傳回的 caHandle 不是 thread safe!
  /// 如果需要 multi thread 簽章,
  /// 則請在每個 thread 建立一個 caHandle
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  delegate IntPtr FnLoadCert(string certConfig, int sgnact, out UInt32 errcode);
  /// 釋放由 LoadCert() 取得的 caHandle
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  delegate bool FnFreeCert(IntPtr caHandle);
  /// <summary>
  /// 建立簽章
  /// </summary>
  /// <param name="caHandle">由LoadCert()取得的憑證Handle</param>
  /// <param name="msg">要簽章的訊息</param>
  /// <param name="iDigSgnAtMsgPos">簽章訊息要插入在 msg 的哪個位置,
  ///      ＝0: 放在最前方
  ///      ＜0: 不簽章, 直接傳回 msg
  ///      ＞strlen(msg)傳回: "\n" "Invalid msg size or iDigSgnAtMsgPos"
  ///      </param>
  /// <returns>若第1碼為 '\n' 則表示為失敗訊息, 否則傳回插入簽章後的訊息內容</returns>
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
  delegate IntPtr FnMakeDigSgn(IntPtr caHandle, string msg, int iDigSgnAtMsgPos);

  enum CAErrCode
  {
     /// 沒有錯誤.
     ERR_Success = 0,
     /// CertConfig 格式錯誤.
     ERR_CertConfigFormat = 1,
     /// 無法開啟:憑證儲存區.
     ERR_CertStore = 2,
     /// 找不到有效憑證.
     ERR_NoCert = 3,
  }

  IntPtr DLLHandle_ = IntPtr.Zero;
  IntPtr CAHandle_ = IntPtr.Zero;
  FnFreeCert FnFreeCert_ = null;
  FnMakeDigSgn FnMakeDigSgn_ = null;

  public DigSgnHandler(string dllname, string certConfig, int sgnact)
  {
     DLLHandle_ = LoadLibrary(dllname);
     if (DLLHandle_ == IntPtr.Zero)
        return;

     IntPtr fn = GetProcAddress(DLLHandle_, "LoadCert");
     if (fn == IntPtr.Zero)
        return;
     FnLoadCert fnLoadCert = (FnLoadCert)Marshal.GetDelegateForFunctionPointer(fn, typeof(FnLoadCert));

     fn = GetProcAddress(DLLHandle_, "MakeDigSgn");
     if (fn == IntPtr.Zero)
        return;
     FnMakeDigSgn_ = (FnMakeDigSgn)Marshal.GetDelegateForFunctionPointer(fn, typeof(FnMakeDigSgn));

     UInt32 errcode;
     CAHandle_ = fnLoadCert(certConfig, sgnact, out errcode);
     if (CAHandle_ == IntPtr.Zero)
        return;
     fn = GetProcAddress(DLLHandle_, "FreeCert");
     if (fn != IntPtr.Zero)
        FnFreeCert_ = (FnFreeCert)Marshal.GetDelegateForFunctionPointer(fn, typeof(FnFreeCert));
  }
  public bool IsCertOK
  {
     get { return CAHandle_ != IntPtr.Zero; }
  }
  public void Dispose()
  {
     if (DLLHandle_ == IntPtr.Zero)
        return;
     if (FnFreeCert_ != null)
        FnFreeCert_(CAHandle_);
     FreeLibrary(DLLHandle_);
     DLLHandle_ = IntPtr.Zero;
  }
  public bool MakeDigSgn(ref string msg, int iDigSgnAtMsgPos)
  {
     if (iDigSgnAtMsgPos < 0)
        return true;
     if (FnMakeDigSgn_ == null)
     {
        msg = "Load MakeDigSgn function FAIL.";
        return false;
     }
     if (CAHandle_ == IntPtr.Zero)
     {
        msg = "Cert FAIL.";
        return false;
     }
     string result = Marshal.PtrToStringAnsi(FnMakeDigSgn_(CAHandle_, msg, iDigSgnAtMsgPos));
     if (result.Length > 0 && result[0] == '\n')
     {
        // 簽章失敗.
        msg = result.Substring(1);
        return false;
     }
     msg = result;
     return true;
  }
};
/// <summary>
/// 交易帳號.
/// </summary>
public class Acc
{
  SorApi.SorMktFlags Mkt_;
  string Acno_;
  string Name_;
  string DispText_;
  string BrkNo_;
  string IvacNo_;
  string SubacNo_;
  DigSgnHandler DigSgnHandler_;

  private void Init(SorApi.SorMktFlags mkt, string brkno, string ivacno, string subacno, string name)
  {
     Mkt_ = mkt;
     Name_ = string.IsNullOrEmpty(name) ? string.Empty : name;
     BrkNo_ = (brkno != null ? brkno : string.Empty);
     IvacNo_ = (ivacno != null ? ivacno : string.Empty);
     SubacNo_ = (subacno != null ? subacno : string.Empty);
     Acno_ = MakeAcno(BrkNo_, IvacNo_, SubacNo_);

     DispText_ = string.Empty;
     if ((int)(Mkt_ & SorApi.SorMktFlags.TwStk) != 0)
        DispText_ += "證";
     if ((int)(Mkt_ & SorApi.SorMktFlags.TwFuo) != 0)
        DispText_ += "期";
     if ((int)(Mkt_ & SorApi.SorMktFlags.FrStk) != 0)
        DispText_ += "複";
     if ((int)(Mkt_ & SorApi.SorMktFlags.FrFuo) != 0)
        DispText_ += "外";
     if ((int)(Mkt_ & SorApi.SorMktFlags.TwfQuot) != 0)
        DispText_ += "報";
     if ((int)(Mkt_ & SorApi.SorMktFlags.CnFuo) != 0)
        DispText_ += "Cf";
     DispText_ += string.Format("-{0}-{1}", Acno_, Name_);
  }
  /// <summary>
  /// 建立[帳號字串] = "brkNo-ivacNo", 如果有 subacNo 則為 "brkNo-ivacNo-subacNo"
  /// </summary>
  public static string MakeAcno(string brkNo, string ivacNo, string subacNo)
  {
     string acno = brkNo + "-" + ivacNo;
     if (string.IsNullOrEmpty(subacNo))
        return acno;
     return acno + "-" + subacNo;
  }
  /// <summary>
  /// 建構, acno 格式為 "brkNo-ivacNo-subacNo" 或 "brkNo-ivacNo".
  /// </summary>
  public Acc(SorApi.SorMktFlags mkt, string acno, string name)
  {
     string[] acs = acno.Split('-');
     Init(mkt, acs.Length > 0 ? acs[0] : null, acs.Length > 1 ? acs[1] : null, acs.Length > 2 ? acs[2] : null, name);
  }
  /// <summary>
  /// 建構.
  /// </summary>
  public Acc(SorApi.SorMktFlags mkt, string brkNo, string ivacNo, string subacNo, string name)
  {
     Init(mkt, brkNo, ivacNo, subacNo, name);
  }
  /// <summary>
  /// 顯示字串: "市場-BrkNo-IvacNo-SubacNo-Name", 如果沒有 SubacNo: "市場-BrkNo-IvacNo-Name"
  /// </summary>
  /// <returns></returns>
  public override string ToString()
  {
     return DispText_;
  }
  /// <summary>
  /// 可交易市場旗標.
  /// </summary>
  public SorApi.SorMktFlags MktFlag { get { return Mkt_; } }
  /// <summary>
  /// 投資人所屬券商代號.
  /// </summary>
  public string BrkNo { get { return BrkNo_; } }
  /// <summary>
  /// 投資人帳號.
  /// </summary>
  public string IvacNo { get { return IvacNo_; } }
  /// <summary>
  /// 子帳號.
  /// </summary>
  public string SubacNo { get { return SubacNo_; } }
  /// <summary>
  /// 帳號Key: "BrkNo-IvacNo" 或 "BrkNo-IvacNo-SubacNo"
  /// </summary>
  public string Key { get { return Acno_; } }
  /// <summary>
  /// 載入簽章憑證.
  /// </summary>
  public bool LoadCertConfig(string dllname, string cert, int sgnact)
  {
     FreeCertConfig();
     if (string.IsNullOrEmpty(cert))
        return true;
     DigSgnHandler_ = new DigSgnHandler(dllname, cert, sgnact);
     return DigSgnHandler_.IsCertOK;
  }
  /// <summary>
  /// 釋放簽章憑證.
  /// </summary>
  public void FreeCertConfig()
  {
     if (DigSgnHandler_ != null)
     {
        DigSgnHandler_.Dispose();
        DigSgnHandler_ = null;
     }
  }
  /// <summary>
  ///  將要傳送的信息簽章.
  /// </summary>
  /// <param name="msg">要簽章的訊息, </param>
  /// <param name="iDigSgnAtMsgPos">簽章要放在msg的哪個位置</param>
  /// <returns>true=成功(或不需要簽章:簽章內容填入msg後返回, false=失敗(找不到憑證or憑證失效...):錯誤訊息放在msg傳回.</returns>
  public bool MakeDigSgn(ref string msg, int iDigSgnAtMsgPos)
  {
     if (DigSgnHandler_ == null)
        return true;
     return DigSgnHandler_.MakeDigSgn(ref msg, iDigSgnAtMsgPos);
  }
}
/// <summary>
/// 交易帳號列表.
/// </summary>
public class Accs
{
  List<Acc> List_ = new List<Acc>();
  SortedList<string, Acc> Sorted_ = new SortedList<string, Acc>();
  /// <summary>
  /// 增加一個帳號.
  /// </summary>
  public bool Add(Acc acc, string caDLLName, string cert, int sgnact)
  {
     if (Sorted_.ContainsKey(acc.Key))
        return false;
     acc.LoadCertConfig(caDLLName, cert, sgnact);
     Sorted_.Add(acc.Key, acc);
     List_.Add(acc);
     return true;
  }
  /// <summary>
  /// 清除全部帳號.
  /// </summary>
  public void Clear()
  {
     foreach (Acc acc in List_)
        acc.FreeCertConfig();
     List_.Clear();
     Sorted_.Clear();
  }
  /// <summary>
  /// 嘗試取的一個帳號, 如果帳號不存在則傳回false.
  /// acno = "BrkNo-IvacNo" 或 "BrkNo-IvacNo-SubacNo"
  /// </summary>
  public bool TryGetValue(string acno, out Acc ac)
  {
     return Sorted_.TryGetValue(acno, out ac);
  }
  /// <summary>
  /// 取得帳號列表.
  /// </summary>
  public List<Acc> Values { get { return List_; } }
  /// <summary>
  ///  取得帳號筆數.
  /// </summary>
  public int Count { get { return List_.Count; } }
  /// <summary>
  /// 從 SorTable 取得可用帳號列表.
  /// </summary>
  public void SorTableParser(SorApi.SorTable table, string caDLLName, int sgnact)
  {
     if (table.IsInvalid)
        return;
     TIndex L, rcount = table.RecordsCount;
     SorApi.SorFields fields = table.Fields;
     SorApi.SorField fldMkt = fields.NameField("mkt");
     SorApi.SorField fldCert = fields.NameField("cert");
     SorApi.SorField fldName;
     Acc acc;
     if (fldMkt != null)
     {  // acno 格式: brkno-ivacno-subacno.
        fldName = fields.NameField("name");
        SorApi.SorField fldAcno = fields.NameField("acno");
        for (L = 0; L < rcount; ++L)
        {
           string mkt = table.RecordField(L, fldMkt);
           string acno = table.RecordField(L, fldAcno);
           string name = table.RecordField(L, fldName);
           if (mkt == null || acno == null)
              continue;
           int imkt;
           int.TryParse(mkt, out imkt);
           acc = new Acc((SorApi.SorMktFlags)imkt, acno, name);
           Add(acc, caDLLName, table.RecordField(L, fldCert), sgnact);                  
        }
     }
     else
     {  // BHNO-ACNO-SUBA 格式
        fldMkt = fields.NameField("MKTT");
        fldName = fields.NameField("CNAM");
        SorApi.SorField fldBhno = fields.NameField("BHNO");
        SorApi.SorField fldIvac = fields.NameField("ACNO");
        SorApi.SorField fldSuba = fields.NameField("SUBA");
        for (L = 0; L < rcount; ++L)
        {
           string mkt = table.RecordField(L, fldMkt);
           string bhno = table.RecordField(L, fldBhno);
           string ivac = table.RecordField(L, fldIvac);
           string suba = table.RecordField(L, fldSuba);
           string name = table.RecordField(L, fldName);
           if (mkt == null || bhno == null || ivac == null)
              continue;
           int imkt;
           int.TryParse(mkt, out imkt);
           acc = new Acc((SorApi.SorMktFlags)imkt, bhno, ivac, suba, name);
           Add(acc, caDLLName, table.RecordField(L, fldCert), sgnact);
        }
     }
  }
}
