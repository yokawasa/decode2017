namespace Search.Azure
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Azure.Search.Models;

    [Serializable]
    public class SearchQueryBuilder
    {
        private const int DefaultHitPerPage = 1;

        public SearchQueryBuilder()
        {
            this.Refinements = new Dictionary<string, IEnumerable<string>>();
        }

        public string SearchText { get; set; }
        public string ScoreProfile { get; set; }
        public SearchMode SearchModeParam { get; set; }
        public QueryType QueryTypeParam { get; set; }

        public int PageNumber { get; set; }

        public int HitsPerPage { get; set; } = DefaultHitPerPage;

        public Dictionary<string, IEnumerable<string>> Refinements { get; private set; }

        public virtual void Reset()
        {
            this.SearchText = null;
            this.ScoreProfile = null;
            this.SearchModeParam = SearchMode.Any;
            this.QueryTypeParam = QueryType.Full;
            this.PageNumber = 0;
            this.Refinements.Clear();
        }
    }
}