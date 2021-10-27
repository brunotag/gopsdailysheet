using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GopsDailySheet
{
    public static class FontExtensions
    {
        public static Font CloneWithNewSize(this Font font, float newSize)
        {
            return new Font(font.FontFamily, newSize, font.Style, font.Unit, font.GdiCharSet, font.GdiVerticalFont);
        }
    }
}
