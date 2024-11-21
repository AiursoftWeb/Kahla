namespace Aiursoft.Kahla.SDK.Models;

public class ChatMessage
{
    public string Content { get; set; } = string.Empty;
    
    public string Preview { get; set; } = string.Empty;

    /// <summary>
    /// Server doesn't trust this value. Only for client side archive.
    /// </summary>
    public Guid SenderId { get; set; } = Guid.Empty;
}