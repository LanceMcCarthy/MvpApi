using System;

namespace MvpApi.Services.Utilities
{
    public static class ServiceConstants
    {
        public static DateTime SubmissionStartDate
        {
            get
            {
                var today = DateTime.Today;

                // After we are running July-Dec, use the current year's June 1st date.
                if (today.Month > 5)
                {
                    return new DateTime(DateTime.Now.Year, 6, 1);
                }

                // - Experimental - //
                // If we are running during the submission lock period, return the next cycle's start date
                // This works on the website, I don't think the API will take it.
                if (today.Month == 4 || today.Month == 5)
                {
                    return new DateTime(DateTime.Now.Year, 6, 1);
                }

                // Jan, Feb, March = Use the previous year's July 1st date
                return new DateTime(DateTime.Now.Year - 1, 6, 1);
            }
        }

        public static DateTime SubmissionDeadline
        {
            get
            {
                var today = DateTime.Today;

                // From July to Dec, we return the the next calendar year's March 31st for the deadline
                if (today.Month > 5)
                {
                    return new DateTime(DateTime.Now.Year + 1, 3, 31);
                    
                }
                
                // - Experimental -
                // If we are running during the submission lock period, return the next calendar year's March 31st deadline
                if (today.Month == 4 || today.Month == 5)
                {
                    return new DateTime(DateTime.Now.Year + 1, 3, 31);
                }

                // Jan, Feb or March, we return the current calendar year's March 31st for the deadline
                return new DateTime(DateTime.Now.Year, 3, 31);
            }
        }
    }
}
