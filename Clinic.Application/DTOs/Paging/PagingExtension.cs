namespace Clinic.Application.DTOs.Paging;

public static class PagingExtension
{
    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, BasePaging paging)
    {
        return query.Skip(paging.SkipEntity).Take(paging.TakeEntity);
    }
}