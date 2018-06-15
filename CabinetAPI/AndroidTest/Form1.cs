using CabinetAPI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AndroidTest
{
    public partial class Form1 : Form
    {
        //正常开门 = 1,
        //密码错误 = 2,
        //正常关门 = 3,
        //非工作时间开门 = 4,
        //非工作时间关门 = 5,
        //外部电源断开 = 6,
        //备份电源电压低 = 7,
        //未按规定关门 = 8,
        //强烈震动 = 9,
        //网络断开 = 10, 
        //请求语音 = 15,
        //结束语音 = 16,
        public Form1()
        {
            InitializeComponent();

        }

        private string server = "";
        private string mac = "";
        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Text = server = "47.98.230.218:8080";
            textBox2.Text = mac = "你的mac地址，可以模拟一个";

        }
        private string Command(CommandRequest cmd)
        {
            try
            {
                cmd.Mac = mac;
                WebClient client = new WebClient();
                client.Encoding = System.Text.Encoding.UTF8;
                client.Headers.Add(HttpRequestHeader.Accept, "json");
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json; charset=UTF-8");

                var result = JsonConvert.SerializeObject(cmd);
                return client.UploadString("http://" + server + "/api/android/Command", result);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        private void Heart()
        {
            CommandRequest cmd = new CommandRequest
            {
                OperatorType = 17,
            };
            for (;;)
            {
                string result = Command(cmd);
                this.Invoke(new Action(delegate
                {
                    richTextBox1.AppendText(17 + ":" + result + Environment.NewLine);
                }));
                Thread.Sleep(20000);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var no = comboBox1.SelectedItem.ToString().Split('=')[1].Trim();
            CommandRequest cmd = new CommandRequest
            {
                OperatorType = int.Parse(no),
                Method = 0,
            };
            string result = Command(cmd);
            richTextBox1.AppendText(no + ":" + result + Environment.NewLine);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            server = textBox1.Text;
            mac = textBox2.Text ;
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            button1.Enabled = false;
            button3.Enabled = true;
            Thread th = new Thread(Heart);
            th.IsBackground = true;
            th.Start();
        }
    }
}
