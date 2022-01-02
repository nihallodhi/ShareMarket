using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Tulpep.NotificationWindow;

namespace ShareMarket_v2
{
    public partial class Form3 : Form
    {      
        string[] columnNames = { "Share", "Market Price", "Change", "Target", "Day High", "Day Low", "52 Week High", "52 Week Low" };
        public string[] companies = new string[100];
        public string[] urls = new string[100];
        string marketPrice = string.Empty;

        int index = 0;
        int check = 0;
        int popup = 1;
        int indexURL = 0;

        bool firstTime = true;

        List<WebBrowser> webBrowsers = new List<WebBrowser>();
        WebBrowser webBrowserNifty;

        Form1 form1;
        Form2 form2;

        System.Timers.Timer timer;
        //System.Timers.Timer niftyTimer;

        public Form3(Form1 frm, Form2 form)
        {
            form1 = frm;
            form2 = form;
            InitializeComponent();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            FetchNiftyData();
            FetchData();

            this.AutoSize = true;

            pictureBox1.Image = Properties.Resources.nifty;
            pictureBox1.Size = new Size(122, 70);

            //pictureBox2.Size = new Size(100, 50);

            pictureBox3.Image = Properties.Resources.refresh;
          
            button3.Location = new Point(dataGridView1.Size.Width - 300, dataGridView1.Size.Height + 100);
            button1.Location = new Point(dataGridView1.Size.Width - 200, dataGridView1.Size.Height + 100);
            label1.Location = new Point(dataGridView1.Size.Width - 200, dataGridView1.Size.Height + 150);
            
        }     
          
        private void FetchData()
        {
            DataTable dataTable = new DataTable();

            foreach (string column in columnNames)
            {
                dataTable.Columns.Add(column);
            }

            using (StreamReader file = new StreamReader("C:\\nl\\nl1.txt"))
            {
                string newLine;

                while ((newLine = file.ReadLine()) != null && newLine.Length > 0)
                {
                    string[] line = newLine.Split(',');

                    DataRow dataRow = dataTable.NewRow();
                    dataRow[0] = line[0];
                    dataRow[3] = line[1];
                    dataTable.Rows.Add(dataRow);

                    LoadData(line[0]);
                    ////

                    //

                    //int j = 0;
                    //for (int i = 0; i < line.Length; i++)
                    //{
                    //    //if (j == 1 || j == 2 || j == 6 || j == 7)
                    //    //{
                    //    //    j++;
                    //    //}

                    //    //if (j == 7 || j == 2)
                    //    //{
                    //    //    j++;
                    //    //}

                    //}


                }

            }
            dataGridView1.DataSource = dataTable;
            //dataGridView1.AutoSize = true;
            dataGridView1.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
        }

        private void LoadData(string company)
        {
            string url = form2.FetchURL(company);

            if (Array.IndexOf(companies, company) < 0)
            {
                companies[index] = company;
            }

            if (Array.IndexOf(urls, url) < 0)
            {
                urls[index++] = url;
            }

            LoadWebBrowsers(url);
        }

        private void DocumentCompleted(object Sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (check < index)  //increment check till it reaches the last index
                check++;

            HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();

            indexURL = Array.IndexOf(urls, e.Url.ToString());

            htmlDocument.LoadHtml(webBrowsers.ElementAt(indexURL).DocumentText);

            marketPrice = FetchMarketPrice(htmlDocument);

            SetMarketPriceColumn();
            SetChangeColumn(htmlDocument);
            InitiatePopup();
            //SetMarketInvestmentProfit();
            SetHighLow(htmlDocument);

            if (indexURL == 0)
            {                
                //CheckMarketOpen();
            }

            if (check == index)
            {
                StartTimer();  //start the timer once everything has loaded in the datagridview
            }

        }

