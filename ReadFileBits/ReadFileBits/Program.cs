﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

namespace ReadFileBits
{
    class Program
    {
        static void Main(string[] args)
        {
            /*byte[] test = File.ReadAllBytes("C:\\Users\\nunesr_info\\Documents\\GitHub\\EditHexa\\TASKS.txt");

            foreach (byte item in test)
            {
                Console.WriteLine(String.Format("{0:X2}", Convert.ToInt32(item)));
            }*/

            HexaEditModel test = new HexaEditModel("C:\\Users\\nunesr_info\\Documents\\GitHub\\EditHexa\\TASKS.txt");
            string[][] tmp = test.Hexadecimal;

            /*Console.WriteLine(ConvertFromHexa.ConvertHexaToBinary("B7"));
            Console.WriteLine(test.ConvertHexaTo32BitsSigned(new Point(3, 14)));*/
            //Console.WriteLine(test.ConvertHexaTo32BitsUnsigned(new Point(4, 0)));

            Console.ReadLine();
        }
    }
}
