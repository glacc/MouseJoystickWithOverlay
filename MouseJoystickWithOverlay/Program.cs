// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2025 Glacc

using System.Runtime.Versioning;
using Vortice.DirectInput;

namespace MouseJoystickWithOverlay
{
    [SupportedOSPlatform("Windows")]
    internal class Program
    {
        public static volatile bool abortFlag = false;

        static void OverlayUpdateThread()
        {
            while (!abortFlag)
            {
                if (GDIJoystickOverlayForm.instance != null)
                {
                    if (GDIJoystickOverlayForm.instance.Visible)
                    {
                        GDIJoystickOverlayForm.RequestRedraw();
                        // GDIJoystickOverlay.instance?.SetToTopMost();
                    }
                }

                Thread.Sleep(15);
            }
        }

        static void ConfineCursor()
        {
            FocusChecker.mutex.WaitOne();

            Win32.RECT? winRect = FocusChecker.winRect;

            FocusChecker.mutex.ReleaseMutex();

            if (winRect == null)
                return;

            Win32.POINT point;
            if (!Win32.GetCursorPos(out point))
                return;

            Win32.RECT rectWindow = (Win32.RECT)winRect;

            int mouseX = point.x;
            int mouseY = point.y;

            if (mouseX < rectWindow.left)
                mouseX = rectWindow.left;
            if (mouseX > rectWindow.right)
                mouseX = rectWindow.right;

            if (mouseY < rectWindow.top)
                mouseY = rectWindow.top;
            if (mouseY > rectWindow.bottom)
                mouseY = rectWindow.bottom;

            Win32.SetCursorPos(mouseX, mouseY);
        }

        static void FocusMonitorThread()
        {
            while (!abortFlag)
            {
                FocusChecker.Update();
                Thread.Sleep(1000);
            }
        }

        static void SeperateThread()
        {
            bool keyLockHold = false;
            bool keyToggleHold = false;

            while (!abortFlag)
            {
                if (GDIJoystickOverlayForm.instance != null)
                    GDIJoystickOverlayForm.ChangeVisibility(FocusChecker.focusedTargetHwnd != IntPtr.Zero);

                ConfineCursor();

                InputHandling.PollKeyboardInput();

                if (FocusChecker.focusedTargetHwnd != IntPtr.Zero)
                {
                    if (InputHandling.pressedKeys.Contains(Key.C))
                    {
                        if (!keyLockHold)
                        {
                            if (MouseJoystick.enabled)
                                MouseJoystick.locked = !MouseJoystick.locked;
                        }

                        keyLockHold = true;
                    }
                    else
                        keyLockHold = false;

                    if ((InputHandling.pressedKeys.Contains(Key.LeftAlt) || InputHandling.pressedKeys.Contains(Key.RightAlt)) &&
                        InputHandling.pressedKeys.Contains(Key.N))
                    {
                        MouseJoystick.blockXAxis = true;

                        if (!keyToggleHold)
                        {
                            MouseJoystick.enabled = !MouseJoystick.enabled;
                            MouseJoystick.locked = false;
                        }

                        keyToggleHold = true;
                    }
                    else
                    {
                        MouseJoystick.blockXAxis = false;

                        keyToggleHold = false;
                    }
                }

                MouseJoystick.Update();

                if (abortFlag)
                    break;

                Thread.Sleep(10);
            }
        }

        static void Main(string[] args)
        {
            GDIJoystickOverlayForm.Run();

            Thread overlayUpdateThread = new Thread(OverlayUpdateThread);
            overlayUpdateThread.Start();

            Thread focusMonitorThread = new Thread(FocusMonitorThread);
            focusMonitorThread.Start();

            Thread seperateThread = new Thread(SeperateThread);
            seperateThread.Start();
        }
    }
}
