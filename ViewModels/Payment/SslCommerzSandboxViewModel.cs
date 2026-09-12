namespace LMS.Web.ViewModels.Payment
{
    public class SslCommerzSandboxViewModel
    {
        public string TransactionId { get; set; } = string.Empty;
        public string BankTranId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string StoreId { get; set; } = "lms6a7f32ae9cbfa";

    }
}
