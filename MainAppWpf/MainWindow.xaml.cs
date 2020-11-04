using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using PluginInterface;
using Image = System.Drawing.Image;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace MainAppWpf {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private readonly Dictionary<string, IPlugin> _plugins = new Dictionary<string, IPlugin>();

        public MainWindow() {
            InitializeComponent();
            FindPlugins();
            CreatePluginsMenu();
        }

        private void FindPlugins() {
            // папка с плагинами
            string folder = System.AppDomain.CurrentDomain.BaseDirectory;

            string[] files;
            if (File.Exists(folder + "config.ini")) {
                var temp = new List<string>();
                using (var streamReader = new StreamReader(folder + "config.ini")) {
                    string line = streamReader.ReadLine();
                    if (line != "Auto") {
                        temp.Add(line);
                        while ((line = streamReader.ReadLine()) != null) {
                            temp.Add(line);
                        }

                        files = temp.ToArray();
                    } else {
                        // все dll-файлы в этой папке
                        files = Directory.GetFiles(folder, "*.dll");
                    }
                }
            } else {
                // все dll-файлы в этой папке
                files = Directory.GetFiles(folder, "*.dll");
            }

            foreach (string file in files)
                try {
                    Assembly assembly = Assembly.LoadFile(file);

                    foreach (Type type in assembly.GetTypes()) {
                        Type iface = type.GetInterface("PluginInterface.IPlugin");

                        if (iface != null) {
                            var plugin = (IPlugin) Activator.CreateInstance(type);
                            _plugins.Add(plugin.Name, plugin);
                        }
                    }
                } catch (Exception ex) {
                    MessageBox.Show("Ошибка загрузки плагина\n" + ex.Message);
                }
        }

        private void CreatePluginsMenu() {
            foreach (var plugin in _plugins) {
                var newItem = new MenuItem {Header = plugin.Key};
                newItem.Click += OnPluginClick;
                filters.Items.Add(newItem);
            }
        }

        private void OnPluginClick(object sender, EventArgs args) {
            IPlugin plugin = _plugins[((MenuItem) sender).Header.ToString()];
            Bitmap temp = ConvertToBitmap((BitmapSource) pictureBox.Source);
            plugin.Transform(temp);
            pictureBox.Source = ConvertToBitmapImage(temp);
        }

        public static Bitmap ConvertToBitmap(BitmapSource bitmapSource) {
            int width = bitmapSource.PixelWidth;
            int height = bitmapSource.PixelHeight;
            int stride = width * ((bitmapSource.Format.BitsPerPixel + 7) / 8);
            IntPtr memoryBlockPointer = Marshal.AllocHGlobal(height * stride);
            bitmapSource.CopyPixels(new Int32Rect(0, 0, width, height), memoryBlockPointer, height * stride, stride);
            var bitmap = new Bitmap(width, height, stride, PixelFormat.Format32bppPArgb, memoryBlockPointer);
            return bitmap;
        }

        public BitmapImage ConvertToBitmapImage(Bitmap src) {
            MemoryStream ms = new MemoryStream();
            ((System.Drawing.Bitmap) src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        private void OpenImage_OnClick(object sender, RoutedEventArgs e) {
            var openFileDialog = new OpenFileDialog {
                Filter = "Image files (*.bmp, *.jpg, *.png)|*.bmp;*.jpg;*.png"
            };

            if (openFileDialog.ShowDialog() == true) {
                pictureBox.Source = new BitmapImage(new Uri(openFileDialog.FileName));
            }
        }

        private void Help_OnClick(object sender, RoutedEventArgs e) {
            var helpWindow = new HelpWindow(_plugins);
            helpWindow.ShowDialog();
        }
    }
}