namespace Shared.PagedList
{
    public interface IPagedResponse<T>
    {
        List<T> Items { get; set; }

        int PageNumber { get; set; }

        int PageSize { get; set; }

        int TotalCount { get; set; }

        int TotalPages { get; set; }

        bool HasPreviousPage { get; set; }

        bool HasNextPage { get; set; }
    }
}