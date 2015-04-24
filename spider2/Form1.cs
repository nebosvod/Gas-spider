using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using MySql.Data;
using System.Threading;
using System.IO;
using System.Net.Mail;
using System.Web;
using System.Net;


namespace spider2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        byte[] data_coll(string date1, string time1)
        {
            byte[] data_end = { 0x01, 0x42, 0x30, 0x03, 0x71, 0x01, 0x42, 0x30, 0x03, 0x71 };
            byte[] data_next = { 0x06 };
            byte[] ra = { 0x01, 0x52, 0x33, 0x02 };
            byte[] etx = { 0x03 };

            string begreq = "3:V.0(3;";
            string endreq = ";1)";
            string s = begreq + date1 + "," + time1 + ";" + date1 + "," + time1 + endreq;

            byte[] midreq = new byte[s.Length];

            int i = 0;

            foreach (char c in s)
            {
                midreq[i] = Convert.ToByte(c);
                i++;
            };

            byte[] reqcrc = new byte[ra.Length + midreq.Length + etx.Length];

            for (int k = 0; k < ra.Length; k++)
            {
                reqcrc[k] = ra[k];
            };
            for (int k = 0; k < midreq.Length; k++)
            {
                reqcrc[k + ra.Length] = midreq[k];
            };
            for (int k = 0; k < etx.Length; k++)
            {
                reqcrc[k + ra.Length + midreq.Length] = etx[k];
            };

            int crc = reqcrc[1];
            for (int k = 2; k < reqcrc.Length; k++)
            {
                crc = crc ^ reqcrc[k];
            };

            byte[] req = new byte[reqcrc.Length + 1];

            for (int k = 0; k < reqcrc.Length; k++)
            {
                req[k] = reqcrc[k];
            };

            // Преобразование целого числа в байт
            byte[] intBytes = BitConverter.GetBytes(crc);
            Array.Reverse(intBytes);
            byte[] result = intBytes;
            req[reqcrc.Length] = result[3];
            //---------------------------------------

            return req;
        }



        private void button1_Click(object sender, EventArgs e)
        {


           /* string[] portnames = SerialPort.GetPortNames();
            SerialPort port = new SerialPort("COM12", 19200, Parity.Even, 7, StopBits.One);
            string conn_str = "Database=resources;Data Source=localhost;User Id=user;Password=password";


            byte[] data_end = { 0x01, 0x42, 0x30, 0x03, 0x71, 0x01, 0x42, 0x30, 0x03, 0x71 };
            byte[] data_next = { 0x06 };

            byte[] data1 = { 47, 63, 33, 13, 10 };
            byte[] data3 = { 0x06, 0x30, 0x36, 0x31, 0x0D, 0x0A };


            byte[] ra = { 0x01, 0x52, 0x33, 0x02 };
            byte[] etx = { 0x03 };


            string day_str;
            string month_str;
            month_str = "00";
            string year_str;
            string date_str;


            string hour_str;


            StreamWriter wr1 = new StreamWriter("gas_log.txt");
            wr1.WriteLine(DateTime.Now + ": Начало старта сбора данных с газового счетчика");



            

            MySqlLib.MySqlData.MySqlExecute.MyResult result = new MySqlLib.MySqlData.MySqlExecute.MyResult();
            result = MySqlLib.MySqlData.MySqlExecute.SqlScalar("SELECT gas_datetime FROM gas ORDER BY gas_id DESC LIMIT 0,1", conn_str);

            string date_from_mysql = result.ResultText;

            DateTime date1 = DateTime.Now;
            DateTime date2 = DateTime.Now;

            if (date_from_mysql.Length == 19)
            {
                date2 = new DateTime(Convert.ToInt32(date_from_mysql.Substring(6, 4)), Convert.ToInt32(date_from_mysql.Substring(3, 2)), Convert.ToInt32(date_from_mysql.Substring(0, 2)), Convert.ToInt32(date_from_mysql.Substring(11, 2)), Convert.ToInt32(date_from_mysql.Substring(14, 2)), Convert.ToInt32(date_from_mysql.Substring(17, 2)));
            }
            if (date_from_mysql.Length == 18)
            {
                date2 = new DateTime(Convert.ToInt32(date_from_mysql.Substring(6, 4)), Convert.ToInt32(date_from_mysql.Substring(3, 2)), Convert.ToInt32(date_from_mysql.Substring(0, 2)), Convert.ToInt32(date_from_mysql.Substring(11, 1)), Convert.ToInt32(date_from_mysql.Substring(13, 2)), Convert.ToInt32(date_from_mysql.Substring(16, 2)));
            }

            wr1.WriteLine(DateTime.Now + ": Дата начала сбора данных - " + date2);



            TimeSpan interval = date1 - date2;
            wr1.WriteLine(DateTime.Now + ": Дата конца сбора данных - " + date2.AddHours(Convert.ToInt32(Math.Floor(interval.TotalHours))));

            wr1.WriteLine(DateTime.Now + ": Количество циклов для сбора данных - " + Convert.ToInt32(Math.Floor(interval.TotalHours)));

            if (interval.TotalHours < 1)
            {
                MessageBox.Show("Данные за период запроса отсутствуют!");
                wr1.WriteLine(DateTime.Now + ": Данные за период запроса отсутствуют!");
            }
            if (interval.TotalHours >= 1)
            {

                int hours = Convert.ToInt32(Math.Floor(interval.TotalHours));

                string[] v_st_s = new string[hours];
                string[] v_r_s = new string[hours];
                string[] pressure = new string[hours];
                string[] temperature = new string[hours];
                string[] kkor = new string[hours];
                string[] sys_status = new string[hours];
                string[] status_vr = new string[hours];
                string[] status_vst = new string[hours];
                string[] status_p = new string[hours];
                string[] status_t = new string[hours];
                string[] n_sit = new string[hours];
                string[] n_sit2 = new string[hours];
                string[] crc_ok = new string[hours];
                string[] datetime = new string[hours];
                string[] datetime_mysql = new string[hours];

                port.Open();

                port.Write(data1, 0, data1.Length);
                Thread.Sleep(1500);

                int byteRecieved = port.BytesToRead;
                byte[] messByte = new byte[byteRecieved];
                port.Read(messByte, 0, byteRecieved);
                Thread.Sleep(1500);

                port.Write(data3, 0, data3.Length);
                Thread.Sleep(1500);

                int byteRecieved2 = port.BytesToRead;
                byte[] messByte2 = new byte[byteRecieved2];
                port.Read(messByte2, 0, byteRecieved2);
                Thread.Sleep(1500);


                progressBar1.Maximum = Convert.ToInt32(hours);
                int k = 0;

                for (k = 0; k <= (interval.TotalHours - 1); k++)
                {
                    date2 = date2.AddHours(1);

                    year_str = Convert.ToString(date2.Year);
                    month_str = Convert.ToString(date2.Month);
                    if (month_str.Length < 2) month_str = '0' + month_str;
                    day_str = Convert.ToString(date2.Day);
                    if (day_str.Length < 2) day_str = '0' + day_str;

                    date_str = year_str + "-" + month_str + "-" + day_str;

                    hour_str = Convert.ToString(date2.Hour);
                    if (hour_str.Length < 2) hour_str = '0' + hour_str;

                    byte[] req = data_coll(date_str, hour_str + ":00:00");

                    port.Write(req, 0, req.Length);
                    Thread.Sleep(4000);



                    int byteRecieved3 = port.BytesToRead;
                    byte[] messByte3 = new byte[byteRecieved3];
                    port.Read(messByte3, 0, byteRecieved3);
                    Thread.Sleep(1500);

                    string s1 = Encoding.ASCII.GetString(messByte3);

                    string[] words = s1.Split('(', ')');



                    v_st_s[k] = words[9];
                    v_r_s[k] = words[13];
                    pressure[k] = words[15];
                    temperature[k] = words[17];
                    kkor[k] = words[21];
                    sys_status[k] = words[23];
                    status_vr[k] = words[25];
                    status_vst[k] = words[27];
                    status_p[k] = words[29];
                    status_t[k] = words[31];
                    n_sit[k] = words[1];
                    n_sit2[k] = words[3];
                    crc_ok[k] = words[35];
                    datetime[k] = words[5];

                    progressBar1.Value = k;
                }



                port.Write(data_end, 0, data_end.Length);
                port.Close();
                wr1.WriteLine(DateTime.Now + ": Сбор данных со счетчика завершен");


                for (k = 0; k <= (interval.TotalHours - 1); k++)
                {
                    result = MySqlLib.MySqlData.MySqlExecute.SqlScalar("SELECT gas_v_r_s FROM gas ORDER BY gas_id DESC LIMIT 0,1", conn_str);
                    
                    char[] chars = v_r_s[k].ToCharArray();
                    for (int j = 0; j < v_r_s[k].Length; j++)
                    {
                        if (chars[j] == '.')
                        {
                            chars[j] = ',';
                        }
                    }
                    string v_r_s_dec = new string(chars);
                    
                   decimal v_r_p = Convert.ToDecimal(v_r_s_dec) - Convert.ToDecimal(result.ResultText);
                   string v_r_p_str = Convert.ToString(v_r_p);
                   
                   char[] chars2 = v_r_p_str.ToCharArray();
                   for (int l = 0; l < v_r_p_str.Length; l++)
                   {
                       if (chars2[l] == ',')
                       {
                           chars2[l] = '.';
                       }
                   }
                   string v_r_p_str2 = new string(chars2);

                   result = MySqlLib.MySqlData.MySqlExecute.SqlScalar("SELECT gas_v_st_s FROM gas ORDER BY gas_id DESC LIMIT 0,1", conn_str);

                   char[] chars3 = v_st_s[k].ToCharArray();
                   for (int m = 0; m < v_st_s[k].Length; m++)
                   {
                       if (chars3[m] == '.')
                       {
                           chars3[m] = ',';
                       }
                   }
                   string v_st_s_dec = new string(chars3);

                   decimal v_st_p = Convert.ToDecimal(v_st_s_dec) - Convert.ToDecimal(result.ResultText);
                   string v_st_p_str = Convert.ToString(v_st_p);

                   char[] chars4 = v_st_p_str.ToCharArray();
                   for (int n = 0; n < v_st_p_str.Length; n++)
                   {
                       if (chars4[n] == ',')
                       {
                           chars4[n] = '.';
                       }
                   }
                   string v_st_p_str2 = new string(chars4);

                   result = MySqlLib.MySqlData.MySqlExecute.SqlScalar("SELECT gas_n FROM gas ORDER BY gas_id DESC LIMIT 0,1", conn_str);

                   int gas_mark_gray = 0;
                   if ((Convert.ToInt32(n_sit[k]) - Convert.ToInt32(result.ResultText)) != 1)
                   {
                       gas_mark_gray = 1;
                   }
                   else
                   {
                       gas_mark_gray = 0;
                   }



                    datetime_mysql[k] = datetime[k].Substring(0, 4) + datetime[k].Substring(5, 2) + datetime[k].Substring(8, 2) + datetime[k].Substring(11, 2) + datetime[k].Substring(14, 2) + datetime[k].Substring(17, 2);
                    result = MySqlLib.MySqlData.MySqlExecute.SqlNoneQuery("INSERT INTO gas (`gas_n`,`gas_n2`,`gas_datetime`,`gas_v_r_s`,`gas_v_st_s`,`gas_pressure`,`gas_temperature`,`gas_kkor`,`gas_sys_status`,`gas_status_vr`,`gas_status_vst`,`gas_status_p`,`gas_status_t`,`gas_crc_ok`,`gas_v_r_p`,`gas_v_st_p`,`gas_mark_gray`) VALUES (" + n_sit[k] + "," + n_sit2[k] + "," + datetime_mysql[k] + "," + v_r_s[k] + "," + v_st_s[k] + "," + pressure[k] + "," + temperature[k] + "," + kkor[k] + "," + sys_status[k] + "," + status_vr[k] + "," + status_vst[k] + "," + status_p[k] + "," + status_t[k] + ",'" + crc_ok[k] + "','" + v_r_p_str2 + "','" + v_st_p_str2 + "','" + gas_mark_gray + "')", conn_str);
                }
                wr1.WriteLine(DateTime.Now + ": Занесение данных в базу завершено");


            }
            wr1.Close();


            using (MailMessage mm = new MailMessage("user@mailserver", "user@mailserver"))
            {
                mm.SubjectEncoding = Encoding.GetEncoding(1251);
                mm.BodyEncoding = Encoding.GetEncoding(1251);
                mm.Subject = "Отчет сбора данных по газу";
                mm.Body = "Отчет сбора данных по газу";
                mm.IsBodyHtml = false;
                mm.Attachments.Add(new Attachment("gas_log.txt"));
                using (SmtpClient sc = new SmtpClient("smtp_server", 25))
                {
                    //sc.EnableSsl = true;
                    sc.DeliveryMethod = SmtpDeliveryMethod.Network;
                    sc.UseDefaultCredentials = false;
                    sc.Credentials = new NetworkCredential("user", "password");





                    sc.Send(mm);
                }
            }
            */
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string conn_str = "Database=resources;Data Source=10.1.1.50;User Id=username;Password=password";
            MySqlLib.MySqlData.MySqlExecute.MyResult result = new MySqlLib.MySqlData.MySqlExecute.MyResult();
            result = MySqlLib.MySqlData.MySqlExecute.SqlScalar("SELECT gas_datetime FROM gas ORDER BY gas_datetime DESC LIMIT 0,1", conn_str);
            MySqlLib.MySqlData.MySqlExecute.MyResult result2 = new MySqlLib.MySqlData.MySqlExecute.MyResult();
            result2 = MySqlLib.MySqlData.MySqlExecute.SqlScalar("SELECT gas_v_st_s FROM gas ORDER BY gas_datetime DESC LIMIT 0,1", conn_str);
            label2.Text = result.ResultText;
            label3.Text = result2.ResultText;
            
            string[] portnames = SerialPort.GetPortNames();
            SerialPort port = new SerialPort("COM8", 19200, Parity.Even, 7, StopBits.One);
  


            byte[] data_end = { 0x01, 0x42, 0x30, 0x03, 0x71, 0x01, 0x42, 0x30, 0x03, 0x71 };
            byte[] data_next = { 0x06 };

            byte[] data1 = { 47, 63, 33, 13, 10 };
            byte[] data3 = { 0x06, 0x30, 0x36, 0x31, 0x0D, 0x0A };


            byte[] ra = { 0x01, 0x52, 0x33, 0x02 };
            byte[] etx = { 0x03 };


            string day_str;
            string month_str;
            month_str = "00";
            string year_str;
            string date_str;


            string hour_str;


            StreamWriter wr1 = new StreamWriter("gas_log.txt");
            wr1.WriteLine(DateTime.Now + ": Начало старта сбора данных с газового счетчика");



            

            MySqlLib.MySqlData.MySqlExecute.MyResult result3 = new MySqlLib.MySqlData.MySqlExecute.MyResult();
            result3 = MySqlLib.MySqlData.MySqlExecute.SqlScalar("SELECT gas_datetime FROM gas ORDER BY gas_id DESC LIMIT 0,1", conn_str);

            string date_from_mysql = result3.ResultText;

            DateTime date1 = DateTime.Now;
            DateTime date2 = DateTime.Now;

            if (date_from_mysql.Length == 19)
            {
                date2 = new DateTime(Convert.ToInt32(date_from_mysql.Substring(6, 4)), Convert.ToInt32(date_from_mysql.Substring(3, 2)), Convert.ToInt32(date_from_mysql.Substring(0, 2)), Convert.ToInt32(date_from_mysql.Substring(11, 2)), Convert.ToInt32(date_from_mysql.Substring(14, 2)), Convert.ToInt32(date_from_mysql.Substring(17, 2)));
            }
            if (date_from_mysql.Length == 18)
            {
                date2 = new DateTime(Convert.ToInt32(date_from_mysql.Substring(6, 4)), Convert.ToInt32(date_from_mysql.Substring(3, 2)), Convert.ToInt32(date_from_mysql.Substring(0, 2)), Convert.ToInt32(date_from_mysql.Substring(11, 1)), Convert.ToInt32(date_from_mysql.Substring(13, 2)), Convert.ToInt32(date_from_mysql.Substring(16, 2)));
            }

            wr1.WriteLine(DateTime.Now + ": Дата начала сбора данных - " + date2);

           // date1 = new DateTime(2015, 01, 11, 16, 0, 0);


            TimeSpan interval = date1 - date2;
            wr1.WriteLine(DateTime.Now + ": Дата конца сбора данных - " + date2.AddHours(Convert.ToInt32(Math.Floor(interval.TotalHours))));

            wr1.WriteLine(DateTime.Now + ": Количество циклов для сбора данных - " + Convert.ToInt32(Math.Floor(interval.TotalHours)));

            if (interval.TotalHours < 1)
            {
                MessageBox.Show("Данные за период запроса отсутствуют!");
                wr1.WriteLine(DateTime.Now + ": Данные за период запроса отсутствуют!");
            }
            if (interval.TotalHours >= 1)
            {

                int hours = Convert.ToInt32(Math.Floor(interval.TotalHours));

                string[] v_st_s = new string[hours];
                string[] v_r_s = new string[hours];
                string[] pressure = new string[hours];
                string[] temperature = new string[hours];
                string[] kkor = new string[hours];
                string[] sys_status = new string[hours];
                string[] status_vr = new string[hours];
                string[] status_vst = new string[hours];
                string[] status_p = new string[hours];
                string[] status_t = new string[hours];
                string[] n_sit = new string[hours];
                string[] n_sit2 = new string[hours];
                string[] crc_ok = new string[hours];
                string[] datetime = new string[hours];
                string[] datetime_mysql = new string[hours];

                port.Open();

                port.Write(data1, 0, data1.Length);
                Thread.Sleep(1500);

                int byteRecieved = port.BytesToRead;
                byte[] messByte = new byte[byteRecieved];
                port.Read(messByte, 0, byteRecieved);
                Thread.Sleep(1500);

                port.Write(data3, 0, data3.Length);
                Thread.Sleep(1500);

                int byteRecieved2 = port.BytesToRead;
                byte[] messByte2 = new byte[byteRecieved2];
                port.Read(messByte2, 0, byteRecieved2);
                Thread.Sleep(1500);


                progressBar1.Maximum = Convert.ToInt32(hours);
                int k = 0;

                for (k = 0; k <= (interval.TotalHours - 1); k++)
                {
                    date2 = date2.AddHours(1);

                    year_str = Convert.ToString(date2.Year);
                    month_str = Convert.ToString(date2.Month);
                    if (month_str.Length < 2) month_str = '0' + month_str;
                    day_str = Convert.ToString(date2.Day);
                    if (day_str.Length < 2) day_str = '0' + day_str;

                    date_str = year_str + "-" + month_str + "-" + day_str;

                    hour_str = Convert.ToString(date2.Hour);
                    if (hour_str.Length < 2) hour_str = '0' + hour_str;

                    byte[] req = data_coll(date_str, hour_str + ":00:00");

                    port.Write(req, 0, req.Length);
                    Thread.Sleep(4000);



                    int byteRecieved3 = port.BytesToRead;
                    byte[] messByte3 = new byte[byteRecieved3];
                    port.Read(messByte3, 0, byteRecieved3);
                    Thread.Sleep(1500);

                    string s1 = Encoding.ASCII.GetString(messByte3);

                    string[] words = s1.Split('(', ')');



                    v_st_s[k] = words[9];
                    v_r_s[k] = words[13];
                    pressure[k] = words[15];
                    temperature[k] = words[17];
                    kkor[k] = words[21];
                    sys_status[k] = words[23];
                    status_vr[k] = words[25];
                    status_vst[k] = words[27];
                    status_p[k] = words[29];
                    status_t[k] = words[31];
                    n_sit[k] = words[1];
                    n_sit2[k] = words[3];
                    crc_ok[k] = words[35];
                    datetime[k] = words[5];

                    progressBar1.Value = k;
                }



                port.Write(data_end, 0, data_end.Length);
                port.Close();
                wr1.WriteLine(DateTime.Now + ": Сбор данных со счетчика завершен");


                for (k = 0; k <= (interval.TotalHours - 1); k++)
                {
                    result = MySqlLib.MySqlData.MySqlExecute.SqlScalar("SELECT gas_v_r_s FROM gas ORDER BY gas_id DESC LIMIT 0,1", conn_str);
                    
                    char[] chars = v_r_s[k].ToCharArray();
                    for (int j = 0; j < v_r_s[k].Length; j++)
                    {
                        if (chars[j] == '.')
                        {
                            chars[j] = ',';
                        }
                    }
                    string v_r_s_dec = new string(chars);
                    
                   decimal v_r_p = Convert.ToDecimal(v_r_s_dec) - Convert.ToDecimal(result.ResultText);
                   string v_r_p_str = Convert.ToString(v_r_p);
                   
                   char[] chars2 = v_r_p_str.ToCharArray();
                   for (int l = 0; l < v_r_p_str.Length; l++)
                   {
                       if (chars2[l] == ',')
                       {
                           chars2[l] = '.';
                       }
                   }
                   string v_r_p_str2 = new string(chars2);

                   result = MySqlLib.MySqlData.MySqlExecute.SqlScalar("SELECT gas_v_st_s FROM gas ORDER BY gas_id DESC LIMIT 0,1", conn_str);

                   char[] chars3 = v_st_s[k].ToCharArray();
                   for (int m = 0; m < v_st_s[k].Length; m++)
                   {
                       if (chars3[m] == '.')
                       {
                           chars3[m] = ',';
                       }
                   }
                   string v_st_s_dec = new string(chars3);

                   decimal v_st_p = Convert.ToDecimal(v_st_s_dec) - Convert.ToDecimal(result.ResultText);
                   string v_st_p_str = Convert.ToString(v_st_p);

                   char[] chars4 = v_st_p_str.ToCharArray();
                   for (int n = 0; n < v_st_p_str.Length; n++)
                   {
                       if (chars4[n] == ',')
                       {
                           chars4[n] = '.';
                       }
                   }
                   string v_st_p_str2 = new string(chars4);

                   result = MySqlLib.MySqlData.MySqlExecute.SqlScalar("SELECT gas_n FROM gas ORDER BY gas_id DESC LIMIT 0,1", conn_str);

                   int gas_mark_gray = 0;
                   if ((Convert.ToInt32(n_sit[k]) - Convert.ToInt32(result.ResultText)) != 1)
                   {
                       gas_mark_gray = 1;
                   }
                   else
                   {
                       gas_mark_gray = 0;
                   }



                    datetime_mysql[k] = datetime[k].Substring(0, 4) + datetime[k].Substring(5, 2) + datetime[k].Substring(8, 2) + datetime[k].Substring(11, 2) + datetime[k].Substring(14, 2) + datetime[k].Substring(17, 2);
                    result = MySqlLib.MySqlData.MySqlExecute.SqlNoneQuery("INSERT INTO gas (`gas_n`,`gas_n2`,`gas_datetime`,`gas_v_r_s`,`gas_v_st_s`,`gas_pressure`,`gas_temperature`,`gas_kkor`,`gas_sys_status`,`gas_status_vr`,`gas_status_vst`,`gas_status_p`,`gas_status_t`,`gas_crc_ok`,`gas_v_r_p`,`gas_v_st_p`,`gas_mark_gray`) VALUES (" + n_sit[k] + "," + n_sit2[k] + "," + datetime_mysql[k] + "," + v_r_s[k] + "," + v_st_s[k] + "," + pressure[k] + "," + temperature[k] + "," + kkor[k] + "," + sys_status[k] + "," + status_vr[k] + "," + status_vst[k] + "," + status_p[k] + "," + status_t[k] + ",'" + crc_ok[k] + "','" + v_r_p_str2 + "','" + v_st_p_str2 + "','" + gas_mark_gray + "')", conn_str);
                }
                wr1.WriteLine(DateTime.Now + ": Занесение данных в базу завершено");


            }
            wr1.Close();


            using (MailMessage mm = new MailMessage("user@mailserver", "user@mailserver"))
            {
                mm.SubjectEncoding = Encoding.GetEncoding(1251);
                mm.BodyEncoding = Encoding.GetEncoding(1251);
                mm.Subject = "Отчет сбора данных по газу";
                mm.Body = "Отчет сбора данных по газу";
                mm.IsBodyHtml = false;
                mm.Attachments.Add(new Attachment("gas_log.txt"));
                using (SmtpClient sc = new SmtpClient("smtp_server", 25))
                {
                    //sc.EnableSsl = true;
                    sc.DeliveryMethod = SmtpDeliveryMethod.Network;
                    sc.UseDefaultCredentials = false;
                    sc.Credentials = new NetworkCredential("user", "password*");





                    sc.Send(mm);

                   
                 }
          }
   Application.Exit();     }

        private void button2_Click(object sender, EventArgs e)
        {
            string conn_str = "Database=resources;Data Source=10.1.1.50;User Id=user;Password=password";
            MySqlLib.MySqlData.MySqlExecute.MyResult result = new MySqlLib.MySqlData.MySqlExecute.MyResult();
            result = MySqlLib.MySqlData.MySqlExecute.SqlScalar("SELECT gas_datetime FROM gas ORDER BY gas_datetime DESC LIMIT 0,1", conn_str);
            MySqlLib.MySqlData.MySqlExecute.MyResult result2 = new MySqlLib.MySqlData.MySqlExecute.MyResult();
            result2 = MySqlLib.MySqlData.MySqlExecute.SqlScalar("SELECT gas_v_st_s FROM gas ORDER BY gas_datetime DESC LIMIT 0,1", conn_str);
            label2.Text = result.ResultText;
            label3.Text = result2.ResultText;
        }


    }
}



