using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Console_Arabic_Support
{
    public class ArabicParser
    {
        private readonly string[] _numbers =
        {
            "٠١٢٣٤٥٦٧٨٩",
            "0123456789",
            "۰۱۲۳۴۵۶۷۸۹"
        };

        private const int LaaIndex = 8 * 50;

        private const string Left = "یٹہےگکڤچپـئظشسيبلتنمكطضصثقفغعهخحج";
        private const string Right = "یٹہےڈڑگکڤژچپـئؤرلالآىآةوزظشسيبللأاأتنمكطضصثقفغعهخحجدذلإإۇۆۈ";

        private const string Harakat = "";
        private const string Symbols = @"ـ.،؟ @#$%^&*-+|\/=~,:";

        private const string Unicode = "ﺁ ﺁ ﺂ ﺂ " + "ﺃ ﺃ ﺄ ﺄ " + "ﺇ ﺇ ﺈ ﺈ " + "ﺍ ﺍ ﺎ ﺎ " + "ﺏ ﺑ ﺒ ﺐ " + "ﺕ ﺗ ﺘ ﺖ " +
                                       "ﺙ ﺛ ﺜ ﺚ " + "ﺝ ﺟ ﺠ ﺞ " + "ﺡ ﺣ ﺤ ﺢ " + "ﺥ ﺧ ﺨ ﺦ " + "ﺩ ﺩ ﺪ ﺪ " + "ﺫ ﺫ ﺬ ﺬ " +
                                       "ﺭ ﺭ ﺮ ﺮ " + "ﺯ ﺯ ﺰ ﺰ " + "ﺱ ﺳ ﺴ ﺲ " + "ﺵ ﺷ ﺸ ﺶ " + "ﺹ ﺻ ﺼ ﺺ " + "ﺽ ﺿ ﻀ ﺾ " +
                                       "ﻁ ﻃ ﻄ ﻂ " + "ﻅ ﻇ ﻈ ﻆ " + "ﻉ ﻋ ﻌ ﻊ " + "ﻍ ﻏ ﻐ ﻎ " + "ﻑ ﻓ ﻔ ﻒ " + "ﻕ ﻗ ﻘ ﻖ " +
                                       "ﻙ ﻛ ﻜ ﻚ " + "ﻝ ﻟ ﻠ ﻞ " + "ﻡ ﻣ ﻤ ﻢ " + "ﻥ ﻧ ﻨ ﻦ " + "ﻩ ﻫ ﻬ ﻪ " + "ﻭ ﻭ ﻮ ﻮ " +
                                       "ﻱ ﻳ ﻴ ﻲ " + "ﺓ ﺓ ﺔ ﺔ " + "ﺅ ﺅ ﺆ ﺆ " + "ﺉ ﺋ ﺌ ﺊ " + "ﻯ ﻯ ﻰ ﻰ " + "ﭖ ﭘ ﭙ ﭗ " +
                                       "ﭺ ﭼ ﭽ ﭻ " + "ﮊ ﮊ ﮋ ﮋ " + "ﭪ ﭬ ﭭ ﭫ " + "ﮒ ﮔ ﮕ ﮓ " + "ﭦ ﭨ ﭩ ﭧ " + "ﮦ ﮨ ﮩ ﮧ " +
                                       "ﮮ ﮰ ﮱ ﮯ " + "ﯼ ﯾ ﯿ ﯽ " + "ﮈ ﮈ ﮉ ﮉ " + "ﮌ ﮌ ﮍ ﮍ " + "ﯗ ﯗ ﯘ ﯘ " + "ﯙ ﯙ ﯚ ﯚ " +
                                       "ﯛ ﯛ ﯜ ﯜ " + "ﮎ ﮐ ﮑ ﮏ " + "ﻵ ﻵ ﻶ ﻶ " + "ﻷ ﻷ ﻸ ﻸ " + "ﻹ ﻹ ﻺ ﻺ " + "ﻻ ﻻ ﻼ ﻼ ";

        private const string Arabic = "آ" + "أ" + "إ" + "ا" + "ب" + "ت" + "ث" + "ج" + "ح" + "خ" + "د" + "ذ" + "ر" +
                                      "ز" + "س" + "ش" + "ص" + "ض" + "ط" + "ظ" + "ع" + "غ" + "ف" + "ق" + "ك" + "ل" +
                                      "م" + "ن" + "ه" + "و" + "ي" + "ة" + "ؤ" + "ئ" + "ى" + "پ" + "چ" + "ژ" + "ڤ" +
                                      "گ" + "ٹ" + "ہ" + "ے" + "ی" + "ڈ" + "ڑ" + "ۇ" + "ۆ" + "ۈ" + "ک";

        private const string NotEng = Arabic + Harakat + "ء،؟";
        private const string Brackets = "(){}[]";

        public string ProcessInput(string valx)
        {
            var old = "";
            var tstr = "";
            var y = valx;
            var x = y.ToCharArray();
            var len = x.Length;
            var outbox = "";

            void AddChar(char chr)
            {
                outbox = chr + outbox;
            }
            for (var g = 0; g < len; g++)
            {
                const int a = 1;
                const int b = a;

                var pos = 0;
                if (g == 0)
                {
                    pos = Right.IndexOf(x[a]) >= 0 ? 2 : 0;
                }
                else if (g == len - 1)
                {
                    pos = Left.IndexOf(x[len - b - 1]) >= 0 ? 6 : 0;
                }
                else
                {
                    if (Left.IndexOf(x[g - b]) < 0)
                        pos = Right.IndexOf(x[g + a]) < 0 ? 0 : 2;
                    else if (Left.IndexOf(x[g - b]) >= 0) pos = Right.IndexOf(x[g + a]) >= 0 ? 4 : 6;
                }

                switch (x[g])
                {
                    case '\n':
                        old = old + outbox + "\n";
                        outbox = "";
                        break;

                    case '\r':
                        old = old + outbox + "\r";
                        outbox = "";
                        break;

                    case 'ء':
                        AddChar('ﺀ');
                        break;

                    default:
                        {
                            if (Brackets.IndexOf(x[g]) >= 0)
                            {
                                var asd = Brackets.IndexOf(x[g]);
                                AddChar(asd % 2 == 0 ? Brackets[asd + 1] : Brackets[asd - 1]);
                            }
                            else if (Arabic.IndexOf(x[g]) >= 0)
                            {
                                if (x[g] == 'ل')
                                {
                                    var arPos = Arabic.IndexOf(x[g + 1]);

                                    if (arPos >= 0 && arPos < 4)
                                    {
                                        AddChar(Unicode[arPos * 8 + pos + LaaIndex]);
                                        g += 1;
                                    }
                                    else
                                    {
                                        AddChar(Unicode[Arabic.IndexOf(x[g]) * 8 + pos]);
                                    }
                                }
                                else
                                {
                                    AddChar(Unicode[Arabic.IndexOf(x[g]) * 8 + pos]);
                                }
                            }
                            else if (Symbols.IndexOf(x[g]) >= 0)
                            {
                                AddChar(x[g]);
                            }
                            else if (Unicode.IndexOf(x[g]) >= 0)
                            {
                                var uniPos = Unicode.IndexOf(x[g]);
                                var laPos = Unicode.IndexOf(x[g]);
                                if (laPos >= LaaIndex)
                                {
                                    for (var temp = 8; temp < 40; temp += 8)
                                        if (laPos < temp + LaaIndex)
                                        {
                                            AddChar(Arabic[temp / 8 - 1]);
                                            AddChar('ل');
                                            temp = 60;
                                        }
                                }
                                else
                                {
                                    for (var temp = 8; temp < LaaIndex + 32; temp += 8)
                                        if (uniPos < temp)
                                        {
                                            AddChar(Arabic[temp / 8 - 1]);
                                            temp = 1000;
                                        }
                                }
                            }
                            else
                            {
                                var h = g;

                                while (x.Length - 1 >= h && NotEng.IndexOf(x[h]) < 0 && Unicode.IndexOf(x[h]) < 0 &&
                                       Brackets.IndexOf(x[h]) < 0)
                                {
                                    foreach (var key in _numbers)
                                    {
                                        var mynumb = key.IndexOf(x[h]);
                                        if (mynumb >= 0) x[h] = key[mynumb];
                                    }

                                    tstr += x[h];
                                    h += 1;
                                }

                                var xstr = tstr.ToCharArray();
                                var r = xstr.Length - 1;
                                if (r == 1 && xstr[1] == ' ')
                                    tstr = " " + xstr[0];
                                else
                                    while (xstr[r] == ' ')
                                    {
                                        tstr = " " + tstr.Substring(0, tstr.Length - 1);
                                        r -= 1;
                                    }

                                outbox = tstr + outbox;
                                tstr = "";
                                g = h - 1;
                            }

                            break;
                        }
                }
            }

            outbox = old + outbox;
            return outbox;
        }
    }

    public class CustomWr : StreamWriter
    {
        private readonly ArabicParser _arb;

        public CustomWr(Stream stream)
            : base(stream)
        {
            _arb = new ArabicParser();
        }

        #region c++ hooking

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal unsafe struct ConsoleFontInfoEx
        {
            internal uint cbSize;
            internal uint nFont;
            internal Coord dwFontSize;
            internal int FontFamily;
            internal int FontWeight;
            internal fixed char FaceName[LfFacesize];
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Coord
        {
            internal short X;
            internal short Y;

            internal Coord(short x, short y)
            {
                X = x;
                Y = y;
            }
        }

        private const int StdOutputHandle = -11;
        private const int TmpfTruetype = 4;
        private const int LfFacesize = 32;
        private static readonly IntPtr InvalidHandleValue = new IntPtr(-1);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetCurrentConsoleFontEx(
            IntPtr consoleOutput,
            bool maximumWindow,
            ref ConsoleFontInfoEx consoleCurrentFontEx);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int dwType);

        #endregion c++ hooking

        public static void SetConsoleFont(string fontName, short fontSize)
        {
            unsafe
            {
                var hnd = GetStdHandle(StdOutputHandle);
                if (hnd != InvalidHandleValue)
                {
                    var info = new ConsoleFontInfoEx();
                    info.cbSize = (uint)Marshal.SizeOf(info);

                    var newInfo = new ConsoleFontInfoEx();
                    newInfo.cbSize = (uint)Marshal.SizeOf(newInfo);
                    newInfo.FontFamily = TmpfTruetype;
                    var ptr = new IntPtr(newInfo.FaceName);
                    Marshal.Copy(fontName.ToCharArray(), 0, ptr, fontName.Length);

                    newInfo.dwFontSize = new Coord(info.dwFontSize.X, fontSize/*info.dwFontSize.Y*/);
                    newInfo.FontWeight = info.FontWeight;
                    SetCurrentConsoleFontEx(hnd, false, ref newInfo);
                }
            }
        }

        public static void InitAr(string fontName = "Arial", short fontSize = 25, int width = 108, int height = 30)
        {
            var standardOutput = new CustomWr(Console.OpenStandardOutput()) { AutoFlush = true };
            SetConsoleFont(fontName, fontSize);
            Console.OutputEncoding = Encoding.UTF8;
            Console.SetWindowSize(width, height);
            Console.SetOut(standardOutput);
        }

        #region OverridedMethods

        public override void Write(string value)
        {
            base.Write(_arb.ProcessInput(value));
        }

        public override void Write(char[] buffer)
        {
            base.Write(_arb.ProcessInput(new string(buffer)).ToCharArray());
        }

        public override void Write(char[] buffer, int index, int count)
        {
            var charArray = _arb.ProcessInput(new string(buffer)).ToCharArray();
            if (charArray.Length > 0)
            {
                count = charArray.Length;
            }
            base.Write(charArray, index, count);
        }

        #endregion OverridedMethods

        public override Encoding Encoding => Encoding.UTF8;
    }
}