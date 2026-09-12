using System.Threading.Tasks;

namespace LMS.Web.Services.Interfaces
{
    public interface ISslCommerzService
    {
        Task<string?> InitiatePaymentAsync(string transactionId, decimal amount, string courseTitle, string studentName, string studentEmail, string successUrl, string failUrl, string cancelUrl);
        Task<bool> ValidatePaymentAsync(string valId);
        Task<bool> RefundPaymentAsync(string bankTranId, decimal amount, string refundRemarks);
    }
}
