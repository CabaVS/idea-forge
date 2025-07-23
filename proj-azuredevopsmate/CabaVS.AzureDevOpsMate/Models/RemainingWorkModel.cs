namespace CabaVS.AzureDevOpsMate.Models;

internal sealed record RemainingWorkModel(
    double Functionality,
    double Requirements,
    double Technical,
    double Other) : IComparable<RemainingWorkModel>
{
    private double Total => Functionality + Requirements + Technical + Other;

    public int CompareTo(RemainingWorkModel? other) =>
        other is null ? 1 : Total.CompareTo(other.Total);
    
    public static RemainingWorkModel operator +(RemainingWorkModel a, RemainingWorkModel b) =>
        new(
            Functionality: a.Functionality + b.Functionality,
            Requirements: a.Requirements + b.Requirements,
            Technical: a.Technical + b.Technical,
            Other: a.Other + b.Other);
}

internal static class RemainingWorkModelExtensions
{
    public static RemainingWorkModel Sum(this IEnumerable<RemainingWorkModel> source) =>
        source.Aggregate(
            new RemainingWorkModel(0, 0, 0, 0),
            (current, model) => current + model);
}
