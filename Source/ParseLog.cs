//
// Copyright (c) 2017 Equine Smart Bits, LLC. All rights reserved

using System;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;

namespace ESB
{
    public class ParseLog
    {
        public static void GetData(string line_input, out int hr, out int sp, out double temp, out bool calculated)
        {
            hr = sp = -1;
            temp = -1.0;
            calculated = false;

            string line = line_input.Trim();

            if (line.Length == 0)
                return;

            // T=%.1fF
            Regex rgx1 = new Regex(@"T=(.*)F");
            MatchCollection matches = rgx1.Matches(line);

            if (matches.Count == 1)
            {
                GroupCollection data = matches[0].Groups;

                Debug.Assert(data.Count == 2);

                temp = Convert.ToDouble(data[1].Value);
                return;
            }

            // Raw: TS=%d, HR=%d (valid=%d), SpO=%d (valid=%d)
            Regex rgx2 = new Regex(@"Raw TS=(-?\d+), HR=(\d+) \(valid=(\d)\), SP=(\d+) \(valid=(\d)\)");

            matches = rgx2.Matches(line);

            if (matches.Count == 1)
            {
                GroupCollection data = matches[0].Groups;

                Debug.Assert(data.Count == 6);

                long ts = Convert.ToInt64(data[1].Value);
                hr = Convert.ToInt32(data[2].Value);
                int hr_valid = Convert.ToInt32(data[3].Value);
                sp = Convert.ToInt32(data[4].Value);
                int sp_valid = Convert.ToInt32(data[5].Value);
                return;
            }

            // Calculated HR=%d, SpO2=%d
            Regex rgx3 = new Regex(@"Calculated HR=(\d+), SP=(\d+)");

            matches = rgx3.Matches(line);

            if (matches.Count == 1)
            {
                GroupCollection data = matches[0].Groups;

                Debug.Assert(data.Count == 3);

                hr = Convert.ToInt32(data[1].Value);
                sp = Convert.ToInt32(data[2].Value);
                calculated = true;
                return;
            }
        }
    };

    public class WriteLog
    {
        private static StreamWriter s_sr = null;

        public WriteLog() { Init();  }

        ~WriteLog() { Close();  }

        public static bool Init()
        {
            if (s_sr != null)
                return true;

            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filename = Path.Combine(path, "esblog.txt");

            try
            {
                s_sr = new StreamWriter(filename, true);
            }
            catch (Exception e)
            {
                s_sr = null;
                return false;
            }

            return true;
        }

        public static void Close()
        {
            if (s_sr != null)
            {
                s_sr.Close();
                s_sr = null;
            }
        }

        public void Write(string str)
        {
            if (s_sr != null)
            {
                s_sr.Write(str);
            }
        }

        public void WriteLn(string str)
        {
            if (s_sr != null)
            {
                s_sr.WriteLine(str);
            }
        }
    }
}
