using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;


namespace AutoClick
{


    /// <summary>
    /// The Data passed to SendInput in an array.
    /// </summary>
    /// <remarks>Contains a union field type specifies what it contains </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct INPUT
    {
        /// <summary>
        /// The actual data type contained in the union Field
        /// </summary>
        public SendInputEventType type;
        public MouseKeybdhardwareInputUnion mkhi;
    }


    /// <summary>
    /// The event type contained in the union field
    /// </summary>
    public enum SendInputEventType : int
    {
        /// <summary>
        /// Contains Mouse event data
        /// </summary>
        InputMouse,
        /// <summary>
        /// Contains Keyboard event data
        /// </summary>
        InputKeyboard,
        /// <summary>
        /// Contains Hardware event data
        /// </summary>
        InputHardware
    }

    /// <summary>
    /// Captures the union of the three three structures.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct MouseKeybdhardwareInputUnion
    {
        /// <summary>
        /// The Mouse Input Data
        /// </summary>
        [FieldOffset(0)]
        public MouseInputData mi;

        /// <summary>
        /// The Keyboard input data
        /// </summary>
        [FieldOffset(0)]
        public KEYBDINPUT ki;

        /// <summary>
        /// The hardware input data
        /// </summary>
        [FieldOffset(0)]
        public HARDWAREINPUT hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HARDWAREINPUT
    {
        public int uMsg;
        public short wParamL;
        public short wParamH;
    }

    /// <summary>
    /// The mouse data structure
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MouseInputData
    {
        /// <summary>
        /// The x value, if ABSOLUTE is passed in the flag then this is an actual X and Y value
        /// otherwise it is a delta from the last position
        /// </summary>
        public int dx;
        /// <summary>
        /// The y value, if ABSOLUTE is passed in the flag then this is an actual X and Y value
        /// otherwise it is a delta from the last position
        /// </summary>
        public int dy;
        /// <summary>
        /// Wheel event data, X buttons
        /// </summary>
        public uint mouseData;
        /// <summary>
        /// ORable field with the various flags about buttons and nature of event
        /// </summary>
        public MouseEventFlags dwFlags;
        /// <summary>
        /// The timestamp for the event, if zero then the system will provide
        /// </summary>
        public uint time;
        /// <summary>
        /// Additional data obtained by calling app via GetMessageExtraInfo
        /// </summary>
        public IntPtr dwExtraInfo;
    }



    [Flags]
    public enum MouseEventFlags
    {
        LEFTDOWN = 0x00000002,
        LEFTUP = 0x00000004,
        MIDDLEDOWN = 0x00000020,
        MIDDLEUP = 0x00000040,
        MOVE = 0x00000001,
        ABSOLUTE = 0x00008000,
        RIGHTDOWN = 0x00000008,
        RIGHTUP = 0x00000010
    }


    public class HotKey : IDisposable
    {
        private static Dictionary<int, HotKey> _dictHotKeyToCalBackProc;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, UInt32 fsModifiers, UInt32 vlc);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public const int WmHotKey = 0x0312;

        private bool _disposed = false;

        public Key Key { get; private set; }
        public KeyModifier KeyModifiers { get; private set; }
        public Action<HotKey> Action { get; private set; }
        public int Id { get; set; }

        // ******************************************************************
        public HotKey(Key k, KeyModifier keyModifiers, Action<HotKey> action, bool register = true)
        {
            Key = k;
            KeyModifiers = keyModifiers;
            Action = action;
            if (register)
            {
                Register();
            }
        }

        // ******************************************************************
        public bool Register()
        {
            int virtualKeyCode = KeyInterop.VirtualKeyFromKey(Key);
            Id = virtualKeyCode + ((int)KeyModifiers * 0x10000);
            bool result = RegisterHotKey(IntPtr.Zero, Id, (UInt32)KeyModifiers, (UInt32)virtualKeyCode);

            if (_dictHotKeyToCalBackProc == null)
            {
                _dictHotKeyToCalBackProc = new Dictionary<int, HotKey>();
                ComponentDispatcher.ThreadFilterMessage += new ThreadMessageEventHandler(ComponentDispatcherThreadFilterMessage);
            }

            _dictHotKeyToCalBackProc.Add(Id, this);

            Debug.Print(result.ToString() + ", " + Id + ", " + virtualKeyCode);
            return result;
        }

        // ******************************************************************
        public void Unregister()
        {
            HotKey hotKey;
            if (_dictHotKeyToCalBackProc.TryGetValue(Id, out hotKey))
            {
                UnregisterHotKey(IntPtr.Zero, Id);
            }
        }

        // ******************************************************************
        private static void ComponentDispatcherThreadFilterMessage(ref MSG msg, ref bool handled)
        {
            if (!handled)
            {
                if (msg.message == WmHotKey)
                {
                    HotKey hotKey;

                    if (_dictHotKeyToCalBackProc.TryGetValue((int)msg.wParam, out hotKey))
                    {
                        if (hotKey.Action != null)
                        {
                            hotKey.Action.Invoke(hotKey);
                        }
                        handled = true;
                    }
                }
            }
        }

        // ******************************************************************
        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // ******************************************************************
        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be _disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be _disposed.
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this._disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    Unregister();
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }
    }

    // ******************************************************************
    [Flags]
    public enum KeyModifier
    {
        None = 0x0000,
        Alt = 0x0001,
        Ctrl = 0x0002,
        NoRepeat = 0x4000,
        Shift = 0x0004,
        Win = 0x0008
    }

    // ******************************************************************
}

