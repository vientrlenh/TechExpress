namespace TechExpress.Application.Dtos.Responses
{
    /// <summary>
    /// DTO option chung cho combobox enum (value int + name string).
    /// </summary>
    public class EnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

