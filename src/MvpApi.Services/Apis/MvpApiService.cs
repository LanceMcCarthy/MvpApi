using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CommonHelpers.Common;
using MvpApi.Common.CustomEventArgs;
using MvpApi.Common.Models;
using MvpApi.Services.Utilities;
using Newtonsoft.Json;

namespace MvpApi.Services.Apis
{
    public class MvpApiService : BindableBase, IDisposable
    {
        private readonly HttpClient client;
        private ContributionViewModel contributionsCachedResult;
        private IReadOnlyList<ContributionTypeModel> contributionTypesCachedResult;
        private IReadOnlyList<ContributionAreasRootItem> contributionAreasCachedResult;
        private IReadOnlyList<VisibilityViewModel> visibilitiesCachedResult;
        private IReadOnlyList<OnlineIdentityViewModel> onlineIdentitiesCachedResult;
        private ProfileViewModel mvp;
        private bool isLoggedIn;
        private string profileImagePath;

        /// <summary>
        /// Service that interacts with the MVP API
        /// </summary>
        /// <param name="authorizationHeaderContent">OAuth 2.0 AccessToken from Live SDK or MS Graph via Azure AD v2 endpoint
        /// IMPORTANT: 'Bearer' prefix needed before the token code</param>
        public MvpApiService(string authorizationHeaderContent)
        {
            if (string.IsNullOrEmpty(authorizationHeaderContent))
            {
                throw new ArgumentNullException(nameof(authorizationHeaderContent), "The authorization header (a.k.a. Bearer token) cannot be null or empty.");
            }

            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
                handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

            client = new HttpClient(handler);
            client.BaseAddress = new Uri("https://mvpapi.azure-api.net/mvp/api/");
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "3d199a7fb1c443e1985375f0572f58f8");
            client.DefaultRequestHeaders.Add("Authorization", authorizationHeaderContent);
        }

        #region Properties

        public ProfileViewModel Mvp
        {
            get => mvp;
            set => SetProperty(ref mvp, value);
        }

        public bool IsLoggedIn
        {
            get => isLoggedIn;
            set => SetProperty(ref isLoggedIn, value);
        }

        public string ProfileImagePath
        {
            get => profileImagePath;
            set
            {
                profileImagePath = value;

                // Always invoke PropertyChanged to ensure image is reloaded, even if the file path is the same.
                OnPropertyChanged();
            }
        }

        #endregion

        #region API Endpoints

        /// <summary>
        /// Returns the profile data of the currently signed in MVP
        /// </summary>
        /// <returns>The MVP's profile information</returns>
        public async Task<ProfileViewModel> GetProfileAsync()
        {
            try
            {
                using (var response = await client.GetAsync("profile"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<ProfileViewModel>(json);
                    }
                    else
                    {
                        await ProcessRefreshOrBadRequestAsync(response);
                    }
                }
            }
            catch (HttpRequestException e)
            {
                if (e.Message.Contains("500"))
                {
                    RequestErrorOccurred?.Invoke(this, new ApiServiceEventArgs { IsServerError = true });
                }

                Trace.WriteLine($"GetProfileAsync HttpRequestException: {e}");
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();

                Trace.WriteLine($"GetProfileAsync Exception: {e}");
            }

            return null;
        }

        /// <summary>
        /// Get the profile picture of the currently signed in MVP
        /// </summary>
        /// <returns>JPG image byte array</returns>
        public async Task<byte[]> GetProfileImageAsync()
        {
            try
            {
                // the result is Detected mime type: image/jpeg; charset=binary
                using (var response = await client.GetAsync("https://mvpapi.azure-api.net/mvp/api/profile/photo"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var base64String = await response.Content.ReadAsStringAsync();

                        try
                        {
                            if (string.IsNullOrEmpty(base64String))
                            {
                                return null;
                            }

                            base64String = base64String.TrimStart('"').TrimEnd('"');

                            return Convert.FromBase64String(base64String);
                        }
                        catch (Exception e)
                        {
                            //await e.LogExceptionAsync();
                            Trace.WriteLine($"GetProfileImageAsync Image Conversion Exception: {e}");
                        }
                    }
                    else
                    {
                        await ProcessRefreshOrBadRequestAsync(response);
                    }
                }
            }
            catch (HttpRequestException e)
            {
                if (e.Message.Contains("500"))
                {
                    RequestErrorOccurred?.Invoke(this, new ApiServiceEventArgs { IsServerError = true });
                }

                Trace.WriteLine($"GetProfileImageAsync HttpRequestException: {e}");
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();
                Trace.WriteLine($"GetProfileImageAsync Exception: {e}");
            }

            return null;
        }

