using CabaVS.AzureDevOpsMate.Models;

namespace CabaVS.AzureDevOpsMate.Extensions;

internal static class RemainingWorkTypeExtensions
{
    private static readonly HashSet<string> FunctionalTags = ["Functionality"];
    private static readonly HashSet<string> PeriodicTags = ["Periodic"];
    private static readonly HashSet<string> RequirementsTags = ["Requirements"];
    private static readonly HashSet<string> TechnicalTags = ["Technical", "Non-functional requirements", "Refactoring"];
    
    public static RemainingWorkType DetermineFromTags(this IEnumerable<string> tags)
    {
        var tagSet = new HashSet<string>(tags, StringComparer.OrdinalIgnoreCase);

        return tagSet.Intersect(FunctionalTags, StringComparer.OrdinalIgnoreCase).Any() ? RemainingWorkType.Functionality
            : tagSet.Intersect(PeriodicTags, StringComparer.OrdinalIgnoreCase).Any() ? RemainingWorkType.Periodic
            : tagSet.Intersect(RequirementsTags, StringComparer.OrdinalIgnoreCase).Any() ? RemainingWorkType.Requirements
            : tagSet.Intersect(TechnicalTags, StringComparer.OrdinalIgnoreCase).Any() ? RemainingWorkType.Technical 
            : RemainingWorkType.Other;
    }
}
