using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Windows.Threading;

namespace ShareMarket_Enhanced
{
    public partial class Form1 : Form
    {
        readonly string[] columnNames = { "Share", "Market Price" };
        private System.Timers.Timer populateStockDatatimer;

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            Utility.LoadUrls();
            InitializeTimers();
            await PrepareStockData();
            PopulateStockData();
        }

        private void InitializeTimers()
        {
            populateStockDatatimer = new System.Timers.Timer(10000);
            populateStockDatatimer.Elapsed += TimerDone;
            populateStockDatatimer.Enabled = true;
        }        

        private async Task PrepareStockData()
        {
            foreach (string key in Utility.stockUrls.Keys)
            {
                Console.WriteLine("PrepareStockDataAsync:" + Thread.CurrentThread.ManagedThreadId);
                await LoadDataAsync(key, Utility.stockUrls[key]);
                Console.WriteLine("PrepareStockDataAsync After:" + Thread.CurrentThread.ManagedThreadId);
            }
            Console.WriteLine("PrepareStockDataAsync Timer Started:" + Thread.CurrentThread.ManagedThreadId);
            //populateStockDatatimer.Start();
        }

        private async void TimerDone(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("TimerDone:" + Thread.CurrentThread.ManagedThreadId);
            populateStockDatatimer.Stop();
            PopulateStockData();
            Console.WriteLine("TimerDone After:" + Thread.CurrentThread.ManagedThreadId);
            await PrepareStockData();
        }

        private async Task LoadDataAsync(string stock, string url)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    Console.WriteLine("LoadDataAsync:" + Thread.CurrentThread.ManagedThreadId);
                    var result = await client.GetAsync(url);
                    string content = await result.Content.ReadAsStringAsync();
                    int index = content.IndexOf("<span class='fs24e cmp fw500 '>") + "<span class='fs24e cmp fw500 '>".Length;
                    string price = content.Substring(index, content.IndexOf("</span>", index) - index);
                    Utility.stockData[stock] = price;
                    Console.WriteLine("LoadDataAsync After:" + Thread.CurrentThread.ManagedThreadId);
                }
            }
            catch (Exception)
            {

            }
        }
        delegate void PopulateStockDataCallback();

        private void PopulateStockData()
        {   
            if(dataGridView1.InvokeRequired)
            {
                Console.WriteLine("PopulateStockData if:" + Thread.CurrentThread.ManagedThreadId);

                PopulateStockDataCallback d = new PopulateStockDataCallback(PopulateStockData);
                this.Invoke(d, new object[] {});
            }
            else
            {
                Task.Run(() =>
                {
                    Dispatcher.Invoke
                }
                );
                Console.WriteLine("PopulateStockData else:" + Thread.CurrentThread.ManagedThreadId);

                DataTable dataTable = new DataTable();

                foreach (string column in columnNames)
                {
                    dataTable.Columns.Add(column);
                }

                foreach (string key in Utility.stockData.Keys)
                {
                    DataRow dataRow = dataTable.NewRow();
                    dataRow[0] = key;
                    dataRow[1] = Utility.stockData[key];
                    dataTable.Rows.Add(dataRow);
                }
                dataGridView1.DataSource = dataTable;
            }                       
        }

        private void button1_ClickAsync(object sender, EventArgs e)
        {
            //try
            //{
            //    using (var client = new HttpClient())
            //    {
            //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //        var result = await client.GetAsync("https://www.indiainfoline.com/company/bharti-airtel-ltd-share-price/15542");
            //        string content = await result.Content.ReadAsStringAsync();
            //        int index = content.IndexOf("<span class='fs24e cmp fw500 '>") + "<span class='fs24e cmp fw500 '>".Length;
            //        string price = content.Substring(index, content.IndexOf("</span>", index) - index);
            //        MessageBox.Show(price);
            //    }
            //}
            //catch (Exception ee)
            //{

            //}
        }
    }
}
