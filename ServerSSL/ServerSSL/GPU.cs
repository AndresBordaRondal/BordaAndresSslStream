using System;
using System.Collections.Generic;
using System.Text;

namespace ServerSSL
{
    public class GPU
    {
        public GPU(string name, string status, string adapterRAM, string adapterDACType, string driverVersion)
        {
            Name = name;
            Status = status;
            AdapterRAM = adapterRAM;
            AdapterDACType = adapterDACType;
            DriverVersion = driverVersion;
        }

        public string Name { get; set; }
        public string Status { get; set; }
        public string AdapterRAM { get; set; }
        public string AdapterDACType { get; set; }
        public string DriverVersion { get; set; }
    }
}
