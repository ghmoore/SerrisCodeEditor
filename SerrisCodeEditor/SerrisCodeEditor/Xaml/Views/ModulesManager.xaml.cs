﻿using GalaSoft.MvvmLight.Messaging;
using Microsoft.Toolkit.Uwp.UI.Animations;
using SCEELibs.Editor.Notifications;
using SerrisCodeEditor.Functions;
using SerrisModulesServer.Items;
using SerrisModulesServer.Manager;
using SerrisModulesServer.Type;
using SerrisModulesServer.Type.Addon;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace SerrisCodeEditor.Xaml.Views
{
    public class ModuleInfosShow
    {
        public BitmapImage Thumbnail { get; set; }
        public InfosModule Module { get; set; }
        public int StrokeThickness { get; set; }

        public Visibility DeleteButtonVisibility
        {
            get
            {
                if(Module.ModuleSystem)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Visible;
                }
            }
        }

        public SolidColorBrush ForegroundColor
        {
            get { return GlobalVariables.CurrentTheme.MainColorFont; }
        }

    }

    public sealed partial class ModulesManager : Page
    {

        public ModulesManager()
        {
            InitializeComponent();
            SetTheme();
        }

        private void ModulesManagerUI_Loaded(object sender, RoutedEventArgs e)
        => ChangeSelectedButton(0);



        /* =============
         * = FUNCTIONS =
         * =============
         */


        private void SetTheme()
        {
            BackgroundList.Fill = GlobalVariables.CurrentTheme.MainColor;
            MenuButtons.Background = GlobalVariables.CurrentTheme.SecondaryColor;

            ButtonsSeparator.Fill = GlobalVariables.CurrentTheme.SecondaryColorFont;
            ButtonsSeparatorB.Fill = GlobalVariables.CurrentTheme.SecondaryColorFont;
            ButtonsSeparatorC.Fill = GlobalVariables.CurrentTheme.SecondaryColorFont;

            AddonsIcon.Foreground = GlobalVariables.CurrentTheme.SecondaryColorFont;
            ThemesIcon.Foreground = GlobalVariables.CurrentTheme.SecondaryColorFont;
            ProgLanguagesIcon.Foreground = GlobalVariables.CurrentTheme.SecondaryColorFont;
            ProjectTypesIcon.Foreground = GlobalVariables.CurrentTheme.SecondaryColorFont;

            InstallButton.Background = GlobalVariables.CurrentTheme.MainColor;
            IconInstallButton.Foreground = GlobalVariables.CurrentTheme.MainColorFont;
            TextInstallButton.Foreground = GlobalVariables.CurrentTheme.MainColorFont;
        }

        private void ChangeSelectedButton(int newSelectedButton)
        {
            if (currentSelectedButton != newSelectedButton)
            {
                switch(newSelectedButton)
                {
                    case 0:
                        AddonsIcon.Light(distance: 50, duration: 1500, delay: 0).Start();
                        ThemesIcon.Light(distance: 8, duration: 1500, delay: 0).Start();
                        ProgLanguagesIcon.Light(distance: 8, duration: 1500, delay: 0).Start();
                        ProjectTypesIcon.Light(distance: 8, duration: 1500, delay: 0).Start();
                        break;

                    case 1:
                        AddonsIcon.Light(distance: 8, duration: 1500, delay: 0).Start();
                        ThemesIcon.Light(distance: 50, duration: 1500, delay: 0).Start();
                        ProgLanguagesIcon.Light(distance: 8, duration: 1500, delay: 0).Start();
                        ProjectTypesIcon.Light(distance: 8, duration: 1500, delay: 0).Start();
                        break;

                    case 2:
                        AddonsIcon.Light(distance: 8, duration: 1500, delay: 0).Start();
                        ThemesIcon.Light(distance: 8, duration: 1500, delay: 0).Start();
                        ProgLanguagesIcon.Light(distance: 50, duration: 1500, delay: 0).Start();
                        ProjectTypesIcon.Light(distance: 8, duration: 1500, delay: 0).Start();
                        break;

                    case 3:
                        AddonsIcon.Light(distance: 8, duration: 1500, delay: 0).Start();
                        ThemesIcon.Light(distance: 8, duration: 1500, delay: 0).Start();
                        ProgLanguagesIcon.Light(distance: 8, duration: 1500, delay: 0).Start();
                        ProjectTypesIcon.Light(distance: 50, duration: 1500, delay: 0).Start();
                        break;
                }
                currentSelectedButton = newSelectedButton;
                LoadModules();
            }

        }

        private async void LoadModules()
        {
            ListModules.Items.Clear();
            string IDThemeMonaco = ModulesAccessManager.GetCurrentThemeMonacoID(), IDTheme = ModulesAccessManager.GetCurrentThemeID();

            switch(currentSelectedButton)
            {
                default:
                    ListModules.SelectionMode = ListViewSelectionMode.Single;
                    break;

                case 2:
                case 3:
                    ListModules.SelectionMode = ListViewSelectionMode.None;
                    break;
            }

            foreach (InfosModule module in ModulesAccessManager.GetModules(true))
            {
                ModuleInfosShow ModuleInfos = new ModuleInfosShow { Module = module, StrokeThickness = 0 };
                ModuleInfos.Thumbnail = await ModulesAccessManager.GetModuleDefaultLogoViaIDAsync(ModuleInfos.Module.ID, ModuleInfos.Module.ModuleSystem);

                switch (module.ModuleType)
                {
                    case ModuleTypesList.Templates when currentSelectedButton == 3:
                    case ModuleTypesList.ProgrammingLanguage when currentSelectedButton == 2:
                    case ModuleTypesList.Addon when currentSelectedButton == 0:
                        ListModules.Items.Add(ModuleInfos);
                        break;

                    case ModuleTypesList.Theme when currentSelectedButton == 1:
                        if (IDTheme == ModuleInfos.Module.ID || IDThemeMonaco == ModuleInfos.Module.ID)
                            ModuleInfos.StrokeThickness = 2;

                        ListModules.Items.Add(ModuleInfos);
                        break;

                }
            }

        }

        private async void ListModules_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListModules.SelectedItem != null)
            {
                var module = (ModuleInfosShow)ListModules.SelectedItem;
                switch (currentSelectedButton)
                {
                    case 0:
                        await Task.Run(() => 
                        {
                            new AddonExecutor(module.Module.ID, new SCEELibs.SCEELibs(module.Module.ID)).ExecuteDefaultFunction(AddonExecutorFuncTypes.main);
                        });
                        break;

                    case 1:
                        await ModulesWriteManager.SetCurrentThemeIDAsync(module.Module.ID, false);

                        if(module.Module.ContainMonacoTheme)
                            await ModulesWriteManager.SetCurrentMonacoThemeIDAsync(module.Module.ID, false);

                        Messenger.Default.Send(new SMSNotification { Type = TypeUpdateModule.CurrentThemeUpdated, ID = module.Module.ID });
                        LoadModules();
                        break;
                }
            }
        }

        private void ModuleOptions_Click(object sender, RoutedEventArgs e)
        {
        }

        private async void ModulePin_Click(object sender, RoutedEventArgs e)
        {
            ModuleInfosShow module = (ModuleInfosShow)(sender as Button).DataContext;

            List<string> list = await ModulesPinned.GetModulesPinned();

            if (list.Contains(module.Module.ID))
                ModulesPinned.RemoveModule(module.Module.ID);
            else
                ModulesPinned.AddNewModule(module.Module.ID);

        }

        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            Messenger.Default.Send(new ModuleSheetNotification { id = "-1", sheetName = "Module installer", type = ModuleSheetNotificationType.NewSheet, sheetContent = new ModulesInstaller(), sheetIcon = new BitmapImage(new Uri(this.BaseUri, "/Assets/Icons/modules_installer.png")), sheetSystem = false });
        }

        private async void DeleteAcceptButton_Click(object sender, RoutedEventArgs e)
        {
            ModuleInfosShow element = (ModuleInfosShow)((Button)sender).DataContext;

            if(await ModulesWriteManager.DeleteModuleViaIDAsync(element.Module.ID))
            {
                LoadModules();
            }
        }

        private void AddonsButton_PointerPressed(object sender, PointerRoutedEventArgs e)
        => ChangeSelectedButton(0);

        private void ThemesButton_PointerPressed(object sender, PointerRoutedEventArgs e)
        => ChangeSelectedButton(1);

        private void ProgLanguagesButton_PointerPressed(object sender, PointerRoutedEventArgs e)
        => ChangeSelectedButton(2);

        private void ProjectTypesButton_PointerPressed(object sender, PointerRoutedEventArgs e)
        => ChangeSelectedButton(3);

        private void MenuButton_Entered(object sender, PointerRoutedEventArgs e)
        => Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Hand, 1);

        private void MenuButton_Exited(object sender, PointerRoutedEventArgs e)
        => Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);




        /* =============
         * = VARIABLES =
         * =============
         */



        int currentSelectedButton = -1;
    }
}
