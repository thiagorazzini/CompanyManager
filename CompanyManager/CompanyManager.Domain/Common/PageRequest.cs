namespace CompanyManager.Domain.Common
{
    public sealed record PageRequest(int Page = 1, int PageSize = 20)
    {
        public int PageSafe => Page < 1 ? 1 : Page;
        public int PageSizeSafe => PageSize < 1 ? 20 : PageSize;

        public int Offset => (PageSafe - 1) * PageSizeSafe;

        public int Take => PageSizeSafe;
    }
}
