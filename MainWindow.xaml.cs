using FSXVORSim.DebugCtl;
using FSXVORSim.SimulatorData;
using System;
using System.Windows;
using System.Windows.Interop;
using FSXVORSim.Resources;
using System.Globalization;
using System.Threading;
using FSXVORSim.AppState;
using System.Windows.Media;

namespace FSXVORSim
{
    /// <summary>
    /// App logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IntPtr handle;
        private DebugCtlUIBox debugCtl;
        private FlightSimulatorBridge.FlightSimulatorBridge simulatorBridge;
        private HwndSource _hwndSource;
        
        internal AppState.AppState appState = AppState.AppState.FromEmptyAppState();
        private AppStateExcercise appStateExcercise;
        private readonly AppStateATCVoice atcVoice = new AppStateATCVoice();

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            simulatorBridge?.Dispose();
            _hwndSource.RemoveHook(WndProc);
            base.OnClosed(e);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            handle = new WindowInteropHelper(this).Handle;
            _hwndSource = HwndSource.FromHwnd(handle);
            _hwndSource.AddHook(WndProc);

            debugCtl = new DebugCtlUIBox(debugBox);
        }

        private void SimulatorBridge_AircraftDataUpdate(object sender, SimulatorAircraftData data)
        {
            appState = AppState.AppState.FromStateAndAircraftKeypair(appState.AsSimulatorStateData(), data, (int)sensitivity.Value);

            if (appStateExcercise == null && appState.VORSignal)
            {
                appStateExcercise = new AppStateExcercise();
                if (this.voiceEnabled?.IsChecked ?? false)
                    atcVoice.SpeakInstruction(Thread.CurrentThread.CurrentUICulture, appStateExcercise);
            }

            if (appStateExcercise?.Equals(appState.VorState) ?? false)
            {
                debugCtl.Debug(String.Format(Strings.CompletedExLog, appStateExcercise.ToString()));
                appStateExcercise = new AppStateExcercise();

                if (this.voiceEnabled?.IsChecked ?? false)
                    atcVoice.SpeakInstruction(Thread.CurrentThread.CurrentUICulture, appStateExcercise);
            }

            UpdateUI();
        }

        private void SimulatorBridge_StatusUpdate(object sender, SimulatorStateData state)
        {
            appState = AppState.AppState.FromStateAndAircraftKeypair(state, appState.AsSimulatorAircraftData(), (int)sensitivity.Value);
            UpdateUI();
        }

        private void UpdateUI()
        {
            /* Update UI Buttons */
            startBtn.IsEnabled = appState.Status != SimulatorStateDataStatus.RUNNING;
            stopBtn.IsEnabled = appState.Status == SimulatorStateDataStatus.RUNNING;

            /* Update UI Fields */
            vorFreq.Text = String.Format("{0:0.00} MHz", appState.VORFreqMhz);
            vorSignal.Text = appState.VORSignal ? Strings.Yes : Strings.No;
            vorRadial.Text = appState.VORRadial.ToString();
            vorOBS.Text = appState.VOROmniBearingSelector.ToString();
            vorFlag.Text = appState.VORFlag.ToString();
            magneticHeading.Text = appState.MagneticHeading.ToString();
            dmeDistance.Text = String.Format("{0:0.0} NM", Math.Round(appState.DMEDistance, 1));
            dmeSpeed.Text = String.Format("{0:0} Kt", Math.Round(appState.DMESpeed, 0));

            /* Update UI VOR State */
            actStatus.Text = appState.VorState.ToString();

            if (appStateExcercise != null)
            {
                atcInstruction.Foreground = Brushes.Black; 
                atcInstruction.Text = appStateExcercise.ToExerciseString();
            }
            else
            { 
                atcInstruction.Foreground = Brushes.Red;
                atcInstruction.Text = Strings.ExerciseMissingVorSignal;
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            simulatorBridge?.handleSimEvent(msg);
            return IntPtr.Zero;
        }

        public MainWindow()
        {
            InitializeComponent();
            /** Load default values */
            voiceEnabled.IsChecked = true;

            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
            FlightSimulatorBridge.FlightSimulatorBridge.StatusUpdate += SimulatorBridge_StatusUpdate;
            FlightSimulatorBridge.FlightSimulatorBridge.AircraftDataUpdate += SimulatorBridge_AircraftDataUpdate;
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            simulatorBridge = new FlightSimulatorBridge.FlightSimulatorBridge(debugCtl, handle);
            UpdateUI();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            simulatorBridge?.Dispose();
            simulatorBridge = null;
            appStateExcercise = null;
            appState = AppState.AppState.FromEmptyAppState();
            UpdateUI();
        }

        private void Sensitivity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            appState.Sensitivity = (int)e.NewValue;
        }
    }
}
