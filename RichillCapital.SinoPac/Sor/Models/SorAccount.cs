namespace RichillCapital.SinoPac.Sor.Models;

public class SorAccount
{
    SorMktFlags Mkt_;
    string Acno_;
    string Name_;
    string DispText_;
    string BrkNo_;
    string IvacNo_;
    string SubacNo_;
    DigSgnHandler DigSgnHandler_;

    private void Init(SorMktFlags mkt, string brkno, string ivacno, string subacno, string name)
    {
        Mkt_ = mkt;
        Name_ = string.IsNullOrEmpty(name) ? string.Empty : name;
        BrkNo_ = (brkno != null ? brkno : string.Empty);
        IvacNo_ = (ivacno != null ? ivacno : string.Empty);
        SubacNo_ = (subacno != null ? subacno : string.Empty);
        Acno_ = MakeAcno(BrkNo_, IvacNo_, SubacNo_);

        DispText_ = string.Empty;
        if ((int)(Mkt_ & SorMktFlags.TwStk) != 0)
            DispText_ += "證";
        if ((int)(Mkt_ & SorMktFlags.TwFuo) != 0)
            DispText_ += "期";
        if ((int)(Mkt_ & SorMktFlags.FrStk) != 0)
            DispText_ += "複";
        if ((int)(Mkt_ & SorMktFlags.FrFuo) != 0)
            DispText_ += "外";
        if ((int)(Mkt_ & SorMktFlags.TwfQuot) != 0)
            DispText_ += "報";
        if ((int)(Mkt_ & SorMktFlags.CnFuo) != 0)
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
    public SorAccount(SorMktFlags mkt, string acno, string name)
    {
        string[] acs = acno.Split('-');
        Init(mkt, acs.Length > 0 ? acs[0] : null, acs.Length > 1 ? acs[1] : null, acs.Length > 2 ? acs[2] : null, name);
    }
    /// <summary>
    /// 建構.
    /// </summary>
    public SorAccount(SorMktFlags mkt, string brkNo, string ivacNo, string subacNo, string name)
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
    public SorMktFlags MktFlag { get { return Mkt_; } }
    /// <summary>
    /// 投資人所屬券商代號.
    /// </summary>
    public string BrokerageNumber { get { return BrkNo_; } }
    /// <summary>
    /// 投資人帳號.
    /// </summary>
    public string Number { get { return IvacNo_; } }
    /// <summary>
    /// 子帳號.
    /// </summary>
    public string SubAccountNumber { get { return SubacNo_; } }
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

    public bool IsSubAccount() => !string.IsNullOrEmpty(SubAccountNumber);
}
