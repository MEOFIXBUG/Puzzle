using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
        int cellHeight;
        int cellWidth;
        const int mRows = 3;
        const int mCols = 3;
        CroppedBitmap[] objImg;
        Image[] images;
        int[] map;

        bool isDragging = false;
        bool isDrop = true;
        Image _selectedCropImage = null;
        Point _lastPosition;
        Tuple<int, int> lastCell;
        bool gameOver = true;

        Timer timer;
        int _time = 180;
        string _previewImageSource;
        int _mainWindowHeight;
        public string PreviewImageSource { get => _previewImageSource; set { _previewImageSource = value; OnPropertyChanged("PreviewImageSource"); } }

        public int MainWindowHeight { get => (int)mainWindow.Height - (int)SystemParameters.CaptionHeight; set { OnPropertyChanged(); _mainWindowHeight = value; } }


        public int Time { get => _time; set { _time = value; OnPropertyChanged(); } }

        public object Int { get; private set; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
            playButton.Content = "Play";
            CreateNewGame();
        }

        private void CreateNewGame()
        {
            objImg = new CroppedBitmap[mRows * mCols];
            map = new int[mRows * mCols];
            images = new Image[mRows * mCols];
            Time = 180;
            var screen = new OpenFileDialog();
            screen.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
       "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
       "Portable Network Graphic (*.png)|*.png";
            if (screen.ShowDialog() == true)
            {
                PreviewImageSource = screen.FileName;
                initTable();
            }
        }

        private void initTable()
        {
            playButton.Content = "Play";
            var source = new BitmapImage(
                   new Uri(PreviewImageSource, UriKind.Absolute));
            CutImage(source);
            DrawPuzzleBoard();
            Shuffle();
            SetPieces();
            try
            {
                destroySnap();
            }
            catch (Exception er)
            {

            }
            //  initSnap();
        }
        private void initSnap()
        {
            this.KeyDown += Window_KeyDown;
            var totalImage = myCanvas.Children.OfType<Image>().ToList();
            foreach (var image in totalImage)
            {
                image.MouseLeftButtonDown += beginDrag;
                image.MouseLeftButtonUp += endDrag;
            }
        }
        private void destroySnap()
        {
            this.KeyDown -= Window_KeyDown;
            var totalImage = myCanvas.Children.OfType<Image>().ToList();
            foreach (var image in totalImage)
            {
                image.MouseLeftButtonDown -= beginDrag;
                image.MouseLeftButtonUp -= endDrag;
            }
        }
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Time--;
            Dispatcher.Invoke(() => //dispatcher.main.sync{code}
            {
                if (Time <= 30)
                {
                    timeProgressBar.Foreground = Brushes.Red;
                }
                else
                {
                    timeProgressBar.Foreground = Brushes.Green;
                }
                int min = Time / 60;
                int second = Time % 60;
                if (second > 9)
                {
                    timeLabel.Content = $"0{Time / 60}:{second}";
                }
                else
                {
                    timeLabel.Content = $"0{Time / 60}:0{second}";
                }
            });//this is closure in swift
            if (Time == 0)
            {

                //xử lí hết giờ ở đây.
                timer.Stop();
                MessageBox.Show("Thua rồi !!!");
                gameOver = true;
                isDragging = false;
                PreviewImageSource = null;
                Dispatcher.Invoke(() =>
                {
                    myCanvas.Children.Clear();
                    playButton.Content = "Play";
                    this.KeyDown -= Window_KeyDown;
                    var totalImage = myCanvas.Children.OfType<Image>().ToList();
                    foreach (var image in totalImage)
                    {
                        image.MouseLeftButtonDown -= beginDrag;
                        image.MouseLeftButtonUp -= endDrag;
                    }

                });
            }
        }

        private void CutImage(BitmapImage source)
        {
            map = new int[mRows * mCols];
            var minPixelEdge = source.PixelWidth < source.PixelHeight ? (int)source.PixelWidth : (int)source.PixelHeight;
            var minEdge = 700 - 80;
            var firstCroppedBitmap = new CroppedBitmap(source, new Int32Rect(0, 0, minPixelEdge, minPixelEdge));
            previewImage.Source = firstCroppedBitmap;
            var pixelHeight = minPixelEdge / mRows;
            var pixelWidth = minPixelEdge / mCols;
            cellHeight = minEdge / mRows;
            cellWidth = minEdge / mCols;

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
                        images[mRows * i + j].Width = cellWidth - 2;
                        images[mRows * i + j].Height = cellHeight - 2;
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
                Point point1 = new Point(startX + j * cellWidth, startY);
                Point point2 = new Point(startX + j * cellWidth, startY + mRows * cellHeight);
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
                Point point1 = new Point(startX, startY + i * cellHeight);
                Point point2 = new Point(startX + mCols * cellWidth, startY + i * cellHeight);
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
                        cropImage.Width = cellWidth;
                        cropImage.Height = cellHeight;
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
        }

        private void SetPieces()
        {
            var uiimages = myCanvas.Children.OfType<Image>().ToList();
            foreach (var image in uiimages)
            {
                myCanvas.Children.Remove(image);
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
                        Canvas.SetLeft(images[map[mRows * i + j]], startX + j * cellWidth);
                        Canvas.SetTop(images[map[mRows * i + j]], startY + i * cellHeight);
                    }
            }

        }


        bool canMove(int x1, int y1, int x2, int y2)
        {
            if (x1 < 0 || x1 >= mCols) return false;
            if (y1 < 0 || y1 >= mRows) return false;
            if (x2 < 0 || x2 >= mCols) return false;
            if (y2 < 0 || y2 >= mRows) return false;
            if (map[mRows * y1 + x1] != -1) return false;
            if (map[mRows * y1 + x1] == -1 && ((x1 == x2 && Math.Abs(y1 - y2) == 1) ^ (Math.Abs(x1 - x2) == 1 && y1 == y2)))
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
            if (!gameOver)
            {
                var position = e.GetPosition(this);
                var cellX = (int)(position.X - startX) / cellWidth;
                var cellY = (int)(position.Y - startY) / cellHeight;
                var curCell = new Tuple<int, int>(-1, -1);

                if (cellX < mCols && cellX >= 0 && cellY < mRows && cellY >= 0 && godhand == 1)
                {
                    curCell = new Tuple<int, int>(cellX, cellY);
                    Debug.WriteLine($"curCell: {cellX} - {cellY}");
                    godhand--;
                }

                isDragging = false;
                if (lastCell == null)
                {
                    return;
                }
                if (canMove(curCell.Item1, curCell.Item2, lastCell.Item1, lastCell.Item2))
                {
                    Debug.WriteLine("true");
                    doMove(curCell.Item1, curCell.Item2, lastCell.Item1, lastCell.Item2);
                }
                else
                {
                    Debug.WriteLine("false");
                    doMove(lastCell.Item1, lastCell.Item2, lastCell.Item1, lastCell.Item2);
                }

            }

        }
        int godhand = 0;
        private void beginDrag(object sender, MouseButtonEventArgs e)
        {
            if (!gameOver)
            {
                var position = e.GetPosition(this);
                var cellX = (int)(position.X - startX) / cellWidth;
                var cellY = (int)(position.Y - startY) / cellHeight;

                if (cellX < mCols && cellX >= 0 && cellY < mRows && cellY >= 0 && godhand == 0)
                {

                    lastCell = new Tuple<int, int>(cellX, cellY);
                    Debug.WriteLine($"lastCell: {cellX} - {cellY}");
                    godhand++;
                }

                _selectedCropImage = sender as Image;
                _lastPosition = e.GetPosition(this);
                isDragging = true;

            }
        }
        private void Mouse_Move(object sender, MouseEventArgs e)
        {
            //var position = e.GetPosition(this);
            //if (position == iamge.edge)
            if (isDragging && !gameOver)
            {
                var position = e.GetPosition(this);
                var dx = position.X - _lastPosition.X;
                var dy = position.Y - _lastPosition.Y;

                if (position.X < startX - 20 || position.Y < startY - 10 || position.X > startX + (mCols * cellWidth) + 2 * cellWidth || position.Y > startY + (mRows * cellHeight) + 2 * cellHeight)
                {
                    isDragging = false;
                    Canvas.SetLeft(_selectedCropImage, startX + lastCell.Item1 * cellWidth);
                    Canvas.SetTop(_selectedCropImage, startY + lastCell.Item2 * cellHeight);
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
            if (!gameOver)
            {
                int indexNull = getIndexNullImage();
                int cellX = indexNull % mCols;
                int cellY = indexNull / mRows;
                var oldImage = new Image();
                var newImage = new Image();

                oldImage.Stretch = Stretch.Fill;
                oldImage.Width = cellWidth;
                oldImage.Height = cellHeight;
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

        }

        void doMove(int nX, int nY, int oX, int oY)
        {
            //    myCanvas.Children.Add(image);
            _selectedCropImage.Uid = $"{map[mRows * oY + oX]}";
            //  myCanvas.Children.Add(image);
            if (true)
            {
                var temp = map[mRows * nY + nX];
                map[mRows * nY + nX] = map[mRows * oY + oX];
                map[mRows * oY + oX] = temp;
                Canvas.SetLeft(_selectedCropImage, startX + nX * cellWidth + 1);
                Canvas.SetTop(_selectedCropImage, startY + nY * cellHeight + 1);
                Debug.WriteLine($"{nX} - {nY} , old: {oX} - {oY}");
            }
            if (checkWin() == true)
            {
                MessageBox.Show("Win");
                this.KeyDown -= Window_KeyDown;
                var totalImage = myCanvas.Children.OfType<Image>().ToList();
                foreach (var image in totalImage)
                {
                    image.MouseLeftButtonDown -= beginDrag;
                    image.MouseLeftButtonUp -= endDrag;
                }
                isDragging = false;
                gameOver = true;
            }
        }


    }

}

