using System.Text;

namespace KOXP.Core
{
    public class Helper
    {
        public static string StringToHex(string Value)
        {
            byte[] bytes = Encoding.Default.GetBytes(Value);
            string retval = "";
            foreach (byte b in bytes)
                retval += b.ToString("X2").ToUpper();
            return retval;
        }

        public static string AlignDWORD(IntPtr Value)
        {
            return AlignDWORD(Value.ToInt32());
        }

        public static string AlignDWORD(long Value)
        {
            string ADpStr, ADpStr2, ADresultStr;

            ADpStr = Convert.ToString(Value, 16);
            ADpStr2 = "";

            int ADpStrLength = ADpStr.Length;

            int i;
            for (i = 0; i < 8 - ADpStrLength; i++)
            {
                ADpStr2 = ADpStr2.Insert(i, "0");
            }

            int j = 0;
            int t = i;
            for (i = t; i < 8; i++)
            {
                ADpStr2 = ADpStr2.Insert(i, ADpStr[j].ToString());
                j++;
            }

            ADresultStr = "";

            ADresultStr = ADresultStr.Insert(0, ADpStr2[6].ToString());
            ADresultStr = ADresultStr.Insert(1, ADpStr2[7].ToString());
            ADresultStr = ADresultStr.Insert(2, ADpStr2[4].ToString());
            ADresultStr = ADresultStr.Insert(3, ADpStr2[5].ToString());
            ADresultStr = ADresultStr.Insert(4, ADpStr2[2].ToString());
            ADresultStr = ADresultStr.Insert(5, ADpStr2[3].ToString());
            ADresultStr = ADresultStr.Insert(6, ADpStr2[0].ToString());
            ADresultStr = ADresultStr.Insert(7, ADpStr2[1].ToString());

            return ADresultStr.ToUpper();
        }

        public static byte[] StringToByte(string text)
        {
            byte[] tmpbyte = new byte[text.Length / 2];
            int count = 0;
            for (int i = 0; i < text.Length; i += 2)
            {
                byte val = byte.MinValue;
                try
                {
                    if (text.Substring(i, 2) != "XX")
                    {
                        val = byte.Parse(text.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
                    }

                    tmpbyte[count] = val;
                    count++;
                }
                catch (Exception)
                {
                }
            }
            return tmpbyte;
        }
    }
}
