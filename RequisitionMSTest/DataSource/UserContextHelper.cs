using Gep.Cumulus.CSM.Entities;
using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace RequisitionMSTest.DataSource
{
    [ExcludeFromCodeCoverage]
    public static class UserContextHelper
    {
        public static UserExecutionContext GetExecutionContext
        {
            get
            {
                var userExecutionContext = new UserExecutionContext
                {
                    BuyerPartnerCode = Convert.ToInt64(ConfigurationManager.AppSettings["BuyerPartnerCode"]), // 180830,
                    ClientName = "BuyerSqlConn",
                    CompanyName = "BuyerSqlConn",
                    ClientID = 2,
                    Product = GEPSuite.eCatalog,
                    UserId = 1,
                    EntityType = "Basic Setting",
                    EntityId = 8888,
                    LoggerCode = "EC101",
                    Culture = "en-US",
                    UserName = ConfigurationManager.AppSettings["UserName"], // "Gepper",
                    ContactCode = Convert.ToInt64(ConfigurationManager.AppSettings["ContactCode"]) //18083004000074
                };

                return userExecutionContext;
            }
        }
    }
}
