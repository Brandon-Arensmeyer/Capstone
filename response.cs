using System;
using System.ComponentModel.DataAnnotations;
using System.Formats.Asn1;
using System.Net.Quic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Spectre.Console;
using Azure.AI.OpenAI;
using Spectre.Console.Rendering;
using Azure.Core;
using System.ComponentModel;
using System.Net.Http;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Reflection.Metadata.Ecma335;
using System.Numerics;
using Azure;

var openAI = new OpenAIClient("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");
string readResource (string resourceName) {
    using var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
    using var reader = new StreamReader(stream);
    return reader.ReadToEnd();
}
var systemFormatingPrompt = readResource("Capstone.SpectreConsoleFormattingSystemPrompt.md");

// async void simpleResponse(){
//     var cts = new CancellationTokenSource();
//     while(true){
//         var prompt = await(new TextPrompt<string>("> ")).ShowAsync(AnsiConsole.Console,cts.Token);
//         await AnsiConsole.Live((new Panel(new Markup("Hello"))).Expand().Header("Response", Justify.Center)).StartAsync(updateLayoutAsync(prompt));
//     }

    
// }

System.BinaryData makeParameters(params (string, string, string)[] parameters) {
    var properties = new System.Collections.Generic.Dictionary<string, object>();
    foreach (var pv in parameters) {
        var p = new System.Collections.Generic.Dictionary<string, object>();
        p.Add("type", pv.Item2);
        p.Add("description", pv.Item3);
        properties.Add(pv.Item1, p);
    }
    var o = new System.Collections.Generic.Dictionary<string, object>();
    o.Add("type", "object");
    o.Add("properties", properties);
    return BinaryData.FromObjectAsJson(o);
}

Task<StreamingResponse<StreamingChatCompletionsUpdate>> getCompletion(List<ChatRequestMessage> context,String prompt) {
    var options = new ChatCompletionsOptions();
    options.DeploymentName = "gpt-4-1106-preview";
    options.Tools.Add(
        new ChatCompletionsFunctionToolDefinition(
            new FunctionDefinition(){
                Name = "getWeather", 
                Description = "You will recive the weather and the JSON document that describes the clouds, temperature, humidity, etc...",
                Parameters = makeParameters(
                    ("cityName", "string", "name of the city we want the weather for")
                )}));
    options.Tools.Add(
        new ChatCompletionsFunctionToolDefinition(
            new FunctionDefinition(){
                Name = "getQuote", 
                Description = "You will recive a quote and the JSON document that gives the author of the quote, its tags, and its id.",
                Parameters = makeParameters(
                    ("quote", "string", "the type of quote that we want")
                )}));
    options.Tools.Add(
        new ChatCompletionsFunctionToolDefinition(
            new FunctionDefinition(){
                Name = "getJoke", 
                Description = "You will recive a chuck norris joke",
                Parameters = makeParameters(
                    ("joke", "string", "the type of joke that we want")
                )}));
    options.Tools.Add(
        new ChatCompletionsFunctionToolDefinition(
            new FunctionDefinition(){
                Name = "NFLSchedule", 
                Description = "To the weekly schedule of the NFL schedule, should contain all the information about the teams who are playing that week",
                Parameters = makeParameters(
                    ("Year", "number", "the start of the year as an integer of the NFL schedule you are asking for"),
                    ("Week", "number", "the week as an integer number of the schedule, with week number one being the start of the season")
                )}));
    options.Tools.Add(
        new ChatCompletionsFunctionToolDefinition(
            new FunctionDefinition(){
                Name = "NFLDepthChart", 
                Description = "It gives you the players on a team durring a specific season",
                Parameters = makeParameters(
                    ("Year", "number", "the start of the year as an integer of the NFL schedule you are asking for"),
                    ("ID", "number", "The ID of the team you want to see the players of")
                )}));
    options.Tools.Add(
        new ChatCompletionsFunctionToolDefinition(
            new FunctionDefinition(){
                Name = "NFLStats", 
                Description = "To get the stats of a player durring a week of an NFL season",
                Parameters = makeParameters(
                    ("Year", "number", "the start of the year as an integer of the NFL schedule you are asking for"),
                    ("Week", "number", "the week as an integer number of the schedule, with week number one being the start of the season")
                )}));
    if(context.Count == 0){
        context.Add(new ChatRequestSystemMessage($"{systemFormatingPrompt}\n You are an AI. "));
    }
    if(!String.IsNullOrWhiteSpace(prompt)){
        context.Add(new ChatRequestUserMessage(prompt));

    }
    
    foreach(var m in context){
        options.Messages.Add(m);
    }
    return openAI.GetChatCompletionsStreamingAsync(options);
}

