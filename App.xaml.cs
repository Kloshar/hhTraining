using System.Configuration;
using System.Data;
using System.Windows;

namespace hhTraining
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void AppStart(object sender, StartupEventArgs e)
        {
            if (e.Args.Count() == 0)
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
            else if (e.Args[0] == "-a")
            {
                AddingNewQuestions AddingNewQuestionsWindow = new AddingNewQuestions();
                AddingNewQuestionsWindow.Show();
            }            
        }
    }
}