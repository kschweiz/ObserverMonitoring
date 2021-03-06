﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.NotificationHubs;


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

        static async void test()
        {
            NotificationHubClient hub = NotificationHubClient
        .CreateClientFromConnectionString("<connection string with full access>", "<hub name>");
            var toast = @"<toast><visual><binding template=""ToastText01""><text id=""1"">Hello from a .NET App!</text></binding></visual></toast>";
            await hub.SendWindowsNativeNotificationAsync(toast);
        }
    }

}
