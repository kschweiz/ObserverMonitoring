using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransporteMonitoring
{
    class Display : IObserver
    {
        protected ISubject subject;
        public Display(ISubject subject)
        {
            this.subject = subject;
            this.subject.RegisterObserver(this);
        }

        public void Update(SortedList<string, List<Hashtable>> monitorList)
        {
            UpdateConsole(monitorList);
        }

        private void UpdateConsole(SortedList<string, List<Hashtable>> monitorList)
        {
            IList<string> keyList = monitorList.Keys;
            IList<List<Hashtable>> valueList = monitorList.Values;

            for(int i = 0; i< keyList.Count; i++)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("[Object ID: {0}", keyList[i]+"]");

                List<Hashtable> run = valueList[i];
                foreach(Hashtable step in run)
                {
                    if ((bool)step["start"])
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("Date:\t");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write(step["date"].ToString() + "\t");

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("Ersteller:\t");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write(step["name"].ToString() + "\n");
                    }

                    if ((bool)step["end"] && !(bool)step["failbit"])
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("Status:\t");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Erfolgreich erstellt!");
                    }
                    else if ((bool)step["end"] && (bool)step["failbit"])
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("Status:\t");
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("FAILED\t\t\t");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("Step:\t\t");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine(step["step"]);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("MSG:\t");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine(step["message"].ToString());
                    }
                }
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------");

                System.Threading.Thread.Sleep(500);
            }
        }
    }
}
