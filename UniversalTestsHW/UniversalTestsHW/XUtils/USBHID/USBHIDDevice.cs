// .NET
using System;
using System.IO;
using System.Threading;

namespace XUtils.USBHID
{
    internal class USBHIDDevice : IDisposable
    {
        private IntPtr handle;          // 'handle' pro komunikaci se zarizenim
        private FileStream fileStream;  // 'stream' pro data do/z zarizeni
        public USBHIDInfo devInfo = null;

        // identifikace do 'ReportID'
        public byte ReportID { get; set; }
        // maximalni doba cekani na odpoved
        public int ReportInTimeToWait { get; set; } = 10000; // 10s

        // pro vystupni a vstupni report
        public byte[] ReportOutBytes = null;
        public byte[] ReportInBytes = null; public byte[] ReportInBytesEvent = null;
        public byte[] ReportFeature = null;

        // delegat pro predani projateho 'ReportIn' do aplikace
        public delegate void DelReportInBytes(USBHIDInfo devInf, byte[] report);
        public DelReportInBytes OnReportInBytes = null;

        // metoda pro test zda jde o odpoved
        public delegate bool DelIsThisAnswer(byte[] reportOut, byte[] reportIn);
        public DelIsThisAnswer IsThisAnswer { get; set; } = null;

        // otevreni zarizeni podle predaneho 'USBHIDInfo'
        public bool Open(USBHIDInfo devInfoIn)
        {
            try
            {
                /* safe file handle */
                Microsoft.Win32.SafeHandles.SafeFileHandle shandle;

                /* opens hid device file */
                handle = HIDUSBExtMethods.CreateFile(devInfoIn.Path,
                    HIDUSBExtMethods.GENERIC_READ | HIDUSBExtMethods.GENERIC_WRITE,
                    HIDUSBExtMethods.FILE_SHARE_READ | HIDUSBExtMethods.FILE_SHARE_WRITE,
                    IntPtr.Zero, HIDUSBExtMethods.OPEN_EXISTING, HIDUSBExtMethods.FILE_FLAG_OVERLAPPED,
                    IntPtr.Zero);

                /* whops */
                if (handle == HIDUSBExtMethods.INVALID_HANDLE_VALUE) { return false; }
                /* build up safe file handle */
                shandle = new Microsoft.Win32.SafeHandles.SafeFileHandle(handle, false);
                /* prepare stream - async */
                fileStream = new FileStream(shandle, FileAccess.ReadWrite, 3200, true);

                // nastaveni bufferu pro 'Report(s)', pokud je platna jejich velikost
                // Out
                if (devInfoIn.OutputReportBytesLength == 0) { ReportOutBytes = null; }
                else if ((ReportOutBytes == null) || (ReportOutBytes.Length != devInfoIn.OutputReportBytesLength))
                    { ReportOutBytes = new byte[devInfoIn.OutputReportBytesLength]; }
                // In
                if (devInfoIn.InputReportBytesLength == 0) { ReportInBytes = null; ReportInBytesEvent = null; }
                else if ((ReportInBytes == null) || (ReportInBytes.Length != devInfoIn.InputReportBytesLength))
                {
                    ReportInBytes = new byte[devInfoIn.InputReportBytesLength];
                    ReportInBytesEvent = new byte[devInfoIn.InputReportBytesLength];
                }
                // Feature(s)
                if (devInfoIn.FeatureReportBytesLength == 0) { ReportFeature = null; }
                else if ((ReportFeature == null) || (ReportFeature.Length != devInfoIn.FeatureReportBytesLength))
                    { ReportFeature = new byte[devInfoIn.FeatureReportBytesLength]; }

                devInfo = devInfoIn;

                /* report status */
                return true;
            } catch { return false; }
        }

        // ziskani 'stream' i pro externi ucely
        //public FileStream DeviceStream
        //{
        //    get { return fileStream; } /* internal set { _fileStream = value; } */
        //}

        // spusteni zarizeni (zatim vyzmam pouze pro pouziti prijmu pomoci vlakna)
        public void Start()
        {
            if (ReceiveByThread == true) { ReceiveStart(); }    // spusteni vlakna prijmu
        }
        // zastaveni zarizeni (zatim vyzman pouze pro pouziti prijmu pomoci vlakna)
        public void Stop()
        {
            if (ReceiveByThread == true) { ReceiveStop(); }     // zastaveni vlakna prijmu
        }

        // close
        public void Close()
        {
            // pokud je 'stream', tak se uzavre
            if (fileStream != null) { fileStream.Close(); fileStream = null; }
            Stop(); // radeji
            /* close handle */
            HIDUSBExtMethods.CloseHandle(handle);
        }

        // dispose
        public void Dispose() { Close(); }

        // --- ---

        // zapis 'report' do HW zarizeni
        public bool ReportOutSend()
        {
            if (fileStream == null) { return false; }
            try
            {
                if (ReportID > 0) { ReportOutBytes[0] = ReportID; }         // pokud je 'ReportID'
                fileStream.Write(ReportOutBytes, 0, ReportOutBytes.Length); // zapis do HW
                fileStream.Flush();                                         // okamzite odeslat
                return true;
            }
            catch(Exception e) { /* */ return false; }
        }

