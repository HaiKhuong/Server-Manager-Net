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
using System.Security.Cryptography;

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

        int countNofti = 1;
        public Form1()
        {
            InitializeComponent();
            Connect();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetDataCBX();
            dataGridView1.DataSource = GetAllListHoiVien().Tables[0];
            tbMessClient2.TextAlign = HorizontalAlignment.Right;
            lbCountNofti.BackColor = System.Drawing.Color.FromArgb(244, 67, 54);
            pnNofti1.Visible = false;
            tbMessClient1.ReadOnly = tbMessClient2.ReadOnly = true;
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
                try
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
                catch { }
            }

        }
        DataSet GetAllMenu(string x)
        {
            string s = x.Replace(" ", "");
            DataSet data = new DataSet();
            string query = "select * from " + s;
            using (SqlConnection connection = new SqlConnection(connectString))
            {
                try
                {
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    adapter.Fill(data);
                    connection.Close();
                }
                catch { }
            }
            return data;
        }
        DataSet GetAllListHoiVien()
        {
            DataSet data = new DataSet();
            string query = "select * from DanhSachHoiVien";
            using (SqlConnection connection = new SqlConnection(connectString))
            {
                try
                {
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    adapter.Fill(data);
                    connection.Close();
                }
                catch { }
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
                catch
                {
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
                while (true)
                {

                    recv = client.Receive(data);
                    string s = Encoding.ASCII.GetString(data, 0, recv);
                    string[] ss = s.Split(":");

                    //if (ss[0] == "order")
                    //{
                    //    string[] row1 = { ss[2] };
                    //    listView1.Items.Add(ss[1]).SubItems.AddRange(row1);
                    //    listView1.EnsureVisible(listView1.Items.Count - 1);

                    //    string maLichSuGD = RandomString(10);
                    //    string query = "insert into DanhSachNapTien(TenDichVu, MaKichSuGD, SDT, SoLuong, ThoiGianMua) values(@TenDichVu, @MaKichSuGD, @SDT, @SoLuong, @ThoiGianMua)";
                    //    using (SqlConnection connection = new SqlConnection(connectString))
                    //    {
                    //        try
                    //        {
                    //            connection.Open();
                    //            SqlCommand cmd = new SqlCommand(query, connection);
                    //            cmd.Parameters.AddWithValue("@TenDichVu", ss[1]);
                    //            cmd.Parameters.AddWithValue("@MaKichSuGD", maLichSuGD.Trim());
                    //            cmd.Parameters.AddWithValue("@SDT", ss[2]);
                    //            cmd.Parameters.AddWithValue("@SoSoLuongLuong", ss[3]);
                    //            cmd.Parameters.AddWithValue("@ThoiGianMua", ss[4]);
                    //            cmd.ExecuteNonQuery();
                    //            connection.Close();
                    //        }
                    //        catch { }
                    //    }
                    //}
                    if (ss[0] == "login")
                    {
                        MessageBox.Show("LOGIN");
                        string query = "select password from DanhSachHoiVien where = @sdt";
                        using (SqlConnection connection = new SqlConnection(connectString))
                        {
                            try
                            {
                                connection.Open();
                                SqlCommand command = new SqlCommand(query, connection);
                                command.Parameters.AddWithValue("@sdt", ss[1].Trim());
                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        string sdt = String.Format("{0}", reader["MatKhau"]);
                                        if (CreateMD5(sdt).Trim() == ss[2].Trim())
                                        {
                                            //SEND CLIENT
                                        }
                                    }
                                }
                                connection.Close();
                            }
                            catch { }
                        }
                    }
                    if (ss[0] == "recharge" || ss[0] == "order")
                    {
                        string maYeuCau = RandomString(5);
                        MessageBox.Show("NAP TIEN");
                        string query = "insert into DanhSacYeuCau(MaYeuCau, SDT, TenDichVu, TrangThai, SoLuong, NgayGioYeuCau) values(@MaYeuCau, @SDT, @TenDichVu, @TrangThai, @SoLuong, @NgayGioYeuCau)";
                        using (SqlConnection connection = new SqlConnection(connectString))
                        {
                            try
                            {
                                connection.Open();
                                SqlCommand cmd = new SqlCommand(query, connection);
                                cmd.Parameters.AddWithValue("@MaYeuCau", maYeuCau.Trim());
                                cmd.Parameters.AddWithValue("@SDT", ss[1]);
                                cmd.Parameters.AddWithValue("@TenDichVu", ss[2]);
                                cmd.Parameters.AddWithValue("@TrangThai", 1);
                                cmd.Parameters.AddWithValue("@SoLuong", ss[3]);
                                cmd.Parameters.AddWithValue("@NgayGioYeuCau", ss[4]);
                                cmd.ExecuteNonQuery();
                                connection.Close();
                            }
                            catch { }
                        }
                        string[] row1 = { ss[2] };
                        listView1.Items.Add(ss[1]).SubItems.AddRange(row1);
                        listView1.EnsureVisible(listView1.Items.Count - 1);

                        string maLichSuGD = RandomString(10);
                        string query1 = "insert into DanhSachNapTien(TenDichVu, MaKichSuGD, SDT, SoLuong, ThoiGianMua) values(@TenDichVu, @MaKichSuGD, @SDT, @SoLuong, @ThoiGianMua)";
                        using (SqlConnection connection = new SqlConnection(connectString))
                        {
                            try
                            {
                                connection.Open();
                                SqlCommand cmd = new SqlCommand(query1, connection);
                                cmd.Parameters.AddWithValue("@TenDichVu", ss[1]);
                                cmd.Parameters.AddWithValue("@MaKichSuGD", maLichSuGD.Trim());
                                cmd.Parameters.AddWithValue("@SDT", ss[2]);
                                cmd.Parameters.AddWithValue("@SoSoLuongLuong", ss[3]);
                                cmd.Parameters.AddWithValue("@ThoiGianMua", ss[4]);
                                cmd.ExecuteNonQuery();
                                connection.Close();
                            }
                            catch { }
                        }
                    }
                    if (ss[0] == "chat")
                    {
                        countNofti++;
                        if (countNofti > 0)
                        {
                            lbCountNofti.Text = countNofti.ToString();
                            pnNofti1.Visible = true;
                        }
                        tbMessClient1.Text += ss[1].Trim() + ": " + ss[2].Trim();
                        tbMessClient2.Text += "\r\n";
                        tbMessClient1.Text += "\r\n";
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

        private void btnSendChat_Click(object sender, EventArgs e)
        {
            tbMessClient2.Text += tbMessChat.Text;
            tbMessClient1.Text += "\r\n";
            tbMessClient2.Text += "\r\n";
            tbMessChat.Text = "";
        }
        public static string HashSHA512(string value)
        {
            using (SHA512 sha512Hash = SHA512.Create())
            {
                byte[] sourceBytes = Encoding.UTF8.GetBytes(value);
                byte[] hashBytes = sha512Hash.ComputeHash(sourceBytes);
                string hash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
                return hash;
            }
        }
        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        private void picNofti_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 2;
            pnNofti1.Visible = false;
        }
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
