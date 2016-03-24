using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Configuration;
using System.Collections.Generic;
using Microsoft.Office.Interop.Excel;

namespace GIC.Common
{
    public class MiscUtility
    {
        public static Boolean ISSameTime(DateTime firsttime, DateTime secondtime)
        {
            //bool flag = false;

            TimeSpan ts1 = new TimeSpan(firsttime.Ticks);
            TimeSpan ts2 = new TimeSpan(secondtime.Ticks);
            TimeSpan ts = ts1.Subtract(ts2).Duration();

            //if (Convert.ToInt16(ts.Hours) == 0 && Convert.ToInt16(ts.Minutes) == 0)
            //    flag = true;

            if (Convert.ToInt16(ts.Hours) == 0)
                return true;

            return false;
        }

        public static string AddSingleQuotation(string sourcestring)
        {
            StringBuilder sb = new StringBuilder();

            foreach (string tempstring in sourcestring.Split(','))
                sb.Append(string.Format("'{0}',", tempstring.Trim()));

            sb.Remove(Convert.ToString(sb).Length - 1, 1);

            return sb.ToString();
        }

        public static string DecryptPassword(string connectionstring)
        {
            string passwordstring = "Password=";

            if (connectionstring.ToUpper().Contains(passwordstring.ToUpper()))
            {
                int startposition = connectionstring.IndexOf(passwordstring) + passwordstring.Length;
                int endposition = connectionstring.LastIndexOf(";");

                string existingpassword = connectionstring.Substring(startposition, endposition - startposition);

                StringBuilder sb = new StringBuilder(connectionstring);
                sb.Replace(existingpassword, PasswordUtility.DesDecrypt(existingpassword));

                return sb.ToString();
            }

            return string.Empty;
        }

        public static string Char2HTML(string sourcestring)
        {
            StringBuilder sb = new StringBuilder(sourcestring);

            sb.Replace("&lt;", "<");
            sb.Replace("&gt;", ">");

            return sb.ToString();
        }

        public static List<string> Sectionalization(List<string> list)
        {
            int limitedlength = 1000;
            List<string> Sectionalizationlist = new List<string>();
            StringBuilder sqlstring = new StringBuilder();

            try
            {
                if (list.Count > 0)
                {
                    if (list.Count <= limitedlength)
                    {
                        for (int count = 0; count < list.Count; count++)
                        {
                            sqlstring.Append("'");
                            sqlstring.Append(list[count]);
                            sqlstring.Append("',");
                        }

                        sqlstring.Remove(sqlstring.Length - 1, 1);
                        Sectionalizationlist.Add(sqlstring.ToString());
                    }
                    else
                    {
                        int multiple = list.Count / limitedlength;
                        int remainder = list.Count % limitedlength;

                        for (int count = 1; count <= multiple * limitedlength; count++)
                        {
                            if (count > 0 && (count % limitedlength == 0))
                            {
                                sqlstring.Remove(sqlstring.Length - 1, 1);
                                Sectionalizationlist.Add(sqlstring.ToString());
                                sqlstring.Clear();
                            }

                            sqlstring.Append("'");
                            sqlstring.Append(list[count - 1]);
                            sqlstring.Append("',");
                        }

                        int startindex = multiple * limitedlength;
                        for (int count = startindex; count < list.Count; count++)
                        {
                            sqlstring.Append("'");
                            sqlstring.Append(list[count]);
                            sqlstring.Append("',");
                        }

                        sqlstring.Remove(sqlstring.Length - 1, 1);
                        Sectionalizationlist.Add(sqlstring.ToString());
                    }
                }

                return Sectionalizationlist;
            }
            catch (System.Exception ex)
            {                
                throw ex;
            }
        }
    }
}
