namespace RichillCapital.SinoPac.Sor;

public enum SorMarketFlag
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
