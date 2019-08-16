using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Automation;
using System.Text.RegularExpressions;

namespace Подключение_к_БД_ver_2._0
{
    public partial class Form1 : Form
    {


        [DllImport("user32.dll")]
        static extern int GetForegroundWindow();

        [DllImport("user32")]
        private static extern UInt32 GetWindowThreadProcessId(Int32 hWnd, out Int32 lpdwProcessId);

        private int teller = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Process[] processes = Process.GetProcesses();
           

                timer1.Start();
                
            

           // foreach (Process p in processes)
           // {
                
                
            //    if (!String.IsNullOrEmpty(p.MainWindowTitle))
           //     {
            //        string output;
            //        output = p.MainWindowTitle + " " + p.StartTime.ToString();
             //       listBox1.Items.Add(output);
                    
             //   }
           // }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("back in da buisness");
            teller++;
            if (teller == 1)
            {
                Console.WriteLine("teller==10");
                //getCurrentURL();
                Console.WriteLine("DONE!");
                getCurrentRunningAppName();
                //timer1.Stop();
                teller = 0;
            }
            
        }
        private Int32 GetWindowProcessID(Int32 hwnd)
        {
            Int32 pid = 1;
            GetWindowThreadProcessId(hwnd, out pid);
            return pid;
        }
        //проверка активного процесса
        private void getCurrentRunningAppName()
        {
            Int32 hwnd = 0;
            hwnd = GetForegroundWindow();
            string appProcessName = Process.GetProcessById(GetWindowProcessID(hwnd)).ProcessName;
            string appExePath = Process.GetProcessById(GetWindowProcessID(hwnd)).MainModule.FileName;
            string appExeName = appExePath.Substring(appExePath.LastIndexOf(@"\") + 1);
            if (appExeName == "chrome.exe")
            {
                getCurrentURL();           
            }
            else
            { 
                textBox1.Text = appProcessName + " | " + appExePath + " | " + appExeName;
                
            }
            Console.WriteLine(teller);        
        }



      
        public void getCurrentURL()
        {
            // переребор по процессам гугл хрома, а затем поиск элемента "Адресная строка и строка поиска"           
            Process[] procsChrome = Process.GetProcessesByName("chrome");
            foreach (Process chrome in procsChrome)
            {
                //Проверка что хром живой
                if (chrome.MainWindowHandle == IntPtr.Zero)
                {
                    continue;
                }

                // ищем элемент Адресная строка и строка поиска
                AutomationElement elm = AutomationElement.FromHandle(chrome.MainWindowHandle);
                AutomationElement elmUrlBar = elm.FindFirst(TreeScope.Subtree,
                  new PropertyCondition(AutomationElement.NameProperty, "Адресная строка и строка поиска"));

                // Если нашли, получаем url
                if (elmUrlBar != null)
                {
                    AutomationPattern[] patterns = elmUrlBar.GetSupportedPatterns();
                   if (patterns.Length > 0)
                    {
                        ValuePattern val = (ValuePattern)elmUrlBar.GetCurrentPattern(patterns[0]);
                        parseURL(val.Current.Value);
                        //textBox1.Text = "Chrome URL found: " + val.Current.Value;//url = 
                    }
                }
            }
        } //возвращает текущий URL
        public void parseURL(string url)
        {
            if (url!="") { 
                int firstChar = url.IndexOf("://")+3;
                int secondChar = url.IndexOf("/", firstChar);
                int lengthOfWord = secondChar - firstChar;
                url = url.Substring(firstChar, lengthOfWord);
                textBox1.Text = "Google Chrome URL found: " + url;
            } else
            {
                textBox1.Text = "Google Chrome";
            }
        }
            private void button2_Click(object sender, EventArgs e)
        {
            timer1.Start();
           
        }
    }
}
