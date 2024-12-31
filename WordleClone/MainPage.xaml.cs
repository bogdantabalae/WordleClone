using System.Net.Http;
using System.IO;
using Microsoft.Maui.Storage;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using Microsoft.Maui.ApplicationModel;


namespace WordleClone
{
    public partial class MainPage : ContentPage
    {

        // Declaring the variables
        int attempts = 0;
        const int maxAttempts = 6;
        string correctWord;
        private List<List<Entry>> allRows;
        bool gameOver = false;
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
            // Initializing the feedback list
            List<string> feedback = new List<string> { "", "", "", "", "" };

            // Tracking used letters in the correct word 
            bool[] correctWordUsed = new bool[5];

            // Tracking the used letters in the guess
            bool[] guessUsed = new bool[5];

            // Converting the guess to lowercase to allow for the game logic to take in uppercase letters in the input boxes
            guess = guess.ToLower();

            // Marking the green letters aka the correct position and letter
            for (int i = 0; i < 5; i++)
            {
                if (guess[i] == correctWord[i])
                {
                    feedback[i] = "Green";
                    correctWordUsed[i] = true;
                    guessUsed[i] = true;
                }
            }

            // Marking the yellow letter which is included in the word again but the position is incorrect
            for (int i = 0; i < 5; i++)
            {
                if (feedback[i] == "")
                {
                    for (int j = 0; j < 5; j++)
                    {
                        // Checking if the letter exists in the correct word and if it hasn't been used already
                        if (!correctWordUsed[j] && correctWord[j] == guess[i])
                        {
                            feedback[i] = "Yellow";
                            correctWordUsed[j] = true;
                            guessUsed[i] = true;
                            break;
                        }
                    }
                }
            }

            // Marking the rest of the incorrect letters as gray
            for (int i = 0; i < 5; i++)
            {
                if (feedback[i] == "")
                {
                    feedback[i] = "Gray";
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
            // Adding an if statement to verify if the game is already over it will not allow the user to submit another guess
            if (gameOver)
            {
                return;
            }

            // Adding animation to te button which will scale the button down slightly when pressed and back to normal
            await SubmitButton.ScaleTo(0.9, 100);
            await SubmitButton.ScaleTo(1.0, 100);

            // Adding an if statement to check if the game is properly initialized before playing
            if (!isGameInitialized)
            {
                await DisplayAlert("Game is not ready yet!", "Please wait while the game is loading.", "Okay");
                return;
            }

            // Gathering the current row guess based on the number of attempts
            List<Entry> currentRow = allRows[attempts];
            string guess = "";

            // Creating a for loop which checks each entry input box in the current row
            foreach (var entry in currentRow)
            {
                guess += entry.Text;
            }

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

            // Displaying the letter colour based on the feedback from the game logic and the current row
            GetLetterColour(feedback, attempts - 1);

            // Checking for a win
            if (guess.ToLower() == correctWord)
            {
                gameOver = true;
                await DisplayAlert("You Win!", $"You guessed the word in {attempts} attempts.", "Okay");

                EndGame();
                return;
            }
            else if (attempts >= maxAttempts)
            {
                gameOver = true;
                await DisplayAlert("Game Over", $"The correct word was: {correctWord}", "Okay");

                EndGame();
                return;
            }

            // Calling the method that enables the next row after a valid guess
            EnabledNextRow(attempts);

            // Calling the method that disables the previous row after a valid guess
            DisabledPreviousRow(attempts - 2);

            // Calling the method that allows for shifting focus to the first entry of the next row
            ShiftFocusToNextRow(attempts);

            // Adding a debugging line to be able to verify the guess and the correct word 
            Debug.WriteLine($"Guess: {guess}, Correct Word: {correctWord}");
        }

        // Creating a helper method that handles the tasks at the end of the game
        private async void EndGame()
        {
            // Disabling the submit button along all the entry rows
            SubmitButton.IsEnabled = false;
            foreach (var row in allRows)
            {
                foreach (var entry in row)
                {
                    entry.IsEnabled = false;
                }
            }

            // Showing the restart button including the animation
            RestartButton.Opacity = 0;
            RestartButton.IsVisible = true;
            await RestartButton.FadeTo(1, 500);
        }

        // Creating an event handler method for the restart button
        private async void OnRestartGame(object sender, EventArgs e)
        {
            // Resetting the game state
            gameOver = false;

            // Adding button press animation to the restart button
            await RestartButton.ScaleTo(0.9, 100);
            await RestartButton.ScaleTo(1.0, 100);

            // Retry attempts
            attempts = 0;

            // Hiding the retry button
            RestartButton.IsVisible = false;

            // Enabling the submit button again 
            SubmitButton.IsEnabled = true;

            // For loop to clear all entries and reset their colours
            foreach (var row in allRows)
            {
                foreach (var entry in row)
                {
                    entry.Text = string.Empty;
                    entry.BackgroundColor = Colors.Transparent;
                    entry.IsEnabled = false;
                }
            }

            // Loop to enable the first row so that the game logic restarts again
            foreach (var entry in allRows[0])
            {
                entry.IsEnabled = true;
            }

            // Gathering a new word from the list
            correctWord = GetRandomWord();

            // Alert to let the player know the game has restarted
            await DisplayAlert("New Game!", "Welcome to Wordle. Good Luck!", "Okay");
        }

        // Creating a method to implement the logic for the OnLetterTextChanged event which allows for shifting focus automatically
        // Editing this method to shift focus when backspace is pressed or a letter is typed
        private async void OnLetterTextChanged(object sender, EventArgs e)
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

                    // Adding animation to make the letters jump once a letter has been entered in the input box
                    await currentEntry.ScaleTo(1.1, 100, Easing.CubicInOut);
                    await currentEntry.ScaleTo(1.0, 100, Easing.CubicInOut);
                }

