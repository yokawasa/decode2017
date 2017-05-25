namespace Search.Azure
{
    using System.Configuration;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.Search;
    using Microsoft.Azure.Search.Models;

    public class AzureSearchClient : ISearchClient
    {
        private readonly ISearchIndexClient searchClient;
        private readonly IMapper<DocumentSearchResult, GenericSearchResult> mapper;
        private readonly string defaultScoreProfile;

        public AzureSearchClient(IMapper<DocumentSearchResult, GenericSearchResult> mapper)
        {
            this.mapper = mapper;
            SearchServiceClient client = new SearchServiceClient(
                ConfigurationManager.AppSettings["SearchDialogsServiceName"],
                new SearchCredentials(ConfigurationManager.AppSettings["SearchDialogsServiceKey"]));
            this.searchClient = client.Indexes.GetClient(ConfigurationManager.AppSettings["SearchDialogsIndexName"]);
            this.defaultScoreProfile = ConfigurationManager.AppSettings["SearchScoreProfileName"];
        }

        public async Task<GenericSearchResult> SearchAsync(SearchQueryBuilder queryBuilder)
        {
            if (queryBuilder.ScoreProfile == null && defaultScoreProfile != "" )
            {
                queryBuilder.ScoreProfile = defaultScoreProfile;
            }
            var documentSearchResult = await this.searchClient.Documents.SearchAsync(queryBuilder.SearchText, BuildParameters(queryBuilder));

            return this.mapper.Map(documentSearchResult);
        }

        private static SearchParameters BuildParameters(SearchQueryBuilder queryBuilder)
        {
            SearchParameters parameters = new SearchParameters
            {
                Top = queryBuilder.HitsPerPage,
                Skip = queryBuilder.PageNumber * queryBuilder.HitsPerPage,
                SearchMode = queryBuilder.SearchModeParam,
                QueryType = queryBuilder.QueryTypeParam
            };

            if ( queryBuilder.ScoreProfile != null)
            {
                parameters.ScoringProfile = queryBuilder.ScoreProfile;
            }

            if (queryBuilder.Refinements.Count > 0)
            {
                StringBuilder filter = new StringBuilder();
                string separator = string.Empty;

                foreach (var entry in queryBuilder.Refinements)
                {
                    foreach (string value in entry.Value)
                    {
                        filter.Append(separator);
                        filter.Append($"{entry.Key} eq '{EscapeFilterString(value)}'");
                        separator = " and ";
                    }
                }
                parameters.Filter = filter.ToString();
            }

            return parameters;
        }

        private static string EscapeFilterString(string s)
        {
            return s.Replace("'", "''");
        }
    }
}
