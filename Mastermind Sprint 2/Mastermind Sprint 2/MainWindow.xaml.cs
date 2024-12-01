﻿using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows;

namespace Mastermind
{
    public partial class MainWindow : Window
    {
        DispatcherTimer timer = new DispatcherTimer();
        private string[] colors = { "Red", "Yellow", "Orange", "White", "Green", "Blue" };
        private string[] secretCode;
        private List<Brush> ellipseColor = new List<Brush> { Brushes.Red, Brushes.Yellow, Brushes.Orange, Brushes.White, Brushes.Green, Brushes.Blue };

        int attempts = 0;
        int countDown = 10;
        int totalScore = 100;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
            StartCountDown();
        }

        // --------------------------- Game Initialization Methods ---------------------------

        private void InitializeGame()
        {
            Random number = new Random();
            secretCode = Enumerable.Range(0, 4)
                             .Select(_ => colors[number.Next(colors.Length)])
                             .ToArray();
            cheatCode.Text = string.Join(" ", secretCode);
            Title = ($"{attempts}");
            totalScore = 100;
            scoreLabel.Text = $"Score: {totalScore}";
            attempts = 0;
            countDown = 10;
            historyPanel.Children.Clear();
            ResetAllColors();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            countDown--;
            timerCounter.Text = $"{countDown}";
            if (countDown == 0)
            {
                attempts++;
                timer.Stop();
                if (attempts >= 10)
                {
                    GameOver();
                    return;
                }
                MessageBox.Show("Poging kwijt");
                StopCountDown();
                UpdateTitle();
            }
        }

        private void StartCountDown()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void StopCountDown()
        {
            timer.Stop(); // Stop the timer before resetting
            countDown = 10;
            timer.Start(); // Start the timer again
        }

        private void GameOver()
        {
            MessageBox.Show($"Game Over! De code was: {string.Join(" ", secretCode)}", "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
            InitializeGame();
        }

        // --------------------------- Player Interaction Methods ---------------------------

        private void ControlButton_Click(object sender, RoutedEventArgs e)
        {
            List<Ellipse> ellipses = new List<Ellipse> { kleur1, kleur2, kleur3, kleur4 };
            string[] selectedColors = ellipses.Select(e => GetColorName(e.Fill)).ToArray();

            if (selectedColors.Any(color => color == "Transparent"))
            {
                MessageBox.Show("Selecteer vier kleuren!", "Foutief", MessageBoxButton.OK);
                return;
            }

            attempts++;
            UpdateTitle();

            if (attempts >= 10)
            {
                GameOver();
                return;
            }

            CheckGuess(selectedColors);
            AddAttemptToHistory(selectedColors);
            UpdateScoreLabel(selectedColors);
            StopCountDown();
        }

        private void CheckGuess(string[] selectedColors)
        {
            int correctPosition = 0;
            int correctColor = 0;

            List<string> tempSecretCode = new List<string>(secretCode);
            List<string> tempPlayerGuess = new List<string>(selectedColors);

            // Step 1: Check correct positions (color and position)
            for (int i = 0; i < 4; i++)
            {
                if (tempPlayerGuess[i] == tempSecretCode[i])
                {
                    correctPosition++;
                    tempSecretCode[i] = null;
                    tempPlayerGuess[i] = null;
                }
            }

            // Step 2: Check correct colors (wrong position)
            for (int i = 0; i < 4; i++)
            {
                if (tempPlayerGuess[i] != null)
                {
                    int index = tempSecretCode.IndexOf(tempPlayerGuess[i]);
                    if (index != -1)
                    {
                        correctColor++;
                        tempSecretCode[index] = null;
                    }
                }
            }

            if (correctPosition == 4)
            {
                MessageBox.Show($"Proficiat! Je hebt de code gekraakt in {attempts} pogingen! Spel herstarten?", "WINNER WINNER CHICKEN DINNER", MessageBoxButton.OK);
            }
        }

        private void AddAttemptToHistory(string[] selectedColors)
        {
            StackPanel attemptPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            for (int i = 0; i < selectedColors.Length; i++)
            {
                Ellipse colorBox = new Ellipse
                {
                    Width = 50,
                    Height = 50,
                    Fill = GetBrushFromColorName(selectedColors[i]),
                    StrokeThickness = 5,
                    Stroke = GetFeedbackBorder(selectedColors[i], i)
                };
                attemptPanel.Children.Add(colorBox);
            }

            historyPanel.Children.Add(attemptPanel);
        }

        private void UpdateScoreLabel(string[] selectedColors)
        {
            int scorePenalty = 0;

            for (int i = 0; i < selectedColors.Length; i++)
            {
                if (selectedColors[i] == secretCode[i])
                {
                    // 0 penalty: correct color and position
                    continue;
                }
                else if (secretCode.Contains(selectedColors[i]))
                {
                    // 1 penalty: correct color, wrong position
                    scorePenalty += 1;
                }
                else
                {
                    // 2 penalties: color not in code
                    scorePenalty += 2;
                }
            }

            totalScore -= scorePenalty;
            if (totalScore < 0) totalScore = 0; // Ensure score doesn't go negative

            scoreLabel.Text = $"Score: {totalScore}";
        }

        private void UpdateTitle()
        {
            this.Title = $"Poging {attempts}";
        }

        // --------------------------- Helper Methods ---------------------------

        private void ResetAllColors()
        {
            List<Ellipse> ellipses = new List<Ellipse> { kleur1, kleur2, kleur3, kleur4 };

            foreach (Ellipse ellipse in ellipses)
            {
                ellipse.Fill = Brushes.Red;
                ellipse.Stroke = Brushes.Transparent;
            }
        }

        private string GetColorName(Brush brush)
        {
            if (brush == Brushes.Red) return "Red";
            if (brush == Brushes.Yellow) return "Yellow";
            if (brush == Brushes.Orange) return "Orange";
            if (brush == Brushes.White) return "White";
            if (brush == Brushes.Green) return "Green";
            if (brush == Brushes.Blue) return "Blue";
            return "Transparent";
        }

        private Brush GetBrushFromColorName(string colorName)
        {
            switch (colorName)
            {
                case "Red": return Brushes.Red;
                case "Yellow": return Brushes.Yellow;
                case "Orange": return Brushes.Orange;
                case "White": return Brushes.White;
                case "Green": return Brushes.Green;
                case "Blue": return Brushes.Blue;
                default: return Brushes.Transparent;
            }
        }

        private Brush GetFeedbackBorder(string color, int index)
        {
            if (color == secretCode[index])
            {
                return Brushes.DarkRed; // Correct color and position
            }
            else if (secretCode.Contains(color))
            {
                return Brushes.Wheat; // Correct color, wrong position
            }
            else
            {
                return Brushes.Transparent; // Incorrect color
            }
        }

        // --------------------------- Debugging Methods ---------------------------

        private void Toggledebug()
        {
            if (cheatCode.Visibility == Visibility.Hidden)
            {
                cheatCode.Visibility = Visibility.Visible;
            }
            else if (cheatCode.Visibility == Visibility.Visible)
            {
                cheatCode.Visibility = Visibility.Hidden;
            }
        }

        private void cheatCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.F12)
            {
                Toggledebug();
            }
        }

        // --------------------------- Event Handlers ---------------------------

        private void color_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Ellipse clickedEllipse = sender as Ellipse;
            int currentColorIndex = ellipseColor.IndexOf(clickedEllipse.Fill);
            int nextColorIndex = (currentColorIndex + 1) % ellipseColor.Count;
            clickedEllipse.Fill = ellipseColor[nextColorIndex];
        }
    }
}
