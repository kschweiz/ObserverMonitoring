using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransporteMonitoring
{
    class MonitorSubject : ISubject
    {
        SortedList<string, List<Hashtable>> monitorObjectList;
        List<Display> displayList = new List<Display>();


        public void NotifyObservers()
        {
            foreach(Display d in displayList)
            {
                d.Update(monitorObjectList); 
            }
        }

        public void RegisterObserver(IObserver o)
        {
            displayList.Add((Display)o);
        }

        public void RemoveObserver(IObserver o)
        {
            int i = displayList.IndexOf((Display)o);
            if (i > 0)
            {
                displayList.RemoveAt(i);
            }
        }

        public void SynchronizeList()
        {
            Read read = new Read();
            monitorObjectList = read.StartRead();
            if(monitorObjectList.Count > 0)
            {
                NotifyObservers();
                monitorObjectList = null;
            }
        }
    }
}
