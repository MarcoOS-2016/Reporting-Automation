using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GIC.Business;
using GIC.Common;

namespace GIC.Reporting
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new ReportingConfigForm());
                }
                else
                {
                    //ScheduledReportingHandler reportprocess = 
                    //    new ScheduledReportingHandler(args[0].ToUpper().Trim(), args[0].ToUpper().Trim());

                    ScheduledReportingHandler reportprocess =
                        new ScheduledReportingHandler(args[0].ToUpper().Trim());
                    reportprocess.Process();
                }
            }
            catch (Exception ex)
            {
                //string foldername = Path.GetDirectoryName(ConfigFileUtility.GetValue("BackEndJobConfigFile"));
                FileUtility.SaveFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Error.txt"),
                    string.Format("Source:{0}, StackTrack:{1}, Error:{2}", ex.Source, ex.StackTrace, ex.Message));
                throw ex;
            }
        }
    }
}
