using MainStreet.BusinessFlow.SDK;
using MainStreet.BusinessFlow.SDK.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainStreetWrapper.Models
{
    static class BusinessFlowContext
    {
        public static void SetContext(string site, bool isProduction)
        {
            BusinessFlow.Context = GetContext(site, isProduction);
        }

        private static IBusinessFlowContext GetContext(string site, bool isProduction)
        {
            var credentials = CreateBFCreds(site, isProduction);
            return isProduction ? ProdContext(credentials) : StageContext(credentials);
        }

        private static IBusinessFlowContext ProdContext(Credentials credentials)
        {
            return new BusinessFlowWebContext(credentials) { ServerBaseUrl = "https://businessflow.mainstreetcommerce.com/3.9.7" };
        }

        private static IBusinessFlowContext StageContext(Credentials credentials)
        {
            return new BusinessFlowWebContext(credentials) { ServerBaseUrl = "https://stagebusinessflow.mainstreetcommerce.com/3.9.7" };
        }

        private static Credentials CreateBFCreds(string site, bool isProduction)
        {
            if (isProduction)
                return (site == "marmot") ? new Credentials("marmot.com", "website", "3UdRuw5E") :
                    new Credentials("exofficio.com", "website", "bfdr74");

            return new Credentials((site == "marmot" ? "stage.marmot.com" : "stage.exofficio.com"), "website", "uiyaiqoa");
        }
    }
}
