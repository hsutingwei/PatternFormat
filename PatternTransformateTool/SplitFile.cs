using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;

namespace PatternTransformateTool
{
    class SplitFile
    {
        public static int BufferSizes = Convert.ToInt32(ConfigurationManager.AppSettings["BufferSizesKB"].ToString()) * 1024;//60000KB//分割檔案的大小
        public static void ChunkFile(DateTime now, string FilePath, string OutPath)
        {
            int Datas = 0;
            byte[] BinaryData = new byte[BufferSizes];
            byte[] tmpBinaryData = null;
            int tmpBufferSizes = 0;
            string FileName = Path.GetFileNameWithoutExtension(FilePath);
            string nowStr = now.ToString("yyyMMddHHmm");

            FileStream fs = File.OpenRead(FilePath);
            Datas = fs.Read(BinaryData, 0, BufferSizes);
            int tmpCount = Convert.ToInt32(fs.Length / BufferSizes);
            int i = 1;

            string OutFilePath = Path.Combine(OutPath, nowStr, FileName, "Split");
            if (!Directory.Exists(OutFilePath))
                Directory.CreateDirectory(OutFilePath);

            int tmpLastNum = -1;

            while (Datas > 0)
            {
                string tmpOutFilePath = Path.Combine(OutFilePath, i.ToString() + ".txt");
                FileStream fsout = File.OpenWrite(tmpOutFilePath);
                List<byte> tmpByteList = new List<byte>();
                bool findEnter = false;

                if (tmpBinaryData != null && tmpBinaryData.Count() > 0)
                {//檢查有無上一次的暫存陣列
                    fsout.Write(tmpBinaryData, 0, tmpBufferSizes);
                    tmpBinaryData = null;
                    tmpBufferSizes = 0;//歸零
                }

                if (Datas == BufferSizes)
                {//切割的最後一個檔案不需要跑loop
                    for (int j = Datas - 1; j >= 0; j--)
                    {//從最後一個byte開始檢查
                        if (Convert.ToInt32(BinaryData[j]) == 10)
                        {//尋找換行字元，10
                            findEnter = true;
                            tmpByteList.Add(BinaryData[j]);
                            tmpBufferSizes++;
                            break;
                        }

                        tmpByteList.Add(BinaryData[j]);
                        tmpBufferSizes++;
                    }
                }

                if (findEnter)
                {
                    tmpByteList.Reverse();//將list反轉
                    tmpBinaryData = new byte[tmpBufferSizes];
                    tmpByteList.CopyTo(0, tmpBinaryData, 0, tmpBufferSizes);
                    fsout.Write(BinaryData, 0, Datas - tmpBufferSizes);
                }
                else
                {
                    if (Datas < BufferSizes)//可能會分割不完整，因此最後檔案依剩餘的byte數直接輸出
                        fsout.Write(BinaryData, 0, Datas);
                    else
                        fsout.Write(BinaryData, 0, BufferSizes);
                }

                Datas = fs.Read(BinaryData, 0, BufferSizes);
                fsout.Flush();
                fsout.Close();
                fsout = null;
                GC.Collect();

                int tmpNum = 0;//LoadingBar數字
                if (tmpCount == 0)
                    tmpNum = 100;
                else
                    tmpNum = LoadingBar.LoadingIdx * LoadingBar.LoadingPerNum + (i * LoadingBar.LoadingPerNum / tmpCount);
                if (tmpNum != tmpLastNum)
                {
                    LoadingBar.DrawConsole(tmpNum);
                    tmpLastNum = tmpNum;
                }
                i++;
            }

            fs.Close();
            fs = null;
            GC.Collect();
        }
    }
}
