﻿using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace AntennaChat.Views.Activity
{
    /// <summary>
    /// Displays a WebBrowser control over a given placement target element in a WPF Window.
    /// The owner window can be transparent, but not this one, due mixing DirectX and GDI drawing. 
    /// WebBrowserOverlayWF uses WinForms to avoid this limitation.
    /// </summary>
    public partial class WebBrowserOverlay : Window
    {
        FrameworkElement _placementTarget;

        public System.Windows.Forms.WebBrowser WebBrowser { get { return _wb; } }

        public WebBrowserOverlay(FrameworkElement placementTarget)
        {
            InitializeComponent();

            _placementTarget = placementTarget;
            Window owner = Window.GetWindow(placementTarget);
            Debug.Assert(owner != null);

            //owner.SizeChanged += delegate { OnSizeLocationChanged(); };
            owner.LocationChanged += delegate { OnSizeLocationChanged(); };
            _placementTarget.SizeChanged += delegate { OnSizeLocationChanged(); };

            if (owner.IsVisible)
            {
                Owner = owner;
                Show();
            }
            else
                owner.IsVisibleChanged += delegate
                {
                    if (owner.IsVisible)
                    {
                        Owner = owner;
                        Show();
                    }
                };

            //owner.LayoutUpdated += new EventHandler(OnOwnerLayoutUpdated);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            if (!e.Cancel)
                // Delayed call to avoid crash due to Window bug.
                Dispatcher.BeginInvoke((Action)delegate
                {
                    Owner.Close();
                });
        }

        void OnSizeLocationChanged()
        {
            Point offset = _placementTarget.TranslatePoint(new Point(), Owner);
            Point size = new Point(_placementTarget.ActualWidth, _placementTarget.ActualHeight);
            HwndSource hwndSource = (HwndSource)HwndSource.FromVisual(Owner);
            CompositionTarget ct = hwndSource.CompositionTarget;
            offset = ct.TransformToDevice.Transform(offset);
            size = ct.TransformToDevice.Transform(size);

            Win32.POINT screenLocation = new Win32.POINT(offset);
            Win32.ClientToScreen(hwndSource.Handle, ref screenLocation);
            Win32.POINT screenSize = new Win32.POINT(size);

            Win32.MoveWindow(((HwndSource)HwndSource.FromVisual(this)).Handle, screenLocation.X, screenLocation.Y, screenSize.X, screenSize.Y, true);
        }
    };
}
