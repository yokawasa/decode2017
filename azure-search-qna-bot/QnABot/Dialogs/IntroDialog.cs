namespace QnABot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Internals.Fibers;
    using Microsoft.Bot.Connector;
    using Search.Azure;

    [Serializable]
    public class IntroDialog : IDialog<object>
    {
        private ISearchClient searchClient;

        public IntroDialog(ISearchClient searchClient)
        {
            SetField.NotNull(out this.searchClient, nameof(searchClient), searchClient);
        }

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(this.StartSearchDialog);
            return Task.CompletedTask;
        }

        public Task StartSearchDialog(IDialogContext context, IAwaitable<IMessageActivity> input)
        {
            context.Call(new QnASearchDialog(this.searchClient), this.Done);
            return Task.CompletedTask;
        }

        public async Task Done(IDialogContext context, IAwaitable<IList<SearchHit>> input)
        {
            await context.PostAsync("ありがとうございました。またいつでもどうぞ!");
            context.Done<object>(null);
        }

    }
}
