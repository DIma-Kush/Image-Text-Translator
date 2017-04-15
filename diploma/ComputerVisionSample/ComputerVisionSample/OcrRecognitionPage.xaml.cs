﻿using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using Plugin.Connectivity;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using ComputerVisionSample.Translator;
using ComputerVisionSample.ClipBoard;
using System.Drawing;


namespace ComputerVisionSample
{
    public partial class OcrRecognitionPage : ContentPage
    {
        public Exception Error
        {
            get;
            private set;
        }
        // Початкове налаштування
        private int count = 0;
        private readonly VisionServiceClient visionClient;
        string sourceLanguage = "en"; // english by default
        string sourceText = "";
        string destinationLanguage = "";
        string bufferSourceText1 = "";
        string bufferSourceText2 = "";
        string bufferSourceText3 = "";
        string bufferSourceText4 = "";
        string bufferSourceText5 = "";
        string bufferSourceText6 = "";
        // визначимо координати лінії тексту
        int Top = 0;
        int Left = 0;
        int width = 0;
        int height = 0;
        //
        string transaltedText = "";
        bool flag = false; // прапорець для делегування зміною стану кнопок Камери та Галереї
        bool imageInverseFlag = false; // прапорець для делегування зумування зображенням
        public OcrRecognitionPage()
        {
            this.Error = null;
            InitializeComponent();
            Random rand = new Random();

            if (rand.Next(0, 2) == 0)
                this.visionClient = new VisionServiceClient("cf3b45431cc14c799696821dd9668990");
            else
                this.visionClient = new VisionServiceClient("315528c6723843db8b09351422e87f19");

            DestinationLangPicker.IsVisible = false;
            GettedLanguage.IsVisible = false;
            BackButton.IsVisible = false;
            BackButton.Text = "<- Back";   
            UploadPictureButton.IsVisible = false;
            TakePictureButton.IsVisible = false;
        }

        private async Task<OcrResults> AnalyzePictureAsync(Stream inputFile)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await DisplayAlert("Network error", "Please check your network connection and retry.", "OK");
                return null;
            }

