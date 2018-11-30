using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransporteMonitoring
{
    class Connection
    {
        public static void CreateConnection()
        {
            System.Diagnostics.Process.Start("Connection.bat");
        }
    }
}