        // cteni 'report' z HW
        public bool ReportInReceive()
        {
            try
            {
                // pokud neni prijem pomoci vlakna / udalosti
                // (prijaty report se ulozi do 'ReportInBytes')
                if (ReceiveByThread == false)
                {
                    // cteni bloku 'byte(s)'
                    int rc = fileStream.Read(ReportInBytes, 0, ReportInBytes.Length);
                    // vraceni zda byl prijat pozadovany pocet
                    return (rc == ReportInBytes.Length);
                }
                // pokud je prijem pomoci vlakna / udalosti
                // (prijaty report se ulozi do 'ReportInBytesEvent')
                else
                {
                    // cteni bloku 'byte(s)'
                    int rc = fileStream.Read(ReportInBytesEvent, 0, ReportInBytesEvent.Length);
                    // vraceni zda byl prijat pozadovany pocet
                    return (rc == ReportInBytesEvent.Length);
                }
            }
            catch(Exception e) { /* System.Windows.MessageBox.Show(e.ToString()); */ return false; }
        }

        private enum AnswerState { None, Wait, Yes, TimeOut, Abort }
        private byte ansCmd = 0; private byte ansCmdSub = 0;
        private AnswerState answerState = AnswerState.None;           // zda byla prijata odpoved

        // pokud OK -> prijaty report je v 'ReportInBytes' (musi se zpracovat samostatne)
        // pokud TimeOut -> pijaty report je v 'ReportInBytesEvent' (automaticky se zpracuje)
        public USBHIDConsts.EnCmnStatus ReportSendAndReceive(int waitTimeMillis = 0)
        {
            // bude se cekat na odpoved, zatim neprisla
            answerState = AnswerState.Wait;
            // odeslani 'ReportOut'
            if (ReportOutSend() == false) { return USBHIDConsts.EnCmnStatus.SendError; }
            // cekani je v poctu '10ms', ale aspon jednu
            // (pokud je zadana hodnota pri volani, tak se pouzije, jinak se pouzije globalne nastavena)
            int rounds = (waitTimeMillis > 0) ? waitTimeMillis / 10 : ReportInTimeToWait / 10;
            if (rounds < 1) { rounds = 1; }
            // ceka se dokud neprijde odpoved, nebo neuplyne cas
            while ((answerState != AnswerState.Yes) && ((rounds--) > 0))
                // cekani '10ms'
                { Thread.Sleep(10); }

            // byla prijata odpoved
            if ((answerState == AnswerState.Yes) || (answerState == AnswerState.Abort))
                { return USBHIDConsts.EnCmnStatus.OK; }
            answerState = AnswerState.TimeOut;
            // nebyla prijata odpoved (vypresel cas)
            return USBHIDConsts.EnCmnStatus.TimeOut;
        }

        // --- prijem pomoci vlakna ---

        public bool ReceiveByThread = false;        // zda je prijem pomoci vlakna
        private Thread dataReadingThread = null;    // vlakno pro prijem dat z externiho zarizeni
        private bool receiving = false;             // zda je prijem ve vlakne aktivni (pro zastaveni vlakna)

        // spusteni prijimaciho vlakna
        public bool ReceiveStart()
        {
            receiving = true;
            // spusteni prjimaciho vlakna
            dataReadingThread = new Thread(new ThreadStart(ReportInReading));
            dataReadingThread.IsBackground = true; // pro jeho ukonceni pri ukonceni (cele) aplikace
            // toto zajisti ukonceni vlanka pri ukonceni aplikace
            dataReadingThread.IsBackground = true;
            dataReadingThread.Start();
            return true;
        }
        // zastaveni prjimaciho vlakna
        public bool ReceiveStop()
        {
            receiving = false;
            // zastaveni prijimaciho vlakna
            if (dataReadingThread != null) { /* dataReadingThread.Join(); */ /* dataReadingThread.Abort(); */ dataReadingThread = null; }
            return true;
        }

        // --- ---

        // cteni dat z externiho zarizeni ve vlakne
        private void ReportInReading()
        {
            // pouze pokud se prijima
            while (receiving == true)
            {
                // pouze pokud je zarizeni platne
                if (fileStream != null)
                {   // pouze pokud je precten dobry 'ReportIn'
                    if (ReportInReceive() == true)
                    {
                        // pokud se ceka / cekalo na odpoved, tak mozna prave prisla
                        if (answerState == AnswerState.Wait)
                        {
                            // pouze pokud je zadana metoda pro urceni odpovedi, tak se pouzije
                            if (IsThisAnswer != null)
                            {
                                // pokud je navrat 'TRUE', tak jde o ocekavanou 'Answer'
                                if (IsThisAnswer(ReportOutBytes, ReportInBytesEvent) == true)
                                {
                                    // prekopirovani reportu do 'ReportInBytes'
                                    Array.Copy(ReportInBytesEvent, ReportInBytes, ReportInBytes.Length);
                                    // hlaseni o nalezeni odpovedi, zde jiz dale nic
                                    answerState = AnswerState.Yes; continue;
                                }
                            }
                        }
                        // jiank se prijaty paket preda, tomu kdo ma o neho zajem (formou udalosti)
                        // (prijaty report je v 'ReportInBytesEvent')
                        if (OnReportInBytes != null) { OnReportInBytes(devInfo, ReportInBytesEvent); }
                    }
                }
            }
        }
    }
}
