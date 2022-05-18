/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 	Addon			:	Timing Measurement tools        															//
//	Description		:	Helps debugging the indicators and strategies												//
//	Author			:	Trading.Codes (contact@trading.codes)	: T. Todua									    	//
//	URL				:	https://puvox.software/blog/ninjatrader-time-measurement-debugger-for-methods/		    	//
//	Version			:   2019.05.28																					//
//	License			: 	CopyRight @  Free for personal use only.													//
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



using NinjaTrader.Gui;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Forms;






#region   Example Usage in indicator
/*
namespace NinjaTrader.NinjaScript.Indicators
{
    public class XYZ_Indicator : Indicator
    {
        ...
        ...
        ...
        ...
        ...
        #region Timings debuger
        private int debugerInitState = 0;
        public virtual void method_hook(string name, bool start_or_end)
        {
            if ( debugerInitState == 0 )
            {
                var prop = this.GetType().GetProperty("DebugTimerEnabled");
                debugerInitState = prop != null && (bool) prop.GetValue(this, null) ? 1 : -1;
            }
            if ( debugerInitState == 1 )
            {
                var callerName = name; // new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name;
                debugTimer(callerName, start_or_end);
            }
        } 
        #endregion
        ...
        ...
        ...
        ...
        ...
    }
}
*/
#endregion



#region   Extensions for Debugger>TimingMeasure
namespace NinjaTrader.NinjaScript.Indicators
{
    [Gui.CategoryOrder("DebugTimer", 999)]
    public partial class Indicator
    {
        global::TradingCodes.Debugger.MethodTimeMeasure Mtm;
        Dictionary<string,  global::TradingCodes.Debugger.MethodTimeMeasure.StopWatchForMethod_Class> sw;

        public bool DebugTimerEnabled { get { return  global::TradingCodes.Debugger.MethodTimeMeasure.TimingsEnabled; } }

        public void initializedCheck() {
            if (Mtm == null)
            {
                Mtm = new  global::TradingCodes.Debugger.MethodTimeMeasure();
                sw = new Dictionary<string,  global::TradingCodes.Debugger.MethodTimeMeasure.StopWatchForMethod_Class>();
            }
        }

        public void DebugTimer(string callerName, bool start_or_end)
        {
            if (DebugTimerEnabled)
            {
                initializedCheck();
                Mtm.debugTimer(this, callerName, start_or_end,  ref sw );   //0 was debugnumber
            }
        }
    }
}
#endregion


