
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;


namespace CowinSearchApp
{
    public partial class frmCowin : Form
    {
        public frmCowin()
        {
            InitializeComponent();
        }
        private void queryState()
        {
            try
            {
                cmbDistrict.Items.Clear();
                string strOutput = string.Empty;
                int iState = int.Parse( Program.strState.Substring(0, 2));
                string url = @"https://cdn-api.co-vin.in/api/v2/admin/location/districts/"+ iState.ToString();

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.GZip;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    strOutput = reader.ReadToEnd();
                }

                //txtResult.Text = strOutput;
                //// Note:Json convertor needs a json with one node as root
                strOutput = $"{{ \"rootNode\": {{{strOutput.Trim().TrimStart('{').TrimEnd('}')}}} }}";
                //// Now it is secure that we have always a Json with one node as root 
                var xd = JsonConvert.DeserializeXmlNode(strOutput);

                //// DataSet is able to read from XML and return a proper DataSet
                var dsData = new DataSet();
                dsData.ReadXml(new XmlNodeReader(xd), XmlReadMode.Auto);
                if (dsData.Tables.Count == 2)
                {
                    for (int i = 0; i < dsData.Tables[1].Rows.Count; i++)
                    {
                        cmbDistrict.Items.Add(dsData.Tables[1].Rows[i][0].ToString()+"-"+ dsData.Tables[1].Rows[i][1].ToString());
                    }
                    cmbDistrict.SelectedIndex = 0;
                }
                else
                {
                    txtLog.Text = "District information not available";
                    //     txtResult.Text = "No appointments available!!!\r\n" + txtResult.Text;
                }
                
            }
            catch (Exception ex)
            {
                txtLog.Text = ex.Message+"\r\n"+ txtLog.Text;
            }
        }
        private void btnQuery()
        {
            try
            {
                string strOutput = string.Empty;
                if (cmbDistrict.Items.Count == 0) return;
                string strDistrictCode = cmbDistrict.Text.Substring(0, cmbDistrict.Text.IndexOf("-"));
                string url = @"https://cdn-api.co-vin.in/api/v2/appointment/sessions/public/findByDistrict?district_id=" + strDistrictCode + "&date=" + dtQuery.ToString("dd-MM-yyyy");

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.GZip;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    strOutput = reader.ReadToEnd();
                }

                //txtResult.Text = strOutput;
                //// Note:Json convertor needs a json with one node as root
                strOutput = $"{{ \"rootNode\": {{{strOutput.Trim().TrimStart('{').TrimEnd('}')}}} }}";
                //// Now it is secure that we have always a Json with one node as root 
                var xd = JsonConvert.DeserializeXmlNode(strOutput);

                //// DataSet is able to read from XML and return a proper DataSet
                var dsData = new DataSet();
                dsData.ReadXml(new XmlNodeReader(xd), XmlReadMode.Auto);
                if (dsData.Tables.Count > 0)
                {
                    //dsData.WriteXml("D:\\RYBProject\\CowinApp\\CowinSearchAppV2\\Test.xml",XmlWriteMode.WriteSchema);
                    dgData.DataSource = dsData.Tables[0];
                    //if(dsData.Tables.Count==2)
                    //dgSlots.DataSource = dsData.Tables[1];
                    string strRowFilter = "";
                    strRowFilter = "(vaccine='" + (chkCovishield.Checked ? "COVISHIELD" : "") + "' or vaccine='" + (chkCovaccine.Checked ? "COVACCINE" : "") + "') AND ";
                    strRowFilter = strRowFilter + "( min_age_limit ='" + (chk45.Checked ? "45" : "") + "' or min_age_limit ='" + (chk40.Checked ? "40" : "") + "' or min_age_limit ='" + (chk18.Checked ? "18" : "") + "' )AND ";
                    strRowFilter = strRowFilter + "( available_capacity_dose1 > " + (chk1stDose.Checked ? "0" : "99999") + " or available_capacity_dose2 > " + (chk2ndDose.Checked ? "0" : "99999") + " )AND ";
                    strRowFilter = strRowFilter + " pincode like '" + txtPincode.Text + "*'";
                    (dgData.DataSource as DataTable).DefaultView.RowFilter = strRowFilter;
                    if (dgData.Rows.Count == 0)
                    {
                        txtLog.Text = "No appointments available!!!\r\n" + txtLog.Text;
                    }
                    else
                    {
                        if (chkStopWhenFound.Checked) btnStartStop_Click(null, null);
                        DataTable dtFull = (DataTable)dgData.DataSource;
                        DataTable dtFiletered = dtFull.DefaultView.ToTable();

                        foreach (DataRow dr in dtFiletered.Rows)
                        {
                            try
                            {
                                dsFullData.sessions.Rows.Add(dr.ItemArray);
                            }
                            catch (Exception ex)
                            { }
                        }

                        for (int i = 0; i < dtFiletered.Rows.Count; i++)
                        {
                            
                            //txtResult.Text = dtQuery.ToString("dd/MMM/yyyy") + " - Appointments available in :" + dsData.Tables[0].Rows[i][2].ToString() + "/" + dsData.Tables[0].Rows[i][5].ToString() + " - Pincode:" + dsData.Tables[0].Rows[i][6].ToString() + "\r\n" + txtResult.Text;
                            txtLog.Text = dtQuery.ToString("dd/MMM/yyyy") + " - Appointments available in :" + dtFiletered.Rows[i][2].ToString() + "/" + dtFiletered.Rows[i][5].ToString() + " - Pincode:" + dtFiletered.Rows[i][6].ToString() + "\r\n" + txtLog.Text;
                        }
                        //ntfyBaloon.Text = "****Appointments available in :" + dsData.Tables[0].Rows[0][2].ToString() + "/" + dsData.Tables[0].Rows[0][5].ToString() + " - Pincode:" + dsData.Tables[0].Rows[0][6].ToString() + " - " + dtQuery.ToString("dd/MMM/yyyy") + "****";
                        ntfyBaloon.Visible = true;
                        //smsAppointment("Appointments available in :"+ dsData.Tables[0].Rows[0][2].ToString() +"/"+ dsData.Tables[0].Rows[0][5].ToString() + " - Pincode:"+dsData.Tables[0].Rows[0][6].ToString() + " - " + dtQuery.ToString("dd/MMM/yyyy"));

                    }
                }
                else
                {
                    //     txtResult.Text = "No appointments available!!!\r\n" + txtResult.Text;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                txtLog.Text = ex.Message+ "\r\n"+txtLog.Text;
            }
        }
        private bool bTimer = false;
        private void btnStartStop_Click(object sender, EventArgs e)
        {
            
            if (bTimer)
            {
                tmrMain.Enabled = false;
                bTimer = false;
                btnStartStop.Text = "Start";
            }
            else
            {
                dtQuery = dtpFromDate.Value;
                tmrMain.Enabled = true;
                bTimer = true;
                btnStartStop.Text = "Stop";
            }
        }

