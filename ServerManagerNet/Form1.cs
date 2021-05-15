using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Threading;

namespace ServerManagerNet
{
    public partial class Form1 : Form
    {
        public string connectString = @"Data Source=Localhost;Initial Catalog=ManagerNET;Integrated Security=True";

        int recv;
        IPEndPoint ipep;
        Socket server;
        byte[] data = new byte[1024];
        byte[] dataSend = new byte[1024];
        List<Socket> clientList;
        public Form1()
        {
            InitializeComponent();
            Connect();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetDataCBX();
            dataGridView1.DataSource = GetAllListHoiVien().Tables[0];
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            //Test();
            dataGridView1.DataSource = GetAllMenu(cbxListTable.SelectedItem.ToString().Trim()).Tables[0];
        }
        void SetDataCBX()
        {
            string query = "select * from DanhSachMenu";
            using (SqlConnection connection = new SqlConnection(connectString))
            {
                connection.Open();
                SqlCommand sqlc = new SqlCommand(query, connection);
                SqlDataReader adapter = sqlc.ExecuteReader();
                while (adapter.Read())
                {
                    cbxListTable.Items.Add(adapter.GetString("name").Trim());
                }
                adapter.Close();
                connection.Close();
                cbxListTable.SelectedIndex = 0;
            }
        }
        DataSet GetAllMenu(string x)
        {
            string s = x.Replace(" ", "");
            DataSet data = new DataSet();
            string query = "select * from " + s;
            using(SqlConnection connection = new SqlConnection(connectString))
            {
                connection.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                adapter.Fill(data);
                connection.Close();
            }
            return data;
        }
        DataSet GetAllListHoiVien()
        {
            DataSet data = new DataSet();
            string query = "select * from DanhSachHoiVien";
            using (SqlConnection connection = new SqlConnection(connectString))
            {
                connection.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                adapter.Fill(data);
                connection.Close();
            }
            return data;
        }

        void Connect()
        {
            clientList = new List<Socket>();
            ipep = new IPEndPoint(IPAddress.Any, 9050);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            server.Bind(ipep);
            Thread listen = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        server.Listen(100);
                        Socket client = server.Accept();
                        clientList.Add(client);

                        Thread recvice = new Thread(Recv);
                        recvice.IsBackground = true;
                        recvice.Start(client);
                    }
                }
                catch {
                    ipep = new IPEndPoint(IPAddress.Any, 9050);
                    server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }
            });
            listen.IsBackground = true;
            listen.Start();
        }

        void Send(Socket client)
        {

        }

        void Recv(object obj)
        {
            Socket client = obj as Socket;
            try
            {
                while(true)
                {

                    recv = client.Receive(data);
                    string s = Encoding.ASCII.GetString(data, 0, recv);
                    string[] ss = s.Split(":");

                    if(ss[0] == "order")
                    {
                        string[] row1 = { ss[2] };
                        listView1.Items.Add(ss[1]).SubItems.AddRange(row1);
                        listView1.EnsureVisible(listView1.Items.Count - 1);
                    }
                    if (ss[0] == "login")
                    {
                        MessageBox.Show("LOGIN");
                    }
                    if (ss[0] == "recharge")
                    {
                        MessageBox.Show("NAP TIEN");
                    }
                    if (ss[0] == "support")
                    {
                        MessageBox.Show("SUPPORT");
                    }
                    if (ss[0] == "chat")
                    {
                        textBox1.Text += "\n" + ss[1].Trim() + ": " + ss[2].Trim();
                    }

                }
            }
            catch { }
        }
        void Test()
        {
            string[] row1 = { "Sửa Chua" };
            listView1.Items.Add("Test").SubItems.AddRange(row1);
        }
    }
}
