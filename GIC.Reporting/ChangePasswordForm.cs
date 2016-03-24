using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GIC.Common;
using GIC.Common.Model;

namespace GIC.Reporting
{
    public partial class ChangePasswordForm : Form
    {
        public ChangePasswordForm()
        {
            InitializeComponent();
        }

        private void changePasswordButton_Click(object sender, EventArgs e)
        {
            string useridstring = "User Id=";
            string passwordstring = "Password=";
            string ntaccount = NTAccountTextBox.Text.Trim();
            string password = passwordTextBox.Text.Trim();
            string confirmpassword = confirmPasswordTextBox.Text.Trim();

            if (NTAccountTextBox.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please enter your NT Account!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!password.Equals(confirmpassword))
            {
                MessageBox.Show("Your password doesn't match!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            List<AppSetting> connectionstringlist = new List<AppSetting>();
            connectionstringlist = ConfigFileUtility.GetKeyValueList();

            foreach (AppSetting connectionstring in connectionstringlist)
            {
                int startposition = 0;
                int endposition = 0;
                string existinguserid = string.Empty;
                string existingpassword = string.Empty;

                if (connectionstring.Key.ToUpper().Contains("GLOVIA") || connectionstring.Key.ToUpper().Contains("DRAGON"))
                {
                    if (connectionstring.KeyValue.ToUpper().Contains(useridstring.ToUpper()))
                    {
                        startposition = connectionstring.KeyValue.IndexOf(useridstring) + useridstring.Length;
                        endposition = connectionstring.KeyValue.IndexOf(passwordstring) - 1;
                        existinguserid = connectionstring.KeyValue.Substring(startposition, endposition - startposition);

                        if (connectionstring.KeyValue.ToUpper().Contains(passwordstring.ToUpper()))
                        {
                            startposition = connectionstring.KeyValue.IndexOf(passwordstring) + passwordstring.Length;
                            endposition = connectionstring.KeyValue.LastIndexOf(";");
                            existingpassword = connectionstring.KeyValue.Substring(startposition, endposition - startposition);

                            StringBuilder sb = new StringBuilder(connectionstring.KeyValue);
                            sb.Replace(existinguserid, ntaccount);
                            sb.Replace(existingpassword, PasswordUtility.DesEncrypt(password));

                            ConfigFileUtility.SetValue(connectionstring.Key, sb.ToString());
                        }
                    }
                }
            }            

            MessageBox.Show("Your password has been changed successful!", "Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            NTAccountTextBox.Text = "";
            passwordTextBox.Text = "";
            confirmPasswordTextBox.Text = "";
        }
    }
}
