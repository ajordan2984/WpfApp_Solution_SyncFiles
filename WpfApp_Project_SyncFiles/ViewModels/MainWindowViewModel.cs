using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfApp_Project_SyncFiles.Commands;
using WpfApp_Project_SyncFiles.ViewModels;
using WpfApp_Project_SyncFiles.Views;

namespace WpfApp_Project_SyncFiles.ViewModels
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public MainWindowViewModel()
        {
            UpdateCommandBrowseExternalDrive1 = new ButtonCommands(Browse);
            UpdateCommandBrowseExternalDrive2 = new ButtonCommands(Browse);
            UpdateCommandBrowseExternalDrive3 = new ButtonCommands(Browse);
            UpdateCommandBrowseExternalDrive4 = new ButtonCommands(Browse);
            _Door = new object();
            _isBusy = false;
        }

        #region Private Members
        private static string _ExternalDrive1Path = null;
        private static string _ExternalDrive2Path = null;
        private static string _ExternalDrive3Path = null;
        private static string _ExternalDrive4Path = null;

        // Used for thread safety
        private static object _Door;
        private static volatile bool _isBusy;
        #endregion

        #region Public Properties
        public string ExternalDrive1Path
        {
            get
            {
                return _ExternalDrive1Path;
            }
            set
            {
                _ExternalDrive1Path = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(ExternalDrive1Path)));
            }
        }
        public string ExternalDrive2Path
        {
            get
            {
                return _ExternalDrive2Path;
            }
            set
            {
                _ExternalDrive2Path = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(ExternalDrive2Path)));
            }
        }
        public string ExternalDrive3Path
        {
            get
            {
                return _ExternalDrive3Path;
            }
            set
            {
                _ExternalDrive3Path = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(ExternalDrive3Path)));
            }
        }
        public string ExternalDrive4Path
        {
            get
            {
                return _ExternalDrive4Path;
            }
            set
            {
                _ExternalDrive4Path = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(ExternalDrive4Path)));
            }
        }

        #endregion


        #region Button Clicks (Commands)
        public ICommand UpdateCommandBrowseExternalDrive1 { get; private set; }
        public ICommand UpdateCommandBrowseExternalDrive2 { get; private set; }
        public ICommand UpdateCommandBrowseExternalDrive3 { get; private set; }
        public ICommand UpdateCommandBrowseExternalDrive4 { get; private set; }

        #endregion

        #region Button Executions
        public void Browse()
        {
            var childView = new FileDialogView();
            var childModel = new FileDialogViewModel(childView.Close);
            childView.DataContext = childModel;
            childView.ShowDialog();

            if (childModel.FileDialogTree.SelectedItem != null)
            {
                ExternalDrive1Path = childModel.FileDialogTree.SelectedItem.FullPath;
            }
        }





        //public void ConvertPicture()
        //{
        //    try
        //    {
        //        lock (_Door)
        //        {
        //            if (_isBusy)
        //            {
        //                MessageBox.Show("Please wait while image is converted.", "Alert");
        //                return;
        //            }
        //        }

        //        if (string.IsNullOrEmpty(_SelectedPath))
        //        {
        //            MessageBox.Show("Please select a picture to convert.", "Alert");
        //            return;
        //        }

        //        if (!File.Exists(_SelectedPath))
        //        {
        //            MessageBox.Show("The file path to selected picture is not valid. Please check the file.", "Alert");
        //            return;
        //        }

        //        // Disable the buttons
        //        CanExecuteButtons(false);

        //        // Get the picture
        //        //_Bitmap = (Bitmap)Image.FromFile(_SelectedPath);

        //        //var childView = new LoadingDialogView();
        //        //ILoadingDialog loadingDialog = new LoadingDialogViewModel(_Bitmap.Width, childView.Close);
        //        //childView.DataContext = loadingDialog;
        //        //childView.Show();

        //        Task.Run(() =>
        //        {
        //            lock (_Door)
        //            {
        //                _isBusy = true;
        //            }

        //            lock (_Door)
        //            {
        //                _isBusy = false;
        //            }

        //            // Creates a bridge to access another thread
        //            App.Current.Dispatcher.Invoke(() =>
        //            {
        //                // Enable the buttons
        //                CanExecuteButtons(true);
        //            });
        //        });
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //    }
        //}
        //public void Clear()
        //{
        //    lock (_Door)
        //    {
        //        if (_isBusy)
        //        {
        //            MessageBox.Show("Please wait while image is converted.", "Alert");
        //            return;
        //        }
        //    }

        //    SelectedPath = null;
        //}
        //public void Save()
        //{
        //    try
        //    {
        //        lock (_Door)
        //        {
        //            if (_isBusy)
        //            {
        //                MessageBox.Show("Please wait while image is converted.", "Alert");
        //                return;
        //            }
        //        }

        //        //if (_Bitmap == null)
        //        //{
        //        //    MessageBox.Show("Please convert an image to save the results.", "Alert");
        //        //}
        //        //else
        //        //{
        //        //    SaveFileDialog dialog = new SaveFileDialog
        //        //    {
        //        //        Filter = "BMP | *.bmp | GIF | *.gif | JPG | *.jpg; *.jpeg | PNG | *.png | TIFF | *.tif; *.tiff"
        //        //    };

        //        //    if (dialog.ShowDialog() == true)
        //        //    {
        //        //        _Bitmap.Save(dialog.FileName);

        //        //        // Declare child view
        //        //        PictureMessageBoxView childView = new PictureMessageBoxView();
        //        //        // Declare and set child model
        //        //        PictureMessageBoxViewModel childModel = new PictureMessageBoxViewModel(dialog.FileName, "The converted picture was saved!", childView.Close);
        //        //        // Set the data context of the child view
        //        //        childView.DataContext = childModel;
        //        //        // Display the child view to the user
        //        //        childView.Show();
        //        //    }
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.Write(ex.Message);
        //    }
        //}
        #endregion

        #region Helper Functions
        //private Bitmap ChangeColor(Bitmap scrBitmap, ILoadingDialog view)
        //{
        //    #region Error Checking
        //    // Check if a valid hex 
        //    if (!int.TryParse(_FromSelectedColorHex, System.Globalization.NumberStyles.HexNumber, null, out _))
        //    {
        //        MessageBox.Show("Please enter a valid hex number into the 'From Color' box.", "Alert");
        //        return null;
        //    }

        //    // Check if a valid hex 
        //    if (!int.TryParse(_ToSelectedColorHex, System.Globalization.NumberStyles.HexNumber, null, out _))
        //    {
        //        MessageBox.Show("Please enter a valid hex number into the 'To Color' box.", "Alert");
        //        return null;
        //    }
        //    #endregion

        //    System.Drawing.Color FromColor = ColorTranslator.FromHtml(string.Format("#{0}", _FromSelectedColorHex));
        //    System.Drawing.Color ToColor = ColorTranslator.FromHtml(string.Format("#{0}", _ToSelectedColorHex));

        //    int Range = string.IsNullOrEmpty(_PixelRange) ? 0 : Convert.ToInt32(_PixelRange);
        //    int PixelsChanged = 0;

        //    System.Drawing.Color actualColor;
        //    // Make a new empty bitmap the same size as scrBitmap
        //    Bitmap newBitmap = new Bitmap(scrBitmap.Width, scrBitmap.Height);
        //    for (int i = 0; i < scrBitmap.Width; i++)
        //    {
        //        for (int j = 0; j < scrBitmap.Height; j++)
        //        {
        //            // Get the pixel from the scrBitmap image
        //            actualColor = scrBitmap.GetPixel(i, j);
        //            // Adjust for range
        //            bool RedInRange = ((actualColor.R >= FromColor.R - Range) && (actualColor.R <= FromColor.R + Range));
        //            bool GreenInRange = ((actualColor.G >= FromColor.G - Range) && (actualColor.G <= FromColor.G + Range));
        //            bool BlueInRange = ((actualColor.B >= FromColor.B - Range) && (actualColor.B <= FromColor.B + Range));

        //            if (RedInRange && GreenInRange && BlueInRange)
        //            {
        //                newBitmap.SetPixel(i, j, ToColor);
        //                PixelCount = (PixelsChanged++).ToString();
        //            }
        //            else
        //            {
        //                newBitmap.SetPixel(i, j, actualColor);
        //            }
        //        }
        //        view.IncrementProgress(1);
        //    }

        //    PixelCount = string.Format("{0:n0}", PixelsChanged);
        //    return newBitmap;
        //}
        //private static BitmapImage ConvertToBitmapImage(Bitmap src)
        //{
        //    MemoryStream ms = new MemoryStream();
        //    src.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
        //    BitmapImage image = new BitmapImage();
        //    image.BeginInit();
        //    ms.Seek(0, SeekOrigin.Begin);
        //    image.StreamSource = ms;
        //    image.EndInit();
        //    image.Freeze();
        //    return image;
        //}
        //private static void SetColorItems()
        //{
        //    if (_ColorModelItems == null)
        //    {
        //        _ColorModelItems = new ObservableCollection<ColorModel>();
        //        foreach (KnownColor kc in Enum.GetValues(typeof(KnownColor)))
        //        {
        //            if (kc > KnownColor.Transparent && kc < KnownColor.ButtonFace)
        //            {
        //                System.Drawing.Color color = System.Drawing.Color.FromKnownColor(kc);
        //                _ColorModelItems.Add(new ColorModel(color.Name, string.Format("{0:x6}", color.ToArgb())));
        //            }
        //        }
        //    }
        //}

        //private void OpenColorDataGrid()
        //{
        //    SetColorItems();
        //    ColorsDataGridView childView = new ColorsDataGridView();
        //    ColorsDataGridViewModel childModel = new ColorsDataGridViewModel((string htmlColor) => FromSelectedColorHex = htmlColor)
        //    {
        //        ColorModelItems = _ColorModelItems
        //    };

        //    childView.DataContext = childModel;
        //    childView.Show();
        //}

        //private void OpenPeopleDataGrid()
        //{
        //    var childView = new PeopleDataGridView();
        //    var childModel = new PeopleDataGridViewModel();
        //    childView.DataContext = childModel;
        //    childView.Show();
        //}

        //private void CanExecuteButtons(bool enabled)
        //{
        //    UpdateCommandBrowse.CanExecute(enabled);
        //}
        #endregion
    }
}
