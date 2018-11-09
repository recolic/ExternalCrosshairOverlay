using System.Windows.Media;

namespace CrossHair_Overlay_Test.Config
{
    public class OverlayConfig
    {
        public string ProcessName { get; set; }
        public string CrosshairFileLocation { get; set; } = "";

        public int OffsetX { get; set; } = 0;
        public int OffsetY { get; set; } = 0;

        public Color CrosshairColor { get; set; } = Colors.Black;
        public int CrosshairScale { get; set; } = 2;
        public int CrosshairTransparency { get; set; } = 100;
    }
}
