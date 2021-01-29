using System;

namespace MvpCompanion.UI.Common
{
    public class LoginChangedEventArgs : EventArgs
    {
        public LoginChangedEventArgs(bool isLoggedIn)
        {
            IsLoggedIn = isLoggedIn;
        }
        
        public bool IsLoggedIn { get; set; }
    }
}