using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace HostEditor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CreateGroupBox();
        }

        private static string[] GetConfigKeys()
        {
            if (ConfigurationSettings.AppSettings != null)
            {
                string[] keys = ConfigurationSettings.AppSettings.AllKeys;
                return keys;
            }
            return null;
        }

        private static string[] GetConfigValuesByKey(string key)
        {
            string[] result = new string[] { };
            AppSettingsReader appRead = new AppSettingsReader();
            object value = appRead.GetValue(key, typeof(string));
            if (value != null)
            {
                string values = (string)value;
                if (values.IndexOf(",") != -1)
                {
                    string[] str = values.Split(',');
                    result = str;
                }
                else
                {
                    result = new string[] { values };
                }
            }
            return result;
        }

        private void CreateGroupBox()
        {
            const int gbHeight = 40;
            string[] keys = GetConfigKeys();
            for (int i = 0; i < keys.Length; i++)
            {
                GroupBox groubox = new GroupBox { Name = "groupbox" + i, Text = keys[i] };
                string[] values = GetConfigValuesByKey(keys[i]);
                int y = 0;
                for (int j = 0; j < values.Length; j++)
                {
                    y += 30;
                    CheckBox cb = CreateCheckBox(150, gbHeight, new Point(j * 150 + 50, 20), values[j], values[j] + " " + keys[i]);
                    groubox.Controls.Add(cb);
                }
                panelGroupBoxes.Controls.Add(groubox);
                panelGroupBoxes.Controls["groupbox" + i].Height = gbHeight + 50;
                if (i == 0)
                {
                    panelGroupBoxes.Controls["groupbox" + i].Location = new Point(40, this.panelGroupBoxes.Controls["groupbox" + i].Height * (i + i) + 20);
                    panelGroupBoxes.Controls["groupbox" + i].Width = panelGroupBoxes.Width - 80;
                }
                else
                {
                    panelGroupBoxes.Controls["groupbox" + i].Location = new Point(40, this.panelGroupBoxes.Controls["groupbox" + (i - 1)].Location.Y + this.panelGroupBoxes.Controls["groupbox" + (i - 1)].Height + 30);
                    panelGroupBoxes.Controls["groupbox" + i].Width = panelGroupBoxes.Width - 80;
                }
            }
        }

        private static CheckBox CreateCheckBox(int width, int height, Point point, string text, string tag)
        {
            CheckBox cb = new CheckBox();
            cb.CheckedChanged += (sender, args) =>
            {
                if (sender != null)
                {
                    CheckBox scb = sender as CheckBox;
                    if (scb != null && scb.Checked)
                    {
                        foreach (CheckBox control in scb.Parent.Controls)
                        {
                            if (control != sender)
                            {
                                control.Checked = false;
                            }
                        }
                    }
                }
            };
            cb.Location = point;
            cb.Text = text;
            cb.Height = height;
            cb.Width = width;
            cb.Tag = tag;
            return cb;
        }

        private void Form1_ClientSizeChanged(object sender, EventArgs e)
        {
            foreach (Control control in panelGroupBoxes.Controls)
            {
                if (control is GroupBox)
                {
                    int formWidth = panelGroupBoxes.Width;
                    control.Width = formWidth - (control.Location.X * 2);
                }
            }
        }

        private void panel3_MouseClick(object sender, MouseEventArgs e)
        {
            //获取光标位置
            Point mousePoint = new Point(e.X, e.Y);
            //换算成相对本窗体的位置
            mousePoint.Offset(Location.X, this.Location.Y);
            //判断是否在panel内
            if (panelGroupBoxes.RectangleToScreen(panelGroupBoxes.DisplayRectangle).Contains(mousePoint))
            {
                //滚动
                panelGroupBoxes.AutoScrollPosition = new Point(0, panelGroupBoxes.VerticalScroll.Value - e.Delta);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string constr = ConfigurationManager.ConnectionStrings["hostpath"].ConnectionString; //host文件路径
            string constrCommonHost = ConfigurationManager.ConnectionStrings["commonhost"].ConnectionString; //常用host路径
            string stredit = "";
            string str = "";
            int count = 0;


            if (!string.IsNullOrEmpty(constrCommonHost))
            {
                constrCommonHost = constrCommonHost.Trim();
                if (constrCommonHost.IndexOf(',') != -1)
                {
                    string[] strings = constrCommonHost.Split(',');
                    foreach (string s in strings)
                    {
                        str += (s + "\r\n");
                    }
                }
                else
                {
                    str += (constrCommonHost.Trim() + "\r\n");
                }
                str += "####################\r\n";
            }

            foreach (Control control in panelGroupBoxes.Controls)
            {
                if (control is GroupBox)
                {
                    foreach (Control controlcb in control.Controls)
                    {
                        if (controlcb is CheckBox)
                        {
                            CheckBox cb = controlcb as CheckBox;
                            if (cb.Checked)
                            {
                                count++;
                                str += cb.Tag + "\r\n\r\n";
                                stredit += cb.Tag + "\r\n\r\n";
                            }
                        }
                    }
                }
            }
            if (count > 0)
            {
                DialogResult dr = MessageBox.Show("Host文件将会被以下内容覆盖：\r\n\r\n" + stredit, "操作确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    try
                    {

                        if (!string.IsNullOrEmpty(constr))
                        {
                            File.WriteAllText(constr, str, Encoding.ASCII);
                            MessageBox.Show("修改成功，重启浏览器生效", "操作提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("修改失败，未能读取到host文件路径信息", "操作提示");
                        }
                    }
                    catch (Exception exx)
                    {
                        MessageBox.Show("修改失败", "操作提示");
                    }

                }
            }
            else
            {
                DialogResult dialogResult = MessageBox.Show("未检测到需要修改的信息，点击确定会清空当前host信息", "操作提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (dialogResult == DialogResult.OK)
                {
                    File.WriteAllText(constr, str, Encoding.ASCII);
                    MessageBox.Show("清空成功，重启浏览器生效", "操作提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string constr = ConfigurationManager.ConnectionStrings["hostpath"].ConnectionString;
            if (!string.IsNullOrEmpty(constr))
            {
                string readAllText = File.ReadAllText(constr, Encoding.ASCII);
                if (!string.IsNullOrEmpty(readAllText) && !string.IsNullOrEmpty(readAllText.Replace("\r\n", "").Replace(" ", "")))
                {
                    MessageBox.Show(readAllText, "本地host信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Host文件为空", "本地host信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

        }
    }
}
