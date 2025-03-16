// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2025 Glacc

using System.Drawing;
using System.Runtime.Versioning;

namespace MouseJoystickWithOverlay
{
    [SupportedOSPlatform("Windows")]
    internal class Program
    {
        static volatile bool abortFlag = false;

        static void GDIHudUpdate()
        {

        }

        static void SeperateThread()
        {
            IntPtr lastFocusedTargetHwnd = FocusedHwndChecker.focusedTargetHwnd;

            while (!abortFlag)
            {
                FocusedHwndChecker.Update();

                if (FocusedHwndChecker.focusedTargetHwnd != IntPtr.Zero)
                {
                    Import.RECT rectWindow;
                    bool getWindowRectSuccess = Import.GetWindowRect(FocusedHwndChecker.focusedTargetHwnd, out rectWindow);

                    /*
                    if (lastFocusedTargetHwnd != FocusedHwndChecker.focusedTargetHwnd)
                    {
                        Console.WriteLine($"Target hwnd focused: {FocusedHwndChecker.focusedTargetHwnd}, {Import.IsWindow(FocusedHwndChecker.focusedTargetHwnd)}");

                        if (getWindowRectSuccess)
                            Console.WriteLine($"Rect: {rectWindow.left}, {rectWindow.top}, {rectWindow.right}, {rectWindow.bottom}");
                    }
                    */

                    // Confining the cursor inside the game window
                    if (getWindowRectSuccess)
                    {
                        Import.POINT point;
                        bool getCursorPosSuccess = Import.GetCursorPos(out point);

                        if (getCursorPosSuccess)
                        {
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

                            Import.SetCursorPos(mouseX, mouseY);
                        }
                    }
                }

                lastFocusedTargetHwnd = FocusedHwndChecker.focusedTargetHwnd;

                MouseJoystick.Update();

                if (abortFlag)
                    break;

                Thread.Sleep(10);
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            Thread seperateThread = new Thread(SeperateThread);
            seperateThread.Start();

            while (true)
            {
                string input = Console.ReadLine() ?? "";

                if (input == "exit")
                {
                    abortFlag = true;

                    while (seperateThread.IsAlive) ;

                    break;
                }
            }
        }
    }
}
