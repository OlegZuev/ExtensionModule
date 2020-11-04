using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MatrixBlurFilter {
    /// <summary>
    /// Interaction logic for PluginWindow.xaml
    /// </summary>
    public partial class PluginWindow : Window {
        private Bitmap _image;
        private const int COLUMNS_COUNT = 30;
        private static double[,] _filterMatrix;

        public PluginWindow(Bitmap image) {
            InitializeComponent();
            _image = image;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e) {
            int dim = (int) sliderMatrixDim.Value;
            CreateFilterMatrix(dim);
            TransformBitmap(_image, dim);
        }

        private static void CreateFilterMatrix(int dim) {
            _filterMatrix = new double[dim, dim];
            for (int i = 0; i < _filterMatrix.GetLength(0); i++) {
                for (int j = 0; j < _filterMatrix.GetLength(1); j++) {
                    _filterMatrix[i, j] = 1.0 / (dim * dim);
                }
            }
        }

        private static Color MulMatrix(Color[,] imageMatrix, int dim) {
            double red = 0;
            double green = 0;
            double blue = 0;
            for (int col = 0; col < dim; col++) {
                for (int row = 0; row < dim; row++) {
                    red += imageMatrix[col, row].R * _filterMatrix[row, col];
                    green += imageMatrix[col, row].G * _filterMatrix[row, col];
                    blue += imageMatrix[col, row].B * _filterMatrix[row, col];
                }
            }

            if (red < 0) red = 0;
            if (red > 255) red = 255;
            if (green < 0) green = 0;
            if (green > 255) green = 255;
            if (blue < 0) blue = 0;
            if (blue > 255) blue = 255;

            Color distColor = Color.FromArgb((int) red, (int) green, (int) blue);
            return distColor;
        }

        private static Color[,] GetMatrixByCenter(Color[,] source, int i, int j, int dim) {
            var imageMatrix = new Color[dim, dim];
            int halfDim = dim / 2;
            for (int x = -halfDim; x < halfDim + 1; x++) {
                for (int y = -halfDim; y < halfDim + 1; y++) {
                    imageMatrix[x + halfDim, y + halfDim] = source[i + x, j + y];
                }
            }

            return imageMatrix;
        }

        private static void ColumnFilter(Color[,] source, Color[,] dist, int i, int dim) {
            int halfDim = dim / 2;
            for (int j = halfDim; j < source.GetLength(1) - halfDim; j++) {
                Color[,] imageMatrix = GetMatrixByCenter(source, i, j, dim);
                Color distColor = MulMatrix(imageMatrix, dim);
                dist[i - halfDim, j - halfDim] = distColor;
            }
        }

        private void TransformBitmap(Bitmap source, int dim) {
            var sourceColors = new Color[source.Width, source.Height];
            for (int col = 0; col < source.Width; col++) {
                for (int row = 0; row < source.Height; row++) {
                    sourceColors[col, row] = source.GetPixel(col, row);
                }
            }

            int halfDim = dim / 2;
            sourceColors = CreateExtendedBitmap(sourceColors, halfDim);

            var distColors = new Color[source.Width, source.Height];
            int maxThread = 8; // Максимальное количество потоков
            // Массив запущенных потоков
            var threads = new Thread[maxThread];
            int i = halfDim;
            while (i < sourceColors.GetLength(0) - COLUMNS_COUNT - halfDim) {
                // Пробегаем по массиву потоков и заменяем закончившие новыми
                for (int threadIndex = 0;
                     threadIndex < maxThread && i < sourceColors.GetLength(0) - COLUMNS_COUNT - halfDim;
                     threadIndex++) {
                    if (threads[threadIndex] == null || threads[threadIndex].ThreadState == ThreadState.Stopped) {
                        int index = i;
                        threads[threadIndex] = new Thread(() => {
                            // Это код запускаемого потока
                            for (int col = index; col < index + COLUMNS_COUNT; col++) {
                                ColumnFilter(sourceColors, distColors, col, dim);
                            }
                        });
                        threads[threadIndex].Start();
                        i += COLUMNS_COUNT;
                    }
                }
            }

            // Пробегаем оставшиеся столбцы <=30
            for (int col = i; col < sourceColors.GetLength(0) - halfDim; col++) {
                ColumnFilter(sourceColors, distColors, col, dim);
            }

            for (int col = 0; col < source.Width; col++) {
                for (int row = 0; row < source.Height; row++) {
                    source.SetPixel(col, row, distColors[col, row]);
                }
            }
        }

        private static Color[,] CreateExtendedBitmap(Color[,] sourceColors, int halfDim) {
            int width = sourceColors.GetLength(0);
            int height = sourceColors.GetLength(1);
            int tempHeight = height + 2 * halfDim, tempWidth = width + 2 * halfDim;
            var extSourceColors = new Color[width + halfDim * 2, height + halfDim * 2];
            //заполнение временного расширенного изображения
            //углы
            for (int i = 0; i < halfDim; i++) {
                for (int j = 0; j < halfDim; j++) {
                    extSourceColors[i, j] = sourceColors[0, 0];
                    extSourceColors[i, tempHeight - 1 - j] = sourceColors[0, height - 1];
                    extSourceColors[tempWidth - 1 - i, j] = sourceColors[width - 1, 0];
                    extSourceColors[tempWidth - 1 - i, tempHeight - 1 - j] = sourceColors[width - 1, height - 1];
                }
            }

            //крайние верхняя и нижняя стороны
            for (int i = halfDim; i < tempWidth - halfDim; i++) {
                for (int j = 0; j < halfDim; j++) {
                    extSourceColors[i, j] = sourceColors[i - halfDim, j];
                    extSourceColors[i, tempHeight - 1 - j] = sourceColors[i - halfDim, height - 1 - j];
                }
            }

            //крайние левая и правая стороны
            for (int i = 0; i < halfDim; i++) {
                for (int j = halfDim; j < tempHeight - halfDim; j++) {
                    extSourceColors[i, j] = sourceColors[i, j - halfDim];
                    extSourceColors[tempWidth - 1 - i, j] = sourceColors[width - 1 - i, j - halfDim];
                }
            }

            //центр
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    extSourceColors[i + halfDim, j + halfDim] = sourceColors[i, j];
                }
            }

            return extSourceColors;
        }
    }
}