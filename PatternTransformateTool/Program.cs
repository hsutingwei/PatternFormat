using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace PatternTransformateTool
{
    class Program
    {
        public static Encoding ReadFileCode = Encoding.UTF8;//來源讀取編碼
        public static Encoding WriteFileCode = Encoding.ASCII;//輸出編碼
        public static string sStartupPath = Application.StartupPath;
        public static int WriteLineCount = 0;
        //public static PerformanceCounter memory = new PerformanceCounter("Memory", "% Committed Bytes in Use");//監聽Memory佔有率
        public static PerformanceCounter ramUsage = null;//監聽Memory剩餘多少mb
        public static List<string> CustomizationPinNameList = new List<string>();
        static void Main(string[] args)
        {
            string filePath = ConfigurationManager.AppSettings["SourceDicPath"];
            string outDicPath = ConfigurationManager.AppSettings["TransformateToPath"];

            if (args.Count() == 2)
            {
                filePath = args[0];
                outDicPath = args[1];
            }

            List<string> FileList = new List<string>(Directory.GetFiles(filePath, "*.tstl2"));

            DateTime now = DateTime.Now;
            RW rw = new RW();
            MakeLog ml = new MakeLog();

            try
            {
                if (!CheckFirst(FileList))
                    return;

                //string CustomizationPinNamePath = ConfigurationManager.AppSettings["CustomizationPinNamePath"];
                //CustomizationPinNameList = new List<string>(rw.ReadFileToList(CustomizationPinNamePath));

                //now = Convert.ToDateTime("2019-10-01 17:59");

                DoSplit(now, FileList, outDicPath);
                DoGetToken(now, FileList, outDicPath);
            }
            catch (Exception e)
            {
                string aa = e.ToString();
                ml.ErrorLog(aa);
            }

            Console.WriteLine("");
            if (args.Count() == 0)
            {
                Console.WriteLine("按任意鍵結束");
                Console.ReadKey();
            }
        }
        /// <summary>
        /// 執行分割檔案
        /// </summary>
        /// <param name="FileList">檔案名稱串列</param>
        /// <param name="outDicPath">輸出資料夾路徑</param>
        public static void DoSplit(DateTime now, List<string> FileList, string outDicPath)
        {
            int listLength = FileList.Count;
            LoadingBar.InitLoading("分割檔案中", listLength);
            //讀取條

            for (int i = 0; i < listLength; i++)
            {
                SplitFile.ChunkFile(now, FileList[i], outDicPath);
                LoadingBar.DrawConsole((i + 1) * 100 / listLength);
                LoadingBar.LoadingIdx++;
            }

            Console.WriteLine("\r\n檔案分割完成");
            WriteLineCount++;
        }
        public static void DoGetToken(DateTime now, List<string> FileList, string DicPath)
        {
            int listLength = FileList.Count;
            LoadingBar.InitLoading("轉換格式中", listLength);
            //讀取條

            for (int i = 0; i < FileList.Count; i++)
            {
                string FileName = Path.GetFileNameWithoutExtension(FileList[i]);
                GetToken.FindToken(now, DicPath, FileName, (i + 1));
                LoadingBar.DrawConsole((i + 1) * 100 / listLength);
                LoadingBar.LoadingIdx++;
            }

            Console.WriteLine("\r\n轉換格式完成");
            WriteLineCount++;
        }
        public static bool CheckFirst(List<string> fileList)
        {
            int Check = 300;
            int LimitKB = Convert.ToInt32(ConfigurationManager.AppSettings["BufferSizesKB"].ToString()) * 1024;//大小限制(KB)
            bool isCheckRam = false;//是否要檢查ram大小

            for (int i = 0; i < fileList.Count; i++)
            {
                FileInfo file = new FileInfo(fileList[i]);
                long length = file.Length;//檔案大小
                file = null;
                if (length >= LimitKB)
                {
                    isCheckRam = true;
                    break;
                }
            }

            if (isCheckRam)
            {
                ramUsage = new PerformanceCounter("Memory", "Available MBytes");
                double nowMemory = ramUsage.NextValue();
                if (nowMemory < Check)
                {//小於300mb提出警告。
                    Console.WriteLine("目前記憶體空間量不足，請預留" + Check + "MB給本程式，或按任意鍵繼續執行本程式。");
                    Console.ReadKey();
                    return false;
                }
            }

            return true;
        }
    }
}
