using FSXVORSim.DebugCtl;
using FSXVORSim.SimulatorData;
using System;
using System.Windows;
using System.Windows.Interop;

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
        
        internal AppState.AppState appState { get; set; } = AppState.AppState.FromEmptyAppState();

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
            appState = AppState.AppState.FromStateAndAircraftKeypair(appState.AsSimulatorStateData(), data);
            updateUI();
        }

        private void SimulatorBridge_StatusUpdate(object sender, SimulatorStateData state)
        {
            appState = AppState.AppState.FromStateAndAircraftKeypair(state, appState.AsSimulatorAircraftData());
            updateUI();
        }

        private void updateUI()
        {
            /* Update UI Buttons */
            startBtn.IsEnabled = appState.Status != SimulatorStateDataStatus.RUNNING;
            stopBtn.IsEnabled = appState.Status == SimulatorStateDataStatus.RUNNING;

            /* Update UI Fields */
            vorFreq.Text = String.Format("{0:0.00} MHz", appState.VORFreqMhz);
            vorSignal.Text = appState.VORSignal.ToString();
            vorRadial.Text = Math.Round(appState.VORRadial, 0).ToString();
            vorOBS.Text = appState.VOROmniBearingSelector.ToString();
            vorFlag.Text = appState.VORFlag.ToString();
            magneticHeading.Text = Math.Round(appState.MagneticHeading, 0).ToString();
            dmeDistance.Text = String.Format("{0:0.0} NM", Math.Round(appState.DMEDistance, 1));
            dmeSpeed.Text = String.Format("{0:0} Kt", Math.Round(appState.DMESpeed, 0));

            /* TODO: Update UI VOR State */
            actStatus.Text = appState.VorState.ToString();
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
            appState = AppState.AppState.FromEmptyAppState();
            updateUI();
        }
    }
}
