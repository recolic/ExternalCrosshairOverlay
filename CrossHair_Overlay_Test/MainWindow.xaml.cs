using System;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using External_Crosshair_Overlay.Config;

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
        // global level app-config saver/loader
        ConfigSaver configSaver = new ConfigSaver();
        // global low-level keyboard hook
        KeyboardHook kHook;

        List<Process> allRunningProcesses;
        List<string> nonEmptyWindowNames = new List<string>();
        List<Color> crosshairColors = new List<Color>();
        List<string> crosshairColorNames = new List<string>();
        string attachedProcessFilePath = "";
        int offsetX = 0;
        int offsetY = 0;
        float minCrosshairScale = 0.0001F;
        float maxCrosshairScale = 50;

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
            minCrosshairScale = Convert.ToSingle(sldr_CrosshairScale.Minimum);
            maxCrosshairScale = Convert.ToSingle(sldr_CrosshairScale.Maximum);
            // attaching to event the handler
            crosshairOverlayWindow.AttachedToProcessComplete += AttachingToProcessComplete;
            // display the transparent crosshair window
            crosshairOverlayWindow.Show();
            // initializing all hotkeys
            kHook = new KeyboardHook();
            kHook.KeyCombinationPressed += KHook_KeyCombinationPressed;
        }

        /// <summary>
        /// Event triggered from keyboard-hook in case a key is pressed
        /// </summary>
        /// <param name="keyPressed">The key that was pressed</param>
        private void KHook_KeyCombinationPressed(Key keyPressed)
        {
            if (isAttachedToSomeProcess)
            {
                if (keyPressed == Key.OemMinus)
                {
                    crosshairOverlayWindow.ToggleOffsetSetupMode();
                }

                if (keyPressed == Key.OemPlus)
                {
                    crosshairOverlayWindow.ToggleOverlayOpacity();
                    Dispatcher.Invoke(() =>
                    {
                        SetTitle();
                    });
                }

                if (crosshairOverlayWindow.OffsetSetupMode)
                {
                    if (keyPressed == Key.Up)
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
                var process = allRunningProcesses[cmb_Processes.SelectedIndex];
                if (!process.HasExited)
                {
                    crosshairOverlayWindow.AttachToProcess(process);
                }
                else
                {
                    MessageBox.Show("The selected process has exited. Please \"Reload Processes\" and select the process again.", "Process Exited", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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

        private void CrosshairScale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            crosshairOverlayWindow.SetCrosshairScale((float)e.NewValue);
            SetTitle();
        }

        private void CrosshairTransparency_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            crosshairOverlayWindow.SetCrosshairTransparency = (double)e.NewValue;
        }

        /// <summary>
        /// Custom event triggered from the global <see cref="OverlayCrosshair"/> window
        /// </summary>
        /// <param name="processName">The name of the process' exe file</param>
        private void AttachingToProcessComplete(string processName, string processFilePath)
        {
            Dispatcher.Invoke(() =>
            {
                if (lbl_attachTo.Content.ToString() != processName)
                    lbl_attachTo.Content = processName;
                if (processName != "None")
                {
                    var config = configSaver.GetConfig(processFilePath);

                    // set color
                    cmb_color.SelectedIndex = config.CrosshairColorIndex;
                    if (cmb_color.SelectedIndex >= 0)
                    {
                        crosshairOverlayWindow.SetCrosshairColor = crosshairColors[cmb_color.SelectedIndex];
                    }

                    // set opacity value
                    sldr_Opacity.Value = config.CrosshairOpacity;
                    crosshairOverlayWindow.SetCrosshairTransparency = sldr_Opacity.Value;

                    // set crosshair scale
                    if (config.CrosshairScale >= minCrosshairScale && config.CrosshairScale <= maxCrosshairScale)
                    {
                        crosshairOverlayWindow.SetCrosshairScale(config.CrosshairScale);
                        sldr_CrosshairScale.Value = crosshairOverlayWindow.CrosshairScale;
                    }

                    // set crosshair mode+picture
                    if (crosshairOverlayWindow.SetCrosshairPic(config.CrosshairFileLocation))
                    {
                        var justFileName = (from x in config.CrosshairFileLocation.Split('\\')
                                            where !string.IsNullOrWhiteSpace(x)
                                            select x).LastOrDefault();

                        if (!String.IsNullOrWhiteSpace(justFileName))
                            lbl_crosshair_pic.Content = justFileName;
                        else if (String.IsNullOrWhiteSpace(justFileName))
                            lbl_crosshair_pic.Content = "Default";
                        else
                            lbl_crosshair_pic.Content = "Img_File_With_Invalid_Name";
                        cmb_color.IsEnabled = lbl_crosshair_pic.Content.ToString() == "Default";
                        btn_SetColor.IsEnabled = lbl_crosshair_pic.Content.ToString() == "Default";
                    }

                    // set crosshair offsets
                    OffsetX = config.OffsetX;
                    OffsetY = config.OffsetY;
                    crosshairOverlayWindow.SetCrosshairOffsets(OffsetX, OffsetY);

                    attachedProcessFilePath = processFilePath;
                    isAttachedToSomeProcess = true;
                    btn_saveConfig.IsEnabled = true;

                    SetTitle();
                }
                else
                {
                    attachedProcessFilePath = "";
                    isAttachedToSomeProcess = false;
                    btn_saveConfig.IsEnabled = false;
                }
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
                                        where !string.IsNullOrWhiteSpace(x)
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

        private void btn_saveConfig_Click(object sender, RoutedEventArgs e)
        {
            var config = new OverlayConfig()
            {
                ProcessFilePath = attachedProcessFilePath,
                CrosshairColorIndex = cmb_color.SelectedIndex,
                CrosshairFileLocation = crosshairOverlayWindow.CrosshairImagePath,
                CrosshairOpacity = sldr_Opacity.Value,
                CrosshairScale = crosshairOverlayWindow.CrosshairScale,
                OffsetX = OffsetX,
                OffsetY = OffsetY
            };

            configSaver.SaveConfig(config);

            MessageBox.Show("Config saved successfully");
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
                                   where process.MainWindowHandle != IntPtr.Zero && !String.IsNullOrWhiteSpace(process.MainWindowTitle) && process.MainWindowTitle != "External Crosshair Overlay by gmastergreatee"
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
            // Black color
            crosshairColorNames.Add("Black");
            crosshairColors.Add(Colors.Black);

            // Red color
            crosshairColorNames.Add("Red");
            crosshairColors.Add(Colors.Red);

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

        /// <summary>
        /// Sets the content of the offsets label
        /// </summary>
        private void SetOffsets()
        {
            lbl_offsets.Content = offsetX + ", " + offsetY + " (x, y from center)";
        }

        private void SetTitle()
        {
            ECOWindow.Title = "ECO by gmastergreatee - " + sldr_CrosshairScale.Value.ToString("n2") + " - [" + (!crosshairOverlayWindow.CrosshairToggled ? "Visible" : "Hidden") + "]";
        }
        #endregion
    }
}