        /// <summary>
        /// Downloads and saves the image file to LocalFolder
        /// </summary>
        /// <returns>File path</returns>
        public async Task<string> DownloadAndSaveProfileImage()
        {
            try
            {
                // the result is Detected mime type: image/jpeg; charset=binary
                using (var response = await client.GetAsync("profile/photo"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var base64String = await response.Content.ReadAsStringAsync();

                        try
                        {
                            if (string.IsNullOrEmpty(base64String))
                            {
                                return null;
                            }

                            base64String = base64String.TrimStart('"').TrimEnd('"');

                            // determine file type
                            var data = base64String.Substring(0, 5);

                            var fileExtension = string.Empty;

                            switch (data.ToUpper())
                            {
                                case "IVBOR":
                                    fileExtension = "png";
                                    break;
                                case "/9J/4":
                                    fileExtension = "jpg";
                                    break;
                            }

                            var imgBytes = Convert.FromBase64String(base64String);

                            var filePath = StorageHelpers.Instance.SaveImage(imgBytes, $"ProfilePicture.{fileExtension}");
                            return filePath;
                        }
                        catch (Exception e)
                        {
                            await e.LogExceptionAsync();
                            Trace.WriteLine($"DownloadAndSaveProfileImage Exception: {e}");
                        }
                    }
                    else
                    {
                        await ProcessRefreshOrBadRequestAsync(response);
                    }
                }
            }
            catch (HttpRequestException e)
            {
                if (e.Message.Contains("500"))
                {
                    RequestErrorOccurred?.Invoke(this, new ApiServiceEventArgs { IsServerError = true });
                }

                Trace.WriteLine($"GetProfileImageAsync HttpRequestException: {e}");
            }
            catch (Exception e)
            {
                //await e.LogExceptionAsync();
                Trace.WriteLine($"GetProfileImageAsync Exception: {e}");
            }

            return null;
        }

        /// <summary>
        /// Gets all the MVP's activities.
        /// </summary>
        /// <param name="forceRefresh">The result is cached in a backing list by default which prevents unnecessary fetches. If you want the cache refreshed, set this to true</param>
        /// <returns>A list of the MVP's contributions</returns>
        public async Task<ContributionViewModel> GetAllContributionsAsync(bool forceRefresh = false)
        {
            if (contributionsCachedResult != null && !forceRefresh)
            {
                // Return the cached result by default.
                return contributionsCachedResult;
            }
            
            try
            {
                int totalCount = 0;

                // The first fetch gets the total count, which we need to do the full fetch
                using (var response = await client.GetAsync($"contributions/0/0"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var deserializedResult = JsonConvert.DeserializeObject<ContributionViewModel>(json);

                        // Read the total
                        totalCount = Convert.ToInt32(deserializedResult.TotalContributions);
                    }
                    else
                    {
                        await ProcessRefreshOrBadRequestAsync(response);
                    }
                }

                // Using the total count, we can now fetch all the items and cache them
                return await GetContributionsAsync(0, totalCount, true);
            }
            catch (HttpRequestException e)
            {
                if (e.Message.Contains("500"))
                {
                    RequestErrorOccurred?.Invoke(this, new ApiServiceEventArgs { IsServerError = true });
                }

                Trace.WriteLine($"GetContributionsAsync HttpRequestException: {e}");
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();

                Trace.WriteLine($"GetContributionsAsync Exception: {e}");
            }

            return null;
        }

