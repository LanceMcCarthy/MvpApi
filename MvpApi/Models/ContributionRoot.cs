using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvpApi.Models
{
    public class ContributionRoot
    {
        public List<Contribution> Contributions { get; set; }
        public int TotalContributions { get; set; }
        public int PagingIndex { get; set; }
    }
}
