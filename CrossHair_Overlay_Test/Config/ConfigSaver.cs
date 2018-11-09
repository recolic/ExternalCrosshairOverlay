using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace CrossHair_Overlay_Test.Config
{
    public class ConfigSaver
    {
        public string DefaultSavePath { get; private set; } = "%APPDATA%\\CrosshairOverlay";

        public ConfigSaver()
        {
            if (!Directory.Exists(DefaultSavePath))
            {
                Directory.CreateDirectory(DefaultSavePath);
            }
        }

        public void SaveConfig(OverlayConfig overlayConfig)
        {
            var targetFile = DefaultSavePath + "\\" + overlayConfig.ProcessName + ".eoc";

            if (File.Exists(targetFile))
            {
                File.Delete(targetFile);
            }

            using (var fStream = File.Create(targetFile))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(fStream, overlayConfig);
                fStream.Close();
            }
        }

        public OverlayConfig GetConfig(string processName)
        {
            var retThis = new OverlayConfig();
            var targetFile = DefaultSavePath + "\\" + processName + ".eoc";
            if (!File.Exists(targetFile))
            {
                retThis.ProcessName = processName;
                return retThis;
            }

            using (var fStream = File.OpenRead(targetFile))
            {
                var formatter = new BinaryFormatter();
                retThis = (OverlayConfig)formatter.Deserialize(fStream);
                fStream.Close();
            }

            return retThis;
        }
    }
}
