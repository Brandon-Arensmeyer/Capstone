using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Quic;
using System.Runtime.InteropServices;
using Spectre.Console;



void simpleResponse(){
    var finish = false;
    // This will loop until they say quit
    while(finish == false){
        AnsiConsole.MarkupLine("[green]What whould you like to ask me? (Type quit to end)[/]");
        var userInput = Console.ReadLine();
        if(userInput != "quit"){
            AnsiConsole.MarkupLine("[red]Thats a dumb question! Ask something else![/]");
        }
        else{
            AnsiConsole.MarkupLine("[magenta]Come back when you actually have a good question.[/] [maroon]>:C[/]");
            finish = true;
        }
    }
}

// This lets you put in names and their age
void simpleTable(){
    var table = new Table().Centered();
    var done = false;

    table.AddColumn("[blue]name[/]");
    table.AddColumn("[yellow]age[/]");
    
    // This will loop making all the rows for the names and ages
    while(done != true){
        AnsiConsole.MarkupLine("[blue]What is the name?[/]");
        string? nameInput = Console.ReadLine();
        AnsiConsole.MarkupLine("[blue]How old are they?[/]");
        string? ageInput = Console.ReadLine();
        table.AddRow(new Markup(nameInput), new Markup(ageInput));

        // This allows the user to say yes or no only
        if(AnsiConsole.Confirm("Is that all?")){
           done = true; 
        }
    }
    AnsiConsole.Write(table);

}

void LiveNameTable(){
    var table = new Table().Centered();

    var done = false;
    var names = new List<string>();
    var ages = new List<string>();
    var sports = new List<string>();
    var desserts = new List<string>();
    var sportList = new List<string>();
    sportList.Add("Football");
    sportList.Add("Baseball");
    sportList.Add("Basketball");
    sportList.Add("Other");
    
    var sportPrompt = new TextPrompt<string>("[blue]What is you favorite sport?[/]");
    foreach(var sport in sportList){
        sportPrompt.AddChoice(sport);
    }
    
    var dessertList = new List<string>();
    dessertList.Add("Vanilla");
    dessertList.Add("Chocolate");
    dessertList.Add("Strawberry");
    dessertList.Add("Other");

    var dessertPrompt = new TextPrompt<string>("[blue]What is you favorite dessert?[/]");
    foreach(var dessert in dessertList){
        dessertPrompt.AddChoice(dessert);
    }


    while(done == false){
        var nameInput = AnsiConsole.Ask<string>("[blue]What's your name?[/]");
        names.Add(nameInput);
        var ageInput = AnsiConsole.Ask<string>("[blue]How old are you?[/]");
        ages.Add(ageInput);
        var sportInput = AnsiConsole.Prompt(sportPrompt);
        sports.Add(sportInput);
        var dessertInput = AnsiConsole.Prompt(dessertPrompt);
        desserts.Add(dessertInput);
        if(AnsiConsole.Confirm("Is that all?")){
            done = true;
        }
    }

    AnsiConsole.Live(table)
        .Start(ctx => 
        {
            table.AddColumn("Name");
            ctx.Refresh();
            Thread.Sleep(1000);

            table.AddColumn("Age");
            ctx.Refresh();
            Thread.Sleep(1000);

            table.AddColumn("Sport");
            ctx.Refresh();
            Thread.Sleep(1000);

            table.AddColumn("Dessert");
            ctx.Refresh();
            Thread.Sleep(1000);

            for(int run = 0; run < names.Count; run++){
                table.AddRow(new Markup(names[run]), new Markup(ages[run]), new Markup(sports[run]), new Markup(desserts[run]));
                ctx.Refresh();
                Thread.Sleep(1000);
            }
        });
}

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



AnsiConsole.MarkupLine("[yellow]Welcome, I am your chatbot![/]");
var quit = false;
while(quit == false){
    var task = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("What would you like to see?")
        .PageSize(10)
        .MoreChoicesText("[grey](What would you like to see?)[/]")
        .AddChoices(new[] {
            "Simple Response", "Simple Name Table", "Live Name Table", "Quit"
        }));

    if(task == "Simple Response"){
        simpleResponse();
    }
    else if(task == "Name Table"){
        simpleTable();
    }
    else if(task == "Live Name Table"){
        LiveNameTable();
    }
    else if(task == "Quit"){
        AnsiConsole.MarkupLine("[yellow]Goodbye![/]");
        quit = true;
    }
}

