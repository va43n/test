using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace _5_crypto_2_app
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void FirstChooseButton_Click(object sender, RoutedEventArgs e)
        {
            string output;
            try
            {
                StartParameters sp = new StartParameters(EnterProbsTextBox.Text);

                output = sp.CodeMessage(EnterInputTextBox.Text);
                ResultTextBox.Text = output;

                CodeWordsTextBox.Text = "";
                for(int i = 0; i < sp.N; i++)
                {
                    CodeWordsTextBox.Text += sp.names[i] + " -> " + sp.code_words[i] + Environment.NewLine;
                }

                CharacteristicsTextBox.Text = "Средняя длина кодового слова - " + sp.average_length + Environment.NewLine;
                CharacteristicsTextBox.Text += "Избыточность - " + sp.redundancy + Environment.NewLine;
                CharacteristicsTextBox.Text += "Неравенство Крафта - сумма равна " + sp.KraftInequality + " ";
                if (sp.KraftInequality < 1) { CharacteristicsTextBox.Text += "< 1, условие выполняется."; }
                else if (sp.KraftInequality == 1) { CharacteristicsTextBox.Text += "= 1, оптимальная кодировка."; }
                else { CharacteristicsTextBox.Text += "> 1, условие не выполняется."; }
            }
            catch (Exception exc)
            {
                MessageDialog message = new MessageDialog(exc.Message);
                message.ShowAsync().AsTask();
            }
        }

        private void SecondChooseButton_Click(object sender, RoutedEventArgs e)
        {
            string output;
            try
            {
                StartParameters sp = new StartParameters(EnterProbsTextBox.Text);

                output = sp.DecodeMessage(EnterInputTextBox.Text);
                ResultTextBox.Text = output;

                CodeWordsTextBox.Text = "";
                for (int i = 0; i < sp.N; i++)
                {
                    CodeWordsTextBox.Text += sp.names[i] + " -> " + sp.code_words[i] + Environment.NewLine;
                }

                CharacteristicsTextBox.Text = "Средняя длина кодового слова - " + sp.average_length + Environment.NewLine;
                CharacteristicsTextBox.Text += "Избыточность - " + sp.redundancy + Environment.NewLine;
                CharacteristicsTextBox.Text += "Неравенство Крафта - сумма равна " + sp.KraftInequality + " ";
                if (sp.KraftInequality < 1) { CharacteristicsTextBox.Text += "< 1, условие выполняется."; }
                else if (sp.KraftInequality == 1) { CharacteristicsTextBox.Text += "= 1, оптимальная кодировка."; }
                else { CharacteristicsTextBox.Text += "> 1, условие не выполняется."; }
            }
            catch (Exception exc)
            {
                MessageDialog message = new MessageDialog(exc.Message);
                message.ShowAsync().AsTask();
            }
        }
    }


    class StartParameters
    {
        public readonly int N;
        public readonly double[] probabilities;
        public readonly string[] names;
        public readonly string[] code_words;
        private readonly string[] indices;
        public readonly int max_code_word_length;
        public readonly double average_length;
        public readonly double redundancy;
        public readonly double KraftInequality;

        public StartParameters(string probs)
        {
            string[] parameters;
            int counter = 0;
            double temp = 0;
            int min1, min2;
            bool[] used;
            string text = probs;


            if (text == null || !text.Contains(" ")) { throw new Exception("Неправильная запись алфавита. Каждый символ должна сопровождаться вероятностью её появления, между символами и вероятностями должны стоять пробелы"); }
            parameters = text.Split(' ');

            N = parameters.Length;
            if (N % 2 != 0) { throw new Exception("Элементов должно быть четное число, нечетные элементы - буквы алфавита, четные элементы - соответствующие буквам вероятности. Скорее всего элементов не хватает."); }

            if (N < 4) { throw new Exception("Символов введено слишком мало. Введите хотя бы два символа и повторите попытку."); }
            N /= 2;

            names = new string[N];
            code_words = new string[N];
            probabilities = new double[N + N];
            probabilities[N + N - 1] = 2;

            foreach (string p in parameters)
            {
                if (counter % 2 == 0)
                {
                    if (Array.Exists(names, element => element == p)) { throw new Exception("Алфавит должен состоять из неповторяющихся элементов. Элемент \"" + p + "\" повторяется."); }
                    names[counter / 2] = p;
                }
                else
                {
                    try { probabilities[(counter - 1) / 2] = Convert.ToDouble(p); }
                    catch { throw new Exception("Вероятность \"" + p + "\" не является числом. Возможно следует поменять \".\" на \",\"."); }

                    if (probabilities[(counter - 1) / 2] < 0) { throw new Exception("Вероятность \"" + p + "\" меньше 0, а должна быть больше или равна 0."); }

                    temp += probabilities[(counter - 1) / 2];
                }

                counter++;
            }

            if (!(temp + Math.Pow(10, -10) >= 1 && temp - Math.Pow(10, -10) <= 1)) { throw new Exception("Сумма всех вероятностей должна быть равна 1. Сейчас она равна " + temp + "."); }

            indices = new string[N + N - 1];
            used = new bool[N + N - 1];

            for (int i = 0; i < N; i++) { indices[i] = Convert.ToString(i); }

            for (int i = N; i < 2 * N - 1; i++)
            {
                min1 = N + N - 1;
                min2 = N + N - 1;
                for (int j = 0; j < i; j++)
                {
                    if (probabilities[j] < probabilities[min1] && j != min1 && !used[j])
                    {
                        min2 = min1;
                        min1 = j;
                    }
                    else if (probabilities[j] < probabilities[min2] && j != min1 && !used[j]) { min2 = j; }
                }
                probabilities[i] = probabilities[min1] + probabilities[min2];
                indices[i] = Convert.ToString(min1) + ":" + Convert.ToString(min2);
                used[min1] = true;
                used[min2] = true;
            }

            FindCodeWords(N + N - 2, "");

            max_code_word_length = 0;
            KraftInequality = 0;
            temp = 0;
            for (int i = 0; i < N; i++)
            {
                temp += code_words[i].Length;
                KraftInequality += Math.Pow(2, -code_words[i].Length);
                if (code_words[i].Length > max_code_word_length) { max_code_word_length = code_words[i].Length; }
            }
            average_length = temp / N;

            redundancy = 0;
            for (int i = 0; i < N; i++) { redundancy += probabilities[i] * Math.Log10(probabilities[i]); }
            redundancy /= Math.Log10(N) / Math.Log10(2);
            redundancy += 1;

        }

        private void FindCodeWords(int i, string current_code_word)
        {
            string[] halfs;
            halfs = indices[i].Split(':');
            if (Convert.ToInt32(halfs[0]) < N) { code_words[Convert.ToInt32(halfs[0])] = current_code_word + "0"; }
            else { FindCodeWords(Convert.ToInt32(halfs[0]), current_code_word + "0"); }
            if (Convert.ToInt32(halfs[1]) < N) { code_words[Convert.ToInt32(halfs[1])] = current_code_word + "1"; }
            else { FindCodeWords(Convert.ToInt32(halfs[1]), current_code_word + "1"); }
        }

        public string CodeMessage(string input)
        {
            string text, output = "";
            bool isElementConsists;

            text = input;
            if (text == null) { throw new Exception("Вы не ввели строку для кодирования."); }

            foreach (char letter in text)
            {
                isElementConsists = false;
                for (int i = 0; i < N; i++)
                {
                    if (names[i] == Convert.ToString(letter))
                    {
                        isElementConsists = true;
                        output += code_words[i];
                        break;
                    }
                }
                if (!isElementConsists) { throw new Exception("Элемент " + Convert.ToString(letter) + " не существует в алфавите."); }
            }

            return output;
        }

        public string DecodeMessage(string input)
        {
            string text, code_word = "", output = "";

            text = input;
            if (text == null) { throw new Exception("Вы не ввели строку для декодирования."); }

            foreach (char letter in text)
            {
                code_word += Convert.ToString(letter);
                for (int i = 0; i < N; i++)
                {
                    if (code_word == code_words[i])
                    {
                        output += names[i];
                        code_word = "";
                        break;
                    }
                }
                if (code_word.Length == max_code_word_length)
                {
                    throw new Exception("Текст невозможно декодировать, проверьте правильность ввода и повторите попытку.");
                }
            }

            return output;
        }
    }
}