// Menu Button
namespace NinjaTrader.NinjaScript.AddOns.TradingCodes
{
    public sealed partial class MethodsTimingMeasurementWindow : NinjaTrader.NinjaScript.AddOnBase
    {
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "Methods timing measurement";
                //Description = @"";
            }
            else if (State == State.Configure)
            {
            }
            else if (State == State.Active)
            {
            }
        }


        private NTMenuItem addOnFrameworkMenuItem;
        private NTMenuItem existingMenuItemInControlCenter;

        // Will be called as a new NTWindow is created. It will be called in the thread of that window
        protected override void OnWindowCreated(Window window)
        {
            try
            {
                // We want to place our AddOn in the Control Center's menus
                ControlCenter cc = window as ControlCenter;
                if (cc == null)
                    return;

                // Determine we want to place our AddOn in the Control Center's "New" menu
                // Other menus can be accessed via the control's "Automation ID". For example: toolsMenuItem, workspacesMenuItem, connectionsMenuItem, helpMenuItem.

                existingMenuItemInControlCenter = cc.FindFirst("ControlCenterMenuItemTools") as NTMenuItem;
                if (existingMenuItemInControlCenter == null)
                    return;

                // 'Header' sets the name of our AddOn seen in the menu structure
                addOnFrameworkMenuItem = new NTMenuItem { Header = " " + Name, Style = System.Windows.Application.Current.TryFindResource("MainMenuItem") as Style };
                existingMenuItemInControlCenter.Items.Add(addOnFrameworkMenuItem);
                addOnFrameworkMenuItem.Click += OnMenuItemClick;
            }
            catch (Exception e)
            {
                Print(e.ToString());
            }
        }

        // Will be called as a new NTWindow is destroyed. It will be called in the thread of that window
        protected override void OnWindowDestroyed(Window window)
        {
            try
            {
                if (addOnFrameworkMenuItem != null && window is ControlCenter)
                {
                    if (existingMenuItemInControlCenter != null && existingMenuItemInControlCenter.Items.Contains(addOnFrameworkMenuItem))
                        existingMenuItemInControlCenter.Items.Remove(addOnFrameworkMenuItem);

                    addOnFrameworkMenuItem.Click -= OnMenuItemClick;
                    addOnFrameworkMenuItem = null;
                }
            }
            catch (Exception e)
            {
                Print(e.ToString());
            }
        }

        public static object locker_ = new object();
        public static Form1 MainForm { get { lock (locker_) { return MainForm_; } return MainForm_; } set { lock (locker_) { MainForm_ = value; } } }
        public static Form1 MainForm_;

        // Open our AddOn's window when the menu item is clicked on
        private void OnMenuItemClick(object sender, RoutedEventArgs eArgs)
        {
            try
            {
                ShowWind();
            }
            catch (Exception e)
            {
                Print(e.ToString());
            }
        }
        private void ShowWind()
        {
            MainForm = new Form1()  {   };
            MainForm.init();
            MainForm.Show();
        }
    }

     
    public partial class Form1 : Form
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            MyTimer.Tick -= new EventHandler(fillTexts);
            base.Dispose(disposing);
        }

        System.Windows.Forms.Timer MyTimer;
        Panel panelsGroup;
        public Form1()
        {
          
        }

        public void init()
        {
            this.Width = 800; //Math.Min((int)System.Windows.SystemParameters.PrimaryScreenWidth - 100, 800),
            this.Height = 800; //(int)System.Windows.SystemParameters.PrimaryScreenHeight - 100,
                               //FormBorderStyle = FormBorderStyle.FixedDialog,
            this.Text = "Methods Timers window";
            this.StartPosition = FormStartPosition.CenterScreen;
            AutoScroll = true;
            panelsGroup = new Panel();
            panelsGroup.Top = 1;
            panelsGroup.Left = 1;
            panelsGroup.Width = this.Width - 20;
            panelsGroup.Height = this.Height - 60;
            panelsGroup.AutoScroll = true;

            
            this.EnableDisableButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // EnableDisableButton 
            this.EnableDisableButton.Top = 0;
            this.EnableDisableButton.Left = 10;
            this.EnableDisableButton.Name = "EnableDisableButton";
            this.EnableDisableButton.TabIndex = 0;
            this.EnableDisableButton.Text = "Enable/Disable";
            this.EnableDisableButton.UseVisualStyleBackColor = true;
            EnableDisableButton.Click += (sender, e) =>  { global::TradingCodes.Debugger.MethodTimeMeasure.ChangeEnableState(); colorizeButton(); };
            colorizeButton();

            this.ClearAll = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // ClearAll 
            this.ClearAll.Top = 0;
            this.ClearAll.Left = 100;
            this.ClearAll.Name = "ClearAll";
            this.ClearAll.TabIndex = 1;
            this.ClearAll.Text = "Clear All";
            this.ClearAll.UseVisualStyleBackColor = true;
            ClearAll.Click += (sender, e) => { textboxGroups.Clear();  top = 0; };  
            //  
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.EnableDisableButton);
            this.Controls.Add(this.ClearAll);
            this.Controls.Add(panelsGroup);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormBorderStyle = FormBorderStyle.Sizable;
            //prompt.BringToFront() ;
            this.TopMost = true;
            this.ResumeLayout(false);

            MyTimer = new System.Windows.Forms.Timer();
            MyTimer.Interval = (1000); // 1 sec
            MyTimer.Tick += new EventHandler(fillTexts);
            MyTimer.Start();
        }
        private System.Windows.Forms.Button ClearAll;
        private System.Windows.Forms.Button EnableDisableButton;

        int labelHeight = 20;
        public void addLabelInPanel(Panel panel, string labelTxt)
        {
            System.Windows.Forms.Label label1= new System.Windows.Forms.Label();
            panel.Controls.Add(label1); 
            label1.Left = 150;
            label1.Width = 250;
            label1.Height = labelHeight;
            label1.Name = labelTxt; 
            label1.Text = labelTxt;
        }
        void colorizeButton(){ EnableDisableButton.Text = global::TradingCodes.Debugger.MethodTimeMeasure.TimingsEnabled ? "Enabled" : "Disabled";  }

        int top = 0;
        int left = 2;
        int idx = 0;

        public Dictionary<string, Dictionary<string, string> > textboxGroups = new Dictionary<string, Dictionary<string, string>>();

        void clearPanel()
        {
            panelsGroup.Controls.Clear();
            top = 0;
        }
        private void fillTexts(object sender, EventArgs e)
        {
            if(this.Visible)
            {
                fillForm(this);
            }
        }
         
        public void fillForm(Form form)
        {
          //  Task.Run(() => { //thread example:  https://pastebin.com/raw/JUEWaAs4 

          //  });
            try
            {
                if (form == null || form.IsDisposed) return;
                if (form.InvokeRequired)
                {
                    form.Invoke(new System.Action(() => { if (form == null || form.IsDisposed) return; fillForm(form); }));   //new Action<string>(SetControlText), new object[] { which, txt }
                    return;
                }
                //clearPanel();
                foreach (var kvp in textboxGroups)
                {
                    string chartHandle = kvp.Key;
                    Panel panel = null;
                    bool exists = panelsGroup.Controls.ContainsKey(chartHandle);
                    if (exists)
                    {
                        panel = panelsGroup.Controls[chartHandle] as Panel;
                    }
                    else 
                    { 
                        panel = new Panel();
                        addLabelInPanel(panel, kvp.Key);
                        //panel.BackColor = System.Drawing.Color.Orange;
                        //panel.SuspendLayout();
                        //panel.Left = left;
                        panel.Name = chartHandle;
                        panel.AutoScroll = true;
                        panel.Width = panelsGroup.Width - 25;
                        panel.Height = 400;
                        panel.Top =top + 5;
                        top += panel.Height;
                        panelsGroup.Controls.Add(panel);
                        //panel.ResumeLayout(false);
                        //panel.PerformLayout();
                    }

                    foreach (var kvp2 in kvp.Value)
                    {
                        string funcHandle = kvp2.Key; int heigh = 0;
                        if (!panel.Controls.ContainsKey(funcHandle))
                        {
                            TextBox textBox = new TextBox() { Left = 1, Width = panel.Width - 25, Height = 20 };  //
                            textBox.Name = funcHandle;
                            textBox.Multiline = false;
                            textBox.ScrollBars = ScrollBars.None;
                            panel.Controls.Add(textBox); 
                            heigh = textBox.Height;
                            textBox.Top = (panel.Controls.Count-1)* heigh;
                        }
                        panel.Controls[funcHandle].Text = kvp2.Value;
                    } 
                }

            }
            catch (Exception e)
            {
                //print(e.ToString());
            }
        }

        

        public static void print(object text) { NinjaTrader.Code.Output.Process(text == null ? "null" : text.ToString(), PrintTo.OutputTab1); }


    }
}



