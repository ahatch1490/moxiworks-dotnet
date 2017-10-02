﻿
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Web.Configuration;


namespace MoxiWorks.Platform
{
    public class UriBuilder
    {

        private const string DEFAULT_HOST = "https://api.moxiworks.com/api/";
        private string _host = null; 
        private string Host
        {
            get
            {
               _host = _host ??  WebConfigurationManager.AppSettings["MoxiWorksApiHost"];
               _host = _host ?? DEFAULT_HOST;

               return _host;
            }
        }

        public Dictionary<string, string> QueryParameters { get; } =  new Dictionary<string, string>(); 
        private Uri Path { get; }

        public UriBuilder(string path)
        {
            Path = new Uri(Host +  path); 
        }

        public string GetUrl()
        {
            return Path + BuildQueryString();
        }

        public string BuildQueryString()
        {
            if (QueryParameters.Count <= 0)
            return string.Empty;
                
            return "?" + string.Join("&", QueryParameters.Select(q => $"{  HttpUtility.UrlEncode(q.Key)}={HttpUtility.UrlEncode(q.Value)}"));
        }

        public void AddQueryParameter(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                return; 
            }
            
            QueryParameters.Add(key,value);
        }

        public void AddQueryParameter(string key, int? value)
        {
            if (string.IsNullOrWhiteSpace(key) || ! value.HasValue)
            {
                return; 
            }
            
            QueryParameters.Add(key,value.Value.ToString());
        }
        
        
        
        
        
    }
}