using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using MvpApi.Common.Models;
using MvpApi.Uwp.Helpers;
using Newtonsoft.Json;
using MvpApi.Uwp.ViewModels;

namespace MvpApi.Uwp.Services
{
    public class MvpApiService : IDisposable
    {
        private readonly HttpClient client;

        /// <summary>
        /// Service that interacts with the MVP API
        /// </summary>
        /// <param name="apiKey">the Ocp-Apim-Subscription-Key you got from your MVP API portal</param>
        /// <param name="authorizationHeader">Authorization header. Example: "Bearer AccessTokenGoesHere"</param>
        public MvpApiService(string apiKey, string authorizationHeader)
        {
            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
                handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

            client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);
            client.DefaultRequestHeaders.Add("Authorization", authorizationHeader);
        }

        /// <summary>
        /// Returns the profile data of the currently signed in MVP
        /// </summary>
        /// <returns>The MVP's profile information</returns>
        public async Task<ProfileViewModel> GetProfileAsync()
        {
            try
            {
                using (var response = await client.GetAsync("https://mvpapi.azure-api.net/mvp/api/profile"))
                {
                    if(response.StatusCode == HttpStatusCode.NotFound)
                    {
                        await LoginPageViewModel.RequestAuthorizationAsync(LoginPageViewModel.AccessTokenUrl, StorageHelpers.LoadToken("refresh_token"), true);
                        return null;
                    }

                    var json = await response.Content.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<ProfileViewModel>(json);
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();

                if (e.Message.Contains("401"))
                {
                    //TODO Try refresh token, access token is only valid for 60 minutes
                }

                if (e.Message.Contains("500"))
                {

                }

                Debug.WriteLine($"GetProfileAsync HttpRequestException: {e}");
                return null;
            }
            catch (Exception e)
            {
                await e.LogExceptionWithUserMessage(
                    "Sorry, there was a problem retrieving your profile. If you'd like to send a technical summary to the app development team, click Yes.",
                    "Get Profile Error");

                Debug.WriteLine($"GetProfileAsync Exception: {e}");
                return null;
            }
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
                    var base64String = await response.Content.ReadAsStringAsync();

                    try
                    {
                        if (string.IsNullOrEmpty(base64String))
                        {
                            return null;
                        }

                        base64String = base64String.TrimStart('"').TrimEnd('"');

                        var imgBytes = Convert.FromBase64String(base64String);
                        
                        Debug.WriteLine($"Image Decoded: {imgBytes?.Length} bytes");

                        return imgBytes;
                    }
                    catch(Exception ex)
                    {
                        var md = new MessageDialog("You can email the error and copy base64 into your clipboard.\r\n\nThis would let you test the image data directly using a base64 converter (base64converter.com) or paste it into the email", "Image Decoding Failed");

                        md.Commands.Add(new UICommand("send error only"));
                        md.Commands.Add(new UICommand("send error and copy base64"));
                        md.Commands.Add(new UICommand("cancel"));

                        var result = await md.ShowAsync();

                        if (result.Label == "send error only")
                        {
                            await FeedbackHelpers.Current.EmailErrorMessageAsync(ex.Message);
                        }
                        else if (result.Label == "send error and copy base64")
                        {
                            FeedbackHelpers.Current.CopyToClipboard(base64String);
                            await FeedbackHelpers.Current.EmailErrorMessageAsync(ex.Message);
                        }

                        return null;
                    }
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();

                if (e.Message.Contains("401"))
                {
                    //TODO Try refresh token, access token is only valid for 60 minutes
                }

                if (e.Message.Contains("500"))
                {

                }

                Debug.WriteLine($"GetProfileImageAsync HttpRequestException: {e}");
                return null;
            }
            catch (Exception e)
            {
                await e.LogExceptionWithUserMessage(
                    "Sorry, there was a problem retrieving your profile image. If you'd like to send a technical summary to the app development team, click Yes.",
                    "Get Profile Image Error");
                
                Debug.WriteLine($"GetProfileImageAsync Exception: {e}");
                return null;
            }
        }

