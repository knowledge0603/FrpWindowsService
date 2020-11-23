using System;
using System.Diagnostics;
using System.ServiceProcess;
using static System.Net.Mime.MediaTypeNames;

namespace FrpWindowsService
{
    public partial class Service1 : ServiceBase
    {
        #region declare
        private Process frpProcess = null;
        private Process getFrpServiceProcess = null;
        #endregion

        #region InitializeComponent
        public Service1()
        {
            InitializeComponent();
        }
        #endregion

        #region service onStart
        protected override void OnStart(string[] args)
        //public  void OnStart( )
        {
            bool systemType = Environment.Is64BitOperatingSystem;
            if (systemType)//64bit
            {
                System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory + "baseDir\\frp");
            }
            else//32bit
            {
                System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory + "baseDir\\frp32bit");
            }
            WriteLog("call frp.bat start");
            getFrpServiceProcess = new Process();
            getFrpServiceProcess.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "baseDir\\frp\\frp.bat ";
            getFrpServiceProcess.StartInfo.CreateNoWindow = true;
            getFrpServiceProcess.StartInfo.UseShellExecute = false;
            // The process output
            getFrpServiceProcess.StartInfo.RedirectStandardOutput = true;
            getFrpServiceProcess.StartInfo.RedirectStandardError = true;
            getFrpServiceProcess.OutputDataReceived += new DataReceivedEventHandler(MyProcOutputHandler);
            getFrpServiceProcess.ErrorDataReceived += new DataReceivedEventHandler(MyProcOutputHandler);
            getFrpServiceProcess.Start();
            getFrpServiceProcess.BeginOutputReadLine();
            getFrpServiceProcess.BeginErrorReadLine();

            startFrp();
        }
        #endregion

        #region onSto
        protected override void OnStop()
        {
            CloseFrp();
            WriteLog("Service Stop\n");
        }
        #endregion

        #region start frp service
        private void startFrp(Object sender = null, EventArgs e = null)
        {
          
            frpProcess = new Process();
            frpProcess.StartInfo.FileName = "frpc.exe";
            frpProcess.StartInfo.Arguments = " -c frpc.ini";
            frpProcess.StartInfo.CreateNoWindow = true;
            frpProcess.StartInfo.UseShellExecute = false;
            // Guardian to restart
            frpProcess.EnableRaisingEvents = true;
            frpProcess.Exited += new EventHandler(startFrp);
            // The process output
            frpProcess.StartInfo.RedirectStandardOutput = true;
            frpProcess.StartInfo.RedirectStandardError = true;
            frpProcess.OutputDataReceived += new DataReceivedEventHandler(MyProcOutputHandler);
            frpProcess.ErrorDataReceived += new DataReceivedEventHandler(MyProcOutputHandler);
            frpProcess.Start();
            frpProcess.BeginOutputReadLine();
            frpProcess.BeginErrorReadLine();
        }
        #endregion

        #region close frp service 
        private void CloseFrp()
        {
            WriteLog("Kill frpc.exe");
            if (null == frpProcess)
                return;
            frpProcess.Kill();
            frpProcess.Close();
            frpProcess = null;
        }
        #endregion

        #region write log
        private void WriteLog(string logStr, bool wTime = true)
        {
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\FrpWindowsServiceAutoService.log", true))
            {
                string timeStr = wTime == true ? DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ") : "";
                sw.WriteLine(timeStr + logStr);
            }
        }
        #endregion

        #region write Frp ServiceStatus
        private void WriteFrpServiceStatus(string statusStr)
        {
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\FrpWindowsServiceStatus.ini", true))
            {
                sw.Write(statusStr);
            }
        }
        #endregion

        #region process out put
        private void MyProcOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                WriteLog(outLine.Data, true);
                if (outLine.Data.ToString().Contains("start proxy success") ) 
                {
                    WriteFrpServiceStatus("[frpServiceStatus]\n\r runing=true");
                }
                if (outLine.Data.ToString().Contains("error: i/o deadline reached"))
                {
                    WriteFrpServiceStatus("[frpServiceStatus]\n\r runing=false");
                }
            }
        }
        #endregion 
    }
}
