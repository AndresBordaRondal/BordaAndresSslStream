using System;
using System.Collections.Generic;
using System.Text;

namespace ServerSSL
{
    public class All
    {
        public All(List<GPU> gpus, List<Storage> storages)
        {
            GPUs = gpus;
            Storages = storages;
        }

        public List<GPU> GPUs { get; set; }
        public List<Storage> Storages { get; set; }
    }
}
