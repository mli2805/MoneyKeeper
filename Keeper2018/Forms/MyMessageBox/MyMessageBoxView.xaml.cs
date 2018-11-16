﻿using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Keeper2018
{
    /// <summary>
    /// Interaction logic for MyMessageBoxView.xaml
    /// </summary>
    public partial class MyMessageBoxView
    {
        private const int GwlStyle = -16;
        private const int WsSysmenu = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public MyMessageBoxView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GwlStyle, GetWindowLong(hwnd, GwlStyle) & ~WsSysmenu);
        }
    }
}
