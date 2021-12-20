using System;
using System.IO;

namespace PatternTransformateTool
{
    class MakeLog
    {
        private string LogPath =  Path.Combine(Program.sStartupPath, "Log.idx");
        public void ErrorLog(string ErrorMsg)
        {
            string Now = DateTime.Now.ToString("yyyMMddHHmmss");
            ErrorMsg = ErrorMsg.Replace("\r", "").Replace("\n", "").Replace(",", "，");
            string outStr = Now + "," + ErrorMsg;
            RW rw = new RW();
            rw.WriteFileFromString(LogPath, outStr, true);
        }
    }
}
