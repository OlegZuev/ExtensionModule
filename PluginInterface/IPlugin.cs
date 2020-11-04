using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace PluginInterface {
    public interface IPlugin {
        string Name { get; }
        string Author { get; }
        void Transform(Bitmap app);
    }
}