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
            if (ActualWidth < 1000)
            {
                FormPanelColumn.Width = new GridLength(0);
            }
            else
            {
                FormPanelColumn.Width = new GridLength(320);
            }
        }
    }
}
