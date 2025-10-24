
using Standard.Licensing;
using Standard.Licensing.Validation;
using System.Text;

namespace BrokenLinkChecker.Licensing
{
    public class Activator
    {
        public string email { get; set; }
        public string licXml { get; set; }
        public string purchaseId { get; set; }

        public Activator(string email, string licXml, string purchaseId)
        {
            this.email = email;
            this.licXml = licXml;
            this.purchaseId = purchaseId;
        }

        public bool isActivated()
        {
            bool activated = true;
            try
            {
                var licenseAct = License.Load(licXml);
                var fail = licenseAct.Validate().Signature(LicensingConstans.LIC_PUB_KEY).AssertValidLicense();
                var lsFails = fail.ToList();
                if (lsFails.Count() > 0)
                    activated = false;
                else
                {
                    if (licenseAct.AdditionalAttributes.Get("ProductName") != LicensingConstans.PROD_NAME)
                        activated = false;
                    if (licenseAct.AdditionalAttributes.Get("PurchaseId") != purchaseId)
                        activated = false;
                    if (licenseAct.Customer.Email != email)
                        activated = false;
                }
            }
            catch
            {
                activated = false;
            }

            return (activated);
        }
    }
}