namespace TradingCodes.Debugger
{
    #region Time Measure class
    public partial class MethodTimeMeasure
    {
        private static bool initedBool;
        public static void initializedBool()
        {
            if (!initedBool)
            {
                initedBool = true;
                TimingsEnabled_ = !String.IsNullOrEmpty(File.ReadAllText(filePath));
            }
        }

        private static bool TimingsEnabled_;
        public static bool TimingsEnabled
        {
            get
            {
                initializedBool();
                return TimingsEnabled_;
            }
        }
         
        static string filePath= Path.GetTempPath() + "\\nt_debug_timer";
        public static void ChangeEnableState()
        { 
            if (!File.Exists(filePath)) File.WriteAllText(filePath, "");
            TimingsEnabled_ = !TimingsEnabled_;
            File.WriteAllText(filePath, TimingsEnabled ? "" : "y");
        }

        private static void m(object o_) { System.Windows.Forms.MessageBox.Show(o_ == null ? "null" : o_.ToString()); }

        public static void message1()
        {
            string path = Path.GetTempPath() + "NT_message1_shown.nt";
            if (!File.Exists(path))
            {
                System.Windows.Forms.MessageBox.Show("Note from Timer-Debugger:  Percentage amount can be considered only when script is being executed on Realtime data, and have more than 1-2 executions. otherwise it might always show up to 99% and shouldnt be considered, unless it does several executions in realtime. ");
                File.WriteAllText(path, "");
            }
        }

         
        public static string getChartHandle(NinjaTrader.NinjaScript.NinjaScriptBase _NSB, bool timeframeSpecific)
        {
            //  ChartControl.OwnerChart.Form.Handle); //same as:  ChartControl.OwnerChart.Form.Handle.GetHashCode();  ||   ChartControl.OwnerChart.GetHashCode());   ||   ChartControl.Parent.GetHashCode());   ||   (_NSB as NinjaTrader.Gui.NinjaScript.IndicatorRenderBase).ChartControl.OwnerChart.Form.Handle.ToString();
            // return _NSB.Instrument.FullName + "_" + _NSB.BarsPeriods[0].BarsPeriodType.ToString() + "_" + _NSB.BarsPeriods[0].Value + "____id" + (_NSB as NinjaTrader.Gui.NinjaScript.IndicatorRenderBase).ChartControl.OwnerChart.Form.Handle.ToString();

            NinjaScriptBase target = _NSB;
            while (target.Parent != null) target = target.Parent;
            return target.Instrument.FullName + "_" + (timeframeSpecific? target.BarsPeriod.BarsPeriodType.ToString() + "_" + target.BarsPeriod.Value : "") + "____id" + (target as NinjaTrader.Gui.NinjaScript.IndicatorRenderBase).ChartControl.OwnerChart.Form.Handle.ToString(); //ChartControl.GetHashCode().ToString();
        }

