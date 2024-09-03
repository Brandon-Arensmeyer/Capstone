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
using static System.Net.Mime.MediaTypeNames;




//Creating an instance of the OpenAiClient using and API key.
var openAI = new OpenAIClient("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"); //Enter you api key here

//Method to read an embedded resource file from the assembly
string readResource (string resourceName) {
    foreach (var name in System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames()) {
        if (name.EndsWith(resourceName)) {
            using var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
    throw new ArgumentException($"Can not find {resourceName} or is suffix in the embedded resources,\n   did you make sure the file has been marked as Embedded resource in the Build Action drop down in the project file", nameof(resourceName));
}

// Reading a system formatting promt from an embedded resource file
var systemFormatingPrompt = readResource("SpectreConsoleFormattingSystemPrompt.md");


//Method to create a BinaryData object from paramets
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

//Method to get a streaming completion from the OpenAI API
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

    // Adding the system formmating prompt to the context if it's empty
    if(context.Count == 0){
        context.Add(new ChatRequestSystemMessage($"{systemFormatingPrompt}\n You are an AI. "));
    }

    //Adding the user prompt to the context if it's not empty
    if(!String.IsNullOrWhiteSpace(prompt)){
        context.Add(new ChatRequestUserMessage(prompt));
    }
    
    //Adding all messages in the context to the options.
    foreach(var m in context){
        options.Messages.Add(m);
    }

    // Getting the streaming chat completions from the OpenAI API
    return openAI.GetChatCompletionsStreamingAsync(options);
}

Task<Response<ChatCompletions>> getCompletion(List<ChatRequestMessage> context,String prompt) {
    //Create ChatCompletionsOptions and set deployment name
    var options = new ChatCompletionsOptions();
    options.DeploymentName = "gpt-4-1106-preview";

    //Add function tool definitions for different types of requests
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

    // If the context is empty, add a system message
    if(context.Count == 0){
        context.Add(new ChatRequestSystemMessage($"{systemFormatingPrompt}\n You are an AI. "));
    }

    // If the prompt is not empty or whitespace, add a user message
    if(!String.IsNullOrWhiteSpace(prompt)){
        context.Add(new ChatRequestUserMessage(prompt));

    }
    
    //Add message from the context to the options
    foreach(var m in context){
        options.Messages.Add(m);
    }

    //Return the completion result from the OpenAI client
    return openAI.GetChatCompletionsAsync(options);
}

Func<LiveDisplayContext, Task> updateLayoutAsync(string prompt){
    var context = new List<ChatRequestMessage>();
    return async ldc =>{
        var txtString = " ";

        //Function to render the text string to the display
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

            // Get streaming completion updates from the OpenAI client
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
                // If there's no function call, add the assistant's response to the context
                context.Add(new ChatRequestAssistantMessage(txtString));
                cont = false;
            }
            else{
                // Handle specific function calls based on the functionName
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

Action<string,bool> writeText = (txt,finished) => { };

async Task processPromptAsync(List<ChatRequestMessage> context, string prompt) {
    var txtString = " ";
    // Function to render the text string
    void render(bool finished) {
        writeText(txtString,finished);
    }

    var cont = false;
    var counter = 0;
    do {
        ChatRole? role = null;
        string? functionName = null;
        string? toolCallId = null;
        string functionArgs = "";


// Get streaming completion updates from the OpenAI client
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
                    render(false);
                    break;
            }

        }
        render(true);
        if (String.IsNullOrEmpty(functionName)) {
            //If there's no function call, add the assistant's response to the context
            context.Add(new ChatRequestAssistantMessage(txtString));
            cont = false;
        } else {
            //Handle specific function calls based on the functionName
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

// Function to process a given prompt within a context of chat messages
async Task<string> processPrompt(List<ChatRequestMessage> context, String prompt){
        var txtString = ""; //Initizalizes an empty string to hold the response text
        var cont = false;   // Flag to indicatate wheter to continue to loop
        var counter = 0;    // Counter to track the number of function calls
        do{
            ChatRole? role = null; // Variable to hold the role of the message sender
            string? functionName = null; // Variable to hold the name of the function to call
            string? toolCallId = null; // Variable to hold the ID of the tool call
            string functionArgs = ""; // Variable to hold the arguments of the function call

            // Get completion results from the AI service

            var updates = await getCompletion(context, prompt);
            var message = updates.Value.Choices[0].Message; // Extract the message from the completion result

            
            // Determine the role of the message
            role = message.Role;    
            switch (role){
                case var r when r == ChatRole.Assistant:
                    if (message.ToolCalls.Count == 1 && message.ToolCalls[0] is ChatCompletionsFunctionToolCall ftc) {
                        // If the message contains a tool call, extract the function name and arguments
                        toolCallId = ftc.Id;
                        functionName = ftc.Name;
                        
                        functionArgs = ftc.Arguments;
                        txtString += $"{functionName}\n{functionArgs}"; // Append function name and arguments to the response text
                    }else{
                        // Otherwise, append the message content to the response text
                        txtString += message.Content;

                    }
                    break;
                
            }
                
            if(String.IsNullOrEmpty(functionName)){
                // If no function is to be called, add the assistant's message to the context
                context.Add(new ChatRequestAssistantMessage(message.Content));
                cont = false; // Set the flag to stop the loop
            }
            else{
                // Handle specific function calls based on the functionName
                if(functionName == "getQuote"){
                    // Deserialize function arguments to extract the quote type
                    var parameters = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(functionArgs);
                    var cityName = parameters["quote"];
                    while(counter == 0){
                        // Add the function call result to the context and set continuation flags
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
    return txtString; // Return the final response text
}

// Function to get weather data based on user input
static async Task Weather()
    {
        Console.WriteLine("Enter the city name to get the weather:");
        string cityName = Console.ReadLine();

        // string apiKey = "ea0f10e33a4f37d8fb4a028c82ac141e";
        // string apiUrl = $"http://api.openweathermap.org/data/2.5/weather?q={cityName}&appid={apiKey}&units=metric";
        
        try
        {
            // Fetch weather data for the given city name
            string weatherData = await GetWeatherData(cityName);

            // Parse and display weather information
            Console.WriteLine("Weather Information:");
            Console.WriteLine(weatherData);
        }
        catch (Exception ex)
        {
            // Handle errors by displaying an error message
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    // Function to fetch weather data from an API
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

    
    // Function to fetch a random quote from an API
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

    // Function to fetch a random joke from an API
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

    // Function to fetch the NFL schedule for a specific year and week from an API
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

    // Function to fetch the NFL roster for a specific team from an API
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

    // Function to fetch the NFL record for a specific team from an API
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

    // Function to parse player names from JSON response
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

// This section simulates two simultaneous loading sequences with progress indicators.
// This creates two progress bar lines
await AnsiConsole.Progress()
        .StartAsync(async ctx =>{
            // Add tasks to the progress context with labels
            var task1 = ctx.AddTask("[blue]Computer Startup[/]");
            var task2 = ctx.AddTask("[blue]Motivation to work[/]");
            
            // Continuously update the progress of the tasks until they are finished
            while(!ctx.IsFinished){
                await Task.Delay(0);
                // Increment the progress of the tasks
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
    // List to keep track of text history
    List<string> textHistory = new List<string>();
    // Configure middleware and routes
    app
        .UseDeveloperExceptionPage()
        .UseHttpsRedirection()
        .UseDefaultFiles()
        .UseStaticFiles()
        .UseRouting()
        .UseWebSockets();

    var counter = 0;
    var runCounter = false;

    // WebSocket processing function
    async Task ProcessWs(WebSocket webSocket){
        var buffer = new byte[1024 * 4];
        var rcvBuffer = new byte[1024 * 4];
        var contRcv = true;

        var oldWriteText = writeText;
        writeText = (txt,finished) => {
            textHistory.Add(txt);
            string[] messages = null;
            if (!finished) {
                messages = new[] { $"<div id=\"response\" hx-swap-oob=\"innerHTML\">{txt}</div>" };
            } else {
                //message = $"<div><div id=\"response\" hx-swap-oob=\"innerHTML\"></div><p hx-select-oob=\"#response\" hx-swap-oob=\"beforebegin\">{txt}</p></div>";
                //message = $"<div id=\"response\" hx-swap-oob=\"innerHTML\"></div>";
                messages = new[] {
                        $"<div id=\"response\" hx-swap-oob=\"innerHTML\"/>",
                        $"<div hx-swap-oob=\"beforeend:#history\"><p id='reply'>{txt}</p></div>"
                };
                //messages = new[] { $"<p id=\"response\" hx-swap-oob=\"beforebegin\">{txt}</p>" };
            }
            if (webSocket.CloseStatus.HasValue) {
                Console.WriteLine($"Ending websocket connection {webSocket.CloseStatus.Value}...");
            } else {
                foreach (var message in messages) {
                    var len = Encoding.UTF8.GetBytes(message, 0, message.Length, buffer, 0);
                    webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, len), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
            oldWriteText(txt,finished);
        };

        // Loop to receive messages from the websocket
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
        // Loop to send counter updates through the websocket
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
    
    // Define routes and their handlers
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
    var context = new List<ChatRequestMessage>();
    app.MapPost("/prompt", async (Dictionary<String, Object> o)=>{
        Console.WriteLine($"{o["Work"]}");
        return await processPrompt(context, o["Work"].ToString());
    });
    app.MapPost("/prompt2", async (Dictionary<String, Object> o) => {
        Console.WriteLine($"{o["Work"]}");
        processPromptAsync(context, o["Work"].ToString());
        return Microsoft.AspNetCore.Http.Results.Content("<input hx-post = \"/prompt2\" hx-ext = 'json-enc' hx-swap = \"outerHTML\" hx-target = \"this\" name = \"Work\"/>");
        //return Microsoft.AspNetCore.Http.Results.NoContent();
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
record Message(string role, string content);
// This is for the console if you want to run it as a console
// AnsiConsole.MarkupLine("[yellow]Welcome, I am your chatbot![/]");
// var cts = new CancellationTokenSource();
// while(true){
//     var prompt = await(new TextPrompt<string>("> ")).ShowAsync(AnsiConsole.Console,cts.Token);
//     await AnsiConsole.Live((new Panel(new Markup("Hello"))).Expand().Header("Response", Justify.Center)).StartAsync(updateLayoutAsync(prompt));
// }

