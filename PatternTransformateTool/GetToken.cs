using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PatternTransformateTool
{
    class GetToken
    {
        public static List<string> HeadList = new List<string> { "AREA1", "AREA2" };//區段開頭的辨認字串
        public static List<string> EndList = new List<string> { "ENDAREA", ";" };//區段結尾的辨認字串
        public static List<string> HeadList2 = new List<string> { "TESTAREA" };//區段開頭的辨認字串
        public static List<string> EndList2 = new List<string> { "ENDTESTAREA" };//區段結尾的辨認字串
        public static List<string> TestPattHead = new List<string> { "SP1", "SP2" };//TESTAREA開頭的辨認字串
        public static List<string> TestPattEnd = new List<string> { ";", ";" };//TESTAREA開頭的辨認字串
        public static List<string> TestPattChar = new List<string> { "Z" };//TESTAREA字串需要替換的辨認字串
        public static List<string> TestPattReplaceChar = new List<string> { "X" };//TESTAREA字串需要替換後的結果，索引值對應TestPattChar
        public static bool[] isFound = new bool[HeadList.Count];//紀錄區塊是否找到，索引值對應HeadList名稱
        //開頭、結尾的辨認字串，頭尾以相同座標對應。例如"AREA1"對應"ENDAREA"
        public static List<PinName> PinNameInfList = new List<PinName>();
        public static List<string> oriPinNameList = new List<string>();//PinName串列，未排序
        public static List<string> PinNameList = new List<string>();//PinName串列，已排序
        public static string firstStr = "  //";
        public static string firstStr2 = "    ";
        public static string StartChar = " ST:";
        public static string EndChar = " SP2:";
        public static double AllRunTime = 0;
        public static string CustomizationTestPatt = "";
        private static RW grw = new RW();//此行作法是為了優化檔案寫入速度的優化，有程式閱讀困難的可能
        public static void FindToken(DateTime now, string DicPath, string FileName, int fileNumber)
        {
            string nowStr = now.ToString("yyyMMddHHmm");
            string TestPattDicPath = Path.Combine(DicPath, nowStr, FileName, "Split");
            List<string> FileList = new List<string>(Directory.GetFiles(TestPattDicPath));
            List<string> aSetOfPattern = new List<string>();
            RW rw = new RW();
            MakeLog ml = new MakeLog();

            bool isFoundArrayAllTrue = true;//isFound這個陣列都是true，(需要找的區塊都已找到)
            string tmpHead = "";//紀錄此次尋找區段的頭//紀錄目前找的區段是屬於哪種區段
            bool findEnd = false;//紀錄是否找到區段結尾
            bool StartGetTestPattToken = false;//開始尋找TESTAREA紀錄
            bool inPattEnable = false;//進入TESTAREA裡的SP1區塊
            string PinNameComment = "";//PinName註解資訊
            bool isFirstTestPatt = true;

            int tmpCount = FileList.Count;
            int tmpLastNum = -1;

            for (int i = 0; i < Program.CustomizationPinNameList.Count; i++)
                CustomizationTestPatt += "1";

            for (int i = 1; i <= FileList.Count; i++)
            {
                string TestPattFilePath = Path.Combine(TestPattDicPath, i.ToString() + ".txt");

                if (File.Exists(TestPattFilePath))
                {
                    List<string> FileTextList = new List<string>(rw.ReadFileToList(TestPattFilePath, Program.ReadFileCode));

                    if (i > 1 && FileTextList[0] == "")//因第一行為換行字元，所以索引0的值應為空值
                        FileTextList.RemoveAt(0);//去除第一個空值

                    for (int j = 0; j < FileTextList.Count;)
                    {
                        if (!StartGetTestPattToken)
                        {//先找AREA1、AREA2
                            #region 尋找AREA1、AREA2(PIN NAME)，找到並輸出至暫存檔
                            if (!findEnd && aSetOfPattern.Count > 0)//因有分割檔的關係，會有下一個檔案接著找end的狀況
                            {//有正在儲存的暫存Pattern、且沒有找到end，需繼續尋找end
                                int tmpEndIdx = HeadList.IndexOf(tmpHead);

                                for (; j < FileTextList.Count && !findEnd; j++)
                                {//找到end，離開迴圈
                                    if (FileTextList[j].IndexOf(EndList[tmpEndIdx]) > -1)//找end
                                        findEnd = true;//找到end，就會離開迴圈

                                    aSetOfPattern.Add(FileTextList[j]);
                                }
                            }
                            else
                            {//區段從 head 開始找
                                tmpHead = "";//初始化
                                for (int k = 0; k < HeadList.Count; k++)
                                {
                                    if (FileTextList[j].IndexOf(HeadList[k]) == 0)
                                    {//找head
                                        tmpHead = HeadList[k];//紀錄此次搜尋區塊的head
                                        findEnd = false;//初始化
                                        aSetOfPattern.Add(FileTextList[j]);
                                        for (j = j + 1; j < FileTextList.Count && !findEnd; j++)
                                        {//找到end，離開迴圈
                                            if (FileTextList[j].IndexOf(EndList[k]) > -1)//找end
                                                findEnd = true;//找到end，就會離開迴圈

                                            aSetOfPattern.Add(FileTextList[j]);
                                        }
                                        break;
                                    }
                                }

                                if (tmpHead == "")//表示目前沒有找到任何區段
                                    j++;
                            }

                            if (findEnd && aSetOfPattern.Count > 0)
                            {//找完完整的區段，即輸出至暫存檔
                                if (tmpHead == "AREA1")
                                {
                                    string OutFilePath = Path.Combine(DicPath, nowStr, FileName, tmpHead);
                                    string GetFileName = aSetOfPattern[0].Replace("AREA1", "").Replace(";", "").Trim();
                                    WriteFile(OutFilePath, GetFileName, aSetOfPattern);
                                }
                                else if (tmpHead == "AREA2")
                                {
                                    string OutFilePath = Path.Combine(DicPath, nowStr, FileName);
                                    WriteFile(OutFilePath, tmpHead, aSetOfPattern);

                                    OutFilePath = Path.Combine(DicPath, nowStr, FileName + ".sd");
                                    GetPinNameListAndWrite(aSetOfPattern, OutFilePath);
                                    PinNameComment = WritePinNameInComment();
                                    string TestPattStart = "@@PATTERN_DEFINE\r\n" + PinNameComment;
                                    //rw.WriteFileFromString(OutFilePath, TestPattStart, true);
                                    grw.gWriteFileFromString(TestPattStart);
                                }

                                int tmpHeadIdx = HeadList.IndexOf(tmpHead);
                                isFound[tmpHeadIdx] = true;

                                aSetOfPattern.Clear();
                                findEnd = false;

                                foreach (bool x in isFound)
                                    if (!x)
                                        isFoundArrayAllTrue = false;

                                if (isFoundArrayAllTrue)
                                    StartGetTestPattToken = true;
                                isFoundArrayAllTrue = true;//初始化
                            }
                            #endregion
                        }
                        else
                        {//找到AREA1、AREA2後，接續找TESTAREA
                            #region 尋找TESTAREA
                            if (!findEnd && aSetOfPattern.Count > 0)//因有分割檔的關係，會有下一個檔案接著找end的狀況
                            {//有正在儲存的暫存Pattern、且沒有找到end，需繼續尋找end
                                int tmpEndIdx = TestPattHead.IndexOf(tmpHead);

                                for (; j < FileTextList.Count && !findEnd; j++)
                                {//找到end，離開迴圈
                                    if (FileTextList[j].IndexOf(TestPattEnd[tmpEndIdx]) > -1)//找end
                                        findEnd = true;//找到end，就會離開迴圈

                                    aSetOfPattern.Add(FileTextList[j]);
                                }

                                bool haveComment = false;
                                for (; findEnd && j < FileTextList.Count && FileTextList[j].IndexOf("/*") == 0; j++)
                                {
                                    aSetOfPattern.Add(FileTextList[j]);
                                    haveComment = true;
                                }

                                if (!haveComment)
                                    j--;//沒找到需要把計數器減回去
                            }
                            else
                            {//區段從 head 開始找
                                tmpHead = "";//初始化
                                findEnd = false;//初始化
                                for (; j < FileTextList.Count && !findEnd;)
                                {
                                    for (int m = 0; m < TestPattHead.Count && !findEnd && j < FileTextList.Count; m++)
                                    {
                                        if (j < FileTextList.Count && FileTextList[j].IndexOf(TestPattHead[m]) == 0)
                                        {
                                            tmpHead = TestPattHead[m];//紀錄此次搜尋區塊的head
                                            if (tmpHead == "SP1")
                                                inPattEnable = true;
                                            else if (tmpHead == "SP2")
                                                inPattEnable = false;

                                            aSetOfPattern.Add(FileTextList[j]);

                                            int n = j + 1;
                                            while (FileTextList[n].IndexOf(TestPattHead[m]) > -1)
                                            {
                                                aSetOfPattern.Add(FileTextList[j]);
                                                n++;
                                            }

                                            for (j = n; j < FileTextList.Count && !findEnd; j++)
                                            {
                                                if (FileTextList[j].IndexOf(TestPattEnd[m]) > -1)//找end
                                                    findEnd = true;//找到end，就會離開迴圈

                                                aSetOfPattern.Add(FileTextList[j]);
                                            }
                                        }
                                    }

                                    if (tmpHead == "" && inPattEnable)
                                    {//SP1直到下個SP2開頭為止，區段都沒有 head ，但有 end -> ;
                                        tmpHead = "SP1";//紀錄此次搜尋區塊的head
                                        int tmpEndIdx = TestPattHead.IndexOf(tmpHead);
                                        aSetOfPattern.Add(FileTextList[j]);

                                        for (j = j + 1; j < FileTextList.Count && !findEnd; j++)
                                        {
                                            if (FileTextList[j].IndexOf(TestPattEnd[tmpEndIdx]) > -1)//找end
                                                findEnd = true;//找到end，就會離開迴圈

                                            aSetOfPattern.Add(FileTextList[j]);
                                        }
                                    }

                                    bool haveComment = false;
                                    for (; findEnd && j < FileTextList.Count && FileTextList[j].IndexOf("/*") == 0; j++)
                                    {//往下尋找，有註解便加入串列
                                        aSetOfPattern.Add(FileTextList[j]);
                                        haveComment = true;
                                    }

                                    if (findEnd && !haveComment)
                                        j--;//沒找到註解需要把計數器減回去

                                    if (tmpHead == "")//沒有找到完整的patt區塊
                                        j++;
                                }
                            }

                            if (findEnd && aSetOfPattern.Count > 0)
                            {//找到完整的區段，即輸出至暫存檔
                                string OutFilePath = Path.Combine(DicPath, nowStr, FileName + ".sd");
                                string ScanDic = Path.Combine(DicPath, nowStr, FileName, "AREA1");
                                string ResultStr = "";
                                bool isStart = false;
                                bool isEnd = false;

                                if (isFirstTestPatt)
                                {
                                    isStart = true;
                                    isFirstTestPatt = false;
                                }
                                else if (j + 1 < FileTextList.Count && FileTextList[j + 1].IndexOf(EndList2[0]) > -1)//表示下一行結束
                                    isEnd = true;//已找到轉碼最後一組TESTAREA

                                if (tmpHead == "SP1")
                                    ResultStr = PatternToString("SP1", aSetOfPattern, ScanDic, isStart, isEnd);
                                else if (tmpHead == "SP2")
                                    ResultStr = PatternToString("SP2", aSetOfPattern, ScanDic, isStart, isEnd);

                                //rw.WriteFileFromString(OutFilePath, ResultStr, true);
                                grw.gWriteFileFromString(ResultStr);

                                aSetOfPattern.Clear();
                                findEnd = false;
                            }
                            #endregion
                        }
                    }
                }

                //Console.WriteLine(i);
                int tmpNum = 0;
                if (tmpCount == 0)
                    tmpNum = 100;
                else
                    tmpNum = LoadingBar.LoadingIdx * LoadingBar.LoadingPerNum + (i * LoadingBar.LoadingPerNum / tmpCount);
                if (tmpNum != tmpLastNum)
                {
                    LoadingBar.DrawConsole(tmpNum);
                    tmpLastNum = tmpNum;
                }
            }

            if (!findEnd && aSetOfPattern.Count > 0)
            {
                int tmpEndIdx = HeadList.IndexOf(tmpHead);
                //ml.ErrorLog(tmpHead + " 找不到 " + EndList[tmpEndIdx]);
            }

            string tmpOutFilePath = Path.Combine(DicPath, nowStr, FileName + ".sd");
            string EndTestPattStr = "@@END_PATTERN_DEFINE\r\n" + PinNameComment;
            //rw.WriteFileFromString(tmpOutFilePath, EndTestPattStr, true);
            grw.gWriteFileFromString(EndTestPattStr);
            grw.gCloseWriteFile();
            PinNameInfList.Clear();
            oriPinNameList.Clear();
            PinNameList.Clear();
            isFound = new bool[HeadList.Count];

            double aaa = AllRunTime;
        }
        public static void WriteFile(string dicPath, string fileName, List<string> reList)
        {
            RW rw = new RW();
            if (!Directory.Exists(dicPath))
                Directory.CreateDirectory(dicPath);

            string outPath = Path.Combine(dicPath, fileName + ".txt");
            rw.WriteFileFromList(outPath, reList, Program.ReadFileCode);
        }
        public static List<PinName> GetPineNameList(List<string> sList)
        {
            List<PinName> reList = new List<PinName>();
            int count = 0;

            for (int i = 0; i < sList.Count; i++)
            {
                string tmpLine = sList[i].Replace("AREA2", "").Replace(";", "").Trim().Replace("[,]+", ",").Replace(":", ",");
                List<string> tmpNameList = new List<string>(tmpLine.Split(','));

                for (int j = 0; j < tmpNameList.Count; j++)
                {
                    if (tmpNameList[j].Length > 0)
                    {
                        reList.Add(new PinName { PINNAME = tmpNameList[j], Idx = count });
                        count++;
                    }
                }
            }

            return reList;
        }
        /// <summary>
        /// 處理含有SP1開頭的TESTAREA
        /// </summary>
        /// <param name="aSetOfPattern"></param>
        /// <returns></returns>
        public static string PatternToString(string TestPattHead, List<string> aSetOfPattern, string ScanPath, bool isFirst, bool isEnd)
        {
            //DateTime time_start = DateTime.Now;//計時開始 取得目前時間

            MakeLog ml = new MakeLog();
            StringBuilder tmpLine = new StringBuilder();
            string reStr = "";
            List<int> tmpIdxList = new List<int>();
            string[] reSpArr = new string[PinNameList.Count];

            if (TestPattHead == "SP1")
            {
                for (int i = 0; i < aSetOfPattern.Count; i++)
                {
                    if (aSetOfPattern[i].IndexOf("SP1") > -1 || aSetOfPattern[i].IndexOf("/*") > -1)
                        tmpIdxList.Add(i);
                    else
                        tmpLine.Append(aSetOfPattern[i]);
                }

                reStr = tmpLine.ToString();
                reStr = reStr.Replace(" ", "");

                for (int i = 0; i < TestPattChar.Count; i++)
                    reStr = reStr.Replace(TestPattChar[i], TestPattReplaceChar[i]);
                if (tmpIdxList.Count > 0)
                {
                    reStr += "//";
                    for (int i = 0; i < tmpIdxList.Count; i++)
                        reStr += aSetOfPattern[tmpIdxList[i]];
                }

                if (isFirst)
                    reStr = StartChar + reStr;
                else if (isEnd)
                    reStr = EndChar + reStr;
                else
                    reStr = firstStr2 + reStr;
            }
            else if (TestPattHead == "SP2")
            {
                RW rw = new RW();
                List<SpLog> spInfList = new List<SpLog>();
                List<string> SpLogPathNameList = new List<string>();
                List<string> ConstPinNameList = new List<string>();
                List<string> ScanReList = new List<string>();
                int SpLogStrLength = 0;

                for (int i = 0; i < aSetOfPattern.Count; i++)
                {
                    if (aSetOfPattern[i].IndexOf("/*") > -1)
                        tmpIdxList.Add(i);
                    else
                    {
                        int tmpIdx = -1;
                        if ((tmpIdx = aSetOfPattern[i].IndexOf(")")) > -1)
                        {
                            string partA = aSetOfPattern[i].Substring(0, tmpIdx + 1);
                            string partB = aSetOfPattern[i].Substring(tmpIdx + 1);
                            for (int j = 0; j < TestPattChar.Count; j++)
                                partB = partB.Replace(TestPattChar[j], TestPattReplaceChar[j]);
                            tmpLine.Append(partA + partB);
                        }
                        else
                        {
                            string tmpStr = aSetOfPattern[i];
                            for (int j = 0; j < TestPattChar.Count; j++)
                                tmpStr = tmpStr.Replace(TestPattChar[j], TestPattReplaceChar[j]);
                            tmpLine.Append(tmpStr);
                        }
                    }
                }

                reStr = tmpLine.ToString();
                reStr = reStr.Replace(" ", "").Replace(";", "");
                List<string> spList = new List<string>(reStr.Split(','));
                tmpLine.Clear();

                for (int i = 0; i < spList.Count; i++)
                {
                    SpLog sl = new SpLog();
                    int SpNameStartIdx = spList[i].IndexOf("(");
                    int SpNameEndIdx = spList[i].IndexOf(")");
                    string SpName = spList[i].Substring(SpNameStartIdx + 1, SpNameEndIdx - SpNameStartIdx - 1);
                    string SpLogStr = spList[i].Substring(SpNameEndIdx + 1);
                    sl.SpName = SpName;
                    sl.SpLogStr = SpLogStr;
                    if (SpLogStr.Length > SpLogStrLength)
                        SpLogStrLength = SpLogStr.Length;

                    string tmpScanPath = Path.Combine(ScanPath, SpName + ".txt");
                    string ScanFileName = Path.GetFileNameWithoutExtension(tmpScanPath);
                    if (File.Exists(tmpScanPath))
                    {
                        List<string> spFtList = new List<string>(rw.ReadFileToList(tmpScanPath));
                        string PathPinName = spFtList[1].Replace("PATH", "").Replace(";", "").Replace(" ", "");

                        for (int j = 2; j < spFtList.Count; j++)
                            if (spFtList[j].IndexOf("ENDAREA") < 0)
                                tmpLine.Append(spFtList[j].Trim());

                        string ConstPinName = tmpLine.ToString();
                        ConstPinName = ConstPinName.Replace("CONST", "").Replace(" ", "").Replace(";", "");

                        sl.PathPinName = PathPinName;
                        sl.ConstPinName = ConstPinName;

                        spInfList.Add(sl);

                        if (spInfList[0].ConstPinName != ConstPinName)
                            ml.ErrorLog("SP2(" + ScanFileName + ") 的CONST和同組的不一樣");
                    }
                    else
                        ml.ErrorLog("AREA1 找不到 " + SpName);

                    tmpLine.Clear();
                }

                /*spInfList = spInfList.OrderBy(x => x.PathPinName).ToList();
                foreach (SpLog x in spInfList)
                    SpLogPathNameList.Add(x.PathPinName);*/
                ConstPinNameList = new List<string>(spInfList[0].ConstPinName.Split(','));
                //ConstPinNameList.Sort();

                List<int> CacheIdxList = new List<int>();//二元搜尋結果快取
                List<int> CacheIdxList2 = new List<int>();//二元搜尋結果快取

                for (int i = 0; i < SpLogStrLength; i++)
                {
                    #region 已註解(較慢)
                    /*for (int j = 0; j < reSpArr.Count(); j++)
                    {
                        int tmpIdx = -1;
                        if ((tmpIdx = SpLogPathNameList.BinarySearch(oriPinNameList[j])) > -1)
                        {
                            string getChar = spInfList[tmpIdx].SpLogStr.Substring(i, 1);
                            int PathPinNameIdx = PinNameList.BinarySearch(spInfList[tmpIdx].PathPinName);
                            reSpArr[j] = getChar;
                        }
                        else if ((tmpIdx = ConstPinNameList.BinarySearch(oriPinNameList[j])) > -1)
                        {
                            List<string> tmpList = new List<string>(ConstPinNameList[tmpIdx].Split('='));
                            string getChar = tmpList[1];
                            int PathPinNameIdx = PinNameList.BinarySearch(tmpList[0]);
                            reSpArr[j] = getChar;
                        }
                        else
                            reSpArr[j] = "0";
                    }*/
                    #endregion

                    for (int j = 0; j < spInfList.Count; j++)
                    {
                        string getChar = spInfList[j].SpLogStr.Substring(i, 1);
                        int PathPinNameIdx = 0;
                        if (j < CacheIdxList.Count)
                            PathPinNameIdx = CacheIdxList[j];
                        else
                        {
                            PathPinNameIdx = PinNameList.BinarySearch(spInfList[j].PathPinName);
                            CacheIdxList.Add(PathPinNameIdx);
                        }

                        reSpArr[PinNameInfList[PathPinNameIdx].Idx] = getChar;
                    }

                    if (i == 0)
                    {//ConstPinName及剩餘pin的值都不會變，因此只需賦予一次即可
                        for (int j = 0; j < ConstPinNameList.Count; j++)
                        {
                            List<string> tmpList = new List<string>(ConstPinNameList[j].Split('='));
                            string getChar = tmpList[1];
                            for (int k = 0; k < TestPattChar.Count; k++)
                                getChar = getChar.Replace(TestPattChar[k], TestPattReplaceChar[k]);
                            int PathPinNameIdx = 0;
                            if (j < CacheIdxList2.Count)
                                PathPinNameIdx = CacheIdxList2[j];
                            else
                            {
                                PathPinNameIdx = PinNameList.BinarySearch(tmpList[0]);
                                CacheIdxList2.Add(PathPinNameIdx);
                            }

                            reSpArr[PinNameInfList[PathPinNameIdx].Idx] = getChar;
                        }
                        for (int j = 0; j < reSpArr.Count(); j++)
                            if (reSpArr[j] == null || reSpArr[j].Length == 0)
                                reSpArr[j] = "X";
                    }

                    string tmpStr = string.Join("", reSpArr);
                    tmpStr += ";";
                    if (tmpIdxList.Count > 0)
                    {
                        tmpStr += "//";
                        for (int j = 0; j < tmpIdxList.Count; j++)
                            tmpStr += aSetOfPattern[tmpIdxList[j]];
                    }

                    if (isFirst && i == 0)
                        ScanReList.Add(StartChar + tmpStr);
                    else if (isEnd && i == SpLogStrLength - 1)
                        ScanReList.Add(EndChar + tmpStr);
                    else
                        ScanReList.Add(firstStr2 + tmpStr);
                }

                reStr = string.Join("\r\n", ScanReList);
            }

            GC.Collect();

            //DateTime time_end = DateTime.Now;//計時結束 取得目前時間
            //string result2 = ((TimeSpan)(time_end - time_start)).TotalMilliseconds.ToString();
            //AllRunTime += (Convert.ToDouble(result2) / 1000 / 60);

            return reStr;
        }
        /// <summary>
        /// 取得PinName串列並輸出
        /// </summary>
        /// <param name="aSetOfPattern"></param>
        public static void GetPinNameListAndWrite(List<string> aSetOfPattern, string OutPath)
        {//對PinNameList排序是為了之後可以使用二元搜尋
            RW rw = new RW();
            PinNameInfList = new List<PinName>(GetPineNameList(aSetOfPattern));

            foreach (PinName x in PinNameInfList)
                oriPinNameList.Add(x.PINNAME);

            PinNameInfList = PinNameInfList.OrderBy(x => x.PINNAME).ToList();

            for (int i = 0; i < PinNameInfList.Count; i++)
                PinNameList.Add(PinNameInfList[i].PINNAME);

            List<string> tmpOutList = new List<string>(oriPinNameList);
            if (Program.CustomizationPinNameList.Count > 0)
                tmpOutList.AddRange(Program.CustomizationPinNameList);

            string PinNameStr = string.Join(",\r\n", tmpOutList);
            PinNameStr = "HEAD[PIN_NAME]:\r\n" + PinNameStr + ";\r\n\r\nEND_HEAD; \r\n";
            //rw.WriteFileFromString(OutPath, PinNameStr, false);

            grw = new RW();
            grw.gOpenWriteFile(OutPath, true, Program.WriteFileCode);
            grw.gWriteFileFromString(PinNameStr);
        }
        /// <summary>
        /// 輸出PinName註解至TESTAREA之前或之後
        /// </summary>
        public static string WritePinNameInComment()
        {
            List<string> tmpPinNameList = new List<string>(oriPinNameList);
            if (Program.CustomizationPinNameList.Count > 0)
                tmpPinNameList.AddRange(Program.CustomizationPinNameList);

            int MaxLength = 0;
            for (int i = 0; i < tmpPinNameList.Count; i++)
                if (tmpPinNameList[i].Length > MaxLength)
                    MaxLength = tmpPinNameList[i].Length;

            StringBuilder reStr = new StringBuilder();
            for (int i = 0; i < MaxLength; i++)
            {
                reStr.Append(firstStr);
                for (int j = 0; j < tmpPinNameList.Count; j++)
                {
                    if (i < tmpPinNameList[j].Length)
                        reStr.Append(tmpPinNameList[j].Substring(i, 1));
                    else
                        reStr.Append(" ");
                }
                reStr.Append("\r\n");
            }

            return reStr.ToString();
        }
    }
    class PinName
    {
        public string PINNAME { get; set; }
        public int Idx { get; set; }
    }
    class SpLog
    {
        public string SpName { get; set; }
        public string SpLogStr { get; set; }
        public string PathPinName { get; set; }
        public string ConstPinName { get; set; }
    }
}
