using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZJUGIS.CoordinateTrans.CommonClass
{
    public class NewTransForm
    {
        public static double GetA1(double E2)
        {
            double dValue = 1 + (3.0 / 4.0) * Math.Pow(E2, 1) + (45.0 / 64.0) * Math.Pow(E2, 2) + (175.0 / 256.0) * Math.Pow(E2, 3)
                          + (11025.0 / 16384.0) * Math.Pow(E2, 4) + (43659.0 / 65536.0) * Math.Pow(E2, 5) + (693693.0 / 1048576.0) * Math.Pow(E2, 6);
            return dValue;
        }

        public static double GetB1(double E2)
        {
            double dValue = (3.0 / 8.0) * Math.Pow(E2, 1) + (15.0 / 32.0) * Math.Pow(E2, 2) + (525.0 / 1024.0) * Math.Pow(E2, 3)
                          + (2205.0 / 4096.0) * Math.Pow(E2, 4) + (72765.0 / 131072.0) * Math.Pow(E2, 5) + (297297.0 / 524288.0) * Math.Pow(E2, 6);
            return dValue;
        }

        public static double GetC1(double E2)
        {
            double dValue = (15.0 / 256.0) * Math.Pow(E2, 2) + (105.0 / 1024.0) * Math.Pow(E2, 3) + (2205.0 / 16384.0) * Math.Pow(E2, 4)
                             + (10395.0 / 65536.0) * Math.Pow(E2, 5) + (1486485.0 / 8388608.0) * Math.Pow(E2, 6);
            return dValue;
        }

        public static double GetD1(double E2)
        {
            double dValue = (35.0 / 3072.0) * Math.Pow(E2, 3) + (105.0 / 4096.0) * Math.Pow(E2, 4)
                            + (10395.0 / 262144.0) * Math.Pow(E2, 5) + (55055.0 / 1048576.0) * Math.Pow(E2, 6);
            return dValue;
        }

        public static double GetE1(double E2)
        {
            double dValue = (315.0 / 131072.0) * Math.Pow(E2, 4) + (3465.0 / 524288.0) * Math.Pow(E2, 5) + (99099.0 / 8388608.0) * Math.Pow(E2, 6);
            return dValue;
        }

        public static double GetF1(double E2)
        {
            double dValue = (693.0 / 1310720.0) * Math.Pow(E2, 5) + (9009.0 / 5242880.0) * Math.Pow(E2, 6);
            return dValue;
        }

        public static double GetG1(double E2)
        {
            double dValue = (1001.0 / 8388608.0) * Math.Pow(E2, 6); ;
            return dValue;
        }
    }
}
