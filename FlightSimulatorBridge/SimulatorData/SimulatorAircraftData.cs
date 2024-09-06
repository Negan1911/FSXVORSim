using System.Runtime.InteropServices;

namespace FSXVORSim.SimulatorData
{
    enum SimulatorAircraftDataVORFlag
    {
        OFF = 0,
        TO = 1,
        FROM = 2
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct SimulatorAircraftData
    {
        internal bool VORSignal { get; set; }

        internal SimulatorAircraftDataVORFlag VORFlag { get; set; }

        internal double VORFreqMhz { get; set; }

        internal double VORRadial { get; set; }

        internal double VOROmniBearingSelector { get; set; }

        internal double DMEDistance { get; set; }

        internal double DMESpeed { get; set; }

        internal double MagneticHeading { get; set; }

    }
}