        /// <summary>
        /// Gets the MVPs activities, depending on the offset (page) and the limit (number of items per-page)
        /// </summary>
        /// <param name="offset">page to return</param>
        /// <param name="limit">number of items for the page</param>
        /// <param name="forceRefresh">The result is cached in a backing list by default which prevents unnecessary fetches. If you want the cache refreshed, set this to true</param>
        /// <returns>A list of the MVP's contributions</returns>
        public async Task<ContributionViewModel> GetContributionsAsync(int? offset, int limit, bool forceRefresh = false)
        {
            if (contributionsCachedResult != null && !forceRefresh)
            {
                // Return the cached result by default.
                return contributionsCachedResult;
            }
            
            if (offset == null)
            {
                offset = 0;
            }

            try
            {
                using (var response = await client.GetAsync($"contributions/{offset}/{limit}"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var deserializedResult = JsonConvert.DeserializeObject<ContributionViewModel>(json);

                        // Update the cached result.
                        contributionsCachedResult = deserializedResult;

                        return contributionsCachedResult;
                    }
                    else
                    {
                        await ProcessRefreshOrBadRequestAsync(response);
                    }
                }
            }
            catch (HttpRequestException e)
            {
                if (e.Message.Contains("500"))
                {
                    RequestErrorOccurred?.Invoke(this, new ApiServiceEventArgs { IsServerError = true });
                }

                Trace.WriteLine($"GetContributionsAsync HttpRequestException: {e}");
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();

                Trace.WriteLine($"GetContributionsAsync Exception: {e}");
            }

            return null;
        }

