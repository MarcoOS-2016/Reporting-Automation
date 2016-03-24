using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GIC.Common
{
    public class ReportingConfig
    {
        private ScheduledReport[] scheduledreports;
        private AXReportConfig[] axreportconfigs;

        public ScheduledReport[] ScheduledReports
        {
            get { return scheduledreports; }
            set { scheduledreports = value; }
        }

        public AXReportConfig[] AXReportConfigs
        {
            get { return axreportconfigs; }
            set { axreportconfigs = value; }
        }

        public ScheduledReport this[string reportname]
        {
            get
            {
                foreach (ScheduledReport scheduledreport in scheduledreports)
                {
                    if (scheduledreport.ReportName.Equals(reportname.Trim().ToUpper()))
                        return scheduledreport;
                }

                return null;
            }
        }
    }

    public class ScheduledReport
    {
        private string reportname;
        private string ccn;
        private string tableaccess;
        private string databasename;
        private string tablename;
        private string importdatabaseflag;
        private string savedfolder;
        private string exporttofileflag;
        private string dayofweek;
        private string starttime;
        private string sqlscript;

        public string ReportName
        {
            get
            {
                return this.reportname;
            }
            set
            {
                if (value != null)
                {
                    this.reportname = value.Trim().ToUpper();
                }
            }
        }

        public string CCN
        {
            get
            {
                return this.ccn;
            }
            set
            {
                if (value != null)
                {
                    this.ccn = value.Trim().ToUpper();
                }
            }
        }

        public string TableAccess
        {
            get
            {
                return this.tableaccess;
            }
            set
            {
                if (value != null)
                    this.tableaccess = value.Trim().ToUpper();
            }
        }

        public string DataBaseName
        {
            get
            {
                return this.databasename;
            }
            set
            {
                if (value != null)
                    this.databasename = value.Trim().ToUpper();
            }
        }

        public string TableName
        {
            get
            {
                return this.tablename;
            }
            set
            {
                if (value != null)
                    this.tablename = value.Trim().ToUpper();
            }
        }

        public string ImportDataBaseFlag
        {
            get
            {
                return this.importdatabaseflag;
            }
            set
            {
                if (value != null)
                    this.importdatabaseflag = value.Trim().ToUpper();
            }
        }

        public string SavedFolder
        {
            get
            {
                return this.savedfolder;
            }
            set
            {
                if (value != null)
                {
                    this.savedfolder = value.Trim().ToUpper();
                }
            }
        }

        public string ExportToFileFlag
        {
            get
            {
                return this.exporttofileflag;
            }
            set
            {
                if (value != null)
                {
                    this.exporttofileflag = value.Trim().ToUpper();
                }
            }
        }

        public string DayOfWeek
        {
            get
            {
                return this.dayofweek;
            }
            set
            {
                if (value != null)
                {
                    this.dayofweek = value.Trim().ToUpper();
                }
            }
        }

        public string StartTime
        {
            get
            {
                return this.starttime;
            }
            set
            {
                if (value != null)
                {
                    this.starttime = value;
                }
            }
        }

        public string SQLScript
        {
            get
            {
                return this.sqlscript;
            }
            set
            {
                if (value != null)
                {
                    this.sqlscript = value.Trim().ToUpper();
                }
            }
        }
    }

    public class AXReportConfig
    {
        private string reportname;
        private string ccn;
        private string sourcefolder;
        private string targetfolder;
        private string archivefolder;
        private string tablename;
        private string dayofweek;
        private string starttime;

        public string ReportName
        {
            get
            {
                return this.reportname;
            }
            set
            {
                if (value != null)
                {
                    this.reportname = value.Trim().ToUpper();
                }
            }
        }

        public string CCN
        {
            get
            {
                return this.ccn;
            }
            set
            {
                if (value != null)
                {
                    this.ccn = value.Trim().ToUpper();
                }
            }
        }

        public string SourceFolder
        {
            get
            {
                return this.sourcefolder;
            }
            set
            {
                if (value != null)
                {
                    this.sourcefolder = value.Trim().ToUpper();
                }
            }
        }

        public string TargetFolder
        {
            get
            {
                return this.targetfolder;
            }
            set
            {
                if (value != null)
                {
                    this.targetfolder = value.Trim().ToUpper();
                }
            }
        }

        public string ArchiveFolder
        {
            get
            {
                return this.archivefolder;
            }
            set
            {
                if (value != null)
                {
                    this.archivefolder = value.Trim().ToUpper();
                }
            }
        }

        public string TableName
        {
            get
            {
                return this.tablename;
            }
            set
            {
                if (value != null)
                    this.tablename = value.Trim().ToUpper();
            }
        }

        public string DayOfWeek
        {
            get
            {
                return this.dayofweek;
            }
            set
            {
                if (value != null)
                {
                    this.dayofweek = value.Trim().ToUpper();
                }
            }
        }

        public string StartTime
        {
            get
            {
                return this.starttime;
            }
            set
            {
                if (value != null)
                {
                    this.starttime = value;
                }
            }
        }
    }
}
