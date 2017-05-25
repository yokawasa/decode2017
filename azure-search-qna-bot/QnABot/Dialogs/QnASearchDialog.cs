namespace QnABot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Internals.Fibers;
    using Microsoft.Bot.Connector;
    using Microsoft.Azure.Search.Models;
    using Search.Azure;

    [Serializable]
    public class QnASearchDialog : IDialog<IList<SearchHit>>
    {
        protected readonly ISearchClient SearchClient;
        protected readonly SearchQueryBuilder QueryBuilder;
        protected readonly PromptStyler HitStyler;
        private readonly IList<SearchHit> selected = new List<SearchHit>();

        private IList<SearchHit> found;

        public QnASearchDialog(ISearchClient searchClient, SearchQueryBuilder queryBuilder = null, PromptStyler searchHitStyler = null, bool multipleSelection = true)
        {
            SetField.NotNull(out this.SearchClient, nameof(searchClient), searchClient);

            this.QueryBuilder = queryBuilder ?? new SearchQueryBuilder();
            this.HitStyler = searchHitStyler ?? new SearchHitStyler();
        }

        public Task StartAsync(IDialogContext context)
        {
            return this.InitialPrompt(context);
        }

        public async Task Search(IDialogContext context, IAwaitable<string> input)
        {
            //Default Param
            this.QueryBuilder.SearchModeParam = SearchMode.Any;
            this.QueryBuilder.QueryTypeParam = QueryType.Full;

            string text = input != null ? await input : null;
            if (text != null)
            {
                this.QueryBuilder.SearchText = text;
            }

            var response = await this.ExecuteSearchAsync();

            if (response.Results.Count() == 0)
            {
                await this.NoResultsConfirmRetry(context);
            }
            else
            {
                var message = context.MakeMessage();
                this.found = response.Results.ToList();
                this.HitStyler.Apply(
                    ref message, "",
                    this.found.ToList().AsReadOnly());
                await context.PostAsync(message);
                await context.PostAsync(
                    "別の回答候補をご希望の場合は*more*を、新しいなQ&Aをお探しの場合は*find*を入力ください");
                context.Wait(this.ActOnSearchResults);
            }
        }

        protected virtual Task InitialPrompt(IDialogContext context)
        {
            string prompt = "どんなQ&Aをお探しですか？";
            PromptDialog.Text(context, this.Search, prompt);
            return Task.CompletedTask;
        }

        protected virtual Task NoResultsConfirmRetry(IDialogContext context)
        {
            PromptDialog.Confirm(context, this.ShouldRetry, "残念ながら見つかりませんでした。他のキーワードでお試しになりますか？");
            return Task.CompletedTask;
        }

        protected virtual async Task ShouldContinueSearching(IDialogContext context, IAwaitable<bool> input)
        {
            try
            {
                bool shouldContinue = await input;
                if (shouldContinue)
                {
                    await this.InitialPrompt(context);
                }
                else
                {
                    context.Done(this.selected);
                }
            }
            catch (TooManyAttemptsException)
            {
                context.Done(this.selected);
            }
        }

        protected async Task<GenericSearchResult> ExecuteSearchAsync()
        {
            return await this.SearchClient.SearchAsync(this.QueryBuilder);
        }


        private async Task ShouldRetry(IDialogContext context, IAwaitable<bool> input)
        {
            try
            {
                bool retry = await input;
                if (retry)
                {
                    await this.InitialPrompt(context);
                }
                else
                {
                    context.Done<IList<SearchHit>>(null);
                }
            }
            catch (TooManyAttemptsException)
            {
                context.Done<IList<SearchHit>>(null);
            }
        }

        private async Task ActOnSearchResults(IDialogContext context, IAwaitable<IMessageActivity> input)
        {
            var activity = await input;
            var choice = activity.Text;

            switch (choice.ToLowerInvariant())
            {
                case "find":
                    this.QueryBuilder.Reset();
                    await this.InitialPrompt(context);
                    break;

                case "more":
                    this.QueryBuilder.PageNumber++;
                    await this.Search(context, null);
                    break;

                case "done":
                    context.Done(this.selected);
                    break;

                default:
                    break;
            }
        }
    }
}