using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMServices.Sletat.DataModel
{
    public enum QuotaAvailability
    {
        NoPlaces = 0,
        Available = 1,
        Request = 2,
        Undefined = -1
    }
}
