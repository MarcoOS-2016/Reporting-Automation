using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GIC.Business;
using GIC.Common;
using GIC.Common.Model;
using GIC.DataAccess;
using Word = Microsoft.Office.Interop.Word;
using Microsoft.Office.Core;
using System.Reflection;

namespace GIC.Reporting
{
    public partial class ReportingConfigForm : Form
    {
        public ReportingConfigForm()
        {
            InitializeComponent();
        }

        #region -------------------- Common functions --------------------
        private bool IsTextBoxEmpty()
        {
            if (reportNameComboBox.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please fill Report Name", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (cCNTextBox.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please fill CCN", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (DayOfWeekTextBox.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please fill Day of Week", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (savedFolderTextBox.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please fill Saved Folder", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (sqlScriptRichTextBox.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please fill SQL Script", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
        
        private string SelectFolder()
        {
            FolderBrowserDialog folderbrowser = new FolderBrowserDialog();
            folderbrowser.RootFolder = Environment.SpecialFolder.MyComputer;
            folderbrowser.SelectedPath = @"C:\";
            folderbrowser.ShowNewFolderButton = true;

            if (folderbrowser.ShowDialog() == DialogResult.OK)
            {
                return folderbrowser.SelectedPath;
            }

            return String.Empty;
        }

        private void UpdateReportingConfigFile(ReportingConfig reportingconfig)
        {
            string xml = SerializationUtility.Serialize(reportingconfig);
            string configfilepath = Path.Combine(Environment.CurrentDirectory, "ReportingConfig.xml");
            string backupfilename = Path.Combine(Environment.CurrentDirectory, "ReportingConfig.bak");

            try
            {
                if (File.Exists(backupfilename)) 
                    File.Delete(backupfilename);

                File.Move(configfilepath, backupfilename);
                FileUtility.SaveFile(configfilepath, xml);           
                
                MessageBox.Show("Completed!", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw ex;
            }
        }
        #endregion

        #region -------------------- WindowsForm functions --------------------
        ReportingConfig reportingconfig = null;

        private void ReportingConfigForm_Load(object sender, EventArgs e)
        {            
            reportingconfig = FileUtility.LoadReportingConfig();

            List<string> reportnamelist = new List<string>();
            //reportnamelist.Add("");

            foreach (ScheduledReport scheduledreport in reportingconfig.ScheduledReports)
            {
                bool flag = false;
                for (int count = 1; count < reportnamelist.Count; count++)
                {
                    if (reportnamelist[count].Equals(scheduledreport.ReportName.ToUpper().Trim()))
                    {
                        flag = true;
                    }
                }

                if (!flag) reportnamelist.Add(scheduledreport.ReportName);
            }

            reportNameComboBox.DataSource = reportnamelist;
        }

        private void reportNameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedreportname = reportNameComboBox.Text;
            
            foreach (ScheduledReport scheduledreport in reportingconfig.ScheduledReports)
            {
                if (scheduledreport.ReportName.Equals(selectedreportname))
                {
                    cCNTextBox.Text = scheduledreport.CCN;
                    DayOfWeekTextBox.Text = scheduledreport.DayOfWeek;
                    savedFolderTextBox.Text = scheduledreport.SavedFolder;
                    sqlScriptRichTextBox.Text = scheduledreport.SQLScript;
                }
            }
        }

        private void saveFolderButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderbrowser = new FolderBrowserDialog();
            folderbrowser.RootFolder = Environment.SpecialFolder.MyComputer;
            folderbrowser.SelectedPath = @"C:\";
            folderbrowser.ShowNewFolderButton = true;

            if (folderbrowser.ShowDialog() == DialogResult.OK)
            {
                savedFolderTextBox.Text = folderbrowser.SelectedPath;
            }  
        }

        private void createButton_Click(object sender, EventArgs e)
        {
            //string reportname = reportNameComboBox.SelectedItem.ToString().Trim();
            string reportname = reportNameComboBox.Text.Trim();

            if (!IsTextBoxEmpty()) return;

            if (MessageBox.Show("Do you really want to create it?", "Reporting Configuration", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                foreach (ScheduledReport scheduledreport in this.reportingconfig.ScheduledReports)
                {
                    if (reportname.Equals(scheduledreport.ReportName))
                    {
                        MessageBox.Show("Cannot create the report configuration because the same report name is existed!", 
                            "Error", 
                            MessageBoxButtons.OK, 
                            MessageBoxIcon.Error);

                        return;
                    }
                }

                int len = this.reportingconfig.ScheduledReports.Length;
                ScheduledReport[] scheduledreports = new ScheduledReport[len + 1];

                int index = 0;
                for (index = 0; index < scheduledreports.Length - 1; index++)
                {
                    scheduledreports[index] = new ScheduledReport();
                    scheduledreports[index].ReportName = this.reportingconfig.ScheduledReports[index].ReportName;
                    scheduledreports[index].CCN = this.reportingconfig.ScheduledReports[index].CCN;
                    scheduledreports[index].DayOfWeek = this.reportingconfig.ScheduledReports[index].DayOfWeek;
                    scheduledreports[index].SavedFolder = this.reportingconfig.ScheduledReports[index].SavedFolder;
                    scheduledreports[index].SQLScript = this.reportingconfig.ScheduledReports[index].SQLScript;
                }
                                
                string ccn = cCNTextBox.Text.ToUpper().Trim();
                string dayofweek = DayOfWeekTextBox.Text.ToUpper().Trim();
                string savedfolder = savedFolderTextBox.Text.ToUpper().Trim();
                string sqlscript = sqlScriptRichTextBox.Text.ToUpper().Trim();

                scheduledreports[index] = new ScheduledReport();
                scheduledreports[index].ReportName = reportname;
                scheduledreports[index].CCN = ccn;
                scheduledreports[index].DayOfWeek = dayofweek;
                scheduledreports[index].SavedFolder = savedfolder;
                scheduledreports[index].SQLScript = sqlscript;

                this.reportingconfig.ScheduledReports = scheduledreports;

                UpdateReportingConfigFile(reportingconfig);
            }
        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            if (!IsTextBoxEmpty()) return;

            if (MessageBox.Show("Do you really want to update it?", "Reporting Configuration", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                string reportname = reportNameComboBox.SelectedItem.ToString().Trim();
                string ccn = cCNTextBox.Text.Trim();
                string dayofweek = DayOfWeekTextBox.Text.Trim();
                string savedfolder = savedFolderTextBox.Text.Trim();
                string sqlscript = sqlScriptRichTextBox.Text.Trim();

                for (int index = 0; index < this.reportingconfig.ScheduledReports.Length; index++)
                {
                    if (this.reportingconfig.ScheduledReports[index].ReportName.Equals(reportname))
                    {
                        this.reportingconfig.ScheduledReports[index].CCN = ccn;
                        this.reportingconfig.ScheduledReports[index].DayOfWeek = dayofweek;
                        this.reportingconfig.ScheduledReports[index].SQLScript = sqlscript;
                        this.reportingconfig.ScheduledReports[index].SavedFolder = savedfolder;
                    }
                }

                UpdateReportingConfigFile(reportingconfig);
            }
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            //string reportname = reportNameComboBox.SelectedItem.ToString().Trim();
            string reportname = reportNameComboBox.Text.ToString().Trim();
            if (reportname.Length == 0)
            {
                MessageBox.Show("Please select a Report Name", "Reporting Configuration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool flag = false;
            int count = 0;
            int len = this.reportingconfig.ScheduledReports.Length;
            ScheduledReport[] scheduledreports = new ScheduledReport[len - 1];

            if (MessageBox.Show("Do you really want to delete it?", "Reporting Configuration", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                for (int index = 0; index < this.reportingconfig.ScheduledReports.Length; index++)
                {
                    if (this.reportingconfig.ScheduledReports[index].ReportName.Equals(reportname))
                    {
                        flag = true;
                        continue;
                    }

                    scheduledreports[count] = new ScheduledReport();
                    scheduledreports[count].ReportName = this.reportingconfig.ScheduledReports[index].ReportName;
                    scheduledreports[count].CCN = this.reportingconfig.ScheduledReports[index].CCN;
                    scheduledreports[count].DayOfWeek = this.reportingconfig.ScheduledReports[index].DayOfWeek;
                    scheduledreports[count].SavedFolder = this.reportingconfig.ScheduledReports[index].SavedFolder;
                    scheduledreports[count].SQLScript = this.reportingconfig.ScheduledReports[index].SQLScript;

                    count++;
                }
            }

            if (!flag)
            {
                MessageBox.Show("Cannot find the report name in the existing Reporting Configuration!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.reportingconfig.ScheduledReports = scheduledreports;

            UpdateReportingConfigFile(reportingconfig);

            //reportNameComboBox.Items.Remove(reportname);
            reportNameComboBox.Text = "";
            cCNTextBox.Text = "";
            DayOfWeekTextBox.Text = "";
            savedFolderTextBox.Text = "";
            sqlScriptRichTextBox.Text = "";
        }

        private void changePasswordButton_Click(object sender, EventArgs e)
        {
            ChangePasswordForm forminstance = new ChangePasswordForm();
            forminstance.ShowDialog();
        }
        
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            ScheduledReportingHandler reportprocess = new ScheduledReportingHandler("C10000");
            reportprocess.Process();
        }
    }
}
