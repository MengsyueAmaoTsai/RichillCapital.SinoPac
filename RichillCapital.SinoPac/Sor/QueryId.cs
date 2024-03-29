namespace RichillCapital.SinoPac.Sor;

internal record struct QueryId
{
    private const string Prefix = "qid";

    public QueryId() => Value = 0;

    public int Value { get; private set; }

    public QueryId Next()
    {
        Value++;
        return this;
    }

    public override readonly string ToString() => $"{Prefix}{Value}";
}