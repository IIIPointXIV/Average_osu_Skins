using System.Diagnostics;
using System.IO;
public class Form1 : Form
{
    enum ColorType
    {
        a,
        r,
        g,
        b,
    };
    List<Bitmap> images = new List<Bitmap>();
    List<int> widthOffset = new List<int>();
    List<int> heightOffset = new List<int>();
    bool AvgOnlyExistingPixels;
    public void RunForm(string path, bool AvgOnlyExistingPixelsArg)
    {
        AvgOnlyExistingPixels = AvgOnlyExistingPixelsArg;
        Stopwatch time = new Stopwatch();
        time.Start();

        DirectoryInfo di = new DirectoryInfo(path);
        foreach(FileInfo file in di.GetFiles())
        {
            if(file.Name.Contains(".png") || file.Name.Contains(".jpg"))
            {
                images.Add((Bitmap)Image.FromFile(file.FullName));
            }
        }

        int maxHeight=0;
        int maxWidth=0;

        foreach(Image thisImage in images)
        {
            maxHeight = (maxHeight < thisImage.Height ? thisImage.Height : maxHeight);
            maxWidth = (maxWidth < thisImage.Width ? thisImage.Width : maxWidth);
        }

        foreach(Bitmap thisImage in images)
        {
            widthOffset.Add((maxWidth-thisImage.Width)/2);
            heightOffset.Add((maxHeight-thisImage.Height)/2);
        }

        Console.WriteLine($"A total of {images.Count} images.");
        Console.WriteLine($"Max width of {maxWidth} and max Height of {maxHeight}.");

        Bitmap finalImage = new Bitmap(maxWidth, maxHeight);

        for (int dx = 0; dx < maxWidth; dx++)
        {
            for (int dy = 0; dy < maxHeight; dy++)
            {
                finalImage.SetPixel(dx, dy, MakeColor(dx, dy));
            }
        }
        finalImage.Save("avg.png");
        Console.WriteLine("Took " + time.ElapsedMilliseconds/1000 + " seconds");
        this.Close();
    }

    private int GetColorAverage(ColorType color, int dx, int dy)
    {
        int newColor=0;
        int origDX = dx;
        int origDY = dy;
        int countOffset = 0;
        foreach(Bitmap image in images)
        {
            dx = origDX;
            dy = origDY;
            int thisImageIndex = images.IndexOf(image);

            dx -= widthOffset[thisImageIndex];
            dy -= heightOffset[thisImageIndex];
            
            if(dx<0||dy<0 || image.Height < dy+1 || image.Width < dx+1)
            {
                if(AvgOnlyExistingPixels)
                    countOffset++;
                continue;
            }

            Color thisColor = image.GetPixel(dx, dy); 
            if(thisColor.A == 0)
            {
                if(AvgOnlyExistingPixels)
                    countOffset++;
                continue;
            }
            switch(color)
            {
                case ColorType.a:
                    newColor += thisColor.A;
                    break;
                case ColorType.r:
                    newColor += thisColor.R;
                    break;
                case ColorType.g:
                    newColor += thisColor.G;
                    break;
                case ColorType.b:
                    newColor += thisColor.B;
                    break;
                default:
                    Console.WriteLine("Error | " + color.ToString());
                    break;
            }
        }
        return (int)MathF.Round(newColor/((images.Count()==countOffset ? 1 : images.Count()-countOffset)));
    }

    private Color MakeColor(int dx, int dy)
    {
        return Color.FromArgb(GetColorAverage(ColorType.a, dx, dy), GetColorAverage(ColorType.r, dx, dy), GetColorAverage(ColorType.g, dx, dy), GetColorAverage(ColorType.b, dx, dy));
    }
}