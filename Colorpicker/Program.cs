using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using static System.Formats.Asn1.AsnWriter;

if (!Directory.Exists("Images"))
    Directory.CreateDirectory("Images");
string WaitUntilPressed(string key, string message)
{
    if (message != "")
        Console.WriteLine(message);
    if (key == "Any")
        return Console.ReadKey(true).Key.ToString();
    else
        while (Console.ReadKey(true).Key.ToString() != key) ;
    return key;
}
int FindColorDistance(Color color1, Color color2)
{
    return Math.Abs(color1.R - color2.R) +
        Math.Abs(color1.G - color2.G) +
        Math.Abs(color1.B - color2.B);
}

Color ClosestMatch(HashSet<Color> colorSet, Color color)
{
    Color[] colorPalette = colorSet.ToArray();
    Color closestMatch = colorPalette[0];
    int smallestDifference = 100000;

    for (int i = 0; i<colorPalette.Length; i++)
    {
        Color toTest = colorPalette[i];
        int difference = FindColorDistance(toTest, color);
        if (smallestDifference > difference)
        {
            smallestDifference = difference;
            closestMatch = toTest;
        }

    }
    return closestMatch;
}

void ReplaceLine(string message, int done, int total)
{
    Console.Write("\r" + message + $" ({100*done/total}%)");
}

Console.Write("Palette (with file extensions) : ");
string paletteFile = Console.ReadLine();
Console.Write("Input (with file extensions) : ");
string inputFile = Console.ReadLine();
Console.Write("Output (without file extensions) : ");
string outputFile = Console.ReadLine();

HashSet<Color> palette = new HashSet<Color>();


Bitmap img = new Bitmap($"Images\\{paletteFile}");
Bitmap imgToConvert = new Bitmap($"Images\\{inputFile}");

Color[,] output = new Color[imgToConvert.Width, imgToConvert.Height];
Bitmap bmpOutput = new Bitmap(imgToConvert.Width, imgToConvert.Height);


for (int i = 0; i < img.Width; i++)
{
    for (int j = 0; j < img.Height; j++)
    {
        Color pixel = img.GetPixel(i, j);
        palette.Add(pixel);
    }
    ReplaceLine($"Generating color palette", i, img.Width);
}
ReplaceLine($"Generating color palette", 1,1);
Console.WriteLine();

for  (int i = 0; i < imgToConvert.Height; i++)
{
    for (int j = 0;j < imgToConvert.Width; j++)
    {
        Color pixel = imgToConvert.GetPixel(j, i);
        output[j,i] = ClosestMatch(palette, pixel);
    }
    ReplaceLine($"Replacing pixels", i, imgToConvert.Height);
}
ReplaceLine($"Replacing pixels", 1, 1);
Console.WriteLine();


for (int i = 0; i < imgToConvert.Height; i++)
{
    for (int j = 0; j < imgToConvert.Width; j++)
    {
        bmpOutput.SetPixel(j, i, output[j,i]);
    }
    ReplaceLine($"Generating image", i, imgToConvert.Height);
}
ReplaceLine($"Generating image", 1,1);
Console.WriteLine();

bmpOutput.Save($"Images\\{outputFile}.png", ImageFormat.Png);

Console.WriteLine("Image succesfully generated! (Images\\" + outputFile + ".png)");

WaitUntilPressed("Any", "Press any key to continue.");