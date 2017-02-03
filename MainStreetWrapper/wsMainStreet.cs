using MainStreet.BusinessFlow.SDK;
using MainStreet.BusinessFlow.SDK.Web;
using MainStreet.BusinessFlow.SDK.Ws;
using MainStreetWrapper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainStreetWrapper
{
    public static class wsMainStreet
    {
        public static IEnumerable<ProductImageModel> QueryProductImages()
        {
            var itemSearch = GetActiveItemSearch();
            itemSearch.AddColumn("item_alt_images", AdditionalColumnType.Attribute);
            itemSearch.AddColumn("item_ColorNums", AdditionalColumnType.Attribute);

            var itemList = BusinessFlow.WebServices.Item.Search(itemSearch);
            var itemModels = new List<ProductImageModel>();

            foreach(dsItemList.ItemsRow item in itemList.Items)
            {
                var tag = item.Isitem_tagNull() ? string.Empty : item.item_tag;
                var altImages = item["item_alt_images"] == DBNull.Value ? string.Empty : item["item_alt_images"].ToString();
                var colorNums = item["item_ColorNums"] == DBNull.Value ? string.Empty : item["item_ColorNums"].ToString();
                itemModels.Add(new ProductImageModel(item.item_cd, tag, colorNums, altImages));
            }

            return itemModels;
        }

        public static List<CustomerModel> CustomerPointInTimeExport(DateTime since, string site, bool isProduction)
        {
            BusinessFlowContext.SetContext(site, isProduction);
            var customerList = new List<CustomerModel>();
            var customerSearch = new CustomerSearchRequest();
            customerSearch.AddCriterion("customer_closed", AdditionalColumnType.Database, "0");
            customerSearch.AddCriterion("customer_suspended", AdditionalColumnType.Database, "0");
            customerSearch.AddCriterion("lu_dt", AdditionalColumnType.Database, since.ToShortDateString(), AdditionalCriterionCondition.GreaterThan);
            customerSearch.MaxRows = 10000;

            var customers = BusinessFlow.WebServices.Customer.Search(customerSearch);

            foreach (dsCustomerList.CustomersRow customer in customers.Customers)
                customerList.Add(SafeCreate(customer));

            return customerList;
        }

        public static List<CustomerModel> CustomerPricePointExport(string priceLevelCode, string site, bool isProduction)
        {
            var customerList = new List<CustomerModel>();
            var customerSearch = new CustomerSearchRequest();
            var priceLevelGuid = GetPricePointGuid(priceLevelCode);
            customerSearch.AddCriterion("customer_closed", AdditionalColumnType.Database, "0");
            customerSearch.AddCriterion("customer_suspended", AdditionalColumnType.Database, "0");
            customerSearch.AddCriterion("item_price_level_guid", AdditionalColumnType.Database, priceLevelGuid.ToString());
            customerSearch.MaxRows = 10000;

            var customers = BusinessFlow.WebServices.Customer.Search(customerSearch);

            foreach (dsCustomerList.CustomersRow customer in customers.Customers)
                customerList.Add(SafeCreate(customer, priceLevelCode));

            return customerList;
        }

        private static Guid GetPricePointGuid(string priceLevelCode)
        {
            var priceLevels = BusinessFlow.WebServices.LookupTables[LookupTables.ItemPriceLevels];
            var priceLevelGuid = Guid.Empty;
            foreach (LookupRow lr in priceLevels.Rows)
            {
                if (lr.LookupCode != priceLevelCode)
                    continue;
                return (Guid)lr.LookupValue;
            }

            return Guid.Empty;
        }

        private static ItemSearchRequest GetActiveItemSearch()
        {
            var itemSearch = new ItemSearchRequest();
            itemSearch.MaxRows = 30000;
            itemSearch.ReturnChildItems = false;
            itemSearch.AddCriterion("item_available", AdditionalColumnType.Database, "1");
            itemSearch.AddCriterion("item_type_id", AdditionalColumnType.Database, "3");
            itemSearch.AddCriterion("item_closed", AdditionalColumnType.Database, "0");
            return itemSearch;
        }

        public static List<CustomerModel> CustomerListExport(string state, string site, bool isProduction)
        {
            BusinessFlowContext.SetContext(site, isProduction);
            var customerList = new List<CustomerModel>();
            var customerSearch = new CustomerSearchRequest();
            var stateCriterion = new AdditionalCriterion();

            stateCriterion.CriterionID = "customer_region_cd";
            stateCriterion.CriterionType = AdditionalCriterionType.Database;
            stateCriterion.Parameters = new string[] { state };

            customerSearch.MaxRows = 50000;
            customerSearch.Filter = CustomerSearchFilter.Active;
            customerSearch.AddCriterion(stateCriterion);
            customerSearch.AddCriterion("lu_dt", AdditionalColumnType.Database, DateTime.Today.AddYears(-3).ToShortDateString(), AdditionalCriterionCondition.GreaterThan);

            var customers = BusinessFlow.WebServices.Customer.Search(customerSearch);

            if (customers.Customers.Rows.Count == 50000)
                throw new NotImplementedException(state + " is just TOO BIG!!");

            foreach (dsCustomerList.CustomersRow customer in customers.Customers)
                customerList.Add(SafeCreate(customer));

            return customerList;
        }

        public static List<CustomerModel> CustomerListExport(IEnumerable<int> zipCodes, string site, bool isProduction)
        {
            BusinessFlowContext.SetContext(site, isProduction);
            var customerList = new List<CustomerModel>();
            var customerSearch = new CustomerSearchRequest();
            var stateCriterion = new AdditionalCriterion();

            stateCriterion.CriterionID = "customer_postal_code";
            stateCriterion.CriterionType = AdditionalCriterionType.Database;
            

            customerSearch.MaxRows = 20000;
            customerSearch.Filter = CustomerSearchFilter.Active;
            customerSearch.AddCriterion(stateCriterion);
            customerSearch.AddCriterion("lu_dt", AdditionalColumnType.Database, DateTime.Today.AddYears(-4).ToShortDateString(), AdditionalCriterionCondition.GreaterThan);

            for (var i = 0; i < zipCodes.Count(); i += 10)
            {
                stateCriterion.Parameters = zipCodes.Skip(i).Take(10).Select(z => z.ToString()).ToArray();
                var customers = BusinessFlow.WebServices.Customer.Search(customerSearch);

                foreach (dsCustomerList.CustomersRow customer in customers.Customers)
                    customerList.Add(SafeCreate(customer));
            }

            return customerList;
        }

        public static void FetchPasswordForCustomer(CustomerModel model)
        {
            try
            {
                var customerSync = BusinessFlow.WebServices.Customer.GetDetail(model.CustomerGuid);
                if (customerSync.CustomerRow != null)
                    model.Password = customerSync.CustomerRow.Iscustomer_passwordNull() ? string.Empty : customerSync.CustomerRow.customer_password;
            }
            catch { }
        }

        private static CustomerModel SafeCreate(dsCustomerList.CustomersRow row)
        {
            return new CustomerModel()
            {
                FirstName = row.Iscustomer_first_nameNull() ? string.Empty : row.customer_first_name,
                LastName = row.Iscustomer_last_nameNull() ? string.Empty : row.customer_last_name,
                Name = row.Iscustomer_nameNull() ? string.Empty : row.customer_name,
                Title = row.Iscustomer_titleNull() ? string.Empty : row.customer_title,
                Email = row.Iscustomer_emailNull() ? string.Empty : row.customer_email,
                Password = row.Iscustomer_passwordNull() ? string.Empty : row.customer_password,
                CompanyName = row.Iscustomer_company_nameNull() ? string.Empty : row.customer_company_name,
                Phone = row.Iscustomer_phoneNull() ? string.Empty : row.customer_phone,
                PhoneWork = row.Iscustomer_phone_workNull() ? string.Empty : row.customer_phone_work,
                Fax = row.Iscustomer_faxNull() ? string.Empty : row.customer_fax,
                CreationDate = row.Iscr_dtNull() ? DateTime.Now : row.cr_dt,
                LastLogin = row.Islu_dtNull() ? DateTime.Now : row.lu_dt,
                CustomerId = row.Iscustomer_cdNull() ? string.Empty : row.customer_cd,
                CustomerGuid = row.Iscustomer_guidNull() ? Guid.Empty : row.customer_guid,
                Attention = row.Iscustomer_attentionNull() ? string.Empty : row.customer_attention,
                Address = row.Iscustomer_addressNull() ? string.Empty : row.customer_address,
                Address2 = row.Iscustomer_address2Null() ? string.Empty : row.customer_address2,
                City = row.Iscustomer_cityNull() ? string.Empty : row.customer_city,
                StateAbbrev = row.Iscustomer_state_abbrevNull() ? string.Empty : row.customer_state_abbrev,
                CountryCode = row.Iscustomer_country_cdNull() ? string.Empty : row.customer_country_cd,
                PostalCode = row.Iscustomer_postal_codeNull() ? string.Empty : row.customer_postal_code,
                Shipping_Address = row.Iscustomer_saddressNull() ? string.Empty : row.customer_saddress,
                Shipping_Address2 = row.Iscustomer_saddress2Null() ? string.Empty : row.customer_saddress2,
                Shipping_Attention = row.Iscustomer_sattentionNull() ? string.Empty : row.customer_sattention,
                Shipping_City = row.Iscustomer_scityNull() ? string.Empty : row.customer_scity,
                Shipping_CompanyName = row.Iscustomer_scompany_nameNull() ? string.Empty : row.customer_scompany_name,
                Shipping_CountryCode = row.Iscustomer_scountry_cdNull() ? string.Empty : row.customer_scountry_cd,
                Shipping_Phone = row.Iscustomer_sphoneNull() ? string.Empty : row.customer_sphone,
                Shipping_PostalCode = row.Iscustomer_spostal_codeNull() ? string.Empty : row.customer_spostal_code,
                Shipping_StateAbbrev = row.Iscustomer_sstate_abbrevNull() ? string.Empty : row.customer_sstate_abbrev
            };
        }

        private static CustomerModel SafeCreate(dsCustomerList.CustomersRow row, string pricePointCode)
        {
            var model = SafeCreate(row);
            model.Price_Point = pricePointCode;

            return model;
        }
    }
}
