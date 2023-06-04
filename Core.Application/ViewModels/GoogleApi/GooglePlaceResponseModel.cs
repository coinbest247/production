using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.ViewModels.GoogleApi
{
    public class GooglePlaceResponseModel
    {
        public List<GoogleApiDataModel> candidates { get; set; } = new List<GoogleApiDataModel>();
    }
}
