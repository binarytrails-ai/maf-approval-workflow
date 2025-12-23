using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace ContosoBikestore.Agent.Host.Services;

/// <summary>
/// Debug wrapper to inspect messages going to/from the LLM.
/// </summary>
#pragma warning disable CS1591 // Missing XML comment
public class DebugChatClient(IChatClient innerClient) : IChatClient
{
    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        // PUT BREAKPOINT HERE - Inspect chatMessages to see conversation history
        var messageList = chatMessages.ToList();
        Console.WriteLine($"[DEBUG] Sending {messageList.Count} messages to LLM:");
        //foreach (var msg in messageList)
        //{
        //    var preview = msg.Text?.Length > 100 ? msg.Text.Substring(0, 100) + "..." : msg.Text;
        //    Console.WriteLine($"  - {msg.Role}: {preview}");
        //}

        var result = await innerClient.GetResponseAsync(messageList, options, cancellationToken);

        // PUT BREAKPOINT HERE - Inspect result to see LLM response
        //Console.WriteLine($"[DEBUG] LLM Response: {result.Message?.Text?.Substring(0, Math.Min(100, result.Message?.Text?.Length ?? 0))}...");
        return result;
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> chatMessages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // PUT BREAKPOINT HERE - Inspect chatMessages for streaming calls
        var messageList = chatMessages.ToList();
        Console.WriteLine($"[DEBUG STREAMING] Sending {messageList.Count} messages to LLM:");
        //foreach (var msg in messageList)
        //{
        //    var preview = msg.Text?.Length > 100 ? msg.Text.Substring(0, 100) + "..." : msg.Text;
        //    Console.WriteLine($"  - {msg.Role}: {preview}");
        //}

        await foreach (var update in innerClient.GetStreamingResponseAsync(chatMessages, options, cancellationToken))
        {
            yield return update;
        }
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
        => innerClient.GetService(serviceType, serviceKey);

    public void Dispose() => innerClient.Dispose();
}
#pragma warning restore CS1591
