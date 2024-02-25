namespace DocumentInvoice.Service.DTO
{
    public class RBACInfo
    {
        public List<int>? UserCompanyIdList { get; set; }
        public bool IsAdminOrAccountant { get; set; }
    }
}
