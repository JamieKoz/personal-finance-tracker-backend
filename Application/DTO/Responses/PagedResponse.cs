using PersonalFinanceTracker.ValueObjects;

namespace PersonalFinanceTracker.DTO
{
    public class PagedResponse<T>
    {
        public List<T>? Data { get; set; }
        public PaginationInfo? Pagination { get; set; }
    }
}
