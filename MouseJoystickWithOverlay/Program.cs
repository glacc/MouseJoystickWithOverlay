// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2025 Glacc

using System.Runtime.Versioning;

namespace MouseJoystickWithOverlay
{
    [SupportedOSPlatform("Windows")]
    internal class Program
    {
        static volatile bool abortFlag = false;

        static void OverlayUpdateThread()
        {
            while (!abortFlag)
            {
                if (GDIJoystickOverlay.instance != null)
                {
                    if (GDIJoystickOverlay.instance.Visible)
                    {
                        GDIJoystickOverlay.RequestRedraw();
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
            while (!abortFlag)
            {
                if (GDIJoystickOverlay.instance != null)
                    // GDIJoystickOverlay.ChangeVisibility(FocusChecker.focusedTargetHwnd != IntPtr.Zero);
                    GDIJoystickOverlay.ChangeVisibility(true);

                ConfineCursor();

                MouseJoystick.Update();

                if (abortFlag)
                    break;

                Thread.Sleep(15);
            }
        }

        static void Main(string[] args)
        {
            GDIJoystickOverlay.Run();

            Thread overlayUpdateThread = new Thread(OverlayUpdateThread);
            overlayUpdateThread.Start();

            Thread focusMonitorThread = new Thread(FocusMonitorThread);
            focusMonitorThread.Start();

            Thread seperateThread = new Thread(SeperateThread);
            seperateThread.Start();

            while (true)
            {
                string input = Console.ReadLine() ?? "";

                if (input == "exit")
                {
                    abortFlag = true;

                    while
                    (
                        seperateThread.IsAlive ||
                        focusMonitorThread.IsAlive
                    ) ;

                    break;
                }

                Thread.Sleep(250);
            }

            GDIJoystickOverlay.RequestClose();
        }
    }
}
