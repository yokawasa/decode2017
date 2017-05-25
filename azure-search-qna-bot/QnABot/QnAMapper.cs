namespace QnABot
{
    using System.Linq;
    using Microsoft.Azure.Search.Models;
    using Search.Azure;

    public class QnAMapper : IMapper<DocumentSearchResult, GenericSearchResult>
    {
        public GenericSearchResult Map(DocumentSearchResult documentSearchResult)
        {
            var searchResult = new GenericSearchResult();

            searchResult.Results = documentSearchResult.Results.Select(r => ToSearchHit(r)).ToList();

            return searchResult;
        }

        private static SearchHit ToSearchHit(SearchResult hit)
        {
            return new SearchHit
            {
                Key = (string)hit.Document["id"],
                Title = (string)hit.Document["question"],
                Description = (string)hit.Document["answer"]
            };
        }
    }
}