        public Dictionary<string, Form> forms = new Dictionary<string, Form>();
        NinjaTrader.NinjaScript.NinjaScriptBase NSB_;
        public void debugTimer(NinjaTrader.NinjaScript.NinjaScriptBase _NSB, string funcName, bool start_or_end, ref Dictionary<string, TradingCodes.Debugger.MethodTimeMeasure.StopWatchForMethod_Class> sw )
        {
            //if (NSB_.BarsInProgress != 0 && NSB_.CurrentBar <= 0) return;
            try
            {
                NSB_ = _NSB;
                if (NSB_ == null || NSB_.State!= State.Realtime || NSB_.CurrentBar < 0 || NSB_.Bars == null ) return;
                if (NSB_.CurrentBar < NSB_.Bars.Count - 2) return; //need only realtime, otherwise no idea of that, will always take 99% of course
                int BIP = NSB_.BarsInProgress;
                //var mName = funcName;
                // Fix for unexplained bug for repeated Stacktrace calling in OBU
                //if (mName == "Update") mName = "OnBarUpdate"; // && stk.GetFrame(3).GetMethod().Name == "TickReplayOrUpdate"
                //if (mName == "Render") { mName = "OnRender"; } //FRAME(3) :  HitTestChartObject or RenderToTarget

                string chartHandle = getChartHandle(NSB_, false);
                var funcName1 = funcName; // + "_"+BIP
                var funcName2 = funcName + "_" + BIP;
                if (!sw.ContainsKey(funcName1))
                {
                    sw[funcName1] = new TradingCodes.Debugger.MethodTimeMeasure.StopWatchForMethod_Class(NSB_ ); 
                }

                if (start_or_end)
                {
                    sw[funcName1].start(funcName1, BIP);
                }
                else
                {
                    sw[funcName1].stopDraw(funcName1, BIP, "");
                }
            }
            catch (Exception e)
            {
                NSB_.Print(e.ToString());
            }
        }
        private void p(object txt) { NSB_.Print(txt==null? "null" : txt.ToString());  }
		public int useDrawMethod = 1;  //1:form   2:chart-draw
		
