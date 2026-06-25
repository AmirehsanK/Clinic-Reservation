namespace Clinic.Application.DTOs.Paging;

public class BasePaging
{
    public int PageId { get; set; }
    public int PageCount { get; set; }
    public int AllEntitiesCount { get; set; }
    public int StartPage { get; set; }
    public int EndPage { get; set; }
    public int TakeEntity { get; set; }
    public int SkipEntity { get; set; }
    public int BeforeAndAfterCount { get; set; }
    
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