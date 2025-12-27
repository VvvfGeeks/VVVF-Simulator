using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace VvvfSimulator.Generation.Video.Fonts
{
    public class Manager
    {
        private static readonly FontFamily GeneralFont = new(GenericFontFamilies.Monospace);
        public static FontFamily DSEG14ModernItalic { get; set; } = GeneralFont;
        public static FontFamily DSEG7ModernItalic { get; set; } = GeneralFont;
        public static FontFamily FugazOne { get; set; } = GeneralFont;
        public static FontFamily Arial { get; set; } = GeneralFont;
        private static void Load(Stream? Reader, out FontFamily Font, out nint RamAddress)
        {
            if (Reader == null) throw new Exception();
            int FontDataLen = (int)Reader.Length;
            RamAddress = Marshal.AllocHGlobal(FontDataLen);
            for(int Offset = 0; Offset < FontDataLen; Offset++)
            {
                Marshal.WriteByte(RamAddress, Offset, (byte)Reader.ReadByte());
            }
            PrivateFontCollection FontCollection = new();
            FontCollection.AddMemoryFont(RamAddress, FontDataLen);
            Font = FontCollection.Families[0];
        }
        private static void Dispose(nint RamAddress)
        {
            Marshal.FreeHGlobal(RamAddress);
        }

        private static List<nint> FontAddressList = [];
        public static void Load()
        {
            Load(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Generation.Video.Fonts.DSEG14Modern-Italic.ttf"), out FontFamily _DSEG14ModernItalicFont, out nint _DSEG14ModernItalicFontAddress);
            DSEG14ModernItalic = _DSEG14ModernItalicFont;
            FontAddressList.Add(_DSEG14ModernItalicFontAddress);

            Load(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Generation.Video.Fonts.DSEG7Modern-Italic.ttf"), out FontFamily _DSEG7ModernItalicFont, out nint _DSEG7ModernItalicFontAddress);
            DSEG7ModernItalic = _DSEG7ModernItalicFont;
            FontAddressList.Add(_DSEG7ModernItalicFontAddress);

            Load(Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.Generation.Video.Fonts.FugazOne-Regular.ttf"), out FontFamily _FugazOneFont, out nint _FugazOneFontAddress);
            FugazOne = _FugazOneFont;
            FontAddressList.Add(_FugazOneFontAddress);
        }
        public static void Dispose()
        {
            for(int i = 0; i < FontAddressList.Count; i++)
            {
                Dispose(FontAddressList[i]);
            }
        }

    }
}