        public class StopWatchForMethod_Class
		{
            public bool WorkOnHistorical { get; set; }
            public bool allowedToWork { get { return WorkOnHistorical || isNearLive; } }
            bool isNearLive { get {
                    bool allowed= NSB_.State == State.Realtime || NSB_.CurrentBars[0] >= NSB_.BarsArray[0].Count - 1;
                    return allowed;
                } }


            private System.Diagnostics.Stopwatch sw;
            private Dictionary<int, string> funcName_ =  new Dictionary<int, string>();  //must initialize
			private NinjaTrader.NinjaScript.NinjaScriptBase NSB_;
			private NinjaTrader.Gui.NinjaScript.IndicatorRenderBase IRB_;  
			private static string nl = Environment.NewLine;
			private static SimpleFont font_ = new SimpleFont("Arial", 11);  //ChartControl.Properties.LabelFont
            private int BIP;
            private Dictionary<int, int> countCycles = new Dictionary<int, int>();
            private  Dictionary<int, double>  
                startAmount = new Dictionary<int, double>(), 
                diffAmount = new Dictionary<int, double>(), 
                totalDiffAmount = new Dictionary<int, double>(), 
                goneFromStart = new Dictionary<int, double>();


            private int totalDiffAmount_ALL;
            //private static Dictionary<string, Dictionary<string, string>> stopWatchTextAll = new Dictionary<string, Dictionary<string, string>>();
            void createSW() { if (sw != null) return;  sw = new System.Diagnostics.Stopwatch(); sw.Start(); }


            public StopWatchForMethod_Class(NinjaTrader.NinjaScript.NinjaScriptBase _NSB )
            {
                NSB_ = _NSB;
                IRB_ = NSB_ as NinjaTrader.Gui.NinjaScript.IndicatorRenderBase;
                // if (stopWatchTextAll.ContainsKey(NSB_.Name)) stopWatchTextAll[NSB_.Name].Clear();
            }

            int fCount;
            public void start(string name, int BIP)
            {
                if (!allowedToWork) return; 
                createSW();
                startAmount[BIP] = sw.Elapsed.TotalSeconds;
            }

            public void stopDraw(string name, int BIP, string additionalText)
            {
                if (!allowedToWork) return;
                stop(name, BIP);
                draw(name, additionalText, BIP);
            }

            public void stop(string name, int BIP)
            {
                if (!allowedToWork) return;
                if (!countCycles.ContainsKey(BIP)) {countCycles[BIP] = 0; }
                countCycles[BIP] = countCycles[BIP] + 1; ;
                diffAmount[BIP] = sw.Elapsed.TotalSeconds - startAmount[BIP];
                totalDiffAmount[BIP] = (totalDiffAmount.ContainsKey(BIP) ? totalDiffAmount[BIP] : 0) + diffAmount[BIP];
                goneFromStart[BIP] = sw.Elapsed.TotalSeconds;
            }