        private void SetMarketPriceColumn()
        {
            if (dataGridView1.Rows[indexURL].Cells[1].Value.ToString().Length > 0 && Convert.ToDouble(marketPrice) > Convert.ToDouble(dataGridView1.Rows[indexURL].Cells[1].Value))
            {
                dataGridView1.Rows[indexURL].Cells[1].Style = new DataGridViewCellStyle { Font = new Font(this.Font, FontStyle.Bold), BackColor = Color.FromArgb(0, 180, 0) };
            }
            else if (dataGridView1.Rows[indexURL].Cells[1].Value.ToString().Length > 0 && Convert.ToDouble(marketPrice) < Convert.ToDouble(dataGridView1.Rows[indexURL].Cells[1].Value))
            {
                dataGridView1.Rows[indexURL].Cells[1].Style = new DataGridViewCellStyle { Font = new Font(this.Font, FontStyle.Bold), BackColor = Color.FromArgb(255, 0, 30) };
            }
            else if ((dataGridView1.Rows[indexURL].Cells[1].Value.ToString().Length > 0) && (Convert.ToDouble(marketPrice) == Convert.ToDouble(dataGridView1.Rows[indexURL].Cells[1].Value)))
            // && (!dataGridView1.Rows[indexURL].Cells[1].Style.BackColor.Name.Equals("Red") && !dataGridView1.Rows[indexURL].Cells[1].Style.BackColor.Name.Equals("Green")))
            {
                dataGridView1.Rows[indexURL].Cells[1].Style = new DataGridViewCellStyle { Font = new Font(this.Font, FontStyle.Bold), BackColor = Color.LightGray };
            }
            else if (dataGridView1.Rows[indexURL].Cells[1].Style.BackColor.Name.Equals("Red") || dataGridView1.Rows[indexURL].Cells[1].Style.BackColor.Name.Equals("Green"))
            {
                dataGridView1.Rows[indexURL].Cells[1].Style = new DataGridViewCellStyle { Font = new Font(this.Font, FontStyle.Bold) };
            }

            dataGridView1.Rows[indexURL].Cells[1].Value = marketPrice;// + " (" + change + ")";
                                                                        //dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
        }

        private void SetChangeColumn(HtmlAgilityPack.HtmlDocument htmlDocument)
        {
            string tempChange = htmlDocument.DocumentNode.Descendants("span").Where(node => node.GetAttributeValue("id", "").Equals("change")).ElementAt(0).InnerText;
            double change = tempChange.Equals("-") ? 0 : Convert.ToDouble(tempChange);

            if (change > 0)
            {
                dataGridView1.Rows[indexURL].Cells[2].Style = new DataGridViewCellStyle { Font = new Font(this.Font, FontStyle.Bold), ForeColor = Color.Green };
                dataGridView1.Rows[indexURL].Cells[2].Value = change.ToString() + " ▲";
            }
            else if (change < 0)
            {
                dataGridView1.Rows[indexURL].Cells[2].Style = new DataGridViewCellStyle { Font = new Font(this.Font, FontStyle.Bold), ForeColor = Color.Red };
                dataGridView1.Rows[indexURL].Cells[2].Value = change.ToString() + " ▼";
            }
            else
            {
                dataGridView1.Rows[indexURL].Cells[2].Style = new DataGridViewCellStyle { Font = new Font(this.Font, FontStyle.Bold), ForeColor = Color.DarkGray };
                dataGridView1.Rows[indexURL].Cells[2].Value = change.ToString() + " -";
            }
        }
        private void InitiatePopup()
        {
            if (Convert.ToDouble(marketPrice) <= Convert.ToDouble(dataGridView1.Rows[indexURL].Cells[3].Value) && popup == 1)
            {
                ShowPopup(dataGridView1.Rows[indexURL].Cells[0].Value.ToString(), dataGridView1.Rows[indexURL].Cells[1].Value.ToString(), "Buy");
            }            
        }