        /// <summary>
        /// Downloads and saves the image file to LocalFolder
        /// </summary>
        /// <param name="folder">StorageFolder location</param>
        /// <param name="fileNameWithoutExtension">filename for image, default is</param>
        /// <returns>File path</returns>
        public async Task<string> DownloadAndSaveProfileImage(StorageFolder folder, string fileNameWithoutExtension = "ProfilePicture")
        {
            try
            {
                // the result is Detected mime type: image/jpeg; charset=binary
                using (var response = await client.GetAsync("https://mvpapi.azure-api.net/mvp/api/profile/photo"))
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

                        var imageFile = await folder.CreateFileAsync($"{fileNameWithoutExtension}.{fileExtension}", CreationCollisionOption.ReplaceExisting);

                        using (var ms = new MemoryStream(imgBytes, 0, imgBytes.Length))
                        using (var fileStream = await imageFile.OpenStreamForWriteAsync())
                        {
                            ms.CopyTo(fileStream);
                        }

                        Debug.WriteLine($"Image Decoded: {imgBytes?.Length} bytes");

                        return imageFile.Path;
                    }
                    catch (Exception ex)
                    {
                        var md = new MessageDialog("You can email the error and copy base64 into your clipboard.\r\n\nThis would let you test the image data directly using a base64 converter (base64converter.com) or paste it into the email", "Image Decoding Failed");

                        md.Commands.Add(new UICommand("send error only"));
                        md.Commands.Add(new UICommand("send error and copy base64"));
                        md.Commands.Add(new UICommand("cancel"));
                        
                        var result = await md.ShowAsync();

                        if (result.Label == "send error only")
                        {
                            await FeedbackHelpers.Current.EmailErrorMessageAsync(ex.Message);
                        }
                        else if (result.Label == "send error and copy base64")
                        {
                            FeedbackHelpers.Current.CopyToClipboard(base64String);
                            await FeedbackHelpers.Current.EmailErrorMessageAsync(ex.Message);
                        }

                        return null;
                    }
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();

                if (e.Message.Contains("401"))
                {
                    //TODO Try refresh token, access token is only valid for 60 minutes
                }

                if (e.Message.Contains("500"))
                {

                }

                Debug.WriteLine($"GetProfileImageAsync HttpRequestException: {e}");
                return null;
            }
            catch (Exception e)
            {
                await e.LogExceptionWithUserMessage(
                    "Sorry, there was a problem retrieving your profile image. If you'd like to send a technical summary to the app development team, click Yes.",
                    "Get Profile Image Error");

                Debug.WriteLine($"GetProfileImageAsync Exception: {e}");
                return null;
            }
        }

        /// <summary>
        /// Gets the MVPs activities, depending on the offset (page) and the limit (number of items per-page)
        /// </summary>
        /// <param name="offset">page to return</param>
        /// <param name="limit">number of items for the page</param>
        /// <returns></returns>
        public async Task<ContributionViewModel> GetContributionsAsync(int? offset, int limit)
        {
            if (offset == null)
                offset = 0;

            try
            {
                using (var response = await client.GetAsync($"https://mvpapi.azure-api.net/mvp/api/contributions/{offset}/{limit}"))
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<ContributionViewModel>(json);
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();

                if (e.Message.Contains("401"))
                {
                    //TODO Try refresh token, access token is only valid for 60 minutes
                }

                if (e.Message.Contains("500"))
                {

                }

                Debug.WriteLine($"GetContributionsAsync HttpRequestException: {e}");
                return null;
            }
            catch (Exception e)
            {
                await e.LogExceptionWithUserMessage(
                    "Sorry, there was a problem retrieving your contributions. If you'd like to send a technical summary to the app development team, click Yes.",
                    "Get Contributions Error");

                Debug.WriteLine($"GetContributionsAsync Exception: {e}");
                return null;
            }
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
                    
                    using (var response = await client.PostAsync("https://mvpapi.azure-api.net/mvp/api/contributions?", content))
                    {
                        var json = await response.Content.ReadAsStringAsync();

                        Debug.WriteLine($"Submission Save JSON: {json}");

                        var result = JsonConvert.DeserializeObject<ContributionsModel>(json);

                        Debug.WriteLine($"Submission Save Result: ID {result.ContributionId}");

                        return result;
                    }
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();

                if (e.Message.Contains("401"))
                {
                    //TODO Try refresh token, access token is only valid for 60 minutes
                }

                if (e.Message.Contains("500"))
                {
                    
                }

                Debug.WriteLine($"SubmitContributionAsync HttpRequestException: {e}");
                return null;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Error converting value"))
                {
                    await e.LogExceptionWithUserMessage(
                        $"There was an invalid value for one of the submissions fields and the API rejected it. See the error message below for more details, make the adjustment and save it again:\r\n\n" +
                        $"{e.Message}",
                        "Submit Contribution Error");
                }
                else
                {
                    await e.LogExceptionWithUserMessage(
                        $"Sorry, there was an unexpected problem uploading the contribution '{contribution.Title}'. If you'd like to send a technical summary to the app development team, click Yes.",
                        "Submit Contribution Error");
                }
                
                Debug.WriteLine($"SubmitContributionAsync Exception: {e}");

                return null;
            }
        }

        /// <summary>
        /// Updates an existing contribution, identified by the contribution ID
        /// </summary>
        /// <param name="contribution">Contribution to be updated</param>
        /// <returns>Bool to denote update success or failure</returns>
        public async Task<bool?> UpdateContributionAsync(ContributionsModel contribution)
        {
            if(contribution == null)
                throw new NullReferenceException("The contribution parameter was null.");

            try
            {
                // Request body
                var serializedContribution = JsonConvert.SerializeObject(contribution);
                byte[] byteData = Encoding.UTF8.GetBytes(serializedContribution);

                using (var content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    using (var response = await client.PutAsync("https://mvpapi.azure-api.net/mvp/api/contributions", content))
                    {
                        return response.IsSuccessStatusCode;
                    }
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();

                if (e.Message.Contains("401"))
                {
                    //TODO Try refresh token, access token is only valid for 60 minutes
                }

                if (e.Message.Contains("500"))
                {
                    
                }

                Debug.WriteLine($"UpdateContributionAsync HttpRequestException: {e}");
                return null;
            }
            catch (Exception e)
            {
                await e.LogExceptionWithUserMessage(
                    $"Sorry, there was a problem updating the contribution '{contribution.Title}'. If you'd like to send a technical summary to the app development team, click Yes.",
                    "Update Contribution Error");

                Debug.WriteLine($"GetProfileAsync Exception: {e}");
                return null;
            }
        }

        /// <summary>
        /// Deleted s contribution
        /// </summary>
        /// <param name="contribution">Item to delete</param>
        /// <returns>Success or failure</returns>
        public async Task<bool?> DeleteContributionAsync(ContributionsModel contribution)
        {
            if (contribution == null)
                throw new NullReferenceException("The contribution parameter was null.");

            try
            {
                using (var response = await client.DeleteAsync($"https://mvpapi.azure-api.net/mvp/api/contributions?id={contribution.ContributionId}"))
                {
                    return response.IsSuccessStatusCode;
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();

                if (e.Message.Contains("401"))
                {
                    //TODO Try refresh token, access token is only valid for 60 minutes
                }

                if (e.Message.Contains("500"))
                {

                }

                Debug.WriteLine($"UpdateContributionAsync HttpRequestException: {e}");
                return null;
            }
            catch (Exception e)
            {
                await e.LogExceptionWithUserMessage(
                    $"Sorry, there was a problem deleting the contribution '{contribution.Title}'. If you'd like to send a technical summary to the app development team, click Yes.",
                    "Delete Contribution Error");

                Debug.WriteLine($"GetProfileAsync Exception: {e}");
                return null;
            }
        }

        /// <summary>
        /// This gets a list if the different contributions types
        /// </summary>
        /// <returns>List of contributions types</returns>
        public async Task<IReadOnlyList<ContributionTypeModel>> GetContributionTypesAsync()
        {
            try
            {
                using (var response = await client.GetAsync("https://mvpapi.azure-api.net/mvp/api/contributions/contributiontypes"))
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<IReadOnlyList<ContributionTypeModel>>(json);
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();

                if (e.Message.Contains("401"))
                {
                    //TODO Try refresh token, access token is only valid for 60 minutes
                }

                if (e.Message.Contains("500"))
                {

                }

                Debug.WriteLine($"GetContributionTypesAsync HttpRequestException: {e}");
                return null;
            }
            catch (Exception e)
            {
                await e.LogExceptionWithUserMessage(
                    "Sorry, there was a problem getting contributions types list. If you'd like to send a technical summary to the app development team, click Yes.",
                    "Get Contribution Types Error");

                Debug.WriteLine($"GetContributionTypesAsync Exception: {e}");
                return null;
            }
        }

        /// <summary>
        /// Gets a list of the Contibution Technologies (aka Contribution Areas)
        /// </summary>
        /// <returns>A list of available contribution areas</returns>
        public async Task<IReadOnlyList<ContributionAreasRootItem>> GetContributionAreasAsync()
        {
            try
            {
                using (var response = await client.GetAsync("https://mvpapi.azure-api.net/mvp/api/contributions/contributionareas"))
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<IReadOnlyList<ContributionAreasRootItem>>(json);
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();

                if (e.Message.Contains("401"))
                {
                    //TODO Try refresh token, access token is only valid for 60 minutes
                }

                if (e.Message.Contains("500"))
                {

                }

                Debug.WriteLine($"GetContributionTechnologiesAsync HttpRequestException: {e}");
                return null;
            }
            catch (Exception e)
            {
                await e.LogExceptionWithUserMessage(
                    "Sorry, there was a problem getting contributions areas list. If you'd like to send a technical summary to the app development team, click Yes.",
                    "Get Contribution Areas Error");

                Debug.WriteLine($"GetContributionTechnologiesAsync Exception: {e}");
                return null;
            }
        }

        /// <summary>
        /// Gets a list of contribution visibility options (aka Sharing Preferences). The traditional results are "Microsoft Only", "MVP Community" and "Everyone"
        /// </summary>
        /// <returns>A list of available visibilities</returns>
        public async Task<IReadOnlyList<VisibilityViewModel>> GetVisibilitiesAsync()
        {
            try
            {
                using (var response = await client.GetAsync("https://mvpapi.azure-api.net/mvp/api/contributions/sharingpreferences"))
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<IReadOnlyList<VisibilityViewModel>>(json);
                }
            }
            catch (HttpRequestException e)
            {
                await e.LogExceptionAsync();

                if (e.Message.Contains("401"))
                {
                    //TODO Try refresh token, access token is only valid for 60 minutes
                }

                if (e.Message.Contains("500"))
                {

                }

                Debug.WriteLine($"GetVisibilitiesAsync HttpRequestException: {e}");
                return null;
            }
            catch (Exception e)
            {
                await e.LogExceptionWithUserMessage(
                    "Sorry, there was a problem getting contribution visibilities list. If you'd like to send a technical summary to the app development team, click Yes.",
                    "Get Visibilities Error");

                Debug.WriteLine($"GetVisibilitiesAsync Exception: {e}");
                return null;
            }
        }
        
        public void Dispose()
        {
            client?.Dispose();
        }
    }
}