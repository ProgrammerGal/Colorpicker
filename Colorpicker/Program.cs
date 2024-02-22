using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using static System.Formats.Asn1.AsnWriter;

if (!Directory.Exists("Images"))
    Directory.CreateDirectory("Images");


string Ask(string message)
{
    Console.Write(message);
    return Console.ReadLine();
}

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

//MAIN

Image paletteImage = new Image();
Image inputImage = new Image();
Image outputImage = new Image();

paletteImage.SetFilePath(Ask("Palette (with file extensions) : "), true);
inputImage.SetFilePath(Ask("Input (with file extensions) : "), true);
outputImage.SetFilePath(Ask("Output (without file extensions) : "), false);

HashSet<Color> palette = new HashSet<Color>();


Bitmap img = paletteImage.bitmapImage;


outputImage.imageTable = new Color[inputImage.GetImage().Width, inputImage.GetImage().Height];
outputImage.SetImage(new Bitmap(inputImage.GetImage().Width, inputImage.GetImage().Height));

paletteImage.GenerateHashSet("Generating color palette");
inputImage.GenerateHashSet("Generating input image palette");

if (paletteImage.palette.Count > inputImage.palette.Count)
{
    outputImage.ReplaceImage(paletteImage.palette, inputImage.palette, inputImage.GetImage());
}
else
{
    for (int i = 0; i < inputImage.GetImage().Height; i++)
    {
        for (int j = 0; j < inputImage.GetImage().Width; j++)
        {
            Color pixel = inputImage.GetImage().GetPixel(j, i);
            outputImage.imageTable[j, i] = ClosestMatch(paletteImage.GetHashSet(), pixel);
        }
        ReplaceLine($"Replacing pixels", i, inputImage.GetImage().Height);
    }
    ReplaceLine($"Replacing pixels", 1, 1);
    Console.WriteLine();


    for (int i = 0; i < inputImage.GetImage().Height; i++)
    {
        for (int j = 0; j < inputImage.GetImage().Width; j++)
        {
            outputImage.bitmapImage.SetPixel(j, i, outputImage.imageTable[j, i]);
        }
        ReplaceLine($"Generating image", i, inputImage.GetImage().Height);
    }
    ReplaceLine($"Generating image", 1, 1);
    Console.WriteLine();
}

outputImage.ExportImage();

WaitUntilPressed("Any", "Press any key to continue.");


class Image
{
    string filePath;
    public Bitmap bitmapImage;
    public HashSet<Color> palette = new HashSet<Color>();
    public Color[,] imageTable;

    public string GetFilePath()
    { return filePath; }

    public void SetFilePath(string newFilePath, bool setImage)
    {
        this.filePath = newFilePath;
        if (setImage)
            this.bitmapImage = new Bitmap($"Images\\{this.filePath}");
    }

    public void SetImage(Bitmap image)
    {
        this.bitmapImage = image;
    }

    public Bitmap GetImage()
    { return this.bitmapImage; }

    public void ExportImage()
    {
        bitmapImage.Save($"Images\\{filePath}.png", ImageFormat.Png);
    }

    void ReplaceLine(string message, int done, int total)
    {
        Console.Write("\r" + message + $" ({100 * done / total}%)");
    }

    public void GenerateHashSet(string message)
    {
        palette.Clear();
        for (int i = 0; i < bitmapImage.Width; i++)
        {
            for (int j = 0; j < bitmapImage.Height; j++)
            {
                Color pixel = bitmapImage.GetPixel(i, j);
                palette.Add(pixel);
            }
            ReplaceLine(message, i, bitmapImage.Width);
        }
        ReplaceLine(message, 1, 1);
        Console.WriteLine();
    }

    public HashSet<Color> GetHashSet()
    { return this.palette; }

    public void ReplaceImage(HashSet<Color> palette, HashSet<Color> inputPalette, Bitmap inputImage)
    {
        Color[] inputPaletteArr = inputPalette.ToArray();
        Color[] paletteArr = new Color[inputPaletteArr.Length];

        //Match
        for (int i = 0; i < inputPaletteArr.Length; i++)
        {
            paletteArr[i] = ClosestMatch(palette, inputPaletteArr[i]);
            ReplaceLine("Matching palette to image colors", i, inputPaletteArr.Length);
        }
        ReplaceLine("Matching palette to image colors", 1, 1);
        Console.WriteLine();

        //Replace
        for (int i = 0; i < bitmapImage.Height; i++)
        {
            for (int j = 0; j < bitmapImage.Width; j++)
            {
                bitmapImage.SetPixel(j, i, GetMatch(inputImage.GetPixel(j,i) ,inputPaletteArr, paletteArr));
            }
            ReplaceLine($"Generating image", i, bitmapImage.Height);
        }
        ReplaceLine($"Generating image", 1, 1);
        Console.WriteLine();


        ReplaceLine($"Replacing pixels", 1, 1);
        Console.WriteLine();
    }


    int FindColorDistance(Color color1, Color color2)
    {
        return Math.Abs(color1.R - color2.R) +
            Math.Abs(color1.G - color2.G) +
            Math.Abs(color1.B - color2.B);
    }

    Color ClosestMatch(HashSet<Color> palette, Color color)
    {
        Color[] colorPalette = palette.ToArray();
        Color closestMatch = colorPalette[0];
        int smallestDifference = 100000;

        for (int i = 0; i < colorPalette.Length; i++)
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

    Color GetMatch(Color color, Color[] colArray, Color[] palette)
    {
        return palette[Array.IndexOf(colArray,color)];
    }
}