using FSXVORSim.SimulatorData;
using System;

namespace FSXVORSim.AppState
{

    internal class AppState
    {
        public int Sensitivity { get; set; } = 5;

        public SimulatorStateDataStatus Status { get; set; }

        public String Error { get; set; }

        internal bool VORSignal { get; set; }

        internal SimulatorAircraftDataVORFlag VORFlag { get; set; }

        internal double VORFreqMhz { get; set; }

        internal int VORRadial { get; set; }

        internal int VOROmniBearingSelector { get; set; }

        internal double DMEDistance { get; set; }

        internal double DMESpeed { get; set; }

        internal int MagneticHeading { get; set; }

        internal AppStateVorState VorState { get; set; }

        internal SimulatorStateData AsSimulatorStateData()
        {
            return new SimulatorStateData
            {
                Status = Status,
                Error = Error
            };
        }

        internal SimulatorAircraftData AsSimulatorAircraftData()
        {
            return new SimulatorAircraftData
            {
                VORSignal = VORSignal,
                VORFlag = VORFlag,
                VORFreqMhz = VORFreqMhz,
                VORRadial = VORRadial,
                VOROmniBearingSelector = VOROmniBearingSelector,
                DMEDistance = DMEDistance,
                DMESpeed = DMESpeed,
                MagneticHeading = MagneticHeading
            };
        }

        internal static AppState FromStateAndAircraftKeypair(SimulatorStateData status, SimulatorAircraftData aircraft, int sensitivity)
        {
            return new AppState
            {
                Status = status.Status,
                Error = status.Error,
                VORSignal = aircraft.VORSignal,
                VORFlag = aircraft.VORFlag,
                VORFreqMhz = aircraft.VORFreqMhz,
                VORRadial = aircraft.VORRadial,
                VOROmniBearingSelector = aircraft.VOROmniBearingSelector,
                DMEDistance = aircraft.DMEDistance,
                DMESpeed = aircraft.DMESpeed,
                MagneticHeading = aircraft.MagneticHeading,
                Sensitivity = sensitivity,
                VorState = AppStateVorState.FromHeadingAndRadial(
                    aircraft.MagneticHeading,
                    aircraft.VORRadial,
                    sensitivity
                )
            };
        }

        

        internal static AppState FromEmptyAppState()
        {
            return new AppState
            {
                Status = SimulatorStateDataStatus.STOPPED,
                Error = null,
                VORSignal = false,
                VORFlag = SimulatorAircraftDataVORFlag.OFF,
                VORFreqMhz = 0,
                VORRadial = 0,
                VOROmniBearingSelector = 0,
                DMEDistance = 0,
                DMESpeed = 0,
                MagneticHeading = 0,
                Sensitivity = 5,
                VorState = AppStateVorState.FromHeadingAndRadial(0,0)
            };
        }
    }
}
