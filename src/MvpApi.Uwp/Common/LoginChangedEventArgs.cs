using System;

namespace MvpApi.Uwp.Common
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