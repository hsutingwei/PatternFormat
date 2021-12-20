using System.Text;
using System.IO;
using System.Collections.Generic;

namespace PatternTransformateTool
{
    class RW
    {
        public StreamWriter gSw = null;
        public void gOpenWriteFile(string filePath, bool isAppend, Encoding code)
        {
            gSw = new StreamWriter(filePath, isAppend, code);
        }
        public void gWriteFileFromString(string str)
        {
            gSw.WriteLine(str);
        }
        public void gWriteFileFromList(List<string> reList)
        {
            foreach (string x in reList)
                gSw.WriteLine(x);
        }
        public void gCloseWriteFile()
        {
            gSw.Close();
            gSw.Dispose();
            gSw = null;
        }
        public void WriteFileFromString(string sFilePath, string sOutStr, bool isAppend)
        {
            using (StreamWriter sw = new StreamWriter(sFilePath, isAppend, Encoding.UTF8))
            {
                sw.WriteLine(sOutStr);
                sw.Close();
            }
        }
        public void WriteFileFromList(string sFilePath, List<string> reList)
        {
            using (StreamWriter sw = new StreamWriter(sFilePath, false, Encoding.UTF8))
            {
                foreach(string x in reList)
                    sw.WriteLine(x);
                sw.Close();
            }
        }
        public void WriteFileFromList(string sFilePath, List<string> reList, Encoding code)
        {
            using (StreamWriter sw = new StreamWriter(sFilePath, false, code))
            {
                foreach (string x in reList)
                    sw.WriteLine(x);
                sw.Close();
            }
        }
        public List<string> ReadFileToList(string sFilePath)
        {
            List<string> reList = new List<string>();

            using (StreamReader sr = new StreamReader(sFilePath, Encoding.UTF8))
            {
                string tmpStr = "";
                while ((tmpStr = sr.ReadLine()) != null)
                    reList.Add(tmpStr);
                sr.Close();
            }

            return reList;
        }
        public List<string> ReadFileToList(string sFilePath, Encoding code)
        {
            List<string> reList = new List<string>();

            using (StreamReader sr = new StreamReader(sFilePath, code))
            {
                string tmpStr = "";
                while ((tmpStr = sr.ReadLine()) != null)
                    reList.Add(tmpStr);
                sr.Close();
            }

            return reList;
        }
    }
}
