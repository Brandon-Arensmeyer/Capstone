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
using System.Threading.Tasks;

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
                Description = "You will recive the weather and the JSON document the describes the clouds, temperature, humidity, etc...",
                Parameters = makeParameters(
                    ("cityName", "string", "name of the city we want the weather for")
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
                var parameters = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(functionArgs);
                var cityName = parameters["cityName"];
                // do the await call to GetWeatherData(cityName)
                context.Add(new ChatRequestFunctionMessage(functionName, $$"""Weather Information:{"coord":{"lon":-0.1257,"lat":51.5085},"weather":[{"id":804,"main":"Clouds","description":"overcast clouds","icon":"04n"}],"base":"stations","main":{"temp":11.64,"feels_like":11.18,"temp_min":10.16,"temp_max":12.75,"pressure":1007,"humidity":89},"visibility":10000,"wind":{"speed":1.34,"deg":267,"gust":2.68},"clouds":{"all":99},"dt":1708042032,"sys":{"type":2,"id":2091269,"country":"GB","sunrise":1708067638,"sunset":1708103732},"timezone":0,"id":2643743,"name":"{{cityName}}","cod":200} """));
                cont = true;
                txtString += $"\ncityName: {cityName}\n";
                prompt = "";
            }
        }while(cont);
    };
    
}


static async Task Weather()
    {
        Console.WriteLine("Enter the city name to get the weather:");
        string cityName = Console.ReadLine();

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

    static async Task<string> GetWeatherData(string cityName)
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

