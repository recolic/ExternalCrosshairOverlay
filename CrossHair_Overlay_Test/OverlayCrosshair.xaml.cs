using CrossHair_Overlay_Test.Config;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace External_Crosshair_Overlay
{
    /// <summary>
    /// Interaction logic for OverlayCrosshair.xaml
    /// </summary>
    public partial class OverlayCrosshair : Window
    {
        public delegate void AttachedTo(string processName);
        public AttachedTo AttachedToProcessComplete;
        public int CrosshairScale;
        private CrosshairMode crosshairMode = CrosshairMode.Default;

        #region Setters
        private Color crosshairColor = Colors.Red;
        private double crosshairTransparency = 1;

        /// <summary>
        /// Sets the crosshair's color
        /// </summary>
        public Color SetCrosshairColor
        {
            set
            {
                crosshairColor = value;
                CrosshairColor(value);
            }
        }

        /// <summary>
        /// Set the crosshair's transparency
        /// </summary>
        public double SetCrosshairTransparency
        {
            set
            {
                crosshairTransparency = value;
                if (crosshairMode == CrosshairMode.Default)
                    grid_crosshair.Opacity = value;
                else
                    img_crosshair.Opacity = value;
            }
        }

        #endregion

        bool errorFiredOnce = false;
        Process currentProcess;
        IntPtr currentWindowHandle;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public OverlayCrosshair()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Specify the process on which to display the crosshair
        /// </summary>
        /// <param name="newProcess">The <see cref="Process"/> object here</param>
        public void AttachToProcess(Process newProcess)
        {
            currentProcess = newProcess;
            AttachedToProcessComplete?.Invoke(currentProcess.ProcessName + ".exe(" + currentProcess.MainWindowTitle + ")");
        }

        /// <summary>
        /// Set the crosshair's scale
        /// </summary>
        /// <param name="scale">The scale</param>
        public void SetCrosshairScale(int scale)
        {
            // recording the crosshair scale to return to the calling GUI when asked for
            CrosshairScale = scale;
            if (crosshairMode == CrosshairMode.Default)
            {
                var originalCrosshairScale = new GridLength(CrosshairScale > 1 ? CrosshairScale * 0.75 : CrosshairScale);

                // (controlled by RowDefinitions &ColumnDefinitions)
                // setting the thickness of the crosshair
                w_grid.Width = originalCrosshairScale;
                h_grid.Height = originalCrosshairScale;
                // setting the crosshair grid's height & thickness
                grid_crosshair.Height = CrosshairScale * 25;
                grid_crosshair.Width = CrosshairScale * 25;
            }
            else
            {
                var originalCrosshairScale = CrosshairScale * 0.25;
                img_crosshair.LayoutTransform = new ScaleTransform(originalCrosshairScale, originalCrosshairScale);
            }
        }

        /// <summary>
        /// Set the crosshair's color
        /// </summary>
        /// <param name="color">The <see cref="Color"/> object to use as the fill</param>
        private void CrosshairColor(Color color)
        {
            // setting alpha value to 100 for semi-transparent
            var solidColor = new SolidColorBrush(Color.FromRgb(color.R, color.G, color.B));

            leftbar.Fill = solidColor;
            rightbar.Fill = solidColor;
            topbar.Fill = solidColor;
            bottombar.Fill = solidColor;
        }

        /// <summary>
        /// Sets/Resets the crosshair picture
        /// </summary>
        /// <param name="filePath">The filepath for the crosshair's image</param>
        /// <returns>True if success, else false</returns>
        public bool SetCrosshairPic(string filePath)
        {
            if (String.IsNullOrWhiteSpace(filePath))
            {
                crosshairMode = CrosshairMode.Default;
            }
            else
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        img_crosshair.Source = new BitmapImage(new UriBuilder(filePath).Uri);
                        crosshairMode = CrosshairMode.Custom;
                    }
                    catch
                    {
                        MessageBox.Show("Invalid image file selected", "Oops!");
                        return false;
                    }
                }
                else
                {
                    // this block of code will never actually execute(at-least IMO)
                    MessageBox.Show("No image file selected", "Oops!");
                    return false;
                }
            }

            SetCrosshairScale(CrosshairScale);
            SetCrosshairTransparency = crosshairTransparency;
            HideWindows();
            DisplayWindow();
            return true;
        }

        /// <summary>
        /// Method responsible for setting the overlay's position on the desktop
        /// </summary>
        private void Thread_loop()
        {
            while (true)
            {
                if (currentProcess != null)
                {
                    var rectangle = GetWindowRect(currentProcess.MainWindowHandle);
                    // case when attached process is minimized/closed
                    if (rectangle.left <= 0 &&
                        rectangle.top <= 0 &&
                        rectangle.right <= 0 &&
                        rectangle.bottom <= 0)
                    {
                        // if error event not triggered once
                        if (!errorFiredOnce)
                        {
                            ProcessError();
                            errorFiredOnce = true;
                        }
                    }
                    else
                    {
                        // Invoke the instructions on the original thread's(GUI) context
                        Dispatcher.Invoke(() =>
                        {
                            // put the crosshair's window to the topmost z-index order(-1)
                            SetWindowPos(currentWindowHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);

                            // set the position & visibility of the crosshair's window
                            Top = rectangle.top;
                            Left = rectangle.left;
                            Height = rectangle.bottom - rectangle.top;
                            Width = rectangle.right - rectangle.left;
                            DisplayWindow();

                            // reset error trigger if no errors found
                            if (errorFiredOnce)
                            {
                                errorFiredOnce = false;
                                AttachedToProcessComplete?.Invoke(currentProcess.ProcessName + ".exe(" + currentProcess.MainWindowTitle + ")");
                            }
                        });
                    }
                }
                // sleep the thread to decrease CPU usage
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Display/Hide a single crosshair-container(based on the value of <see cref="crosshairMode"/>)
        /// </summary>
        /// <param name="makeWindowVisible">Whether to display or hide the window</param>
        private void DisplayWindow(bool makeWindowVisible = true)
        {
            var visibility = Visibility.Visible;
            if (!makeWindowVisible)
            {
                visibility = Visibility.Hidden;
            }

            if (crosshairMode == CrosshairMode.Default)
            {
                grid_crosshair.Visibility = visibility;
            }
            else
            {
                img_crosshair.Visibility = visibility;
            }
        }

        /// <summary>
        /// Hide all the crosshair-containers
        /// </summary>
        private void HideWindows()
        {
            grid_crosshair.Visibility = Visibility.Hidden;
            img_crosshair.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Gets the coordinates(window related) of the target process
        /// </summary>
        /// <param name="hWnd">The handle to the target process</param>
        /// <returns></returns>
        private RECT GetWindowRect(IntPtr hWnd)
        {
            RECT result = new RECT();
            GetWindowRect(hWnd, out result);
            return result;
        }

        /// <summary>
        /// The event triggered after successful creation of the <see cref="OverlayCrosshair"/> window
        /// </summary>
        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            // setting the crosshair window transparent & non-focusable by altering styles
            currentWindowHandle = (new WindowInteropHelper(this)).Handle;
            var extendedStyle = GetWindowLong(currentWindowHandle, GWL_EXSTYLE);
            SetWindowLong(currentWindowHandle, GWL_EXSTYLE, extendedStyle | WS_EX_NOACTIVATE | WS_EX_TRANSPARENT);

            // creating a thread to check for "AttachedWindow"'s coordinates
            Thread newThread = new Thread(new ThreadStart(Thread_loop));
            newThread.Start();

            // setting the default crosshair scale
            SetCrosshairScale(2);
        }

        /// <summary>
        /// Run this method when the process doesn't exist anymore
        /// </summary>
        private void ProcessError()
        {
            Dispatcher.Invoke(() =>
            {
                // only run if attached process isn't null or the event hasn't been triggered
                if (currentProcess != null && !errorFiredOnce)
                {
                    HideWindows();
                    AttachedToProcessComplete?.Invoke("None");
                }
            });
        }

        #region Imports from User32.dll

        #region Constants
        public const int WS_EX_NOACTIVATE = 0x08000000;
        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const int GWL_EXSTYLE = -20;

        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public const int SWP_NOACTIVATE = 0x0010;
        public const int SWP_NOSIZE = 1;
        public const int SWP_NOMOVE = 2;
        #endregion

        /// <summary>
        /// Structure to store coordinates of the target window
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left, top, right, bottom;
        }

        /// <summary>
        /// User32's method to query for the target's coordinates
        /// </summary>
        /// <param name="hwnd">The handle of the target process</param>
        /// <param name="lpRect">The output <see cref="RECT"/> object</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        /// <summary>
        /// Gets the styles of the window
        /// </summary>
        /// <param name="hwnd">The handle of the window whose styles to query for</param>
        /// <param name="index">Unknown to me(gmastergreatee) what this does</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        /// <summary>
        /// Set the styles of the window
        /// </summary>
        /// <param name="hwnd">The handle of the target window</param>
        /// <param name="index">Unknown to me(gmastergreatee) what this does</param>
        /// <param name="newStyle">The new style(ORed value)</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        #endregion
    }
}