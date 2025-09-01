using System.Runtime.InteropServices;

public static class BacklightDriver
{
    [DllImport("legion_rgb_driver.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int set_colors(
        byte r1, byte g1, byte b1,
        byte r2, byte g2, byte b2,
        byte r3, byte g3, byte b3,
        byte r4, byte g4, byte b4);

    [DllImport("legion_rgb_driver.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int set_brightness(int level);

    [DllImport("legion_rgb_driver.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int init_keyboard();

    [DllImport("legion_rgb_driver.dll")]
    private static extern int set_effect_static();



    public static void InitializeKeybord()
    {
        int init_result = init_keyboard();
        Console.WriteLine("init: " + init_result);
    }

    public static void SetKeyboardEffect()
    {
        int set_effect_result = set_effect_static();
        Console.WriteLine("set effect: " + set_effect_result);
    }

    public static void SetBacklightBrightness(int _level)
    {
        if (_level < 1 || _level > 2)
        {
            throw new IndexOutOfRangeException("Invalid brightness value! Can be 1 or 2 only!");
        }

        int bResult = set_brightness(2);
        Console.WriteLine($"set_brightness returned {bResult}");
    }

    public static void SetBacklightColour(Color zone1Colour, Color zone2Colour, Color zone3Colour, Color zone4Colour)
    {

        int colResult = set_colors(zone1Colour.R, zone1Colour.G, zone1Colour.B,
                                   zone2Colour.R, zone2Colour.G, zone2Colour.B,
                                   zone3Colour.R, zone3Colour.G, zone3Colour.B,
                                   zone4Colour.R, zone4Colour.G, zone4Colour.B);

        //Console.WriteLine($"set_colors returned {colResult}");
    }
}