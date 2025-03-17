using Microsoft.Win32;
// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2025 Glacc

using System.Drawing;

namespace MouseJoystickWithOverlay
{
    internal class GDIJoystickOverlay : Form
    {
        const float joystickHudSizePercent = 0.75f;
        const float joystickHudBorderThickness = 8f;
        const float joystickHudCenterCircleThickness = 3f;

        const float centerCircleDiameter = 48f;
        const float centerCircleRadius = centerCircleDiameter / 2f;

        const float joystickPositionCircleDiameter = 32f;
        const float joystickPositionCircleRadius = joystickPositionCircleDiameter / 2f;

        static Pen penJoystickHudBorder = new Pen(Brushes.White, joystickHudBorderThickness);
        static Pen penJoystickHudCenter = new Pen(Brushes.White, joystickHudCenterCircleThickness);

        public static GDIJoystickOverlay? instance = null;

        public static volatile bool isRunning = false;

        const int GWL_EXSTYLE = -20;
        const int WS_EX_NOACTIVATE = 0x08000000;
        const int WS_EX_LAYERED = 0x00080000;
        const int WS_EX_TRANSPARENT = 0x00000020;
        const int WS_EX_TOPMOST = 0x00000008;

        System.Windows.Forms.Timer timer;

        protected override bool ShowWithoutActivation
        {
            get => true;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;

                createParams.ExStyle |= WS_EX_NOACTIVATE;

                return createParams;
            }
        }

        public GDIJoystickOverlay()
        {
            DoubleBuffered = true;

            // StartPosition = FormStartPosition.CenterScreen;

            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;

            TopMost = true;
            Visible = false;
            ShowInTaskbar = false;

            BackColor = Color.Black;
            TransparencyKey = Color.Black;

            SetStyle
            (
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.Opaque,
                true
            );

            int initialStyle = Win32.GetWindowLong(Handle, GWL_EXSTYLE);
            Win32.SetWindowLong(Handle, GWL_EXSTYLE, initialStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOPMOST);

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000;
            timer.Tick += SetToTopMost;
            timer.Start();

            OnResize(new EventArgs());
        }

        /*
        protected override void WndProc(ref Message m)
        {
            const int WM_ERASEBKGND = 0x0014;
            const int WM_NCPAINT = 0x0085;

            switch (m.Msg)
            {
                case WM_ERASEBKGND:
                    m.Result = IntPtr.Zero;
                    return;

                case WM_NCPAINT:
                    m.Result = IntPtr.Zero;
                    return;
            }

            base.WndProc(ref m);
        }
        */

        void SetToTopMost(object? sender, EventArgs args)
        {
            if (InvokeRequired)
            {
                Invoke(SetToTopMost);
                return;
            }

            if (!Visible)
                return;

            Win32.SetWindowPos
            (
                Handle,
                Win32.HWND_TOPMOST,
                0, 0, 0, 0,
                Win32.SWP_NOMOVE | Win32.SWP_NOSIZE | Win32.SWP_NOACTIVATE
            );
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            FocusChecker.mutex.WaitOne();

            Win32.RECT? winRect = FocusChecker.winRect;

            FocusChecker.mutex.ReleaseMutex();

            if (winRect != null)
            {
                Win32.RECT windowRect = (Win32.RECT)winRect;

                float centerX = (windowRect.left + windowRect.right) / 2f;
                float centerY = (windowRect.top + windowRect.bottom) / 2f;

                float winWidth = windowRect.right - windowRect.left;
                float winHeight = windowRect.bottom - windowRect.top;

                float shortestDim = MathF.Min(winWidth, winHeight);
                float joystickHudSize = shortestDim * joystickHudSizePercent;
                float halfJoystickHudSize = joystickHudSize / 2f;

                float left = centerX - halfJoystickHudSize;
                float right = centerX + halfJoystickHudSize;
                float top = centerY - halfJoystickHudSize;
                float bottom = centerY + halfJoystickHudSize;

                float joystickPositionCircleX = left + ((float)MouseJoystick.mouseX / MouseJoystick.mouseXMax) * joystickHudSize - joystickPositionCircleRadius;
                float joystickPositionCircleY = top + ((float)MouseJoystick.mouseY / MouseJoystick.mouseYMax) * joystickHudSize - joystickPositionCircleRadius;

                Graphics graphics = e.Graphics;

                graphics.DrawRectangle(penJoystickHudBorder, left, top, joystickHudSize, joystickHudSize);
                graphics.DrawEllipse(penJoystickHudCenter, centerX - centerCircleRadius, centerY - centerCircleRadius, centerCircleDiameter, centerCircleDiameter);
                graphics.FillEllipse(Brushes.White, joystickPositionCircleX, joystickPositionCircleY, joystickPositionCircleDiameter, joystickPositionCircleDiameter);
            }

            int[] margins = [0, 0, Width, Height];
            Win32.DwmExtendFrameIntoClientArea(Handle, ref margins);
        }

        protected override void OnResize(EventArgs e)
        {
            int[] margins = [0, 0, Width, Height];
            Win32.DwmExtendFrameIntoClientArea(Handle, ref margins);
        }

        protected override void OnClosed(EventArgs e)
        {
            isRunning = false;

            base.OnClosed(e);
        }

        public static void RequestRedraw()
            => instance?.Invoke(() => instance.Invalidate());

        public static void ChangeVisibility(bool visable)
            => instance?.Invoke
            (
                (bool newVisibility) =>
                {
                    if (newVisibility != instance.Visible)
                        instance.Visible = newVisibility;
                },
                visable
            );

        public static void RequestClose()
            => instance?.Invoke(() => instance.Close());

        static void LauncherThread()
        {
            instance = new GDIJoystickOverlay();
            Application.Run(instance);

            isRunning = false;
        }

        public static void Run()
        {
            Thread launcher = new Thread(LauncherThread);
            launcher.SetApartmentState(ApartmentState.STA);
            launcher.Start();

            isRunning = true;
        }
    }
}
