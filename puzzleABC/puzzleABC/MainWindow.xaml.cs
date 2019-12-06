﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        const int startX = 0;
        const int startY = 0;
        int height = 120;
        int width = 120;
        const int mRows = 5;
        const int mCols = 5;
        CroppedBitmap[] objImg;
        Image[] images;
        int[] map;

        bool isDragging = false;

        Image _selectedCropImage = null;
        Point _lastPosition;
        Tuple<int, int> lastCell;
        bool gameOver = false;

        string _previewImageSource;
        int _mainWindowHeight;
        int _mainWindowWidth;
        public string PreviewImageSource { get => _previewImageSource; set { _previewImageSource = value; OnPropertyChanged("PreviewImageSource"); } }

        public int MainWindowHeight { get => _mainWindowHeight; set { OnPropertyChanged(); _mainWindowHeight = value; } }

        public int MainWindowWidth { get => _mainWindowWidth; set { OnPropertyChanged(); _mainWindowWidth = value; } }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindowHeight = (int)mainWindow.Height - (int)SystemParameters.CaptionHeight;
            objImg = new CroppedBitmap[mRows * mCols];
            map = new int[mRows * mCols];
            images = new Image[mRows * mCols];
            var screen = new OpenFileDialog();

            if (screen.ShowDialog() == true)
            {
                PreviewImageSource = screen.FileName;
                var source = new BitmapImage(
                   new Uri(screen.FileName, UriKind.Absolute));
                var image = new Image();
                var screenWidth = SystemParameters.WorkArea.Width;
                var screenHeight = SystemParameters.WorkArea.Height;
                //previewImage.Width = 400;
                //previewImage.Height = 400;
                //previewImage.Source = source;

                //Canvas.SetLeft(previewImage, 440);
                //Canvas.SetTop(previewImage, 10);
                CutImage(source);
                //SetPiecesPosition();
                DrawPuzzleBoard();
                Shuffle();
            }

        }

        private void CutImage(BitmapImage source)
        {
            var minPixelEdge = source.PixelWidth < source.PixelHeight ? (int)source.PixelWidth : (int)source.PixelHeight;
            var minEdge = (int)MainWindowHeight - (int)SystemParameters.CaptionHeight - 10;
            var firstCroppedBitmap = new CroppedBitmap(source, new Int32Rect(0, 0, minPixelEdge, minPixelEdge));
            previewImage.Source = firstCroppedBitmap;
            var pixelHeight = minPixelEdge / mRows;
            var pixelWidth = minPixelEdge / mCols;
            height = minEdge / mRows;
            width = minEdge / mCols;

            for (int i = 0; i < mRows; i++)
            {
                for (int j = 0; j < mCols; j++)
                {
                    if (!((i == mRows - 1) && (j == mCols - 1)))
                    {
                        //Debug.WrbiteLine($"Len = {len}");
                        var rect = new Int32Rect(j * pixelWidth, i * pixelHeight, pixelHeight, pixelHeight);
                        var cropBitmap = new CroppedBitmap(firstCroppedBitmap, rect);
                        objImg[mRows * i + j] = cropBitmap;
                        map[mRows * i + j] = (mRows * i + j) % (mRows * mCols);


                    }
                    if (mRows * i + j != mRows * mCols)
                    {
                        images[mRows * i + j] = new Image();
                        images[mRows * i + j].Stretch = Stretch.Fill;
                        images[mRows * i + j].Width = width - 2;
                        images[mRows * i + j].Height = height - 2;
                        images[mRows * i + j].Source = objImg[map[mRows * i + j]];
                        images[mRows * i + j].Uid = $"{mRows * i + j}";
                    }
                }
            }
            map[mRows * (mRows - 1) + mCols - 1] = -1;
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
                myline.StrokeThickness = 2;
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
                myline.StrokeThickness = 2;
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
                        //  cropImage.Tag = mRows * i + j;
                        cropImage.Source = objImg[mRows * i + j];

                    }
                }
            }
        }

        private void Shuffle()
        {
            Random r = new Random();
            int n = mRows * mCols - 1;
            int lastMove = -1;
            bool isDup = false;
            int i2;
            for (int i = 0; i < 99; i++)
            {
                int i1 = getIndexNullImage();
                int cellX = i1 % mCols;
                int cellY = i1 / mRows;
                i2 = r.Next(0, 11111);

                i2 = i2 % 4;
                Debug.WriteLine(i2);
                switch (i2)
                {
                    case 0:
                        i2 = mRows * (cellY - 1) + cellX;
                        if (lastMove == 1)
                        {
                            i--;
                            isDup = true;
                        }
                        else
                        {
                            lastMove = 0;
                            isDup = false;
                        }
                        break;
                    case 1:
                        i2 = mRows * (cellY + 1) + cellX;
                        if (lastMove == 0)
                        {
                            i--;
                            isDup = true;
                        }
                        else
                        {
                            lastMove = 1;
                            isDup = false;
                        }
                        break;
                    case 2:
                        i2 = mRows * (cellY) + cellX + 1;
                        if (lastMove == 3)
                        {
                            i--;
                            isDup = true;
                        }
                        else
                        {
                            lastMove = 2;
                            isDup = false;
                        }
                        break;
                    case 3:
                        i2 = mRows * (cellY) + cellX - 1;
                        if (lastMove == 2)
                        {
                            i--;
                            isDup = true;
                        }
                        else
                        {
                            lastMove = 3;
                            isDup = false;

                        }
                        break;
                }
                if (isDup == true) continue;
                int oldX = i2 % mCols;
                int oldY = i2 / mRows;

                if (canMove(cellX, cellY, oldX, oldY))
                {
                    int temp = map[i1];
                    map[i1] = map[i2];
                    map[i2] = temp;
                }
                else
                {
                    i--;
                }


            }

            for (int i = 0; i < mRows; i++)
            {
                for (int j = 0; j < mCols; j++)
                {
                    if (mRows * i + j != -1)
                        Debug.Write(map[mRows * i + j]);
                }
                Debug.WriteLine("");
            }

            for (int i = 0; i < mRows; i++)
            {
                for (int j = 0; j < mCols; j++)
                    if (map[mRows * i + j] != -1)
                    {

                        myCanvas.Children.Add(images[map[mRows * i + j]]);
                        Canvas.SetLeft(images[map[mRows * i + j]], startX + j * width + 1);
                        Canvas.SetTop(images[map[mRows * i + j]], startY + i * height + 1);
                        images[map[mRows * i + j]].MouseLeftButtonDown += beginDrag;
                        images[map[mRows * i + j]].PreviewMouseLeftButtonUp += endDrag;
                    }
            }

        }


        bool canMove(int x1, int y1, int x2, int y2)
        {
            if (x1 < 0 || x1 >= mCols) return false;
            if (x2 < 0 || x2 >= mCols) return false;
            if (y1 < 0 || y1 >= mRows) return false;
            if (y2 < 0 || y2 >= mRows) return false;

            if (map[mRows * y1 + x1] == -1 && ((Math.Abs(x1 - x2) == 0 && Math.Abs(y1 - y2) == 1) ^ (Math.Abs(x1 - x2) == 1 && Math.Abs(y1 - y2) == 0)))
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
                doMove(cellX, cellY, lastCell.Item1, lastCell.Item2);
            }
            else
            {
                doMove(lastCell.Item1, lastCell.Item2, lastCell.Item1, lastCell.Item2);
            }


        }

        private void beginDrag(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(this);
            var cellX = (int)(position.X - startX) / width;
            var cellY = (int)(position.Y - startY) / height;
            if (cellX < mCols && cellY < mRows)
            {
                lastCell = new Tuple<int, int>(cellX, cellY);
            }
            _selectedCropImage = sender as Image;
            _lastPosition = e.GetPosition(this);
            isDragging = true;
        }
        private void Mouse_Move(object sender, MouseEventArgs e)
        {
            //var position = e.GetPosition(this);
            //if (position == iamge.edge)
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

        //Use keyboard
        int getIndexNullImage()
        {
            for (int i = 0; i < map.Length; i++)
            {
                if (map[i] == -1) return i;
            }
            return -2;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {

            int indexNull = getIndexNullImage();
            int cellX = indexNull % mCols;
            int cellY = indexNull / mRows;
            var oldImage = new Image();
            var newImage = new Image();

            oldImage.Stretch = Stretch.Fill;
            oldImage.Width = width;
            oldImage.Height = height;
            switch (e.Key)
            {
                case Key.Down:
                    if (canMove(cellX, cellY, cellX, cellY - 1))
                    {
                        int index = mRows * (cellY - 1) + cellX;
                        oldImage.Source = objImg[map[index]];
                        var images2 = myCanvas.Children.OfType<Image>().ToList();
                        foreach (var image in images2)
                        {
                            try
                            {
                                if (Int32.Parse(image.Uid) == map[mRows * (cellY - 1) + cellX])
                                {
                                    _selectedCropImage = image;
                                    break;
                                }
                            }
                            catch (Exception r)
                            {
                            }

                        }
                        doMove(cellX, cellY, cellX, cellY - 1);
                    }

                    break;
                case Key.Up:
                    if (canMove(cellX, cellY, cellX, cellY + 1))
                    {
                        oldImage.Source = objImg[map[mRows * (cellY + 1) + (cellX)]];
                        var images2 = myCanvas.Children.OfType<Image>().ToList();
                        foreach (var image in images2)
                        {

                            try
                            {
                                if (Int32.Parse(image.Uid) == map[mRows * (cellY + 1) + cellX])
                                {
                                    _selectedCropImage = image;
                                    break;
                                }
                            }
                            catch (Exception r)
                            {
                            }
                        }
                        doMove(cellX, cellY, cellX, cellY + 1);
                    }
                    break;
                case Key.Left:
                    if (canMove(cellX, cellY, cellX + 1, cellY))
                    {
                        oldImage.Source = objImg[map[mRows * (cellY) + cellX + 1]];
                        var images2 = myCanvas.Children.OfType<Image>().ToList();
                        foreach (var image in images2)
                        {

                            try
                            {
                                if (Int32.Parse(image.Uid) == map[mRows * (cellY) + cellX + 1])
                                {
                                    _selectedCropImage = image;
                                    break;
                                }
                            }
                            catch (Exception r)
                            {
                            }

                        }
                        doMove(cellX, cellY, cellX + 1, cellY);

                    }
                    break;
                case Key.Right:
                    if (canMove(cellX, cellY, cellX - 1, cellY))
                    {
                        oldImage.Source = objImg[map[mRows * (cellY) + cellX - 1]];
                        var images2 = myCanvas.Children.OfType<Image>().ToList();
                        foreach (var image in images2)
                        {

                            try
                            {
                                if (Int32.Parse(image.Uid) == map[mRows * (cellY) + cellX - 1])
                                {
                                    _selectedCropImage = image;
                                    doMove(cellX, cellY, cellX - 1, cellY);
                                    break;
                                }
                            }
                            catch (Exception r)
                            {
                            }
                        }


                    }
                    break;
            }
        }

        void doMove(int nX, int nY, int oX, int oY)
        {
            //    myCanvas.Children.Add(image);
            _selectedCropImage.Uid = $"{map[mRows * oY + oX]}";
            Canvas.SetLeft(_selectedCropImage, startX + nX * width + 1);
            Canvas.SetTop(_selectedCropImage, startY + nY * height + 1);
            if (nX == oX && nY == oY)
            {
                return;
            }
            //  myCanvas.Children.Add(image);
            map[mRows * nY + nX] = map[mRows * oY + oX];
            map[mRows * oY + oX] = -1;


            if (checkWin() == true)
            {
                MessageBox.Show("Win");
            }
        }
    }
}
