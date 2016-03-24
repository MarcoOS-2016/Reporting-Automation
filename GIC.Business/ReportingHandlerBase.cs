using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GIC.Common;
using GIC.Common.Model;
using GIC.DataAccess;
using log4net;
using log4net.Config;

namespace GIC.Business
{
    public abstract class ReportingHandlerBase
    {
        protected static readonly ILog log = LogManager.GetLogger(typeof(ReportingHandlerBase));
        protected ReportingConfig reportingconfig = FileUtility.LoadReportingConfig();
        protected ScheduledReport scheduledreport = new ScheduledReport();
        protected string gicconnectionstring = ConfigFileUtility.GetValue("GIC_Database");
        protected string gloviaconnectionstring = string.Empty;
        protected DataSet rawdata = new DataSet();

        public abstract void Process();

        public ReportingHandlerBase()
        {
            XmlConfigurator.Configure(new System.IO.FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config")));
        }        

        protected virtual void FetchGloviaData()
        {
            Console.WriteLine(string.Format("[{0}] - Starting pulling {1} report for CCN {2} from Glovia database...",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), this.scheduledreport.ReportName, this.scheduledreport.CCN));
            log.Info(string.Format("Starting pulling {0} report for CCN {1} from Glovia database...",
                this.scheduledreport.ReportName, this.scheduledreport.CCN));
            try
            {
                using (OracleAccessDAO dao = new OracleAccessDAO(MiscUtility.DecryptPassword(gloviaconnectionstring)))
                {
                    this.rawdata = dao.FetchDataFromDataBase(this.scheduledreport.SQLScript);
                }

                log.Info("Done!");
                Console.WriteLine(string.Format("[{0}] - Done!", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));

                log.Info("The Raw data has been pulled from Glovia database, It will be exported into an excel file...");
                Console.WriteLine(string.Format("[{0}] The Raw data has been pulled from Glovia database, It will be exported into an excel file...", 
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            }
            catch (Exception e)
            {
                log.Error(string.Format("Function name: [FetchRawData], Error: {0}, StackTrack: {1}", e.Message, e.StackTrace));
                throw e;
            }
        }

        protected virtual void ImportGloviaData()
        {
            // if the importdatabaseflag is false, then means that don't need to save raw data into database.
            if (this.scheduledreport.ImportDataBaseFlag.Equals("FALSE")) return;

            log.Info("Starting import Glovia report into MS SQLServer Database...");
            Console.WriteLine(string.Format("[{0}] - Starting import Glovia report into MS SQLServer Database...", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));

            try
            {
                string currentdate = DateTime.Now.ToShortDateString();

                using (SQLServerAccessDAO dao = new SQLServerAccessDAO(MiscUtility.DecryptPassword(gicconnectionstring)))
                {
                    if (this.scheduledreport.TableAccess == "APPEND")
                    {
                        int recordcount =
                            dao.GetRecordCountByCCNANDSnapShotDate(this.scheduledreport.TableName, this.scheduledreport.CCN, currentdate);

                        if (recordcount > 0)
                        {
                            dao.DeleteDataByCCNAndSnapShotDate(this.scheduledreport.TableName, this.scheduledreport.CCN, currentdate);
                        }
                    }
                    else if (this.scheduledreport.TableAccess == "OVERWRITE")
                    {
                        dao.DeleteDataByCCN(this.scheduledreport.TableName, this.scheduledreport.CCN);
                    }   

                    switch (this.scheduledreport.TableName)
                    {
                        case "STOCK_STATUS_GLOVIA":

                            dao.InsertStockStatusGlovia(this.rawdata);
                            break;

                        case "PART_DETAIL":

                            dao.InsertPartDetail(this.rawdata, this.scheduledreport.CCN);
                            break;
                    }

                    log.Info("Done!");
                    Console.WriteLine(string.Format("[{0}] - Done!", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                }
            }
            catch (Exception e)
            {
                log.Error(
                    string.Format("Function name: [ImportGloviaData], Error: {0}, StackTrack: {1}", e.Message, e.StackTrace));
                throw e;
            }
        }

        protected virtual void SaveReportingFile()
        {
            if (this.scheduledreport.ExportToFileFlag.Equals("FALSE")) return;

            string filename = String.Format("{0}-{1}-{2}.csv",
                this.scheduledreport.CCN, this.scheduledreport.ReportName, DateTime.Now.ToString("yyyy_MM_dd"));
            string fullfilename = Path.Combine(this.scheduledreport.SavedFolder, filename);

            try
            {
                ExcelFileUtility.ExportDataIntoExcelFile(fullfilename, rawdata.Tables[0]);

                Console.WriteLine(string.Format("[{0}] - The Glovia report file {1} has been created already.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), filename));
                log.Info(string.Format("The Glovia report file {0} has been created already.", filename));                
            }
            catch (Exception e)
            {
                log.Error(string.Format("Function name: [SaveReportFile], Error: {0}, StackTrack: {1}", e.Message, e.StackTrace));
                throw e;
            }
        }

        protected virtual void ReadAXReport(string filename)
        {
            Console.WriteLine(string.Format("[{0}] - Starting read AX report file - {1}...", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), filename));
            log.Info(string.Format("Starting read AX report file - {0}...", filename));

            try
            {
                string sheetname = ExcelFileUtility.GetExcelFileSheetName(filename, true);

                using (ExcelAccessDAO dao = new ExcelAccessDAO(filename))
                {
                    this.rawdata = dao.ReadExcelFile(sheetname);
                }

                log.Info("Done!");
                Console.WriteLine(string.Format("[{0}] - Done!", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            }
            catch (Exception e)
            {
                log.Error(string.Format("Function name: [ReadAXReport], Error: {0}, StackTrack: {1}", e.Message, e.StackTrace));
                throw e;
            }
        }

        protected virtual void ImportAXData(string ccn)
        {
            log.Info("Starting import AX data into MS SQL Database...");
            Console.WriteLine(string.Format("[{0}] - Starting import AX data into MS SQL Database...", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));

            try
            {
                using (SQLServerAccessDAO dao = new SQLServerAccessDAO(MiscUtility.DecryptPassword(gicconnectionstring)))
                {
                    dao.InsertStockStatusAX(ccn, this.rawdata);
                }

                log.Info("Done!");
                Console.WriteLine(string.Format("[{0}] - Done!", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            }
            catch (Exception e)
            {
                log.Error(string.Format("Function name: [ImportAXData], Error: {0}, StackTrack: {1}", e.Message, e.StackTrace));
                throw e;
            }
        }
        
        // Log pulling report record into MS SQLServer database
        protected virtual void LogHistory(string reportname, string starttime, string endtime, string category, Int32 totalrecordnumber)
        {
            using (SQLServerAccessDAO dao = new SQLServerAccessDAO(MiscUtility.DecryptPassword(gicconnectionstring)))
            {
                dao.InsertLog(reportname, category, starttime, endtime, totalrecordnumber);
            }
        }        
        
        #region FetchATSData
        //protected virtual void FetchATSData(string region)
        //{
        //    DataTable datatable = null;

        //    try
        //    {
        //        using (SQLServerAccessDAO dao = new SQLServerAccessDAO(MiscUtility.DecryptPassword(connectionstring)))
        //        {                    
        //            datatable = dao.GetCountryCodeByRegion(region);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        log.Error(string.Format("Function name: [FetchATSData], Error: {0}, StackTrack: {1}", e.Message, e.StackTrace));
        //        throw e;
        //    }

        //    for (int index = 0; index < datatable.Rows.Count; index++)
        //    {
        //        string countryid = datatable.Rows[index]["Id"].ToString();
        //        string countrycode = datatable.Rows[index]["CountryCode"].ToString();

        //        GIC.ATSWcfService.ATSServiceReference.OnHandInTransit[] inventorydata = FetchOnHandInTransitInventory(countryid);

        //        if (inventorydata.Length != 0)
        //            WriteOnHandInTransitInventoryData(region, countrycode, inventorydata);
        //    }
        //}

        //protected GIC.ATSWcfService.ATSServiceReference.OnHandInTransit[] FetchOnHandInTransitInventory(string countryid)
        //{
        //    GIC.ATSWcfService.ATSServiceReference.FetchGOPOnHandInTransitResponse response = null;
        //    GIC.ATSWcfService.ATSServiceReference.GOPServiceClient client = null;
        //    GIC.ATSWcfService.ATSServiceReference.SerachGOPRequest request = null;

        //    try
        //    {
        //        client = new ATSWcfService.ATSServiceReference.GOPServiceClient();
        //        request = new ATSWcfService.ATSServiceReference.SerachGOPRequest();
        //        request.Country = countryid;
        //        response = client.FetchGOPOnHandInTransit(request);
        //    }
        //    catch (Exception e)
        //    {
        //        log.Error(string.Format("Function name: [FetchOnHandInTransitInventory], Error: {0}, StackTrack: {1}", e.Message, e.StackTrace));
        //        throw e;
        //    }

        //    return response.OnHandInTransit;
        //}

        //protected void WriteOnHandInTransitInventoryData(
        //    string region, string countrycode, GIC.ATSWcfService.ATSServiceReference.OnHandInTransit[] onhandintransitinventorydata)
        //{
        //    DataTable datatable = new DataTable();
        //    datatable.Columns.Add("CountryCode");
        //    datatable.Columns.Add("ASNID");
        //    datatable.Columns.Add("BrandName");
        //    datatable.Columns.Add("CatalogID");
        //    datatable.Columns.Add("Catalogs");
        //    datatable.Columns.Add("CountryID");
        //    datatable.Columns.Add("Identifier");
        //    datatable.Columns.Add("InTransitQty");
        //    datatable.Columns.Add("OnHandDetails");
        //    datatable.Columns.Add("OnHandQty");
        //    datatable.Columns.Add("PartNo");
        //    datatable.Columns.Add("ProductID");
        //    datatable.Columns.Add("SKUNo");
        //    datatable.Columns.Add("SnapShotDate");

        //    DataRow datarow = null;
        //    for (int index = 0; index < onhandintransitinventorydata.Length; index++)
        //    {
        //        datarow = datatable.NewRow();

        //        datarow["CountryCode"] = countrycode;
        //        datarow["ASNID"] = onhandintransitinventorydata[index].ASNID;
        //        datarow["BrandName"] = onhandintransitinventorydata[index].BrandName;
        //        datarow["CatalogID"] = onhandintransitinventorydata[index].CatalogID;
        //        datarow["Catalogs"] = onhandintransitinventorydata[index].Catalogs;
        //        datarow["CountryID"] = onhandintransitinventorydata[index].CountryID;
        //        datarow["Identifier"] = onhandintransitinventorydata[index].Identifier;
        //        datarow["InTransitQty"] = onhandintransitinventorydata[index].InTransitQty;
        //        datarow["OnHandDetails"] = onhandintransitinventorydata[index].OnHandDetails;
        //        datarow["OnHandQty"] = onhandintransitinventorydata[index].OnHandQty;
        //        datarow["PartNo"] = onhandintransitinventorydata[index].PartNo;
        //        datarow["ProductID"] = onhandintransitinventorydata[index].ProductID;
        //        datarow["SKUNo"] = onhandintransitinventorydata[index].SKUNo;
        //        datarow["SnapShotDate"] = DateTime.Now.ToShortDateString();

        //        datatable.Rows.Add(datarow);
        //    }

        //    try
        //    {
        //        using (SQLServerAccessDAO dao = new SQLServerAccessDAO(MiscUtility.DecryptPassword(connectionstring)))
        //        {
        //            switch (region)
        //            {
        //                case "APJ":
        //                    dao.BulkWriteToServer(datatable, "ATSOnHandInTransit_APJ");
        //                    break;

        //                case "AMER":
        //                    dao.BulkWriteToServer(datatable, "ATSOnHandInTransit_AMER");
        //                    break;

        //                case "EMEA":
        //                    dao.BulkWriteToServer(datatable, "ATSOnHandInTransit_EMEA");
        //                    break;
        //            }
                    
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        log.Error(string.Format("Function name: [WriteOnHandInTransitInventoryData], Error: {0}, StackTrack: {1}", e.Message, e.StackTrace));
        //        throw e;
        //    }
        //}
        #endregion
    }
}