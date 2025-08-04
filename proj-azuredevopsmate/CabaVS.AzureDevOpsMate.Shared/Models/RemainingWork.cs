namespace CabaVS.AzureDevOpsMate.Shared.Models;

public sealed record RemainingWork(
    double Functionality,
    double Requirements,
    double Technical,
    double Other) : IComparable<RemainingWork>
{
    public double Total => Functionality + Requirements + Technical + Other;

    public int CompareTo(RemainingWork? other) =>
        other is null ? 1 : Total.CompareTo(other.Total);
    
    public static RemainingWork operator +(RemainingWork a, RemainingWork b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        
        return new RemainingWork(
            Functionality: a.Functionality + b.Functionality,
            Requirements: a.Requirements + b.Requirements,
            Technical: a.Technical + b.Technical,
            Other: a.Other + b.Other);
    }
}

public static class RemainingWorkModelExtensions
{
    public static RemainingWork Sum(this IEnumerable<RemainingWork> source) =>
        source.Aggregate(
            new RemainingWork(0, 0, 0, 0),
            (current, model) => current + model);
}