        private void SetHighLow(HtmlAgilityPack.HtmlDocument htmlDocument)
        {
            string weekHigh = htmlDocument.DocumentNode.Descendants("span").Where(node => node.GetAttributeValue("id", "").Equals("high52")).ElementAt(0).InnerText;
            weekHigh += " " + htmlDocument.DocumentNode.Descendants("span").Where(node => node.GetAttributeValue("id", "").Equals("mock_cm_adj_high_dt")).ElementAt(0).InnerText;

            string dayHigh = htmlDocument.DocumentNode.Descendants("div").Where(node => node.GetAttributeValue("id", "").Equals("dayHigh")).ElementAt(0).InnerText;

            dataGridView1.Rows[indexURL].Cells[4].Value = dayHigh;
            dataGridView1.Rows[indexURL].Cells[4].Style = new DataGridViewCellStyle { ForeColor = Color.Green };

            dataGridView1.Rows[indexURL].Cells[6].Value = weekHigh;
            dataGridView1.Rows[indexURL].Cells[6].Style = new DataGridViewCellStyle { ForeColor = Color.Green };
            dataGridView1.Columns[6].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;

            string weekLow = htmlDocument.DocumentNode.Descendants("span").Where(node => node.GetAttributeValue("id", "").Equals("low52")).ElementAt(0).InnerText;
            weekLow += " " + htmlDocument.DocumentNode.Descendants("span").Where(node => node.GetAttributeValue("id", "").Equals("mock_cm_adj_low_dt")).ElementAt(0).InnerText;

            string dayLow = htmlDocument.DocumentNode.Descendants("div").Where(node => node.GetAttributeValue("id", "").Equals("dayLow")).ElementAt(0).InnerText;

            dataGridView1.Rows[indexURL].Cells[5].Value = dayLow;
            dataGridView1.Rows[indexURL].Cells[5].Style = new DataGridViewCellStyle { ForeColor = Color.Red };

            dataGridView1.Rows[indexURL].Cells[7].Value = weekLow;
            dataGridView1.Rows[indexURL].Cells[7].Style = new DataGridViewCellStyle { ForeColor = Color.Red };
            dataGridView1.Columns[7].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
        }

        private void LoadWebBrowsers(string url)
        {
            WebBrowser webBrowser = new WebBrowser();
            webBrowser.ScriptErrorsSuppressed = true;
            webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(DocumentCompleted);
            webBrowser.Navigate(url);
            webBrowsers.Add(webBrowser);
        }

        private void TimerDone(Object source, ElapsedEventArgs e)
        {
            timer.Stop();

            for (int i = 0; i < index; i++)
            {
                dataGridView1.Rows[i].Cells[1].Style = new DataGridViewCellStyle { Font = new Font(this.Font, FontStyle.Bold) };
                webBrowsers.ElementAt(i).Navigate(urls[i]);
            }

            //if (firstTime)
            //{
            //    firstTime = false;
            //    AdjustDataGrid();
            //}
        }

        private void StartTimer()
        {
            check = 0;
            timer = new System.Timers.Timer(15000);
            timer.Elapsed += TimerDone;
            timer.Enabled = true;
        }

        private void ShowPopup(string company, string marketPrice, string target)
        {
            PopupNotifier popup = new PopupNotifier();
            popup.Image = Properties.Resources.money;
            popup.TitleText = "Target Reached!";
            popup.ContentText = "Target reached for " + company + Environment.NewLine + "Current Market Price: " + marketPrice + Environment.NewLine + "Target: " + target;
            popup.Popup();
        }

        private void Label1_Click(object sender, EventArgs e)
        {
            if (popup == 1)
                popup = 0;
            else
                popup = 1;
        }

        private void Button1_Click(object sender, EventArgs e)
        {            
            form1.Activated += new EventHandler(form1.CheckButtons);
            form1.showButton = true;
            form1.fromWatchlist = true;
            form1.Show();
        }

        public void LoadDataAgain(object sender, EventArgs e)
        {
            Activated -= new EventHandler(LoadDataAgain);

            if (!firstTime)
            {
                FetchData();
            }
            else
            {
                firstTime = false;
            }
        }

        private void FetchNiftyData()
        {
            label6.Text = form2.niftyStatus;
            label2.Text = Convert.ToDouble(form2.niftyChange) > 0 ? " ▲ " + form2.niftyChange : "▼ " + form2.niftyChange;

            if (label2.Text.Contains("▲"))
            {
                label2.ForeColor = Color.Green;
            }
            else
            {
                label2.ForeColor = Color.Red;
            }
        }      

        private void RefreshNiftyData(object sender, EventArgs e)
        {
            FetchNiftyData();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            form1.Activated += new EventHandler(form1.CheckButtons);
            form1.showButton = false;
            form1.fromWatchlist = true;
            form1.Show();
        }

        public string FetchMarketPrice(HtmlAgilityPack.HtmlDocument htmlDocument)
        {
            string marketPrice = string.Empty;

            marketPrice = htmlDocument.DocumentNode.Descendants("span").Where(node => node.GetAttributeValue("id", "").Equals("lastPrice")).ElementAt(0).InnerText;

            return marketPrice;
        }

    }
}