namespace MySqlLib
{
    /// <summary>
    /// Набор компонент для простой работы с MySQL базой данных.
    /// </summary>
    public class MySqlData
    {

        /// <summary>
        /// Методы реализующие выполнение запросов с возвращением одного параметра либо без параметров вовсе.
        /// </summary>
        public class MySqlExecute
        {

            /// <summary>
            /// Возвращаемый набор данных.
            /// </summary>
            public class MyResult
            {
                /// <summary>
                /// Возвращает результат запроса.
                /// </summary>
                public string ResultText;
                /// <summary>
                /// Возвращает True - если произошла ошибка.
                /// </summary>
                public string ErrorText;
                /// <summary>
                /// Возвращает текст ошибки.
                /// </summary>
                public bool HasError;
            }

            /// <summary>
            /// Для выполнения запросов к MySQL с возвращением 1 параметра.
            /// </summary>
            /// <param name="sql">Текст запроса к базе данных</param>
            /// <param name="connection">Строка подключения к базе данных</param>
            /// <returns>Возвращает значение при успешном выполнении запроса, текст ошибки - при ошибке.</returns>
            public static MyResult SqlScalar(string sql, string connection)
            {
                MyResult result = new MyResult();
                try
                {
                    MySql.Data.MySqlClient.MySqlConnection connRC = new MySql.Data.MySqlClient.MySqlConnection(connection);
                    MySql.Data.MySqlClient.MySqlCommand commRC = new MySql.Data.MySqlClient.MySqlCommand(sql, connRC);
                    connRC.Open();
                    try
                    {
                        result.ResultText = commRC.ExecuteScalar().ToString();
                        result.HasError = false;
                    }
                    catch (Exception ex)
                    {
                        result.ErrorText = ex.Message;
                        result.HasError = true;
                        
                    }
                    connRC.Close();
                }
                catch (Exception ex)//Этот эксепшн на случай отсутствия соединения с сервером.
                {
                    result.ErrorText = ex.Message;
                    result.HasError = true;
                }
                return result;
            }


