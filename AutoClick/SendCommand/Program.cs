using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SendCommand
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length == 1 && args[0].Equals("-list"))
            {
                var ps = Process.GetProcesses();
                foreach (var pp in ps)
                {
                    System.Console.WriteLine(pp.ProcessName);
                }
                return;
            }


            if (args.Length < 2)
            {


                System.Console.WriteLine("Not enough arguments");
                System.Console.WriteLine();
                System.Console.WriteLine("Usage: 'SendCommand <ProcessName> <Key1> <Key2> <Key3> ...");
                System.Console.WriteLine("<Key> can be a letter or quote-enclosed string, or {TAB} {ENTER} {ESC} {F1} {F12}");
                System.Console.WriteLine("Modifier keys: Shift: +, Ctrl: ^ Alt: %");
                System.Console.WriteLine();
                System.Console.WriteLine("Example: 'SendCommand notepad \"Hello World\" ^s %{F4}' will send 'Hello world, follewed by CTRL-s and ALT-F4 to a running notepad process.");
                System.Console.WriteLine();
                System.Console.WriteLine("Use 'SendCommand -list' to view a list of all running processes.");
                System.Console.WriteLine("See https://msdn.microsoft.com/en-us/library/system.windows.forms.sendkeys.aspx for all options");
                return;
            }




            
            Process[] foundProcesses = Process.GetProcessesByName(args[0]);


            if (foundProcesses.Length >0)
            {
                Process p = foundProcesses[0];

                IntPtr h = p.MainWindowHandle;
                SetForegroundWindow(h);

                for (int i = 1; i < args.Length; i++ ){
                    System.Console.WriteLine("Sending '{0}' to '{1}'.", args[i], p.ProcessName);
                    SendKeys.SendWait(args[i]);
                    Thread.Sleep(100); 
                }
            }
            else
            {
                System.Console.WriteLine("No such application: {0}", args[0]);
            }

            // keep open.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Console.ReadLine();
            }

        }


        [DllImport ("User32.dll")]
        static extern int SetForegroundWindow(IntPtr point);

    }
}
