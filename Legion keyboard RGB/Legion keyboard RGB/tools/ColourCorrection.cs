using System.Drawing;

internal class ColourCorrection
{
    public static Color AdjustSaturationAndLightness(Color color, float saturationFactor, float lightnessFactor)
    {
        // Convert RGB -> HSL
        float h, s, l;
        RgbToHsl(color, out h, out s, out l);

        // Apply correction
        s *= saturationFactor;
        l *= lightnessFactor;

        // Clamp to [0,1]
        s = Math.Min(1, Math.Max(0, s));
        l = Math.Min(1, Math.Max(0, l));

        // Convert back HSL -> RGB
        return HslToRgb(h, s, l);
    }

    private static void RgbToHsl(Color color, out float h, out float s, out float l)
    {
        float r = color.R / 255f;
        float g = color.G / 255f;
        float b = color.B / 255f;

        float max = Math.Max(r, Math.Max(g, b));
        float min = Math.Min(r, Math.Min(g, b));
        l = (max + min) / 2f;

        if (max == min)
        {
            h = 0; s = 0; // Achromatic
        }
        else
        {
            float d = max - min;
            s = l > 0.5f ? d / (2f - max - min) : d / (max + min);

            if (max == r)
                h = (g - b) / d + (g < b ? 6f : 0f);
            else if (max == g)
                h = (b - r) / d + 2f;
            else
                h = (r - g) / d + 4f;

            h /= 6f;
        }
    }

    private static Color HslToRgb(float h, float s, float l)
    {
        float r, g, b;

        if (s == 0)
        {
            r = g = b = l; // Achromatic
        }
        else
        {
            Func<float, float, float, float> HueToRgb = (p, q, t) =>
            {
                if (t < 0) t += 1;
                if (t > 1) t -= 1;
                if (t < 1f / 6f) return p + (q - p) * 6f * t;
                if (t < 1f / 2f) return q;
                if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
                return p;
            };

            float q = l < 0.5f ? l * (1 + s) : l + s - l * s;
            float p = 2 * l - q;
            r = HueToRgb(p, q, h + 1f / 3f);
            g = HueToRgb(p, q, h);
            b = HueToRgb(p, q, h - 1f / 3f);
        }

        return Color.FromArgb(
            255,
            (int)Math.Round(r * 255),
            (int)Math.Round(g * 255),
            (int)Math.Round(b * 255)
        );
    }
}