        /// <summary>
        /// Submits a new contribution to the currently sign-in MVP profile
        /// </summary>
        /// <param name="contribution">The contribution to be submitted</param>
        /// <returns>Contribution submitted. This object should now have a valid ID and be added to the app's Contributions collection</returns>
        public async Task<ContributionsModel> SubmitContributionAsync(ContributionsModel contribution)
        {
            if (contribution == null)
                throw new NullReferenceException("The contribution parameter was null.");

            try
            {
                var serializedContribution = JsonConvert.SerializeObject(contribution);
                byte[] byteData = Encoding.UTF8.GetBytes(serializedContribution);

                using (var content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    using (var response = await client.PostAsync("contributions?", content))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            return JsonConvert.DeserializeObject<ContributionsModel>(json);
                        }
                        else
                        {
                            await ProcessRefreshOrBadRequestAsync(response);
                        }
                    }
                }
            }
            catch (HttpRequestException e)
            {
                if (e.Message.Contains("500"))
                {
                    RequestErrorOccurred?.Invoke(this, new ApiServiceEventArgs { IsServerError = true });
                }

                Trace.WriteLine($"SubmitContributionAsync HttpRequestException: {e}");
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();
                Trace.WriteLine($"SubmitContributionAsync Exception: {e}");
            }

            return null;
        }

        /// <summary>
        /// Updates an existing contribution, identified by the contribution ID
        /// </summary>
        /// <param name="contribution">Contribution to be updated</param>
        /// <returns>Bool to denote update success or failure</returns>
        public async Task<bool?> UpdateContributionAsync(ContributionsModel contribution)
        {
            if (contribution == null)
                throw new NullReferenceException("The contribution parameter was null.");

            try
            {
                // Request body
                var serializedContribution = JsonConvert.SerializeObject(contribution);
                byte[] byteData = Encoding.UTF8.GetBytes(serializedContribution);

                using (var content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    using (var response = await client.PutAsync("contributions?", content))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            return true;
                        }
                        else
                        {
                            await ProcessRefreshOrBadRequestAsync(response);
                        }
                    }
                }
            }
            catch (HttpRequestException e)
            {
                if (e.Message.Contains("500"))
                {
                    RequestErrorOccurred?.Invoke(this, new ApiServiceEventArgs { IsServerError = true });
                }

                Trace.WriteLine($"UpdateContributionAsync HttpRequestException: {e}");
                return null;
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();

                Trace.WriteLine($"GetProfileAsync Exception: {e}");
            }

            return null;
        }

        /// <summary>
        /// Delete contribution
        /// </summary>
        /// <param name="contribution">Item to delete</param>
        /// <returns>Success or failure</returns>
        public async Task<bool?> DeleteContributionAsync(ContributionsModel contribution)
        {
            if (contribution == null)
                throw new NullReferenceException("The contribution parameter was null.");

            try
            {
                using (var response = await client.DeleteAsync($"contributions?id={contribution.ContributionId}"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    else
                    {
                        await ProcessRefreshOrBadRequestAsync(response);
                    }
                }
            }
            catch (HttpRequestException e)
            {
                if (e.Message.Contains("500"))
                {
                    RequestErrorOccurred?.Invoke(this, new ApiServiceEventArgs { IsServerError = true });
                }

                Trace.WriteLine($"UpdateContributionAsync HttpRequestException: {e}");
                return null;
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();

                Trace.WriteLine($"GetProfileAsync Exception: {e}");
                return null;
            }

            return null;
        }

        /// <summary>
        /// This gets a list if the different contributions types
        /// </summary>
        /// <param name="forceRefresh">The result is cached in a backing list by default which prevents unnecessary fetches. If you want the cache refreshed, set this to true</param>
        /// <returns>List of contributions types</returns>
        public async Task<IReadOnlyList<ContributionTypeModel>> GetContributionTypesAsync(bool forceRefresh = false)
        {
            if (contributionTypesCachedResult?.Count == 0 && !forceRefresh)
            {
                // Return the cached result by default.
                return contributionTypesCachedResult;
            }

            try
            {
                using (var response = await client.GetAsync("contributions/contributiontypes"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var deserializedResult = JsonConvert.DeserializeObject<IReadOnlyList<ContributionTypeModel>>(json);

                        // Update the cached result.
                        contributionTypesCachedResult = new List<ContributionTypeModel>(deserializedResult);
                        
                        return contributionTypesCachedResult;
                    }
                    else
                    {
                        await ProcessRefreshOrBadRequestAsync(response);
                    }
                }
            }
            catch (HttpRequestException e)
            {
                if (e.Message.Contains("500"))
                {
                    RequestErrorOccurred?.Invoke(this, new ApiServiceEventArgs { IsServerError = true });
                }

                Trace.WriteLine($"GetContributionTypesAsync HttpRequestException: {e}");
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();

                Trace.WriteLine($"GetContributionTypesAsync Exception: {e}");
            }

            return null;
        }

        /// <summary>
        /// Gets a list of the Contribution Technologies (aka Contribution Areas)
        /// </summary>
        /// <param name="forceRefresh">The result is cached in a backing list by default which prevents unnecessary fetches. If you want the cache refreshed, set this to true</param>
        /// <returns>A list of available contribution areas</returns>
        public async Task<IReadOnlyList<ContributionAreasRootItem>> GetContributionAreasAsync(bool forceRefresh = false)
        {
            if (contributionAreasCachedResult?.Count == 0 && !forceRefresh)
            {
                // Return the cached result by default.
                return contributionAreasCachedResult;
            }

            try
            {
                using (var response = await client.GetAsync("contributions/contributionareas"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var deserializedResult = JsonConvert.DeserializeObject<IReadOnlyList<ContributionAreasRootItem>>(json);

                        // Update the cached result.
                        contributionAreasCachedResult = new List<ContributionAreasRootItem>(deserializedResult);

                        return contributionAreasCachedResult;
                    }
                    else
                    {
                        await ProcessRefreshOrBadRequestAsync(response);
                    }
                }
            }
            catch (HttpRequestException e)
            {
                if (e.Message.Contains("500"))
                {
                    RequestErrorOccurred?.Invoke(this, new ApiServiceEventArgs { IsServerError = true });
                }

                Trace.WriteLine($"GetContributionTechnologiesAsync HttpRequestException: {e}");
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();

                Trace.WriteLine($"GetContributionTechnologiesAsync Exception: {e}");
            }

            return null;
        }

        /// <summary>
        /// Gets a list of contribution visibility options (aka Sharing Preferences). The traditional results are "Microsoft Only", "MVP Community" and "Everyone"
        /// </summary>
        /// <param name="forceRefresh">The result is cached in a backing list by default which prevents unnecessary fetches. If you want the cache refreshed, set this to true</param>
        /// <returns>A list of available visibilities</returns>
        public async Task<IReadOnlyList<VisibilityViewModel>> GetVisibilitiesAsync(bool forceRefresh = false)
        {
            if (visibilitiesCachedResult?.Count == 0 && !forceRefresh)
            {
                // Return the cached result by default.
                return visibilitiesCachedResult;
            }

            try
            {
                using (var response = await client.GetAsync("contributions/sharingpreferences"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();

                        var deserializedResult = JsonConvert.DeserializeObject<IReadOnlyList<VisibilityViewModel>>(json);

                        // Update the cached result.
                        visibilitiesCachedResult = new List<VisibilityViewModel>(deserializedResult);

                        return visibilitiesCachedResult;
                    }
                    else
                    {
                        await ProcessRefreshOrBadRequestAsync(response);
                    }
                }
            }
            catch (HttpRequestException e)
            {
                if (e.Message.Contains("500"))
                {
                    RequestErrorOccurred?.Invoke(this, new ApiServiceEventArgs { IsServerError = true });
                }

                Trace.WriteLine($"GetVisibilitiesAsync HttpRequestException: {e}");
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();

                Trace.WriteLine($"GetVisibilitiesAsync Exception: {e}");
            }

            return null;
        }

        /// <summary>
        /// Returns a list of the MVP's OnlineIdentities (social media accounts and other identities)
        /// </summary>
        /// <param name="forceRefresh">The result is cached in a backing list by default which prevents unnecessary fetches. If you want the cache refreshed, set this to true</param>
        /// <returns></returns>
        public async Task<IReadOnlyList<OnlineIdentityViewModel>> GetOnlineIdentitiesAsync(bool forceRefresh = false)
        {
            if (contributionTypesCachedResult?.Count == 0 && !forceRefresh)
            {
                // Return the cached result by default.
                return onlineIdentitiesCachedResult;
            }

            try
            {
                using (var response = await client.GetAsync("onlineidentities"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var deserializedResult = JsonConvert.DeserializeObject<IReadOnlyList<OnlineIdentityViewModel>>(json);

                        // Update the cached result.
                        onlineIdentitiesCachedResult = new List<OnlineIdentityViewModel>(deserializedResult);

                        return onlineIdentitiesCachedResult;
                    }
                    else
                    {
                        await ProcessRefreshOrBadRequestAsync(response);
                    }
                }
            }
            catch (HttpRequestException e)
            {
                if (e.Message.Contains("500"))
                {
                    RequestErrorOccurred?.Invoke(this, new ApiServiceEventArgs { IsServerError = true });
                }

                Trace.WriteLine($"GetOnlineIdentitiesAsync HttpRequestException: {e}");
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();

                Trace.WriteLine($"GetOnlineIdentitiesAsync Exception: {e}");
            }

            return null;
        }
        
        /// <summary>
        /// Saves an OnlineIdentity
        /// </summary>
        /// <param name="onlineIdentity"></param>
        /// <returns></returns>
        public async Task<OnlineIdentity> SubmitOnlineIdentityAsync(OnlineIdentityViewModel onlineIdentity)
        {
            if (onlineIdentity == null)
                throw new NullReferenceException("The OnlineIdentity parameter was null.");

            try
            {
                var serializedOnlineIdentity = JsonConvert.SerializeObject(onlineIdentity);
                byte[] byteData = Encoding.UTF8.GetBytes(serializedOnlineIdentity);

                using (var content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    using (var response = await client.PostAsync("onlineidentities?", content))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            Trace.WriteLine($"OnlineIdentity Save JSON: {json}");

                            var result = JsonConvert.DeserializeObject<OnlineIdentity>(json);
                            Trace.WriteLine($"OnlineIdentity Save Result: ID {result.PrivateSiteId}");

                            return result;
                        }
                        else
                        {
                            await ProcessRefreshOrBadRequestAsync(response);
                        }
                    }
                }
            }
            catch (HttpRequestException e)
            {
                if (e.Message.Contains("500"))
                {
                    RequestErrorOccurred?.Invoke(this, new ApiServiceEventArgs { IsServerError = true });
                }

                Trace.WriteLine($"SubmitOnlineIdentitiesAsync HttpRequestException: {e}");
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();

                Trace.WriteLine($"SubmitOnlineIdentitiesAsync Exception: {e}");
            }

            return null;
        }

        public async Task<bool> DeleteOnlineIdentityAsync(OnlineIdentityViewModel onlineIdentity)
        {
            if (onlineIdentity == null)
                throw new NullReferenceException("The OnlineIdentity parameter was null.");

            try
            {
                using (var response = await client.DeleteAsync($"onlineidentities?id={onlineIdentity.PrivateSiteId}"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    else
                    {
                        await ProcessRefreshOrBadRequestAsync(response);
                    }
                }
            }
            catch (HttpRequestException e)
            {
                if (e.Message.Contains("500"))
                {
                    RequestErrorOccurred?.Invoke(this, new ApiServiceEventArgs { IsServerError = true });
                }

                Trace.WriteLine($"SubmitOnlineIdentitiesAsync HttpRequestException: {e}");
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();

                Trace.WriteLine($"SubmitOnlineIdentitiesAsync Exception: {e}");
            }

            return false;
        }
        
        /// <summary>
        /// Gets the current Award Consideration Questions list.
        /// </summary>
        /// <returns>The list of questions to be answered for consideration in the next award period.</returns>
        public async Task<IReadOnlyList<AwardConsiderationQuestionModel>> GetAwardConsiderationQuestionsAsync()
        {
            try
            {
                using (var response = await client.GetAsync("awardconsideration/getcurrentquestions"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<IReadOnlyList<AwardConsiderationQuestionModel>>(json);
                    }
                    else
                    {
                        await ProcessRefreshOrBadRequestAsync(response);
                    }
                }
            }
            catch (HttpRequestException e)
            {
                if (e.Message.Contains("500"))
                {
                    RequestErrorOccurred?.Invoke(this, new ApiServiceEventArgs { IsServerError = true });
                }

                Trace.WriteLine($"GetOnlineIdentitiesAsync HttpRequestException: {e}");
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();
                Trace.WriteLine($"GetOnlineIdentitiesAsync Exception: {e}");
            }

            return null;
        }
        
        /// <summary>
        /// Gets the MVP's currently saved answers for the Award consideration questions
        /// </summary>
        /// <returns>The list of questions to be answered for consideration in the next award period.</returns>
        public async Task<IReadOnlyList<AwardConsiderationAnswerModel>> GetAwardConsiderationAnswersAsync()
        {
            try
            {
                using (var response = await client.GetAsync("awardconsideration/GetAnswers"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<IReadOnlyList<AwardConsiderationAnswerModel>>(json);
                    }
                    else
                    {
                        await ProcessRefreshOrBadRequestAsync(response);
                    }
                }
            }
            catch (HttpRequestException e)
            {
                if (e.Message.Contains("500"))
                {
                    RequestErrorOccurred?.Invoke(this, new ApiServiceEventArgs { IsServerError = true });
                }

                Trace.WriteLine($"GetOnlineIdentitiesAsync HttpRequestException: {e}");
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();
                Trace.WriteLine($"GetOnlineIdentitiesAsync Exception: {e}");
            }

            return null;
        }

        /// <summary>
        /// Saves the MVP's answers to the Aware Consideration questions.
        /// IMPORTANT NOTE:
        /// This does NOT submit them for review by the MVP award team, it is intended to be used to save the answers.
        /// To submit the questions, call SubmitAwardConsiderationAnswerAsync after saving the answers.
        /// </summary>
        /// <param name="answers"></param>
        /// <returns></returns>
        public async Task<List<AwardConsiderationAnswerModel>> SaveAwardConsiderationAnswerAsync(IEnumerable<AwardConsiderationAnswerModel> answers)
        {
            if (answers == null)
                throw new NullReferenceException("The contribution parameter was null.");

            try
            {
                var serializedContribution = JsonConvert.SerializeObject(answers);
                byte[] byteData = Encoding.UTF8.GetBytes(serializedContribution);

                using (var content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    using (var response = await client.PostAsync("awardconsideration/saveanswers?", content))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            return JsonConvert.DeserializeObject<List<AwardConsiderationAnswerModel>>(json);
                        }
                        else
                        {
                            await ProcessRefreshOrBadRequestAsync(response);
                        }
                    }
                }
            }
            catch (HttpRequestException e)
            {
                if (e.Message.Contains("500"))
                {
                    RequestErrorOccurred?.Invoke(this, new ApiServiceEventArgs { IsServerError = true });
                }

                Trace.WriteLine($"SubmitContributionAsync HttpRequestException: {e}");
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();
                Trace.WriteLine($"SubmitContributionAsync Exception: {e}");
            }

            return null;
        }
        
        /// <summary>
        /// Submits the MVP's answers for award consideration questions.
        /// WARNING - THIS CAN ONLY BE DONE ONCE PER AWARD PERIOD, THE ANSWERS CANNOT BE CHANGED AFTER SUBMISSION.
        /// </summary>
        /// <returns>If the submission was successful</returns>
        public async Task<bool> SubmitAwardConsiderationAnswerAsync()
        {
            try
            {
                var serializedContribution = JsonConvert.SerializeObject(string.Empty);
                byte[] byteData = Encoding.UTF8.GetBytes(serializedContribution);

                using (var content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    using (var response = await client.PostAsync("awardconsideration/SubmitAnswers?", content))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            return true;
                        }
                        else
                        {
                            await ProcessRefreshOrBadRequestAsync(response);
                        }
                    }
                }
            }
            catch (HttpRequestException e)
            {
                if (e.Message.Contains("500"))
                {
                    RequestErrorOccurred?.Invoke(this, new ApiServiceEventArgs { IsServerError = true, Message = e.Message});
                }

                Trace.WriteLine($"SubmitContributionAsync HttpRequestException: {e}");
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();

                Trace.WriteLine($"SubmitContributionAsync Exception: {e}");
            }

            return false;
        }

        #endregion

        #region Utilities

        public async Task<string> ExportContributionsAsync()
        {
            string json = string.Empty;

            try
            {
                using (var response = await client.GetAsync($"contributions/0/0"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        json = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        await ProcessRefreshOrBadRequestAsync(response);
                    }
                }
            }
            catch (HttpRequestException e)
            {
                if (e.Message.Contains("500"))
                {
                    RequestErrorOccurred?.Invoke(this, new ApiServiceEventArgs { IsServerError = true });
                }

                Trace.WriteLine($"ExportContributionsAsync HttpRequestException: {e}");
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();

                Trace.WriteLine($"ExportContributionsAsync Exception: {e}");
            }

            return json;
        }

        public async Task<string> ExportOnlineIdentitiesAsync()
        {
            string json = string.Empty;

            try
            {
                using (var response = await client.GetAsync("onlineidentities"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        json = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        await ProcessRefreshOrBadRequestAsync(response);
                    }
                }
            }
            catch (HttpRequestException e)
            {
                if (e.Message.Contains("500"))
                {
                    RequestErrorOccurred?.Invoke(this, new ApiServiceEventArgs { IsServerError = true });
                }

                Trace.WriteLine($"GetOnlineIdentitiesAsync HttpRequestException: {e}");
            }
            catch (Exception e)
            {
                await e.LogExceptionAsync();

                Trace.WriteLine($"GetOnlineIdentitiesAsync Exception: {e}");
            }

            return json;
        }

        private async Task ProcessRefreshOrBadRequestAsync(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                AccessTokenExpired?.Invoke(this, new ApiServiceEventArgs { IsTokenRefreshNeeded = true });
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await response.Content.ReadAsStringAsync();

                RequestErrorOccurred?.Invoke(this, new ApiServiceEventArgs { IsBadRequest = true, Message = message });
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// This event will fire when there is a 401 or 403 returned from an API call. This indicates that a new Access Token is needed.
        /// Use this event to use the refresh token to get a new access token automatically.
        /// </summary>
        public event EventHandler<ApiServiceEventArgs> AccessTokenExpired;

        /// <summary>
        /// This event fires when the API call results in a HttpStatusCode 500 result is obtained.
        /// </summary>
        public event EventHandler<ApiServiceEventArgs> RequestErrorOccurred;

        #endregion

        public void Dispose()
        {
            client?.Dispose();
        }
    }
}