            OcrResults ocrResult = await visionClient.RecognizeTextAsync(inputFile);
            return ocrResult;
        }

        private async void TakePictureButton_Clicked(object sender, EventArgs e)
        {
            try
            {
               
                await CrossMedia.Current.Initialize();

                if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                {
                    await DisplayAlert("No Camera", "No camera available.", "OK");
                    return;
                }
                var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    SaveToAlbum = false,
                    Name = "test.jpg",
                    CompressionQuality = 75,
                    AllowCropping = true
                });
               

                UploadPictureButton.IsVisible = false;
                TakePictureButton.IsVisible = false;
                BackButton.IsVisible = true;
                Image1.IsVisible = true;
                if (file == null)
                    return;
                if (backgroundImage.Opacity != 0)
                {
                    backgroundImage.Opacity = 0;
                }
                this.Indicator1.IsVisible = true;
                this.Indicator1.IsRunning = true;


                Image1.Source = ImageSource.FromStream(() => file.GetStream());


                var ocrResult = await AnalyzePictureAsync(file.GetStream());
               
                this.BindingContext = null;
                this.BindingContext = ocrResult;
                sourceLanguage = ocrResult.Language;

                PopulateUIWithRegions(ocrResult);
                TranslatedText.IsVisible = true;
                Image1.IsVisible = true;
                this.Indicator1.IsRunning = false;
                this.Indicator1.IsVisible = false;
                //  DestinationLangPicker.SelectedIndex = 0;
                //  DestinationLangPicker.Title = "Destination language";
                flag = true;
                Image1.IsVisible = true;
               // this.TranslatedText.Children.Clear();
                DestinationLangPicker.IsVisible = true;
                GettedLanguage.IsVisible = true;

               
            }
            catch (Exception ex)
            {
                this.Error = ex;
            }
        }

        private void PopulateUIWithRegions(OcrResults ocrResult)
        {
          destinationLanguage = 
                DestinationLangPicker.Items[DestinationLangPicker.SelectedIndex];
                TranslatedText.Text = "";
       
                //Ітерація по регіонах
                foreach (var region in ocrResult.Regions)
                {
                //Ітерація по лініях в регіоні
                foreach (var line in region.Lines)
                {
                    //   Для кожної лінії згенерувати горизонтальну панель
                    var lineStack = new StackLayout
                    { Orientation = StackOrientation.Horizontal };

                    //Ітерація по словах в лінії
                    foreach (var word in line.Words)
                    {
                        var textLabel = new Label
                        {
                            TextColor = Xamarin.Forms.Color.Black,
                            Text = word.Text,  
                        };
                        sourceText += textLabel.Text + " ";

                       
                        if(bufferSourceText1.Length < 400)
                        {
                            bufferSourceText1 += textLabel.Text + " ";
                        }
                        else if(bufferSourceText2.Length < 400)
                        {
                            bufferSourceText2 += textLabel.Text + " ";
                        }
                        else if (bufferSourceText3.Length < 400)
                        {
                            bufferSourceText3 += textLabel.Text + " ";
                        }
                        else if (bufferSourceText4.Length < 400)
                        {
                            bufferSourceText4 += textLabel.Text + " ";
                        }
                        else if (bufferSourceText5.Length < 400)
                        {
                            bufferSourceText5 += textLabel.Text + " ";
                        }
                        else
                        {
                            bufferSourceText6 += textLabel.Text + " ";
                        }
                        lineStack.Children.Add(textLabel);
                    }

                    height = line.Rectangle.Height;
                    width = line.Rectangle.Width;
                    Left = line.Rectangle.Left;
                    Top = line.Rectangle.Top;

                   Xamarin.Forms.Rectangle rec = new Xamarin.Forms.Rectangle(Top, Left, width, height);

                    // Відправка обробленого тексту на переклад
                    Translate_Txt(sourceText, destinationLanguage);
                    sourceText = "";
                }
                 
            }
        }
    //    void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
    //    {
           
      //  }
        private async void UploadPictureButton_Clicked(object sender, EventArgs e)
        {         
            try
            {
                if (!CrossMedia.Current.IsPickPhotoSupported)
                {
                    await DisplayAlert("No upload", "Picking a photo is not supported.", "OK");
                    return;
                }
                var file = await CrossMedia.Current.PickPhotoAsync();
                if (file == null)
                    return;
                if (backgroundImage.Opacity !=0)
                {
                    backgroundImage.Opacity = 0;
                }
                UploadPictureButton.IsVisible = false;
                TakePictureButton.IsVisible = false;
                Image1.IsVisible = true;
                this.Indicator1.IsVisible = true;
                this.Indicator1.IsRunning = true;
                Image1.Source = ImageSource.FromStream(() => file.GetStream());

                var ocrResult = await AnalyzePictureAsync(file.GetStream());

                this.BindingContext = ocrResult;
                sourceLanguage = ocrResult.Language;
                
                PopulateUIWithRegions(ocrResult);
                TranslatedText.IsVisible = true;

             
            }
            catch (Exception ex)
            {
                this.Error = ex;
            }
            BackButton.IsVisible = true;
            this.Indicator1.IsRunning = false;
            this.Indicator1.IsVisible = false;
            flag = true;
            //  DestinationLangPicker.SelectedIndex = 0;
            //    DestinationLangPicker.Title = "Destination language";

            Image1.IsVisible = true;
          //  this.TranslatedText.Children.Clear();
            DestinationLangPicker.IsVisible = true;
            GettedLanguage.IsVisible = true;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            if (count == 0)
            {
                DestinationLangPicker.Focus();
                count++;
                if (Device.OS == TargetPlatform.iOS) // 
                {
                    DestinationLangPicker.Items.Clear();
                    DestinationLangPicker.Items.Add("Destination language");  
               
                DestinationLangPicker.Items.Add("English");
                DestinationLangPicker.Items.Add("Ukrainian");
                DestinationLangPicker.Items.Add("French");
                DestinationLangPicker.Items.Add("Polish");
                DestinationLangPicker.Items.Add("Spanish");
                DestinationLangPicker.Items.Add("German");
                DestinationLangPicker.Items.Add("Italian");
                DestinationLangPicker.Items.Add("Latvian");
                DestinationLangPicker.Items.Add("Chinese");
                DestinationLangPicker.Items.Add("Japanese");
                DestinationLangPicker.Items.Add("Portuguese");
                DestinationLangPicker.Items.Add("Arabic");
                DestinationLangPicker.Items.Add("Hindi");
                DestinationLangPicker.Items.Add("Hebrew");
                DestinationLangPicker.Items.Add("Swedish");
                DestinationLangPicker.Items.Add("Danish");
                DestinationLangPicker.Items.Add("Norwegian");
                DestinationLangPicker.Items.Add("Finnish");
                DestinationLangPicker.Items.Add("Georgian");
                DestinationLangPicker.Items.Add("Turkish");
                DestinationLangPicker.Items.Add("Russian");
                DestinationLangPicker.Items.Add("Czech");

                    DestinationLangPicker.SelectedIndexChanged += (sender, e) =>
                {
                    if (DestinationLangPicker.SelectedIndex == 0)
                        DestinationLangPicker.SelectedIndex = 1;
                };
               }
            }
            base.OnSizeAllocated(width, height);

            if (DeviceInfo.IsOrientationPortrait() && width < height || !DeviceInfo.IsOrientationPortrait() && width > height)
            {
                //Image1.IsVisible = false;
            }
            else
            {
              // Image1.IsVisible = true;
            }
        }

        void BackButton_Clicked(object sender, EventArgs e)
        {
            if (imageInverseFlag == false) // якщо кнопка використовується для виходу в голове меню
            {
                TakePictureButton.IsVisible = true;
                UploadPictureButton.IsVisible = true;
                BackButton.IsVisible = false;
                Image1.IsVisible = false;
                TranslatedText.IsVisible = false;
                GettedLanguage.IsVisible = false;
                DestinationLangPicker.IsVisible = false;
                TranslatedText.IsVisible = false;
                // ImageBackButton.IsVisible = false;
                backgroundImage.Opacity = 0.4;
                bufferSourceText1 = "";
                bufferSourceText2 = "";
                bufferSourceText3 = "";
                bufferSourceText4 = "";
                bufferSourceText5 = "";
                bufferSourceText6 = "";
                UploadPictureButton.IsVisible = false;
                TakePictureButton.IsVisible = false;
                backgroundImage.IsVisible = true;
                flag = false;
                DestinationLangPicker.Focus();
                picker_func();
            }
            else // інакше, для виходу з режиму збільшеного зображення
            {
                TapGesture(true);
            }
        }

 
            /// //////////////// TRANSLATION///////////////////////
            void Translate_Txt(string sourceTxt, string destLang)
        {
            if (sourceLanguage != "unk")
            {
                    this.TranslatedText.Text += DependencyService.Get<PCL_Translator>().
                            Translate(sourceTxt, sourceLanguage, destLang) + " ";

                transaltedText = TranslatedText.Text;       
            }
            else
            {
                var Error = "unknown language! Please try again";
                this.TranslatedText.Text = Error;
            } 
        }
        /// //////////////// TRANSLATION END///////////////////////
        public void ClipboardFunc(object sender, EventArgs e)
        {
            DependencyService.Get<PCL_ClipBoard>().GetTextFromClipBoard(transaltedText);
            DisplayAlert("", "Successfully copied to the clipboard", "OK");
            // clipboardText = TranslatedText.Text;
        }
        void TapGesture(bool move_to_default)
        {
            if (move_to_default == true)
            {
                BackButton.Text = " < -Back";
                TranslatedText.IsVisible = true;
                GettedLanguage.IsVisible = true;
                DestinationLangPicker.IsVisible = true;
                Image1.HeightRequest = 240;
                Image1.WidthRequest = 240;
                imageInverseFlag = false;
            }
            else
            {
                BackButton.Text = "Resize";
                TakePictureButton.IsVisible = false;
                UploadPictureButton.IsVisible = false;
                Image1.HeightRequest = 600;
                Image1.WidthRequest = 600;
                TranslatedText.IsVisible = false;
                GettedLanguage.IsVisible = false;
                DestinationLangPicker.IsVisible = false;
                imageInverseFlag = true;
                
            }
            
        }

        void OnTapGestureRecognizerTapped(object sender, EventArgs args) {
            if (imageInverseFlag == false)
                TapGesture(false);
            else
                TapGesture(true);
        }
    
        void UnfocusedPicker(object sender, EventArgs e)
        {
            DestinationLangPicker.SelectedIndex = 0;
        }
        void picker_language_choose(object sender, EventArgs e)
        {
            picker_func();
        }
        void picker_func()
        {
            DestinationLangPicker.Title = DestinationLangPicker.Items[DestinationLangPicker.SelectedIndex];
            if (Device.OS == TargetPlatform.Android) // 
            {
                DestinationLangPicker.WidthRequest = DestinationLangPicker.Title.Length * 11.2;
            }
            TranslatedText.Text = "";
            if (bufferSourceText1.Length > 1)
                Translate_Txt(bufferSourceText1, DestinationLangPicker.Title);
            if (bufferSourceText2.Length > 1)
                Translate_Txt(bufferSourceText2, DestinationLangPicker.Title);
            if (bufferSourceText3.Length > 1)
                Translate_Txt(bufferSourceText3, DestinationLangPicker.Title);
            if (bufferSourceText4.Length > 1)
                Translate_Txt(bufferSourceText4, DestinationLangPicker.Title);
            if (bufferSourceText5.Length > 1)
                Translate_Txt(bufferSourceText5, DestinationLangPicker.Title);
            if (bufferSourceText6.Length > 1)
                Translate_Txt(bufferSourceText6, DestinationLangPicker.Title);

            if (flag == false)
            {
                UploadPictureButton.IsVisible = true;
                TakePictureButton.IsVisible = true;
            }
        }
    }
}
