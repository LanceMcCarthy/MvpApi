using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvpApi.Models
{
    public class Certification
    {
        public int PrivateSiteId { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public CertificationVisibility CertificationVisibility { get; set; }
    }
}
