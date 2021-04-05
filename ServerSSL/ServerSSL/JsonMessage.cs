using System;
using System.Collections.Generic;
using System.Text;

namespace ServerSSL
{
    public class JsonMessage
    {
        public JsonMessage(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
    }
}
