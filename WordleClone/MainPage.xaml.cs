using System.Net.Http;
using System.IO;
using Microsoft.Maui.Storage;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;


namespace WordleClone
{
    public partial class MainPage : ContentPage
    {

        // Declaring the variables
        int attempts = 0;
        const int maxAttempts = 6;
        string correctWord;
        private List<List<Entry>> allRows;
        public MainPage()
        {
            InitializeComponent();

            // Calling the method to initialize the game asynchronously
            InitializingGameAsync();

            // Initializing all the rows starting from the second one in order to have them disabled on game startup
            allRows = new List<List<Entry>>()
            {
                new List<Entry> { Letter1Row1, Letter2Row1, Letter3Row1, Letter4Row1, Letter5Row1 },
                new List<Entry> { Letter1Row2, Letter2Row2, Letter3Row2, Letter4Row2, Letter5Row2 },
                new List<Entry> { Letter1Row3, Letter2Row3, Letter3Row3, Letter4Row3, Letter5Row3 },
                new List<Entry> { Letter1Row4, Letter2Row4, Letter3Row4, Letter4Row4, Letter5Row4 },
                new List<Entry> { Letter1Row5, Letter2Row5, Letter3Row5, Letter4Row5, Letter5Row5 },
                new List<Entry> { Letter1Row6, Letter2Row6, Letter3Row6, Letter4Row6, Letter5Row6 }
            };

            // Calling the method that disables all rows except the first one
            DisableAllRowsExceptFirst();
        }

        // Creating a constructor / task that downloads the words file if it does not exist

        bool isGameInitialized = false;
        private async Task InitializingGameAsync()
        {
            var localFilePath = Path.Combine(FileSystem.AppDataDirectory, "words.txt");

            if (!File.Exists(localFilePath))
            {
                await DownloadAndSaveWordList();
            }

            correctWord = GetRandomWord();
            isGameInitialized = true;
        }

