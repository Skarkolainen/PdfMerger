using PdfSharp.Pdf.IO;
using PdfSharp.Pdf;
using System.Diagnostics;

Console.WriteLine("Welcome to the PDF Merger  Console App!");

string location = GetLocation();
Console.WriteLine($"You entered a valid location: {location}");

string[] filenames = GetAllFilenames(location, ["pdf"]);

Console.WriteLine("Files in current order:");
foreach (string filename in filenames.Select(Path.GetFileName))
{
    Console.WriteLine(filename);
}

LinkedList<FileToMerge> filesWithOrder = new();
foreach (string filename in filenames)
{
    filesWithOrder.AddLast(new FileToMerge { FilePath = filename });
}

Console.WriteLine(" Happy with order? (y/n)");

string happy = Console.ReadLine();

while (happy.ToLower() != "y")
{

    ChooseOrder(filesWithOrder);

    Console.WriteLine(" Happy with order? (y/n)");
    printOrder(filesWithOrder);
    happy = Console.ReadLine();
}

Console.WriteLine(" Merging files in the specified order:");
printOrder(filesWithOrder);

string outputFile = Path.GetDirectoryName(filesWithOrder.First().FilePath);
Console.WriteLine(outputFile);

mergeAndSavePDF(filesWithOrder, outputFile);







static string[] GetAllFilenames(string path, string[] fileFormats)
{
    try
    {
        // Get all filenames in the specified path and filter by file formats
        string[] filenames = Directory.GetFiles(path, "*.*")
            .Where(file => fileFormats.Any(format => file.EndsWith("." + format, StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        return filenames;
    }
    catch (Exception ex)
    {
        // Handle any potential exceptions (e.g., unauthorized access, invalid path)
        Console.WriteLine($"Error: {ex.Message}");
        return new string[0]; // Return an empty array in case of an error
    }
}

static string GetLocation()
{

    string location;
    do
    {
        Console.Write("Please enter a valid path to a folder: ");
        location = Console.ReadLine();

        if (!Directory.Exists(location))
        {
            Console.WriteLine($"The entered location '{location}' is not a valid path to a folder. Please try again.");
        }

    } while (!Directory.Exists(location));

    return location;
}

static void DisplayFilesWithOrder(LinkedList<FileToMerge> filesWithOrder, LinkedListNode<FileToMerge> selectedNode, ConsoleColor selectionColor)
{
    int i = 1;
    var node = filesWithOrder.First;

    Console.WriteLine();
    while (node != null)
    {

        Console.ForegroundColor = node.Equals(selectedNode) ? selectionColor : ConsoleColor.White;
        Console.WriteLine($"{i++}. {Path.GetFileName(node.Value.FilePath)}");


        node = node.Next;

    }
    Console.ResetColor();
}

static void ChooseOrder(LinkedList<FileToMerge> filesWithOrder)
{
    LinkedListNode<FileToMerge> selectedNode = filesWithOrder.First;
    bool moving = false;
    FileToMerge selectedFile;
    ConsoleColor selectionColor = ConsoleColor.Green;



    do
    {
        Console.Clear();
        Console.WriteLine("Specify the order of the files:");
        Console.WriteLine("     1. Navigate with up/down arrows");
        Console.WriteLine("     2. Select/ Unselect file with right-arrow");
        Console.WriteLine("     3. Move selected file up/down with with up/down arrows");
        Console.WriteLine("     4. Press enter do confirm current selection.");
        Console.WriteLine();
        DisplayFilesWithOrder(filesWithOrder, selectedNode, selectionColor);

        ConsoleKeyInfo key = Console.ReadKey();

        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                if (selectedNode != selectedNode.List.First)
                {
                    if (moving)
                    {
                        moveForward(selectedNode);
                    }
                    else
                    {
                        selectedNode = selectedNode.Previous;
                    }

                }

                break;

            case ConsoleKey.DownArrow:
                if (selectedNode != selectedNode.List.Last)
                {

                    if (moving)
                    {
                        moveBackward(selectedNode);
                    }
                    else
                    {
                        selectedNode = selectedNode.Next;
                    }
                }


                break;

            case ConsoleKey.RightArrow:
                selectionColor = ConsoleColor.Green;
                if (!moving)
                {
                    selectionColor = ConsoleColor.Red;
                }
                moving = !moving;

                break;

            case ConsoleKey.Escape:
            case ConsoleKey.Enter:
                return; // Exit the function if the user presses Escape
        }

    } while (true);
}

static void moveBackward(LinkedListNode<FileToMerge> node)
{
    if (node.Next != null)
    {
        var nextNode = node.Next;
        node.List.Remove(node);
        nextNode.List.AddAfter(nextNode, node);
    }
}

static void moveForward(LinkedListNode<FileToMerge> node)
{
    if (node.Previous != null)
    {
        var prev = node.Previous;
        node.List.Remove(node);
        prev.List.AddBefore(prev, node);
    }
}

static void printOrder(LinkedList<FileToMerge> list)
{
    Console.WriteLine();
    foreach (var file in list)
    {
        Console.WriteLine(Path.GetFileName(file.FilePath));
    }
    Console.WriteLine();
}

static void mergeAndSavePDF(LinkedList<FileToMerge> files, string outputFolder)
{

    // Open the output document
    PdfDocument outputDocument = new PdfDocument();

    // Iterate files
    foreach (var file in files)
    {
        // Open the document to import pages from it.
        PdfDocument inputDocument = PdfReader.Open(file.FilePath, PdfDocumentOpenMode.Import);

        // Iterate pages
        int count = inputDocument.PageCount;
        for (int idx = 0; idx < count; idx++)
        {
            // Get the page from the external document...
            PdfPage page = inputDocument.Pages[idx];
            // ...and add it to the output document.
            outputDocument.AddPage(page);
        }
    }

    // Save the document...
    string filename = outputFolder + $"\\MergedPdfFile_{DateTime.UtcNow.ToString("dd_MM_yyyy_HH_mm_ss")}.pdf";

    outputDocument.Save(filename);
    // ...and start a viewer.
    var p = new Process();
    p.StartInfo = new ProcessStartInfo(filename)
    {
        UseShellExecute = true
    };
    p.Start();
}
public class FileToMerge
{
    public string FilePath { get; set; }
    public int Order { get; set; }
}