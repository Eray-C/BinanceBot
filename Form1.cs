using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace BinanceBot
{
    public partial class Form1 : Form
    {
        
        public Form1()
        {
            InitializeComponent();
        }
        string connectionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=BinanceBot;Integrated Security=True";
        public void LoadPrices()
        {
            dataGridView1.DataSource = GetPrices();
        }

        public List<Price> GetPrices()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"https://api.binance.com/api/v3/ticker/24hr");
            request.Timeout = 5000;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string content = new StreamReader(response.GetResponseStream()).ReadToEnd();

            List<Price> PriceList = JsonConvert.DeserializeObject<List<Price>>(content);
            List<Price> filteredList = PriceList.Where(p => p.symbol.EndsWith("USDT")).Where(p => !p.symbol.Contains("BEAR")).Where(p => !p.symbol.Contains("BULL")).Where(p => !p.symbol.Contains("UP")).Where(p => !p.symbol.Contains("DOWN")).ToList();
            return filteredList;
        }

        public void senddatasql()
        {
            using (SqlConnection connect = new SqlConnection(connectionString))
            using (SqlConnection connect2 = new SqlConnection(connectionString))
            {

                SqlCommand EraseTableCommand = new SqlCommand("DELETE Coins", connect2);
                connect2.Open();
                EraseTableCommand.ExecuteNonQuery();
                connect2.Close();

                SqlCommand cmd = new SqlCommand("INSERT INTO Coins(symbol, priceChange, priceChangePercent,lastPrice) VALUES (@val1, @val2, @val3,@val4)", connect);

                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    cmd.Parameters.AddWithValue("@val1", dataGridView1.Rows[i].Cells[0].Value);
                    cmd.Parameters.AddWithValue("@val2", dataGridView1.Rows[i].Cells[1].Value);
                    cmd.Parameters.AddWithValue("@val3", dataGridView1.Rows[i].Cells[2].Value);
                    cmd.Parameters.AddWithValue("@val4", dataGridView1.Rows[i].Cells[3].Value);
                    connect.Open();
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                    connect.Close();
                }
            }
        }
        public List<Price> getdatasql()
        {
            SqlConnection connect = new SqlConnection(connectionString);
            connect.Open();
            string query = "SELECT * FROM Coins ";
            SqlCommand command = new SqlCommand(query, connect);
            SqlDataReader reader = command.ExecuteReader();
            DataTable dataTable = new DataTable();

            dataTable.Load(reader);
            reader.Close();
            dataGridView2.AutoGenerateColumns = true;
            dataGridView2.DataSource = dataTable;
            List<Price> list = ((DataTable)dataGridView2.DataSource).AsEnumerable()
                    .Select(row => new Price
                    {
                        symbol = row.Field<string>("symbol"),
                        lastPrice = row.Field<double>("lastPrice"),
                        priceChangePercent = row.Field<double>("priceChangePercent")
                    })
                    .ToList();
            connect.Close();
            return list;


        }



        private void button2_Click(object sender, EventArgs e)
        {
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //LoadPrices();
            ////

        }


        private void timer1_Tick(object sender, EventArgs e)//60 saniye
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                LoadPrices();
            senddatasql();
            Thread.Sleep(60 * 1000);
            });
        }
        private void timer3_Tick(object sender, EventArgs e)//60 saniye
        {
            getdatasql();
        }
        private void timer2_Tick(object sender, EventArgs e)//60saniye
        {

            List<Price> previousPrices = getdatasql();
            List<Price> currentPrices = GetPrices();
            foreach (var current in currentPrices)
            {
                var previous = previousPrices.FirstOrDefault(p => p.symbol == current.symbol);

                if (previous != null && current.lastPrice > previous.lastPrice * 1.1)
                {
                    string message = $"{current.symbol} yüzde 10'dan fazla arttı ({current.priceChangePercent}%).";
                    SendMessage(message);
                }
            }

            dataGridView1.DataSource = currentPrices;
            dataGridView2.DataSource = previousPrices;



        }

        private void SendMessage(string message)
        {
            try
            {
                string botToken = "6195578957:AAEtAN5JIkYCm_vlGYXfgedSIXdNXcwovE0";
                string chatId = "1424481356";
                string url = $"https://api.telegram.org/bot{botToken}/sendMessage?chat_id={chatId}&text={message}";

                using (WebClient client = new WebClient())
                {
                    client.DownloadString(url);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("HATA" + ex);
            }

        }


        private void searchbtn_Click(object sender, EventArgs e)
        {
            string searchTerm = textBox1.Text.ToUpper();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Value != null && cell.Value.ToString().ToUpper().Contains(searchTerm))
                    {
                        row.Selected = true;
                        dataGridView1.FirstDisplayedScrollingRowIndex = row.Index;
                        return;
                    }
                }
            }

        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == System.Windows.Forms.Keys.Enter)
            {
                searchbtn.PerformClick();
            }
        }
        private void exitbtn_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    

    }
}
