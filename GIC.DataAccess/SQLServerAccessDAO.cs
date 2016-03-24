using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace GIC.DataAccess
{
    public class SQLServerAccessDAO : SqlServerDataBaseAccess
    {
        private string connectionstring = string.Empty;

        public SQLServerAccessDAO(string connectionstring)
            : base(connectionstring)
        {
            this.connectionstring = connectionstring;

            try
            {
                if (base.connection.State == System.Data.ConnectionState.Closed)
                {
                    base.connection.Open();
                }
            }
            catch
            {
                throw;
            }
        }

        // Get CountryCode from GIC database
        public DataTable GetCountryCode()
        {            
            string sqlstring = "SELECT * FROM CountryCode";
            DataTable datatable = this.ExecuteQuery(sqlstring).Tables[0];

            return datatable;
        }

        public DataTable GetCountryCodeByRegion(string regionname)
        {
            string sqlstring = string.Format("SELECT * FROM CountryCode WHERE Region = '{0}'", regionname);
            DataTable datatable = this.ExecuteQuery(sqlstring).Tables[0];

            return datatable;
        }

        // Get FacilityCode from GIC database
        public DataTable GetFacilityCode()
        {   
            string sqlstring = "SELECT * FROM FacilityCode";
            DataTable datatable = this.ExecuteQuery(sqlstring).Tables[0];

            return datatable;
        }

        // Get Catalogs from GIC database
        public DataTable GetCatalogs()
        {   
            string sqlstring = "SELECT * FROM Catalogs";
            DataTable datatable = this.ExecuteQuery(sqlstring).Tables[0];

            return datatable;
        }

        public Int32 GetRecordCountByCCNANDSnapShotDate(string tablename, string ccn, string date)
        {
            SqlDataReader recordcount = null;

            string sqlString = 
                String.Format("SELECT count(*) FROM {0} WHERE ccn = '{1}' AND Snapshot_Date = '{2}'", tablename, ccn, date);
            recordcount = this.ExecuteReader(sqlString);
            recordcount.Read();
            Int32 totalrecord = recordcount.GetInt32(0);
            recordcount.Close();

            return Convert.ToInt32(totalrecord);
        }

        public void BulkWriteToServer(DataTable datatable, string tablename)
        {
            using (SqlBulkCopy bulkcopy = new SqlBulkCopy(this.connection))
            {
                bulkcopy.DestinationTableName = tablename;

                try
                {
                    bulkcopy.WriteToServer(datatable);
                }
                catch
                {
                    throw;
                }
            }
        }

        public void DeleteDataFromDB(string tablename)
        {
            string sqlString = String.Format("DELETE FROM {0}", tablename);
            this.ExecuteNonQuery(sqlString);
        }

        public void DeleteDataByCCN(string tablename, string ccn)
        {
            string sqlString = String.Format("DELETE FROM {0} WHERE ccn = '{1}'", tablename, ccn);
            this.ExecuteNonQuery(sqlString);
        }

        public void DeleteDataBySnapShotDate(string tablename, string date)
        {
            string sqlString = String.Format("DELETE FROM {0} WHERE snapshot_date = '{1}'", tablename, date);
            this.ExecuteNonQuery(sqlString);
        }

        public void DeleteDataByCCNAndSnapShotDate(string tablename, string ccn, string date)
        {
            string sqlString = 
                String.Format("DELETE FROM {0} WHERE ccn = '{1}' AND snapshot_date = '{2}'", tablename, ccn, date);
            this.ExecuteNonQuery(sqlString);
        }

        // Insert records into the Part_Detail table in MS Access Database
        public void InsertPartDetail(DataSet ds, string ccn)
        {
            DateTime currentdate = DateTime.Now.Date;

            SqlTransaction transaction = null;
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = this.connection;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Insert_Part_Detail";

            try
            {
                DataTable datatable = ds.Tables[0];
                if (datatable.Rows.Count != 0)
                {
                    if (this.connection.State == System.Data.ConnectionState.Closed)
                        connection.Open();

                    transaction = cmd.Connection.BeginTransaction();
                    cmd.Transaction = transaction;

                    cmd.Parameters.Add("CCN", SqlDbType.Char);
                    cmd.Parameters.Add("ITEM", SqlDbType.Char);
                    cmd.Parameters.Add("DESCRIPTION", SqlDbType.Char);
                    cmd.Parameters.Add("COMMODITY", SqlDbType.Char);                    
                    cmd.Parameters.Add("ISSUE_CODE", SqlDbType.Char);
                    cmd.Parameters.Add("BOX_CODE", SqlDbType.Char);
                    cmd.Parameters.Add("BULK_EXPENSED", SqlDbType.Char);
                    cmd.Parameters.Add("CONSIGNED", SqlDbType.Char);
                    cmd.Parameters.Add("COA", SqlDbType.Char);
                    cmd.Parameters.Add("RETURN_ON_ASN", SqlDbType.Char);
                    cmd.Parameters.Add("PAU", SqlDbType.Char);
                    cmd.Parameters.Add("SNAPSHOT_DATE", SqlDbType.SmallDateTime);

                    for (int rowcount = 0; rowcount < datatable.Rows.Count; rowcount++)
                    {
                        cmd.Parameters["CCN"].Value = datatable.Rows[rowcount][0].ToString();
                        cmd.Parameters["ITEM"].Value = datatable.Rows[rowcount][1].ToString();
                        cmd.Parameters["DESCRIPTION"].Value = datatable.Rows[rowcount][2].ToString();
                        cmd.Parameters["COMMODITY"].Value = datatable.Rows[rowcount][3].ToString();                        
                        cmd.Parameters["ISSUE_CODE"].Value = datatable.Rows[rowcount][4].ToString();
                        cmd.Parameters["BOX_CODE"].Value = datatable.Rows[rowcount][5].ToString();
                        cmd.Parameters["BULK_EXPENSED"].Value = datatable.Rows[rowcount][6].ToString();
                        cmd.Parameters["CONSIGNED"].Value = datatable.Rows[rowcount][7].ToString();
                        cmd.Parameters["COA"].Value = datatable.Rows[rowcount][8].ToString();
                        cmd.Parameters["RETURN_ON_ASN"].Value = datatable.Rows[rowcount][9].ToString();
                        cmd.Parameters["PAU"].Value = datatable.Rows[rowcount][10].ToString();
                        cmd.Parameters["SNAPSHOT_DATE"].Value = currentdate;

                        if (ccn.Equals("BRH"))
                            cmd.Parameters["PAU"].Value = "";
                        else
                            cmd.Parameters["PAU"].Value = datatable.Rows[rowcount][10].ToString();

                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    cmd.Connection.Close();
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw ex;
            }
            finally
            {
                cmd.Dispose();
            }
        }

        // Insert records into the AX_Stock_Status table in MS Access Database
        public void InsertStockStatusAX(string ccn, DataSet ds)
        {
            SqlTransaction transaction = null;
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = this.connection;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Insert_Stock_Status_AX";

            try
            {
                if (this.connection.State == System.Data.ConnectionState.Closed)
                    connection.Open();

                transaction = cmd.Connection.BeginTransaction();
                cmd.Transaction = transaction;

                cmd.Parameters.Add("CCN", SqlDbType.VarChar);
                cmd.Parameters.Add("StockRoom_ID", SqlDbType.VarChar);
                cmd.Parameters.Add("WareHouse", SqlDbType.VarChar);
                cmd.Parameters.Add("Item_number", SqlDbType.VarChar);
                cmd.Parameters.Add("Item_name", SqlDbType.VarChar);
                cmd.Parameters.Add("Commodity_code", SqlDbType.Char);
                cmd.Parameters.Add("Box_code", SqlDbType.Char);
                cmd.Parameters.Add("Available_physical", SqlDbType.Char);
                cmd.Parameters.Add("Physical_reserved", SqlDbType.Char);
                cmd.Parameters.Add("Standard_cost", SqlDbType.Float);
                cmd.Parameters.Add("Extd_cost", SqlDbType.Float);
                cmd.Parameters.Add("Last_Trans_Date", SqlDbType.SmallDateTime);
                cmd.Parameters.Add("SnapShot_Date", SqlDbType.SmallDateTime);

                DataTable datatable = ds.Tables[0];
                for (int rowcount = 0; rowcount < datatable.Rows.Count; rowcount++)
                {
                    if (datatable.Rows[rowcount][0].ToString().Length != 0)
                    {
                        cmd.Parameters["CCN"].Value = ccn;
                        cmd.Parameters["StockRoom_ID"].Value = datatable.Rows[rowcount][0].ToString();
                        cmd.Parameters["WareHouse"].Value = datatable.Rows[rowcount][1].ToString();
                        cmd.Parameters["Item_number"].Value = datatable.Rows[rowcount][2].ToString();
                        cmd.Parameters["Item_name"].Value = datatable.Rows[rowcount][3].ToString();
                        cmd.Parameters["Commodity_code"].Value = datatable.Rows[rowcount][4].ToString();
                        cmd.Parameters["Box_code"].Value = datatable.Rows[rowcount][6].ToString();
                        cmd.Parameters["Available_physical"].Value = Convert.ToInt32(datatable.Rows[rowcount][7]);
                        cmd.Parameters["Physical_reserved"].Value = Convert.ToInt32(datatable.Rows[rowcount][8]);
                        cmd.Parameters["Standard_cost"].Value = Convert.ToSingle(datatable.Rows[rowcount][9]);
                        cmd.Parameters["Extd_cost"].Value = Convert.ToSingle(datatable.Rows[rowcount][10]);
                        cmd.Parameters["Last_Trans_Date"].Value = Convert.ToDateTime(datatable.Rows[rowcount][11]);
                        cmd.Parameters["SnapShot_Date"].Value = DateTime.Now.Date;

                        //string text = string.Format("{0}, {1}, {2}, {4}, {5}, {6}, {7}, {8}, {9}",
                        //    cmd.Parameters["StockRoom"].Value,
                        //    cmd.Parameters["WareHouse"].Value,
                        //    cmd.Parameters["ItemNumber"].Value,
                        //    cmd.Parameters["ItemName"].Value,
                        //    cmd.Parameters["Commodity"].Value,
                        //    cmd.Parameters["BoxCode"].Value,
                        //    cmd.Parameters["StandardCost"].Value,
                        //    cmd.Parameters["AvailablePhysical"].Value,
                        //    cmd.Parameters["PhysicalReserved"].Value,
                        //    cmd.Parameters["SnapshotDate"].Value);

                        //FileUtility.SaveFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sql.txt"), text);

                        cmd.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
                cmd.Connection.Close();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw ex;
            }
            finally
            {
                cmd.Dispose();
            }
        }

        // Insert records into the Glovia_Stock_Status table in MS Access Database
        public void InsertStockStatusGlovia(DataSet ds)
        {
            SqlTransaction transaction = null;
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = this.connection;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Insert_Stock_Status_Glovia";

            try
            {
                if (this.connection.State == System.Data.ConnectionState.Closed)
                    connection.Open();

                transaction = cmd.Connection.BeginTransaction();
                cmd.Transaction = transaction;

                cmd.Parameters.Add("CCN", SqlDbType.VarChar);
                cmd.Parameters.Add("MAS_LOC", SqlDbType.VarChar);
                cmd.Parameters.Add("LOCATION", SqlDbType.VarChar);
                cmd.Parameters.Add("BIN", SqlDbType.VarChar);
                cmd.Parameters.Add("ITEM", SqlDbType.VarChar);
                cmd.Parameters.Add("DESCRIPTION", SqlDbType.VarChar);
                cmd.Parameters.Add("COMMODITY", SqlDbType.VarChar);
                cmd.Parameters.Add("ISSUE_CODE", SqlDbType.VarChar);
                cmd.Parameters.Add("BOX_CODE", SqlDbType.VarChar);
                cmd.Parameters.Add("OH_QTY", SqlDbType.Int);
                cmd.Parameters.Add("STD_COST", SqlDbType.Float);
                cmd.Parameters.Add("EXTD_COST", SqlDbType.Float);
                cmd.Parameters.Add("COA", SqlDbType.VarChar);
                cmd.Parameters.Add("CONSIGNED", SqlDbType.VarChar);
                cmd.Parameters.Add("BULK_EXPENSED", SqlDbType.VarChar);
                cmd.Parameters.Add("RETURN_ON_ASN", SqlDbType.VarChar);
                cmd.Parameters.Add("SNAPSHOT_DATE", SqlDbType.SmallDateTime);

                DataTable datatable = ds.Tables[0];
                for (int rowcount = 0; rowcount < datatable.Rows.Count; rowcount++)
                {
                    cmd.Parameters["CCN"].Value = datatable.Rows[rowcount][0].ToString();
                    cmd.Parameters["MAS_LOC"].Value = datatable.Rows[rowcount][1].ToString();
                    cmd.Parameters["LOCATION"].Value = datatable.Rows[rowcount][2].ToString();
                    cmd.Parameters["BIN"].Value = datatable.Rows[rowcount][3].ToString();
                    cmd.Parameters["ITEM"].Value = datatable.Rows[rowcount][4].ToString();
                    cmd.Parameters["DESCRIPTION"].Value = datatable.Rows[rowcount][5].ToString();
                    cmd.Parameters["COMMODITY"].Value = datatable.Rows[rowcount][6].ToString();
                    cmd.Parameters["ISSUE_CODE"].Value = datatable.Rows[rowcount][7].ToString();
                    cmd.Parameters["BOX_CODE"].Value = datatable.Rows[rowcount][8].ToString();
                    cmd.Parameters["OH_QTY"].Value = Convert.ToInt32(datatable.Rows[rowcount][9]);
                    cmd.Parameters["STD_COST"].Value = Convert.ToSingle(datatable.Rows[rowcount][10]);
                    cmd.Parameters["EXTD_COST"].Value = Convert.ToSingle(datatable.Rows[rowcount][11]);
                    cmd.Parameters["COA"].Value = datatable.Rows[rowcount][12].ToString();
                    cmd.Parameters["CONSIGNED"].Value = datatable.Rows[rowcount][13].ToString();
                    cmd.Parameters["BULK_EXPENSED"].Value = datatable.Rows[rowcount][14].ToString();
                    cmd.Parameters["RETURN_ON_ASN"].Value = datatable.Rows[rowcount][15].ToString();
                    cmd.Parameters["SNAPSHOT_DATE"].Value = Convert.ToDateTime(datatable.Rows[rowcount][16]);

                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
                cmd.Connection.Close();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw ex;
            }
            finally
            {
                cmd.Dispose();
            }
        }

        // Insert pulling report log into the Pulling_Report_Log table
        public void InsertLog(string reportname, string category, string starttime, string endtime, Int32 totalrecordnumber)
        {
            string currentdate = DateTime.Now.ToShortDateString();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = this.connection;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "Insert_Log";

            try
            {
                if (reportname.Length != 0)
                {
                    if (this.connection.State == System.Data.ConnectionState.Closed)
                        connection.Open();

                    //transaction = cmd.Connection.BeginTransaction();
                    //cmd.Transaction = transaction;

                    cmd.Parameters.Add("ReportName", SqlDbType.Char);
                    cmd.Parameters.Add("Category", SqlDbType.Char);
                    cmd.Parameters.Add("StartTime", SqlDbType.Char);
                    cmd.Parameters.Add("EndTime", SqlDbType.Char);
                    cmd.Parameters.Add("TotalRecordNumber", SqlDbType.Int);
                    cmd.Parameters.Add("CreatedDate", SqlDbType.Char);

                    cmd.Parameters["ReportName"].Value = reportname;
                    cmd.Parameters["Category"].Value = category;
                    cmd.Parameters["StartTime"].Value = starttime;
                    cmd.Parameters["EndTime"].Value = endtime;
                    cmd.Parameters["TotalRecordNumber"].Value = totalrecordnumber;
                    cmd.Parameters["CreatedDate"].Value = currentdate;

                    cmd.ExecuteNonQuery();

                    //transaction.Commit();
                    cmd.Connection.Close();
                }
            }
            catch (Exception ex)
            {
                //transaction.Rollback();
                throw ex;
            }
            finally
            {
                cmd.Dispose();
            }
        }
    }
}
