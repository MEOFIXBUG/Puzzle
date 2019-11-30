using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace puzzleABC
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
        const int startX = 30;
        const int startY = 30;
        int height = 120;
        int width = 120;
        const int mRows = 3;
        const int mCols = 3;
        CroppedBitmap[] objImg;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            objImg = new CroppedBitmap[9];
            var screen = new OpenFileDialog();

            if (screen.ShowDialog() == true)
            {
                var source = new BitmapImage(
                   new Uri(screen.FileName, UriKind.Absolute));

                //previewImage.Width = 400;
                //previewImage.Height = 400;
                previewImage.Source = source;

                Canvas.SetLeft(previewImage, 440);
                //Canvas.SetTop(previewImage, 10);
                CutImage(source);
                SetPiecesPosition();
                DrawPuzzleBoard();
            }

        }
        private void CutImage(BitmapImage source)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (!((i == 2) && (j == 2)))
                    {
                        height = (int)source.Height / 3;
                        width = (int)source.Width / 3;
                        //Debug.WrbiteLine($"Len = {len}");
                        var rect = new Int32Rect(j * width, i * height, width, height);
                        var cropBitmap = new CroppedBitmap(source,
                            rect);
                        objImg[3 * i + j] = cropBitmap;
                    }
                }
            }
        }

        private void DrawPuzzleBoard()
        {
            for (int j = 0; j <= mCols; j++)
            {
                Point point1 = new Point(startX + j * width, startY);
                Point point2 = new Point(startX + j * width, startY + mRows * height);
                Line myline = new Line();
                myline.X1 = point1.X;
                myline.Y1 = point1.Y;
                myline.X2 = point2.X;
                myline.Y2 = point2.Y;
                myline.Stroke = Brushes.DarkGray;
                myCanvas.Children.Add(myline);
            }

            for (int i = 0; i <= mRows; i++)
            {
                Point point1 = new Point(startX, startY + i * height);
                Point point2 = new Point(startX + mCols * width, startY + i * height);
                Line myline = new Line();
                myline.X1 = point1.X;
                myline.Y1 = point1.Y;
                myline.X2 = point2.X;
                myline.Y2 = point2.Y;
                myline.Stroke = Brushes.DarkGray;
                myCanvas.Children.Add(myline);
            }

        }

        private void SetPiecesPosition()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (!((i == 2) && (j == 2)))
                    {
                        var cropImage = new Image();
                        cropImage.Stretch = Stretch.Fill;
                        cropImage.Width = width;
                        cropImage.Height = height;
                        cropImage.Source = objImg[3 * i + j];
                        myCanvas.Children.Add(cropImage);
                        Canvas.SetLeft(cropImage, startX + j * width);
                        Canvas.SetTop(cropImage, startY + i * height);
                    }
                }
            }
        }
    }
}
