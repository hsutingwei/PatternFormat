using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatternTransformateTool
{
    class LoadingBar
    {
        public static int LoadingPerNum = 0;//進度分幾區塊
        public static int LoadingIdx = 0;//進度區塊所在座標
        public static ConsoleColor colorBack = Console.BackgroundColor;
        public static ConsoleColor colorFore = Console.ForegroundColor;
        private static int LoadingBarIdx = 0;
        private static int LoadingNumIdx = 0;
        public static void InitLoading(string titleStr, int count)
        {
            LoadingPerNum = 100 / count;
            LoadingIdx = 0;

            Console.WriteLine("****** " + titleStr + "...******");
            Program.WriteLineCount++;

            //第二行繪制進度條背景           
            Console.BackgroundColor = ConsoleColor.DarkCyan;
            for (int i = 0; ++i <= 25;)
            {
                Console.Write(" ");
            }
            Console.WriteLine(" ");
            Console.BackgroundColor = colorBack;
            //LoadingBarIdx = Program.WriteLineCount;
            LoadingBarIdx = Console.CursorTop;
            Program.WriteLineCount++;

            //第三行輸出進度           
            Console.WriteLine("0%");
            //第四行輸出提示,按下回車可以取消當前進度  
            //LoadingNumIdx = Program.WriteLineCount;
            LoadingNumIdx = Console.CursorTop;
            Program.WriteLineCount++;
        }
        /// <summary>
        /// 繪製百分比讀取條
        /// </summary>
        /// <param name="Num">百分比</param>
        public static void DrawConsole(int Num)
        {
            ConsoleColor colorBack = Console.BackgroundColor;
            ConsoleColor colorFore = Console.ForegroundColor;

            Console.BackgroundColor = ConsoleColor.Yellow;//設置進度條顏色               
            //Console.SetCursorPosition(Num / 4, LoadingBarIdx);//設置光標位置,參數為第幾列和第幾行               
            Console.SetCursorPosition(Num / 4, LoadingBarIdx - 1);//設置光標位置,參數為第幾列和第幾行               
            Console.Write(" ");//移動進度條               
            Console.BackgroundColor = colorBack;//恢復輸出顏色               
                                                //更新進度百分比,原理同上.               
            Console.ForegroundColor = ConsoleColor.Green;
            //Console.SetCursorPosition(0, LoadingNumIdx);
            Console.SetCursorPosition(0, LoadingNumIdx - 1);
            Console.Write("{0}%", Num);
            Console.ForegroundColor = colorFore;
        }
    }
}
