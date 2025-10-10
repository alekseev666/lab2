using System.Windows;

namespace lab2.Views
{
    /// <summary>
    /// Окно справки с подсказками и объяснениями
    /// </summary>
    public partial class HelpWindow : Window
    {
        public HelpWindow()
        {
            InitializeComponent();
        }

        // Обработчик кнопки "Понятно!" - закрывает окно
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}