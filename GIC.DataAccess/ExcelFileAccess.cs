﻿using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Data.SqlClient;
using log4net;

namespace GIC.DataAccess
{
    public class ExcelFileAccess : IDisposable
    {
        private static ILog log = LogManager.GetLogger(typeof(ExcelFileAccess));
        
        protected OleDbConnection connection = null;
        protected string fileName = String.Empty;
        protected bool isfield;

        public string FileName
        {
            get { return this.fileName; }
        }

        public ExcelFileAccess()
        {
        }

        public ExcelFileAccess(string filename)
        {
            this.fileName = filename;

            connection = new OleDbConnection();
            this.connection.ConnectionString =
                //String.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties='Excel 8.0;IMEX=1';", this.fileName);
                String.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0;HDR=YES;IMEX=1;READONLY=TRUE\"", this.fileName);

            try
            {
                connection.Open();
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                throw e;
            }
        }

        public ExcelFileAccess(string filename, bool isfield)
        {
            this.fileName = filename;
            this.isfield = isfield;

            connection = new OleDbConnection();

            if (this.isfield)
            {
                this.connection.ConnectionString =
                    String.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0;HDR=YES;IMEX=1;READONLY=TRUE\"", fileName);
            }
            else
            {
                this.connection.ConnectionString =
                    String.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0;HDR=NO;IMEX=1;READONLY=TRUE\"", fileName);
            }

            try
            {
                connection.Open();
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                throw e;
            }
        }

        public DataSet ExecuteQuery(string sql)
        {
            DataSet ds = null;
            OleDbDataAdapter adapter = null;

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                adapter = new OleDbDataAdapter(sql, connection);
                ds = new DataSet();
                adapter.Fill(ds);
            
            }
            catch (Exception e)
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    connection.Dispose();
                }

                log.Error(string.Format(sql, e.Message));
                throw e;
            }

            finally
            {
                adapter.Dispose();
            }

            return ds;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free managed objects.
            }
            // Free unmanaged objects.
            if (this.connection != null)
            {
                this.connection.Close();
                this.connection.Dispose();
                this.connection = null;
            }

            // Set large fields to null.
        }

        ~ExcelFileAccess()
        {
            if (this.connection != null && connection.State == ConnectionState.Open)
            {
                log.Error("Connection is supposed to be closed by clients instead of waiting for GC.");
            }

            Dispose(false);
        }    
    }
}
