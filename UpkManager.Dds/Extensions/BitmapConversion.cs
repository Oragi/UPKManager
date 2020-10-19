using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

public static class BitmapConversion
{
    // https://stackoverflow.com/a/6775114/
    public static Bitmap ToWinFormsBitmap(this BitmapSource bitmapsource)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            BitmapEncoder enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bitmapsource));
            enc.Save(stream);

            using (var tempBitmap = new Bitmap(stream))
            {
                // According to MSDN, one "must keep the stream open for the lifetime of the Bitmap."
                // So we return a copy of the new bitmap, allowing us to dispose both the bitmap and the stream.
                return new Bitmap(tempBitmap);
            }
        }
    }

    public static BitmapSource ToWpfBitmap(this Bitmap bitmap)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            bitmap.Save(stream, ImageFormat.Bmp);

            stream.Position = 0;
            BitmapImage result = new BitmapImage();
            result.BeginInit();
            // According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
            // Force the bitmap to load right now so we can dispose the stream.
            result.CacheOption = BitmapCacheOption.OnLoad;
            result.StreamSource = stream;
            result.EndInit();
            result.Freeze();
            return result;
        }
    }
}

public static class FontConversion
{
    public static PrivateFontCollection Load(MemoryStream stream)
    {
        byte[] streamData = new byte[stream.Length];
        stream.Read(streamData, 0, streamData.Length);
        IntPtr data = Marshal.AllocCoTaskMem(streamData.Length); // Very important.
        Marshal.Copy(streamData, 0, data, streamData.Length);
        PrivateFontCollection pfc = new PrivateFontCollection();
        pfc.AddMemoryFont(data, streamData.Length);
        // MemoryFonts.Add(pfc); // Your own collection of fonts here.
        Marshal.FreeCoTaskMem(data); // Very important.
        return pfc;
    }

    /* public static System.Windows.Media.FontFamily LoadFont(int fontId)
    {
        if (!Exists(fontId))
        {
            return null;
        }
        // NOTE:
        // This is basically how you convert a System.Drawing.FontFamily to System.Windows.Media.FontFamily, using PrivateFontCollection.
        return new System.Windows.Media.FontFamily(MemoryFonts[fontId].Families[0].Name);
    } */
}