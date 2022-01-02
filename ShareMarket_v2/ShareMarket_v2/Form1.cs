using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShareMarket_v2
{
    public partial class Form1 : Form
    {
        string stockName;
        string[] fileData = new string[100];
        string buySell = string.Empty;
        string newLine = string.Empty;
        string dateTime = string.Empty;

        double buyingPrice;
        double investedAmount;
        double target;

        int index = 0;
        int numberShares;

        bool fileExists = false;
        public bool showButton = true;
        public bool fromWatchlist = false;
        public bool sellShare = false;

        Form2 form2;
        public Form3 form3;

        WebBrowser webBrowser;


        public Form1(Form3 frm = null, Form2 form = null)
        {
            InitializeComponent();

            if (showButton)
            {
                ShowButton();
            }
            else
            {
                HideButton();
            }

            if (File.Exists("C:\\nl\\nl.txt") && form == null)
            {
                form2 = new Form2(this);
                form2.Show();
                fileExists = true;
            }
            else if (form != null)
            {
                form2 = form;
            }
            else
            {
                form2 = new Form2(this);
            }
            dateTimePicker1.Format = DateTimePickerFormat.Custom;
            dateTimePicker1.CustomFormat = "dd MMM yyyy";
            label12.Hide();
            textBox6.Hide();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            //stockName = textBox1.Text;
            //buyingPrice = Convert.ToDouble(textBox2.Text);
            //numberShares = Convert.ToInt32(textBox3.Text);
            //target = Convert.ToDouble(textBox5.Text);

            //fileData[index] = stockName + "," + buyingPrice + "," + numberShares + "," + investedAmount + "," + target;

            if (!fileExists)
            {
                using (StreamWriter file = new StreamWriter("C:\\nl\\nl.txt"))
                {
                    for (int i = 0; i < index; i++)
                    {
                        file.WriteLine(fileData[i]);
                    }
                }
            }
            else if(showButton)
            {
                WriteAllFile();
            }

            if(!fromWatchlist)
            {
                form2.Activated += new EventHandler(form2.LoadDataAgain);
            }
            else
            {
                form2.form3.Activated += new EventHandler(form2.form3.LoadDataAgain);
            }

            fromWatchlist = false;
            sellShare = false;
            label11.Text = "Buying Date";
            this.Hide();
            form2.Show();
            ClearFields();
        }

        private void CalculateInvestedAmount(object sender, EventArgs e)
        {
            buyingPrice = Convert.ToDouble(textBox2.Text);

            if (textBox3.Text.Length > 0)
            {
                numberShares = Convert.ToInt32(textBox3.Text);
                investedAmount = Math.Round(buyingPrice * numberShares, 2);
                textBox4.Text = investedAmount.ToString();
            }

        }

        private void Button2_Click(object sender, EventArgs e)
        {
            FetchFormData();

            if (!fromWatchlist)
            {
                fileData[index++] = stockName + "," + buyingPrice + "," + numberShares + "," + investedAmount + "," + target + "," + buySell + "," + dateTime;
            }
            else
            {
                fileData[index++] = stockName + "," + target;
            }            

            ClearFields();
        }

        private void FetchFormData()
        {
            if (!fromWatchlist)
            {
                stockName = textBox1.Text;
                buyingPrice = Convert.ToDouble(textBox2.Text);
                numberShares = Convert.ToInt32(textBox3.Text);
                target = Convert.ToDouble(textBox5.Text);
                buySell = comboBox1.SelectedItem.ToString();
                dateTime = dateTimePicker1.Text;
            }
            else
            {
                stockName = textBox1.Text;
                target = Convert.ToDouble(textBox5.Text);
            }            
        }

        private void CheckForFile(object sender, EventArgs e)
        {
            if (fileExists)
            {
                this.Hide();
            }
        }

        private void ClearFields()
        {
            foreach (Control control in this.Controls)
            {
                if (control is TextBox)
                {
                    ((TextBox)control).Clear();
                }
                else if (control is ComboBox)
                {
                    ((ComboBox)control).Text = "";
                }
            }
            label10.Text = "--";
        }

        private void Label7_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void ShowButton()
        {
            button3.Hide();
            label8.Hide();
            comboBox2.Hide();
            label12.Hide();
            textBox6.Hide();

            label1.Show();
            textBox1.Show();
            button2.Show();
            label2.Show();
            textBox2.Show();
            label3.Show();
            textBox3.Show();
            label4.Show();
            textBox4.Show();
        }

        private void HideButton()
        {
            label1.Hide();
            textBox1.Hide();
            button2.Hide();

            dateTimePicker1.Show();
            label11.Show();

            if (sellShare)
            {
                label11.Text = "Selling Date";
                label12.Show();
                textBox6.Show();
            }

            label8.Show();
            label8.Location = new Point(28,43);
            label2.Show();
            textBox2.Show();
            label3.Show();
            label6.Show();
            textBox3.Show();
            label4.Show();
            label5.Location = new Point(28, 210);
            textBox4.Show();
            comboBox1.Show();
            comboBox2.Show();
            comboBox2.Location = new Point(212,43);
            comboBox2.DataSource = form2.companies;
            button3.Show();
            button3.Location = new Point(134, 370);
        }

        private void ShowButtonWatchlist()
        {
            button3.Hide();
            label8.Hide();
            comboBox2.Hide();
            label6.Hide();
            comboBox1.Hide();
            label2.Hide();
            textBox2.Hide();
            label3.Hide();
            textBox3.Hide();
            label4.Hide();
            textBox4.Hide();
            dateTimePicker1.Hide();
            label11.Hide();
            label12.Hide();
            textBox6.Hide();

            label1.Show();
            textBox1.Show();
            button2.Show();
            label5.Location = new Point(28, 84);
            textBox5.Location = new Point(212, 84);
        }

        private void HideButtonWatchlist()
        {
            label1.Hide();
            textBox1.Hide();
            button2.Hide();
            label6.Hide();
            comboBox1.Hide();
            label2.Hide();
            textBox2.Hide();
            label3.Hide();
            textBox3.Hide();
            label4.Hide();
            textBox4.Hide();
            dateTimePicker1.Hide();
            label11.Hide();
            label12.Hide();
            textBox6.Hide();

            label8.Show();
            label8.Location = new Point(28, 43);
            comboBox2.Show();
            comboBox2.Location = new Point(212, 43);
            comboBox2.DataSource = form2.form3.companies;
            label5.Location = new Point(28, 84);
            textBox5.Location = new Point(212, 84);
            button3.Show();
            button3.Location = new Point(134, 159);
        }

        public void CheckButtons(object sender, EventArgs e)
        {
            Activated -= new EventHandler(CheckButtons);

            if (showButton && !fromWatchlist)
            {
                ShowButton();
            }
            else if(showButton && fromWatchlist)
            {
                ShowButtonWatchlist();
            }
            else if (!fromWatchlist)
            {
                HideButton();
            }
            else
            {
                HideButtonWatchlist();
            }
        }

        private void LoadCompanyData(object sender, EventArgs e)
        {
            if (!fromWatchlist)
            {
                using (StreamReader file = new StreamReader("C:\\nl\\nl.txt"))
                {
                    while ((newLine = file.ReadLine()) != null && newLine.Length > 0)
                    {
                        string[] line = newLine.Split(',');

                        if (line[0].Equals(comboBox2.SelectedItem.ToString()))
                        {
                            textBox2.Text = line[1];
                            textBox3.Text = line[2];
                            textBox4.Text = line[3];
                            textBox5.Text = line[4];
                            comboBox1.SelectedItem = line[5];
                            dateTimePicker1.Value = Convert.ToDateTime(line[6]);
                            break;
                        }
                    }
                }
            }
            else
            {
                using (StreamReader file = new StreamReader("C:\\nl\\nl1.txt"))
                {
                    while ((newLine = file.ReadLine()) != null && newLine.Length > 0)
                    {
                        string[] line = newLine.Split(',');

                        if (line[0].Equals(comboBox2.SelectedItem.ToString()))
                        {                            
                            textBox5.Text = line[1];
                            break;
                        }
                    }
                }
            }

            GetMarketPrice(sender, e);
        }

        private void WriteAllFile()
        {
            string text = string.Empty;
                
            if (!fromWatchlist)
            {
                text = File.ReadAllText("C:\\nl\\nl.txt");
            }
            else
            {
                //fromWatchlist = false;
                text = File.ReadAllText("C:\\nl\\nl1.txt");
            }              

            for (int i = 0; i < index; i++)
            {
                text += fileData[i] + Environment.NewLine;
            }

            if (!fromWatchlist)
            {
                File.WriteAllText("C:\\nl\\nl.txt", text);
            }
            else
            {
                File.WriteAllText("C:\\nl\\nl1.txt", text);
            }            
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            FetchFormData();
            string tempText = string.Empty;
            string text = string.Empty;
            string tempSellText = string.Empty;

            if (!fromWatchlist)
            {
                tempText = comboBox2.SelectedItem.ToString() + "," + buyingPrice + "," + numberShares + "," + investedAmount + "," + target + "," + buySell + "," + dateTime;
                text = File.ReadAllText("C:\\nl\\nl.txt");

                if (sellShare)
                {
                    var test = form2.companies.ToList();
                    int testIndex = test.IndexOf(comboBox2.SelectedItem.ToString());
                    test.RemoveAt(testIndex);
                    form2.companies = test.ToArray();

                    test = form2.urls.ToList();
                    test.RemoveAt(testIndex);
                    form2.urls = test.ToArray();

                    form2.webBrowsers.RemoveAt(testIndex);                    
                    text = text.Replace(newLine + Environment.NewLine, string.Empty);

                    if (File.Exists("C:\\nl\\nl2.txt"))
                    {
                        tempSellText = File.ReadAllText("C:\\nl\\nl2.txt");
                    }                    
                    tempSellText += newLine + "," + dateTime + "," + textBox6.Text + Environment.NewLine;
                    File.WriteAllText("C:\\nl\\nl2.txt", tempSellText);
                }
                else
                {
                    text = text.Replace(newLine, tempText);
                }                
                File.WriteAllText("C:\\nl\\nl.txt", text);
            }
            else
            {
                tempText = comboBox2.SelectedItem.ToString() + "," + target;
                text = File.ReadAllText("C:\\nl\\nl1.txt");
                text = text.Replace(newLine, tempText);
                File.WriteAllText("C:\\nl\\nl1.txt", text);
            }

            ClearFields();
        }

        private void GetMarketPrice(object sender, EventArgs e)
        {
            //int index = Array.IndexOf(form2.companies, textBox1.Text);
            string url = string.Empty;

            if (showButton && textBox1!= null && textBox1.Text.Length > 0)
            {
                url = form2.FetchURL(textBox1.Text);
            }
            else if (!showButton && comboBox2 != null && comboBox2.SelectedItem != null)
            {
                url = form2.FetchURL(comboBox2.SelectedItem.ToString());
            }            

            if (url.Length > 0)
            {
                webBrowser = new WebBrowser();
                webBrowser.ScriptErrorsSuppressed = true;
                webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(DocumentCompleted);
                webBrowser.Navigate(url);
            }
        }

        private void DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
            htmlDocument.LoadHtml((webBrowser).DocumentText);
            label10.Text = form2.FetchMarketPrice(htmlDocument);
        }
    }
}
