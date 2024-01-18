using System;
using System.Net.Quic;
using System.Runtime.InteropServices;
using Spectre.Console;

AnsiConsole.MarkupLine("[yellow]Welcome, I am your chatbot![/]");

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
    var table = new Table();
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
var quit = false;
while(quit == false){
    var task = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("What would you like to see?")
        .PageSize(10)
        .MoreChoicesText("[grey](What would you like to see?)[/]")
        .AddChoices(new[] {
            "Simple Response", "Name Table", "Quit"
        }));

    if(task == "Simple Response"){
        simpleResponse();
    }
    else if(task == "Name Table"){
        simpleTable();
    }
    else if(task == "Quit"){
        Console.WriteLine("Goodbye!");
        quit = true;
    }
}
