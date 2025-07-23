using CabaVS.AzureDevOpsMate.Models;

namespace CabaVS.AzureDevOpsMate.Extensions;

internal static class RemainingWorkTypeExtensions
{
    private static readonly HashSet<string> FunctionalTags = ["Functionality"];
    private static readonly HashSet<string> RequirementsTags = ["Requirements"];
    private static readonly HashSet<string> TechnicalTags = ["Technical", "Non-functional requirements", "Refactoring"];
    
    public static RemainingWorkType DetermineFromTags(this IEnumerable<string> tags)
    {
        var tagSet = new HashSet<string>(tags, StringComparer.OrdinalIgnoreCase);

        return tagSet.Intersect(FunctionalTags).Any() ? RemainingWorkType.Functionality
            : tagSet.Intersect(RequirementsTags).Any() ? RemainingWorkType.Requirements
            : tagSet.Intersect(TechnicalTags).Any() ? RemainingWorkType.Technical 
            : RemainingWorkType.Other;
    }
}
