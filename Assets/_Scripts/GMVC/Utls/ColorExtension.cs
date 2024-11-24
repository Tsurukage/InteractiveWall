using System.Drawing;
using System.Text;

namespace GMVC.Utls
{
    public static class ColorExtension
    {
            public static StringBuilder Sb(this object obj) => new(obj.ToString());
            public static StringBuilder Bold(this StringBuilder text)
            {
                text.Insert(0, "<b>");
                text.Append("</b>");
                return text;
            }
            public static StringBuilder Color(this StringBuilder text, Color color)
            {
                var colorText = $"<color={HexConverter(color)}>";
                text.Insert(0, colorText);
                text.Append("</color>");
                return text;
            }

            static string HexConverter(Color c) => "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
    }
}