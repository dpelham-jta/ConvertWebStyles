using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainStreetWrapper.Models
{
    public class CustomerModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string CompanyName { get; set; }
        public string Phone { get; set; }
        public string PhoneWork { get; set; }
        public string Fax { get; set; }

        public DateTime CreationDate { get; set; }
        public DateTime LastLogin { get; set; }
        public Guid CustomerGuid { get; set; }
        public string CustomerId { get; set; }

        public string Attention { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string StateAbbrev { get; set; }
        public string CountryCode { get; set; }
        public string PostalCode { get; set; }
        public string Shipping_Attention { get; set; }
        public string Shipping_Address { get; set; }
        public string Shipping_Address2 { get; set; }
        public string Shipping_CompanyName { get; set; }
        public string Shipping_City { get; set; }
        public string Shipping_StateAbbrev { get; set; }
        public string Shipping_CountryCode { get; set; }
        public string Shipping_PostalCode { get; set; }
        public string Shipping_Phone { get; set; }
        public string Price_Point { get; set; }


        public string AttentionFirstName
        {
            get
            {
                if (string.IsNullOrEmpty(Attention))
                    return string.Empty;
                var namePieces = Attention.Trim().Split(' ');
                if (namePieces.Length < 4)
                    return namePieces[0];

                return string.Join(" ", namePieces.Take(namePieces.Length - 2));
            }
        }

        public string AttentionSecondName
        {
            get
            {
                if (string.IsNullOrEmpty(Attention))
                    return string.Empty;
                var namePieces = Attention.Trim().Split(' ');
                if (namePieces.Length > 2)
                    return namePieces[namePieces.Length - 2];

                return string.Empty;
            }
        }

        public string AttentionLastName
        {
            get
            {
                if (string.IsNullOrEmpty(Attention))
                    return string.Empty;
                var namePieces = Attention.Trim().Split(' ');
                if (namePieces.Length > 1)
                    return namePieces.Last();

                return string.Empty;
            }
        }

        public string ShippingAttentionFirstName
        {
            get
            {
                if (string.IsNullOrEmpty(Shipping_Attention))
                    return string.Empty;
                var namePieces = Shipping_Attention.Trim().Split(' ');
                if (namePieces.Length < 4)
                    return namePieces[0];

                return string.Join(" ", namePieces.Take(namePieces.Length - 2));
            }
        }

        public string ShippingAttentionSecondName
        {
            get
            {
                if (string.IsNullOrEmpty(Shipping_Attention))
                    return string.Empty;
                var namePieces = Shipping_Attention.Trim().Split(' ');
                if (namePieces.Length > 2)
                    return namePieces[namePieces.Length - 2];

                return string.Empty;
            }
        }

        public string ShippingAttentionLastName
        {
            get
            {
                if (string.IsNullOrEmpty(Shipping_Attention))
                    return string.Empty;
                var namePieces = Shipping_Attention.Trim().Split(' ');
                if (namePieces.Length > 1)
                    return namePieces.Last();

                return string.Empty;
            }
        }

        private string GetPriceLevel
        {
            get
            {
                switch(this.Price_Point)
                {
                    case "Pro":
                        return "m_pro";
                    case "Pro Deal":
                        return "e_proDeal";
                    case "Super Pro":
                        return "m_superPro";
                    case "Pro Deal Plus":
                        return "e_proDealPlus";
                    case "Pro Legacy":
                        return "m_proLegacy";
                    case "VIP Industry Pro":
                        return "m_vipIndustryPro";
                    case "VIP Legacy":
                        return "m_vipLegacy";
                    case "VIP Executive Team":
                        return "m_vipExecutive";
                    case "Employee":
                        return "employee";
                    default:
                        return "retail";
                }
            }
        }

        public override string ToString()
        {
            return $@"
    <customer customer-no=""{CustomerId}""> 
        <credentials>
            <login>{Email}</login>
            <password encrypted=""false"">{Password}</password>
            <enabled-flag>true</enabled-flag>
            <password-question />
            <password-answer />
        </credentials>
        <profile>
            <salutation/>
            <title>{Title}</title>
            <first-name>{FirstName}</first-name>
            <second-name/>
            <last-name>{LastName}</last-name>
            <suffix/>
            <company-name>{CompanyName}</company-name>
            <job-title/>
            <email>{Email}</email>
            <phone-home>{Phone}</phone-home>
            <phone-business>{PhoneWork}</phone-business>
            <phone-mobile/>
            <fax>{Fax}</fax>
            <gender></gender>
            <creation-date>{CreationDate.ToUniversalTime().ToString("yyy-MM-ddTHH:mm:ssZ")}</creation-date>
            <last-login-time>{LastLogin.ToUniversalTime().ToString("yyy-MM-ddTHH:mm:ssZ")}</last-login-time>
            <last-visit-time>{LastLogin.ToUniversalTime().ToString("yyy-MM-ddTHH:mm:ssZ")}</last-visit-time>
            <preferred-locale/>
            <custom-attributes>
                <custom-attribute attribute-id=""priceLevel"">{GetPriceLevel}</custom-attribute>
            </custom-attributes>
        </profile>
        <addresses>
            <address address-id=""Billing"" preferred=""true"">
                <salutation/>
                <title/>
                <first-name>{AttentionFirstName}</first-name>
                <second-name>{AttentionSecondName}</second-name>
                <last-name>{AttentionLastName}</last-name>
                <suffix/>
                <company-name>{CompanyName}</company-name>
                <job-title/>
                <address1>{Address}</address1>
                <address2>{Address2}</address2>
                <suite/>
                <postbox/>
                <city>{City}</city>
                <postal-code>{PostalCode}</postal-code>
                <state-code>{StateAbbrev}</state-code>
                <country-code>US</country-code>
                <phone>{Phone}</phone>
            </address>
            <address address-id=""Shipping"" preferred=""false"">
                <salutation/>
                <title/>
                <first-name>{ShippingAttentionFirstName}</first-name>
                <second-name>{ShippingAttentionSecondName}</second-name>
                <last-name>{ShippingAttentionLastName}</last-name>
                <suffix/>
                <company-name>{Shipping_CompanyName}</company-name>
                <job-title/>
                <address1>{Shipping_Address}</address1>
                <address2>{Shipping_Address2}</address2>
                <suite/>
                <postbox/>
                <city>{Shipping_City}</city>
                <postal-code>{Shipping_PostalCode}</postal-code>
                <state-code>{Shipping_StateAbbrev}</state-code>
                <country-code>US</country-code>
                <phone>{Shipping_Phone}</phone>
            </address>
        </addresses>
        <note/>
    </customer>";
        }
    }
}
