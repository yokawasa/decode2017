namespace Search.Azure
{
    using System.Collections.Generic;

    public class GenericSearchResult
    {
        public IEnumerable<SearchHit> Results { get; set; }
    }
}