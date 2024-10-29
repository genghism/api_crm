namespace api_crm.Models
{
    public class AgingReport
    {
        public string CustomerCode { get; init; } = string.Empty;
        public string CustomerName { get; init; } = string.Empty;
        public string Manager { get; init; } = string.Empty;
        public decimal CurrentBalance { get; init; }
        public decimal Days0To30 { get; init; }
        public decimal Days31To60 { get; init; }
        public decimal Days61To90 { get; init; }
        public decimal Days91To120 { get; init; }
        public decimal Days121To150 { get; init; }
        public decimal Days151To180 { get; init; }
        public decimal Days181To210 { get; init; }
        public decimal Days211To240 { get; init; }
        public decimal Days241To270 { get; init; }
        public decimal Days271To300 { get; init; }
        public decimal Days301To330 { get; init; }
        public decimal Days331To360 { get; init; }
        public decimal Days360Plus { get; init; }
    }
}
