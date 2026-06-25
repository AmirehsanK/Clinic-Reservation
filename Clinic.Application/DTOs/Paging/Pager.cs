namespace Clinic.Application.DTOs.Paging;

public class Pager
{
    public static BasePaging Build(int pageId, int allEntitiesCount, int takeEntity,int BeforeAndAfterCount)
    {
        var pageCount = Convert.ToInt32(Math.Ceiling(allEntitiesCount / (double)takeEntity));
        return new BasePaging
        {
            PageId = pageId,
            PageCount = pageCount,
            TakeEntity = takeEntity,
            AllEntitiesCount = allEntitiesCount,
            BeforeAndAfterCount = BeforeAndAfterCount,
            SkipEntity = (pageId - 1) * takeEntity,
            StartPage = pageId - BeforeAndAfterCount < 1 ? 1 : pageId - BeforeAndAfterCount,
            EndPage = pageId + BeforeAndAfterCount > pageCount ? pageCount : pageId + BeforeAndAfterCount
        };
    }
}