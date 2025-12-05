using Npgsql;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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
        int currentQuestion;

        public MainWindow()
        {
            InitializeComponent();
            currentQuestion = 1; //текущий номер вопроса
            GetData(currentQuestion); //загрузка первого вопроса
            //startPostgreServer();
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
        } //попытка запустить локальный сервер
        async Task<int> GetData(int id)
        {            
            await using var dataSource = NpgsqlDataSource.Create(connectionString);
            //чтение вопроса из таблицы questions
            await using (var cmd = dataSource.CreateCommand("SELECT text, code FROM questions WHERE id = " + id))
            await using (var reader = await  cmd.ExecuteReaderAsync())
            {                
                while (reader.Read())
                {                    
                    string text = reader.GetValue(0).ToString();
                    string code = reader.GetValue(1).ToString();
                    questionText.Text = text;
                    if(code != "")
                    {
                        questionCode.Visibility = Visibility.Visible;
                        questionCode.Text = code;
                    }
                    else
                    {
                        questionCode.Visibility = Visibility.Collapsed;
                    }                    
                }
            }
            //чтение ответов из таблицы answers
            await using (var cmd = dataSource.CreateCommand("SELECT text FROM answers WHERE questionid = " + id))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (reader.HasRows)
                {
                    answers.Children.Clear();
                    while (reader.Read())
                    {
                        string text = reader.GetValue(0).ToString();
                        RadioButton radioButton = new RadioButton();
                        radioButton.Content = text;
                        radioButton.Checked += (sender, args) => btnAnswer.IsEnabled = true;
                        answers.Children.Add(radioButton);
                    }
                }
                else return 0;
                return currentQuestion;
            }
        } //получение данных вопроса и ответов по номеру
        private async void btnAnswer_Click(object sender, RoutedEventArgs e)
        {
            //соединяемся с бд
            await using var dataSource = NpgsqlDataSource.Create(connectionString);
            await using (var cmd = dataSource.CreateCommand($"SELECT text FROM answers WHERE questionid = {currentQuestion} AND isright = TRUE"))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    string rightAnswer = reader.GetValue(0).ToString();
                    checkName.Text = "Правильный ответ: " + rightAnswer;
                    foreach(RadioButton rb in answers.Children)
                    {
                        if(rb.IsChecked == true)
                        {
                            string textAnswer = rb.Content.ToString();
                            Border border = new Border();
                            if (rb.Content.ToString() == rightAnswer) border.Background = Brushes.GreenYellow;
                            else border.Background = Brushes.Red;
                            TextBlock tb = new TextBlock();
                            tb.Text = textAnswer;
                            border.Child = tb;
                            rb.Content = border;
                        }
                    }
                }
            }
            answers.IsEnabled = false;
            ((Button)sender).IsEnabled = false;
        } //проверка ответа
        private async void btnNext_Click(object sender, RoutedEventArgs e)
        {
            currentQuestion += 1; //прибавляем текущее значение
            if (await GetData(currentQuestion) == 0) //если возвращается ноль
            {
                currentQuestion -= 1; //то убавляем обратно
            }
            else
            {
                checkName.Text = "";
                btnAnswer.IsEnabled = true;
                answers.IsEnabled = true;
            }                
        } //обработчик кнопки следующий вопрос
        private async void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            currentQuestion -= 1;
            if(await GetData(currentQuestion) == 0)
            {
                currentQuestion += 1;
            }
            else
            {
                checkName.Text = "";
                btnAnswer.IsEnabled = true;
                answers.IsEnabled = true;
            }
        } //обработчик кнопки предыдущий вопрос
    }
}