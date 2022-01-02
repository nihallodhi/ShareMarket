using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows.Forms;
using Tulpep.NotificationWindow;

namespace ShareMarket_v2
{
    public partial class Form2 : Form
    {
        string[] columnNames = { "Share", "Market Price", "Change", "Buying Price", "Number of Shares", "Invested Amount", "Current Market Investment", "Profit", "Target", "Buy/Sell", "Day High", "Day Low", "52 Week High", "52 Week Low" , "Purchased On"};
        public string[] companies = new string[100];
        public List<string> companiesTest = new List<string>();
        public string[] urls = new string[100];
        string marketPrice = string.Empty;
        public string niftyStatus;
        public string niftyChange;

        int index = 0;
        int check = 0;
        int popup = 1;
        int indexURL = 0;

        bool firstTime = true;
        public bool marketOpen = true;

        double totalEvaluation = 0d;

        public List<WebBrowser> webBrowsers = new List<WebBrowser>();
        WebBrowser webBrowserNifty;

        Form1 form1;
        public Form3 form3;

        System.Timers.Timer timer;
        //System.Timers.Timer niftyTimer;

        public Form2(Form1 form)
        {
            form1 = form;
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            FetchNiftyData();
            FetchData();

            this.AutoSize = true;

            pictureBox1.Image = Properties.Resources.nifty;
            pictureBox1.Size = new Size(122, 70);

            pictureBox2.Size = new Size(100, 50);

            pictureBox3.Image = Properties.Resources.refresh;            

            //if (ShareMarketOpen())
            //{
            //    pictureBox2.Image = Properties.Resources.open;
            //}
            //else
            //{
            //    pictureBox2.Image = Properties.Resources.close;
            //}
            
            button3.Location = new Point(dataGridView1.Size.Width - 400, dataGridView1.Size.Height + 100);
            button1.Location = new Point(dataGridView1.Size.Width - 300, dataGridView1.Size.Height + 100);
            label1.Location = new Point(dataGridView1.Size.Width - 200, dataGridView1.Size.Height + 150);
            button2.Location = new Point(dataGridView1.Size.Width - 100, dataGridView1.Size.Height + 100);
            button4.Location = new Point(dataGridView1.Size.Width - 200, dataGridView1.Size.Height + 100);
        }

        private void FetchData()
        {
            double totalInvestment = 0d;
            DataTable dataTable = new DataTable();

            foreach (string column in columnNames)
            {
                dataTable.Columns.Add(column);
            }

            using (StreamReader file = new StreamReader("C:\\nl\\nl.txt"))
            {
                string newLine;

                while ((newLine = file.ReadLine()) != null && newLine.Length > 0)
                {
                    DataRow dataRow = dataTable.NewRow();
                    string[] line = newLine.Split(',');

                    LoadData(line[0]);
                    totalInvestment += Convert.ToDouble(line[3]);

                    int j = 0;
                    for (int i = 0; i < line.Length; i++)
                    {
                        if (j == 1 || j == 2 || j == 6 || j == 7)
                        {
                            j++;
                        }

                        if (j == 7 || j == 2)
                        {
                            j++;
                        }
                        if (i==1||i==3||i==4)
                        {
                            line[i] = string.Format("{0:#,0.00}", Convert.ToDouble(line[i]));
                        }
                        dataRow[j++] = line[i];
                    }
                    dataRow[14] = line[6];
                    dataTable.Rows.Add(dataRow);
                }

            }
            dataGridView1.DataSource = dataTable;
            //dataGridView1.AutoSize = true;
            dataGridView1.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;            
            label7.Text = string.Format("{0:#,0.00}", totalInvestment);
        }

