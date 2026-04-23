namespace Api_bolsatrabajo.Model.Dtos
{
    public class StripePaymentIntentDto
    {
        public string Id { get; set; } = string.Empty;
        public string Object { get; set; } = "payment_intent";

        public long Amount { get; set; }
        public long AmountReceived { get; set; }

        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public string? LatestCharge { get; set; }

        public long Created { get; set; }

        public Dictionary<string, string> Metadata { get; set; }
            = new Dictionary<string, string>();
    }
}
