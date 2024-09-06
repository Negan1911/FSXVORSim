using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.FlightSimulator.SimConnect;
using FSXVORSim.SimulatorData;

enum DEFINITIONS
{
    SimulatorAircraftData,
}

enum REQUEST_ID
{
    REQUEST
}

namespace FSXVORSim.FlightSimulatorBridge
{
    internal class FlightSimulatorBridge: IDisposable
    {
        private CancellationTokenSource loopCancellationTokenSrc;

        private const int WM_USER_SIMCONNECT = 0x0402;

        private readonly SimConnect simconnect = null;

        private readonly DebugCtl.IDebuggable debuggable;

        public delegate void StatusUpdateHandler(object sender, SimulatorStateData state);

        public delegate void AircraftDataUpdateHandler(object sender, SimulatorAircraftData simulatorAircraftData);

        public static event StatusUpdateHandler StatusUpdate;

        public static event AircraftDataUpdateHandler AircraftDataUpdate;

        public FlightSimulatorBridge(DebugCtl.IDebuggable debuggable, IntPtr hWnd)
        {
            this.debuggable = debuggable;

            try
            {
                simconnect = new SimConnect("Managed Data Request", hWnd, WM_USER_SIMCONNECT, null, 0);

                // listen to connect and quit msgs
                simconnect.OnRecvOpen += new SimConnect.RecvOpenEventHandler(Simconnect_OnRecvOpen);
                simconnect.OnRecvQuit += new SimConnect.RecvQuitEventHandler(Simconnect_OnRecvQuit);

                // listen to exceptions
                simconnect.OnRecvException += new SimConnect.RecvExceptionEventHandler(Simconnect_OnRecvException);

                /** Map data from the struct */
                simconnect.AddToDataDefinition(DEFINITIONS.SimulatorAircraftData, "NAV HAS NAV:1", "bool", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.SimulatorAircraftData, "NAV TOFROM:1", "enum", SIMCONNECT_DATATYPE.INT32, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.SimulatorAircraftData, "NAV ACTIVE FREQUENCY:1", "MHz", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.SimulatorAircraftData, "NAV RADIAL:1", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.SimulatorAircraftData, "NAV OBS:1", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.SimulatorAircraftData, "NAV DME:1", "nautical miles", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.SimulatorAircraftData, "NAV DMESPEED:1", "knots", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.SimulatorAircraftData, "PLANE HEADING DEGREES MAGNETIC", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);

                /** Register the struct */
                simconnect.RegisterDataDefineStruct<SimulatorAircraftData>(DEFINITIONS.SimulatorAircraftData);

                /** Set event listener */
                simconnect.OnRecvSimobjectDataBytype += new SimConnect.RecvSimobjectDataBytypeEventHandler(Simconnect_OnRecvSimobjectDataBytype);

            }
            catch (COMException ex)
            {
                debuggable.Debug("Unable to connect to FSX: " + ex.Message);
                StatusUpdate?.Invoke(this, new SimulatorStateData { Status = SimulatorStateDataStatus.ERRORED, Error = ex.Message });
            }
        }

        /// <summary>
        /// Dispose connection to FSX.
        /// </summary>
        public void Dispose()
        {
            simconnect?.Dispose();
        }

        /// <summary>
        /// Delegated handler for the event of receiving data from FSX, which needs to be happening from a window event.
        /// </summary>
        /// <param name="msg">Message ID received</param>
        /// <returns>True if handled, false if not.</returns>
        public bool handleSimEvent(int msg)
        {
            if (msg == WM_USER_SIMCONNECT && simconnect != null)
            {
                try
                {
                    simconnect.ReceiveMessage();
                }
                catch (COMException ex)
                {
                    debuggable.Debug("Error receiving message from FSX: " + ex.Message);
                    StatusUpdate?.Invoke(this, new SimulatorStateData { Status = SimulatorStateDataStatus.ERRORED, Error = ex.Message });
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// This task run in a fixed amount of time and polls FSX for packet of data defined on our constructor,
        /// this is later going to be notified in the event "AircraftDataUpdate".
        /// </summary>
        /// <param name="interval">Interval to request data to FSX</param>
        /// <returns>The task scheduled</returns>
        private async Task LoopDataCollection(TimeSpan interval)
        {
            while (!loopCancellationTokenSrc.IsCancellationRequested)
            {
                simconnect.RequestDataOnSimObjectType(REQUEST_ID.REQUEST, DEFINITIONS.SimulatorAircraftData, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
                await Task.Delay(interval, loopCancellationTokenSrc.Token);
            }
        }

        /// <summary>
        /// Triggered when the connection with FSX has started.
        /// We start the loop packet collection from FSX here.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="data">Event data</param>
        void Simconnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            debuggable.Debug("Connected to FSX");
            StatusUpdate?.Invoke(this, new SimulatorStateData { Status = SimulatorStateDataStatus.RUNNING, Error = null });
            loopCancellationTokenSrc = new CancellationTokenSource();
            LoopDataCollection(TimeSpan.FromMilliseconds(1000));
        }

        /// <summary>
        /// Triggered when the connection to FSX is closed, because FSX was closed.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="data">Event data</param>
        void Simconnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            loopCancellationTokenSrc.Cancel();
            debuggable.Debug("FSX has exited");
            StatusUpdate?.Invoke(this, new SimulatorStateData { Status = SimulatorStateDataStatus.STOPPED, Error = null });
        }

        /// <summary>
        /// Triggered when an exception has been received from FSX.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="data">Exception data</param>
        void Simconnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            loopCancellationTokenSrc.Cancel();
            debuggable.Debug("FSX Bridge Exception received: " + data.dwException);
            StatusUpdate?.Invoke(this, new SimulatorStateData { Status = SimulatorStateDataStatus.ERRORED, Error = data.dwException.ToString() });
        }

        private void Simconnect_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            switch ((DEFINITIONS)data.dwRequestID)
            {
                case DEFINITIONS.SimulatorAircraftData:
                    SimulatorAircraftData d = (SimulatorAircraftData)data.dwData[0];
                    AircraftDataUpdate?.Invoke(this, d);

                    break;

                default:
                    debuggable.Debug("Unknown request ID: " + data.dwRequestID);
                    break;
            }
        }
    }
}
