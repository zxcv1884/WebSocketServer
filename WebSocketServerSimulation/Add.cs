using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace WebSocketServerSimulation
{
    class Add : WebSocketBehavior
    {
        static Timer RunTimer;

        private string _name;
        private static int _number = 0;
        private string _prefix;
        private static int status=0;
        private static int peptide=0;
        private static int tubes=0;
        private static int tubeNum=0;
        private static double time = 0;
        private static double pumpA=0;
        private static double pumpB=0;
        private static double pumpC=0;
        private static double pumpD=0;
        private static double pumpAspacing=0;
        private static double pumpBspacing=0;
        private static double pumpCspacing=0;
        private static double pumpDspacing=0;
        private static double pumpAml=0;
        private static double pumpBml=0;
        private static double pumpCml=0;
        private static double pumpDml=0;
        private static double pressure;
        private static double wavelength;
        private static double au;
        private static double waste=0;
        private static double holding=0;
        private static int purificationCounter=0;
        private static int washcycleCounter=0;
        private static int washcycleTemp = 0;
        private static double tubeml;
        private static double nowtubeml;
        public class Purification
        {
            public int TimeStart { get; set; }
            public int TimeEnd { get; set; }
            public int PumpAStart { get; set; }
            public int PumpAEnd { get; set; }
            public int PumpBStart { get; set; }
            public int PumpBEnd { get; set; }
            public int PumpCStart { get; set; }
            public int PumpCEnd { get; set; }
            public int PumpDStart { get; set; }
            public int PumpDEnd { get; set; }
            public int FlowRate { get; set; }
            public int FlowDestination { get; set; }
        }
        public class WashCycle
        {
            public int TimeStart { get; set; }
            public int TimeEnd { get; set; }
            public int PumpAStart { get; set; }
            public int PumpAEnd { get; set; }
            public int PumpBStart { get; set; }
            public int PumpBEnd { get; set; }
            public int PumpCStart { get; set; }
            public int PumpCEnd { get; set; }
            public int PumpDStart { get; set; }
            public int PumpDEnd { get; set; }
            public int FlowRate { get; set; }
            public int FlowDestination { get; set; }
        }
        List<Purification> purification = new List<Purification>();
        List<WashCycle> washcycle = new List<WashCycle>();
        public Add()
            //:this(null)
        {

        }
        //public Add(string prefix)
        //{
        //    _prefix = !prefix.IsNullOrEmpty() ? prefix : "anon#";
        //}
        private string getName()
        {
            var name = Context.User.Identity.Name;
            // HelloWorld(name);
            return !name.IsNullOrEmpty() ? name : _prefix + getNumber();
        }
        private static int getNumber()
        {
            return Interlocked.Increment(ref _number);
        }
        protected override void OnOpen()
        {
            //_name = getName();
            //Send(JsonConvert.SerializeObject(new { code = 200, msg = "result is " }));
            //Console.WriteLine(_name +"Connection Open");
            base.OnOpen();
        }
        protected override void OnMessage(MessageEventArgs e)
        {
            var data = e.Data;
            if (TestJson(data))
            {
                var param = JToken.Parse(data);
                if (Convert.ToInt32(param["Status"]) == 0 && status == 1)
                {
                    var Status = param["Status"].ToObject<int>();
                    var Peptide = param["Peptide"].ToObject<int>();
                    var Tubes = param["Tubes"].ToObject<int>();
                    var TubeNum = param["TubeNum"].ToObject<int>();
                    var TubeML = param["TubeML"].ToObject<int>();
                    tubeml = TubeML;
                    status = Status;
                    peptide = Peptide;
                    tubes = Tubes;
                    tubeNum = TubeNum;
                    RunTimer = new Timer(Run, null, 0, 100);

                    purification = param["Purification"].ToObject<List<Purification>>();
                    washcycle = param["WashCycle"].ToObject<List<WashCycle>>();
                }
                else if (Convert.ToInt32(param["Status"]) == 0)
                {
                    var Status = param["Status"].ToObject<int>();
                    var Peptide = param["Peptide"].ToObject<int>();
                    var Tubes = param["Tubes"].ToObject<int>();
                    var TubeNum = param["TubeNum"].ToObject<int>();
                    var TubeML = param["TubeML"].ToObject<int>();
                    purification = param["Purification"].ToObject<List<Purification>>();
                    washcycle = param["WashCycle"].ToObject<List<WashCycle>>();
                    status = 0;
                    time = 0;
                    pumpA = 0;
                    pumpB = 0;
                    pumpC = 0;
                    pumpD = 0;
                    waste = 0;
                    holding = 0;
                    tubeml = TubeML;
                    status = Status;
                    peptide = Peptide;
                    tubes = Tubes;
                    tubeNum = TubeNum;
                    RunTimer = new Timer(Run, null, 0, 100);
                }
            }
            else if(data == "stop")
            {
                RunTimer.Dispose();
                status = 2;
                time = 0;
                pumpA = 0;
                pumpB = 0;
                pumpC = 0;
                pumpD = 0;
                waste = 0;
                holding = 0;
                purificationCounter = 0;
                washcycleCounter = 0;
            }
            else if(data == "pause")
            {
                RunTimer.Dispose();
                status = 1;
            }
            else
            {
                var root = new
                {
                    status,
                    peptide,
                    tubeNum,
                    time,
                    pumpA,
                    pumpB,
                    pumpC,
                    pumpD,
                    pumpAml,
                    pumpBml,
                    pumpCml,
                    pumpDml,
                    pressure,
                    waste,
                    holding,
                    au,
                    wavelength,
                };
                Send(JsonConvert.SerializeObject(root));

            }
        }
        private void Run(Object o)
        {
            if (purificationCounter <= purification.Count - 1)
            {
                if (time <= purification[purificationCounter].TimeStart)
                {
                    pumpA = purification[purificationCounter].PumpAStart;
                    pumpB = purification[purificationCounter].PumpBStart;
                    pumpC = purification[purificationCounter].PumpCStart;
                    pumpD = purification[purificationCounter].PumpDStart;
                }
                pumpAspacing = (double)(purification[purificationCounter].PumpAEnd - purification[purificationCounter].PumpAStart) / (purification[purificationCounter].TimeEnd - purification[purificationCounter].TimeStart) / 60 / 10;
                pumpBspacing = (double)(purification[purificationCounter].PumpBEnd - purification[purificationCounter].PumpBStart) / (purification[purificationCounter].TimeEnd - purification[purificationCounter].TimeStart) / 60 / 10;
                pumpCspacing = (double)(purification[purificationCounter].PumpCEnd - purification[purificationCounter].PumpCStart) / (purification[purificationCounter].TimeEnd - purification[purificationCounter].TimeStart) / 60 / 10;
                pumpDspacing = (double)(purification[purificationCounter].PumpDEnd - purification[purificationCounter].PumpDStart) / (purification[purificationCounter].TimeEnd - purification[purificationCounter].TimeStart) / 60 / 10;
                

                pumpA += pumpAspacing;
                pumpB += pumpBspacing;
                pumpC += pumpCspacing;
                pumpD += pumpDspacing;
                if (purification[purificationCounter].FlowDestination == 1)
                {
                    waste += (double)purification[purificationCounter].FlowRate / 600;
                    pumpAml = (double)pumpA * purification[purificationCounter].FlowRate * 0.01;
                    pumpBml = (double)pumpB * purification[purificationCounter].FlowRate * 0.01;
                    pumpCml = (double)pumpC * purification[purificationCounter].FlowRate * 0.01;
                    pumpDml = (double)pumpD * purification[purificationCounter].FlowRate * 0.01;
                }
                else if (purification[purificationCounter].FlowDestination == 2)
                {
                    holding += (double)purification[purificationCounter].FlowRate / 600;
                    pumpAml = (double)pumpA * purification[purificationCounter].FlowRate * 0.01;
                    pumpBml = (double)pumpB * purification[purificationCounter].FlowRate * 0.01;
                    pumpCml = (double)pumpC * purification[purificationCounter].FlowRate * 0.01;
                    pumpDml = (double)pumpD * purification[purificationCounter].FlowRate * 0.01;
                }
                else
                {
                    if (tubeNum == -1)
                    {
                        tubeNum = 0;
                    }
                    nowtubeml += (double)purification[purificationCounter].FlowRate / 600;
                    if (nowtubeml >= tubeml)
                    {
                        tubeNum++;
                        nowtubeml = 0;
                    }
                    pumpAml = (double)pumpA * purification[purificationCounter].FlowRate * 0.01;
                    pumpBml = (double)pumpB * purification[purificationCounter].FlowRate * 0.01;
                    pumpCml = (double)pumpC * purification[purificationCounter].FlowRate * 0.01;
                    pumpDml = (double)pumpD * purification[purificationCounter].FlowRate * 0.01;

                }
                Random random = new Random();
                pressure = random.Next(20, 50);
                au = random.Next(0, 100);
                wavelength = random.Next(0, 100);

                time += 0.0016666666666667;
                if (time >= purification[purificationCounter].TimeEnd)
                {
                    purificationCounter++;
                    pumpA = purification[purificationCounter].PumpAStart;
                    pumpB = purification[purificationCounter].PumpBStart;
                    pumpC = purification[purificationCounter].PumpCStart;
                    pumpD = purification[purificationCounter].PumpDStart;
                }
            }
            else if (washcycleCounter <= washcycle.Count - 1)
            {
                if (washcycleTemp == 0)
                {
                    time = 0;
                }
                washcycleTemp++;
                if (time <= washcycle[washcycleCounter].TimeStart)
                {
                    pumpA = washcycle[washcycleCounter].PumpAStart;
                    pumpB = washcycle[washcycleCounter].PumpBStart;
                    pumpC = washcycle[washcycleCounter].PumpCStart;
                    pumpD = washcycle[washcycleCounter].PumpDStart;
                }
                pumpAspacing = (double)(washcycle[washcycleCounter].PumpAEnd - washcycle[washcycleCounter].PumpAStart) / (washcycle[washcycleCounter].TimeEnd - washcycle[washcycleCounter].TimeStart) / 60 / 10;
                pumpBspacing = (double)(washcycle[washcycleCounter].PumpBEnd - washcycle[washcycleCounter].PumpBStart) / (washcycle[washcycleCounter].TimeEnd - washcycle[washcycleCounter].TimeStart) / 60 / 10;
                pumpCspacing = (double)(washcycle[washcycleCounter].PumpCEnd - washcycle[washcycleCounter].PumpCStart) / (washcycle[washcycleCounter].TimeEnd - washcycle[washcycleCounter].TimeStart) / 60 / 10;
                pumpDspacing = (double)(washcycle[washcycleCounter].PumpDEnd - washcycle[washcycleCounter].PumpDStart) / (washcycle[washcycleCounter].TimeEnd - washcycle[washcycleCounter].TimeStart) / 60 / 10;

                pumpA += pumpAspacing;
                pumpB += pumpBspacing;
                pumpC += pumpCspacing;
                pumpD += pumpDspacing;
                if (washcycle[washcycleCounter].FlowDestination == 1)
                {
                    waste += (double)washcycle[washcycleCounter].FlowRate / 600;
                    pumpAml = (double)pumpA * washcycle[washcycleCounter].FlowRate * 0.01;
                    pumpBml = (double)pumpB * washcycle[washcycleCounter].FlowRate * 0.01;
                    pumpCml = (double)pumpC * washcycle[washcycleCounter].FlowRate * 0.01;
                    pumpDml = (double)pumpD * washcycle[washcycleCounter].FlowRate * 0.01;
                }
                else if (washcycle[washcycleCounter].FlowDestination == 2)
                {
                    holding += (double)washcycle[washcycleCounter].FlowRate / 600;
                    pumpAml = (double)pumpA * washcycle[washcycleCounter].FlowRate * 0.01;
                    pumpBml = (double)pumpB * washcycle[washcycleCounter].FlowRate * 0.01;
                    pumpCml = (double)pumpC * washcycle[washcycleCounter].FlowRate * 0.01;
                    pumpDml = (double)pumpD * washcycle[washcycleCounter].FlowRate * 0.01;
                }
                else
                {
                    nowtubeml += (double)washcycle[washcycleCounter].FlowRate / 600;
                    if (nowtubeml >= tubeml)
                    {
                        tubeNum++;
                        nowtubeml = 0;
                    }
                    pumpAml = (double)pumpA * washcycle[washcycleCounter].FlowRate * 0.01;
                    pumpBml = (double)pumpB * washcycle[washcycleCounter].FlowRate * 0.01;
                    pumpCml = (double)pumpC * washcycle[washcycleCounter].FlowRate * 0.01;
                    pumpDml = (double)pumpD * washcycle[washcycleCounter].FlowRate * 0.01;

                }
                Random random = new Random();
                pressure = random.Next(20, 50);
                au = random.Next(0, 100);
                wavelength = random.Next(0, 100);
                time += 0.0016666666666667;
                if (time >= washcycle[washcycleCounter].TimeEnd)
                {
                    washcycleCounter++;
                    pumpA = washcycle[washcycleCounter].PumpAStart;
                    pumpB = washcycle[washcycleCounter].PumpBStart;
                    pumpC = washcycle[washcycleCounter].PumpCStart;
                    pumpD = washcycle[washcycleCounter].PumpDStart;
                }
            }
            else
            {
                status = 3;
            }
            Console.WriteLine("Status: " + status + "\tPeptide: " + peptide + "\tTubeNum: " + tubeNum + "\tTime: " + Math.Round(time, 2) + "\tPumpA: " + Math.Round(pumpA, 2) + "\tPumpB: " + Math.Round(pumpB, 2) + "\tPumpC: " + Math.Round(pumpC, 2) + "\tPumpD: " + Math.Round(pumpD, 2) + "\nPumpAml: " + Math.Round(pumpAml, 2) + "\tPumpBml: " + Math.Round(pumpBml, 2) + "\tPumpCml: " + Math.Round(pumpCml, 2) + "\tPumpDml: " + Math.Round(pumpDml, 2) + "\tWaste: " + Math.Round(waste, 2) + "\tHolding: " + Math.Round(holding, 2) + "\tPressure: " + Math.Round(pressure, 2) + "\tAU: " + Math.Round(au, 2) + "\tWaveLength: " + Math.Round(wavelength, 2));
            Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------------------------");
        }
        protected override void OnClose(CloseEventArgs e)
        {
            Console.WriteLine("Connection Closed");
            base.OnClose(e);
        }

        protected override void OnError(ErrorEventArgs e)
        {
            Console.WriteLine("Error: " + e.Message);
            base.OnError(e);
        }

        private static bool TestJson(string json)
        {
            try
            {
                JToken.Parse(json);
                return true;
            }
            catch (JsonReaderException ex)
            {
               // Console.WriteLine(ex);
                return false;
            }
        }
    }
}
