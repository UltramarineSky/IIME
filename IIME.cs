using KeePass.Plugins;
using KeePass.Util;
using KeePass.Util.Spr;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;




namespace IIME
{
    public sealed class IIMEExt : Plugin
    {
        private const string UPDATEURL = "https://raw.githubusercontent.com/iuuniang/IIME/main/update.txt";

        private const string m_strPlaceholderEnglish = "{IME:EN}";
        private const string m_strPlaceholderChinese = "{IME:CN}";

        private IPluginHost m_host = null;

        public override bool Initialize(IPluginHost host)
        {
            m_host = host;
            AutoType.FilterSendPre += this.OnAutoTypeFilterSendPre;
            SprEngine.FilterPlaceholderHints.Add(m_strPlaceholderEnglish);
            SprEngine.FilterPlaceholderHints.Add(m_strPlaceholderChinese);

            return true;
        }

        public override void Terminate()
        {
            SprEngine.FilterPlaceholderHints.Remove(m_strPlaceholderEnglish);
            SprEngine.FilterPlaceholderHints.Remove(m_strPlaceholderChinese);
            AutoType.FilterSendPre -= this.OnAutoTypeFilterSendPre;
        }

        public override string UpdateUrl
        {
            get { return UPDATEURL; }
        }

        public override Image SmallIcon
        {
            get { return (Image)KeePass.Program.Resources.GetObject("B16x16_KTouch"); }
        }

        private void OnAutoTypeFilterSendPre(object sender, AutoTypeEventArgs autoTypeEventArgs)
        {
            Regex replacerEnglish = new Regex(Regex.Escape(m_strPlaceholderEnglish), RegexOptions.IgnoreCase);
            Regex replacerChinese = new Regex(Regex.Escape(m_strPlaceholderChinese), RegexOptions.IgnoreCase);


            autoTypeEventArgs.Sequence = replacerEnglish.Replace(autoTypeEventArgs.Sequence, match =>
            {

                SetIMEStatus(0x0001); // 英文输入
                return String.Empty;
            });

            autoTypeEventArgs.Sequence = replacerChinese.Replace(autoTypeEventArgs.Sequence, match =>
            {
                SetIMEStatus(0x0009); // 中文输入
                return String.Empty;
            });
        }

        [DllImport("Imm32.dll")]
        private static extern bool ImmSetConversionStatus(IntPtr hIMC, int fdwConversion, int fdwSentence);

        [DllImport("Imm32.dll")]
        private static extern IntPtr ImmGetContext(IntPtr hWnd);

        [DllImport("Imm32.dll")]
        private static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("Imm32.dll")]
        private static extern IntPtr ImmGetDefaultIMEWnd(IntPtr unnamedParam1);

        [DllImport("user32.dll ")]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);


        private void SetIMEStatus(int status)
        {
            IntPtr hWnd = GetForegroundWindow();
            IntPtr hIMC = ImmGetContext(hWnd);
            if (hIMC != IntPtr.Zero)
            {
                ImmSetConversionStatus(hIMC, status, 0);
                ImmReleaseContext(hWnd, hIMC);
            }
        }
    }
}
