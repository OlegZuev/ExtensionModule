using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PluginInterface;

namespace MainAppWpf {
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : Window {
        public HelpWindow(Dictionary<string, IPlugin> plugins) {
            InitializeComponent();
            InitializeListOfPlugins(plugins);
        }

        private void InitializeListOfPlugins(Dictionary<string, IPlugin> plugins) {
            foreach (var plugin in plugins) {
                var attrs = plugin.Value.GetType().GetCustomAttributes(false);
                foreach (VersionAttribute attr in attrs) {
                    listOfPlugins.Items.Add(new ListViewItem {
                        Content =
                            $"Name: {plugin.Value.Name}; Author: {plugin.Value.Author}; Version: {attr.Major}.{attr.Minor}"
                    });
                }
            }
        }
    }
}