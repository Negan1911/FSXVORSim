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
        private DebugCtlUIBox debugCtl;
        private FlightSimulatorBridge.FlightSimulatorBridge simulatorBridge;
        private HwndSource _hwndSource;
        internal SimulatorStateData SimulatorStateData { get; set; }
        internal SimulatorAircraftData SimulatorAircraftData { get; set; }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            simulatorBridge?.Dispose();
            _hwndSource.RemoveHook(WndProc);
            base.OnClosed(e);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            _hwndSource = HwndSource.FromHwnd(hwnd);
            _hwndSource.AddHook(WndProc);

            debugCtl = new DebugCtlUIBox(debugBox);
            simulatorBridge = new FlightSimulatorBridge.FlightSimulatorBridge(debugCtl, hwnd);
            simulatorBridge.StatusUpdate += SimulatorBridge_StatusUpdate;
            simulatorBridge.AircraftDataUpdate += SimulatorBridge_AircraftDataUpdate;
        }

        private void SimulatorBridge_AircraftDataUpdate(object sender, SimulatorAircraftData data)
        {
            SimulatorAircraftData = data;

            /* Update UI, find a better way to do it */
            vorFreq.Text = data.VORFreqMhz.ToString();
            vorSignal.Text = data.VORSignal.ToString();
            vorRadial.Text = data.VORRadial.ToString();
            vorOBS.Text = data.VOROmniBearingSelector.ToString();
            vorFlag.Text = data.VORFlag.ToString();
            magneticHeading.Text = data.MagneticHeading.ToString();
            dmeDistance.Text = data.DMEDistance.ToString();
            dmeSpeed.Text = data.DMESpeed.ToString();

        }

        private void SimulatorBridge_StatusUpdate(object sender, SimulatorStateData state)
        {
            SimulatorStateData = state;
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
        }
    }
}
