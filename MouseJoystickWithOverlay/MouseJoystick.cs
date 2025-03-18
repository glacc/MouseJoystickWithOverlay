// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2025 Glacc

using vJoyInterfaceWrap;
using System.Runtime.Versioning;

namespace MouseJoystickWithOverlay
{
    [SupportedOSPlatform("Windows")]
    internal class MouseJoystick
    {
        public static bool enabled = true;
        public static bool locked = false;

        static vJoy joystick = new vJoy();
        static uint joystickId = 1;

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

        public static void Update()
        {
            int deltaX, deltaY;

            (deltaX, deltaY) = InputHandling.PollMouseInput();

            if (enabled)
            {
                if (!locked)
                {
                    if (!InputHandling.rmbPressed)
                    {
                        mouseX += deltaX;
                        mouseY += deltaY;
                    }
                }
            }
            else
            {
                mouseX = mouseXMax / 2;
                mouseY = mouseYMax / 2;
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
        }
    }
}
