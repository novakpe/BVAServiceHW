// .NET
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUtils.Commun.SerialPortX
{
    internal class SerialPortMain
    {
        public enum EnCmnState
        {
            None = 0,      // nic se nedeje
            Recv = 1,      // prijima se (prave) paket
            Cptl = 2       // prijat spravny paket (zpracovava se)
        };
        public enum EnCmnResult
        {
            None = 0,
            OK = 1,
            TimeOut = 2
        }

        // stav prijmu paketu
        private static EnCmnState State = EnCmnState.None;
        // pouzity seriovy port
        private static SerialPort serialPort = new SerialPort() { StopBits = StopBits.One };

        public static CmnConsts.DelReceiveData OnReceiveData = null;

        // --- ---

        // inicializace
        public static void Init()
        {
            // udalost pri prijmu dat 'SerialPort'
            serialPort.DataReceived += SerialPortOnDataReceived;
        }

        // --- ---

        //// vraceni vsech dostupnych komunikacnich kanalu
        //public List<CmnChannelsBase> AllPossibleChannels
        //{
        //    get
        //    {
        //        List<CmnChannelsBase> channels = new List<CmnChannelsBase>();
        //        string[] ports = SerialPort.GetPortNames();
        //        foreach (string port in ports)
        //        { channels.Add(new CmnChannelsSerPort() { Name = port }); }
        //        // vraceni vsech nalezenych kanalu / portu
        //        return channels;
        //    }
        //}

        // --- ---

        // pripojeni na COM port
        public static bool Start(string name, int baudRate = 19200)
        {
            serialPort.BaudRate = baudRate;
            //serialPort.BaudRate = 115200;
            //serialPort.BaudRate = 9600;
            // pokud neni vybran vhodny port, tak navrat
            try { serialPort.PortName = name; } catch { return false; }
            // otevreni vybraneho portu
            try { serialPort.Open(); } catch { }
            // otevreni serioveho portu / komunikace
            return serialPort.IsOpen;
        }

        // odpojeni od COM port
        public static bool Stop()
        {
            // uzavreni portu
            serialPort.Close();
            // uzavreni serioveho portu / komunikace
            return !serialPort.IsOpen;
        }

        // --- ---

        // prijem dat z VCP (serial port)
        private static StringBuilder sb = new StringBuilder();
        private static void SerialPortOnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte value = 0;                                 // jedna prave prijata hodnota
            while (serialPort.BytesToRead > 0)              // prectou se vsechny dostupne / prijate hodnoty
            {
                value = (byte)serialPort.ReadByte();        // precteni jednoho byte
                if ((State == EnCmnState.None) && (value == '['))
                    { sb.Clear(); State = EnCmnState.Recv; }
                if (State == EnCmnState.Recv)
                {
                    sb.Append((char)value);
                    if (value == ']')
                    {
                        State = EnCmnState.Cptl;
                        if (OnReceiveData != null) { OnReceiveData(sb.ToString()); }
                        //SerialWork(sb.ToString());
                        //if (waitForPaket == false)
                        //{
                        //    // (toto neni uplne dobry, to by se melo zmenit)
                        //    //if (CmnMain.OnReceivePacket != null) { CmnMain.OnReceivePacket(sb.ToString()); }
                        //}
                        //else
                        //{
                        //    waitForPaket = false;
                        //    receivedPaket = sb.ToString();
                        //    are.Set();
                        //}
                        State = EnCmnState.None;
                    }
                }
            }
        }

        //// ukonceni zpracovani prijateho paketu
        //public static void Processed() { State = EnCmnState.None; }

        //private static string mac = null;
        //public static void SerialWork(string paket)
        //{
        //    mac = paket.Substring(3, 12);

        //    // (pokud neni, tak neni nutno paket vubec zpracovavat)
        //    HomeDeviceBase homeDeviceBase = DataWork.devices.FirstOrDefault(item => item.MacAdr == mac);
        //    if (homeDeviceBase != null)
        //    {
        //        // prevod 'rowData' z 'AsciiHex' na 'byte[]'
        //        byte[] rawDataBin = XUtils.BitsAndBytes.Utils01.AsciiHexStringToBytes(paket.Substring(15, paket.Length - 16));
        //        // zdracovani 'rowData'
        //        Project.Minew.MinewWork.EnPaketResult paketResult = Project.Minew.MinewWork.ProcessPacket(mac /* adresa senzoru */, rawDataBin /* data ze senzoru */, 0);
        //        if (paketResult != Project.Minew.MinewWork.EnPaketResult.Done)
        //            { }
        //    }
        //    //else { paketResult = EnPaketResult.NoDevice; }

        //    Processed();
        //}
    }
}
