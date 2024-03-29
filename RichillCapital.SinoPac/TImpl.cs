namespace RichillCapital.SinoPac.Sor;

struct TImpl
{
    IntPtr Impl_;

    public bool IsInvalid { get { return Impl_ == IntPtr.Zero; } }
}
