using System;
using Spectre.Console;

AnsiConsole.MarkupLine("[red]Hello [green]World[/]![/]");

var table = new Table();
table.AddColumn("[blue]name[/]");
table.AddColumn("[yellow]age[/]");

table.AddRow(new Markup("Brandon"), new Markup("22"));
AnsiConsole.Write(table);

void main(string[] args) {
        Console.WriteLine("Welcome, what whould you like to ask me?");
        var userInput = Console.ReadLine();
        
        
        var finish = false;

        while(finish == false){
            Console.WriteLine(getChatbotResponse(userInput));
            // String userInput = question.nextLine();
            // if(userInput.equals("hello")){
            //     System.out.println("Hello, how are you?");
            // }
            // else if(userInput.equals("exit")){H
            //     System.out.println("Thanks for talking with me. :)")
            //     finish = true;
            // }
            // else{
            //     System.out.println("What?");
            // }
            // System.out.println("Is there anything else you would like to ask?");h
            finish = true;
        }
        
    }

string getChatbotResponse(string? userMessage) {
        

        return $"Hello! I am your chatbot. You said: {userMessage}";
    }

var root = new Tree("Root");

// Add some nodes
var foo = root.AddNode("[yellow]Foo[/]");
var tables = foo.AddNode(new Table()
    .RoundedBorder()
    .AddColumn("First")
    .AddColumn("Second")
    .AddRow("1", "2")
    .AddRow("3", "4")
    .AddRow("5", "6"));

tables.AddNode("[blue]Baz[/]");
foo.AddNode("Qux");

var bar = root.AddNode("[yellow]Bar[/]");
bar.AddNode(new Calendar(2020, 12)
    .AddCalendarEvent(2020, 12, 12)
    .HideHeader());

// Render the tree
AnsiConsole.Write(root);

// var name = AnsiConsole.Ask<bool>("What's your [green]name[/]?");

// AnsiConsole.Prompt(
// new TextPrompt<int>("How [green]old[/] are you?")
//         .PromptStyle("green italic")
//         .ValidationErrorMessage("[red]That's not a valid age[/]")
//         .Validate(age =>
//         {
//             return age switch
//             {
//                 <= 0 => ValidationResult.Error("[red]You must at least be 1 years old[/]"),
//                 >= 123 => ValidationResult.Error("[red]You must be younger than the oldest person alive[/]"),
//                 _ => ValidationResult.Success(),
//             };
//         }));

main(Environment.GetCommandLineArgs());

// var fruits = new []{new Fruits("apple", 25), new("apricot", 10), new("Avacado", 60)};
// var fruit = AnsiConsole.Prompt(
//     new SelectionPrompt<Fruits>()
//         .Title("What's your [green]favorite fruit[/]?")
//         .PageSize(5)
//         .MoreChoicesText("[grey](Move up and down to reveal more fruits)[/]")
//         .AddChoices(fruits)
//         .UseConverter(x => x.name));

// // Echo the fruit back to the terminal
// AnsiConsole.WriteLine($"I agree. {fruit} is tasty!");

// record Fruits(string name, decimal calories);


var fruits = AnsiConsole.Prompt(
    new MultiSelectionPrompt<string>()
        .Title("What are your [green]favorite fruits[/]?")
        .NotRequired() // Not required to have a favorite fruit
        .PageSize(10)
        .MoreChoicesText("[grey](Move up and down to reveal more fruits)[/]")
        .InstructionsText(
            "[grey](Press [blue]<space>[/] to toggle a fruit, " + 
            "[green]<enter>[/] to accept)[/]")
        .AddChoices(new[] {
            "Apple", "Apricot", "Avocado",
            "Banana", "Blackcurrant", "Blueberry",
            "Cherry", "Cloudberry", "Coconut",
        }));

// Write the selected fruits to the terminal
foreach (string fruit in fruits)
{
    AnsiConsole.WriteLine(fruit);
}