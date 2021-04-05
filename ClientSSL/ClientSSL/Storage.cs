using System;
using System.Collections.Generic;
using System.Text;

namespace ClientSSL
{
    public class Storage
    {
        public Storage(double totalAvailableSpace, double totalSizeOfDrive, string rootDirectory)
        {
            TotalAvailableSpace = totalAvailableSpace;
            TotalSizeOfDrive = totalSizeOfDrive;
            RootDirectory = rootDirectory;
        }

        public double TotalAvailableSpace { get; set; }
        public double TotalSizeOfDrive { get; set; }
        public string RootDirectory { get; set; }
    }
}
