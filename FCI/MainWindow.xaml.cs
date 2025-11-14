using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FakeCheatIndicator
{
    public partial class MainWindow : Window
    {
        private IndicatorWindow? indicatorWindow;
        private readonly Settings settings;

        public MainWindow()
        {
            InitializeComponent();
            settings = Settings.Instance;
            LoadSettings();
            UpdateSystemInfo();
        }

        private void LoadSettings()
        {
            try
            {
               
                SliderOpacity.ValueChanged -= SliderOpacity_ValueChanged;
                SliderSize.ValueChanged -= SliderSize_ValueChanged;
                SliderBorder.ValueChanged -= SliderBorder_ValueChanged;
                ChkAutoStart.Checked -= ChkAutoStart_Changed;
                ChkAutoStart.Unchecked -= ChkAutoStart_Changed;
                ChkMinimizeToTray.Checked -= ChkMinimizeToTray_Changed;
                ChkMinimizeToTray.Unchecked -= ChkMinimizeToTray_Changed;
                ChkStartMinimized.Checked -= ChkStartMinimized_Changed;
                ChkStartMinimized.Unchecked -= ChkStartMinimized_Changed;
                ChkSavePosition.Checked -= ChkSavePosition_Changed;
                ChkSavePosition.Unchecked -= ChkSavePosition_Changed;
                
                SliderOpacity.Value = settings.Opacity;
                SliderSize.Value = settings.Size;
                SliderBorder.Value = settings.BorderThickness;
                TxtPosX.Text = settings.PositionX.ToString();
                TxtPosY.Text = settings.PositionY.ToString();
                CmbColor.SelectedIndex = settings.ColorIndex;
                ChkAutoStart.IsChecked = settings.AutoStart;
                ChkMinimizeToTray.IsChecked = settings.MinimizeToTray;
                ChkStartMinimized.IsChecked = settings.StartMinimized;
                ChkSavePosition.IsChecked = settings.SavePosition;
                
                
                TxtOpacity.Text = $"{settings.Opacity}%";
                TxtSize.Text = $"{settings.Size} px";
                TxtBorder.Text = $"{settings.BorderThickness} px";
                
                
                SliderOpacity.ValueChanged += SliderOpacity_ValueChanged;
                SliderSize.ValueChanged += SliderSize_ValueChanged;
                SliderBorder.ValueChanged += SliderBorder_ValueChanged;
                ChkAutoStart.Checked += ChkAutoStart_Changed;
                ChkAutoStart.Unchecked += ChkAutoStart_Changed;
                ChkMinimizeToTray.Checked += ChkMinimizeToTray_Changed;
                ChkMinimizeToTray.Unchecked += ChkMinimizeToTray_Changed;
                ChkStartMinimized.Checked += ChkStartMinimized_Changed;
                ChkStartMinimized.Unchecked += ChkStartMinimized_Changed;
                ChkSavePosition.Checked += ChkSavePosition_Changed;
                ChkSavePosition.Unchecked += ChkSavePosition_Changed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки настроек: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateSystemInfo()
        {
            TxtSystemInfo.Text = $"ОС: {Environment.OSVersion}\n" +
                               $".NET: {Environment.Version}\n" +
                               $"Разрешение экрана: {SystemParameters.PrimaryScreenWidth}x{SystemParameters.PrimaryScreenHeight}";
        }

       
        private void BtnActivation_Click(object sender, RoutedEventArgs e)
        {
            ShowPage(PageActivation, BtnActivation);
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            ShowPage(PageSettings, BtnSettings);
        }

        private void BtnAbout_Click(object sender, RoutedEventArgs e)
        {
            ShowPage(PageAbout, BtnAbout);
        }

        private void ShowPage(UIElement page, Button activeButton)
        {
            PageActivation.Visibility = Visibility.Collapsed;
            PageSettings.Visibility = Visibility.Collapsed;
            PageAbout.Visibility = Visibility.Collapsed;
            
            page.Visibility = Visibility.Visible;

            BtnActivation.Style = (Style)FindResource("SidebarButtonStyle");
            BtnSettings.Style = (Style)FindResource("SidebarButtonStyle");
            BtnAbout.Style = (Style)FindResource("SidebarButtonStyle");
            
            activeButton.Style = (Style)FindResource("SidebarButtonActiveStyle");
        }

       
        private void BtnToggle_Click(object sender, RoutedEventArgs e)
        {
            if (indicatorWindow == null || !indicatorWindow.IsVisible)
            {
                ActivateIndicator();
            }
            else
            {
                DeactivateIndicator();
            }
        }

        private void ActivateIndicator()
        {
            if (indicatorWindow == null)
            {
                indicatorWindow = new IndicatorWindow();
            }
            
            indicatorWindow.UpdateFromSettings();
            indicatorWindow.Show();
            
            BtnToggle.Content = "Деактивировать индикатор";
            StatusIndicator.Fill = (SolidColorBrush)FindResource("SuccessBrush");
            StatusText.Text = "Активен";
        }

        private void DeactivateIndicator()
        {
            indicatorWindow?.Hide();
            
            BtnToggle.Content = "Активировать индикатор";
            StatusIndicator.Fill = (SolidColorBrush)FindResource("DangerBrush");
            StatusText.Text = "Неактивен";
        }

        
        private void SliderOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtOpacity == null || !IsLoaded) return;
            
            TxtOpacity.Text = $"{(int)SliderOpacity.Value}%";
            settings.Opacity = (int)SliderOpacity.Value;
            indicatorWindow?.UpdateFromSettings();
        }

        private void SliderSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtSize == null || !IsLoaded) return;
            
            TxtSize.Text = $"{(int)SliderSize.Value} px";
            settings.Size = (int)SliderSize.Value;
            indicatorWindow?.UpdateFromSettings();
        }

        private void SliderBorder_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtBorder == null || !IsLoaded) return;
            
            TxtBorder.Text = $"{(int)SliderBorder.Value} px";
            settings.BorderThickness = (int)SliderBorder.Value;
            indicatorWindow?.UpdateFromSettings();
        }

        private void CmbColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded || CmbColor.SelectedIndex < 0) return;
            
            settings.ColorIndex = CmbColor.SelectedIndex;
            indicatorWindow?.UpdateFromSettings();
        }

        
        private void TxtPos_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) return;
            
            if (int.TryParse(TxtPosX.Text, out int x) && int.TryParse(TxtPosY.Text, out int y))
            {
                settings.PositionX = x;
                settings.PositionY = y;
                indicatorWindow?.UpdatePosition();
            }
        }

        private void BtnResetPosition_Click(object sender, RoutedEventArgs e)
        {
            TxtPosX.Text = "25";
            TxtPosY.Text = "15";
            settings.PositionX = 25;
            settings.PositionY = 15;
            indicatorWindow?.UpdatePosition();
        }

        
        private void ChkAutoStart_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded || settings == null) return;
            settings.AutoStart = ChkAutoStart.IsChecked ?? false;
        }

        private void ChkMinimizeToTray_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded || settings == null) return;
            settings.MinimizeToTray = ChkMinimizeToTray.IsChecked ?? false;
        }

        private void ChkStartMinimized_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded || settings == null) return;
            settings.StartMinimized = ChkStartMinimized.IsChecked ?? false;
        }

        private void ChkSavePosition_Changed(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded || settings == null) return;
            settings.SavePosition = ChkSavePosition.IsChecked ?? false;
        }

        private void BtnApplySettings_Click(object sender, RoutedEventArgs e)
        {
            settings.Save();
            MessageBox.Show("Настройки успешно сохранены!", "Сохранение", 
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected override void OnClosed(EventArgs e)
        {
            indicatorWindow?.Close();
            settings.Save();
            base.OnClosed(e);
        }
    }
}
