namespace Shop_ProjForWeb.Presentation.Models;

public class ApiErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public string? Details { get; set; }
}
