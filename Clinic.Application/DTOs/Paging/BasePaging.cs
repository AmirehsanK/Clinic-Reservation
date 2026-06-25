namespace Clinic.Application.DTOs.Paging;

public class BasePaging
{
    public int PageId { get; set; } = 1;
    public int PageCount { get; set; }
    public int AllEntitiesCount { get; set; }
    public int StartPage { get; set; }
    public int EndPage { get; set; }
    public int TakeEntity { get; set; } = 12;
    public int SkipEntity { get; set; }
    public int BeforeAndAfterCount { get; set; } = 3;

    public int GetLastPage()
    {
        return (int)Math.Ceiling(AllEntitiesCount / (double)TakeEntity);
    }

    public string GetCurrentPagingStatus()
    {
        var startItem = 1;
        var endItem = AllEntitiesCount;

        if (EndPage > 1)
        {
            startItem = (PageId - 1) * TakeEntity + 1;
            endItem = PageId * TakeEntity;

            if (endItem > AllEntitiesCount)
            {
                endItem = AllEntitiesCount;
            }
            
        }
        return $"نمایش {startItem} تا {endItem} از {AllEntitiesCount} مورد";
    }
    
    public BasePaging GetCurrentPaging()
    {
        return this;
    }
}