namespace Freedom.WebApi.Models
{
    public class AggregateRequest
    {
        public string GroupBy { get; set; }

        public string Function { get; set; }

        public string Value { get; set; }
    }
}