using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Deno.Behaviors
{
    public partial class VirtualNumpad : UserControl
    {
        public VirtualNumpad()
        {
            InitializeComponent();
        }

        private void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string value = button.Content.ToString();
                IInputElement originalFocus = Keyboard.FocusedElement;
                SendKey(value);
                RestoreFocus(originalFocus);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            IInputElement originalFocus = Keyboard.FocusedElement;
            SendKey("{BACKSPACE}");
            RestoreFocus(originalFocus);
        }

        private void ClearAllButton_Click(object sender, RoutedEventArgs e)
        {
            IInputElement originalFocus = Keyboard.FocusedElement;
            if (originalFocus is TextBox textBox)
            {
                textBox.Text = string.Empty;
                textBox.CaretIndex = 0;
            }
            else if (originalFocus is PasswordBox passwordBox)
            {
                passwordBox.Clear();
            }
            RestoreFocus(originalFocus);
        }

        private void TabButton_Click(object sender, RoutedEventArgs e)
        {
            IInputElement originalFocus = Keyboard.FocusedElement;
            SendKey("{TAB}");
        }

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            IInputElement originalFocus = Keyboard.FocusedElement;
            SendKey("{ENTER}");
            RestoreFocus(originalFocus);
        }

        private void SendKey(string key)
        {
            var focusedElement = Keyboard.FocusedElement;
            if (focusedElement == null)
            {
                // Fallback to the main window's focused element
                var mainWindow = Application.Current.MainWindow;
                focusedElement = FocusManager.GetFocusedElement(mainWindow);
            }

            if (focusedElement is IInputElement inputElement)
            {
                try
                {
                    if (key == "{BACKSPACE}")
                    {
                        if (inputElement is TextBox textBox)
                        {
                            if (!string.IsNullOrEmpty(textBox.Text) && textBox.CaretIndex > 0)
                            {
                                int caretIndex = textBox.CaretIndex;
                                textBox.Text = textBox.Text.Remove(caretIndex - 1, 1);
                                textBox.CaretIndex = caretIndex - 1;
                            }
                        }
                        else if (inputElement is PasswordBox passwordBox)
                        {
                            if (!string.IsNullOrEmpty(passwordBox.Password))
                            {
                                passwordBox.Password = passwordBox.Password.Remove(passwordBox.Password.Length - 1);
                            }
                        }
                        else
                        {
                            RaiseKeyEvent(inputElement, Key.Back, true);
                        }
                    }
                    else if (key == "{TAB}")
                    {
                        if (inputElement is UIElement uiElement)
                        {
                            var request = new TraversalRequest(FocusNavigationDirection.Next);
                            uiElement.MoveFocus(request);
                        }
                        else
                        {
                            RaiseKeyEvent(inputElement, Key.Tab, true);
                        }
                    }
                    else if (key == "{ENTER}")
                    {
                        if (inputElement is TextBox || inputElement is PasswordBox)
                        {
                            RaiseKeyEvent(inputElement, Key.Enter, true);
                            RaiseKeyEvent(inputElement, Key.Enter, false);
                        }
                        else
                        {
                            RaiseKeyEvent(inputElement, Key.Enter, true);
                        }
                    }
                    else
                    {
                        // Handle text input (numbers, decimal point, "00")
                        if (inputElement is TextBox textBox)
                        {
                            int caretIndex = textBox.CaretIndex;
                            textBox.Text = textBox.Text.Insert(caretIndex, key);
                            textBox.CaretIndex = caretIndex + key.Length;
                        }
                        else if (inputElement is PasswordBox passwordBox)
                        {
                            passwordBox.Password += key;
                        }
                        else
                        {
                            var textEvent = new TextCompositionEventArgs(
                                Keyboard.PrimaryDevice,
                                new TextComposition(InputManager.Current, inputElement, key))
                            {
                                RoutedEvent = UIElement.TextInputEvent
                            };
                            inputElement.RaiseEvent(textEvent);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error sending key: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RaiseKeyEvent(IInputElement element, Key key, bool isKeyDown)
        {
            var keyEvent = new KeyEventArgs(
                Keyboard.PrimaryDevice,
                GetPresentationSource(element),
                Environment.TickCount,
                key)
            {
                RoutedEvent = isKeyDown ? UIElement.KeyDownEvent : UIElement.KeyUpEvent
            };
            element.RaiseEvent(keyEvent);
        }

        private PresentationSource GetPresentationSource(IInputElement element)
        {
            if (element is Visual visual)
            {
                return PresentationSource.FromVisual(visual) ?? InputManager.Current.PrimaryKeyboardDevice.ActiveSource;
            }
            // Fallback for non-Visual IInputElements (e.g., ContentElement)
            return InputManager.Current.PrimaryKeyboardDevice.ActiveSource;
        }

        private void RestoreFocus(IInputElement originalFocus)
        {
            if (originalFocus != null && originalFocus != Keyboard.FocusedElement)
            {
                Keyboard.Focus(originalFocus);
            }
        }
    }
}