Func<LiveDisplayContext, Task> updateLayoutAsync(string prompt){
    var context = new List<ChatRequestMessage>();
    return async ldc =>{
        var txtString = " ";
        void render(){
            try{
                ldc.UpdateTarget(new Panel(new Markup($"{txtString}")).Expand().Header("Response", Justify.Center));
            }catch{
                try{
                    var openi = txtString.LastIndexOf('[');
                    var closei = txtString.LastIndexOf(']');
                    if(openi < closei){
                        ldc.UpdateTarget(new Panel(new Markup($"{txtString}[/]")).Expand().Header("Response", Justify.Center));

                    }
                    else{
                        ldc.UpdateTarget(new Panel(new Markup($"{txtString[..(openi - 1)]}")).Expand().Header("Response", Justify.Center));
                    }
                }catch{
                    // Console.WriteLine(txtString);
                    txtString = txtString.Replace("[color=", "[");
                    ldc.UpdateTarget(new Panel(Markup.FromInterpolated($"[blue]{txtString}[/]")).Expand().Header("Response", Justify.Center));
                    // throw;
                }
            }
            
            ldc.Refresh(); 
        }

        var cont = false;
        var counter = 0;
        do{
            ChatRole? role = null;
            string? functionName = null;
            string? toolCallId = null;
            string functionArgs = "";

            var updates = await getCompletion(context, prompt);
            await foreach(var chunk in updates){
                if (chunk.Role is not null) {
                    role = chunk.Role;
                }
                switch (role){
                    case var r when r == ChatRole.Assistant:
                        if (chunk.ToolCallUpdate is null) {
                            txtString += chunk.ContentUpdate;
                        } else {
                            if (chunk.ToolCallUpdate is StreamingFunctionToolCallUpdate ftc) {
                                if (toolCallId is null) {
                                    toolCallId = ftc.Id;
                                    functionName = ftc.Name;
                                }
                                functionArgs += ftc.ArgumentsUpdate;
                                txtString = $"{functionName}\n{functionArgs}";
                            }
                        }
                        render();
                        break;
                }
                
            }
            render();
            if(String.IsNullOrEmpty(functionName)){
                context.Add(new ChatRequestAssistantMessage(txtString));
                cont = false;
            }
            else{
    


                if(functionName == "getQuote"){
                    var parameters = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(functionArgs);
                    var cityName = parameters["quote"];
                    while(counter == 0){
                        context.Add(new ChatRequestFunctionMessage(functionName, $$"""{{{await quote()}}"""));
                        cont = true;
                        // add counter
                        // txtString += $"\ncityName: {cityName}\n";
                        prompt = "";                            
                        counter += 1;
                    }
                        
                }
                else if(functionName == "getWeather"){
                    var parameters = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(functionArgs);
                    var cityName = parameters["cityName"];
                            
                    context.Add(new ChatRequestFunctionMessage(functionName, $$"""{{{await GetWeatherData(cityName)}}"""));
                            
                    cont = true;
                    // add counter
                    // txtString += $"\ncityName: {cityName}\n";
                    prompt = "";
                    counter += 1;    
                }
                else if(functionName == "getJoke"){
                    var parameters = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(functionArgs);
                    var cityName = parameters["joke"];
                            
                    context.Add(new ChatRequestFunctionMessage(functionName, $$"""{{{await joke()}}"""));
                            
                    cont = true;
                    // add counter
                    // txtString += $"\ncityName: {cityName}\n";
                    prompt = "";
                    counter += 1;    
                }
                else if(functionName == "NFLSchedule"){
                    var parameters = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(functionArgs);
                    var year = Int32.Parse(parameters["Year"].ToString());
                    var week = Int32.Parse(parameters["Week"].ToString());
                            
                    context.Add(new ChatRequestFunctionMessage(functionName, $$"""{{{await NFLSchedule((int) year,(int) week)}}"""));
                            
                    cont = true;
                    // add counter
                    // txtString += $"\ncityName: {cityName}\n";
                    prompt = "";
                    counter += 1;    
                }
                else if(functionName == "NFLDepthChart"){
                    var parameters = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(functionArgs);        
                    var year = Int32.Parse(parameters["Year"].ToString());
                    var ID = Int32.Parse(parameters["ID"].ToString());
                    context.Add(new ChatRequestFunctionMessage(functionName, $$"""{{{await NFLDepthChart((int) year,(int) ID)}}"""));
                            
                    cont = true;
                    // add counter
                    // txtString += $"\ncityName: {cityName}\n";
                    prompt = "";
                    counter += 1;    
                }
                else if(functionName == "NFLStats"){
                    var parameters = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(functionArgs);  
                    var year = Int32.Parse(parameters["Year"].ToString());
                    var week = Int32.Parse(parameters["Week"].ToString());      
                    context.Add(new ChatRequestFunctionMessage(functionName, $$"""{{{await NFLStats((int) year, (int) week)}}"""));
                            
                    cont = true;
                    prompt = "";
                    counter += 1;    
                }
            }
                
            
        }while(cont);
    };
    
}


