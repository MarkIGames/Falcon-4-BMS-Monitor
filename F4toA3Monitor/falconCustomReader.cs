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
    class falconCustomLocator
    {

        public static void Start( monitorUi userDisplay )
        {
            FalconDataFormats source = new FalconDataFormats();

            Boolean soundActive = userDisplay.getSoundActive();
            string callSign     = userDisplay.getCallsign();

            Reader memReader = new Reader(source);

            Process[] processes = Process.GetProcessesByName("Falcon BMS");

            int seconds = 0;
            int minutes = 0;
            int hours   = 0;

            userDisplay.AppendTextBox("Searching for Falcon...\r\n");

            while (processes.Length < 1)
            {
                processes = Process.GetProcessesByName("Falcon BMS");

                System.Threading.Thread.Sleep(3000);
            }

            if (processes.Length > 0)
            {
                userDisplay.AppendTextBox("Falcon Found!\r\n");

                System.Diagnostics.Process eqproc = processes[0];

                int addrname = 0x4A2E848;

                DBConnect mySQLConnection = new DBConnect();

                mySQLConnection.deactivateUserInDatabase( userDisplay );

                mySQLConnection.saveUserToDatabase(0, 0, 0, callSign, userDisplay);

                while (true)
                {

                    MemoryLoc Pmhp3 = new MemoryLoc(eqproc, addrname);
                    // string nameData = Pmhp3.getString(100, false);
                    string nameData = callSign;

                    var data1 = memReader.GetCurrentData();

                    double mapratio = 30000 / ((85 * 1640) * 0.3048);

                    double xoffset = 597 * 1640;
                    double yoffset = 1402.5 * 1640;

                    double xm = (((data1.y - xoffset) * 0.3048) * mapratio);
                    double ym = (((data1.x - yoffset) * 0.3048) * mapratio);

                    string[] laserData = mySQLConnection.getBombData(userDisplay);

                    string profile = laserData[4];

                    double laserX = Convert.ToDouble(laserData[1]);
                    double laserY = Convert.ToDouble(laserData[2]);

                    double dX = xm - laserX;
                    double dY = ym - laserY;

                    double diff = Math.Sqrt(dX * dX + dY * dY);

                    bool testDistance  = diff < 3000;
                    bool testDistance2 = diff < 1600;

                    string[] laserCodes = userDisplay.getLaserCodes();
                    
                    bool code1match = false;
                    bool code2match = false;


                    if (laserData[0] != null)
                    {
                        code1match = (laserData[0].ToString() == laserCodes[0]);
                        code2match = (laserData[0].ToString() == laserCodes[1]);
                    }


                    if (soundActive == true && testDistance && (code1match || code2match))
                    {
                        userDisplay.setActiveSound("intervalBeep");
                        userDisplay.setProfile( "" );

                        userDisplay.setLaserX(0);
                        userDisplay.setLaserY(0);
                    }

                    if (soundActive == true && testDistance2 && (code1match || code2match))
                    {
                        userDisplay.setLaserX(laserX);
                        userDisplay.setLaserY(laserY);

                        userDisplay.setProfile(profile);
                        userDisplay.setActiveSound("solidTone");
                    }

                    if (!testDistance2 && !testDistance)
                    {
                        userDisplay.setActiveSound( null );
                        userDisplay.setProfile( "" );

                        userDisplay.setLaserX( 0 );
                        userDisplay.setLaserY( 0 );
                    }

                    double altitude = data1.z * -1;

                    userDisplay.AppendTextBox("addUnit&position=[" + xm + "," + ym + "," + altitude + "]&name=" + nameData + "&active=true\r\n");

                    mySQLConnection.updateUserInDatabase(xm, ym, altitude, nameData, userDisplay);

                    /*
                    string url2 = @"http://infernusdealtis.com/terminal/update.php?page=addUnit&position=[" + xm + "," + ym + "," + data1.z + "]&name=" + nameData + "&active=true";

                    HttpWebRequest req3 = (HttpWebRequest)WebRequest.Create(url2);
                    HttpWebResponse response3 = (HttpWebResponse)req3.GetResponse();

                    response3.Close();
                    */
                     
                    System.Threading.Thread.Sleep(2000);

                    seconds = seconds + 2;

                    string secondsText = seconds.ToString();

                    if (seconds < 10)
                    {
                        secondsText = "0" + seconds;
                    }

                    if (seconds == 60)
                    {
                        seconds = 0;
                        secondsText = "00";
                        minutes = minutes + 1;
                    }
                    if (minutes == 60)
                    {
                        hours = hours + 1;
                    }

                    string minutesText = minutes.ToString();

                    if (minutes < 10)
                    {
                        minutesText = "0" + minutes;
                    }

                    userDisplay.updateTimer(hours + " : " + minutesText + " : " + secondsText);
                }
            }

            while (true)
            {
                System.Threading.Thread.Sleep(3000);
            }

        }
    }
}
