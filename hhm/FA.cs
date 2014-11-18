﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hhm {
    public class FA {

        public string description { get; set; }
        public string data { get; set; }

        public static FA readFromFile( string fileLocation ) {
            String[] file = File.ReadAllLines( fileLocation );
            string desc = file[0];
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i < file.Length; i++) {
                sb.Append( file[i] );
            }
            var result = new FA() { description = desc, data = sb.ToString() };
            return result;
        }
    }
}
