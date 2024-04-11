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
using System.Collections.Specialized;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Json.Nodes;
using System.Security.Cryptography.X509Certificates;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc;





var openAI = new OpenAIClient("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");
string readResource (string resourceName) {
    using var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
    using var reader = new StreamReader(stream);
    return reader.ReadToEnd();
}
var systemFormatingPrompt = readResource("Capstone.SpectreConsoleFormattingSystemPromptHTMX.md");

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

Task<StreamingResponse<StreamingChatCompletionsUpdate>> getStreamingCompletion(List<ChatRequestMessage> context,String prompt) {
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
                Name = "NFLRoster", 
                Description = "It gives you the players on a team durring a specific season",
                Parameters = makeParameters(
                    // ("Year", "number", "the start of the year as an integer of the NFL schedule you are asking for"),
                    ("team_name", "number", "You will be given the team name and convert it to the team id number based on the list in the system prompt. Once you get the team id, you will put that id into the url where {team_name} is located")
                )}));
    options.Tools.Add(
        new ChatCompletionsFunctionToolDefinition(
            new FunctionDefinition(){
                Name = "NFLRecord", 
                Description = "It will give you the wins, losses and ties in their season",
                Parameters = makeParameters(
                    // ("Year", "number", "the start of the year as an integer of the NFL schedule you are asking for"),
                    ("team_name", "number", "You will be given the team name and convert it to the team id number based on the list in the system prompt. Once you get the team id, you will put that id into the url where {team_name} is located")
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

Task<Response<ChatCompletions>> getCompletion(List<ChatRequestMessage> context,String prompt) {
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
                Name = "NFLRoster", 
                Description = "It gives you the players on a team durring a specific season",
                Parameters = makeParameters(
                    // ("Year", "number", "the start of the year as an integer of the NFL schedule you are asking for"),
                    ("team_name", "number", "You will be given the team name and convert it to the team id number based on the list in the system prompt. Once you get the team id, you will put that id into the url where {team_name} is located")
                )}));
    options.Tools.Add(
        new ChatCompletionsFunctionToolDefinition(
            new FunctionDefinition(){
                Name = "NFLRecord", 
                Description = "It will give you the wins, losses and ties in their season",
                Parameters = makeParameters(
                    // ("Year", "number", "the start of the year as an integer of the NFL schedule you are asking for"),
                    ("team_name", "number", "You will be given the team name and convert it to the team id number based on the list in the system prompt. Once you get the team id, you will put that id into the url where {team_name} is located")
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
    return openAI.GetChatCompletionsAsync(options);
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

            var updates = await getStreamingCompletion(context, prompt);
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
                else if(functionName == "NFLRoster"){
                    var parameters = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(functionArgs);        
                    // var year = Int32.Parse(parameters["Year"].ToString());
                    var ID = Int32.Parse(parameters["team_name"].ToString());
                    context.Add(new ChatRequestFunctionMessage(functionName, $$"""{{{await NFLRoster((int) ID)}}"""));
                            
                    cont = true;
                    // add counter
                    // txtString += $"\ncityName: {cityName}\n";
                    prompt = "";
                    counter += 1;    
                }
                else if(functionName == "NFLRecord"){
                    var parameters = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(functionArgs);  
                    // var year = Int32.Parse(parameters["Year"].ToString());
                    var team_name = parameters["team_name"].ToString();      
                    context.Add(new ChatRequestFunctionMessage(functionName, $$"""{{{await NFLRecord((string) team_name)}}"""));
                            
                    cont = true;
                    prompt = "";
                    counter += 1;    
                }
            }
                
            
        }while(cont);
    };
    
}

Action<string> writeText = txt => { };

async Task processPromptAsync(string prompt) {
    var context = new List<ChatRequestMessage>();
    var txtString = " ";
    void render() {
        writeText(txtString);
    }

    var cont = false;
    var counter = 0;
    do {
        ChatRole? role = null;
        string? functionName = null;
        string? toolCallId = null;
        string functionArgs = "";

        var updates = await getStreamingCompletion(context, prompt);
        await foreach (var chunk in updates) {
            if (chunk.Role is not null) {
                role = chunk.Role;
            }
            switch (role) {
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
        if (String.IsNullOrEmpty(functionName)) {
            context.Add(new ChatRequestAssistantMessage(txtString));
            cont = false;
        } else {



            if (functionName == "getQuote") {
                var parameters = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(functionArgs);
                var cityName = parameters["quote"];
                while (counter == 0) {
                    context.Add(new ChatRequestFunctionMessage(functionName, $$"""{{{await quote()}}"""));
                    cont = true;
                    // add counter
                    // txtString += $"\ncityName: {cityName}\n";
                    prompt = "";
                    counter += 1;
                }

            } else if (functionName == "getWeather") {
                var parameters = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(functionArgs);
                var cityName = parameters["cityName"];

                context.Add(new ChatRequestFunctionMessage(functionName, $$"""{{{await GetWeatherData(cityName)}}"""));

                cont = true;
                // add counter
                // txtString += $"\ncityName: {cityName}\n";
                prompt = "";
                counter += 1;
            } else if (functionName == "getJoke") {
                var parameters = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(functionArgs);
                var cityName = parameters["joke"];

                context.Add(new ChatRequestFunctionMessage(functionName, $$"""{{{await joke()}}"""));

                cont = true;
                // add counter
                // txtString += $"\ncityName: {cityName}\n";
                prompt = "";
                counter += 1;
            } else if (functionName == "NFLSchedule") {
                var parameters = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(functionArgs);
                var year = Int32.Parse(parameters["Year"].ToString());
                var week = Int32.Parse(parameters["Week"].ToString());

                context.Add(new ChatRequestFunctionMessage(functionName, $$"""{{{await NFLSchedule((int)year, (int)week)}}"""));

                cont = true;
                // add counter
                // txtString += $"\ncityName: {cityName}\n";
                prompt = "";
                counter += 1;
            } else if (functionName == "NFLRoster") {
                var parameters = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(functionArgs);
                // var year = Int32.Parse(parameters["Year"].ToString());
                var ID = Int32.Parse(parameters["team_name"].ToString());
                context.Add(new ChatRequestFunctionMessage(functionName, $$"""{{{await NFLRoster((int)ID)}}"""));

                cont = true;
                // add counter
                // txtString += $"\ncityName: {cityName}\n";
                prompt = "";
                counter += 1;
            } else if (functionName == "NFLRecord") {
                var parameters = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(functionArgs);
                // var year = Int32.Parse(parameters["Year"].ToString());
                var team_name = parameters["team_name"].ToString();
                context.Add(new ChatRequestFunctionMessage(functionName, $$"""{{{await NFLRecord((string)team_name)}}"""));

                cont = true;
                prompt = "";
                counter += 1;
            }
        }


    } while (cont);
}

async Task<string> processPrompt(List<ChatRequestMessage> context, String prompt){
        var txtString = "";
        var cont = false;
        var counter = 0;
        do{
            ChatRole? role = null;
            string? functionName = null;
            string? toolCallId = null;
            string functionArgs = "";

            var updates = await getCompletion(context, prompt);
            var message = updates.Value.Choices[0].Message;

            
            
            role = message.Role;    
            switch (role){
                case var r when r == ChatRole.Assistant:
                    if (message.ToolCalls.Count == 1 && message.ToolCalls[0] is ChatCompletionsFunctionToolCall ftc) {
                        toolCallId = ftc.Id;
                        functionName = ftc.Name;
                        
                        functionArgs = ftc.Arguments;
                        txtString += $"{functionName}\n{functionArgs}";
                    }else{
                        txtString += message.Content;

                    }
                    break;
                
            }
                
            if(String.IsNullOrEmpty(functionName)){
                context.Add(new ChatRequestAssistantMessage(message.Content));
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
                else if(functionName == "NFLRoster"){
                    var parameters = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(functionArgs);        
                    // var year = Int32.Parse(parameters["Year"].ToString());
                    var ID = Int32.Parse(parameters["team_name"].ToString());
                    context.Add(new ChatRequestFunctionMessage(functionName, $$"""{{{await NFLRoster((int) ID)}}"""));
                            
                    cont = true;
                    // add counter
                    // txtString += $"\ncityName: {cityName}\n";
                    prompt = "";
                    counter += 1;    
                }
                else if(functionName == "NFLRecord"){
                    var parameters = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(functionArgs);  
                    // var year = Int32.Parse(parameters["Year"].ToString());
                    var team_name = parameters["team_name"].ToString();      
                    context.Add(new ChatRequestFunctionMessage(functionName, $$"""{{{await NFLRecord((string) team_name)}}"""));
                            
                    cont = true;
                    prompt = "";
                    counter += 1;    
                }
            }
                
            
        }while(cont);
    return txtString;
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

    
    static async Task<string> NFLRoster(int team_name) 
    {
        // string apiUrl = $"https://site.api.espn.com/apis/site/v2/sports/football/nfl/teams/{team_id}/roster";
        string apiUrl = $"https://site.api.espn.com/apis/site/v2/sports/football/nfl/teams/{team_name}?enable=roster,projection,stats";

        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                
                string responseData = await response.Content.ReadAsStringAsync();
                string combinedString = string.Join(",", ParsePlayerNames(responseData).ToArray());
                return combinedString;
                // var json = System.Text.Json.Nodes.JsonObject.Parse(responseData);
                
                // return json["team"]["athletes"].ToJsonString();
            }
            else
            {
                throw new Exception($"Failed to fetch NFL data. Status Code: {response.StatusCode}");
            }
        }
    }

    static async Task<string> NFLRecord(string team_name) 
    {
        string apiUrl = $"https://site.api.espn.com/apis/site/v2/sports/football/nfl/teams/{team_name}?enable=roster,projection,stats";

        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                var json = System.Text.Json.Nodes.JsonObject.Parse(responseData);
                return json["team"]["record"].ToJsonString();
            }
            else
            {
                throw new Exception($"Failed to fetch NFL data. Status Code: {response.StatusCode}. URL: {apiUrl}");
            }
        }
    }


    static List<(string, string)> ParsePlayerNames(string json)
    {
        List<(string, string)> playerNames = new List<(string, string)>();

        // Parse the JSON
        using (JsonDocument doc = JsonDocument.Parse(json))
        {
            JsonElement root = doc.RootElement;

            // Get the "team" object
            JsonElement team = root.GetProperty("team");

            // Get the "athletes" array
            JsonElement athletes = team.GetProperty("athletes");

            // JsonElement position = athletes.GetProperty("position");

            // Iterate over each athlete
            foreach (JsonElement athlete in athletes.EnumerateArray())
            {
                // Get the first and last names
                string name = athlete.GetProperty("fullName").GetString();
                string pos = athlete.GetProperty("position").GetProperty("displayName").GetString();

                // Add the first and last names to the list
                playerNames.Add((name, pos));
            }
            
        }
        return playerNames;

        // Return the list of player names
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

// AnsiConsole.Status()
//     .Start("One second...", ctx => 
//     {
//         // Update the status and spinner
//         ctx.Status("One second...");
//         ctx.Spinner(Spinner.Known.Christmas);
//         ctx.SpinnerStyle(Style.Parse("green"));
        
//         // Simulate some work
//         AnsiConsole.MarkupLine("Getting some coffee...");
//         Thread.Sleep(0);
        
       

//         // Simulate some work
//         AnsiConsole.MarkupLine("Stretching...");
//         Thread.Sleep(0);
//     });


void startup(){
    var builder = WebApplication.CreateBuilder(Environment.GetCommandLineArgs());
    var app = builder.Build();
    app
        .UseDeveloperExceptionPage()
        .UseHttpsRedirection()
        .UseDefaultFiles()
        .UseStaticFiles()
        .UseRouting()
        .UseWebSockets();

    var counter = 0;
    var runCounter = false;

    async Task ProcessWs(WebSocket webSocket){
        var buffer = new byte[1024 * 4];
        var rcvBuffer = new byte[1024 * 4];
        var contRcv = true;

        var oldWriteText = writeText;
        writeText = txt => {
            var message = $"<div id=\"response\" hx-swap-oob=\"innerHTML\">{System.Web.HttpUtility.HtmlEncode(txt)}</div>";
            if (webSocket.CloseStatus.HasValue) {
                Console.WriteLine("Ending websocket connection ...");
            } else {
                var len = Encoding.UTF8.GetBytes(message, 0, message.Length, buffer, 0);
                webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, len), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            oldWriteText(txt);
        };

        async Task RcLoop(){
            while(contRcv){
                Console.WriteLine("rcvLoop waiting");
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(rcvBuffer), CancellationToken.None);
                Console.WriteLine($@"Ws:Close Status: {result.CloseStatus}
                                    Type: {result.MessageType}
                                    Count:{result.Count}
                                    EndOfMessage: {result.EndOfMessage}
                                    Content: {Encoding.UTF8.GetString(rcvBuffer, 0, result.Count)}");
            }
        }
        async Task Loop(){
            while (true) {
                if (runCounter) {
                    var message = $"<div id=\"counter\" hx-swap-oob=\"true\">{counter}</div>";
                    if (webSocket.CloseStatus.HasValue) {
                        Console.WriteLine("Ending websocket connection ...");
                    } else {
                        var len = Encoding.UTF8.GetBytes(message, 0, message.Length, buffer, 0);
                        await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, len), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    counter++;
                }
                await Task.Delay(1000);
            }
        }

        var rcvFinished = RcLoop();
        await Loop();
        Console.WriteLine("stopping receive message loop");
        contRcv = false;
        await rcvFinished;
    }
    
    app.MapGet("/bob", async (HttpContext context) =>
    {
        runCounter = true;
        await context.Response.WriteAsync($@"<button hx-get=""/stop"" hx-swap=""outerHTML"">Stop Counter</button>");
    });
    app.MapGet("/stop", async (HttpContext context) => {
        runCounter = false;
        await context.Response.WriteAsync($@"<button hx-get=""/bob"" hx-swap=""outerHTML"">Start Counter</button>");
    });
    app.Use(async (context, next) => {
        if (context.Request.Path == "/ws") {
            Console.WriteLine("connecting with a websocket");
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await ProcessWs(webSocket);
        } else {
            await next.Invoke();
        }
    });
    app.MapPost("/prompt", async (Dictionary<String, Object> o)=>{
        Console.WriteLine($"{o["Work"]}");
        return await processPrompt(new List<ChatRequestMessage>(), o["Work"].ToString());
    });
    app.MapPost("/prompt2", async (Dictionary<String, Object> o) => {
        Console.WriteLine($"{o["Work"]}");
        processPromptAsync(o["Work"].ToString());
        return Microsoft.AspNetCore.Http.Results.NoContent();
    });

    app.Run("https://localhost:5000");

    // app.MapGet("/bob",()=>{
    //     return """<button hx-get="/bill" hx-swap="outerHTML" >Goodbye</button>""";
    // });
    // app.MapGet("/bill",()=>{
    //     return """<p style="color:red" >Goodbye</p>""";
    // });
    
        
    // });
    // app.Run("https://localhost:5000");
}

// This is where you can use all the programs created above
startup();

// This is for the console if you want to run it as a console
// AnsiConsole.MarkupLine("[yellow]Welcome, I am your chatbot![/]");
// var cts = new CancellationTokenSource();
// while(true){
//     var prompt = await(new TextPrompt<string>("> ")).ShowAsync(AnsiConsole.Console,cts.Token);
//     await AnsiConsole.Live((new Panel(new Markup("Hello"))).Expand().Header("Response", Justify.Center)).StartAsync(updateLayoutAsync(prompt));
// }

