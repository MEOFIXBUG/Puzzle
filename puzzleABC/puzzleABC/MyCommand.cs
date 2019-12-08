using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace puzzleABC
{
    public partial class MainWindow
    {
        ICommand _playButtonClickCommand;
        ICommand _newGameButtonClickCommand;
        ICommand _saveButtonClickCommand;
        ICommand _loadButtonClickCommand;
        public ICommand PlayButtonClickCommand
        {
            get
            {
                return _playButtonClickCommand ??
                    (new MyCommand(
                        () =>
                        {
                            if (playButton.Content as string == "Pause")
                            {
                                playButton.Content = "Continue";
                                timer.Stop();
                                try
                                {
                                    destroySnap();
                                } catch(Exception er)
                                {

                                }
                                gameOver = true;
                            }
                            else
                            {
                                playButton.Content = "Pause";
                                timer.Start();
                                initSnap();
                                gameOver = false;
                            };
                        },
                        () =>
                        {
                            return PreviewImageSource != null;
                        }
                    ));
            }
        }
        public ICommand NewGameButtonClickCommand
        {
            get
            {
                return _newGameButtonClickCommand ??
                    (new MyCommand(
                        () =>
                        {
                            timer?.Stop();
                            timer?.Close();
                            myCanvas.Children.Clear();
                            timeLabel.Content = "03:00";
                            CreateNewGame();
                        },
                        () => { return true; }
                    ));
            }
        }

        public ICommand SaveButtonClickCommand
        {
            get
            {
                return _saveButtonClickCommand ??
                    (new MyCommand(
                        () =>
                        {
                            timer.Stop();
                            //save map arr, image source. time. notify if image source not available
                            var writer = new StreamWriter("save.txt");
                            writer.WriteLine(Time.ToString());
                            for (int i = 0; i < map.Length; i++)
                            {
                                writer.Write(map[i]);
                                writer.Write(" ");
                            }
                            writer.Close();

                            Bitmap bmpOut = null;

                            using (MemoryStream ms = new MemoryStream())
                            {
                                PngBitmapEncoder encoder = new PngBitmapEncoder();
                                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)this.previewImage.Source));
                                encoder.Save(ms);

                                using (Bitmap bmp = new Bitmap(ms))
                                {
                                    bmpOut = new Bitmap(bmp);

                                }

                                bmpOut.Save("previewImage.jpg");
                            }

                            MessageBox.Show("Game save");
                            timer.Start();
                        },
                        () =>
                        {
                            return PreviewImageSource != null;
                        }
                    ));
            }
        }
        public ICommand LoadButtonClickCommand
        {
            get
            {
                return _loadButtonClickCommand ??
                    (new MyCommand(
                        () =>
                        {
                            var reader = new StreamReader("save.txt");

                            var line = reader.ReadLine();
                            int res;
                            Int32.TryParse(line, out res);
                            Time = res;

                            line = reader.ReadLine();
                            var tokens = line.Split(new string[] { " " }, StringSplitOptions.None);
                            int[] temp = new int[mRows * mCols];
                            for (int i = 0; i < map.Length; i++)
                            {
                                Int32.TryParse(tokens[i], out res);
                                temp[i] = res;
                            }

                            reader.Close();
                            playButton.Content = "Play";
                            timer.Stop();
                          //  destroySnap();
                            string path = AppDomain.CurrentDomain.BaseDirectory + "previewImage.jpg";
                            PreviewImageSource = path;

                            BitmapImage source = new BitmapImage();
                            source.BeginInit();
                            source.CacheOption = BitmapCacheOption.OnLoad;
                            source.UriSource = new Uri(path);
                            source.EndInit();
                            previewImage.Source = source;
                            PreviewImageSource = path;
                            myCanvas.Children.Clear();
                            timer?.Close();
                            timer = new System.Timers.Timer();
                            timer.Interval = 1000;
                            timer.Elapsed += Timer_Elapsed;
                            playButton.Content = "Continue";
                            CutImage(source);
                            DrawPuzzleBoard();
                            map = temp;
                            SetPieces();
                            try
                            {
                                destroySnap();
                            } catch(Exception er)
                            {

                            }
                            //initSnap();
                            MessageBox.Show("Game is Loaded!");

                        },
                        () => { return true; }
                    ));
            }
        }
    }
}

//source: https://stackoverflow.com/questions/12422945/how-to-bind-wpf-button-to-a-command-in-viewmodelbase
internal class MyCommand : ICommand
{
    private Action _action;
    private Func<bool> _canExecute;

    /// <summary>
    /// Creates instance of the command handler
    /// </summary>
    /// <param name="action">Action to be executed by the command</param>
    /// <param name="canExecute">A bolean property to containing current permissions to execute the command</param>
    public MyCommand(Action action, Func<bool> canExecute)
    {
        _action = action;
        _canExecute = canExecute;
    }

    /// <summary>
    /// Wires CanExecuteChanged event 
    /// </summary>
    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    /// <summary>
    /// Forcess checking if execute is allowed
    /// </summary>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public bool CanExecute(object parameter)
    {
        return _canExecute.Invoke();
    }

    public void Execute(object parameter)
    {
        _action();
    }
}