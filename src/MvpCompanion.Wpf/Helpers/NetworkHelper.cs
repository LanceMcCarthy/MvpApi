using System;

namespace MvpCompanion.Wpf.Helpers
{
    public class NetworkHelper
    {
        private static volatile NetworkHelper _current;
        private static readonly object SyncRoot = new object();

        public static NetworkHelper Current
        {
            get
            {
                if (_current != null)
                    return _current;

                lock (SyncRoot)
                {
                    _current ??= new NetworkHelper();
                }

                return _current;
            }
        }

        public bool CheckInternetConnection()
        {
            System.Net.WebRequest req = System.Net.WebRequest.Create("https://www.google.com/");
            try
            {
                var resp = req.GetResponse();
                resp.Close();
                req = null;
                return true;
            }
            catch (Exception ex)
            {
                req = null;
                return false;
            }
        }
    }
}
