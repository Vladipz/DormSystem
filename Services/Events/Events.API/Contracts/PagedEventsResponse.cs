using Events.API.Shared;

namespace Events.API.Contracts
{
    public class PagedEventsResponse
    {
        public List<EventResponce> Events { get; set; } = new();

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int TotalPages { get; set; }

        public bool HasPreviousPage { get; set; }

        public bool HasNextPage { get; set; }

        public static PagedEventsResponse FromPagedList(PagedList<EventResponce> pagedList)
        {
            return new PagedEventsResponse
            {
                Events = pagedList.Items,
                PageNumber = pagedList.PageNumber,
                PageSize = pagedList.PageSize,
                TotalCount = pagedList.TotalCount,
                TotalPages = pagedList.TotalPages,
                HasPreviousPage = pagedList.HasPreviousPage,
                HasNextPage = pagedList.HasNextPage,
            };
        }
    }
}