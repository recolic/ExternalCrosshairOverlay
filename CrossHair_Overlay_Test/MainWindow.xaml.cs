using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace External_Crosshair_Overlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ECO_MainGUI : Window
    {
        #region Variables
        // global level crosshair window
        OverlayCrosshair crosshairOverlayWindow = new OverlayCrosshair();
        KeyboardHook kHook;

        List<Process> allRunningProcesses;
        List<string> nonEmptyWindowNames = new List<string>();
        List<Color> crosshairColors = new List<Color>();
        List<string> crosshairColorNames = new List<string>();
        int offsetX = 0;
        int offsetY = 0;

        public int OffsetX
        {
            get
            {
                return offsetX;
            }
            set
            {
                offsetX = value;
                SetOffsets();
            }
        }

        public int OffsetY
        {
            get
            {
                return offsetY;
            }
            set
            {
                offsetY = value;
                SetOffsets();
            }
        }

        bool isAttachedToSomeProcess = false;
        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public ECO_MainGUI()
        {
            InitializeComponent();
        }

        #region Event Handling

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // loads all the crosshair colors
            LoadColors();
            // load all the processes
            LoadProcesses();
            // attaching to event the handler
            crosshairOverlayWindow.AttachedToProcessComplete += AttachingToProcessComplete;
            // display the transparent crosshair window
            crosshairOverlayWindow.Show();
            // initializing all hotkeys
            kHook = new KeyboardHook();
            kHook.KeyCombinationPressed += KHook_KeyCombinationPressed;
        }

        private void KHook_KeyCombinationPressed(Key keyPressed)
        {
            if (isAttachedToSomeProcess)
            {
                if (keyPressed == Key.OemMinus)
                {
                    crosshairOverlayWindow.ToggleOffsetSetupMode();
                }
                else if (keyPressed == Key.Up)
                {
                    OffsetY = crosshairOverlayWindow.MoveCrosshairUp();
                }
                else if (keyPressed == Key.Down)
                {
                    OffsetY = crosshairOverlayWindow.MoveCrosshairDown();
                }
                else if (keyPressed == Key.Left)
                {
                    OffsetX = crosshairOverlayWindow.MoveCrosshairLeft();
                }
                else if (keyPressed == Key.Right)
                {
                    OffsetX = crosshairOverlayWindow.MoveCrosshairRight();
                }
                else if (keyPressed == Key.W)
                {
                    OffsetY = crosshairOverlayWindow.MoveCrosshairUp(true);
                }
                else if (keyPressed == Key.S)
                {
                    OffsetY = crosshairOverlayWindow.MoveCrosshairDown(true);
                }
                else if (keyPressed == Key.A)
                {
                    OffsetX = crosshairOverlayWindow.MoveCrosshairLeft(true);
                }
                else if (keyPressed == Key.D)
                {
                    OffsetX = crosshairOverlayWindow.MoveCrosshairRight(true);
                }
            }
        }

        private void SetOffsets()
        {
            lbl_offsets.Content = offsetX + ", " + offsetY + " (x, y from center)";
        }

        private void ReloadProcesses_Click(object sender, RoutedEventArgs e)
        {
            LoadProcesses();
        }

        private void LoadSelectedProcess_Click(object sender, RoutedEventArgs e)
        {
            // if selected combo box item isn't empty(empty one is at index -1)
            if (cmb_Processes.SelectedIndex >= 0)
            {
                crosshairOverlayWindow.AttachToProcess(allRunningProcesses[cmb_Processes.SelectedIndex]);
            }
        }

        private void ReduceCrosshairScale_Click(object sender, RoutedEventArgs e)
        {
            if (crosshairOverlayWindow.CrosshairScale > 1)
            {
                crosshairOverlayWindow.SetCrosshairScale(crosshairOverlayWindow.CrosshairScale - 1);
                lbl_crosshairScale.Content = crosshairOverlayWindow.CrosshairScale;
            }
        }

        private void IncreaseCrosshairScale_Click(object sender, RoutedEventArgs e)
        {
            if (crosshairOverlayWindow.CrosshairScale < 25)
            {
                crosshairOverlayWindow.SetCrosshairScale(crosshairOverlayWindow.CrosshairScale + 1);
                lbl_crosshairScale.Content = crosshairOverlayWindow.CrosshairScale;
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void ChangeCrosshairColor_Click(object sender, RoutedEventArgs e)
        {
            if (cmb_color.SelectedIndex >= 0)
            {
                crosshairOverlayWindow.SetCrosshairColor = crosshairColors[cmb_color.SelectedIndex];
            }
        }

        private void CrosshairTransparency_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            crosshairOverlayWindow.SetCrosshairTransparency = (double)e.NewValue;
        }

        /// <summary>
        /// Custom event triggered from the global <see cref="OverlayCrosshair"/> window
        /// </summary>
        /// <param name="processName">The name of the process' exe file</param>
        private void AttachingToProcessComplete(string processName)
        {
            Dispatcher.Invoke(() =>
            {
                if (lbl_attachTo.Content.ToString() != processName)
                    lbl_attachTo.Content = processName;
                isAttachedToSomeProcess = processName != "None";
            });
        }

        private void btn_resetPic_Click(object sender, RoutedEventArgs e)
        {
            if (crosshairOverlayWindow.SetCrosshairPic(""))
            {
                lbl_crosshair_pic.Content = "Default";
                cmb_color.IsEnabled = true;
                btn_SetColor.IsEnabled = true;
            }
        }

        private void btn_loadPic_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                // Set filter for file extension and default file extension
                DefaultExt = ".png",
                Filter = "PNG Files (*.png)|*.png|JPEG Files (*.jpeg)|*.jpeg|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif|BMP Files (*.bmp)|*.bmp|All Files (*.*)|*.*"
            };

            // Display OpenFileDialog by calling ShowDialog method
            var result = dlg.ShowDialog();

            // Get the selected file name
            if (result == true)
            {
                // Open document
                var fileName = dlg.FileName;
                if (crosshairOverlayWindow.SetCrosshairPic(fileName))
                {
                    var justFileName = (from x in fileName.Split('\\')
                                        select x).LastOrDefault();
                    if (!String.IsNullOrWhiteSpace(justFileName))
                        lbl_crosshair_pic.Content = justFileName;
                    else
                        lbl_crosshair_pic.Content = "Img_File_With_Invalid_Name";
                    cmb_color.IsEnabled = false;
                    btn_SetColor.IsEnabled = false;
                }
            }
        }
        #endregion

        #region Custom Methods

        /// <summary>
        /// Loads all the processes & updates the total processes label
        /// </summary>
        private void LoadProcesses()
        {
            nonEmptyWindowNames.Clear();
            lbl_procCount.Content = "Loading...";

            // re-load all the processes in another thread to avoid GUI lag
            var processLoadThread = new Thread(Thread_LoadProcess);
            processLoadThread.Start();
        }

        /// <summary>
        /// This method must run in seperate thread
        /// </summary>
        private void Thread_LoadProcess()
        {
            // only collect process with a valid window title
            allRunningProcesses = (from process in Process.GetProcesses()
                                   where !String.IsNullOrWhiteSpace(process.MainWindowTitle) && process.MainWindowTitle != "External Crosshair Overlay by gmastergreatee"
                                   select process).ToList();

            // change the gui in accordance with the data collected
            Dispatcher.Invoke(() =>
            {
                // set the combo-box source to list of "Window Titles"
                nonEmptyWindowNames = (from process in allRunningProcesses
                                       select process.MainWindowTitle).ToList();
                cmb_Processes.ItemsSource = nonEmptyWindowNames;
                // update the found process count
                lbl_procCount.Content = allRunningProcesses.Count;
            });
        }

        /// <summary>
        /// Loads the color collection
        /// </summary>
        private void LoadColors()
        {
            // Red color
            crosshairColorNames.Add("Red");
            crosshairColors.Add(Colors.Red);

            // Black color
            crosshairColorNames.Add("Black");
            crosshairColors.Add(Colors.Black);

            // Blue color
            crosshairColorNames.Add("Blue");
            crosshairColors.Add(Colors.Blue);

            // Aqua color
            crosshairColorNames.Add("Aqua");
            crosshairColors.Add(Colors.Aqua);

            // Violet color
            crosshairColorNames.Add("Violet");
            crosshairColors.Add(Colors.Violet);

            // Brown color
            crosshairColorNames.Add("Brown");
            crosshairColors.Add(Colors.Brown);

            // SlateGray color
            crosshairColorNames.Add("SlateGray");
            crosshairColors.Add(Colors.SlateGray);

            // Chocolate color
            crosshairColorNames.Add("Chocolate");
            crosshairColors.Add(Colors.Chocolate);

            // Crimson color
            crosshairColorNames.Add("Crimson");
            crosshairColors.Add(Colors.Crimson);

            // LightGreen color
            crosshairColorNames.Add("LightGreen");
            crosshairColors.Add(Colors.LightGreen);

            // Maroon color
            crosshairColorNames.Add("Maroon");
            crosshairColors.Add(Colors.Maroon);

            // YellowGreen color
            crosshairColorNames.Add("YellowGreen");
            crosshairColors.Add(Colors.YellowGreen);

            cmb_color.ItemsSource = crosshairColorNames;
            cmb_color.SelectedIndex = 0;
        }

        #endregion
    }
}
