using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace WMS.Common
{
    public class Conversion
    {

        public static int toInt(object obj)
        {
            int d;
            if (int.TryParse(obj.ToString(), out d))
            {
                d = Convert.ToInt32(obj);
            }
            else
            {
                try
                {
                    d = (int)obj;

                }
                catch
                {
                    return d = 0;
                }

            }

            return d;
        }

        public static double toDbl(object obj)
        {
            double d;
            if (double.TryParse(obj.ToString(), out d))
            {
                d = Convert.ToDouble(obj);
            }
            else
            {
                d = 0;
            }

            return Math.Round(d, 2);
        }

        public static string ToDate(string dateTimeStr, string dateFmt)
        {
            try
            {
                string dt = Convert.ToDateTime(dateTimeStr).ToString(dateFmt);
                return dt;
            }
            catch
            {
                return null;
            }

            
        }

    }
}
