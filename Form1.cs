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
using System.Data.SQLite;
using System.IO;



namespace Подключение_к_БД_ver_2._0
{
    public partial class Form1 : Form
    {
        SQLiteConnection liteConnection = new SQLiteConnection();
        SQLiteCommand workWithDB = new SQLiteCommand();
        

        [DllImport("user32.dll")]
        static extern int GetForegroundWindow();

        [DllImport("user32")]
        private static extern UInt32 GetWindowThreadProcessId(Int32 hWnd, out Int32 lpdwProcessId);

        string appName = "";
        string tempAppName = "";
        Boolean flagOnStart = true;

        private int teller = 0; //служит для таймера

        public Form1()
        {
            InitializeComponent();
            initSQLite();
            timer1.Start();
        }
        //
        private void button1_Click(object sender, EventArgs e)
        {
            Process[] processes = Process.GetProcesses();
           

                timer1.Start();

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //Console.WriteLine("back in da buisness");
            teller++;
            if (teller == 1)
            {
                //Console.WriteLine("teller==10");
                //Console.WriteLine("DONE!");
                getCurrentRunningAppName();
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
          //  try { 
                Int32 hwnd = 0;
                hwnd = GetForegroundWindow();
                Console.WriteLine(hwnd);
                if (hwnd!=0)
                { 
                    string appProcessName = Process.GetProcessById(GetWindowProcessID(hwnd)).ProcessName;
                    string appExePath = Process.GetProcessById(GetWindowProcessID(hwnd)).MainModule.FileName;
                    string appExeName = appExePath.Substring(appExePath.LastIndexOf(@"\") + 1);
                

                    if (appExeName == "chrome.exe")
                    {
                        tempAppName = "chrome.exe";
                        getCurrentURL();           
                    }
                    else
                    {
                        tempAppName = appExeName;
                        textBox1.Text = appProcessName + " | " + appExePath + " | " + appExeName;
                    }
                }
                else
                {
                    tempAppName = "ID: " + hwnd;
                }
                if (appName != tempAppName)
                {
                    appName = tempAppName;
                    writeToDB();
                    //Здесь вызов процедуры для записи в БД
                }

           // }
           // catch (Exception ex)
           // {
           //    MessageBox.Show("Error: " + ex);
           // }
        }


  
        public void getCurrentURL()
        {
           // try
           // {
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
           // }
           // catch (Exception ex)
           // {
           //     MessageBox.Show("Error: "+ ex);
            //}
        } //возвращает текущий URL
        public void parseURL(string url)//todo: parse file:c// ...
        {
            // try
            url = url.Replace("https://", "").Replace("http://", "").Replace("www.", "");
            // {   
            if (url != "" & url.IndexOf(":/")<0)
                {
                int startIndex = 0;
                int stopIndex = url.IndexOf("/");
                if (stopIndex > 0)
                { 
                    url = url.Substring(startIndex, stopIndex);
                    tempAppName = "Google Chrome: " + url;
                    textBox1.Text = "Google Chrome: " + url;
                }
                else
                {
                    url = url.Substring(startIndex);
                    tempAppName = "Google Chrome: " + url;
                    textBox1.Text = "Google Chrome: " + url;
                }
            } else
            {
                tempAppName = "Google Chrome";
                textBox1.Text = "Google Chrome";
            }
          // }
          // catch (Exception ex)
           //{
           //    MessageBox.Show("Error: " + ex);
          // }

        }


        public void writeToDB () //запись в БД
        {
            try
            {
                if (flagOnStart)
                {
                    flagOnStart = false;
                    workWithDB.CommandText = "Insert Into time_log ([app_name],[time_start]) Values ('" + appName + "', datetime(current_timestamp))"; //insert new row to time log
                    workWithDB.ExecuteNonQuery();
                }
                else
                {
                    workWithDB.CommandText = "Update time_log Set time_end = datetime(current_timestamp) where row_id = (select MAX(row_id) from time_log)"; //update time end
                    workWithDB.ExecuteNonQuery();
                    workWithDB.CommandText = "Update time_log Set duration = (Select (Cast ((JulianDay(time_end) - JulianDay(time_start)) * 24 * 60 * 60 As Integer))) where row_id = (select MAX(row_id) from time_log)";//update duration
                    workWithDB.ExecuteNonQuery();
                    workWithDB.CommandText = "Insert Into time_log ([app_name],[time_start]) Values ('" + appName + "', datetime(current_timestamp))";//insert new row to time log
                    workWithDB.ExecuteNonQuery();
                }
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Error: " + ex);
            }
        }

            public void initSQLite() //initializing SQLite
        {
            string baseName = "main.db3";
            if (File.Exists(baseName))
            {
                Console.WriteLine("There is file " + baseName);
            }
            else
            {
                SQLiteConnection.CreateFile(baseName);
            }

            if (liteConnection.State.ToString() == "Closed")
            {
                liteConnection.ConnectionString = @"Data Source = " + baseName;
                liteConnection.Open();
            }

            if (liteConnection.State.ToString() == "Open")
            {
                SQLiteCommand createDb = new SQLiteCommand();
                createDb.Connection = liteConnection;
                createDb.CommandText = "create table if not exists time_log(row_id integer primary key autoincrement, app_name TEXT, time_start TEXT, time_end TEXT, duration TEXT)";
                createDb.ExecuteNonQuery();
            }
            workWithDB.Connection = liteConnection;
        }

        private void formClosing(object sender, FormClosingEventArgs e) //Closing form listener
        {
            try
            { 
                e.Cancel = true;
                //insert into query
                try
                { 
                    SQLiteCommand closingAppCommand = new SQLiteCommand();
                    if (!flagOnStart)
                    { 
                        closingAppCommand.Connection = liteConnection;
                        closingAppCommand.CommandText = "Update time_log Set time_end = datetime(current_timestamp) where row_id = (select MAX(row_id) from time_log)";
                        closingAppCommand.ExecuteNonQuery();
                        closingAppCommand.CommandText = "Update time_log Set duration = (Select (Cast ((JulianDay(time_end) - JulianDay(time_start)) * 24 * 60 * 60 As Integer))) where row_id = (select MAX(row_id) from time_log)";//update duration
                        closingAppCommand.ExecuteNonQuery();
                    }
                }
                catch (SQLiteException ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                Environment.Exit(0);
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            initSQLite();
            timer1.Start();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {

                initSQLite();
                SQLiteCommand timeLogInsert = new SQLiteCommand();
                timeLogInsert.Connection = liteConnection;
                timeLogInsert.CommandText = "Insert into time_log (app_name,time_start,time_end) Values ('chrome.exe', datetime(), datetime())";
                timeLogInsert.ExecuteNonQuery();
                //selectFromDB.Connection = liteConnection;
                //selectFromDB.CommandText = "create table if not exists (row_id integer primary key autoincrement, app_name nvarchar(255), time_start datetime, time_end datetime, duration datetime)";
                //selectFromDB.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
/*
        private void chart1_Click(object sender, EventArgs e)
        {
            chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
        }
*/
    }
}







/*
 start script:
 Insert Into time_log ([app_name],[time_start]) Values()
 second script:
 Insert into time_log ([app_name],[time_start]) Values()
 Update time_log set time_end = (select time_start from time_log where row_id=MAX(row_id) where row_id = (MAX(row_id)-1)
 --trigger
 Exit script:
 Update time_log set time_end = '' where row_id = MAX(row_id)

 */
