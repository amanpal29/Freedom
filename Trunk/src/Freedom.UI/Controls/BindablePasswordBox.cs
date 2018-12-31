using Freedom.Cryptography;
using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Freedom.UI.Controls
{    
    public sealed class BindablePasswordBox : Decorator
    {
        private bool _inHandlePasswordChanged;

        public static readonly DependencyProperty PasswordHashProperty =
            DependencyProperty.Register("PasswordHash", typeof(string), typeof(BindablePasswordBox),
                                        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPasswordHashChanged));

        public static readonly DependencyProperty HashAlgorithmProperty = DependencyProperty.Register(
            "HashAlgorithm", typeof(HashAlgorithm), typeof(BindablePasswordBox), new PropertyMetadata(default(HashAlgorithm)));

        private static readonly DependencyPropertyKey PasswordLengthPropertyKey =
            DependencyProperty.RegisterReadOnly("PasswordLength", typeof(int), typeof(BindablePasswordBox),
                                                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty PasswordLengthProperty =
            PasswordLengthPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey PasswordComplexityPropertyKey =
            DependencyProperty.RegisterReadOnly("PasswordComplexity", typeof(int), typeof(BindablePasswordBox),
                                                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty PasswordComplexityProperty =
            PasswordComplexityPropertyKey.DependencyProperty;

        public BindablePasswordBox()
        {
            PasswordBox passwordBox = new PasswordBox();
            passwordBox.MaxLength = 127;
            passwordBox.PasswordChanged += HandlePasswordChanged;
            Child = passwordBox;
        }

        public string PasswordHash
        {
            get { return GetValue(PasswordHashProperty) as string; }
            set { SetValue(PasswordHashProperty, value); }
        }

        public HashAlgorithm HashAlgorithm
        {
            get { return (HashAlgorithm)GetValue(HashAlgorithmProperty); }
            set { SetValue(HashAlgorithmProperty, value); }
        }

        public int PasswordLength
        {
            get { return (int)GetValue(PasswordLengthProperty); }
            private set { SetValue(PasswordLengthPropertyKey, value); }
        }

        public int PasswordComplexity
        {
            get { return (int)GetValue(PasswordComplexityProperty); }
            private set { SetValue(PasswordComplexityPropertyKey, value); }
        }

        public event EventHandler PasswordChanged;

        private void OnPasswordChanged()
        {
            PasswordChanged?.Invoke(this, EventArgs.Empty);
        }

        private static void OnPasswordHashChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BindablePasswordBox self = (BindablePasswordBox)d;

            if (self._inHandlePasswordChanged) return;

            PasswordBox passwordBox = (PasswordBox)self.Child;

            if (self.HashAlgorithm == null)
            {
                passwordBox.Password = (string)e.NewValue;
            }
            else
            {
                if (!string.IsNullOrEmpty((string)e.NewValue))
                    throw new InvalidOperationException("Password hashing is not reversible.");

                passwordBox.Password = null;
            }
        }

        private void HandlePasswordChanged(object sender, RoutedEventArgs eventArgs)
        {
            try
            {
                _inHandlePasswordChanged = true;

                PasswordBox passwordBox = (PasswordBox)sender;

                if (HashAlgorithm == null)
                {
                    PasswordHash = passwordBox.Password;
                    PasswordLength = passwordBox.Password.Length;
                    PasswordComplexity = PasswordUtility.GetPasswordComplexity(passwordBox.Password);
                }
                else
                {
                    // This code manages the plain-text password as an array instead of a string, which
                    // in turn gives us the ability to nul-out the array when we're finished with it,
                    // thereby reducing the amount of time the plain-text password is in managed memory.
                    // When used with a HashAlgorithm that takes similar precautions, it becomes less
                    // likely that plain text password will be found in a memory dump.
                    //
                    // It should not even remotly be considered secure, but this class shouldn't be used
                    // in the first place, in situations where keeping the password out of managed memory
                    // is important. - DGG 2013-02-22, 2016-03-09

                    char[] passwordChars;

                    // Get the password as a char array

                    using (SecureString password = passwordBox.SecurePassword)
                    {
                        passwordChars = new char[password.Length];

                        IntPtr passwordPtr = Marshal.SecureStringToBSTR(password);

                        try
                        {
                            Marshal.Copy(passwordPtr, passwordChars, 0, passwordChars.Length);
                        }
                        finally
                        {
                            Marshal.ZeroFreeBSTR(passwordPtr);
                        }
                    }

                    // Compute its Hash, Length, and Complexity

                    if (passwordChars.Length == 0)
                    {
                        PasswordHash = null;
                        PasswordLength = 0;
                        PasswordComplexity = 0;
                    }
                    else
                    {
                        PasswordLength = passwordChars.Length;
                        PasswordComplexity = PasswordUtility.GetPasswordComplexity(passwordChars);

                        byte[] passwordBytes = Encoding.UTF8.GetBytes(passwordChars);

                        for (int i = 0; i < passwordChars.Length; i++)
                            passwordChars[i] = '\0';

                        PasswordHash = Convert.ToBase64String(HashAlgorithm.ComputeHash(passwordBytes));

                        // Nul out the array.
                        for (int i = 0; i < passwordBytes.Length; i++)
                            passwordBytes[i] = 0;
                    }
                }

                OnPasswordChanged();
            }
            finally
            {
                _inHandlePasswordChanged = false;
            }
        }
    }
}
