using System.Net.Mime;
using System.Security.AccessControl;
using System.Reflection;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

public class DirectBitmap : IDisposable
{
    public Bitmap Bitmap { get; private set; }
    public Int32[] Bits { get; private set; }
    public bool Disposed { get; private set; }
    public int Height { get; private set; }
    public int Width { get; private set; }

    protected GCHandle BitsHandle { get; private set; }

    public DirectBitmap(int width, int height)
    {
        Width = width;
        Height = height;
        Bits = new Int32[width * height];
        BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
        Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
    }

     /*public void SetNewBitmap(Bitmap image)
    {
        Width = image.Width;
        Height = image.Height;
        Bits = new Int32[Width * Height];
        BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
        Bitmap = new Bitmap(Width, Height, Width*4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
    } */

    public void SetPixel(int x, int y, Color colour)
    {
        int index = x + (y * Width);
        int col = colour.ToArgb();

        Bits[index] = col;
    }

    public Color GetPixel(int x, int y)
    {
        int index = x + (y * Width);
        int col = Bits[index];
        Color result = Color.FromArgb(col);

        return result;
    }

    public void Dispose()
    {
        if (Disposed) return;
        Disposed = true;
        Bitmap.Dispose();
        BitsHandle.Free();
    }
}

public class Form1 : Form
{
    enum NumberNames
    {
        HitCirclePrefix,
        ScorePrefix,
        ComboPrefix,
    };
    enum ScoreNumNames
    {
        comma,
        dot,
        percent,
        x,
    }
    List<int> widthOffset = new List<int>();
    List<int> heightOffset = new List<int>();
    bool avgOnlyExistingPixels = false;
    int threadCount = 8;
    DirectoryInfo mainFolderDi;
        CheckBox textBox;
        CheckBox workingTextBox;
        CheckBox cursorTrail;
        CheckBox percentageTextBox;
    Button makeAvgButton;
    TextBox threadCountBox;
    ToolTip toolTip;
    List<DirectBitmap> images = new List<DirectBitmap>();
    Font mainFont;
    string skinFolderPath;
    string currentFileName;
    Dictionary<string, string> skinFileNames = new Dictionary<string, string>() //the names of all of the skin files that may exist. second string is the offset that osu uses when displaying it
    {
        {"welcome_text", "Centre"},
        {"menu-snow", "Centre"},
        {"options-offset-tick", "Centre"},
        {"cursor", "Centre"},
        {"cursortrail", "Centre"},
        {"cursormiddle", "Centre"},
        {"cursor-smoke", "Centre"},
        {"cursor-ripple", "Centre"},
        {"star", "Centre"},
        {"star2", "Centre"},
        {"selection-mode", "BottomLeft"},
        {"selection-mode-over", "BottomLeft"},
        {"selection-mods", "BottomLeft"},
        {"selection-mods-over", "BottomLeft"},
        {"selection-random", "BottomLeft"},
        {"selection-random-over", "BottomLeft"},
        {"selection-options", "BottomLeft"},
        {"selection-options-over", "BottomLeft"},
        {"selection-tab", "TopLeft"},
        {"button-left", "TopRight"},
        {"button-right", "TopLeft"},
        {"button-middle", "Top"},
        {"menu-back-0", "BottomLeft"},
        {"menu-back", "BottomLeft"},
        {"mode-osu", "Centre"},
        {"mode-taiko", "Centre"},
        {"mode-fruits", "Centre"},
        {"mode-mania", "Centre"},
        {"mode-osu-med", "Centre"},
        {"mode-taiko-med", "Centre"},
        {"mode-fruits-med", "Centre"},
        {"mode-mania-med", "Centre"},
        {"mode-osu-small", "Centre"},
        {"mode-taiko-small", "Centre"},
        {"mode-fruits-small", "Centre"},
        {"mode-mania-small", "Centre"},
        {"selection-mod-easy", "Centre"},
        {"selection-mod-nofail", "Centre"},
        {"selection-mod-halftime", "Centre"},
        {"selection-mod-hardrock", "Centre"},
        {"selection-mod-suddendeath", "Centre"},
        {"selection-mod-perfect", "Centre"},
        {"selection-mod-doubletime", "Centre"},
        {"selection-mod-nightcore", "Centre"},
        {"selection-mod-hidden", "Centre"},
        {"selection-mod-fadein", "Centre"},
        {"selection-mod-flashlight", "Centre"},
        {"selection-mod-relax", "Centre"},
        {"selection-mod-relax2", "Centre"},
        {"selection-mod-spunout", "Centre"},
        {"selection-mod-autoplay", "Centre"},
        {"selection-mod-cinema", "Centre"},
        {"selection-mod-scorev2", "Centre"},
        {"selection-mod-key1", "Centre"},
        {"selection-mod-key2", "Centre"},
        {"selection-mod-key3", "Centre"},
        {"selection-mod-key4", "Centre"},
        {"selection-mod-key5", "Centre"},
        {"selection-mod-key6", "Centre"},
        {"selection-mod-key7", "Centre"},
        {"selection-mod-key8", "Centre"},
        {"selection-mod-key9", "Centre"},
        {"selection-mod-keycoop", "Centre"},
        {"selection-mod-mirror", "Centre"},
        {"selection-mod-random", "Centre"},
        {"selection-mod-touchdevice", "Centre"},
        {"selection-mod-freemodallowed", "Centre"},
        {"ranking-xh", "Centre"},
        {"ranking-xh-small", "Centre"},
        {"ranking-sh", "Centre"},
        {"ranking-sh-small", "Centre"},
        {"ranking-x", "Centre"},
        {"ranking-x-small", "Centre"},
        {"ranking-s", "Centre"},
        {"ranking-s-small", "Centre"},
        {"ranking-a", "Centre"},
        {"ranking-a-small", "Centre"},
        {"ranking-b", "Centre"},
        {"ranking-b-small", "Centre"},
        {"ranking-c", "Centre"},
        {"ranking-c-small", "Centre"},
        {"ranking-d", "Centre"},
        {"ranking-d-small", "Centre"},
        {"ranking-title", "TopRight"},
        {"ranking-panel", "TopLeft"},
        {"ranking-maxcombo", "TopLeft"},
        {"ranking-accuracy", "TopLeft"},
        {"ranking-graph", "TopLeft"},
        {"ranking-perfect", "Centre"},
        {"ranking-winner", "TopLeft"},
        {"ranking-replay", "Right"},
        {"ranking-retry", "Right"},
        {"pause-overlay", "Centre"},
        {"fail-background", "Centre"},
        {"pause-back", "Centre"},
        {"pause-continue", "Centre"},
        {"pause-retry", "Centre"},
        {"pause-replay", "Right"},
        {"scorebar-bg", "TopLeft"},
        {"scorebar-colour", "TopLeft"},
        {"scorebar-colour-0", "TopLeft"},
        {"scorebar-ki", "Centre"},
        {"scorebar-kidanger", "Centre"},
        {"scorebar-kidanger2", "Centre"},
        {"scorebar-marker", "Centre"},
        {"ready", "Centre"},
        {"count3", "Centre"},
        {"count2", "Centre"},
        {"count1", "Centre"},
        {"go", "Centre"},
        {"comboburst", "BottomLeft"},
        {"comboburst-mania", "BottomLeft"},
        {"comboburst-fruits", "BottomLeft"},
        {"play-skip-0", "BottomRight"},
        {"play-skip", "BottomRight"},
        {"menu-button-background", "Left"},
        {"play-unranked", "Centre"},
        {"play-warningarrow", "Centre"},
        {"arrow-pause", "Centre"},
        {"arrow-warning", "Centre"},
        {"section-pass", "Centre"},
        {"section-fail", "Centre"},
        {"multi-skipped", "BottomRight"},
        {"masking-border", "Right"},
        {"hitcircleselect", "Centre"},
        {"inputoverlay-background", "TopRight"},
        {"inputoverlay-key", "Centre"},
        {"approachcircle", "Centre"},
        {"hitcircle", "Centre"},
        {"hitcircleoverlay", "Centre"},
        {"followpoint", "Centre"},
        {"followpoint-0", "Centre"},
        {"reversearrow", "Centre"},
        {"sliderstartcircle", "Centre"},
        {"sliderstartcircleoverlay", "Centre"},
        {"sliderendcircle", "Centre"},
        {"sliderendcircleoverlay", "Centre"},
        {"sliderfollowcircle", "Centre"},
        {"sliderfollowcircle-0", "Centre"},
        {"sliderb", "Centre"},
        {"sliderb0", "Centre"},
        {"sliderb-nd", "Centre"},
        {"sliderb-spec", "Centre"},
        {"spinner-background", "Centre"},
        {"spinner-metre", "TopLeft"},
        {"spinner-bottom", "Centre"},
        {"spinner-glow", "Centre"},
        {"spinner-middle", "Centre"},
        {"spinner-middle2", "Centre"},
        {"spinner-top", "Centre"},
        {"spinner-rpm", "TopLeft"},
        {"spinner-clear", "Centre"},
        {"spinner-spin", "Centre"},
        {"spinner-osu", "Centre"},
        {"hit0", "Centre"},
        {"hit0-0", "Centre"},
        {"hit50", "Centre"},
        {"hit50-0", "Centre"},
        {"hit100", "Centre"},
        {"hit100-0", "Centre"},
        {"hit100k", "Centre"},
        {"hit100k-0", "Centre"},
        {"hit300k", "Centre"},
        {"hit300", "Centre"},
        {"hit300-0", "Centre"},
        {"hit300k-0", "Centre"},
        {"hit300g", "Centre"},
        {"hit300g-0", "Centre"},
        {"particle50", "Centre"},
        {"particle100", "Centre"},
        {"particle300", "Centre"},
        {"lighting", "Centre"},
        {"sliderpoint10", "Centre"},
        {"sliderpoint30", "Centre"},
        {"pippidonidle", "BottomLeft"},
        {"pippidonkiai", "BottomLeft"},
        {"pippidonfail", "BottomLeft"},
        {"pippidonclear", "BottomLeft"},
        {"taiko-flower-group", "Bottom"},
        {"taiko-slider", "TopLeft"},
        {"taiko-slider-fail", "TopLeft"},
        {"taiko-bar-left", "TopLeft"},
        {"taiko-drum-inner", "TopLeft"},
        {"taiko-drum-outer", "TopLeft"},
        {"taiko-bar-right", "TopLeft"},
        {"taiko-bar-right-glow", "TopLeft"},
        {"taiko-barline", "Centre"},
        {"taikohitcircle", "Centre"},
        {"taikohitcircleoverlay", "Centre"},
        {"taikobigcircle", "Centre"},
        {"taikobigcircleoverlay", "Centre"},
        {"taiko-glow", "Centre"},
        {"taiko-roll-middle", "TopLeft"},
        {"taiko-roll-end", "TopLeft"},
        {"sliderscorepoint", "Centre"},
        {"spinner-warning", "Centre"},
        {"spinner-circle", "Centre"},
        {"spinner-approachcircle", "Centre"},
        {"taiko-hit0", "Centre"},
        {"taiko-hit100", "Centre"},
        {"taiko-hit300", "Centre"},
        {"taiko-hit100k", "Centre"},
        {"taiko-hit300k", "Centre"},
        {"taiko-hit300g", "Centre"},
        {"fruit-catcher-idle", "Top"},
        {"fruit-catcher-kiai", "Top"},
        {"fruit-catcher-fail", "Top"},
        {"fruit-ryuuta", "Top"},
        {"fruit-apple", "Centre"},
        {"fruit-apple-overlay", "Centre"},
        {"fruit-grapes", "Centre"},
        {"fruit-grapes-overlay", "Centre"},
        {"fruit-orange", "Centre"},
        {"fruit-orange-overlay", "Centre"},
        {"fruit-pear", "Centre"},
        {"fruit-pear-overlay", "Centre"},
        {"fruit-bananas", "Centre"},
        {"fruit-bananas-overlay", "Centre"},
        {"fruit-drop", "Centre"},
        {"fruit-drop-overlay", "Centre"},
        {"selection-mod-target", "Centre"},
        {"target", "Centre"},
        {"targetoverlay", "Centre"},
        {"target-pt-1", "Centre"},
        {"target-pt-2", "Centre"},
        {"target-pt-3", "Centre"},
        {"target-pt-4", "Centre"},
        {"target-pt-5", "Centre"},
        {"targetoverlay-pt-1", "Centre"},
        {"targetoverlay-pt-2", "Centre"},
        {"targetoverlay-pt-3", "Centre"},
        {"targetoverlay-pt-4", "Centre"},
        {"targetoverlay-pt-5", "Centre"},
    };
    Dictionary<string, int> skinINIBool = new Dictionary<string, int>()
    {
        {"CursorExpand", 0},
        {"CursorCentre", 0},
        {"CursorRotate", 0},
        {"CursorTrailRotate", 0},
        {"LayeredHitSounds", 0},
        {"ComboBurstRandom", 0},
        {"HitCircleOverlayAboveNumber", 0},
        {"SliderStyle", 0},
        {"SliderBallFlip", 0},
        {"AllowSliderBallTint", 0},
        {"SpinnerNoBlink", 0},
        {"SpinnerFadePlayfield", 0},
        {"SpinnerFrequencyModulate", 0},
    };
    Dictionary<string, string> skinINIRGB = new Dictionary<string, string>()
    {
        {"SongSelectActiveText", "0,0,0"},
        {"SongSelectInactiveText", "0,0,0"},
        {"MenuGlow", "0,0,0"},
        {"StarBreakAdditive", "0,0,0"},
        {"InputOverlayText", "0,0,0"},
        {"SliderBall", "0,0,0"},
        {"SliderTrackOverride", "0,0,0"},
        {"SliderBorder", "0,0,0"},
        {"SpinnerBackground", "0,0,0"},
        {"Combo1", "0,0,0"},
        {"Combo2", "0,0,0"},
        {"Combo3", "0,0,0"},
        {"Combo4", "0,0,0"},
        {"Combo5", "0,0,0"},
        {"Combo6", "0,0,0"},
        {"Combo7", "0,0,0"},
        {"Combo8", "0,0,0"},
        {"HyperDash", "0,0,0"},
        {"HyperDashFruit", "0,0,0"},
        {"HyperDashAfterImage", "0,0,0"},
    };
    //Stopwatch averageCurrentImageTime = new Stopwatch();
    List<List<DirectBitmap>> hitCircleNumbers = new List<List<DirectBitmap>>();
    List<List<DirectBitmap>> scoreNumbers = new List<List<DirectBitmap>>();
    List<List<DirectBitmap>> comboNumbers = new List<List<DirectBitmap>>();
    public void RunForm()
    {
        mainFont = new Font("Segoe UI", 12);
        this.MaximizeBox = false;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.Name = "Average Your osu Skins!";
        this.Text = "Average Your osu Skins!";
        this.Size = new Size(450, 450);

        if(!File.Exists(Path.Combine(Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), "appdata", "Local", "osu!"), "osu!.exe")))
        {
            Console.WriteLine("No osu path found. Stopping.");
            return;
        }
        skinFolderPath = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), "appdata", "Local", "osu!", "skins");
        mainFolderDi = new DirectoryInfo(skinFolderPath);

        textBox = new CheckBox()
        {
            Font = mainFont,
            Left = -12,
            Text = (File.Exists(Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), "appdata", "Local", "osu!", "skins", "!!Most Average Skin", "skin.ini"))
                    ? mainFolderDi.GetDirectories().Count() : mainFolderDi.GetDirectories().Count()-1) + " total skins found.",
            Width = 180,
        };
        Controls.Add(textBox);

        threadCountBox = new TextBox()
        {
            Font = mainFont,
            Left = 170,
            Text = threadCount.ToString(),
            Top = 25,
            Width = 32,
        };
        threadCountBox.TextChanged += (sender, ev) =>
        {
            if(int.TryParse(threadCountBox.Text, out int num))
                threadCount = num;
        };
        threadCountBox.LostFocus += (sender, ev) =>
        {
            if(String.IsNullOrWhiteSpace(threadCountBox.Text))
                threadCountBox.Text = "1";
        };
        Controls.Add(threadCountBox);

        cursorTrail = new CheckBox()
        {
            Font = mainFont,
            Left = 170,
            Text = "Smooth cursor trail",
            Width = 300,
            Checked = false,
        };
        Controls.Add(cursorTrail);

        makeAvgButton = new Button()
        {
            Text = "Make Average Skin",
            Font = mainFont,
            Top = 25,
            Width = 160,
        };
        makeAvgButton.Click += new EventHandler(ButtonClick);
        Controls.Add(makeAvgButton);

        workingTextBox = new CheckBox()
        {
            Font = mainFont,
            Left = -12,
            Width = 500,
            Top = 50,
        };

        percentageTextBox = new CheckBox()
        {
            Font = mainFont,
            Left = -12,
            Width = 100,
            Top = 75,
        };
        ClearTheNumbers();

        toolTip = new ToolTip();
        toolTip.SetToolTip(threadCountBox, "The amount of threads the program will use");
        toolTip.SetToolTip(cursorTrail, "Should the cursor trail be smooth?\nChecked means it is smooth.\nIt is recommended to leave this off.");
    }
    
    private void ButtonClick(object thing, EventArgs e)
    {
        Controls.Add(workingTextBox);
        Controls.Add(percentageTextBox);
        UpdateWorkingText("Starting Up...");
        Task.Factory.StartNew(() => MakeAverageSkin());
    }

    private void UpdateWorkingText(string text)
    {
        if(string.IsNullOrWhiteSpace(text))
        {
            List<string> thing = new List<string>(skinFileNames.Keys);
            workingTextBox.Text = $"Working on \"{skinFileNames.Keys.ElementAt(thing.IndexOf(currentFileName)+1)}\"...";
            percentageTextBox.Text = ((int)((((float)thing.IndexOf(currentFileName)+1)/skinFileNames.Keys.Count())*100)).ToString() + "% done";
        }
        else
            workingTextBox.Text = text;
    }
    
    private void MakeAverageSkin()
    {
        Console.WriteLine("Using " + threadCount + " threads.");
        Stopwatch mainTimer = new Stopwatch();
        mainTimer.Restart();
        Directory.CreateDirectory(Path.Combine(skinFolderPath, "!!Most Average Skin"));
        ClearSkinFolder();

        if(!cursorTrail.Checked)
            skinFileNames.Remove("cursormiddle");

        MakeAverageImages();
        UpdateWorkingText("Averaging the skin.ini");
        MakeAverageSkinINI();
        /* Console.WriteLine($"Skin averaged! Took {mainTimer.ElapsedMilliseconds} ms for {mainFolderDi.GetDirectories().Count()-1} skins!");
        Console.WriteLine($"Tried to look at {pixelsTried} pixels and looked at {pixelsLookedAt} pixels");
        Console.WriteLine($"Took {doubleImageTimer.ElapsedMilliseconds} ms to double image size");
        Console.WriteLine($"Took {getPlacePixels.ElapsedMilliseconds} ms to get and place pixels"); */

        mainTimer.Stop();

        UpdateWorkingText($"Skins averaged in {MathF.Round(mainTimer.ElapsedMilliseconds/1000)} seconds! Enjoy your new skin!");
        Controls.Remove(percentageTextBox);
    }
    
    private void MakeAverageImages()
    {
        //make static elements
        foreach(string currentSkinFileName in skinFileNames.Keys)
        {
            ClearImages();
            if(cursorTrail.Checked && currentFileName.Contains("cursormiddle", StringComparison.OrdinalIgnoreCase))
                continue;

            UpdateWorkingText("");
            string foundImage = "";
            foreach(DirectoryInfo currentSkinFolder in mainFolderDi.GetDirectories())
            {
                bool sdFound = false;

                foreach(FileInfo file in currentSkinFolder.GetFiles())
                {
                    string currentName = file.Name;
                    if(currentName.Contains("play-skip"))
                        currentName.Replace("play-skip", "play-skip-0");
                    else if(currentName.Contains("menu-back"))
                        currentName.Replace("menu-back", "menu-back-0");

                    if(!currentName.Contains("png"))
                        continue;
                    
                    if(string.Equals(currentSkinFileName+"@2x.png", currentName, StringComparison.OrdinalIgnoreCase))
                    {
                        images.Add(ConvertToDirectBitmap(Bitmap.FromFile(file.FullName)));
                        sdFound = false;
                        break;
                    }
                    else if(string.Equals(currentSkinFileName+".png", currentName, StringComparison.OrdinalIgnoreCase))
                    {
                        foundImage = file.FullName;
                        sdFound = true;
                    }
                }
                if(sdFound)
                {
                    images.Add(DoubleImageSize(Image.FromFile(foundImage)));
                }
            }
            currentFileName = currentSkinFileName;
            MakeAverageImg(Path.Combine(skinFolderPath, "!!Most Average Skin", currentSkinFileName + "@2x.png"));
            ClearImages();
        }
    }

    private void MakeAverageSkinINI()
    {
        Dictionary<string, int> skinINIBoolCount = new Dictionary<string, int>(skinINIBool);
        Dictionary<string, int> skinINIRGBCount = new Dictionary<string, int>();
        foreach(string key in skinINIRGB.Keys)
            skinINIRGBCount.Add(key, 0);

        foreach(string key in skinINIBool.Keys)
            skinINIBool[key] = 0;
        foreach(string key in skinINIRGB.Keys)
            skinINIRGB[key] = "0,0,0";
        
        foreach(DirectoryInfo currentSkinFolder in mainFolderDi.GetDirectories())
        {
            foreach(FileInfo file in currentSkinFolder.GetFiles())
            {
                if(file.Name != "skin.ini" || file.FullName.Contains("!!Most Average Skin"))
                    continue;
                
                using(StreamReader currentSkinIni = new StreamReader(file.FullName))
                {
                    string thisCurrentLine;
                    while((thisCurrentLine = currentSkinIni.ReadLine()) != null)
                    {
                        string[] lineArray = thisCurrentLine.Split(':');
                        if(skinINIBool.ContainsKey(lineArray[0]))
                        {
                            skinINIBool[lineArray[0]] += int.Parse(lineArray[1].Replace(" ", "").Remove(1));
                            skinINIBoolCount[lineArray[0]]++;
                            continue;
                        }

                        if(skinINIRGB.ContainsKey(lineArray[0]))
                        {
                            string[] colorArray = lineArray[1].Replace(" ", "").Split(',');
                            string[] currentColor = skinINIRGB[lineArray[0]].Split(',');
                            string newColor = "";

                            for (int i = 0; i < 3; i++)
                            {
                                string num = colorArray[i];
                                if(!int.TryParse(num, out int y))
                                {
                                    num = num.Remove(2);
                                    num = new String(num.Where(char.IsNumber).ToArray());
                                }
                                
                                newColor += (int.Parse(num)+int.Parse(currentColor[i])).ToString() + (i==2 ? "" : ",");
                            }
                            skinINIRGB[lineArray[0]] = newColor;
                            skinINIRGBCount[lineArray[0]]++;
                            continue;
                        }

                        if(Enum.TryParse(typeof(NumberNames), lineArray[0], true, out object temp)) //if line contains the prefix of a font files
                        {
                            string prefix = lineArray[1].Replace(" ", ""); //the prefix of the files
                            NumberNames name = (NumberNames)temp;
                            foreach(FileInfo nowFile in currentSkinFolder.GetFiles())
                            {
                                if(nowFile.FullName.Contains(prefix+"-", StringComparison.OrdinalIgnoreCase))
                                {
                                    string replaced = nowFile.Name.Replace("@2x", "").Replace(".png", "").Replace("-", "");
                                    int num;
                                    if(!int.TryParse(replaced.ElementAt(replaced.Count()-1).ToString(), out num))
                                    {   
                                        if(Enum.TryParse(typeof(ScoreNumNames), replaced.Replace(prefix, ""), true, out object val))
                                            num = 9+(int)val;
                                        else
                                            break;
                                    }

                                    DirectBitmap img = ConvertToDirectBitmap(Bitmap.FromFile(nowFile.FullName));
                                    switch((NumberNames)temp)
                                    {
                                        case NumberNames.HitCirclePrefix:
                                            hitCircleNumbers[num].Add(img);
                                            break;
                                        case NumberNames.ComboPrefix:
                                            comboNumbers[num].Add(img);
                                            break;
                                        case NumberNames.ScorePrefix:
                                            scoreNumbers[num].Add(img);
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                        }
                    }
                };
            }
        }

        foreach(string key in skinINIBool.Keys)
        {
            skinINIBool[key] = (int)MathF.Round(skinINIBool[key]/(skinINIBoolCount[key]==0? 1 : skinINIBoolCount[key]));
        }
        skinINIBool["CursorCentre"] = 1;

        foreach(string key in skinINIRGB.Keys)
        {
            string[] colorsArray = skinINIRGB[key].Split(',');
            string avgColor = "";
            for (int i = 0; i < 3; i++)
            {
                avgColor += (int.Parse(colorsArray[i])/(skinINIRGBCount[key]==0? 1 : skinINIRGBCount[key])).ToString() + (i==2 ? "": ',');
            }
            skinINIRGB[key] = avgColor;
        }

        File.Copy("skin.ini", Path.Combine(skinFolderPath, "!!Most Average Skin", "skin.ini"));
        StreamWriter mainSkinIni = new StreamWriter(Path.Combine(skinFolderPath, "!!Most Average Skin", "skin.ini"));
        string currentLine;
        using(StreamReader tempSkinIni = new StreamReader("skin.ini"))
        {
            while((currentLine = tempSkinIni.ReadLine()) != null)
                mainSkinIni.WriteLine(currentLine);
        }

        foreach(string key in skinINIBool.Keys)
        {
            mainSkinIni.WriteLine(key + ": " + skinINIBool[key]);
        }
        mainSkinIni.WriteLine("[Colours]");
        foreach(string key in skinINIRGB.Keys)
        {
            if(key == "HyperDash")
            {
                mainSkinIni.WriteLine("[Fonts]");
                mainSkinIni.WriteLine("HitCirclePrefix: default");
                mainSkinIni.WriteLine("ScorePrefix: score");
                mainSkinIni.WriteLine("ComboPrefix: combo");
                mainSkinIni.WriteLine("[CatchTheBeat]");
            }
            else if(key.Contains("Combo") && skinINIRGB[key] == "0,0,0")
                continue;
            mainSkinIni.WriteLine(key + ": " + skinINIRGB[key]);
        }
        mainSkinIni.Close();
        UpdateWorkingText("Working on the numbers");
        currentFileName = "welcome_text";
        for (int i = 0; i <= 14; i++)
        {
            string name = i.ToString();
            if(i<=9)
            {
                ClearImages();
                UpdateWorkingText($"Currently working on \"default-{i}\"");
                images = hitCircleNumbers[i];
                MakeAverageImg(Path.Combine(skinFolderPath, "!!Most Average Skin", $"default-{i}@2x.png"));
            }
            else
            {
                name = ((ScoreNumNames)(i-9)).ToString();
            }
            ClearImages();

            images = comboNumbers[i];
            UpdateWorkingText($"Currently working on \"combo-{name}\"");
            MakeAverageImg(Path.Combine(skinFolderPath, "!!Most Average Skin", $"combo-{name}@2x.png"));
            ClearImages();

            images = scoreNumbers[i];
            UpdateWorkingText($"Currently working on \"score-{name}\"");
            MakeAverageImg(Path.Combine(skinFolderPath, "!!Most Average Skin", $"score-{name}@2x.png"));
        }
        ClearTheNumbers();
        ClearImages();
    }
    
    private DirectBitmap DoubleImageSize(Image image)  
    {  
        DirectBitmap returned = new DirectBitmap(image.Width*2, image.Height*2);
        Rectangle dub = new Rectangle(0, 0, image.Width*2, image.Height*2);

        using (Graphics graphic = Graphics.FromImage(returned.Bitmap))  
        {  
            graphic.DrawImage(image, 0, 0, image.Width*2, image.Height*2);  
            graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;  
            graphic.SmoothingMode = SmoothingMode.HighQuality;  
            graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;  
            graphic.CompositingQuality = CompositingQuality.HighQuality;
            graphic.DrawImage(returned.Bitmap, dub);
        }  
        image.Dispose();

        return returned;  
    }  

    private DirectBitmap ConvertToDirectBitmap(Image image)
    {
        DirectBitmap temp = new DirectBitmap(image.Width, image.Height);
        Rectangle thing = new Rectangle(0, 0, image.Width, image.Height);

        using(Graphics graph = Graphics.FromImage(temp.Bitmap))
            graph.DrawImage(image, thing);
        
        image.Dispose();
        return temp;
    }

    private void MakeAverageImg(string savePath)
    {
        if(images.Count() == 0)
            return;
        
        widthOffset.Clear();
        heightOffset.Clear();
        //averageCurrentImageTime.Restart();
        int maxImgHeight = 0;
        int maxImgWidth = 0;
        foreach(DirectBitmap thisImage in images)
        {
            maxImgHeight = (int)MathF.Max(thisImage.Height, maxImgHeight);
            maxImgWidth = (int)MathF.Max(thisImage.Width, maxImgWidth);
            //maxHeight = (maxHeight < thisImage.Height ? thisImage.Height : maxHeight);
            //maxWidth = (maxWidth < thisImage.Width ? thisImage.Width : maxWidth);
        }
        string offsetType = skinFileNames[currentFileName];
        foreach(DirectBitmap thisImage in images)
        {
            widthOffset.Add(offsetType.Contains("Left") ? 0 : (maxImgWidth-thisImage.Width)/2);
            heightOffset.Add(offsetType.Contains("Top") ? 0 : (maxImgHeight-thisImage.Height)/(offsetType.Contains("Bottom") ? 1 : 2));
        }

        //Console.WriteLine($"A total of {images.Count} images.");
        //Console.WriteLine($"Max width of {maxWidth} and max Height of {maxHeight}.");

        DirectBitmap finalImage = new DirectBitmap(maxImgWidth, maxImgHeight);
        List<Task> threadList = new List<Task>();
        int amount = (int)MathF.Floor(maxImgWidth/threadCount);
        int endingOffset = maxImgWidth - (amount*threadCount);

        for(int i = 0; i<threadCount; i++)
        {
            int startX = i*amount;
            int endX = (i+1)*amount + ((i+1)==threadCount ? endingOffset :0)-1;
            threadList.Add(Task.Factory.StartNew(()=>RunByThread(finalImage, startX, endX)));
        }
        Task.WaitAll(threadList.ToArray());
        
        foreach(Task thing in threadList)
            thing.Dispose();

        finalImage.Bitmap.Save(savePath);
        finalImage.Dispose();
        //averageCurrentImageTime.Stop();
    }

    private void RunByThread(DirectBitmap finalImage, int startX, int endX)
    {
        for (int dx = startX; dx <= endX; dx++)
        {
            for (int dy = 0; dy < finalImage.Height; dy++)
            {
                finalImage.SetPixel(dx, dy, GetColorAverage(dx, dy));
            }
        }
    }

    private Color GetColorAverage(int dx, int dy)
    {
        int origDX = dx;
        int origDY = dy;
        int countOffset = 0;
        int colorA=0;
        int colorR=0;
        int colorG=0;
        int colorB=0;
        
        Color thisColor;
        int count = images.Count();
        for(int thisImageIndex = 0; thisImageIndex<count; thisImageIndex++)
        {
            DirectBitmap image = images[thisImageIndex];
            dx = origDX;
            dy = origDY;

            dx -= widthOffset[thisImageIndex];
            dy -= heightOffset[thisImageIndex];
            if(dx<0||dy<0 || image.Height < dy+1 || image.Width < dx+1)
            {
                countOffset += avgOnlyExistingPixels ? 1 : 0; 
                continue;
            }
            thisColor = image.GetPixel(dx, dy); 

            if(thisColor.A == 0)
            {
                countOffset += avgOnlyExistingPixels ? 1 : 0; 
                continue;
            }
            colorA += thisColor.A;
            colorR += thisColor.R;
            colorG += thisColor.G;
            colorB += thisColor.B;
        }
        int divisor = images.Count()==countOffset ? 1 : images.Count()-countOffset;

        return Color.FromArgb(colorA/divisor, colorR/divisor, colorG/divisor, colorB/divisor);
    }

    private void ClearSkinFolder()
    {
        DirectoryInfo di = new DirectoryInfo(Path.Combine(skinFolderPath, "!!Most Average Skin"));
        foreach(FileInfo file in di.GetFiles())
            file.Delete();
    }

    private void ClearImages()
    {
        foreach(DirectBitmap img in images)
            img.Dispose();
        
        images.Clear();
    }

    private void ClearTheNumbers()
    {
        foreach(List<DirectBitmap> imgList in hitCircleNumbers)
            foreach(DirectBitmap img in imgList)
                img.Dispose();
        
        foreach(List<DirectBitmap> imgList in comboNumbers)
            foreach(DirectBitmap img in imgList)
                img.Dispose();
        
        foreach(List<DirectBitmap> imgList in scoreNumbers)
            foreach(DirectBitmap img in imgList)
                img.Dispose();

        hitCircleNumbers.Clear();
        comboNumbers.Clear();
        scoreNumbers.Clear();

        for (int i = 0; i <= 14; i++)
        {
            hitCircleNumbers.Add(new List<DirectBitmap>());
            scoreNumbers.Add(new List<DirectBitmap>());
            comboNumbers.Add(new List<DirectBitmap>());
        }
    }
}