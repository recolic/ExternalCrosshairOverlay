using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
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

        List<Process> allRunningProcesses;
        List<string> nonEmptyWindowNames = new List<string>();
        List<Color> crosshairColors = new List<Color>();
        List<string> crosshairColorNames = new List<string>();
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
            if (crosshairOverlayWindow.CrosshairScale < 15)
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
            crosshairOverlayWindow.SetCrosshairTransparency = (byte)e.NewValue;
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
            });
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
