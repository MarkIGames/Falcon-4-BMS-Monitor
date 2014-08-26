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
    class falconCustomSounds
    {

        public static void Start( monitorUi userDisplay )
        {

            while (true)
            {
                if (userDisplay.getActiveSound() != null)
                {
                    System.Media.SoundPlayer player = new System.Media.SoundPlayer();

                    player.SoundLocation = userDisplay.getActiveSound() + ".wav";
                    try
                    {
                        player.Play();
                    }
                    catch
                    {

                    }
                    System.Threading.Thread.Sleep(2000);
                }
            }
        }
    }
}
