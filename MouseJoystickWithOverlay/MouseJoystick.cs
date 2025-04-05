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

        public static volatile bool blockXAxis = false;

        static vJoy joystick = new vJoy();
        static uint joystickId = 1;

        // Joystick

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

        // Head

        const int mouseRXMax = 1024;
        const int mouseRYMax = 1024;

        static int mouseRX = mouseRXMax / 2;
        static int mouseRY = mouseRYMax / 2;

        // Will be updated in runtime

        static long joystickXMax = 0;
        static long joystickYMax = 0;
        static long joystickZMax = 0;

        static long joystickRXMax = 0;
        static long joystickRYMax = 0;
        static long joystickRZMax = 0;

        static void ClampMousePositionAndUpdateJoystick()
        {
            mouseX = Math.Clamp(mouseX, 0, mouseXMax);
            mouseY = Math.Clamp(mouseY, 0, mouseYMax);

            // Aileron & Rudder
            if (!blockXAxis)
                joystick.SetAxis((int)(mouseX / (float)mouseXMax * joystickXMax), joystickId, HID_USAGES.HID_USAGE_X);
            else
                joystick.SetAxis((int)(joystickXMax / 2), joystickId, HID_USAGES.HID_USAGE_X);

            joystick.SetAxis((int)(mouseX / (float)mouseXMax * joystickZMax), joystickId, HID_USAGES.HID_USAGE_Z);

            // Elevator
            joystick.SetAxis((int)(mouseY / (float)mouseYMax * joystickYMax), joystickId, HID_USAGES.HID_USAGE_Y);
        }

        static void UpdateHeadRotation(int deltaX, int deltaY)
        {
            mouseRX += deltaX;
            mouseRY += deltaY;

            mouseRX = Math.Clamp(mouseRX, 0, mouseRXMax);
            mouseRY = Math.Clamp(mouseRY, 0, mouseRYMax);

            joystick.SetAxis((int)(mouseRX / (float)mouseRXMax * joystickRXMax), joystickId, HID_USAGES.HID_USAGE_RX);
            joystick.SetAxis((int)(mouseRY / (float)mouseRYMax * joystickRYMax), joystickId, HID_USAGES.HID_USAGE_RY);
        }

        public static void ResetHeadPosition()
        {
            mouseRX = mouseRXMax / 2;
            mouseRY = mouseRYMax / 2;
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

                /*
                if (InputHandling.rmbPressed)
                    UpdateHeadRotation(deltaX, deltaY);
                */
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

            joystick.GetVJDAxisMax(joystickId, HID_USAGES.HID_USAGE_RX, ref joystickRXMax);
            joystick.GetVJDAxisMax(joystickId, HID_USAGES.HID_USAGE_RY, ref joystickRYMax);
            joystick.GetVJDAxisMax(joystickId, HID_USAGES.HID_USAGE_RZ, ref joystickRZMax);
        }
    }
}