            string chartHandle = "";
            public void draw(string funcName, string additionalText, int BIP)
            {
                if (!allowedToWork) return; 
                if (NSB_.Bars==null || NSB_.CurrentBar < NSB_.Bars.Count - 2) return;   //barsArray[0] in rare cases gives error 

                double goneFromStart_ALL = goneFromStart[BIP];



                //reset-recollect
                double diffAmount_ALL = 0;
                double totalDiffAmount_ALL = 0;
                double countALL = 0;

                /*
                for (var i=0; i< countCycles.Count; i++) //same as count.Count  NSB_.BarsArray.Length
                {
                    p("cur count" + countCycles[i]);
                    countALL += countCycles[i];
                    diffAmount_ALL += diffAmount[i];
                    totalDiffAmount_ALL += totalDiffAmount[i];
                }
                */
                 
                countALL += countCycles[BIP];
                diffAmount_ALL += diffAmount[BIP];
                totalDiffAmount_ALL += totalDiffAmount[BIP];
                bool ignore = goneFromStart_ALL < 5;
                string txt_ =
                    string.Format
                    (
                    " % {6}  |  {0}=> {1}; (BIP:" + BIP + "; cycle: {2}): {3} s [total: {4}; Elapsed: {5} ]{7}",
                    NSB_.Name,
                    textLengthen(funcName, 14, "-"),
                    textLengthen(countALL.ToString(), 5, " "),
                    ignore ? "" : diffAmount_ALL.ToString("0.######"),
                    ignore ? "" : totalDiffAmount_ALL.ToString("0.######"),
                    ignore ? "" : goneFromStart_ALL.ToString("0.######"),
                    ignore ? "" : Math.Round((totalDiffAmount_ALL / goneFromStart_ALL) * 100, 2).ToString(),
                    (additionalText == "" ? "" : " (" + additionalText.ToString() + ")")
                    ); 

                message1();

                 
                chartHandle = getChartHandle(NSB_, false);
                if (NinjaTrader.NinjaScript.AddOns.TradingCodes.MethodsTimingMeasurementWindow.MainForm !=null)
                {
                    if (!NinjaTrader.NinjaScript.AddOns.TradingCodes.MethodsTimingMeasurementWindow.MainForm.textboxGroups.ContainsKey(chartHandle))
                        NinjaTrader.NinjaScript.AddOns.TradingCodes.MethodsTimingMeasurementWindow.MainForm.textboxGroups[chartHandle] = new Dictionary<string, string>();
                    NinjaTrader.NinjaScript.AddOns.TradingCodes.MethodsTimingMeasurementWindow.MainForm.textboxGroups[chartHandle][NSB_.Name + "_"+funcName + "_" + BIP] = txt_;
                }
                //     NinjaTrader.NinjaScript.DrawingTools.Draw.TextFixed(NSB_, "stopWatch_" + NSB_.Name + funcName_[BIP],
                //         txt_,
                //         NinjaTrader.NinjaScript.DrawingTools.TextPosition.TopLeft,
                //         Brushes.Yellow,
                //         font_,
                //         Brushes.Transparent,
                //         Brushes.Transparent, 0
                //    ).YPixelOffset = -(int)(font_.Size * offset_ + font_.Size * 4 * (DebugNumber_ - 1));
                //(NSB_ as NinjaTrader.Gui.NinjaScript.IndicatorRenderBase).ForceRefresh();
            }


            //private void p(object x) { NSB_.Print(x == null ? "null" : x.ToString()); }
            private void p(object x) { print(x);  }
            public static void print(object text) { NinjaTrader.Code.Output.Process(text == null ? "null" : text.ToString(), PrintTo.OutputTab1); }
            public static string textLengthen(string txt, int char_count, string letter) { while (txt.Length < char_count) txt += letter; return txt; }

            // alternative/older way: https://pastebin.com/MsrAHdUt
        }
    }
    #endregion
}








// ############ BONUS ############ //
#region Shorthand for measuring the action time
// i.e. PrintTiming( (()=>{ myAction... }) ;
namespace NinjaTrader.NinjaScript.Indicators
{
    public partial class Indicator 
    {
        public virtual void PrintTiming(Action func)
        {
			var a=DateTime.Now;
			for (var i=0; i<10000; i++)
				func();
			var b=DateTime.Now;
			Print("Timings diff:"+  (b-a).TotalMilliseconds);
        }

        protected DateTime PrintTiming_dt;
        public virtual void PrintTiming(bool startEnd)
        {
            if(startEnd) PrintTiming_dt = DateTime.Now;
            else Print("Timings Start/End diff:" + (DateTime.Now - PrintTiming_dt).TotalMilliseconds);
        }
    }
}
#endregion