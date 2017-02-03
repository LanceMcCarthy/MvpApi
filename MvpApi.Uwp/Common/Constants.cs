using System;

namespace MvpApi.Uwp.Common
{
    public static class Constants
    {
        public const string Scope = "wl.emails%20wl.basic%20wl.offline_access%20wl.signin";
        

        public static Uri SignInUrl = new Uri(string.Format(@"https://login.live.com/oauth20_authorize.srf?client_id={0}&redirect_uri=https:%2F%2Flogin.live.com%2Foauth20_desktop.srf&response_type=code&scope={1}", ClientId, Scope));
        public static string RedirectUrl = "https://login.live.com/oauth20_desktop.srf";
        public static string AccessTokenUrl = string.Format(@"https://login.live.com/oauth20_token.srf?client_id={0}&client_secret={1}&redirect_uri=https://login.live.com/oauth20_desktop.srf&grant_type=authorization_code&code=", ClientId, ClientSecret);
        public static string RefreshTokenUrl = string.Format(@"https://login.live.com/oauth20_token.srf?client_id={0}&client_secret={1}&redirect_uri=https://login.live.com/oauth20_desktop.srf&grant_type=refresh_token&refresh_token=", ClientId, ClientSecret);
    }
}
