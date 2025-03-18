// SPDX-License-Identifier: GPL-3.0-only
// Copyright (C) 2025 Glacc

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MouseJoystickWithOverlay
{
    internal class GDIJoystickOverlayForm : Form
    {
        const int GWL_EXSTYLE = -20;
        const int WS_EX_NOACTIVATE = 0x08000000;
        const int WS_EX_LAYERED = 0x00080000;
        const int WS_EX_TRANSPARENT = 0x00000020;
        const int WS_EX_TOPMOST = 0x00000008;

        const float joystickHudSizePercent = 0.75f;
        const float joystickHudBorderThickness = 4f;
        const float joystickHudCenterCircleThickness = 2f;

        const float centerCircleDiameter = 48f;
        const float centerCircleRadius = centerCircleDiameter / 2f;

        const float joystickPositionCircleDiameter = 24f;
        const float joystickPositionCircleRadius = joystickPositionCircleDiameter / 2f;

        static readonly Color colorNormal = Color.White;
        static readonly Color colorLocked = Color.Red;

        SolidBrush brushJoystickHud = new SolidBrush(colorNormal);

        Pen penJoystickHudBorder;
        Pen penJoystickHudCenter;

        public static GDIJoystickOverlayForm? instance = null;

        public static volatile bool isRunning = false;

        ContextMenuStrip contextMenu = new ContextMenuStrip();
        NotifyIcon notifyIcon = new NotifyIcon();

        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

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

        public GDIJoystickOverlayForm()
        {
            penJoystickHudBorder = new Pen(brushJoystickHud, joystickHudBorderThickness);
            penJoystickHudCenter = new Pen(brushJoystickHud, joystickHudCenterCircleThickness);

            #region WindowStyle

            DoubleBuffered = true;

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

            OnResize(new EventArgs());

            #endregion

            #region NotifyIcon

            notifyIcon = new NotifyIcon();
            notifyIcon.Text = "Mouse Joystick Overlay";
            notifyIcon.Icon = new Icon("./icon1.ico");

            ToolStripMenuItem menuItemExit = new ToolStripMenuItem("Exit");
            menuItemExit.Click += delegate
            {
                RequestClose();
            };
            contextMenu.Items.Add(menuItemExit);

            notifyIcon.ContextMenuStrip = contextMenu;

            notifyIcon.Visible = true;

            FormClosed += delegate
            {
                notifyIcon.Visible = false;
            };

            #endregion

            #region Timer

            timer.Interval = 2000;
            timer.Tick += SetToTopMost;
            timer.Start();

            #endregion
        }

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

            if (winRect != null && MouseJoystick.enabled)
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

                if (!MouseJoystick.locked)
                {
                    brushJoystickHud.Color =
                    penJoystickHudBorder.Color =
                    penJoystickHudCenter.Color = colorNormal;
                }
                else
                {
                    brushJoystickHud.Color =
                    penJoystickHudBorder.Color =
                    penJoystickHudCenter.Color = colorLocked;
                }

                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                graphics.DrawRectangle(penJoystickHudBorder, left, top, joystickHudSize, joystickHudSize);
                graphics.DrawEllipse(penJoystickHudCenter, centerX - centerCircleRadius, centerY - centerCircleRadius, centerCircleDiameter, centerCircleDiameter);
                graphics.FillEllipse(brushJoystickHud, joystickPositionCircleX, joystickPositionCircleY, joystickPositionCircleDiameter, joystickPositionCircleDiameter);
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
            => instance?.Invoke
            (
                () =>
                {
                    Program.abortFlag = true;

                    instance.Close();
                }
            );

        static void LauncherThread()
        {
            instance = new GDIJoystickOverlayForm();
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
