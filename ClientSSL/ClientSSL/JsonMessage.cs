﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ClientSSL
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
