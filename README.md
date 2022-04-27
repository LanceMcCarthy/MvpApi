## Important Notice

Microsoft is sunsetting the MVP API on **April 29th, 2022**. This means after that date, the MVP Companion app will no longer work and I will need to hide it from the Microsoft Store.

However, I will leave this repo as it is. It has been, and will continue to be, a developer community resource and a source of inspirpation. From API service classes to UI code, it has a plethora of different coding approaches that will be helpful for developers building their own applications and want to know how to do "X, Y, or Z" thing in their project.

### Future Options

Microsoft has stated that the web UI is the only way to submit your community contributions moving forward. Many MVPs have used MVP Companion to efficiently submit and manage their contributions, either through batch operations or the fast GUI capabilities of the app. There is the [SellMVP Powershell Module](https://github.com/ZanattaMichael/SelMVP) that doesn't rely on the API. It instead uses the Selenium web driver (which is normally used for UI tests) to interact with the website, and could be automated to handle your more arduous tasks.

# MVP Companion 

A client application to allow for faster contribution browsing, editing and upload to help renewing MVPs get up to date for renewal.

## Installation
- Windows
  - [Microsoft Store](https://www.microsoft.com/store/apps/9NRXNX3WLH77): Latest
  - [App Installer](https://dvlup.blob.core.windows.net/general-app-files/Installers/MvpCompanion/MvpApi.Uwp.appinstaller): Prerelease, code-signed.
- iOS (in progress)
- Android (in progress)
- Mac (pending demand)

## Pipelines

You can view [the Azure DevOps pipelines](https://dev.azure.com/lance/MVP%20Companion%20Ops/_build) directly or glance at the status tables below.

### Build Pipelines

| Platform | Dev | Master | Prerelease (Appinstaller) | Release (Microsoft Store) |
|----------|-----|--------|-----------------|------------------------|
| UWP | ![UWP Dev](https://dev.azure.com/lance/MVP%20Companion%20Ops/_apis/build/status/UWP%20%5BDev%5D) | ![UWP Master](https://dev.azure.com/lance/MVP%20Companion%20Ops/_apis/build/status/UWP%20%5BMaster%5D) | ![Release Appinstaller](https://dev.azure.com/lance/MVP%20Companion%20Ops/_apis/build/status/UWP%20%5BRelease%20Appinstaller%5D) | ![Release Microsoft Store](https://dev.azure.com/lance/MVP%20Companion%20Ops/_apis/build/status/UWP%20%5BRelease%5D?branchName=release)|

### Release Pipelines

| Pipeline | Stages |
|----------|--------|
| Microsoft Store | Beta ![beta](https://vsrm.dev.azure.com/lance/_apis/public/Release/badge/343301de-d63e-46b2-8816-7da7ade8002d/2/2) Publish ![Publish](https://vsrm.dev.azure.com/lance/_apis/public/Release/badge/343301de-d63e-46b2-8816-7da7ade8002d/2/6) |
| Google Play | Beta ![beta](https://vsrm.dev.azure.com/lance/_apis/public/Release/badge/343301de-d63e-46b2-8816-7da7ade8002d/3/4) Publish ![Publish](https://vsrm.dev.azure.com/lance/_apis/public/Release/badge/343301de-d63e-46b2-8816-7da7ade8002d/3/5) |

> If you'd like to understand the architecture of this CI-CD implementation, see my post on how I set this up [Free Yourself with DevOps](https://dvlup.com/2022/03/04/free-yourself-with-devops/).

## Features

The following feature provide you with powerful and fast management of your community contributions.

- New
  - Automatic Load on Demand, with maintained grouping, filering and sorting support
  - DataGrid Details Row instead of details page
  - Fast inline edit, clone, delete commands leveraging a new popup editor dialog
  - Excel import (experimental)
- Queue uploading for super-fast contribution submissions without having to wait for each save.
- View and delete MVP Profile OnlineIdentities
- Annual MVP Survey Questions support

### Revamped Home Page with Load On Demand DataGrid Experience
![new homepage](https://user-images.githubusercontent.com/3520532/153973369-b3a44f1d-024e-4243-a363-51054454cd09.png)

### Queued Uploads (to avoid waiting for every save)
![image](https://content.screencast.com/users/lance.mccarthy/folders/Snagit/media/054a5bfe-3d1f-4aec-b4df-1473d662e789/03.09.2018-18.36.GIF)

### Set Multiple Technology Areas
![image](https://dvlup.blob.core.windows.net/general-app-files/MVP%20Companion/MutipleTechAreas.gif)

### Manage Online Identities
![Online Identities](https://user-images.githubusercontent.com/3520532/50461434-5a614780-094c-11e9-856c-14fdfc1dd5ac.png)

### Automatic Login with Encrypted Token Handling
![image](https://dvlup.blob.core.windows.net/general-app-files/MVP%20Companion/MVP_Companion_1.7_update.gif)
