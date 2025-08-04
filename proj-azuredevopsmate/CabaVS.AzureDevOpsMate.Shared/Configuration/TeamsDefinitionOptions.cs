namespace CabaVS.AzureDevOpsMate.Shared.Configuration;

public sealed class TeamsDefinitionOptions
{
    public Dictionary<string, HashSet<string>> Teams { get; set; } = [];
}
