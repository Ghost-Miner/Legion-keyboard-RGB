using System.Runtime.InteropServices;
using System.Drawing.Imaging;

public static class FastAverager
{
    /// <summary>
    /// Returns the average colors of 'slices' vertical regions of the frame.
    /// No unsafe code, no extra bitmaps, single LockBits.
    /// </summary>
    /// <param name="_frame">Input bitmap (ideally already 32bppPArgb for best speed).</param>
    /// <param name="_slices">Number of vertical slices (default 4).</param>
    /// <param name="_excludeBottom">Pixels to exclude from the bottom (e.g., taskbar height).</param>
    public static Color[] GetSliceAverages(Bitmap _frame, int _slices = 4, int _excludeBottom = 0)
    {
        //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        //stopwatch.Start();

        if (_frame == null) throw new ArgumentNullException(nameof(_frame));
        if (_slices <= 0) throw new ArgumentOutOfRangeException(nameof(_slices));
        if (_excludeBottom < 0) throw new ArgumentOutOfRangeException(nameof(_excludeBottom));

        // Make sure we will lock a region with positive height
        int lockHeight = _frame.Height - _excludeBottom;
        if (lockHeight <= 0)
            throw new ArgumentException("excludeBottom is larger than or equal to frame height.");

        // For consistent, fast access use 32bpp (P)ARGB. If your capture already provides this, no conversion happens.
        Bitmap source = _frame;
        bool createdTemp = false;

        PixelFormat targetFmt = PixelFormat.Format32bppPArgb; // fast blend, common in desktop capture
        if (source.PixelFormat != targetFmt && source.PixelFormat != PixelFormat.Format32bppArgb)
        {
            // Convert once here. For best performance, capture directly in 32bppPArgb and skip this branch.
            source = new Bitmap(_frame.Width, _frame.Height, targetFmt);
            using (var g = Graphics.FromImage(source))
            {
                g.DrawImage(_frame, 0, 0, _frame.Width, _frame.Height);
            }
            createdTemp = true;
        }

        // Prepare accumulators
        long[] sumR = new long[_slices];
        long[] sumG = new long[_slices];
        long[] sumB = new long[_slices];
        long[] count = new long[_slices];

        Rectangle rect = new Rectangle(0, 0, source.Width, lockHeight);
        BitmapData bitmapData = null;

        try
        {
            // Lock once; use 32bpp to simplify 4-byte step (B,G,R,A)
            bitmapData = source.LockBits(rect, ImageLockMode.ReadOnly,
                              source.PixelFormat == PixelFormat.Format32bppArgb ? PixelFormat.Format32bppArgb
                                                                            : PixelFormat.Format32bppPArgb);

            IntPtr basePtr = bitmapData.Scan0;
            int stride = bitmapData.Stride;        // bytes per row (can be > width*4, do not assume tight packing)
            int width = rect.Width;
            int height = rect.Height;

            // Single pass over all pixels, binning into 'slices'
            // sliceIndex = x * slices / width guarantees full coverage without gaps
            for (int y = 0; y < height; y++)
            {
                IntPtr rowPtr = IntPtr.Add(basePtr, y * stride);
                for (int x = 0; x < width; x++)
                {
                    int slice = (int)((long)x * _slices / width);

                    // Compute pixel address: rowPtr + x*4
                    IntPtr pxPtr = IntPtr.Add(rowPtr, x * 4);

                    byte b = Marshal.ReadByte(pxPtr);         // B
                    byte g = Marshal.ReadByte(IntPtr.Add(pxPtr, 1)); // G
                    byte r = Marshal.ReadByte(IntPtr.Add(pxPtr, 2)); // R
                    // byte a = Marshal.ReadByte(IntPtr.Add(pxPtr, 3)); // A (ignored)

                    sumR[slice] += r;
                    sumG[slice] += g;
                    sumB[slice] += b;
                    count[slice]++;
                }
            }
        }
        finally
        {
            if (bitmapData != null) source.UnlockBits(bitmapData);
            if (createdTemp) source.Dispose();
        }

        // Build result
        var result = new Color[_slices];
        for (int i = 0; i < _slices; i++)
        {
            if (count[i] == 0)
            {
                result[i] = Color.Black; // or keep previous, or throw
            }
            else
            {
                result[i] = Color.FromArgb(
                    (int)(sumR[i] / count[i]),
                    (int)(sumG[i] / count[i]),
                    (int)(sumB[i] / count[i]));
            }
        }

        //stopwatch.Stop();
        //double ticks = stopwatch.ElapsedTicks;
        //double seconds = ticks / System.Diagnostics.Stopwatch.Frequency;
        //double milliseconds = (ticks / System.Diagnostics.Stopwatch.Frequency) * 1000;
        //double nanoseconds = (ticks / System.Diagnostics.Stopwatch.Frequency) * 1000000000;

        //Console.WriteLine("stopwatch ns: " + nanoseconds + " us: " + (nanoseconds / 1000) + " ms: " + milliseconds + " s: " + seconds + " t: " + ticks);
        //Program.avgExecTimeItems.Add(nanoseconds);

        return result;
    }
}
