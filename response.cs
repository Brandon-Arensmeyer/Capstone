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


var openAI = new OpenAIClient("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");


async void simpleResponse(){
    var cts = new CancellationTokenSource();
    while(true){
        var prompt = await(new TextPrompt<string>("> ")).ShowAsync(AnsiConsole.Console,cts.Token);
        await AnsiConsole.Live((new Panel(new Markup("Hello"))).Expand().Header("Response", Justify.Center)).StartAsync(updateLayoutAsync(prompt));
    }

    // var table = new Table().Centered(); 
    // table.AddColumn("[blue]question[/]");
    // table.AddColumn("[yellow]answer[/]");
    // var finish = false;
    // var content = new List<ChatRequestMessage>();
    // // This will loop until they say quit
    // while(finish == false){

    //     await AnsiConsole.Live(new Panel(new Markup("Hello!"))).Expand().Header("Response", Justify.Center)).StartAsync(updateLayoutAsync(prompt));
    //     AnsiConsole.MarkupLine("[green]What whould you like to ask me? (Type quit to end)[/]");
    //     var userInput = Console.ReadLine();
        
    //     if(userInput != "quit"){
            
    //         var message = getResponse(content, userInput);
    //         AnsiConsole.MarkupLineInterpolated($"[Blue]{message}[/]");     
    //     }
    //     else{
    //         AnsiConsole.MarkupLine("[magenta]Come back when you actually have a good question.[/] :angry_face:");
    //         finish = true;
    //     }
    // }
}


Task<StreamingResponse<StreamingChatCompletionsUpdate>> getCompletion(List<ChatRequestMessage> context,String prompt) {
    var options = new ChatCompletionsOptions();
    options.DeploymentName = "gpt-3.5-turbo";
    if(context.Count == 0){
        context.Add(new ChatRequestUserMessage("You are an AI."));
    }
    context.Add(new ChatRequestUserMessage(prompt));
    foreach(var m in context){
        options.Messages.Add(m);
    }
    return openAI.GetChatCompletionsStreamingAsync(options);
}

Func<LiveDisplayContext, Task> updateLayoutAsync(string prompt){
    var context = new List<ChatRequestMessage>();
    return async ldc =>{
        var txtString = " ";
        var updates = await getCompletion(context, prompt);
        await foreach(var chunk in updates){
            txtString += chunk.ContentUpdate;
            ldc.UpdateTarget(new Panel(new Markup(txtString)).Expand().Header("Response", Justify.Center));
            ldc.Refresh();
        }
        context.Add(new ChatRequestAssistantMessage(txtString));
    };
    
}

void changeColor(){
    AnsiConsole.Ask<String>("[green] Enter string here [/]?");
}




// --------------------------------------------------------------------------------------------------------

// Above here is all the classes created to be used in the future

// Below here is everying being called


// This creates two progress bar lines
await AnsiConsole.Progress()
        .StartAsync(async ctx =>{
            var task1 = ctx.AddTask("[green]Computer Startup[/]");
            var task2 = ctx.AddTask("[green]Motivation to work[/]");
            

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

// var quit = false;
// while(quit == false){
//     var task = AnsiConsole.Prompt(
//     new SelectionPrompt<string>()
//         .Title("What would you like to see?")
//         .PageSize(10)
//         .MoreChoicesText("[grey](What would you like to see?)[/]")
//         .AddChoices(new[] {
//             "Simple Response","Quit"
//         }));

//     if(task == "Simple Response"){
//         simpleResponse();
        
        
//     }
//     else if(task == "Quit"){
//         AnsiConsole.MarkupLine("[yellow]Goodbye![/]");
//         quit = true;
//     }
// }

