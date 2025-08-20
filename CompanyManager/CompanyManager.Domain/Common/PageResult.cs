namespace CompanyManager.Domain.Common
{
    public sealed record PageResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize)
    {
        public bool HasNext => Page * PageSize < Total;
        public bool HasPrev => Page > 1;
    }
}
