using System.Linq;
using System.Windows;
using System.Windows.Automation;

namespace VisualTreeHelper.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string formatName = "Password Name {0}";
        string formatHelp = "Password Help {0}";
        int i = 0;

        public MainWindow()
        {
            InitializeComponent();
            this.passwordBox.Loaded += this.OnLoadedPaswordTemplate;
        }

        private void OnLoadedPaswordTemplate(object sender, RoutedEventArgs e)
        {
            SetAllAutomationProperties(this.passwordBox);
        }

        private void SetAllAutomationProperties(DependencyObject dependencyObject)
        {
            if (dependencyObject != null)
            {
                AutomationProperties.SetName(dependencyObject, string.Format(formatName, i));
                dependencyObject.SetValue(AutomationProperties.HelpTextProperty, string.Format(formatHelp,i));
                i++;

                var children = dependencyObject.GetChildren().ToList();

                if (children != null)
                {
                    foreach(var child in children)
                    {
                        SetAllAutomationProperties(child);
                    }
                }
            }
        }
    }
}
