namespace QnABot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Search.Azure;

    [Serializable]
    public class SearchHitStyler : PromptStyler
    {
        public override void Apply<T>(ref IMessageActivity message, string prompt, IReadOnlyList<T> options, IReadOnlyList<string> descriptions = null)
        {
            var hits = options as IList<SearchHit>;
            if (hits != null)
            {
                message.Text = hits[0].Description;
            }
            else
            {
                base.Apply<T>(ref message, prompt, options, descriptions);
            }
        }
    }
}