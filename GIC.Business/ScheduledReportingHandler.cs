using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using GIC.DataAccess;
using GIC.Common;
using GIC.Common.Model;

namespace GIC.Business
{
    public class ScheduledReportingHandler : ReportingHandlerBase
    {
        private int currentdayofweek = (int)DateTime.Now.DayOfWeek;
        private DateTime initialtime = DateTime.Now;
        //private string ccn = string.Empty;

        //private string starttime = DateTime.Now.ToShortDateString();
        //private string endtime = DateTime.Now.ToShortDateString();

        //private string databasename = string.Empty;
        //private string region = string.Empty;

        //public ScheduledReportingHandler(string databasename, string region)
        //{
        //    this.databasename = databasename;
        //    this.region = region;
        //}

        public ScheduledReportingHandler(string ccn)
        { 
        }

        public override void Process()
        {
            ExportDragonReport();

            //UploadGloviaReport();
            //UploadAXReport();
        }

        private void ExportDragonReport()
        {
            string connectionstring = ConfigFileUtility.GetValue("CCC_Dragon");
            //string sqlString = string.Empty;
            //string reportName = string.Empty;
            ScheduledReport tempScheduleReport = null;

            foreach (ScheduledReport scheduledReport in this.reportingconfig.ScheduledReports)
            {
                if (scheduledReport.ReportName.ToUpper().Contains("INVENTORY_VISIBILITY"))
                {
                    tempScheduleReport = scheduledReport;
                }
            }

            Console.WriteLine(string.Format("[{0}] - Starting pulling {1} report from CCC Dragon database...",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), tempScheduleReport.ReportName));
            log.Info(string.Format("Starting pulling {0} report from CCC Dragon database...", tempScheduleReport.ReportName));

            try
            {                
                using (OracleAccessDAO dao = new OracleAccessDAO(MiscUtility.DecryptPassword(connectionstring)))
                {
                    this.rawdata = dao.FetchDataFromDataBase(tempScheduleReport.SQLScript);
                }

                log.Info("Done!");
                Console.WriteLine(string.Format("[{0}] - Done!", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));

                string filename = String.Format("CCC_Dragon_Inventory_Visibility_{0}.csv", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                string fullfilename = Path.Combine(tempScheduleReport.SavedFolder, filename);

                Console.WriteLine(
                    string.Format("[{0}] - Start to export data into the excel file - {1}...", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), fullfilename));
                log.Info(
                    string.Format("[{0}] - Start to export data into the excel file - {1}...", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), fullfilename));

                ExcelFileUtility.ExportDataIntoExcelFile(fullfilename, rawdata.Tables[0]);

                log.Info("Done!");
                Console.WriteLine(string.Format("[{0}] - Done!", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            }
            catch (Exception e)
            {
                log.Error(string.Format("Function name: [ExportDragonReport], Error: {0}, StackTrack: {1}", e.Message, e.StackTrace));
                throw e;
            }
        }

        private void UploadAXReport()
        {
            string filename = string.Empty;
            string fileextension = string.Empty;
            string targetfilename = string.Empty;
            string fulltargetfilename = string.Empty;

            foreach (AXReportConfig axreportconfig in this.reportingconfig.AXReportConfigs)
            {
                DateTime starttime = DateTime.ParseExact(axreportconfig.StartTime, "HHmmss", System.Globalization.CultureInfo.CurrentCulture);

                if (axreportconfig.DayOfWeek.Contains(this.currentdayofweek.ToString())
                        && MiscUtility.ISSameTime(this.initialtime, starttime))
                {
                    DirectoryInfo dir = new DirectoryInfo(axreportconfig.SourceFolder);

                    foreach (FileInfo fi in dir.GetFiles())
                    {
                        filename = Path.GetFileNameWithoutExtension(fi.FullName);
                        fileextension = Path.GetExtension(fi.FullName);
                        targetfilename = string.Format("{0}_{1}{2}", filename, DateTime.Now.ToString("yyyyMMdd_HHmmss"), fileextension);

                        FileUtility.CopyFile(axreportconfig.TargetFolder, fi.FullName, targetfilename);

                        fulltargetfilename = ExcelFileUtility.SaveAsStandardFileFormat(Path.Combine(axreportconfig.TargetFolder, targetfilename));
                        //List<string> filenamelist = FileUtility.GetFileNameList(axreportconfig.TargetFolder);

                        string begintime = DateTime.Now.ToString();

                        ReadAXReport(fulltargetfilename);
                        ImportAXData(axreportconfig.CCN);

                        string endtime = DateTime.Now.ToString();

                        LogHistory(axreportconfig.ReportName, begintime, endtime, "AX", this.rawdata.Tables[0].Rows.Count);
                                                
                        File.Delete(fulltargetfilename);
                        FileUtility.MoveFile(axreportconfig.ArchiveFolder, Path.Combine(axreportconfig.TargetFolder, targetfilename));                         
                    }
                }                
            }
        }

        private void UploadGloviaReport()
        {
            //string keyvalue = string.Format("{0}_GLOVIA", ccn);
            List<AppSetting> appsettings = ConfigFileUtility.GetKeyValueList();

            foreach (ScheduledReport scheduledreport in this.reportingconfig.ScheduledReports)
            {
                foreach (AppSetting appsetting in appsettings)
                {
                    if (appsetting.Key.ToUpper().Contains(scheduledreport.CCN))
                    {
                        int startposition = appsetting.Key.ToString().IndexOf("_");         // Database connection string is "C10000_Glovia" in the configuration file
                        string ccnname = appsetting.Key.ToString().Substring(0, startposition);

                        DateTime starttime = DateTime.ParseExact(scheduledreport.StartTime, "HHmmss",
                            System.Globalization.CultureInfo.CurrentCulture);

                        if (scheduledreport.DayOfWeek.Contains(this.currentdayofweek.ToString())
                            && MiscUtility.ISSameTime(this.initialtime, starttime))
                        {
                            base.scheduledreport = scheduledreport;
                            base.gloviaconnectionstring = appsetting.KeyValue.ToString();

                            string begintime = DateTime.Now.ToString();

                            this.FetchGloviaData();
                            this.ImportGloviaData();
                            this.SaveReportingFile();

                            string endtime = DateTime.Now.ToString();

                            LogHistory(this.scheduledreport.ReportName, begintime, endtime, "GLOVIA", this.rawdata.Tables[0].Rows.Count);
                        }
                    }
                }
            }
        }
    }
}
