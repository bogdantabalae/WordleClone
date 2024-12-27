using System.Net.Http;
using System.IO;
using Microsoft.Maui.Storage;

namespace WordleClone
{
    public partial class MainPage : ContentPage
    {

        // Declaring the variables
        int attempts = 0;
        const int maxAttempts = 6;
        string correctWord;
        public MainPage()
        {
            InitializeComponent();
        }

        // Creating a function that gives feedback about the guess
        private List<string> GetFeedback(string guess, string correctWord)
        {
            List<string> feedback = new List<string>();

            for (int i = 0; i < 5; i++)
            {
                if (guess[i] == correctWord[i])
                {
                    feedback.Add("Green");
                }
                else if (correctWord.Contains(guess[i]))
                {
                    feedback.Add("Yellow");
                }
                else
                {
                    feedback.Add("Gray");
                }
            }

            return feedback;
        }

        // Creating a method that downloads the word list and saves it locally
        private async Task DownloadAndSaveWordList()
        {
            string fileUrl = "https://raw.githubusercontent.com/DonH-ITS/jsonfiles/main/words.txt";
            string localFilePath = Path.Combine(FileSystem.AppDataDirectory, "words.txt");

            if (File.Exists(localFilePath))
            {
                using (var client = new HttpClient())
                {
                    var wordsData = await client.GetStringAsync(fileUrl);
                    await File.WriteAllTextAsync(localFilePath, wordsData);
                }
            }
        }

        // Creating a method to select a random word from the list
        public string GetRandomWord()
        {
            var words = File.ReadAllLines(Path.Combine(FileSystem.AppDataDirectory, "words.txt"));
            Random rand = new Random();
            return words[rand.Next(words.Length)];
        }

        // Creating a method for the submit guess button
        private async void OnSubmitGuess(object sender, EventArgs e)
        {
            string guess = Letter1.Text + Letter2.Text + Letter3.Text + Letter4.Text + Letter5.Text;

            // Adding validation to the guess
            if (guess.Length != 5 || !guess.All(char.IsLetter))
            {
                await DisplayAlert("Invalid Input", "Please enter a valid 5-letter word.", "OK");
                return;
            }

            attempts++;

            // Processing the guess

            var feedback = GetFeedback(guess, correctWord);

            // Displaying feedback
            Console.WriteLine(string.Join(", ", feedback));

            if (guess == correctWord)
            {
                await DisplayAlert("You Win!", $"You guessed the word in {attempts} attempts.", "Okay");
            }
            else if (attempts >= maxAttempts)
            {
                await DisplayAlert("Game Over", $"The correct word was: {correctWord}", "Okay");
            }
        }

        // Creating a method to implement the logic for the OnLetterTextChanged event which allows for shifting focus automatically
        private void OnLetterTextChanged(object sender, EventArgs e)
        {
            // Declaring the variables
            var currentEntry = sender as Entry;

            // Checking to see if a single character is entered (The character length matches to 1)
            if(currentEntry.Text.Length == 1)
            {
                // Moving focus to the next box
                if(currentEntry == Letter1)
                    Letter2.Focus();
                else if (currentEntry == Letter2)
                    Letter3.Focus();
                else if (currentEntry == Letter3)
                    Letter4.Focus();
                else if (currentEntry == Letter4)
                    Letter5.Focus();
            }
        }
    }
}
