using System.Runtime.CompilerServices;
using ChatbotAI.Application.Interfaces;

namespace ChatbotAI.Infrastructure.FakeAI;

public class FakeAiResponder : IAiResponder
{
    private static readonly string[] Sentences =
    [
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
        "Pellentesque vitae velit ex.",
        "Mauris dapibus risus quis suscipit vulputate.",
        "Eros diam egestas libero eu vulputate risus.",
        "Curabitur lacinia turpis nec sem bibendum, id pulvinar nunc hendrerit.",
        "Nullam vitae maximus nulla.",
        "Vivamus consequat lorem at nisi interdum, a facilisis erat pulvinar.",
        "Donec ac elit fermentum, porta risus sed, malesuada justo.",
        "Sed a est nec orci dapibus commodo.",
        "Cras in ligula id odio posuere fermentum."
    ];

    private readonly Random _random = new();

    public async IAsyncEnumerable<string> GenerateResponseStreamAsync(string input, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var response = GenerateRandomResponse();

        foreach (char ch in response)
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;

            yield return ch.ToString();
            await Task.Delay(20, cancellationToken);
        }
    }

    private string GenerateRandomResponse()
    {
        int responseType = _random.Next(3);

        int sentenceCount = responseType switch
        {
            0 => _random.Next(1, 3),
            1 => _random.Next(3, 6),
            2 => _random.Next(6, 10),
            _ => 3
        };

        var selectedSentences = Enumerable.Range(0, sentenceCount)
            .Select(_ => Sentences[_random.Next(Sentences.Length)]);
        
        if (sentenceCount > 5)
        {
            return string.Join("\n\n", selectedSentences.Chunk(3).Select(chunk => string.Join(" ", chunk)));
        }

        return string.Join(" ", selectedSentences);
    }
}