﻿using GalaSoft.MvvmLight.Messaging;
using SerrisCodeEditor.Functions;
using SerrisCodeEditor.Functions.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace SerrisCodeEditor.Xaml.Views
{

    public sealed partial class SettingsManager : Page
    {
        ApplicationDataContainer AppSettings = ApplicationData.Current.LocalSettings;

        public SettingsManager()
        {
            this.InitializeComponent();

            SetTheme();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            int ColumnCount = 0;

            foreach (SettingsMenu menu in DefaultSettings.DefaultSettingsMenuList)
            {
                //BUTTON
                ButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                Grid ButtonMenu = new Grid();
                ButtonMenu.Name = menu.Name;
                ButtonMenu.Background = new SolidColorBrush(Colors.Transparent);
                ButtonMenu.HorizontalAlignment = HorizontalAlignment.Stretch; ButtonMenu.VerticalAlignment = VerticalAlignment.Stretch;
                ButtonMenu.PointerPressed += ((a, f) => 
                {
                    SelectMenu(menu.Name);
                });

                ButtonMenu.PointerEntered += ((a, f) =>
                {
                    Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Hand, 1);
                });

                ButtonMenu.PointerExited += ((a, f) =>
                {
                    Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
                });


                //BUTTON CONTENT
                StackPanel ButtonContent = new StackPanel();
                ButtonContent.HorizontalAlignment = HorizontalAlignment.Center;
                ButtonContent.VerticalAlignment = VerticalAlignment.Center;

                TextBlock ButtonIcon = new TextBlock();
                ButtonIcon.FontSize = 18;
                ButtonIcon.FontFamily = new FontFamily("Segoe MDL2 Assets");
                ButtonIcon.HorizontalAlignment = HorizontalAlignment.Center;
                ButtonIcon.Foreground = GlobalVariables.CurrentTheme.SecondaryColorFont;
                ButtonIcon.Text = menu.Icon;
                ButtonContent.Children.Add(ButtonIcon);

                TextBlock ButtonTitle = new TextBlock();
                ButtonTitle.FontSize = 15;
                ButtonTitle.Text = menu.Name;
                ButtonTitle.Foreground = GlobalVariables.CurrentTheme.SecondaryColorFont;
                ButtonContent.Children.Add(ButtonTitle);

                ButtonMenu.Children.Add(ButtonContent);

                ButtonsGrid.Children.Add(ButtonMenu);
                Grid.SetColumn(ButtonMenu, ColumnCount);

                ColumnCount++;

                //SEPARATOR
                if(ColumnCount < (DefaultSettings.DefaultSettingsMenuList.Length * 2) - 1)
                {
                    ButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Pixel) });

                    Rectangle Separator = new Rectangle();
                    Separator.Width = 2;
                    Separator.Margin = new Thickness(0, 15, 0, 15);
                    Separator.RadiusX = 2; Separator.RadiusY = 1;
                    Separator.Fill = GlobalVariables.CurrentTheme.SecondaryColorFont;

                    ButtonsGrid.Children.Add(Separator);
                    Grid.SetColumn(Separator, ColumnCount);

                    ColumnCount++;
                }

            }

            //Default selection
            SelectMenu(DefaultSettings.DefaultSettingsMenuList[0].Name);
        }

        private void SetTheme()
        {
            BackgroundList.Fill = GlobalVariables.CurrentTheme.MainColor;
            ButtonsGrid.Background = GlobalVariables.CurrentTheme.SecondaryColor;
        }

        private void SelectMenu(string MenuName)
        {
            foreach(SettingsMenu menu in DefaultSettings.DefaultSettingsMenuList)
            {
                if(menu.Name == MenuName)
                {
                    //Delete the controls of the ancient menu
                    MenuControls.Children.Clear();

                    //Add the new controls of selected menu
                    foreach(Setting SettingControl in menu.Settings)
                    {
                        switch(SettingControl.Type)
                        {
                            case SettingType.Checkbox:
                                ToggleSwitch Switch = new ToggleSwitch();
                                Switch.Style = (Style)Application.Current.Resources["SwitchControl"];
                                Switch.Margin = new Thickness(0, 20, 0, 0);
                                Switch.Foreground = GlobalVariables.CurrentTheme.MainColorFont;
                                Switch.Background = GlobalVariables.CurrentTheme.MainColor;
                                Switch.Header = SettingControl.Description;

                                if (AppSettings.Values.ContainsKey(SettingControl.VarSaveName))
                                    Switch.IsOn = (bool)AppSettings.Values[SettingControl.VarSaveName];
                                else
                                    Switch.IsOn = (bool)SettingControl.VarSaveDefaultContent;

                                Switch.Toggled += ((e, f) => 
                                {
                                    ToggleSwitch SwitchContent = (ToggleSwitch)e;
                                    AppSettings.Values[SettingControl.VarSaveName] = SwitchContent.IsOn;
                                    Messenger.Default.Send(new SettingsNotification { SettingsUpdatedName = menu.Name });

                                });


                                MenuControls.Children.Add(Switch);
                                break;

                            case SettingType.TextboxText:
                                StackPanel TextBoxControl = new StackPanel();
                                TextBoxControl.Margin = new Thickness(0, 20, 0, 0);

                                TextBlock TitleTextBox = new TextBlock();
                                TitleTextBox.FontSize = 15;
                                TitleTextBox.TextWrapping = TextWrapping.Wrap;
                                TitleTextBox.Text = SettingControl.Description;
                                TitleTextBox.Foreground = GlobalVariables.CurrentTheme.MainColorFont;

                                TextBoxControl.Children.Add(TitleTextBox);

                                TextBox TextBox = new TextBox();
                                TextBox.Margin = new Thickness(5, 5, 0, 0);

                                TextBox.Style = (Style)Application.Current.Resources["RoundTextBox"];
                                TextBox.Width = 150; TextBox.Height = 25;
                                //TextBox.PlaceholderText = "";
                                TextBox.FontSize = 14; TextBox.Background = GlobalVariables.CurrentTheme.SecondaryColor; TextBox.Foreground = GlobalVariables.CurrentTheme.SecondaryColorFont;
                                TextBox.KeyDown += ((e, f) =>
                                {
                                    if (f.KeyStatus.RepeatCount == 1)
                                    {
                                        if (f.Key == Windows.System.VirtualKey.Enter)
                                        {
                                            Messenger.Default.Send(new SettingsNotification { SettingsUpdatedName = menu.Name });
                                        }
                                    }
                                });

                                TextBoxControl.Children.Add(TextBox);
                                MenuControls.Children.Add(TextBoxControl);
                                break;

                            case SettingType.TextboxNumber:
                                StackPanel TextBoxNmbControl = new StackPanel();
                                TextBoxNmbControl.Margin = new Thickness(0, 20, 0, 0);

                                TextBlock TitleTextBoxNmb = new TextBlock();
                                TitleTextBoxNmb.FontSize = 15;
                                TitleTextBoxNmb.TextWrapping = TextWrapping.Wrap;
                                TitleTextBoxNmb.Text = SettingControl.Description;
                                TitleTextBoxNmb.Foreground = GlobalVariables.CurrentTheme.MainColorFont;

                                TextBoxNmbControl.Children.Add(TitleTextBoxNmb);

                                TextBox TextBoxNmb = new TextBox();
                                TextBoxNmb.Margin = new Thickness(5, 5, 0, 0);

                                TextBoxNmb.Style = (Style)Application.Current.Resources["RoundTextBox"];
                                TextBoxNmb.Width = 150; TextBoxNmb.Height = 25;
                                TextBoxNmb.HorizontalAlignment = HorizontalAlignment.Left;
                                TextBoxNmb.FontSize = 14; TextBoxNmb.Background = GlobalVariables.CurrentTheme.SecondaryColor; TextBoxNmb.Foreground = GlobalVariables.CurrentTheme.SecondaryColorFont;

                                if (AppSettings.Values.ContainsKey(SettingControl.VarSaveName))
                                    TextBoxNmb.Text = ((int)AppSettings.Values[SettingControl.VarSaveName]).ToString();
                                else
                                    TextBoxNmb.Text = ((int)SettingControl.VarSaveDefaultContent).ToString();

                                TextBoxNmb.KeyDown += ((e, f) =>
                                {
                                    if (f.Key == Windows.System.VirtualKey.Enter)
                                    {
                                        f.Handled = false;
                                        if (string.IsNullOrEmpty(TextBoxNmb.Text))
                                        {
                                            AppSettings.Values[SettingControl.VarSaveName] = (int)SettingControl.VarSaveDefaultContent;
                                        }
                                        else
                                        {
                                            AppSettings.Values[SettingControl.VarSaveName] = int.Parse(TextBoxNmb.Text);
                                        }
                                        Messenger.Default.Send(new SettingsNotification { SettingsUpdatedName = menu.Name });

                                        return;
                                    }

                                    if (f.Key.ToString().Equals("Back"))
                                    {
                                        f.Handled = false;
                                        return;
                                    }

                                    for (int i = 0; i < 10; i++)
                                    {
                                        if (f.Key.ToString() == string.Format("Number{0}", i))
                                        {
                                            f.Handled = false;
                                            return;
                                        }
                                    }

                                    f.Handled = true;

                                });

                                TextBoxNmbControl.Children.Add(TextBoxNmb);
                                MenuControls.Children.Add(TextBoxNmbControl);
                                break;

                            case SettingType.Link:
                                StackPanel LinkControl = new StackPanel();
                                LinkControl.Margin = new Thickness(0, 20, 0, 0);

                                TextBlock TitleLink = new TextBlock();
                                TitleLink.FontSize = 15;
                                TitleLink.TextWrapping = TextWrapping.Wrap;
                                TitleLink.Text = SettingControl.Description;
                                TitleLink.Foreground = GlobalVariables.CurrentTheme.MainColorFont;

                                LinkControl.Children.Add(TitleLink);

                                TextBlock Link = new TextBlock();
                                Link.Margin = new Thickness(5, 5, 0, 0);
                                Link.FontSize = 12;
                                Link.TextWrapping = TextWrapping.Wrap;
                                Link.FontStyle = FontStyle.Italic;
                                Link.FontWeight = FontWeights.Light;
                                Link.Foreground = GlobalVariables.CurrentTheme.MainColorFont;

                                Underline UnderlineText = new Underline();
                                Run TextLink = new Run();
                                TextLink.Text = (string)SettingControl.Parameter;
                                UnderlineText.Inlines.Add(TextLink);
                                Link.Inlines.Add(UnderlineText);

                                Link.PointerPressed += (async (e, f) => 
                                {
                                    TextBlock LinkContent = (TextBlock)e;
                                    await Windows.System.Launcher.LaunchUriAsync(new Uri(LinkContent.Text));
                                });

                                Link.PointerEntered += ((e, f) =>
                                {
                                    Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Hand, 1);
                                });

                                Link.PointerExited += ((e, f) =>
                                {
                                    Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
                                });

                                LinkControl.Children.Add(Link);
                                MenuControls.Children.Add(LinkControl);
                                break;

                            case SettingType.SecondDescription:
                                StackPanel DescriptionControl = new StackPanel();
                                DescriptionControl.Margin = new Thickness(0, 20, 0, 0);

                                TextBlock TitleDescription = new TextBlock();
                                TitleDescription.FontSize = 15;
                                TitleDescription.TextWrapping = TextWrapping.Wrap;
                                TitleDescription.Text = SettingControl.Description;
                                TitleDescription.Foreground = GlobalVariables.CurrentTheme.MainColorFont;

                                DescriptionControl.Children.Add(TitleDescription);

                                TextBlock Description = new TextBlock();
                                Description.Margin = new Thickness(5, 5, 0, 0);
                                Description.TextWrapping = TextWrapping.Wrap;
                                Description.FontSize = 13;
                                Description.FontWeight = FontWeights.Light;
                                Description.Text = (string)SettingControl.Parameter;
                                Description.Foreground = GlobalVariables.CurrentTheme.MainColorFont;

                                DescriptionControl.Children.Add(Description);
                                MenuControls.Children.Add(DescriptionControl);
                                break;

                            case SettingType.Separator:
                                Grid SeparatorControl = new Grid();
                                SeparatorControl.Margin = new Thickness(0, 20, 0, 0);
                                SeparatorControl.HorizontalAlignment = HorizontalAlignment.Stretch;

                                SeparatorControl.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                                SeparatorControl.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                                TextBlock TitleSeparator = new TextBlock();
                                TitleSeparator.FontSize = 17;
                                TitleSeparator.FontWeight = FontWeights.SemiBold;
                                TitleSeparator.Text = SettingControl.Description;
                                TitleSeparator.Foreground = GlobalVariables.CurrentTheme.MainColorFont;

                                SeparatorControl.Children.Add(TitleSeparator);

                                Rectangle RectSeparator = new Rectangle();
                                Grid.SetColumn(RectSeparator, 1);
                                RectSeparator.Height = 2;
                                RectSeparator.Margin = new Thickness(5, 2, 0, 0);
                                RectSeparator.VerticalAlignment = VerticalAlignment.Center;
                                RectSeparator.HorizontalAlignment = HorizontalAlignment.Stretch;
                                RectSeparator.Fill = GlobalVariables.CurrentTheme.MainColorFont;

                                SeparatorControl.Children.Add(RectSeparator);
                                MenuControls.Children.Add(SeparatorControl);
                                break;

                            case SettingType.License:
                                StackPanel LicenseControl = new StackPanel();
                                LicenseControl.Margin = new Thickness(0, 20, 0, 0);

                                TextBlock TitleLicense = new TextBlock();
                                TitleLicense.TextWrapping = TextWrapping.Wrap;
                                TitleLicense.FontSize = 15;
                                TitleLicense.Text = SettingControl.Description;
                                TitleLicense.Foreground = GlobalVariables.CurrentTheme.MainColorFont;

                                LicenseControl.Children.Add(TitleLicense);

                                TextBlock Author = new TextBlock();
                                Author.TextWrapping = TextWrapping.Wrap;
                                Author.Margin = new Thickness(5, 5, 0, 0);
                                Author.FontSize = 12;
                                Author.FontWeight = FontWeights.Light;
                                Author.Foreground = GlobalVariables.CurrentTheme.MainColorFont;
                                Author.Text = ((Tuple<string, string>)SettingControl.Parameter).Item1;

                                LicenseControl.Children.Add(Author);

                                Button LicenseButton = new Button();
                                LicenseButton.Margin = new Thickness(0, 5, 0, 0);
                                LicenseButton.Content = GlobalVariables.GlobalizationRessources.GetString("settings-buttonlicense");
                                LicenseButton.FontSize = 14;
                                LicenseButton.Foreground = GlobalVariables.CurrentTheme.MainColorFont;
                                LicenseButton.Background = GlobalVariables.CurrentTheme.MainColor;
                                LicenseButton.Click += (async (e, f) => {
                                    var currentAV = ApplicationView.GetForCurrentView(); var newAV = CoreApplication.CreateNewView();

                                    await newAV.Dispatcher.RunAsync(
                                                    CoreDispatcherPriority.Normal,
                                                    async () =>
                                                    {
                                                        var newWindow = Window.Current;
                                                        var newAppView = ApplicationView.GetForCurrentView();
                                                        newAppView.Title = SettingControl.Description + " " + ((Tuple<string, string>)SettingControl.Parameter).Item1;

                                                        var frame = new Frame();
                                                        frame.Navigate(typeof(LicenseView), ((Tuple<string, string>)SettingControl.Parameter).Item2);
                                                        newWindow.Content = frame;
                                                        newWindow.Activate();

                                                        await ApplicationViewSwitcher.TryShowAsStandaloneAsync(
                                                            newAppView.Id,
                                                            ViewSizePreference.UseHalf,
                                                            currentAV.Id,
                                                            ViewSizePreference.UseHalf);
                                                    });
                                });
                                LicenseControl.Children.Add(LicenseButton);

                                MenuControls.Children.Add(LicenseControl);
                                break;

                            case SettingType.ComboBox:
                                StackPanel ListControl = new StackPanel();
                                ListControl.Margin = new Thickness(0, 20, 0, 0);

                                TextBlock TitleList = new TextBlock();
                                TitleList.TextWrapping = TextWrapping.Wrap;
                                TitleList.FontSize = 15;
                                TitleList.Text = SettingControl.Description;
                                TitleList.Foreground = GlobalVariables.CurrentTheme.MainColorFont;

                                ListControl.Children.Add(TitleList);

                                ComboBox List = new ComboBox();
                                List.Margin = new Thickness(5, 5, 0, 0);
                                List.FontSize = 14;
                                List.FontWeight = FontWeights.Light;
                                List.Foreground = GlobalVariables.CurrentTheme.MainColorFont;
                                List.Background = GlobalVariables.CurrentTheme.MainColor;

                                List.SelectionChanged += ((e, f) =>
                                {
                                    if(List.SelectedIndex != -1)
                                    {
                                        AppSettings.Values[SettingControl.VarSaveName] = List.SelectedValue;
                                        Messenger.Default.Send(new SettingsNotification { SettingsUpdatedName = menu.Name });
                                    }
                                });

                                foreach (string Item in (List<string>)SettingControl.Parameter)
                                {
                                    List.Items.Add(Item);
                                    if (AppSettings.Values.ContainsKey(SettingControl.VarSaveName))
                                    {
                                        if(Item == (string)AppSettings.Values[SettingControl.VarSaveName])
                                        {
                                            List.SelectedIndex = (List.Items.Count - 1);
                                        }
                                    }
                                    else
                                    {
                                        if (Item == (string)SettingControl.VarSaveDefaultContent)
                                        {
                                            List.SelectedIndex = (List.Items.Count - 1);
                                        }
                                    }
                                }

                                ListControl.Children.Add(List);
                                MenuControls.Children.Add(ListControl);

                                break;
                        }
                    }
                }

            }

        }
    }

}
