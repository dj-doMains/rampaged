namespace RamPaged
{
    public abstract class Pageable
    {
        const int maxPageSize = 200;
        private int _pageSize = 10;

        public int PageNumber { get; set; } = 1;
        [IgnorePagedQueryString]
        public int SkipCount => PageSize * (PageNumber - 1);
        public virtual string SortBy { get; set; }

        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = (value > maxPageSize) ? maxPageSize : value;
            }
        }
    }
}