        // Creating a function that gives feedback about the guess
        private List<string> GetFeedback(string guess, string correctWord)
        {
            List<string> feedback = new List<string>();

            // Converting the guess to lowercase to allow for the game logic to take in uppercase letters in the input boxes
            guess = guess.ToLower();

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

            if (!Directory.Exists(FileSystem.AppDataDirectory))
            {
                Directory.CreateDirectory(FileSystem.AppDataDirectory);
            }
            using (var client = new HttpClient())
            {
                var wordsData = await client.GetStringAsync(fileUrl);
                await File.WriteAllTextAsync(localFilePath, wordsData);
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
        // Now I will adjust the OnSubmitGuess method to call the GetLetterColour method in order to apply each color to the letter input box
        private async void OnSubmitGuess(object sender, EventArgs e)
        {

            // Adding animation to te button which will scale the button down slightly when pressed and back to normal
            await SubmitButton.ScaleTo(0.9, 100);
            await SubmitButton.ScaleTo(1.0, 100);

            // Adding an if statement to check if the game is properly initialized before playing
            if (!isGameInitialized)
            {
                await DisplayAlert("Game is not ready yet!", "Please wait while the game is loading.", "Okay");
                return;
            }
            string guess = Letter1Row1.Text + Letter2Row1.Text + Letter3Row1.Text + Letter4Row1.Text + Letter5Row1.Text;

            // Adding validation to the guess
            if (guess.Length != 5 || !guess.All(char.IsLetter))
            {
                await DisplayAlert("Invalid Input", "Please enter a valid 5-letter word.", "OK");
                return;
            }

            // Adding validation to check if the word entered is in the word list and also allow for case insensitive entries such as "APPLE", "Apple" and "apple".
            var words = File.ReadAllLines(Path.Combine(FileSystem.AppDataDirectory, "words.txt"));
            if (!words.Contains(guess.ToLower()))
            {
                await DisplayAlert("Invalid Word", "Not in word list!", "OK");
                return;
            }

            attempts++;

            // Processing the guess

            var feedback = GetFeedback(guess, correctWord);

            // Displaying feedback
            Debug.WriteLine(string.Join(", ", feedback));

            // Displaying the letter colour based on the feedback from the game logic
            GetLetterColour(feedback);

            if (guess == correctWord)
            {
                await DisplayAlert("You Win!", $"You guessed the word in {attempts} attempts.", "Okay");
            }
            else if (attempts >= maxAttempts)
            {
                await DisplayAlert("Game Over", $"The correct word was: {correctWord}", "Okay");
            }

            // Calling the method that enables the next row after a valid guess
            EnabledNextRow(attempts);
        }

        // Creating a method to implement the logic for the OnLetterTextChanged event which allows for shifting focus automatically
        // Editing this method to shift focus when backspace is pressed or a letter is typed
        private void OnLetterTextChanged(object sender, EventArgs e)
        {
            // Declaring the variables
            var currentEntry = sender as Entry;

            if (currentEntry != null)
            {
                // Checking to see if the user typed a character
                if (currentEntry.Text.Length == 1)
                {
                    // Adding functionality to automatically shift focus to the next box
                    ShiftFocusForward(currentEntry);
                }

                // Checking to see if the user pressed backspace to automatically shift focus to the previous box
                else if (currentEntry.Text.Length == 0)
                {
                    ShiftFocusBackward(currentEntry);
                }
            }

            // Checking to see if a single character is entered (The character length matches to 1)
            /*if(currentEntry.Text.Length == 1)
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
            }*/
        }

        // Creating the move focus forward method
        private void ShiftFocusForward(Entry currentEntry)
        {
            if (currentEntry == Letter1Row1)
                Letter2Row1.Focus();
            else if (currentEntry == Letter2Row1)
                Letter3Row1.Focus();
            else if (currentEntry == Letter3Row1)
                Letter4Row1.Focus();
            else if (currentEntry == Letter4Row1)
                Letter5Row1.Focus();
        }

        // Creating the move focus backward method
        private void ShiftFocusBackward(Entry currentEntry)
        {
            if (currentEntry == Letter5Row1)
                Letter4Row1.Focus();
            else if (currentEntry == Letter4Row1)
                Letter3Row1.Focus();
            else if (currentEntry == Letter3Row1)
                Letter2Row1.Focus();
            else if (currentEntry == Letter2Row1)
                Letter1Row1.Focus();
        }

        // Creating a method to update the colour of each letter based on the feedback of the game logic
        private void GetLetterColour(List<string> feedback)
        {
            SetLetterColour(Letter1Row1, feedback[0]);
            SetLetterColour(Letter2Row1, feedback[1]);
            SetLetterColour(Letter3Row1, feedback[2]);
            SetLetterColour(Letter4Row1, feedback[3]);
            SetLetterColour(Letter5Row1, feedback[4]);
        }

        // Creating a method that allows to set the colour of each letter based on the feedback of the game logic once it has been updated in the GetLetterColour method
        private void SetLetterColour(Entry currentEntry, string colour)
        {
            if (colour == "Green")
            {
                currentEntry.BackgroundColor = Colors.DarkGreen;
            }
            else if (colour == "Yellow")
            {
                currentEntry.BackgroundColor = Colors.Orange;
            }
            else
            {
                currentEntry.BackgroundColor = Colors.Gray;
            }
        }

        // Creating a method to disable all rows except first one
        private void DisableAllRowsExceptFirst()
        {
            // Checking through each row starting frome the second one
            for (int i = 1; i < allRows.Count; i++)
            {
                foreach (var entry in allRows[i])
                {
                    entry.IsEnabled = false;
                }
            }
        }

        // Creating a method to enable the next row after the user inputs a valid guess
        private void EnabledNextRow(int rowLocation)
        {
            if (rowLocation + 0 < allRows.Count)
            {
                foreach (var entry in allRows[rowLocation + 0])
                {
                    entry.IsEnabled = true;
                }
            }
        }
    }
}
