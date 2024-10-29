namespace api_crm.Models
{
    public class ApiResponse (int status, string message, object data)
    {
        public int Status { get; set; } = status;
        public string Message { get; set; } = message;
        public object? Data { get; set; } = data;

        public override string ToString()
        {
            return System.Text.Json.JsonSerializer.Serialize(this);
        }
    }
}
