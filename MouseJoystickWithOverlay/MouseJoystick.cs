// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2025 Glacc

using vJoyInterfaceWrap;
using System.Runtime.Versioning;

namespace MouseJoystickWithOverlay
{
    [SupportedOSPlatform("Windows")]
    internal class MouseJoystick
    {
        static vJoy joystick = new vJoy();
        const int joystickId = 1;

        // static int joystickTick = 0;

        public const int mouseXMax = 1024;
        public const int mouseYMax = 1024;

        static int m_mouseX = mouseXMax / 2;
        static int m_mouseY = mouseYMax / 2;
        public static int mouseX
        {
            get => m_mouseX;
            private set => m_mouseX = value;
        }
        public static int mouseY
        {
            get => m_mouseY;
            private set => m_mouseY = value;
        }

        // Will be updated in runtime
        static long joystickXMax = 0;
        static long joystickYMax = 0;
        static long joystickZMax = 0;

        static void ClampMousePositionAndUpdateJoystick()
        {
            mouseX = Math.Clamp(mouseX, 0, mouseXMax);
            mouseY = Math.Clamp(mouseY, 0, mouseYMax);

            // Aileron & Rudder
            joystick.SetAxis((int)(mouseX / (float)mouseXMax * joystickXMax), joystickId, HID_USAGES.HID_USAGE_X);
            joystick.SetAxis((int)(mouseX / (float)mouseXMax * joystickZMax), joystickId, HID_USAGES.HID_USAGE_Z);

            // Elevator
            joystick.SetAxis((int)(mouseY / (float)mouseYMax * joystickYMax), joystickId, HID_USAGES.HID_USAGE_Y);
        }

        /*
        static void OnRawInput(object? sender, RawInputEventArgs args)
        {
            RawInputData inputData = args.Data;

            RawInputMouseData? mouseInputData = inputData as RawInputMouseData;
            if (mouseInputData != null)
            {
                mouseX += mouseInputData.Mouse.LastX;
                mouseY += mouseInputData.Mouse.LastY;
            }

            ClampMousePositionAndUpdateJoystick();
        }
        */

        public static void Update()
        {
            /*
            long maxVal = 0;
            joystick.GetVJDAxisMax(joystickId, HID_USAGES.HID_USAGE_Y, ref maxVal);

            joystick.SetAxis((int)((MathF.Sin(joystickTick / 90f * MathF.PI) + 1f) / 2f * maxVal), joystickId, HID_USAGES.HID_USAGE_Y);

            joystickTick++;
            */

            int deltaX, deltaY;

            (deltaX, deltaY) = MouseInputHandling.Poll();

            if (!MouseInputHandling.rmbPressed)
            {
                mouseX += deltaX;
                mouseY += deltaY;
            }

            ClampMousePositionAndUpdateJoystick();
        }

        static MouseJoystick()
        {
            joystick.AcquireVJD(joystickId);

            joystick.ResetVJD(joystickId);

            joystick.GetVJDAxisMax(joystickId, HID_USAGES.HID_USAGE_X, ref joystickXMax);
            joystick.GetVJDAxisMax(joystickId, HID_USAGES.HID_USAGE_Y, ref joystickYMax);
            joystick.GetVJDAxisMax(joystickId, HID_USAGES.HID_USAGE_Z, ref joystickZMax);

            /*
            RawInputHandling.onInput += OnRawInput;

            RawInputHandling.Start();
            */

            MouseInputHandling.Init();
        }
    }
}
