using Microsoft.Win32;
using PPMParser.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PPMParser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ParseFile(string fileName)
        {
            try
            {
                Bitmap bitmap = Parser.ParsePPMFileToBitMap(fileName);
                BitmapImage image = ConvertBitmapToBitmapImage(bitmap);
                Image.Source = image;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message,"",MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }

        private BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();

                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Image.LayoutTransform = new ScaleTransform(e.NewValue, e.NewValue);
        }

        private void OpenFileJPGClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "pliki jpg|*.jpg";
            if (openFileDialog.ShowDialog() == true)
            {
                Image.Source = new BitmapImage(new Uri(openFileDialog.FileName));
            }
        }

        private void OpenFilePPMClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "pliki ppm|*.ppm";
            if (openFileDialog.ShowDialog() == true)
            {
                ParseFile(openFileDialog.FileName);
            }
        }

        private void SaveFile(object sender, RoutedEventArgs e)
        {
            BitmapImage image = Image.Source as BitmapImage;
            if (image == null)
            {
                MessageBox.Show("Musisz najpierw załadować zdjęcie");
                return;
            }
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "JPEG Image|*.jpg"
            };
            if (dialog.ShowDialog() == true)
            {
                var quantity = (int)Quantity.Value;
                var save = new JpegBitmapEncoder();
                save.QualityLevel = quantity;
                save.Frames.Add(BitmapFrame.Create(image));
                using (var stream = dialog.OpenFile())
                {
                    save.Save(stream);
                    stream.Close();
                }
                MessageBox.Show("Zapisano pomyślnie");
            }       

        }     
    }
}
