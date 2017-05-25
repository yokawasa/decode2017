# Azure Search QnA Bot

QnA Bot that use [Azure Search](https://azure.microsoft.com/en-us/services/search/) as the backend for dialogs like answering to your question. This is an demo bot app used during de:code 2017 DI08 session (Azure Search).

![](https://github.com/yokawasa/decode2017/blob/master/azure-search-qna-bot/images/capture-slack.PNG)

## Prerequisites

The minimum prerequisites to run this sample are:
* The latest update of Visual Studio 2015. You can download the community version [here](http://www.visualstudio.com) for free.
* The Bot Framework Emulator. To install the Bot Framework Emulator, download it from [here](https://emulator.botframework.com/). Please refer to [this documentation article](https://github.com/microsoft/botframework-emulator/wiki/Getting-Started) to know more about the Bot Framework Emulator

## Bot App Configuration
### Web.config
Add your Bot and azure search service info in Web.config (azure-search-qna-bot\QnABot\Web.config) 
```
  <appSettings>
    <!-- update these with your BotId, Microsoft App Id and your Microsoft App Password-->
    <add key="BotId" value="YourBotId" />
    <add key="MicrosoftAppId" value="" />
    <add key="MicrosoftAppPassword" value="" />

    <!-- These point to a sample dataset. Update with your own Azure Search service information as needed -->
    <add key="SearchDialogsServiceName" value="YourAzureSearchServiceName" />
    <add key="SearchDialogsServiceKey" value="YourAzureSearchServiceKey" />
    <add key="SearchDialogsIndexName" value="YourAzureSearchIndexName" />
    <add key="SearchScoreProfileName" value="YourAzureSearchScoreProfileName" />
  </appSettings
```

## Azure Search
### Index Schema (index: qnakb)
```
{
    "name": "qnakb",
    "fields": [
        { "name":"id", "type":"Edm.String", "key":true, "retrievable":true, "searchable":false, "filterable":false, "sortable":false, "facetable":false },
        { "name":"question", "type":"Edm.String", "retrievable":true, "searchable":true, "filterable":false, "sortable":false, "facetable":false,"analyzer":"ja.lucene"},
        { "name":"answer", "type":"Edm.String", "retrievable":true, "searchable":true, "filterable":false, "sortable":false, "facetable":false,"analyzer":"ja.lucene"},
        { "name":"category", "type":"Edm.String", "retrievable":true, "searchable":false, "filterable":true, "sortable":true, "facetable":true },
        { "name":"url", "type":"Edm.String", "retrievable":true, "searchable":false, "filterable":false, "sortable":false, "facetable":false },
        { "name":"tags", "type":"Collection(Edm.String)", "retrievable":true, "searchable":false, "filterable":true, "sortable":false, "facetable":false }
     ],
     "scoringProfiles": [
         {
            "name": "weightedFields",
            "text": {
                "weights": {
                    "question": 9,
                    "answer": 1
                }
            }
        },
        {
            "name": "personalizedBoost",
            "functions": [
            {
                "type": "tag",
                "boost": 5,
                "fieldName": "tags",
                "tag": { "tagsParameter": "featuredtags" }
            }
            ]
        }
     ],
     "corsOptions": {
        "allowedOrigins": ["*"],
        "maxAgeInSeconds": 300
    }
}
```