        private void LoadData(string company)
        {
            string url = FetchURL(company);           

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
            SetMarketInvestmentProfit();
            SetHighLow(htmlDocument);
            
            if(indexURL == 0)
            {
            //    firstTime = false;
                CheckMarketOpen();                
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
            if (Convert.ToDouble(marketPrice) >= Convert.ToDouble(dataGridView1.Rows[indexURL].Cells[8].Value) && popup == 1 && dataGridView1.Rows[indexURL].Cells[9].Value.ToString().Equals("Sell"))
            {
                ShowPopup(dataGridView1.Rows[indexURL].Cells[0].Value.ToString(), dataGridView1.Rows[indexURL].Cells[1].Value.ToString(), dataGridView1.Rows[indexURL].Cells[8].Value.ToString());
            }
            else if (Convert.ToDouble(marketPrice) <= Convert.ToDouble(dataGridView1.Rows[indexURL].Cells[8].Value) && popup == 1 && dataGridView1.Rows[indexURL].Cells[9].Value.ToString().Equals("Buy"))
            {
                ShowPopup(dataGridView1.Rows[indexURL].Cells[0].Value.ToString(), dataGridView1.Rows[indexURL].Cells[1].Value.ToString(), dataGridView1.Rows[indexURL].Cells[8].Value.ToString());
            }
        }

        private void SetHighLow(HtmlAgilityPack.HtmlDocument htmlDocument)
        {
            string weekHigh = htmlDocument.DocumentNode.Descendants("span").Where(node => node.GetAttributeValue("id", "").Equals("high52")).ElementAt(0).InnerText;
            weekHigh += " " + htmlDocument.DocumentNode.Descendants("span").Where(node => node.GetAttributeValue("id", "").Equals("mock_cm_adj_high_dt")).ElementAt(0).InnerText;

            string dayHigh = htmlDocument.DocumentNode.Descendants("div").Where(node => node.GetAttributeValue("id", "").Equals("dayHigh")).ElementAt(0).InnerText;

            dataGridView1.Rows[indexURL].Cells[10].Value = dayHigh;
            dataGridView1.Rows[indexURL].Cells[10].Style = new DataGridViewCellStyle { ForeColor = Color.Green };

            dataGridView1.Rows[indexURL].Cells[12].Value = weekHigh;
            dataGridView1.Rows[indexURL].Cells[12].Style = new DataGridViewCellStyle { ForeColor = Color.Green };
            dataGridView1.Columns[12].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;

            string weekLow = htmlDocument.DocumentNode.Descendants("span").Where(node => node.GetAttributeValue("id", "").Equals("low52")).ElementAt(0).InnerText;
            weekLow += " " + htmlDocument.DocumentNode.Descendants("span").Where(node => node.GetAttributeValue("id", "").Equals("mock_cm_adj_low_dt")).ElementAt(0).InnerText;

            string dayLow = htmlDocument.DocumentNode.Descendants("div").Where(node => node.GetAttributeValue("id", "").Equals("dayLow")).ElementAt(0).InnerText;

            dataGridView1.Rows[indexURL].Cells[11].Value = dayLow;
            dataGridView1.Rows[indexURL].Cells[11].Style = new DataGridViewCellStyle { ForeColor = Color.Red };

            dataGridView1.Rows[indexURL].Cells[13].Value = weekLow;
            dataGridView1.Rows[indexURL].Cells[13].Style = new DataGridViewCellStyle { ForeColor = Color.Red };
            dataGridView1.Columns[13].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
        }

        private void SetMarketInvestmentProfit()
        {
            double marketInvestment = Convert.ToDouble(marketPrice) * Convert.ToInt32(dataGridView1.Rows[indexURL].Cells[4].Value);
            dataGridView1.Rows[indexURL].Cells[6].Value = string.Format("{0:#,0.00}", Math.Round(marketInvestment, 2));
                //Convert.ToString(Math.Round(marketInvestment, 2));
            totalEvaluation += marketInvestment;

            dataGridView1.Rows[indexURL].Cells[7].Value = string.Format("{0:#,0.00}", Math.Round(marketInvestment - Convert.ToDouble(dataGridView1.Rows[indexURL].Cells[5].Value), 2));
                //Convert.ToString(Math.Round(marketInvestment - Convert.ToDouble(dataGridView1.Rows[indexURL].Cells[5].Value), 2));

            if (dataGridView1.Rows[indexURL].Cells[7].Value.ToString().Length > 0 && Convert.ToDouble(dataGridView1.Rows[indexURL].Cells[7].Value) > 0)
            {
                dataGridView1.Rows[indexURL].Cells[7].Style = new DataGridViewCellStyle { Font = new Font(this.Font, FontStyle.Bold), ForeColor = Color.Green };
            }
            else if (dataGridView1.Rows[indexURL].Cells[7].Value.ToString().Length > 0 && Convert.ToDouble(dataGridView1.Rows[indexURL].Cells[7].Value) < 0)
            {
                dataGridView1.Rows[indexURL].Cells[7].Style = new DataGridViewCellStyle { Font = new Font(this.Font, FontStyle.Bold), ForeColor = Color.Red };
            }
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

            if (marketOpen)
            {
                for (int i = 0; i < index; i++)
                {
                    dataGridView1.Rows[i].Cells[1].Style = new DataGridViewCellStyle { Font = new Font(this.Font, FontStyle.Bold) };
                    webBrowsers.ElementAt(i).Navigate(urls[i]);
                }
            }

            //if (firstTime)
            //{
            //    firstTime = false;
            //    AdjustDataGrid();
            //}
        }

        private void StartTimer()
        {
            //label8.Text = Convert.ToString(totalEvaluation);
            label8.Text = string.Format("{0:#,0.00}", totalEvaluation);
            
            label8.ForeColor = totalEvaluation >= Convert.ToDouble(label7.Text)? Color.Green : Color.Red;

            label10.Text = string.Format("{0:#,0.00}", totalEvaluation - Convert.ToDouble(label7.Text));
                //(totalEvaluation - Convert.ToDouble(label7.Text)).ToString();
            label10.ForeColor = Convert.ToDouble(label10.Text) > 0 ? Color.Green : Color.Red;
            totalEvaluation = 0d;

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
            //this.Hide();
            //urls = null;
            //timer.Stop();
            //timer.Enabled = false;
            //this.Close();
            form1.Activated += new EventHandler(form1.CheckButtons);
            form1.showButton = true;
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

        private bool ShareMarketOpen()
        {
            //string time = DateTime.Now().
            return true; //to be implemented
        }

        private void FetchNiftyData()
        {
            webBrowserNifty = new WebBrowser();
            webBrowserNifty.ScriptErrorsSuppressed = true;
            webBrowserNifty.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(NiftyDocumentCompleted);
            webBrowserNifty.Navigate("https://money.rediff.com/indices/nse/nifty-50");            
        }

        private void NiftyDocumentCompleted(object Sender, WebBrowserDocumentCompletedEventArgs e)
        {
            HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
            htmlDocument.LoadHtml(webBrowserNifty.DocumentText);
            niftyStatus = htmlDocument.DocumentNode.Descendants("span").Where(node => node.GetAttributeValue("id", "").Equals("ltpid")).ElementAt(0).InnerText;

            niftyChange = htmlDocument.DocumentNode.Descendants("span").Where(node => node.GetAttributeValue("id", "").Equals("iconarrow")).ElementAt(0).Descendants("span").ElementAt(0).InnerText;
            //var x = niftyChange.Descendants("span").ElementAt(0).InnerText;
            label6.Text = string.Format("{0:#,0.00}", Convert.ToDouble(niftyStatus));
            label2.Text = Convert.ToDouble(niftyChange) > 0 ? " ▲ " + string.Format("{0:#,0.00}", Convert.ToDouble(niftyChange)) : "▼ " + string.Format("{0:#,0.00}", Convert.ToDouble(niftyChange));

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
            form1.Show();
        }

        private void CheckMarketOpen()
        {
            string timeNow = DateTime.Now.ToString("h:mm");
            string amPm = DateTime.Now.ToString("tt");
            string[] time = timeNow.Split(':');

            if (Convert.ToInt32(time[0]) >= 9 && amPm.Equals("AM"))
            {
                if (Convert.ToInt32(time[0]) == 9 && Convert.ToInt32(time[1]) >= 15 && amPm.Equals("AM"))
                {
                    if (pictureBox2.Image != Properties.Resources.open)
                    {
                        pictureBox2.Image = Properties.Resources.open;
                    }
                    else
                    {
                        pictureBox2.Image = Properties.Resources.close;
                        marketOpen = false;
                    }
                }
                else if (pictureBox2.Image != Properties.Resources.open)
                {
                    pictureBox2.Image = Properties.Resources.open;
                }

            }
            else if (Convert.ToInt32(time[0]) <= 3 && amPm.Equals("PM"))
            {
                if (Convert.ToInt32(time[0]) == 3 && Convert.ToInt32(time[1]) <= 30 && amPm.Equals("PM"))
                {
                    if (pictureBox2.Image != Properties.Resources.open)
                    {
                        pictureBox2.Image = Properties.Resources.open;
                        marketOpen = true;
                    }
                    else
                    {
                        pictureBox2.Image = Properties.Resources.close;
                        marketOpen = false;
                    }
                }
                else if (Convert.ToInt32(time[0]) == 3 && Convert.ToInt32(time[1]) > 30 && amPm.Equals("PM"))
                {
                    pictureBox2.Image = Properties.Resources.close;
                    marketOpen = false;
                }
                else if (pictureBox2.Image != Properties.Resources.open)
                {
                    pictureBox2.Image = Properties.Resources.open;
                    marketOpen = true;
                }
            }
            else if (Convert.ToInt32(time[0]) == 12 && amPm.Equals("PM"))
            {
                
                pictureBox2.Image = Properties.Resources.open;
                marketOpen = true;

            }
            else
            {
                pictureBox2.Image = Properties.Resources.close;
                marketOpen = false;
            }

            //if (Convert.ToInt32(time[0]) >= 9 && Convert.ToInt32(time[1]) >= 15 && amPm.Equals("AM"))
            //{

            //    (Convert.ToInt32(time[0]) == 3 && Convert.ToInt32(time[1])==30 && amPm.Equals("PM"))
            //{

            //}
            //else if (Convert.ToInt32(time[0]) == 9 && Convert.ToInt32(time[1]) == 15 && amPm.Equals("AM"))
            //{

            //}
            //var tempStatus = htmlDocument.DocumentNode.Descendants("span").Where(node => node.GetAttributeValue("id", "").Equals("status1")).ElementAt(0);
            //var tempStatus1 = htmlDocument.DocumentNode.Descendants("p").Where(node => node.GetAttributeValue("class", "").Equals("notification")).ElementAt(0).InnerText;

            //if (tempStatus.Contains("Open"))
            //{

            //}
            //else if (tempStatus.Contains("Closed"))
            //{

            //}
            //}
        }

        public string FetchURL(string company)
        {
            string url;
            string urlCompany = string.Empty;

            switch (company)
            {
                case "Maruti Suzuki":
                    urlCompany = "MARUTI";
                    break;
                case "Eicher Motors":
                    urlCompany = "EICHERMOT";
                    break;
                case "Tata Motors":
                    urlCompany = "TATAMOTORS";
                    break;
                case "IDFC":
                    urlCompany = "IDFCFIRSTB";
                    break;
                case "MRF":
                    urlCompany = "MRF";
                    break;
                case "Mahindra & Mahindra":
                    urlCompany = "M%26M";
                    break;
                case "Bajaj Auto":
                    urlCompany = "BAJAJ-AUTO";
                    break;
                case "ITC":
                    urlCompany = "ITC";
                    break;
                case "Ashok Leyland":
                    urlCompany = "ASHOKLEY";
                    break;
                case "Britannia":
                    urlCompany = "BRITANNIA";
                    break;
                case "Tata Motors DVR":
                    urlCompany = "TATAMTRDVR";
                    break;
                case "Hero Motorcorp":
                    urlCompany = "HEROMOTOCO";
                    break;
                case "Nestle":
                    urlCompany = "NESTLEIND";
                    break;
                case "CPSE ETF":
                    urlCompany = "CPSEETF";
                    break;
                case "Gillette":
                    urlCompany = "GILLETTE";
                    break;
                case "SML Isuzu":
                    urlCompany = "SMLISUZU";
                    break;
                case "HDFC":
                    urlCompany = "HDFCBANK";
                    break;
                case "Kotak Mahindra":
                    urlCompany = "KOTAKBANK";
                    break;
                case "Reliance":
                    urlCompany = "RELIANCE";
                    break;
                case "Airtel":
                    urlCompany = "BHARTIARTL";
                    break;
                case "Abbott":
                    urlCompany = "ABBOTINDIA";
                    break;
                case "Sun Pharma":
                    urlCompany = "SUNPHARMA";
                    break;
                case "Apollo":
                    urlCompany = "APOLLOHOSP";
                    break;
                case "Astrazen":
                    urlCompany = "ASTRAZEN";
                    break;
                    //case "CPSE ETF":
                    //    urlCompany = "CPSEETF";
                    //    break;
                    //case "CPSE ETF":
                    //    urlCompany = "CPSEETF";
                    //    break;
            }

        url = "https://www1.nseindia.com/live_market/dynaContent/live_watch/get_quote/GetQuote.jsp?symbol=" + urlCompany + "&illiquid=0&smeFlag=0&itpFlag=0";
        //url = "https://www.nseindia.com/get-quotes/equity?symbol=" +urlCompany;
            return url;
        }

        public string FetchMarketPrice(HtmlAgilityPack.HtmlDocument htmlDocument)
        {
            string marketPrice = string.Empty;

            marketPrice = htmlDocument.DocumentNode.Descendants("span").Where(node => node.GetAttributeValue("id", "").Equals("lastPrice")).ElementAt(0).InnerText;

            return marketPrice;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            form3 = new Form3(form1, this);
            form3.Show();
        }

        private void Shutdown(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            form1.Activated += new EventHandler(form1.CheckButtons);
            form1.showButton = false;
            form1.sellShare = true;
            form1.Show();
        }

        //private void AdjustDataGrid()
        //{
        //dataGridView1.AutoSize = false;
        //Size x = dataGridView1.Size;//1321 355
        //Size y = dataGridView1.ClientSize;


        //dataGridView1.Size = new Size(new Point(dataGridView1.Size.Width - 50, dataGridView1.Size.Height - 50));
        //int x = 0;
        //int y = 0;

        //for (int i =0;i<dataGridView1.Columns.Count;i++)
        //{
        //    y += dataGridView1.Columns[i].Width;
        //}

        //for (int i = 0; i < dataGridView1.Rows.Count; i++)
        //{
        //    x += dataGridView1.Rows[i].Height;
        //}
        // DataGridViewElementStates states = DataGridViewElementStates.Displayed;
        // int x1 = dataGridView1.Rows.GetRowsHeight(states);
        // int y1 = dataGridView1.Columns.GetColumnsWidth(states);
        //// dataGridView1.AutoSize = false;
        // dataGridView1.Size = new Size(1300, 300);

        //dataGridView1.ClientSize = new Size(new Point(x, y));

        //DataGridViewElementStates states = DataGridViewElementStates.None;
        ////dataGridView1.ScrollBars = ScrollBars.None;
        //var totalHeight = dataGridView1.Rows.GetRowsHeight(states) + dataGridView1.ColumnHeadersHeight;
        //totalHeight += dataGridView1.Rows.Count * 4;  // a correction I need
        //var totalWidth = dataGridView1.Columns.GetColumnsWidth(states) + dataGridView1.RowHeadersWidth;
        //dataGridView1.ClientSize = new Size(totalWidth, totalHeight);
        //}

        //private void TestFunction ()
        //{
        //    Thread[] threads = new Thread[index];

        //    for (int i=0;i<index;i++)
        //    {
        //        threads[i] = new Thread(() =>
        //        {
        //            LoadWebpage(urls[i]);
        //        });
        //        threads[i].Start();
        //    }
        //    for (int i=0;i<index;i++)
        //    {
        //        threads[i].Join();
        //    }
        //}

        //private void Button2_Click(object sender, EventArgs e)
        //{
        //    string oldTarget = string.Empty;
        //    string buySell = string.Empty;

        //    using (StreamReader file = new StreamReader("C:\\nl\\nl.txt"))
        //    {
        //        string newLine;

        //        while ((newLine = file.ReadLine()).Length > 0)
        //        {
        //            string[] line = newLine.Split(',');

        //            if (line[0].Equals(comboBox1.SelectedItem))
        //            {
        //                oldTarget = line[4];
        //                break;
        //            }
        //        }
        //    }
        //    string text = File.ReadAllText("C:\\nl\\nl.txt");
        //    text = text.Replace(oldTarget, textBox1.Text);
        //    File.WriteAllText("C:\\nl\\nl.txt", text);

        //    FetchData();
        //    comboBox1.Text = "";
        //    textBox1.Clear();
        //}

        //HttpClient httpClient = new HttpClient();
        //var html = await httpClient.GetStringAsync(url);

        //HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
        //htmlDocument.LoadHtml(html);
        ////htmlDocument.
        //var div = htmlDocument.DocumentNode.Descendants("span").Where(node => node.GetAttributeValue("id", "").Equals("lastPrice")).ElementAt(0);
        //var div1 = div.ChildNodes.ElementAt(1).InnerText;

        //webBrowser = new WebBrowser();
        //webBrowser.Navigate(url);
        //webBrowser.ScriptErrorsSuppressed = true;
        //webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(DocumentCompleted);

        //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        //HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        //Stream stream = response.GetResponseStream();
        //StreamReader reader = new StreamReader(stream, Encoding.GetEncoding(response.CharacterSet));
        //string data = reader.ReadToEnd();

        //this.webBrowser1.Navigate(url);
        //dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        //dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;


        //private void NiftyTimerDone(Object source, ElapsedEventArgs e)
        //{
        //    niftyTimer.Stop();
        //    //to be implemented
        //    //webBrowsers.Add(webBrowser);           

        //    FetchNiftyData();
        //}
        //to be implemented
        //niftyTimer = new System.Timers.Timer(5000);
        //niftyTimer.Elapsed += NiftyTimerDone;
        //niftyTimer.Enabled = true;
    }
}
