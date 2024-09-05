using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FSXVORSim.DebugCtl
{
    internal class DebugCtlUIBox: IDebuggable
    {
        private ListBox control;

        public DebugCtlUIBox(ListBox control)
        {
            this.control = control;
        }

        public void Debug(string message)
        {
            this.control.Items.Add($"${DateTime.Now.ToString()} - {message}");
        }
    }
}
