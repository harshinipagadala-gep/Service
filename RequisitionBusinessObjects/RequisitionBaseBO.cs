using Gep.Cumulus.Partner.Entities;
using GEP.Cumulus.Caching;
using GEP.Cumulus.Documents.DataAccessObjects.SQLServer;
using GEP.Cumulus.Logging;
using GEP.Cumulus.P2P.BusinessObjects;
using GEP.Cumulus.P2P.DataAccessObjects.SQLServer;
using GEP.Cumulus.P2P.Req.DataAccessObjects;
using GEP.SMART.Security.ClaimsManagerNet;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace GEP.Cumulus.P2P.Req.BusinessObjects
{
    public class RequisitionBaseBO : Gep.Cumulus.CSM.BaseBusinessObjects.BaseBO
    {
        private static readonly ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);

        string _jwtToken = string.Empty;
        public string JWTToken
        {
            get
            {   
                if (string.IsNullOrEmpty(_jwtToken) && SmartClaimsManager.IsAuthenticated())
                {
                    _jwtToken = "Bearer " + GEP.SMART.Security.ClaimsManagerNet.JwtTokenHelper.CreateJwtTokenFromClaimsIdentity();
                }
                return _jwtToken;
            }
            set
            {
                _jwtToken = value;
                if (string.IsNullOrEmpty(_jwtToken) && SmartClaimsManager.IsAuthenticated())
                {
                    _jwtToken = "Bearer " + GEP.SMART.Security.ClaimsManagerNet.JwtTokenHelper.CreateJwtTokenFromClaimsIdentity();
                }               
            }
        }

        public RequisitionBaseBO(string jwtToken)
        {
            this.JWTToken = jwtToken;
        }

        public RequisitionInterfaceDAO GetReqInterfaceDao()
        {
            return new RequisitionInterfaceDAO()
            {
                GepConfiguration = this.GepConfiguration,
                UserContext = this.UserContext
            };

        }

        public OperationalBudgetManager GetOperationalBudgetManager()
        {
            return new OperationalBudgetManager()
            {
                GepConfiguration = this.GepConfiguration,
                UserContext = this.UserContext,
                jwtToken = this.JWTToken
            };
        }

        public SQLRequisitionDAO GetReqDao()
        {
            return new SQLRequisitionDAO()
            {
                GepConfiguration = this.GepConfiguration,
                UserContext = this.UserContext
            };
        }

        public NewRequisitionDAO GetNewReqDao()
        {
            return new NewRequisitionDAO()
            {
                GepConfiguration = this.GepConfiguration,
                UserContext = this.UserContext
            };
        }

        public SQLCommonDAO GetCommonDao()
        {
            return new SQLCommonDAO()
            {
                GepConfiguration = this.GepConfiguration,
                UserContext = this.UserContext
            };
        }

        public SQLDocumentDAO GetDocumentDao()
        {
            return new SQLDocumentDAO()
            {
                GepConfiguration = this.GepConfiguration,
                UserContext = this.UserContext
            };
        }

        public SQLP2PDocumentDAO GetSQLP2PDocumentDAO()
        {
            return new SQLP2PDocumentDAO()
            {
                GepConfiguration = this.GepConfiguration,
                UserContext = this.UserContext
            };
        }

        public RequisitionCommonDAO GetRequisitionCommonDAO()
        {
            return new RequisitionCommonDAO()
            {
                GepConfiguration = this.GepConfiguration,
                UserContext = this.UserContext
            };
        }

        public string GetCurrencySymbol(string currencyCode)
        {
            if (!string.IsNullOrEmpty(currencyCode))
            {
                var lstRegionInfo = (from culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                                     let region = new System.Globalization.RegionInfo(culture.LCID)
                                     where String.Equals(region.ISOCurrencySymbol, currencyCode, StringComparison.InvariantCultureIgnoreCase)
                                     select region).ToList();

                return lstRegionInfo.Any() ? lstRegionInfo.First().CurrencySymbol : string.Empty;
            }
            return string.Empty;
        }

        public int convertStringToInt(string value)
        {
            return Convert.ToInt16(string.IsNullOrEmpty(value) ? "0" : value);
        }

        public List<ContactORGMapping> GetContactORGMapping(long contactCode, int entityId)
        {
            List<ContactORGMapping> retContactORGMapping = GEPDataCache.GetFromCacheJSON<List<ContactORGMapping>>(CacheConstants.ContactORGMapping, UserContext.BuyerPartnerCode, contactCode, "en-US");
            if (retContactORGMapping == null)
            {
                try
                {
                    GEP.Cumulus.Web.Utils.UserIdentityContext objUserIdentityContext = new GEP.Cumulus.Web.Utils.UserIdentityContext(UserContext);
                    retContactORGMapping = objUserIdentityContext.BUByContactCode(contactCode, entityId);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, "Error occured in GetContactORGMapping Method in NewBOManager", ex);
                    GEPDataCache.RemoveFromCache(CacheConstants.ContactORGMapping, UserContext.BuyerPartnerCode, contactCode, "en-US");
                    throw;
                }
            }

            if (entityId > 0 && retContactORGMapping.Count() > 0)
                retContactORGMapping = retContactORGMapping.Where(n => n.EntityId == entityId).ToList();
            return retContactORGMapping;
        }
    }
}
