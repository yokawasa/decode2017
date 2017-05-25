var azureSearchServiceName = "YourAzureSearchServiceName";
var azureSearchQueryApiKey = "YourAzureSearchQueryKey";
var inSearch = false;

function execSearch()
{
    // Execute a search to lookup viable sessions
    var q_index = $("#q_index").val();
    var q_fuzzy = $('input[id="q_fuzzy"]:checked').length > 0;
    var q_search = $("#q_search").val();
    if (q_fuzzy) {
        q_search = q_search + '~2';
    }
    q_search = encodeURIComponent(q_search);

    var q_filter = encodeURIComponent($("#q_filter").val());
    var q_searchmode = $("#q_searchmode").val();
    var q_querytype = $("#q_querytype").val();
    var q_scoringprofile = $("#q_scoringprofile").val();
    var q_highlight = $('input[id="q_highlight"]:checked').length > 0;
    var q_scoringparameter = encodeURIComponent($("#q_scoringparameter").val());

    var searchQueryURL = "https://" + azureSearchServiceName + ".search.windows.net/indexes/" + q_index + "/docs?$top=100&$select=id,question,answer,url,category,tags&$count=true&highlight=question,answer&api-version=2016-09-01-Preview&search=" + q_search + "&queryType=" + q_querytype + "&searchMode=" + q_searchmode;
    if (q_filter != '' ) {
        searchQueryURL = searchQueryURL + "&$filter=" + q_filter;
    }
    if (q_scoringprofile != '' ) {
        searchQueryURL = searchQueryURL + "&scoringProfile=" + q_scoringprofile;
    }
    if (q_scoringparameter != '' ) {
        searchQueryURL = searchQueryURL + "&scoringParameter=" + q_scoringparameter;
    }

    inSearch= true;
    $.ajax({
        url: searchQueryURL,
        beforeSend: function (request) {
            request.setRequestHeader("api-key", azureSearchQueryApiKey);
            request.setRequestHeader("Content-Type", "application/json");
            request.setRequestHeader("Accept", "application/json; odata.metadata=none");
        },
        type: "GET",
        success: function (data) {
            $( "#hits" ).html('');
            $( "#hits" ).append( 'hits: ' + data['@odata.count']);
            $( "#colcontainer" ).html('');
            $( "#colcontainer" ).append('<ul class="list-group">');
            for (var item in data.value)
            {
                var id = data.value[item].id;
                var queryType = ''
                if (q_highlight) {
                    question = data.value[item]['@search.highlights'].question;
                    if (typeof question === "undefined") {
                        question = data.value[item].question;
                    }
                } else {
                    question = data.value[item].question;
                }
                var answer = ''
                if (q_highlight) {
                    answer = data.value[item]['@search.highlights'].answer;
                    if (typeof answer === "undefined") {
                        answer = data.value[item].answer;
                    }
                } else {
                    answer = data.value[item].answer;
                }
                var tags = (data.value[item].tags).join();
                var score = data.value[item]['@search.score'];
                $( "#colcontainer" ).append( 
                        '<li class="list-group-item">' 
                        + '<b><h4>' +  question + '</h4></b></br>'
                        + answer
                        + '</br>Tags: <font color=blue>' + tags + '</font>'
                        + '</br>(Score: <font color=red>'+ score +'</font>)</li>' );
            }
            $( "#colcontainer" ).append('</ul>');
            inSearch= false;
        }
    });
}

