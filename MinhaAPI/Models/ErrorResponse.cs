namespace MinhaAPI.Models;

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public string Details { get; set; }
    public DateTime Timestamp { get; set; }
    public string Path { get; set; }
    public Dictionary<string, string[]> Errors { get; set; }

    public ErrorResponse()
    {
        Timestamp = DateTime.UtcNow;
    }
}