using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
namespace hhTraining
{
    public partial class AddingNewQuestions : Window
    {
        string connectionString = @"Host=localhost;Username=postgres;Password=chistiyList;Database=hhtraining";
        public AddingNewQuestions()
        {
            InitializeComponent();

            QuestionData question = new QuestionData();

            using (StreamReader sr = new StreamReader("LastInput.json"))
            {
                string json = sr.ReadToEnd();
                question = JsonSerializer.Deserialize<QuestionData>(json);

                themeBox.Text = question.theme;
                numberBox.Text = question.number;
                questionBox.Text = question.question;
                codeBox.Text = question.code;
                answersBox.Text = question.answers;
            }
        }
        private void AddQuestionWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            QuestionData question = new QuestionData()
            {
                theme = themeBox.Text,
                number = numberBox.Text,
                question = questionBox.Text,
                code = codeBox.Text,
                answers = answersBox.Text
            };
            
            string json = JsonSerializer.Serialize(question);

            using (StreamWriter sw = new StreamWriter("LastInput.json"))
            {
                sw.Write(json);
            }
        }
        private async void SaveToDB_Click(object sender, RoutedEventArgs e)
        {
            string[] anwwerz = answersBox.Text.Split('\n');
            //Debug.WriteLine(string.Join(' ', anwwerz));

            var codeValue = codeBox.Text == "" ? null : $"'{codeBox.Text}'"; //эксперимент...

            string cmdInsertInQuestion = $"INSERT INTO questions (level, text, code, subject) " +
                $"VALUES ({numberBox.Text}, '{questionBox.Text}', {codeValue}, '{themeBox.Text}') RETURTING id";

            await using var dataSource = NpgsqlDataSource.Create(connectionString);
            await using var connection = await dataSource.OpenConnectionAsync();
            try {
                await using var transaction = await connection.BeginTransactionAsync();
                await using var command1 = new NpgsqlCommand(cmdInsertInQuestion, connection, transaction);
                //await command1.ExecuteNonQueryAsync();
                var num = await command1.ExecuteScalarAsync();
                Debug.WriteLine(num);

                foreach (string s in anwwerz)
                {
                    //сначала нужно получить questionid !!!
                    //далее нужно выбрать правильный ответ!!!

                    string cmdInsertInAnswers = $"INSERT INTO answers (questionid, text, isright, subject) " +
                    $"VALUES ({numberBox.Text}, '{questionBox.Text}', '{codeBox.Text}', '{themeBox.Text}')";
                    
                    await using var command2 = new NpgsqlCommand(cmdInsertInAnswers, connection, transaction);
                    await command2.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                MessageBox.Show("Данные добавлены!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Данные не добавелены! Ошибка: {ex.Message}");
            }

            //await using (var cmd = dataSource.CreateCommand($"INSERT INTO questions (level, text, code, subject) VALUES " +
            //    $"({numberBox.Text}, '{questionBox.Text}', '{codeBox.Text}', '{themeBox.Text}')"))
            //    {   
            //    try { 
            //        cmd.ExecuteNonQuery();
            //        MessageBox.Show("Данные добавлены!");
            //    }
            //    catch (Exception ex) 
            //    {
            //        MessageBox.Show($"Данные не добавелены! Ошибка: {ex.Message}");
            //    }
            //}
        }
    }
    [Serializable]
    public class QuestionData
    {
        public string theme { get; set; }
        public string number { get; set; }
        public string question { get; set; }
        public string code { get; set; }
        public string answers { get; set; }
    }
}