            /// <summary>
            /// Для выполнения запросов к MySQL без возвращения параметров.
            /// </summary>
            /// <param name="sql">Текст запроса к базе данных</param>
            /// <param name="connection">Строка подключения к базе данных</param>
            /// <returns>Возвращает True - ошибка или False - выполнено успешно.</returns>
            public static MyResult SqlNoneQuery(string sql, string connection)
            {
                MyResult result = new MyResult();
                try
                {
                    MySql.Data.MySqlClient.MySqlConnection connRC = new MySql.Data.MySqlClient.MySqlConnection(connection);
                    MySql.Data.MySqlClient.MySqlCommand commRC = new MySql.Data.MySqlClient.MySqlCommand(sql, connRC);
                    connRC.Open();
                    try
                    {
                        commRC.ExecuteNonQuery();
                        result.HasError = false;
                    }
                    catch (Exception ex)
                    {
                        result.ErrorText = ex.Message;
                        result.HasError = true;
                    }
                    connRC.Close();
                }
                catch (Exception ex)//Этот эксепшн на случай отсутствия соединения с сервером.
                {
                    result.ErrorText = ex.Message;
                    result.HasError = true;
                }
                return result;
            }

        }
        /// <summary>
        /// Методы реализующие выполнение запросов с возвращением набора данных.
        /// </summary>
        public class MySqlExecuteData
        {
            /// <summary>
            /// Возвращаемый набор данных.
            /// </summary>
            public class MyResultData
            {
                /// <summary>
                /// Возвращает результат запроса.
                /// </summary>
                public DataTable ResultData;
                /// <summary>
                /// Возвращает True - если произошла ошибка.
                /// </summary>
                public string ErrorText;
                /// <summary>
                /// Возвращает текст ошибки.
                /// </summary>
                public bool HasError;
            }
            /// <summary>
            /// Выполняет запрос выборки набора строк.
            /// </summary>
            /// <param name="sql">Текст запроса к базе данных</param>
            /// <param name="connection">Строка подключения к базе данных</param>
            /// <returns>Возвращает набор строк в DataSet.</returns>
            public static MyResultData SqlReturnDataset(string sql, string connection)
            {
                MyResultData result = new MyResultData();
                try
                {
                    MySql.Data.MySqlClient.MySqlConnection connRC = new MySql.Data.MySqlClient.MySqlConnection(connection);
                    MySql.Data.MySqlClient.MySqlCommand commRC = new MySql.Data.MySqlClient.MySqlCommand(sql, connRC);
                    connRC.Open();
                    try
                    {
                        MySql.Data.MySqlClient.MySqlDataAdapter AdapterP = new MySql.Data.MySqlClient.MySqlDataAdapter();
                        AdapterP.SelectCommand = commRC;
                        DataSet ds1 = new DataSet();
                        AdapterP.Fill(ds1);
                        result.ResultData = ds1.Tables[0];
                    }
                    catch (Exception ex)
                    {
                        result.HasError = true;
                        result.ErrorText = ex.Message;
                    }
                    connRC.Close();
                }
                catch (Exception ex)//Этот эксепшн на случай отсутствия соединения с сервером.
                {
                    result.ErrorText = ex.Message;
                    result.HasError = true;
                }
                return result;
            }
        }
    }
}