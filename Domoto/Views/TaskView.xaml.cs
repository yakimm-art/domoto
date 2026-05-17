using System.Windows;
using System.Windows.Controls;

namespace Domoto.Views
{
    public partial class TaskView : UserControl
    {
        public TaskView()
        {
            InitializeComponent();
        }

        private void TaskView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FormPanelColumn.Width = new GridLength(0);
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            ShortcutsPopup.IsOpen = !ShortcutsPopup.IsOpen;
        }
    }
}
