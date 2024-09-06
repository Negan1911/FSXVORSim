using FSXVORSim.SimulatorData;
using System;
using System.Linq;
using System.Net.NetworkInformation;

namespace FSXVORSim.AppState
{
    enum AppStateVorState
    {
        INBOUND,
        OUTBOUND,
        CROSSING
    }

    internal class AppState
    {
        public SimulatorStateDataStatus Status { get; set; }

        public String Error { get; set; }

        internal bool VORSignal { get; set; }

        internal SimulatorAircraftDataVORFlag VORFlag { get; set; }

        internal double VORFreqMhz { get; set; }

        internal double VORRadial { get; set; }

        internal double VOROmniBearingSelector { get; set; }

        internal double DMEDistance { get; set; }

        internal double DMESpeed { get; set; }

        internal double MagneticHeading { get; set; }

        internal AppStateVorState VorState { get; set; }

        internal SimulatorStateData AsSimulatorStateData()
        {
            return new SimulatorStateData
            {
                Status = this.Status,
                Error = this.Error
            };
        }

        internal SimulatorAircraftData AsSimulatorAircraftData()
        {
            return new SimulatorAircraftData
            {
                VORSignal = this.VORSignal,
                VORFlag = this.VORFlag,
                VORFreqMhz = this.VORFreqMhz,
                VORRadial = this.VORRadial,
                VOROmniBearingSelector = this.VOROmniBearingSelector,
                DMEDistance = this.DMEDistance,
                DMESpeed = this.DMESpeed,
                MagneticHeading = this.MagneticHeading
            };
        }

        internal static AppState FromStateAndAircraftKeypair(SimulatorStateData status, SimulatorAircraftData aircraft)
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
                VorState = AppState.GetVorState((int)Math.Round(aircraft.MagneticHeading, 0), (int)Math.Round(aircraft.VORRadial, 0))
            };
        }

        private static AppStateVorState GetVorState(int heading, int radial)
        {
            var sensitivity = 5;
            var oppositeHeading = (heading + 180) % 360;

            var headingRange = Enumerable.Range(heading - sensitivity, sensitivity * 2 + 1).Select(x => x % 360);
            var oppositeHeadingRange = Enumerable.Range(oppositeHeading - sensitivity, sensitivity * 2 + 1).Select(x => x % 360);
            
            if (headingRange.Contains(radial))
                return AppStateVorState.OUTBOUND;

            if (oppositeHeadingRange.Contains(radial))
                return AppStateVorState.INBOUND;

            return AppStateVorState.CROSSING;
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
                MagneticHeading = 0
            };
        }
    }
}
