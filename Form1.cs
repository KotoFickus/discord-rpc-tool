using DiscordRPC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Reflection;
using System.Xml;
using Button = System.Windows.Forms.Button;

namespace RPCtool
{
    public partial class Form1 : Form
    {
        public int clicksForEasterEgg = 0;
        public PrivateFontCollection pfc = new PrivateFontCollection();
        public DiscordRpcClient client;
        public Timer updater = new Timer();
        public bool exit = false;
        SettingsForm settingsForm;
        public Form1()
        {
            //checkAlreadyLaunched();
            InitializeComponent();
            notifyIcon1.Visible = true;
            settingsForm = new SettingsForm();
        }

        private void checkAlreadyLaunched() {
            var exists = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location)).Count() > 1;
            if (exists) {
            
            }
        }

        private void loadLang() {
            string resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lang");
            string currentlang = Properties.Settings.Default.lang;
            string langfile = Path.Combine(resourcesPath, "lang_" + currentlang + ".xml");
            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(langfile);

                XmlElement xRoot = xDoc.DocumentElement;

                Debug.WriteLine("Loading language file " + langfile);
                Utils.GetAllControls(this);
                foreach (XmlElement xnode in xRoot)
                {
                    XmlNode key = xnode.Attributes.GetNamedItem("key");
                    XmlNode value = xnode.Attributes.GetNamedItem("value");
                    foreach (Control control in Utils.controls)
                    {
                        if (control.Text == key.Value)
                        {
                            control.Text = value.Value;
                        }
                    }
                }
            }
            catch(Exception e) {
                if (MessageBox.Show("Error at loading language file, skip(might be artifacts)?", "Discord RPC tool", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.No) {
                    exit = true;
                    Application.Exit();
                }
            }
            /*
                foreach (Control control in Utils.controls) {
                Debug.WriteLine(control);
                if (control is Button || control is Label || control is GroupBox) {
                    string text;
                    if (langText.TryGetValue(control.Text, out text)) {
                        Debug.WriteLine(text);
                        control.Text = text;
                    }
                }
            }
            */
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            loadLang();
            //load custom font
            int length = Properties.Resources.Uni_Sans_Thin.Length;
            byte[] fontdata = Properties.Resources.Uni_Sans_Thin;
            System.IntPtr data = Marshal.AllocCoTaskMem(length);
            Marshal.Copy(fontdata, 0, data, length);
            pfc.AddMemoryFont(data, length);
            this.Font = new System.Drawing.Font(pfc.Families[0], 10);
            settingsForm.Font = new System.Drawing.Font(pfc.Families[0], 10);
            contextMenuStrip1.Font = new System.Drawing.Font(pfc.Families[0], 8);
            //load properties
            loadSettings();
        }

        private void loadSettings()
        {
            token.Text = Properties.Settings.Default.token;
            RPCtext.Text = Properties.Settings.Default.text;
            details.Text = Properties.Settings.Default.details;
            textBox1.Text = Properties.Settings.Default.img;
            smallImage.Text = Properties.Settings.Default.smallImage;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RPCenable();
            button2.Enabled = true;
            button1.Enabled = false;
            disableRPC.Enabled = true;
            enableRPC.Enabled = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            updater.Stop();
            try
            {
                client.Deinitialize();
            }
            catch
            {
                Debug.WriteLine("Deinitialize error, ignoring it");
            }
            button1.Enabled = true;
            button2.Enabled = false;
            disableRPC.Enabled = false;
            enableRPC.Enabled = true;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            hideMainForm.Enabled = false;
            showMainForm.Enabled = true;
            if(!exit)
                e.Cancel = true;
            /*
            try { if(client != null) client.Deinitialize(); }
            catch
            {
                Debug.WriteLine("Deinitialize error, ignoring it");
            }
            Application.Exit();
            */
        }

        private void token_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default["token"] = token.Text;
            Properties.Settings.Default.Save();
            try
            {
                if (updater.Enabled)
                {
                    RPCrestart();
                }
            }
            catch
            {
                Debug.WriteLine("Restart error, ignoring it");
            }
        }

        private void RPCtext_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default["text"] = RPCtext.Text;
            Properties.Settings.Default.Save();
            try
            {
                if (updater.Enabled)
                {
                    RPCrestart();
                }
            }
            catch { }
        }

        private void details_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default["details"] = details.Text;
            Properties.Settings.Default.Save();
            try
            {
                if (updater.Enabled)
                {
                    RPCrestart();
                }
            }
            catch { }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default["img"] = textBox1.Text;
            Properties.Settings.Default.Save();
            try
            {
                if (updater.Enabled)
                {
                    RPCrestart();
                }
            }
            catch { }
        }

        private void disableRPC_Click(object sender, EventArgs e)
        {
            try
            {
                client.Deinitialize();
            }
            catch { }
            button1.Enabled = true;
            button2.Enabled = false;
            enableRPC.Enabled = true;
            disableRPC.Enabled = false;
        }

        private void enableRPC_Click(object sender, EventArgs e)
        {
            RPCenable();
            button2.Enabled = true;
            button1.Enabled = false;
            disableRPC.Enabled = true;
            enableRPC.Enabled = false;
        }

        private void hideMainForm_Click(object sender, EventArgs e)
        {
            this.Hide();
            hideMainForm.Enabled = false;
            showMainForm.Enabled = true;
        }

        private void showMainForm_Click(object sender, EventArgs e)
        {
            this.Show();
            hideMainForm.Enabled = true;
            showMainForm.Enabled = false;
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            try
            {
                if(client != null) client.Deinitialize();
            }
            catch { }
            exit = true;
            Application.Exit();
        }

        private void smallImage_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default["smallImage"] = smallImage.Text;
            Properties.Settings.Default.Save();
            try
            {
                if (updater.Enabled)
                {
                    RPCrestart();
                }
            }
            catch { }
        }
        //hide notify icon
        private void button4_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        //start RPC
        private void RPCenable()
        {

            client = new DiscordRpcClient(token.Text);
            if (!client.Initialize())
            {
                MessageBox.Show("Can't connect to RPC", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                RichPresence rc = new RichPresence()
                {
                    Details = details.Text,
                    State = RPCtext.Text,
                    Timestamps = Timestamps.Now,
                    Assets = new Assets()
                    {
                        LargeImageKey = textBox1.Text,
                        SmallImageKey = smallImage.Text
                    }
                };
                client.SetPresence(rc);
                updater.Interval = 150;
                updater.Tick += (from, msg) =>
                {
                    //client.SynchronizeState();
                    client.Invoke();
                };
                updater.Start();
            }
        }
        //restart RPC
        private void RPCrestart()
        {
            try
            {
                updater.Stop();
                client.Deinitialize();
            }
            catch { }
            rpcUpdateTimer.Stop();
            rpcUpdateTimer.Start();
        }

        private void rpcUpdateTimer_Tick(object sender, EventArgs e)
        {
            RPCenable();
            rpcUpdateTimer.Stop();
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        public static Point GetCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            // NOTE: If you need error handling
            // bool success = GetCursorPos(out lpPoint);
            // if (!success)

            return lpPoint;
        }

        private void eastereggTimer_Tick(object sender, EventArgs e)
        {
            clicksForEasterEgg = 0;
            eastereggTimer.Stop();
        }

        string[] langs = {
            "en",
            "ru",
            "ua"
        };

        private void button3_Click(object sender, EventArgs e)
        {

            if (settingsForm.ShowDialog() == DialogResult.OK) {
                Properties.Settings.Default["lang"] = langs[settingsForm.domainUpDown1.SelectedIndex];
                Properties.Settings.Default.Save();
                Application.Restart();
                Environment.Exit(0);
            }
        }
    }
}
