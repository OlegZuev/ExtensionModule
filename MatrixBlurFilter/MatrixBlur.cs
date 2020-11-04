using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using PluginInterface;

namespace MatrixBlurFilter {
    [Version(1, 0)]
    public class MatrixBlur : IPlugin {
        public string Name => "Матричный фильтр размытия";
        public string Author => "Olegase";
        public void Transform(Bitmap app) {
            Window pluginWindow = new PluginWindow(app);
            pluginWindow.ShowDialog();
        }
    }
}