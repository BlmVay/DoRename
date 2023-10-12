using do9Rename.ViewModel;
using System;
using System.Windows;

namespace do9Rename.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ListView_Drop(object sender, DragEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.DragFilesCommand.Invoke(sender,e);
            }
        }
    }
}
