using System.Windows;

namespace SoloFileManager
{
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
