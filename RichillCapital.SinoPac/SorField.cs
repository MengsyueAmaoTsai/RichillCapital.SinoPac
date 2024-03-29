using System.Runtime.InteropServices;
using RichillCapital.SharedKernel;
using RichillCapital.SharedKernel.Monads;

namespace RichillCapital.SinoPac.Sor;

public sealed partial class SorField
{
    public const uint InvalidIndex = 0xffffffff;

    internal TImpl Impl_;

    internal SorField(TImpl impl)
    {
        Impl_ = impl;
    }

    internal static Result<SorField> Create(TImpl impl)
    {
        if (impl.IsInvalid)
        {
            return Result<SorField>
                .Failure(Error.Invalid("Invalid field implementation."));
        }

        return new SorField(impl).ToResult();
    }

    public SorProperties Properties => new(GetProperties(ref Impl_));

    public Result<uint> Index
    {
        get
        {
            var index = GetIndex(ref Impl_);

            if (index == InvalidIndex)
            {
                return Result<uint>.Failure(Error.Invalid("Invalid field index."));
            }

            return index.ToResult();
        }
    }
}

public sealed partial class SorField
{
    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorField_Properties")]
    private static extern TImpl GetProperties(ref TImpl impl);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorField_Index")]
    internal static extern uint GetIndex(ref TImpl impl);
}