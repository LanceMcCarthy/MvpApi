using System;
using Newtonsoft.Json;

namespace MvpApi.Common.Models
{
    // TODO remove shortly. This is the same object as ContributionTechnologyModel.
    //public class ContributionAreaModel : ObservableObject
    //{
    //    private Guid? id;
    //    private string awardCategory;
    //    private string awardName;
    //    private string name;
    //    private int? statuscode;
    //    private bool? active;

    //    public ContributionAreaModel() { }

    //    public ContributionAreaModel(Guid? id = default(Guid?), string name = default(string), string awardName = default(string), string awardCategory = default(string), int statusCode = default(int), bool active = default(bool))
    //    {
    //        Id = id;
    //        Name = name;
    //        AwardName = awardName;
    //        AwardCategory = awardCategory;
    //        Statuscode = statuscode;
    //        Active = active;
    //    }

    //    [JsonProperty(PropertyName = "Id")]
    //    public Guid? Id
    //    {
    //        get => id;
    //        set => SetProperty(ref id, value);
    //    }

    //    [JsonProperty(PropertyName = "Name")]
    //    public string Name
    //    {
    //        get => name;
    //        set => SetProperty(ref name, value);
    //    }

    //    [JsonProperty(PropertyName = "AwardName")]
    //    public string AwardName
    //    {
    //        get => awardName;
    //        set => SetProperty(ref awardName, value);
    //    }

    //    [JsonProperty(PropertyName = "AwardCategory")]
    //    public string AwardCategory
    //    {
    //        get => awardCategory;
    //        set => SetProperty(ref awardCategory, value);
    //    }

    //    [JsonProperty(PropertyName = "Statuscode")]
    //    public int? Statuscode
    //    {
    //        get => statuscode;
    //        set => SetProperty(ref statuscode, value);
    //    }

    //    [JsonProperty(PropertyName = "Active")]
    //    public bool? Active
    //    {
    //        get => active;
    //        set => SetProperty(ref active, value);
    //    }
    //}
}