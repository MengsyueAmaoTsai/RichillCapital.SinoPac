using System.Runtime.InteropServices;

namespace RichillCapital.SinoPac.Sor;

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
