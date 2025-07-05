using System.Runtime.CompilerServices;
using ChatbotAI.Application.Interfaces;

namespace ChatbotAI.Infrastructure.FakeAI;

public class FakeAiResponder : IAiResponder
{
    public async IAsyncEnumerable<string> GenerateResponseStreamAsync(string input, [EnumeratorCancellation] CancellationToken ct)
    {
        var response = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.";

        foreach (var ch in response)
        {
            if (ct.IsCancellationRequested) yield break;

            yield return ch.ToString();
            await Task.Delay(20, ct);
        }
    }
}