                // Checking to see if the user pressed backspace to automatically shift focus to the previous box
                else if (currentEntry.Text.Length == 0)
                {
                    ShiftFocusBackward(currentEntry);
                }
            }
        }

        // Creating the move focus forward method
        private void ShiftFocusForward(Entry currentEntry)
        {
            // Finding the current active row
            List<Entry> currentRow = allRows.FirstOrDefault(row => row.Contains(currentEntry));

            if (currentRow != null)
            {
                int currentLocation = currentRow.IndexOf(currentEntry);
                if (currentLocation < currentRow.Count - 1)
                {
                    currentRow[currentLocation + 1].Focus();
                }
            }
        }

        // Creating the move focus backward method
        private void ShiftFocusBackward(Entry currentEntry)
        {
            // Finding the current active row
            List<Entry> currentRow = allRows.FirstOrDefault(row => row.Contains(currentEntry));

            if (currentRow != null)
            {
                int currentLocation = currentRow.IndexOf(currentEntry);
                if (currentLocation > 0)
                {
                    currentRow[currentLocation - 1].Focus();
                }
            }
        }

        // Creating a method that allows to shift focus to the next row
        private void ShiftFocusToNextRow(int rowLocation)
        {
            if (rowLocation < allRows.Count)
            {
                List<Entry> nextRow = allRows[rowLocation];
                var firstEntryInNextRow = nextRow.FirstOrDefault();
                firstEntryInNextRow?.Focus();
            }
        }

        // Creating a method to update the colour of each letter based on the feedback of the game logic
        private async void GetLetterColour(List<string> feedback, int rowLocation)
        {
            // Getting the current row based on the rowLocation
            var currentRow = allRows[rowLocation];

            // Creating a for loop in order to search through each entry in the row and apply the colour based on the feedback
            for (int i = 0; i < currentRow.Count; i++)
            {
                SetLetterColour(currentRow[i], feedback[i]);

                // Adding animation to flip the entry boxes like in the orignial wordle
                var currentEntry = currentRow[i];
                await currentEntry.RotateXTo(180, 300, Easing.CubicInOut);
                await currentEntry.RotateXTo(0, 300, Easing.CubicInOut);
            }
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

        // Creating a method to disable the previous row after the user inputs a valid guess
        private void DisabledPreviousRow(int rowLocation)
        {
            if (rowLocation >= 0 && rowLocation < allRows.Count)
            {
                foreach (var entry in allRows[rowLocation])
                {
                    entry.IsEnabled = false;
                }
            }
        }

        // Creating a method that opens up a link to the official wordle rules when the how to button is clicked
        private async void OnHowTo(object sender, EventArgs e)
        {
            string wordleRulesLink = "https://www.nytimes.com/2023/08/01/crosswords/how-to-talk-about-wordle.html";
            await Launcher.OpenAsync(wordleRulesLink);

            // Adding animation to te button which will scale the button down slightly when pressed and back to normal
            await HowToButton.ScaleTo(0.9, 100);
            await HowToButton.ScaleTo(1.0, 100);
        }
    }
}