using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransporteMonitoring
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Transporte Monitoring";
            Connection.CreateConnection();
            System.Threading.Thread.Sleep(3000);

            ISubject monitoring = new MonitorSubject();

            IObserver display = new Display(monitoring);

            while (true)
            {
                monitoring.SynchronizeList();
                System.Threading.Thread.Sleep(10000);
            }
        }
    }
}
