namespace RichillCapital.SinoPac.Sor;

public class ExecutionTable : ReportTableBase
{
    public ExecutionTable(SorTable table)
        : base(table)
    {
        FieldCount = table.Fields.Count;
    }

    public uint FieldCount { get; private init; }
}
