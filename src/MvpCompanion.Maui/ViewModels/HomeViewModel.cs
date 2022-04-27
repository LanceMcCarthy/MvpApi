using CommonHelpers.Common;
using MvpApi.Common.Interfaces;
using MvpApi.Common.Models;
using MvpApi.Services.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Telerik.XamarinForms.Common;
using Telerik.XamarinForms.DataControls.ListView;
using Telerik.XamarinForms.DataControls.ListView.Commands;

namespace MvpCompanion.Maui.ViewModels;

public class HomeViewModel : ViewModelBase
{
    private ObservableCollection<ContributionsModel> _contributions;
    private ObservableCollection<OnlineIdentityViewModel> _onlineIdentities;
    private ContributionsModel _selectedContribution;
    private int _selectedSegmentIndex;
    private bool _isDrawerOpen;
    private string _status;
    private bool _isInEditMode;

    public HomeViewModel()
    {
        //GoToViewCommand = new Command(LoadView);
        ToggleDrawerCommand = new Command(() => IsDrawerOpen = !IsDrawerOpen);
        ToggleEditModeCommand = new Command(() => IsInEditMode = !IsInEditMode);

        // TODO Restore Telerik reference
        ItemTapCommand = new Command<ItemTapCommandContext>(ItemTapped);
    }
    
    public ObservableCollection<ContributionsModel> Contributions
    {
        get => _contributions ??= new ObservableCollection<ContributionsModel>();
        set => SetProperty(ref _contributions, value);
    }

    public ObservableCollection<OnlineIdentityViewModel> OnlineIdentities
    {
        get => _onlineIdentities ??= new ObservableCollection<OnlineIdentityViewModel>();
        set => SetProperty(ref _onlineIdentities, value);
    }

    public ContributionsModel SelectedContribution
    {
        get => _selectedContribution;
        set
        {
            if (SetProperty(ref _selectedContribution, value))
            {
                //LoadView(_selectedContribution == null ? ViewType.Home : ViewType.Detail);
            }
        }
    }

    public int SelectedSegmentIndex
    {
        get => _selectedSegmentIndex;
        set
        {
            SetProperty(ref _selectedSegmentIndex, value);
            SetGrouping(value);
        }
    }

    public bool IsDrawerOpen
    {
        get => _isDrawerOpen;
        set => SetProperty(ref _isDrawerOpen, value);
    }

    public bool IsInEditMode
    {
        get => _isInEditMode;
        set => SetProperty(ref _isInEditMode, value);
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public List<string> GroupingOptions { get; } = new() { "None", "Visibility", "Social" };

    public ObservableCollection<GroupDescriptorBase> GroupDescriptors { get; set; }

    public Command<ItemTapCommandContext> ItemTapCommand { get; set; }

    public Command GoToViewCommand { get; set; }

    public Command ToggleDrawerCommand { get; set; }

    public Command ToggleEditModeCommand { get; set; }

    public INavigationHandler NavigationHandler { private get; set; }


    //public async void LoadView(object viewType)
    //{
    //    // Pre-navigation work
    //    if ((ViewType)viewType == ViewType.Home)
    //    {
    //    }
    //    else if ((ViewType)viewType == ViewType.Upload)
    //    {
    //        SelectedContribution = new ContributionsModel();
    //    }

    //    // Invoke View change
    //    NavigationHandler.LoadView((ViewType)viewType);

    //    // Post-navigation work
    //    if ((ViewType)viewType == ViewType.Home)
    //    {
    //        if (!IsBusy)
    //        {
    //            IsBusy = true;
    //        }

    //        IsBusyMessage = "refreshing contributions...";

    //        // TODO temporary, replace with incremental loading collection
    //        //await RefreshContributionsAsync();
    //    }
    //    else if ((ViewType)viewType == ViewType.Profile)
    //    {
    //        if (!IsBusy)
    //        {
    //            IsBusy = true;
    //        }

    //        IsBusyMessage = "loading Online Identities...";

    //        //await RefreshOnlineIdentitiesAsync();
    //    }

    //    //Close drawer if it open
    //    if (IsDrawerOpen)
    //    {
    //        IsDrawerOpen = false;
    //    }

    //    if (IsBusy)
    //    {
    //        IsBusyMessage = "";
    //        IsBusy = false;
    //    }
    //}

    public async Task RefreshContributionsAsync()
    {
        if (App.ApiService == null || !App.ApiService.IsLoggedIn)
            return;

        var contributionsResult = await App.ApiService.GetContributionsAsync(0, 30);

        if (contributionsResult != null & contributionsResult?.Contributions.Count > 0)
        {
            if (Contributions.Count > 1)
                Contributions.Clear();

            foreach (var contribution in contributionsResult.Contributions)
            {
                Contributions.Add(contribution);
            }
        }
    }

    public async Task RefreshOnlineIdentitiesAsync()
    {
        var identities = await App.ApiService.GetOnlineIdentitiesAsync();

        if (identities != null & identities?.Count > 0)
        {
            foreach (var onlineIdentity in identities)
            {
                OnlineIdentities.Add(onlineIdentity);
            }
        }
    }

    private void SetGrouping(int groupOptionIndex)
    {
        try
        {
            if (GroupDescriptors == null)
            {
                return;
            }

            GroupDescriptors.Clear();

            var propertyToGroupBy = GroupingOptions[groupOptionIndex];

            if (string.IsNullOrEmpty(propertyToGroupBy))
            {
                return;
            }

            switch (propertyToGroupBy)
            {
                case "Visibility":
                    GroupDescriptors.Add(new DelegateGroupDescriptor
                    {
                        KeyExtractor = (arg) => (arg as OnlineIdentityViewModel)?.OnlineIdentityVisibility?.Description,
                        SortOrder = SortOrder.Descending
                    });
                    break;
                case "Social":
                    GroupDescriptors.Add(new DelegateGroupDescriptor
                    {
                        KeyExtractor = (arg) => (arg as OnlineIdentityViewModel)?.SocialNetwork.Name,
                        SortOrder = SortOrder.Descending
                    });
                    break;
                default:
                    GroupDescriptors.Clear();
                    break;
            }
        }
        catch (Exception ex)
        {
            ex.LogException();
        }
    }

    private void ItemTapped(ItemTapCommandContext context)
    {
        if (context.Item is OnlineIdentityViewModel item)
        {
            // TODO Show popup or modal page for editing/deleting identities
            Debug.WriteLine($"{item.SocialNetwork.Name} Tapped");
        }
    }
}