static async Task Weather()
    {
        Console.WriteLine("Enter the city name to get the weather:");
        string cityName = Console.ReadLine();

        // string apiKey = "ea0f10e33a4f37d8fb4a028c82ac141e";
        // string apiUrl = $"http://api.openweathermap.org/data/2.5/weather?q={cityName}&appid={apiKey}&units=metric";
        
        try
        {
            string weatherData = await GetWeatherData(cityName);

            // Parse and display weather information
            Console.WriteLine("Weather Information:");
            Console.WriteLine(weatherData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    static async Task<string> GetWeatherData(object cityName)
    {
        using (HttpClient client = new HttpClient())
        
        {
            string apiKey = "ea0f10e33a4f37d8fb4a028c82ac141e";
            string apiUrl = $"http://api.openweathermap.org/data/2.5/weather?q={cityName}&appid={apiKey}&units=metric";
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception($"Failed to fetch weather data. Status Code: {response.StatusCode}");
            }
        }
    }

    

    static async Task<string> quote(){
        string apiUrl = "https://api.quotable.io/random";

        using (HttpClient client = new HttpClient()){
        
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if(response.IsSuccessStatusCode){
                string responseData = await response.Content.ReadAsStringAsync();
                return responseData;
            }
            else{
                throw new Exception($"Failed to fetch quote data. Status Code: {response.StatusCode}");
            }
            
        }

    }

    static async Task<string> joke()
    {
        string apiUrl = "https://api.chucknorris.io/jokes/random";

        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                return responseData;
            }
            else
            {
                throw new Exception($"Failed to fetch joke data. Status Code: {response.StatusCode}");
            }
        }
    }

    static async Task<string> NFLSchedule(int year, int week)
    {
        string apiUrl = $"https://cdn.espn.com/core/nfl/schedule?xhr=1&year={year}&week={week}";

        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                var json = System.Text.Json.Nodes.JsonObject.Parse(responseData);
                return json["content"].ToJsonString();
            }
            else
            {
                throw new Exception($"Failed to fetch NFL data. Status Code: {response.StatusCode}");
            }
        }
    }

    
    static async Task<string> NFLDepthChart(int year, int team_id) 
    {
        string apiUrl = $"https://sports.core.api.espn.com/v2/sports/football/leagues/nfl/seasons/{year}/teams/{team_id}/depthcharts";

        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                var json = System.Text.Json.Nodes.JsonObject.Parse(responseData);
                return json["content"].ToJsonString();
            }
            else
            {
                throw new Exception($"Failed to fetch NFL data. Status Code: {response.StatusCode}");
            }
        }
    }

    static async Task<string> NFLStats(int year, int week) 
    {
        string apiUrl = $"https://sports.core.api.espn.com/v2/sports/football/leagues/nfl/seasons/{year}/types/2/weeks/{week}/qbr/10000";

        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                var json = System.Text.Json.Nodes.JsonObject.Parse(responseData);
                return json["content"].ToJsonString();
            }
            else
            {
                throw new Exception($"Failed to fetch NFL data. Status Code: {response.StatusCode}");
            }
        }
    }
    

    









// --------------------------------------------------------------------------------------------------------

// Above here is all the functions created to be used in the future

// Below here is everying being called


// This creates two progress bar lines
await AnsiConsole.Progress()
        .StartAsync(async ctx =>{
            var task1 = ctx.AddTask("[blue]Computer Startup[/]");
            var task2 = ctx.AddTask("[blue]Motivation to work[/]");
            

            while(!ctx.IsFinished){
                await Task.Delay(0);

                task1.Increment(1.5);
                task2.Increment(0.5);
            }
        });



// This creates to simatanious load up sequencens

AnsiConsole.Status()
    .Start("One second...", ctx => 
    {
        // Update the status and spinner
        ctx.Status("One second...");
        ctx.Spinner(Spinner.Known.Christmas);
        ctx.SpinnerStyle(Style.Parse("green"));
        
        // Simulate some work
        AnsiConsole.MarkupLine("Getting some coffee...");
        Thread.Sleep(0);
        
       

        // Simulate some work
        AnsiConsole.MarkupLine("Stretching...");
        Thread.Sleep(0);
    });




// This is where you can use all the programs created above

AnsiConsole.MarkupLine("[yellow]Welcome, I am your chatbot![/]");
var cts = new CancellationTokenSource();
while(true){
    var prompt = await(new TextPrompt<string>("> ")).ShowAsync(AnsiConsole.Console,cts.Token);
    await AnsiConsole.Live((new Panel(new Markup("Hello"))).Expand().Header("Response", Justify.Center)).StartAsync(updateLayoutAsync(prompt));
}

