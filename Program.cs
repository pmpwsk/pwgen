using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;

string versionString = $"uwap.org/pwgen {VersionString(Assembly.GetExecutingAssembly())}";

bool help = false,
     verbose = false,
     encode = false,
     copy = false;
int firstArgumentIndex = 0;

//read flags
if (args.Length > 0)
{
    if (args[0].StartsWith('-'))
    {
        firstArgumentIndex = 1;
        foreach (char flag in args[0][1..])
            switch (flag)
            {
                case 'v': //verbose
                    verbose = true;
                    break;
                case 'e': //url encode
                    encode = true;
                    break;
                case 'c': //copy to clipboard if possible
                    copy = true;
                    break;
                case 'h': //help
                    help = true;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"Unrecognized flag '{flag}'!");
                    Console.ResetColor();
                    Console.WriteLine();
                    break;
            }
    }
}

if (help)
{
    Console.WriteLine(versionString);
    Console.WriteLine();

    //pwgen [length]
    PrintTwoParted(ConsoleColor.Green, "pwgen [length]", " to generate a password with lower- and uppercase letters, digits and common symbols of the given length");
    PrintTwoParted(ConsoleColor.Blue, "Example:", " pwgen 25");
    Console.WriteLine();

    //pwgen [length] [sets]
    PrintTwoParted(ConsoleColor.Green, "pwgen [length] [sets]", " to generate a password with the given length and sets");
    PrintTwoParted(ConsoleColor.Yellow, "a", " lowercase letters");
    PrintTwoParted(ConsoleColor.Yellow, "A", " uppercase letters");
    PrintTwoParted(ConsoleColor.Yellow, "1", " digits");
    PrintTwoParted(ConsoleColor.Yellow, ".", " common symbols");
    PrintThreeParted(ConsoleColor.Yellow, "+", " ASCII characters 32-126 ", ConsoleColor.DarkGray, "(exclusive)");
    PrintThreeParted(ConsoleColor.Yellow, "?", " numbers 0-65535 encoded as characters ", ConsoleColor.DarkGray, "(exclusive)");
    PrintTwoParted(ConsoleColor.Blue, "Example:", " pwgen 25 aA1.");
    PrintTwoParted(ConsoleColor.Blue, "Example:", " pwgen 25 +");
    PrintTwoParted(ConsoleColor.Blue, "Example:", " pwgen 25 ?");
    Console.WriteLine();
    
    //pwgen -[flags] [arguments]
    PrintTwoParted(ConsoleColor.Green, "pwgen -[flags] [arguments]", " to run pwgen with flags");
    PrintTwoParted(ConsoleColor.Yellow, "h", " view this help page");
    PrintTwoParted(ConsoleColor.Yellow, "c", " try to copy the result to the clipboard");
    PrintTwoParted(ConsoleColor.Yellow, "e", " URL encode the result");
    PrintTwoParted(ConsoleColor.Yellow, "v", " verbose output for debugging ");
    PrintTwoParted(ConsoleColor.Blue, "Example:", " pwgen -c 25");
    PrintTwoParted(ConsoleColor.Blue, "Example:", " pwgen -cev 25 aA1.");
}
else if (args.Length - firstArgumentIndex == 0 || args.Length - firstArgumentIndex > 2)
{
    PrintError("Invalid argument count!");
    return;
}
else
{
    //get length
    if (!uint.TryParse(args[firstArgumentIndex], out var length))
    {
        PrintError("Invalid length argument!");
        return;
    }

    //get sets
    string sets;
    if (args.Length - firstArgumentIndex >= 2)
    {
        sets = args[firstArgumentIndex + 1];
        if (sets != "+" && sets != "?" && sets.Any(x => !"aA1.".Contains(x)))
        {
            PrintError("Invalid sets argument!");
            return;
        }
    }
    else sets = "aA1.";

    //generate
    if (verbose)
    {
        Console.WriteLine(versionString);
        Console.WriteLine();
        Console.WriteLine($"Length: {length}");
        Console.WriteLine($"Sets: {sets}");
        Console.WriteLine();
    }
    GenerateAndOutput(length, sets);
}


void GenerateAndOutput(uint length, string sets)
{
    //generate result
    if (verbose)
        Console.WriteLine("Generating...");
    StringBuilder builder = new();
    for (int i = 0; i < length; i++)
        switch (sets)
        {
            case "+":
                builder.Append(Encoding.ASCII.GetString([(byte)(32 + RandomNumberGenerator.GetInt32(95))]));
                break;
            case "?":
                builder.Append((char)RandomNumberGenerator.GetInt32(char.MinValue, char.MaxValue + 1));
                break;
            default:
                builder.Append(RandomChar(sets[RandomNumberGenerator.GetInt32(sets.Length)] switch
                {
                    'a' => "abcdefghijklmnopqrstuvwxyz",
                    'A' => "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
                    '1' => "0123456789",
                    '.' => "^!$%/()=?+#-.,;:~*@[]{}_°§&",
                    _ => ""
                }));
                break;
        }
    string result = builder.ToString();

    //encode
    if (encode)
    {
        if (verbose)
            Console.WriteLine("URL encoding...");

        result = HttpUtility.UrlEncode(result);
    }

    //copy
    if (copy)
        try
        {
            if (verbose)
                Console.WriteLine("Copying to clipboard...");

            TextCopy.ClipboardService.SetText(result);
        }
        catch (Exception ex)
        {
            if (verbose)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to copy the result to the clipboard. If you're on Linux, make sure xsel is installed.");
                Console.ResetColor();
                PrintTwoParted(ConsoleColor.Yellow, "Exception: ", ex.Message);
            }
        }

    //output
    if (verbose)
    {
        Console.WriteLine();
        Console.Write("Result: ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(result);
        Console.ResetColor();
    }
    else Console.WriteLine(result);
}

static char RandomChar(string characters)
    => characters[RandomNumberGenerator.GetInt32(characters.Length)];

void PrintError(string message)
{
    Console.WriteLine(versionString);
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Write($"{message} Run pwgen -h for help.");
    Console.ResetColor();
    Console.WriteLine();
}

static void PrintTwoParted(ConsoleColor color, string partWithColor, string partWithoutColor)
{
    Console.ForegroundColor = color;
    Console.Write(partWithColor);
    Console.ResetColor();
    Console.WriteLine(partWithoutColor);
}

static void PrintThreeParted(ConsoleColor firstColor, string firstPartWithColor, string partWithoutColor, ConsoleColor secondColor, string secondPartWithColor)
{
    Console.ForegroundColor = firstColor;
    Console.Write(firstPartWithColor);
    Console.ResetColor();
    Console.Write(partWithoutColor);
    Console.ForegroundColor = secondColor;
    Console.Write(secondPartWithColor);
    Console.ResetColor();
    Console.WriteLine();
}

static string VersionString(Assembly assembly)
{
    var version = assembly.GetName().Version;
    if (version == null)
        return "0.1";
    if (version.MinorRevision != 0)
        return $"{version.Major}.{version.Minor}.{version.Build}.{version.MinorRevision}";
    if (version.Build != 0)
        return $"{version.Major}.{version.Minor}.{version.Build}";
    return $"{version.Major}.{version.Minor}";
}