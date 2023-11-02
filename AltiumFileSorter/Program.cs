using AltiumFileSorter.Services;

Console.WriteLine("Give path to file you want to sort:");
string filePath = Console.ReadLine();
if (String.IsNullOrEmpty(filePath))
    return;

try
{
    ExternalSortingFacade externalSorting = new ExternalSortingFacade(filePath);

    externalSorting.Run();
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}
Console.WriteLine("Done");