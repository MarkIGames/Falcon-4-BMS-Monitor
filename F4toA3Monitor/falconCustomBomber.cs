using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Data;
using System;
using System.Diagnostics;
using System.Media;
using System.Threading;

namespace F4toA3Monitor
{
    class falconCustomBomber
    {

        public static void Start( monitorUi userDisplay )
        {
            FalconDataFormats source = new FalconDataFormats();

            Boolean soundActive = userDisplay.getSoundActive();
            string callSign     = userDisplay.getCallsign();

            Reader memReader = new Reader(source);

            Process[] processes = Process.GetProcessesByName("Falcon BMS");

            while (processes.Length < 1)
            {
                processes = Process.GetProcessesByName("Falcon BMS");

                System.Threading.Thread.Sleep(3000);
            }

            if (processes.Length > 0)
            {
                userDisplay.AppendTextBox("Starting Bomber!\r\n");
                System.Diagnostics.Process eqproc = processes[0];

                int addrbase = 0x4E985B1;
                int addrname = 0x4A2E848;

                string bombData = "";
                string bombText = "";

                DBConnect mySQLConnection = new DBConnect();

                while (true)
                {

                    if (bombData == "")
                    {
                        MemoryLoc Pmhp = new MemoryLoc(eqproc, addrbase);
                        bombData = Pmhp.getString(6, false);
                        bombText = Pmhp.getString(6, false);
                        if (bombData.Contains("GB12"))
                        {
                            userDisplay.AppendTextBox(@"Start: " + bombData + "\r\n");
                            bombData = bombData.Replace(" GB12", "");
                            userDisplay.AppendTextBox(@"Bomb Count: " + bombData + "\r\n");
                        }
                    }

                    MemoryLoc Pmhp3 = new MemoryLoc(eqproc, addrname);
                    // string nameData = Pmhp3.getString(100, false);
                    string nameData = callSign;

                    /*
                    int index = nameData.IndexOf(" at ");

                    if (index > 0)
                    {
                        nameData = nameData.Substring(0, index);
                    }
                    else
                    {
                        nameData = "notAssigned";
                    }
                    */

                    MemoryLoc Pmhp2 = new MemoryLoc(eqproc, addrbase);

                    string bombData2 = Pmhp2.getString(6, false);
                    string bombText2 = Pmhp2.getString(6, false);

                    if (bombData2.Contains("GB12"))
                    {
                        bombData2 = bombData2.Replace(" GB12", "");
                    }
                    else
                    {
                        bombData2 = "";
                    }
                    var data1 = memReader.GetCurrentData();

                    double mapratio = 30000 / ((85 * 1640) * 0.3048);

                    double xoffset = 597 * 1640;
                    double yoffset = 1402.5 * 1640;

                    double xm = (((data1.y - xoffset) * 0.3048) * mapratio);
                    double ym = (((data1.x - yoffset) * 0.3048) * mapratio);

                    double altitude = (data1.z * 0.3048) * -1;

                    if (bombData != bombData2 && bombData != "" && bombData2 != "" && bombData != "SMS")
                    {

                        string profile = userDisplay.getProfile();

                        double laserX = userDisplay.getLaserX();
                        double laserY = userDisplay.getLaserY();

                        mySQLConnection.saveBombToDatabase(laserX, laserY, 0, profile, altitude, userDisplay);

                        userDisplay.AppendTextBox("addBomb&x=" + laserX + "&y=" + laserY + "&type=1&profile=" + profile + "&altitude=" + altitude + "\r\n");

                        /*
                        string url = @"http://infernusdealtis.com/terminal/ajax.php?page=addBomb&x=" + xm + "&y=" + ym + "&type=1";

                        HttpWebRequest req2 = (HttpWebRequest)WebRequest.Create(url);
                        HttpWebResponse response = (HttpWebResponse)req2.GetResponse();

                        response.Close();
                        */

                        bombData = bombData2;
                    }
                     
                    System.Threading.Thread.Sleep(2000);
                }
            }

            while (true)
            {
                System.Threading.Thread.Sleep(3000);
            }

        }
    }
}
