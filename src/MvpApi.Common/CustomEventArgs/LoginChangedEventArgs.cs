using System;

namespace MvpApi.Common.CustomEventArgs
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