using System.Collections.Generic;
using System.Threading;
using System.IO;
using System;
using TrueGearSDK;


namespace MyTrueGear
{
    public class TrueGearMod
    {
        private static TrueGearPlayer _player = null;

        private static ManualResetEvent heartbeatMRE = new ManualResetEvent(false);
        private static ManualResetEvent leftcrushcrystalMRE = new ManualResetEvent(false);
        private static ManualResetEvent rightcrushcrystalMRE = new ManualResetEvent(false);

        private static int leftCrushCount = 1;
        private static int rightCrushCount = 1;

        public void HeartBeat()
        {
            while(true)
            {
                heartbeatMRE.WaitOne();
                _player.SendPlay("HeartBeat");
                Thread.Sleep(600);
            }            
        }

        public void LeftCrushCrystal()
        {
            while (true)
            {
                leftcrushcrystalMRE.WaitOne();
                //MelonLogger.Msg($"LeftCrushCrystal{leftCrushCount}");
                _player.SendPlay("LeftHandCrushCrystal");
                //leftCrushCount++;
                Thread.Sleep(120);
            }
        }

        public void RightCrushCrystal()
        {
            while (true)
            {
                rightcrushcrystalMRE.WaitOne();
                //MelonLogger.Msg($"RightCrushCrystal{rightCrushCount}");
                _player.SendPlay("RightHandCrushCrystal");
                //rightCrushCount++;
                Thread.Sleep(120);
            }
        }


        public TrueGearMod() 
        {
            _player = new TrueGearPlayer("858260","UntilYouFall");
            _player.Start();
            new Thread(new ThreadStart(this.HeartBeat)).Start();
            new Thread(new ThreadStart(this.LeftCrushCrystal)).Start();
            new Thread(new ThreadStart(this.RightCrushCrystal)).Start();
        }    


        public void Play(string Event)
        { 
            _player.SendPlay(Event);
        }

        public void PlayDir(string tmpEvent, float dir)
        {
            dir = Math.Abs(dir);
            if (dir > 180)
            {
                dir = dir - 180;
            }
            string shockEvent = "";
            if (dir > 0 && dir < 90)
            {
                shockEvent = tmpEvent + "2";
            }
            else if (dir == 90)
            {
                shockEvent = tmpEvent + "3";
            }
            else if (dir > 90 && dir < 180)
            {
                shockEvent = tmpEvent + "4";
            }
            else
            {
                shockEvent = tmpEvent + "1";
            }
            _player.SendPlay(shockEvent);
        }

        public void StartHeartBeat()
        {
            heartbeatMRE.Set();
        }

        public void StopHeartBeat()
        {
            heartbeatMRE.Reset();
        }

        public void StartLeftCrushCrystal()
        {
            //leftCrushCount = 1;
            leftcrushcrystalMRE.Set();
        }

        public void StopLeftCrushCrystal()
        {
            //leftCrushCount = 1;
            leftcrushcrystalMRE.Reset();
        }

        public void StartRightCrushCrystal()
        {
            //rightCrushCount = 1;
            rightcrushcrystalMRE.Set();
        }

        public void StopRightCrushCrystal()
        {
            //rightCrushCount = 1;
            rightcrushcrystalMRE.Reset();
        }

    }
}
