using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        int[] map;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            objImg = new CroppedBitmap[9];
            map = new int[mRows * mCols];
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
                Shuffle();
            }

        }
        private void CutImage(BitmapImage source)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (!((i == mRows - 1) && (j == mCols - 1)))
                    {
                        height = (int)source.Height / 3;
                        width = (int)source.Width / 3;
                        //Debug.WrbiteLine($"Len = {len}");
                        var rect = new Int32Rect(j * width, i * height, width, height);
                        var cropBitmap = new CroppedBitmap(source, rect);
                        objImg[3 * i + j] = cropBitmap;
                        map[3 * i + j] = (3 * i + j) % (mRows * mCols);
                    }
                }
            }
            map[3 * (mRows-1) + mCols -1] = -1;
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
            for (int i = 0; i < mRows; i++)
            {
                for (int j = 0; j < mCols; j++)
                {
                    
                    if (!((i == mRows - 1) && (j == mCols - 1)))
                    {
                        var cropImage = new Image();
                        cropImage.Stretch = Stretch.Fill;
                        cropImage.Width = width;
                        cropImage.Height = height;
                        cropImage.Source = objImg[mRows * i + j];

                    }
                }
            }
        }

        private void Shuffle()
        {
            Random r = new Random();
            int n = mRows * mCols - 1;
            for (int i = 0; i < 5; i++)
            {
                int i1 = 1, i2 = 1;
                while (i1 == i2)
                {
                    i1 = r.Next(0, n-2);
                    i2 = r.Next(0, n-2);
                }

                int temp = map[i1];
                map[i1] = map[i2];
                map[i2] = temp;
            }

            for (int i = 0; i < mRows; i++)
            {
                for (int j = 0; j < mCols; j++)
                    if (!((i == mRows - 1) && (j == mCols - 1)))
                    {
                        var cropImage = new Image();
                        cropImage.Stretch = Stretch.Fill;
                        cropImage.Width = width;
                        cropImage.Height = height;
                        cropImage.Source = objImg[map[3 * i + j]];
                        myCanvas.Children.Add(cropImage);
                        Canvas.SetLeft(cropImage, startX + j * width);
                        Canvas.SetTop(cropImage, startY + i * height);
                        cropImage.MouseLeftButtonDown += beginDrag;
                        cropImage.PreviewMouseLeftButtonUp += endDrag;
                        cropImage.Tag = 3 * i + j;
                    }
            }

        }

        Tuple<int, int> lastCell;
        bool gameOver = false;

        bool canMove(int x1, int y1, int x2, int y2)
        {
            if (x1 < 0 || x1 >= mCols) return false;
            if (x2 < 0 || x2 >= mCols) return false;
            if (y1 < 0 || y1 >= mRows) return false;
            if (y2 < 0 || y2 >= mRows) return false;

            if (map[3*y1 + x1] == -1 && ((Math.Abs(x1 - x2) == 0 && Math.Abs(y1 - y2) == 1) ^ (Math.Abs(x1 - x2) == 1 && Math.Abs(y1 - y2) == 0)))
            {
                return true;
            }
            return false;
        }
        bool checkWin()
        {
            int size = mRows * mCols;
            for (int i = 0; i < size - 1; i++)
            {
                if (map[i] != i) return false;
            }
            return true;
        }
        private void endDrag(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(this);
            var cellX = (int)(position.X - startX) / width;
            var cellY = (int)(position.Y - startY) / height;
            isDragging = false;
            if (canMove(cellX, cellY, lastCell.Item1, lastCell.Item2))
            {
                Canvas.SetLeft(_selectedCropImage, startX + cellX * width);
                Canvas.SetTop(_selectedCropImage, startY + cellY * height);
                if (map[3 * lastCell.Item2 + lastCell.Item1] != -1)
                {
                    map[3 * cellY + cellX] = map[3 * lastCell.Item2 + lastCell.Item1];
                    map[3 * lastCell.Item2 + lastCell.Item1] = -1;

                }
                if (checkWin() == true)
                {
                    MessageBox.Show("Win");

                } else
                {
                    Debug.WriteLine(map.ToString());
                }
            }
            else
            {
                if (checkWin() == true)
                {
                    MessageBox.Show("Win");

                }
                Canvas.SetLeft(_selectedCropImage, startX + lastCell.Item1 * width);
                Canvas.SetTop(_selectedCropImage, startY + lastCell.Item2 * height);
            }



        }

        bool isDragging = false;

        Image _selectedCropImage = null;
        Point _lastPosition;

        private void beginDrag(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(this);
            var cellX = (int)(position.X - startX) / width;
            var cellY = (int)(position.Y - startY) / height;
            if (cellX < mCols && cellY < mRows)
            {
                lastCell = new Tuple<int, int>(cellX, cellY);
            }
            // this.CaptureMouse();
            _selectedCropImage = sender as Image;
            _lastPosition = e.GetPosition(this);
            isDragging = true;
        }
        private void Mouse_Move(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var position = e.GetPosition(this);
                var dx = position.X - _lastPosition.X;
                var dy = position.Y - _lastPosition.Y;

                if (position.X < startX - 20 || position.Y < startY - 10 || position.X > startX + (mRows * height) + 2 * height || position.Y > startY + (mCols * width) + 2 * width)
                {
                    isDragging = false;
                    Canvas.SetLeft(_selectedCropImage, startX + lastCell.Item1 * width);
                    Canvas.SetTop(_selectedCropImage, startY + lastCell.Item2 * height);
                }
                else
                {
                    var lastLeft = Canvas.GetLeft(_selectedCropImage);
                    var lastTop = Canvas.GetTop(_selectedCropImage);
                    Canvas.SetLeft(_selectedCropImage, lastLeft + dx);
                    Canvas.SetTop(_selectedCropImage, lastTop + dy);
                    _lastPosition = position;
                }

            }
        }


    }
}
