namespace QLBT.Shared.Models
{
    public class Message
    {
        public Command Type { get; set; }
        public string Token { get; set; } = "";
        public object? Data { get; set; }
    }

    public class Response
    {
        public bool Success { get; set; }
        public string Error { get; set; } = "";
        public object? Data { get; set; }
    }
}