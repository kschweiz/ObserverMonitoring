using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace TransporteMonitoring
{
    public class Read
    {
        SortedList<string, List<Hashtable>> monitorObjectList;
        string[] fileList;

        public Read()
        {
            this.monitorObjectList = new SortedList<string, List<Hashtable>>();
            this.fileList = Directory.GetFiles(@"\\srhw8321\web\hw-extranet\EasyControls\Log\Monitoring");
        }

        public SortedList<string, List<Hashtable>> StartRead()
        {
            foreach(string file in fileList)
            {
                List<Hashtable> monitorObject = null;

                FileStream stream = new FileStream(file, FileMode.Open);
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    monitorObject = (List<Hashtable>)formatter.Deserialize(stream);
                    monitorObjectList.Add(Path.GetFileName(file), monitorObject);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    stream.Close();
                }
            }

           
            DeleteFiles();

            return monitorObjectList;
        }

        private void DeleteFiles()
        {
            foreach(string file in fileList)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
        }      
    }
}
