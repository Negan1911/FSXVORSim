using FSXVORSim.DebugCtl;
using FSXVORSim.SimulatorData;
using System;
using System.Windows;
using System.Windows.Interop;
using System.Globalization;
using System.Speech.Synthesis;
using FSXVORSim.Resources;

namespace FSXVORSim
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IntPtr handle;
        private DebugCtlUIBox debugCtl;
        private FlightSimulatorBridge.FlightSimulatorBridge simulatorBridge;
        private HwndSource _hwndSource;
        
        internal AppState.AppState AppState { get; set; } = FSXVORSim.AppState.AppState.FromEmptyAppState();
        internal AppState.AppStateExcercise AppStateExcercise = new AppState.AppStateExcercise();

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
            AppState = FSXVORSim.AppState.AppState.FromStateAndAircraftKeypair(AppState.AsSimulatorStateData(), data, (int)sensitivity.Value);

            if (AppStateExcercise?.Equals(AppState.VorState) ?? false)
            {
                //TODO: Speak...
                AppStateExcercise = new AppState.AppStateExcercise();
            }

            updateUI();
        }

        private void SimulatorBridge_StatusUpdate(object sender, SimulatorStateData state)
        {
            AppState = FSXVORSim.AppState.AppState.FromStateAndAircraftKeypair(state, AppState.AsSimulatorAircraftData(), (int)sensitivity.Value);
            updateUI();
        }

        private void updateUI()
        {
            /* Update UI Buttons */
            startBtn.IsEnabled = AppState.Status != SimulatorStateDataStatus.RUNNING;
            stopBtn.IsEnabled = AppState.Status == SimulatorStateDataStatus.RUNNING;

            /* Update UI Fields */
            vorFreq.Text = String.Format("{0:0.00} MHz", AppState.VORFreqMhz);
            vorSignal.Text = AppState.VORSignal ? Strings.Yes : Strings.No;
            vorRadial.Text = AppState.VORRadial.ToString();
            vorOBS.Text = AppState.VOROmniBearingSelector.ToString();
            vorFlag.Text = AppState.VORFlag.ToString();
            magneticHeading.Text = AppState.MagneticHeading.ToString();
            dmeDistance.Text = String.Format("{0:0.0} NM", Math.Round(AppState.DMEDistance, 1));
            dmeSpeed.Text = String.Format("{0:0} Kt", Math.Round(AppState.DMESpeed, 0));

            /* TODO: Update UI VOR State */
            atcInstruction.Text = AppStateExcercise.ToString();
            actStatus.Text = AppState.VorState.ToString();
            
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            simulatorBridge?.handleSimEvent(msg);
            return IntPtr.Zero;
        }

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
            FlightSimulatorBridge.FlightSimulatorBridge.StatusUpdate += SimulatorBridge_StatusUpdate;
            FlightSimulatorBridge.FlightSimulatorBridge.AircraftDataUpdate += SimulatorBridge_AircraftDataUpdate;
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            simulatorBridge = new FlightSimulatorBridge.FlightSimulatorBridge(debugCtl, handle);
            updateUI();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            simulatorBridge?.Dispose();
            simulatorBridge = null;
            AppState = FSXVORSim.AppState.AppState.FromEmptyAppState();
            updateUI();
        }

        private void Sensitivity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AppState.Sensitivity = (int)e.NewValue;
        }
    }
}
