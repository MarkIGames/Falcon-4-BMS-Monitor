using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace F4toA3Monitor
{
    public partial class monitorUi : Form
    {
        private Thread  falconLocatorThread;
        private Thread  falconSoundThread;
        private Thread  falconBomberThread;

        private Boolean audioActive   = false;
        private Boolean locatorActive = false;
        private Boolean bomberActive  = false;
        private Boolean callsignSet   = false;

        private string activeSound    = null;
        private string callSign       = "notAssigned";
        private string laserCode1     = null;
        private string laserCode2     = null;

        private double laserx         = 0;
        private double lasery         = 0;

        private string profile        = "";

        public monitorUi()
        {
            InitializeComponent();

            readConfigFile();

            this.TopMost = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            disableInterfaceButtons();

            this.spawnLocatorThread();

            this.spawnAudioThread();

            this.spawnBomberThread();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            disableInterfaceButtons();

            this.spawnLocatorThread();

            this.spawnBomberThread();

            this.spawnAudioThread();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.abortLocatorThread();

            this.abortAudioThread();

            this.abortBomberThread();

            enableInterfaceButtons();

            DBConnect monitorConnection = new DBConnect();

            monitorConnection.deactivateUserInDatabase( this );
        }

        public void AppendTextBox(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendTextBox), new object[] { value });
                return;
            }
            textBox1.AppendText( value );
        }

        public void updateTimer(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(updateTimer), new object[] { value });
                return;
            }
            label6.Text = value;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (this.audioActive == false)
            {
                this.audioActive = true;

                label4.Text = "Audio Enabled";
            }
            else
            {
                this.audioActive = false;

                label4.Text = "Audio Disabled";
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (textBox2.Text != "" && textBox2.Text != null && textBox2.Text.Length > 3)
            {
                this.callSign = textBox2.Text;

                this.enableUpdateButtons();
            }
            else
            {
                MessageBox.Show("Callsign invalid!", "F4 to A3 Monitor", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk);
            }
            if (textBox3.Text != "" && textBox3.Text != null && textBox3.Text.Length == 5)
            {
                this.laserCode1 = textBox3.Text;

                this.enableUpdateButtons();
            }
            else
            {
                MessageBox.Show("Laser Code 1 invalid!", "F4 to A3 Monitor", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk);
            }
            if (textBox4.Text != "" && textBox4.Text != null && textBox4.Text.Length == 5)
            {
                this.laserCode2 = textBox4.Text;

                this.enableUpdateButtons();
            }
            else
            {
                MessageBox.Show("Laser Code 2 invalid!", "F4 to A3 Monitor", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk);
            }

            saveConfigDetails();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (this.bomberActive == false)
            {
                if (this.textBox3.TextLength == 5 || this.textBox4.TextLength == 5)
                {
                    this.laserCode1 = this.textBox3.Text;
                    this.laserCode2 = this.textBox4.Text;

                    this.bomberActive = true;

                    label10.Text = "Bomber Enabled";
                }
                else
                {
                    MessageBox.Show("Laser Codes invalid!", "F4 to A3 Monitor", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk);

                    this.bomberActive = false;

                    label10.Text = "Bomber Disabled";
                }
            }
            else
            {
                this.bomberActive = false;

                label10.Text = "Bomber Disabled";
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (this.locatorActive == false)
            {
                this.locatorActive = true;

                label11.Text = "Locator Enabled";
            }
            else
            {
                this.locatorActive = false;

                label11.Text = "Locator Disabled";
            }
        }

        public Boolean getSoundActive()
        {
            return this.audioActive;
        }

        public void setActiveSound( string activeSound )
        {
            this.activeSound = activeSound;
        }

        public void setProfile(string profile)
        {
            this.profile = profile;
        }

        public string getCallsign()
        {
            return this.callSign;
        }

        public void setLaserX(double laserx)
        {
            this.laserx = laserx;
        }

        public double getLaserX()
        {
            return this.laserx ;
        }

        public void setLaserY(double lasery)
        {
            this.lasery = lasery;
        }

        public double getLaserY()
        {
            return this.lasery;
        }

        public string getProfile()
        {
            return this.profile;
        }

        public string getActiveSound()
        {
            return this.activeSound;
        }

        private void disableUpdateButtons()
        {
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
        }

        private void disableInterfaceButtons()
        {
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            button7.Enabled = false;
        }

        private void enableInterfaceButtons()
        {
            button4.Enabled = true;
            button5.Enabled = true;
            button6.Enabled = true;
            button7.Enabled = true;
        }

        private void enableUpdateButtons()
        {
            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
        }

        private void spawnAudioThread()
        {
            if (this.audioActive == true)
            {
                this.abortAudioThread();

                this.falconSoundThread = new System.Threading.Thread(delegate()
                {
                    falconCustomSounds.Start(this);
                });

                this.falconSoundThread.Start();

                AppendTextBox("Sounder Started!\r\n");
            }
        }

        private void spawnLocatorThread()
        {
            if (this.locatorActive == true)
            {
                this.abortLocatorThread();

                this.falconLocatorThread = new System.Threading.Thread(delegate()
                {
                    falconCustomLocator.Start(this);
                });

                this.falconLocatorThread.Start();

                AppendTextBox("Monitor Started!\r\n");
            }
        }

        private Boolean spawnBomberThread()
        {
            if (this.bomberActive == true)
            {
                if (this.textBox3.TextLength != 5 || this.textBox4.TextLength != 5)
                {
                    MessageBox.Show("Laser Codes invalid!", "F4 to A3 Monitor", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk);

                    return false;
                }

                this.abortBomberThread();

                this.falconBomberThread = new System.Threading.Thread(delegate()
                {
                    falconCustomBomber.Start(this);
                });

                this.falconBomberThread.Start();

                AppendTextBox("Bomber Started!\r\n");

                return true;
            }

            return false;
        }

        private void abortAudioThread()
        {
            if (this.falconSoundThread != null && this.falconSoundThread.IsAlive)
            {
                this.falconSoundThread.Abort();

                AppendTextBox("Sounder Aborted!\r\n");
            }
        }

        private void abortLocatorThread()
        {
            if (this.falconLocatorThread != null && this.falconLocatorThread.IsAlive)
            {
                this.falconLocatorThread.Abort();

                AppendTextBox("Monitor Aborted!\r\n");
            }
        }

        private void abortBomberThread()
        {
            if (this.falconBomberThread != null && this.falconBomberThread.IsAlive)
            {
                this.falconBomberThread.Abort();

                AppendTextBox("Bomber Aborted!\r\n");
            }
        }

        public string[] getLaserCodes()
        {
            string[] laserCodes = new string[2] { this.laserCode1, this.laserCode2 };

            return laserCodes;
        }

        private void readConfigFile() {
            string line;

            var namesearchtext    = "user=";
            var code1searchtext   = "code1=";
            var code2searchtext   = "code2=";
            var locatorsearchtext = "locator=on";
            var bombersearchtext  = "bomber=on";
            var audiosearchtext   = "audio=on";

            string configFile = System.IO.Directory.GetCurrentDirectory() + @"\config.ini";

            if (!(File.Exists(configFile)))
            {
                File.Create(configFile).Dispose();
            }
            else
            {
                System.IO.StreamReader file = new System.IO.StreamReader(configFile);

                while ((line = file.ReadLine()) != null)
                {
                    if (line.Contains(namesearchtext))
                    {
                        line = line.Replace(namesearchtext, "");

                        this.callSign = line;

                        this.textBox2.AppendText(line);
                    }
                    if (line.Contains(code1searchtext))
                    {
                        line = line.Replace(code1searchtext, "");

                        this.laserCode1 = line;

                        this.textBox3.AppendText(line);
                    }
                    if (line.Contains(code2searchtext))
                    {
                        line = line.Replace(code2searchtext, "");

                        this.laserCode2 = line;

                        this.textBox4.AppendText(line);
                    }
                    if (line.Contains(audiosearchtext))
                    {
                        this.audioActive = true;

                        label4.Text = "Audio Enabled";
                    }
                    if (line.Contains(locatorsearchtext))
                    {
                        this.locatorActive = true;

                        label11.Text = "Locator Enabled";
                    }
                    if (line.Contains(bombersearchtext))
                    {
                        this.bomberActive = true;

                        label10.Text = "Bomber Enabled";
                    }
                }

                file.Close();
            }
        }

        private void saveConfigDetails()
        {
            string configFile = System.IO.Directory.GetCurrentDirectory() + @"\config.ini";

            if (File.Exists(configFile))
            {
                File.Delete(configFile);
            }

            File.Create(configFile).Dispose();

            string newLine = "\r\n";

            string lines = "";

            lines = lines + "user=" + this.callSign;

            lines = lines + newLine;

            lines = lines + "code1=" + this.laserCode1;

            lines = lines + newLine;

            lines = lines + "code2=" + this.laserCode2;

            lines = lines + newLine;

            if (this.audioActive == true)
            {
                lines = lines + "audio=on";

                lines = lines + newLine;
            }
            else
            {
                lines = lines + "audio=off";

                lines = lines + newLine;
            }

            if (this.bomberActive == true)
            {
                lines = lines + "bomber=on";

                lines = lines + newLine;
            }
            else
            {
                lines = lines + "bomber=off";

                lines = lines + newLine;
            }

            if (this.locatorActive == true)
            {
                lines = lines + "locator=on";

                lines = lines + newLine;
            }
            else
            {
                lines = lines + "locator=off";

                lines = lines + newLine;
            }

            System.IO.StreamWriter file = new System.IO.StreamWriter(configFile);

            file.WriteLine(lines);

            file.Close();
        }
    }
}
