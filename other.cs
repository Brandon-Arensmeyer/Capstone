// void simpleTable(){
//     var table = new Table().Centered();
//     var done = false;

//     table.AddColumn("[blue]name[/]");
//     table.AddColumn("[yellow]age[/]");
    
//     // This will loop making all the rows for the names and ages
//     while(done != true){
//         AnsiConsole.MarkupLine("[blue]What is the name?[/]");
//         string? nameInput = Console.ReadLine();
//         AnsiConsole.MarkupLine("[blue]How old are they?[/]");
//         string? ageInput = Console.ReadLine();
//         table.AddRow(new Markup(nameInput), new Markup(ageInput));

//         // This allows the user to say yes or no only
//         if(AnsiConsole.Confirm("Is that all?")){
//            done = true; 
//         }
//     }
//     AnsiConsole.Write(table);

// }







// // ask for names and things about you then puts it into a live graph

// void LiveNameTable(){
//     var table = new Table().Centered();

//     var done = false;
//     var names = new List<string>();
//     var ages = new List<string>();
//     var sports = new List<string>();
//     var desserts = new List<string>();
//     var sportList = new List<string>();
//     sportList.Add("Football");
//     sportList.Add("Baseball");
//     sportList.Add("Basketball");
//     sportList.Add("Other");
    
//     var sportPrompt = new TextPrompt<string>("[blue]What is you favorite sport?[/]");
//     foreach(var sport in sportList){
//         sportPrompt.AddChoice(sport);
//     }
    
//     var dessertList = new List<string>();
//     dessertList.Add("Cake");
//     dessertList.Add("Ice Cream");
//     dessertList.Add("Cupcake");
//     dessertList.Add("Other");

//     var dessertPrompt = new TextPrompt<string>("[blue]What is you favorite dessert?[/]");
//     foreach(var dessert in dessertList){
//         dessertPrompt.AddChoice(dessert);
//     }

//     // This asks for information from the user

//     while(done == false){
//         var nameInput = AnsiConsole.Ask<string>("[blue]What's your name?[/]");
//         names.Add(nameInput);
//         var ageInput = AnsiConsole.Ask<string>("[blue]How old are you?[/]");
//         ages.Add(ageInput);
//         var sportInput = AnsiConsole.Prompt(sportPrompt);
//         sports.Add(sportInput);
//         var dessertInput = AnsiConsole.Prompt(dessertPrompt);
//         desserts.Add(dessertInput);
//         if(AnsiConsole.Confirm("Is that all?")){
//             done = true;
//         }
//     }

//     // This is where the table is created

//     AnsiConsole.Live(table)
//         .Start(ctx => 
//         {
//             table.AddColumn("Name");
//             ctx.Refresh();
//             Thread.Sleep(1000);

//             table.AddColumn("Age");
//             ctx.Refresh();
//             Thread.Sleep(1000);

//             table.AddColumn("Sport");
//             ctx.Refresh();
//             Thread.Sleep(1000);

//             table.AddColumn("Dessert");
//             ctx.Refresh();
//             Thread.Sleep(1000);

//             for(int run = 0; run < names.Count; run++){
//                 table.AddRow(new Markup(names[run]), new Markup(ages[run]), new Markup(sports[run]), new Markup(desserts[run]));
//                 ctx.Refresh();
//                 Thread.Sleep(1000);
//             }
//         });
// }



// void TimesTable(){
//     var table = new Table().Centered();

//     // This asks for information from the user
//     var number = AnsiConsole.Ask<int>("[blue]Whats the number you want to see multiplied?[/]");
//     var length = AnsiConsole.Ask<int>($"[blue]How many times do you want to multiply {number} by?[/]");


//     int x = 1;
//     // This is where the table is created

//     AnsiConsole.Live(table)
//         .Start(ctx => 
//         {
//             table.AddColumn("Number");
//             ctx.Refresh();
//             Thread.Sleep(100);

//             table.AddColumn("Answer");
//             ctx.Refresh();
//             Thread.Sleep(100);

//             for(var i = 0; i < length; i++){
//                 table.AddRow($"{number} * {x}", $"{number * x}");
//                 ctx.Refresh();
//                 Thread.Sleep(100);
//                 x++;
//             }

//         });
// }


// void BreakdownChart(){
//     Random rnd = new Random();
//     AnsiConsole.Write(new BreakdownChart()
//     .FullSize()
//     .ShowPercentage()
//     // Add item is in the order of label, value, then color.
//     .AddItem("Python", rnd.Next(1, 25), Color.Red)
//     .AddItem("HTML", rnd.Next(1, 25), Color.Blue)
//     .AddItem("C#", rnd.Next(1, 25), Color.Green)
//     .AddItem("JavaScript", rnd.Next(1, 25), Color.Yellow)
//     .AddItem("Java", rnd.Next(1, 25), Color.LightGreen)
//     .AddItem("Shell", rnd.Next(1, 25), Color.Aqua));
// }



// // Renders a FIGlet

// void Figlet(){

//     AnsiConsole.Write(
//     new FigletText("Packers are the greatest team")
//         .LeftJustified()
//         .Color(Color.Green));
// }
