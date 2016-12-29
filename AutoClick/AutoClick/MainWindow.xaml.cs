using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AutoClick
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HotKey hk;
        private int _count = 100;
        private int _delay = 10;

        public MainWindow()
        {
            InitializeComponent();
            hk = new HotKey(Key.NumPad0, KeyModifier.Alt, HotKeyClick, true);
        }
        

        private void HotKeyClick(HotKey hk)
        {
            for (int i = 0; i < _count; i++)
            {
                Click();
                Thread.Sleep(_delay);
            }
                
        }

        private void Click()
        {
            INPUT structInput = new INPUT();
            structInput.type = SendInputEventType.InputMouse;
            structInput.mkhi.mi.dwFlags =  MouseEventFlags.LEFTDOWN | MouseEventFlags.LEFTUP;
            structInput.mkhi.mi.dx = 0;
            structInput.mkhi.mi.dy = 0;
            uint i = SendInput(1, ref structInput, Marshal.SizeOf(new INPUT()));
        }


        /// <summary>
        /// Synthesizes keystrokes, mouse motions, and button clicks.
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern uint SendInput(uint nInputs,
           ref INPUT pInputs,
           int cbSize);

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int value = 100;
            if (!Int32.TryParse(ClickCount.Text, out value))
            {
                ClickCount.Text = "100";
                _count = 100;
            }
            else
            {
                _count = value;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }



        


    }
}
