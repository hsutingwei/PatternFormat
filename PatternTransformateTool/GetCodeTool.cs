using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatternTransformateTool
{
    class GetCodeTool
    {
        static Encoding GetEncoding(string input)
        {

            Encoding encoder = null;
            byte[] header = new byte[4];// 讀取前四個Byte
            using (Stream reader = File.Open(input, FileMode.Open, FileAccess.Read))
            {
                reader.Read(header, 0, 4);
                reader.Close();
            }

            if (header[0] == 0xFF && header[1] == 0xFE)
                encoder = Encoding.Unicode;// UniCode File
            else if (header[0] == 0xEF && header[1] == 0xBB && header[2] == 0xBF)
                encoder = Encoding.UTF8;// UTF-8 File
            else
            {
                if (!IsBig5Encoding(input))
                    encoder = Encoding.UTF8;//無BOM的utf8
                else
                    encoder = Encoding.ASCII;// Default Encoding File//Ansi  File
                                               //encoder = Encoding.Default;// Default Encoding File//Ansi  File
            }

            return encoder;
        }

        private static bool IsBig5Encoding(byte[] bytes)
        {//將byte[]轉為string再轉回byte[]看位元數是否有變
            Encoding big5 = Encoding.GetEncoding(950);
            return bytes.Length == big5.GetByteCount(big5.GetString(bytes));
        }

        private static bool IsBig5Encoding(string file)
        {//偵測檔案否為BIG5編碼
            if (!File.Exists(file)) return false;
            return IsBig5Encoding(File.ReadAllBytes(file));
        }
    }
}
