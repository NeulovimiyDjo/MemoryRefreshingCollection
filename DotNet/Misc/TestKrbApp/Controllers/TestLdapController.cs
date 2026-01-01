using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.DirectoryServices.Protocols;

namespace TestApp.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class TestLdapController : ControllerBase
    {
        private readonly ILogger<TestLdapController> _logger;
        private readonly IConfiguration _config;

        public TestLdapController(ILogger<TestLdapController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route(nameof(GetTestInfoFromLdap))]
        public string GetTestInfoFromLdap([FromQuery] string query)
        {
            string searchBase = _config.GetValue<string>("LdapSearchBase");
            string searchFilter = _config.GetValue<string>("LdapSearchFilter");
            string[] attributesList = _config.GetSection("LdapAttributesList").Get<string[]>();

            if (!string.IsNullOrEmpty(query))
                searchFilter = query;
            SearchResultEntryCollection searchResCollection = SearchData(searchBase, searchFilter, attributesList);
            string res = $"Query results (searchBase='{searchBase}',searchFilter='{searchFilter}':\n";
            foreach (SearchResultEntry searchRes in searchResCollection)
            {
                res += "-----\n";
                foreach (string attrName in searchRes.Attributes.AttributeNames)
                    res += $"{attrName}='{string.Join(";", searchRes.Attributes[attrName].GetValues(typeof(string)))}'\n";
            }
            return res;
        }

        private SearchResultEntryCollection SearchData(string searchBase, string searchFilter, string[] attributesList)
        {
            using LdapConnection connection = CreateLdapConnection();
            SearchRequest searchRequest = new(searchBase, searchFilter, SearchScope.Subtree, attributesList);
            SearchResponse response = (SearchResponse)connection.SendRequest(searchRequest, TimeSpan.FromSeconds(90));
            return response.Entries;
        }

        private LdapConnection CreateLdapConnection()
        {
            string ADSyncLdapServer = _config.GetValue<string>("LdapServer");

            LdapDirectoryIdentifier ldapDirectoryIdentifier = new(ADSyncLdapServer, 389);
            LdapConnection connection = new(ldapDirectoryIdentifier, null, AuthType.Kerberos);
            connection.SessionOptions.ProtocolVersion = 3;
            connection.Bind();
            return connection;
        }
    }
}
