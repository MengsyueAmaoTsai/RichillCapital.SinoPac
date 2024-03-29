using System.Runtime.InteropServices;
using RichillCapital.SharedKernel;
using RichillCapital.SharedKernel.Monads;

namespace RichillCapital.SinoPac.Sor;

public sealed partial class SorFields
{
    private TImpl Impl_;

    internal SorFields(TImpl impl) => Impl_ = impl;

    public uint Count => GetCount(ref Impl_);

    public Maybe<SorField> GetByName(string fieldName)
    {
        var result = SorField.Create(CSorFields_NameField(ref Impl_, fieldName));

        if (result.IsFailure)
        {
            return Maybe<SorField>.Null;
        }

        return result.Value.ToMaybe();
    }

    public Maybe<SorField> GetByIndex(uint fieldIndex)
    {
        var result = SorField.Create(CSorFields_IndexField(ref Impl_, fieldIndex));

        if (result.IsFailure)
        {
            return Maybe<SorField>.Null;
        }

        return result.Value.ToMaybe();
    }

    public Result<uint> GetIndexByName(string name)
    {
        TImpl impl = CSorFields_NameField(ref Impl_, name);


        if (impl.IsInvalid)
        {
            return Result<uint>
                .Failure(Error.Invalid("Invalid field implementation."));
        }

        return SorField.GetIndex(ref impl).ToResult();
    }
}

public sealed partial class SorFields
{

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorFields_NameField")]
    private static extern TImpl CSorFields_NameField(ref TImpl impl, string fieldName);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorFields_IndexField")]
    private static extern TImpl CSorFields_IndexField(ref TImpl impl, uint fieldIndex);

    [DllImport(SorApi.Dll.SorClient, EntryPoint = "CSorFields_Count")]
    private static extern uint GetCount(ref TImpl impl);
}