namespace CabaVS.AzureDevOpsMate.Configuration;

internal sealed class TeamsDefinitionOptions
{
    public Dictionary<string, HashSet<string>> Teams { get; set; } = [];
}
