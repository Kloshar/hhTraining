using Npgsql;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace hhTraining
{
    public partial class MainWindow : Window
    {
        string connectionString = @"Host=localhost;Username=postgres;Password=chistiyList;Database=hhtraining";

        public MainWindow()
        {
            InitializeComponent();
            //GetData(1);
            startPostgreServer();
        }
        void startPostgreServer()
        {
            Debug.WriteLine("Debug.WriteLine is working!");

            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.FileName = "cmd.exe";
            processInfo.Arguments = "dir c:";
            processInfo.CreateNoWindow = false; //не запускать новое окно
            processInfo.WorkingDirectory = Environment.CurrentDirectory;
            processInfo.UseShellExecute = false; //использовать стандартные потоки приложения            
            processInfo.RedirectStandardOutput = true;
            processInfo.RedirectStandardInput = true;           

            Process p = Process.Start(processInfo);                       

            using(StreamWriter sw = p.StandardInput)
            {
                sw.WriteLine("dir");
                sw.WriteLine(Console.ReadLine());
                p.WaitForExit();
            }            

            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            Debug.WriteLine(output);
        }

        async void GetData(int id)
        {            
            await using var dataSource = NpgsqlDataSource.Create(connectionString);
            await using (var cmd = dataSource.CreateCommand("SELECT text FROM questions WHERE id = " + id))
            await using (var reader = await  cmd.ExecuteReaderAsync()) 
            {
                while (reader.Read())
                {
                    string text = reader.GetValue(0).ToString();
                    question.Text = text;
                }
            }
            await using (var cmd = dataSource.CreateCommand("SELECT text FROM answers WHERE questionid = " + id))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    string text = reader.GetValue(0).ToString();
                    //Debug.WriteLine($"{reader.GetValue(0)}");
                    RadioButton radioButton = new RadioButton();
                    radioButton.Content = text;
                    answers.Children.Add(radioButton);
                }
            }
        }
    }
}