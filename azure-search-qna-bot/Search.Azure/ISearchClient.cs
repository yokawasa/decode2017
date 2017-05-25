namespace Search.Azure
{
    using System.Threading.Tasks;
    public interface ISearchClient
    {
        Task<GenericSearchResult> SearchAsync(SearchQueryBuilder queryBuilder);
    }
}