        private void tmrMain_Tick(object sender, EventArgs e)
        {
            //if (txtResult.Text.Length > 3000) txtResult.Text = "";
            if (txtLog.Text.Length > 3000) txtLog.Text = "";
            lblSearchStatus.Text = "Query for " + dtQuery.ToString("dd/MMM/yyyy");
            txtLog.Text = "Query for " + dtQuery.ToString("dd/MMM/yyyy")+"\r\n" + txtLog.Text;
            btnQuery();
            dtQuery = dtQuery.AddDays(1);
            if (dtQuery >= dtpToDate.Value)
            {
                dtQuery = dtpFromDate.Value;
            }
        }
        DateTime dtQuery = DateTime.Today;
        private void frmCowin_Load(object sender, EventArgs e)
        {
            dtpFromDate.Value = DateTime.Today;
            dtpToDate.Value = DateTime.Today.AddDays(21);
            dtQuery = dtpFromDate.Value;
            cmbDistrict.Items.Clear();
            cmbDistrict.DropDownStyle = ComboBoxStyle.DropDownList;
            queryState();
            
            
        }

        private void btnGet_Click(object sender, EventArgs e)
        {
            btnQuery();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            

        }

        private void btnDistrictRefresh_Click(object sender, EventArgs e)
        {
            queryState();
        }

        private void btnBookAppointment_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Direct appointment booking feature is disabled for Public use.\r\nIt can be enabled Only for Hospitals or authorized Government local bodies as per Central government regulations!!!", "Restricted", MessageBoxButtons.OK, MessageBoxIcon.Information);
            txtLog.Text = "Direct appointment booking feature is disabled for Public use.\r\nIt can be enabled Only for Hospitals or authorized Government local bodies as per Central government regulations!!!\r\n" + txtLog.Text;
            /*Removed code for booking to comply with Government policy
             "Co-WIN Protected APIs allow any third-party application to access certain restricted information, in order to access, the service providers must agree to the extant “Terms of Service”. All third-party applications that wish to integrate with Co-WIN using Protected APIs should undertake a prescribed integration and testing process with the Co-WIN APIs. For this purpose, the authenticated ASP system administrators will be provided with access to a sandbox environment with staging-level API keys to test integration with the Co-WIN APIs, that the ASP wishes to use in their operations. Only after a successful testing cycle is completed and demonstrated to a competent authority, the ASP will be provided with production-level API keys for their Application. "
             https://apisetu.gov.in/public/api/cowin/cowin-protected-v2
             */
        }
    }
}
