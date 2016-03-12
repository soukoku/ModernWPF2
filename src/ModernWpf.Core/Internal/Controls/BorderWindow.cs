﻿using ModernWpf.Converters;
using ModernWpf.Native;
using ModernWpf.Native.Api;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Provides a single side of the resize/glow border window.
    /// </summary>
    class BorderWindow : Window
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static BorderWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BorderWindow), new FrameworkPropertyMetadata(typeof(BorderWindow)));
            WindowStyleProperty.OverrideMetadata(typeof(BorderWindow), new FrameworkPropertyMetadata(WindowStyle.None));
            ShowInTaskbarProperty.OverrideMetadata(typeof(BorderWindow), new FrameworkPropertyMetadata(false));
            AllowsTransparencyProperty.OverrideMetadata(typeof(BorderWindow), new FrameworkPropertyMetadata(true));
            ShowActivatedProperty.OverrideMetadata(typeof(BorderWindow), new FrameworkPropertyMetadata(false));
            ResizeModeProperty.OverrideMetadata(typeof(BorderWindow), new FrameworkPropertyMetadata(ResizeMode.NoResize));
            BorderBrushProperty.OverrideMetadata(typeof(BorderWindow), new FrameworkPropertyMetadata(Brushes.DimGray));
            // override to make border less visible initially for slow machines
            WidthProperty.OverrideMetadata(typeof(BorderWindow), new FrameworkPropertyMetadata(1d));
            HeightProperty.OverrideMetadata(typeof(BorderWindow), new FrameworkPropertyMetadata(1d));
        }

        #region DPs

        public BorderSide Side
        {
            get { return (BorderSide)GetValue(SideProperty); }
            set { SetValue(SideProperty, value); }
        }

        public static readonly DependencyProperty SideProperty =
            DependencyProperty.Register("Side", typeof(BorderSide), typeof(BorderWindow), new FrameworkPropertyMetadata(BorderSide.Left));



        public bool IsContentActive
        {
            get { return (bool)GetValue(IsContentActiveProperty); }
            set { SetValue(IsContentActiveProperty, value); }
        }

        public static readonly DependencyProperty IsContentActiveProperty =
            DependencyProperty.Register("IsContentActive", typeof(bool), typeof(BorderWindow), new FrameworkPropertyMetadata(false, ActiveChanged));

        private static void ActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BorderWindow)d).UpdateBorderBrush();
        }

        /// <summary>
        /// Updates the border brush based on content window's active state.
        /// </summary>
        private void UpdateBorderBrush()
        {
            if (_chrome != null)
            {
                if (IsContentActive)
                {
                    GlowOpacity = .9;
                }
                else
                {
                    GlowOpacity = .5;
                }
                var test = BindingOperations.GetMultiBindingExpression(this, BorderBrushProperty);
                if (test != null)
                {
                    test.UpdateTarget();
                }
            }
        }

        public double BorderLength
        {
            get { return (double)GetValue(BorderLengthProperty); }
            set { SetValue(BorderLengthProperty, value); }
        }

        public static readonly DependencyProperty BorderLengthProperty =
            DependencyProperty.Register("BorderLength", typeof(double), typeof(BorderWindow), new FrameworkPropertyMetadata(1d));

        public double PadSize
        {
            get { return (double)GetValue(PadSizeProperty); }
            set { SetValue(PadSizeProperty, value); }
        }

        public static readonly DependencyProperty PadSizeProperty =
            DependencyProperty.Register("PadSize", typeof(double), typeof(BorderWindow), new FrameworkPropertyMetadata(8d));



        public double GlowOpacity
        {
            get { return (double)GetValue(GlowOpacityProperty); }
            set { SetValue(GlowOpacityProperty, value); }
        }

        public static readonly DependencyProperty GlowOpacityProperty =
            DependencyProperty.Register("GlowOpacity", typeof(double), typeof(BorderWindow), new FrameworkPropertyMetadata(0.9d));



        #endregion


        IntPtr _hwnd;
        BorderManager _manager;
        Chrome _chrome;
        public BorderWindow(BorderManager manager)
        {
            // only works if set directly, no in override
            this.Background = Brushes.Transparent;

            _manager = manager;

            CreateBinding(IsActiveProperty.Name, _manager.ContentWindow, IsContentActiveProperty);
            UpdateChromeBindings(Chrome.GetChrome(_manager.ContentWindow));
        }

        private Binding CreateBinding(string sourcePath, object source, DependencyProperty bindToProperty, IValueConverter converter = null, object converterParameter = null)
        {
            var bind = new Binding(sourcePath);
            bind.Source = source;
            bind.NotifyOnSourceUpdated = true;
            bind.Mode = BindingMode.OneWay;
            bind.Converter = converter;
            bind.ConverterParameter = converterParameter;
            BindingOperations.SetBinding(this, bindToProperty, bind);
            return bind;
        }

        internal void UpdateChromeBindings(Chrome chrome)
        {
            _chrome = chrome;
            CreateBinding(Chrome.ResizeBorderThicknessProperty.Name, _chrome, PadSizeProperty, ThicknessToDoubleConverter.Instance, Side);

            var brushBinding = new MultiBinding
            {
                Converter = new BorderBrushConverter(this),
                Mode = BindingMode.OneWay,
            };
            brushBinding.Bindings.Add(new Binding(Chrome.ActiveBorderBrushProperty.Name)
            {
                Source = _chrome,
            });
            brushBinding.Bindings.Add(new Binding(Chrome.InactiveBorderBrushProperty.Name)
            {
                Source = _chrome,
            });
            BindingOperations.SetBinding(this, BorderBrushProperty, brushBinding);
            UpdateBorderBrush();
        }

        protected override void OnClosed(EventArgs e)
        {
            BindingOperations.ClearAllBindings(this);
            base.OnClosed(e);
        }

        internal void UpdatePosn(double left, double top, double width, double height)
        {
            //User32.SetWindowPos(_hwnd, _manager.hWndContent, left, top, width, height, SetWindowPosOptions.SWP_NOACTIVATE);
            this.Left = left;
            this.Top = top;
            this.Width = width;
            this.Height = height;
            var pad = 2 * PadSize;
            switch (Side)
            {
                case BorderSide.Left:
                case BorderSide.Right:
                    BorderLength = Math.Max(0, height - pad);
                    break;
                case BorderSide.Top:
                case BorderSide.Bottom:
                    BorderLength = Math.Max(0, width - pad);
                    break;
            }

            //Debug.WriteLine("Side {0} W={1}, actual W={2}", Side, Width, ActualWidth);
        }

        internal void ShowNoActivate()
        {
            Show();
            User32.SetWindowPos(_hwnd, _manager.hWndContent, 0, 0, 0, 0,
                SetWindowPosOptions.SWP_NOMOVE | SetWindowPosOptions.SWP_NOSIZE | SetWindowPosOptions.SWP_NOACTIVATE);
        }


        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _hwnd = new WindowInteropHelper(this).Handle;
            var src = HwndSource.FromHwnd(_hwnd);
            src.AddHook(WndProc);

            ApplyWin32Stuff(_hwnd);
        }

        static void ApplyWin32Stuff(IntPtr hwnd)
        {
            var wsEx = User32.GetWindowLong(hwnd, WindowLong.GWL_EXSTYLE).ToInt32();
            wsEx |= (int)(WindowStylesEx.WS_EX_TOOLWINDOW | WindowStylesEx.WS_EX_NOACTIVATE);

            User32.SetWindowLong(hwnd, WindowLong.GWL_EXSTYLE, new IntPtr(wsEx));


            var ws = (uint)User32.GetWindowLong(hwnd, WindowLong.GWL_STYLE);
            // remove these to allow nchittest even when ResizeMode is NoResize
            ws &= ~(uint)(WindowStyles.WS_SYSMENU | WindowStyles.WS_OVERLAPPED);
            // todo: fix in 32bit world
            //ws |= (uint)(WindowStyles.WS_POPUP);

            User32.SetWindowLong(hwnd, WindowLong.GWL_STYLE, new IntPtr(ws));

            //// make resize more performant?
            //User32.SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0,
            //    SetWindowPosOptions.SWP_NOOWNERZORDER |
            //    SetWindowPosOptions.SWP_DRAWFRAME |
            //    SetWindowPosOptions.SWP_NOACTIVATE |
            //    SetWindowPosOptions.SWP_NOZORDER |
            //    SetWindowPosOptions.SWP_NOMOVE |
            //    SetWindowPosOptions.SWP_NOSIZE);
        }

        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            IntPtr retVal = IntPtr.Zero;
            if (!handled)
            {
                var wmsg = (WindowMessage)msg;
                //Debug.WriteLine(wmsg);
                switch (wmsg)
                {
                    case WindowMessage.WM_NCCALCSIZE:
                        handled = true;
                        break;
                    case WindowMessage.WM_NCHITTEST:
                        ChromeHitTest res = HandleHcHitTest(lParam);
                        retVal = new IntPtr((int)res);
                        handled = true;
                        break;
                    case WindowMessage.WM_NCRBUTTONDOWN:
                    case WindowMessage.WM_NCMBUTTONDOWN:
                    case WindowMessage.WM_NCRBUTTONDBLCLK:
                    case WindowMessage.WM_NCMBUTTONDBLCLK:
                    case WindowMessage.WM_RBUTTONDBLCLK:
                    case WindowMessage.WM_MBUTTONDBLCLK:
                    case WindowMessage.WM_LBUTTONDBLCLK:
                        handled = true;
                        User32.SetForegroundWindow(_manager.hWndContent);
                        break;
                    case WindowMessage.WM_NCLBUTTONDOWN:
                    case WindowMessage.WM_NCLBUTTONDBLCLK:
                        handled = true;
                        // pass resizer msg to the content window instead
                        User32.SetForegroundWindow(_manager.hWndContent);
                        User32.SendMessage(_manager.hWndContent, (uint)msg, wParam, lParam);
                        break;
                    case WindowMessage.WM_MOUSEACTIVATE:
                        var lowword = 0xffff & lParam.ToInt32();
                        var hchit = (ChromeHitTest)lowword;

                        // in case of non-resizable window eat this msg
                        if (hchit == ChromeHitTest.Client)
                        {
                            retVal = new IntPtr((int)MouseActivate.MA_NOACTIVATEANDEAT);
                        }
                        else
                        {
                            retVal = new IntPtr((int)MouseActivate.MA_NOACTIVATE);
                        }
                        handled = true;
                        break;
                    case WindowMessage.WM_GETMINMAXINFO:
                        // overridden so max size = normal max + resize border (for when content window is max size without maximizing)
                        var thick = 2 * (int)PadSize;

                        MINMAXINFO para = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
                        var orig = para.ptMaxTrackSize;
                        orig.x += thick;
                        orig.y += thick;
                        para.ptMaxTrackSize = orig;
                        Marshal.StructureToPtr(para, lParam, true);
                        break;
                }
            }
            return retVal;
        }

        private ChromeHitTest HandleHcHitTest(IntPtr lParam)
        {
            ChromeHitTest res = ChromeHitTest.Border;
            if (_manager.ContentWindow.ResizeMode == ResizeMode.CanResizeWithGrip ||
                _manager.ContentWindow.ResizeMode == ResizeMode.CanResize)
            {
                var pt = PointFromScreen(lParam.ToPoint());
                int diagSize = (int)(2 * PadSize);
                switch (Side)
                {
                    case BorderSide.Left:
                        if (pt.Y <= diagSize) { res = ChromeHitTest.TopLeft; }
                        else if (pt.Y >= Height - diagSize) { res = ChromeHitTest.BottomLeft; }
                        else { res = ChromeHitTest.Left; }
                        break;
                    case BorderSide.Top:
                        if (pt.X <= diagSize) { res = ChromeHitTest.TopLeft; }
                        else if (pt.X >= Width - diagSize) { res = ChromeHitTest.BottomRight; }
                        else { res = ChromeHitTest.Top; }
                        break;
                    case BorderSide.Right:
                        if (pt.Y <= diagSize) { res = ChromeHitTest.TopRight; }
                        else if (pt.Y >= Height - diagSize) { res = ChromeHitTest.BottomRight; }
                        else { res = ChromeHitTest.Right; }
                        break;
                    case BorderSide.Bottom:
                        if (pt.X <= diagSize) { res = ChromeHitTest.BottomLeft; }
                        else if (pt.X >= Width - diagSize) { res = ChromeHitTest.BottomRight; }
                        else { res = ChromeHitTest.Bottom; }
                        break;
                }
            }

            return res;
        }
    }
}
