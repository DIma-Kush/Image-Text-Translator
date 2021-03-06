﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ComputerVisionSample.Translator;
using Xamarin.Forms;

namespace ComputerVisionSample
{
    public partial class App : Application
    {
        public static double ScreenWidth;
        public static double ScreenHeight;

        public App()
        {
            InitializeComponent();
            MainPage = new ComputerVisionSample.OcrRecognitionPage();
        }
        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
