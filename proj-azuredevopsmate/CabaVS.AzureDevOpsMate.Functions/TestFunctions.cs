using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Worker;

namespace CabaVS.AzureDevOpsMate.Functions;

[SuppressMessage("Style", "IDE0060:Remove unused parameter")]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class SanityPing
{
    [Function("ping")]
    public string Ping(
        [TimerTrigger("*/5 * * * * *")] TimerInfo timer,
        FunctionContext context) => "pong";
}
