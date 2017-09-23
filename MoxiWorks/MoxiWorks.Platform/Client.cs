﻿using System;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Web.UI;
using Newtonsoft.Json;
namespace MoxiWorks.Platform
{
    public static class Client
    {
        private static HttpClient _context;
        public static HttpClient Context => _context ?? (_context = GetContext());


        private static CookieContainer _cookies;
        public static CookieContainer Cookies => _cookies ?? (_cookies = new CookieContainer());

        public static void ResetClient()
        {
            _context = GetContext();
        }

        private static string Identifier { get; } = "5d39ba58-bfc3-11e5-a4e3-d0e1408e8026";

        private static string Secret { get; } = "a56sthhidTlUsLyp8eFZBQtt";

        private static HttpClientHandler _handler;
        
        private static HttpClient GetContext()
        {
            _handler = new HttpClientHandler
            {
                CookieContainer = Cookies,
                UseCookies = true
            };
            return new HttpClient(_handler);
        }

        public static int GetTimeStamp(DateTime value)
        {
           return (int)Math.Truncate((value.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
        }

        public static HttpClient RequestClient()
        {
            var client = Context;  
            var cred = new Credentials(Identifier, Secret);
            var auth = AuthenticationHeaderValue.Parse("Basic " + cred.ToBase64());
            client.DefaultRequestHeaders.Authorization = auth;
            client.DefaultRequestHeaders.Add("Accept", "application/vnd.moxi-platform+json;version=1");

            if (!Session.Instance.IsSessionCookieSet) return client;
            
            var cookie = Session.Instance.SessionCookie; 
            client.DefaultRequestHeaders.Add("Cookie", $"{cookie.Name}={cookie.Value}; path=/; HttpOnly");

            return client; 
        }
        
        public static T GetRequest<T>(string url)
        {
            var client = RequestClient(); 
            var result = client.GetAsync(new Uri(url)).Result;
            var content = result.Content;

            if (!Session.Instance.IsSessionCookieSet)
            {
                Session.Instance.SessionCookie = _handler.CookieContainer.GetCookies(new Uri("https://api-qa.moxiworks.com"))["_wms_svc_public_session"];
            }

            var s = content.ReadAsStringAsync().Result;
            Console.Write(s);
//            Console.Write(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/BuyerTransactions.json");
           
//           // Console.Write(Environment.GetFolderPath(Environment.SpecialFolder.MyComputer));
//
//            //Environment.GetFolderPath(Environment.SpecialFolder.Personal)
//            var str  = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/BuyerTransactions.json");
//            
//            
//            //Console.Write(Environment.SpecialFolder.Personal);
            return JsonConvert.DeserializeObject<T>(s,new JsonSerializerSettings{DefaultValueHandling = DefaultValueHandling.Ignore});  
        }

        public static T PostRequest<T>(string url, T obj)
        {
            var client = RequestClient();
            var result = client.PostAsJsonAsync(url,obj).Result;
            var content = result.Content;
            
            
            if (!Session.Instance.IsSessionCookieSet)
            {
                Session.Instance.SessionCookie = _handler.CookieContainer.GetCookies(new Uri("https://api-qa.moxiworks.com"))["_wms_svc_public_session"];
            }

            return JsonConvert.DeserializeObject<T>(content.ReadAsStringAsync().Result);  
                        
        }
        
        public static T PutRequest<T>(string url, T obj)
        {
            var client = RequestClient();
            var result = client.PutAsJsonAsync(url,obj).Result;
            var content = result.Content;
            
            
            if (!Session.Instance.IsSessionCookieSet)
            {
                Session.Instance.SessionCookie = _handler.CookieContainer.GetCookies(new Uri("https://api-qa.moxiworks.com"))["_wms_svc_public_session"];
            }

            return JsonConvert.DeserializeObject<T>(content.ReadAsStringAsync().Result);  
                        
        }
        
        public static ListingResults GetListingsUpdateSince(string companyId,DateTime updateSince,string lastMoxiWorksListingId = null,int perpage = 10 )
        {
            var timestamp = GetTimeStamp(updateSince);
            var builder = new UriBuilder("https://api-qa.moxiworks.com/api/listings");
            builder.AddQueryParameter("moxi_works_company_id",companyId);
            builder.AddQueryParameter("updated_since", timestamp.ToString());
            builder.AddQueryParameter("per_page",perpage.ToString());
            builder.AddQueryParameter("last_moxi_works_listing_id",lastMoxiWorksListingId);

            return GetRequest<ListingResults>(builder.getUrl()); 
        }

        public static Listing GetListing(string companyId, string moxiWorksListingId)
        {
            var builder = new UriBuilder($"https://api-qa.moxiworks.com/api/listings/{moxiWorksListingId}");
            builder.AddQueryParameter("moxi_works_company_id",companyId);
            return GetRequest<Listing>(builder.getUrl());

        }

        public static Agent GetAgent(string moxiWorksAgentId,  string companyId)
        {
            var builder = new UriBuilder($"https://api-qa.moxiworks.com/api/agent/{moxiWorksAgentId}");
            builder.AddQueryParameter("moxi_works_company_id",companyId);
            return GetRequest<Agent>(builder.getUrl());
        }

        public static AgentResults GetAgentsUpdatedSince(string moxiWorksCompanyId, DateTime updatedSince ,int? totalPages = null, int pageNumber = 1 )
        {
            var builder = new UriBuilder("https://api-qa.moxiworks.com/api/agents");
            builder.AddQueryParameter("moxi_works_company_id",moxiWorksCompanyId);

            var timestamp = GetTimeStamp(updatedSince);
            builder.AddQueryParameter("updated_since",timestamp);
            builder.AddQueryParameter("total_pages",totalPages);
            builder.AddQueryParameter("page_number",pageNumber);
            
            return GetRequest<AgentResults>(builder.getUrl());
        }

        public static Company GetCompany(string moxiWorksCompanyId)
        {
            var builder = new UriBuilder($"https://api-qa.moxiworks.com/api/companies/{moxiWorksCompanyId}");
            return GetRequest<Company>(builder.getUrl());
        }

        public static Contact GetContact(string agentId,AgentIdType agentIdType, string partnerContactId)
        {
            var builder = new UriBuilder($"https://api-qa.moxiworks.com/api/contacts/{partnerContactId}");
            builder.AddQueryParameter(agentIdType == AgentIdType.AgentUuid ? "agent_uuid" : "moxi_works_agent_id",
                agentId);
  
            return GetRequest<Contact>(builder.getUrl());
       }

        public static Contact CreateContact(Contact contact)
        {
            var builder = new UriBuilder($"https://api-qa.moxiworks.com/api/contacts");
        
            return PostRequest(builder.getUrl(),contact);
        }
        
        public static Contact UpdateContact(Contact contact)
        {
            var builder = new UriBuilder($"https://api-qa.moxiworks.com/api/contacts/{contact.PartnerContactId}");
            return PutRequest(builder.getUrl(), contact);

        }
        
        public static ContactResults GetContactResultsAgentUuid(string AgentId, string emailAddress = null, 
            string contactName = null, string phoneNumber = null, DateTime? updatedSince = null, int pageNumber = 1)
        {
            return GetContactsUpdatedSince(AgentId,AgentIdType.AgentUuid, emailAddress, contactName, phoneNumber, updatedSince, pageNumber);
        }

        public static ContactResults GetContactResultsMoxiWorksagentId(string AgentId, string emailAddress = null,
            string contactName = null, string phoneNumber = null, DateTime? updatedSince = null, int pageNumber = 1)
        {
            return GetContactsUpdatedSince(AgentId, AgentIdType.MoxiWorksagentId, emailAddress, contactName,
                phoneNumber, updatedSince, pageNumber);
        }
        
        public static ContactResults GetContactsUpdatedSince(string AgentId, AgentIdType agentIdType, string emailAddress = null, 
            string contactName = null, string phoneNumber = null, DateTime? updatedSince = null, int pageNumber = 1)
        {

            var builder = new UriBuilder("https://api-qa.moxiworks.com/api/contacts/");

            builder.AddQueryParameter(agentIdType == AgentIdType.AgentUuid ? "agent_uuid" : "moxi_works_agent_id",
                AgentId);
            

            if (updatedSince != null)
            {
                builder.AddQueryParameter("updated_since", GetTimeStamp(updatedSince.Value).ToString());
            }
            
            builder.AddQueryParameter("email_address", emailAddress);
            builder.AddQueryParameter("contact_name",contactName); 
            builder.AddQueryParameter("phone_number",phoneNumber);
            builder.AddQueryParameter("page_number", pageNumber);
            return GetRequest<ContactResults>(builder.getUrl());
        }

        public static BuyerTransaction CreateBuyerTransaction( BuyerTransaction buyerTransaction)
        {
            var builder = new UriBuilder("https://api-qa.moxiworks.com/api/buyer_transactions/");
            return PostRequest(builder.getUrl(), buyerTransaction); 
        }


        public static BuyerTransaction UpdateBuyerTransaction(BuyerTransaction buyerTransaction)
        {
            var builder = new UriBuilder($"https://api-qa.moxiworks.com/api/buyer_transactions/{buyerTransaction.MoxiWorksTransactionId}");
            return PutRequest(builder.getUrl(), buyerTransaction);
        }

        public static BuyerTransaction GetBuyerTransaction(string agentId, AgentIdType agentIdType, string moxiworksTransactionId)
        {
            var builder = new UriBuilder($"https://api-qa.moxiworks.com/api/buyer_transactions/{moxiworksTransactionId}");
            builder.AddQueryParameter(agentIdType == AgentIdType.AgentUuid ? "agent_uuid" : "moxi_works_agent_id",
                agentId);
           
            return GetRequest<BuyerTransaction>(builder.getUrl());
        }


        public static BuyerTransactionResults GetBuyerTransactions(string agentId, AgentIdType agentIdType,string moxiworksContactId = null, string partnerContactId = null, int pageNumber = 1 )
        {
            var builder = new UriBuilder("https://api-qa.moxiworks.com/api/buyer_transactions/");
            
            builder.AddQueryParameter(agentIdType == AgentIdType.AgentUuid ? "agent_uuid" : "moxi_works_agent_id",
                agentId);
            
            builder.AddQueryParameter("partner_contact_id", partnerContactId);
            builder.AddQueryParameter("moxi_works_contact_id", moxiworksContactId);
            builder.AddQueryParameter("page_number",pageNumber);
            
            return GetRequest<BuyerTransactionResults>(builder.getUrl());
        }
    }
    
    /*
    agent_uuid=12345678-1234-1234-1234-1234567890ab&partner_contact_id=abc982cdf345&contact_name=Billy+Football&gender=m&primary_email_address=johnny%40football.foo&secondary_email_address=fooball%40johnnyshead.lost&primary_phone_number=123123213&secondary_phone_number=2929292922&home_street_address=1234+Winterfell+Way&home_city=Cityville&home_state=Stateland&home_zip=12345&home_country=Westeros&job_title=Junior+Bacon+Burner&occupation=Chef&property_url=http%3A%2F%2Fteh.property.is%2Fhere&property_mls_id=123abc&property_street_address=1234+here+ave.&property_city=anywhereville&property_state=anystate&property_zip=918928&property_beds=21&property_baths=12.75&property_list_price=1231213&property_listing_status=Active&property_photo_url=http%3A%2F%2Fthis.is%2Fthe%2Fphoto.jpg&search_city=searchville&search_state=ofconfusion&search_zip=92882&search_min_beds=12&search_min_baths=4&search_min_price=1234&search_max_price=22324&search_min_sq_ft=1234&search_max_sq_ft=23455&search_min_lot_size=29299&search_max_lot_size=292929&search_min_year_built=1999&search_max_year_built=2004&search_property_types=Condo%2C+Single-Family&note=whatevers2009
    */
    
    


}