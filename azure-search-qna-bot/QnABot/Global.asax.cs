namespace QnABot
{
    using System.Web;
    using System.Web.Http;
    using Autofac;
    using Microsoft.Azure.Search.Models;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Internals.Fibers;
    using QnABot.Dialogs;
    using Search.Azure;

    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            ContainerBuilder builder = new ContainerBuilder();

            builder.RegisterType<IntroDialog>()
              .As<IDialog<object>>()
              .InstancePerDependency();

            builder.RegisterType<QnAMapper>()
               .Keyed<IMapper<DocumentSearchResult, GenericSearchResult>>(FiberModule.Key_DoNotSerialize)
               .AsImplementedInterfaces()
               .SingleInstance();

            builder.RegisterType<AzureSearchClient>()
                .Keyed<AzureSearchClient>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .SingleInstance();

            //it's obsolate but do not remove
            builder.Update(Conversation.Container);

            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}