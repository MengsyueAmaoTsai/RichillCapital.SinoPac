using RichillCapital.SinoPac.Sor.Models;

namespace RichillCapital.SinoPac.Sor;

public class AccountManager
{
    List<SorAccount> List_ = new List<SorAccount>();
    SortedList<string, SorAccount> Sorted_ = new SortedList<string, SorAccount>();

    public bool Add(SorAccount acc, string caDLLName, string cert, int sgnact)
    {
        if (Sorted_.ContainsKey(acc.Key))
            return false;
        acc.LoadCertConfig(caDLLName, cert, sgnact);
        Sorted_.Add(acc.Key, acc);
        List_.Add(acc);

        return true;
    }

    /// 清除全部帳號.
    public void Clear()
    {
        foreach (SorAccount acc in List_)
            acc.FreeCertConfig();
        List_.Clear();
        Sorted_.Clear();
    }

    /// 嘗試取的一個帳號, 如果帳號不存在則傳回false.
    /// acno = "BrkNo-IvacNo" 或 "BrkNo-IvacNo-SubacNo"
    public bool TryGetValue(string acno, out SorAccount ac)
    {
        return Sorted_.TryGetValue(acno, out ac);
    }

    public List<SorAccount> Values { get { return List_; } }
    public int Count { get { return List_.Count; } }

    /// 從 SorTable 取得可用帳號列表.
    public void SorTableParser(SorTable table, string caDLLName, int sgnact)
    {
        if (table.IsInvalid)
            return;

        uint L, rcount = table.RecordsCount;

        SorFields fields = table.Fields;
        SorField fldMkt = fields.NameField("mkt");
        SorField fldCert = fields.NameField("cert");
        SorField fldName;
        SorAccount acc;

        if (fldMkt != null)
        {  // acno 格式: brkno-ivacno-subacno.
            fldName = fields.NameField("name");
            SorField fldAcno = fields.NameField("acno");
            for (L = 0; L < rcount; ++L)
            {
                string mkt = table.RecordField(L, fldMkt);
                string acno = table.RecordField(L, fldAcno);
                string name = table.RecordField(L, fldName);
                if (mkt == null || acno == null)
                    continue;
                int imkt;
                int.TryParse(mkt, out imkt);
                acc = new SorAccount((SorMarketFlag)imkt, acno, name);
                Add(acc, caDLLName, table.RecordField(L, fldCert), sgnact);
            }
        }
        else
        {  // BHNO-ACNO-SUBA 格式
            fldMkt = fields.NameField("MKTT");
            fldName = fields.NameField("CNAM");
            SorField fldBhno = fields.NameField("BHNO");
            SorField fldIvac = fields.NameField("ACNO");
            SorField fldSuba = fields.NameField("SUBA");
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
                acc = new SorAccount((SorMarketFlag)imkt, bhno, ivac, suba, name);
                Add(acc, caDLLName, table.RecordField(L, fldCert), sgnact);
            }
        }
    }
}
