using Gep.Cumulus.CSM.Entities;
using Gep.Cumulus.CSM.Extensions;
using Gep.Cumulus.ExceptionManager;
using Gep.Cumulus.Partner.Entities;
using GEP.Cumulus.Logging;
using GEP.Cumulus.OrganizationStructure.Entities;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.Cumulus.P2P.BusinessObjects.PartnerService;
using GEP.Cumulus.Web.Utils;
using GEP.SMART.Configuration;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.ServiceModel;
using PartnerContract = Gep.Cumulus.Partner.ServiceContracts;
using SMARTFaultException = Gep.Cumulus.ExceptionManager;
namespace GEP.Cumulus.P2P.Req.BusinessObjects.Proxy
{
    [ExcludeFromCodeCoverage]
    internal class ProxyPartnerService  : RequisitionBaseProxy
    {

        //
        // GET: /ProxyIRService/
        #region "Variables"
        public GepServiceFactory GepServices = GepServiceManager.GetInstance;

        private UserExecutionContext UserExecutionContext = null;

        private static readonly ILog log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);

        private string appName = System.Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition";
        public ProxyPartnerService(UserExecutionContext userExecutionContext, string jwtToken): base(userExecutionContext, jwtToken)
        {
            this.UserExecutionContext = GetUserExecutionContext();
        }
        public interface IPartnerServiceChannel : PartnerContract.IPartner, IClientChannel
        {
        }
        #endregion

        public Contact GetContactByContactCode(long PartnerCode, long ContactCode)
        {
            Contact contactDetails = new Contact();
            IPartnerChannel objPartnerServiceChannel = null;
            OperationContextScope scope = null;
            GEPBaseProxy gepBaseProxy = new GEPBaseProxy();
            try
            {
                objPartnerServiceChannel = gepBaseProxy.ConfigureChannel<IPartnerChannel>(CloudConfig.PartnerServiceURL, UserExecutionContext, ref scope);

                MessageHeader<string> objMhgAuth = new MessageHeader<string>(GetToken());
                System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                contactDetails = objPartnerServiceChannel.GetContactByContactCode(PartnerCode, ContactCode);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(log, "Error occured in GetContactByContactCode Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetContactByContactCode", "GetContactByContactCode", "AppStartBO", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetContactByContactCode");
            }
            finally
            {
                GEPServiceManager.DisposeService(objPartnerServiceChannel, scope);
            }
            return contactDetails;
        }

        public List<Contact> GetAllContactsByPartnerCode(long PartnerCode, int pageno, int pagesize, string SortName, string SortDirection, string PASCode, string RegionId, string OrgEntityCode, string SearchText)
        {
            List<Contact> lstcontactDetails = new List<Contact>();
            IPartnerChannel objPartnerServiceChannel = null;
            OperationContextScope scope = null;
            GEPBaseProxy gepBaseProxy = new GEPBaseProxy();
            try
            {
                objPartnerServiceChannel = gepBaseProxy.ConfigureChannel<IPartnerChannel>(CloudConfig.PartnerServiceURL, UserExecutionContext, ref scope);

                MessageHeader<string> objMhgAuth = new MessageHeader<string>(GetToken());
                System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                lstcontactDetails = objPartnerServiceChannel.GetAllContactsByPartnerCode(PartnerCode, pageno, pagesize, SortName, SortDirection, PASCode, RegionId, OrgEntityCode, SearchText);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(log, "Error occured in GetAllContactsByPartnerCode Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetAllContactsByPartnerCode", "GetAllContactsByPartnerCode", "AppStartBO", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetAllContactsByPartnerCode");
            }
            finally
            {
                GEPServiceManager.DisposeService(objPartnerServiceChannel, scope);
            }
            return lstcontactDetails;
        }

        public List<User> GetUsersBasedOnActivityCode(string ActivityCodes, string SearchText, long PartnerCode, int PageIndex, int PageSize)
        {
            List<User> usersBasedOnActivityCode = new List<User>();
            IPartnerChannel objPartnerServiceChannel = null;
            OperationContextScope scope = null;
            GEPBaseProxy gepBaseProxy = new GEPBaseProxy();
            try
            {
                objPartnerServiceChannel = gepBaseProxy.ConfigureChannel<IPartnerChannel>(CloudConfig.PartnerServiceURL, UserExecutionContext, ref scope);

                MessageHeader<string> objMhgAuth = new MessageHeader<string>(GetToken());
                System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                usersBasedOnActivityCode = objPartnerServiceChannel.GetUsersBasedOnActivityCode(ActivityCodes, SearchText, PartnerCode, PageIndex, PageSize);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(log, "Error occured in GetUsersBasedOnActivityCode Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetUsersBasedOnActivityCode", "GetUsersBasedOnActivityCode", "AppStartBO", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetUsersBasedOnActivityCode");
            }
            finally
            {
                GEPServiceManager.DisposeService(objPartnerServiceChannel, scope);
            }
            return usersBasedOnActivityCode;
        }

        public ContactPreferences GetContactInformationByContactCode(long PartnerCode, long ContactCode)
        {
            ContactPreferences contactPreferences = new ContactPreferences();
            IPartnerChannel objPartnerServiceChannel = null;
            OperationContextScope scope = null;
            GEPBaseProxy gepBaseProxy = new GEPBaseProxy();
            try
            {
                objPartnerServiceChannel = gepBaseProxy.ConfigureChannel<IPartnerChannel>(CloudConfig.PartnerServiceURL, UserExecutionContext, ref scope);

                MessageHeader<string> objMhgAuth = new MessageHeader<string>(GetToken());
                System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                contactPreferences = objPartnerServiceChannel.GetContactInformationByContactCode(PartnerCode, ContactCode);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(log, "Error occured in GetContactInformationByContactCode Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetContactInformationByContactCode", "GetContactInformationByContactCode", "AppStartBO", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetContactInformationByContactCode");
            }
            finally
            {
                GEPServiceManager.DisposeService(objPartnerServiceChannel, scope);
            }
            return contactPreferences;
        }

        public string GetUserActivitiesByContactCode(long ContactCode, long PartnerCode)
        {
            string contactPreferences = string.Empty;
            IPartnerChannel objPartnerServiceChannel = null;
            OperationContextScope scope = null;
            GEPBaseProxy gepBaseProxy = new GEPBaseProxy();
            try
            {
                objPartnerServiceChannel = gepBaseProxy.ConfigureChannel<IPartnerChannel>(CloudConfig.PartnerServiceURL, UserExecutionContext, ref scope);

                MessageHeader<string> objMhgAuth = new MessageHeader<string>(GetToken());
                System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                contactPreferences = objPartnerServiceChannel.GetUserActivitiesByContactCode(ContactCode, PartnerCode);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(log, "Error occured in GetContactInformationByContactCode Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetContactInformationByContactCode", "GetContactInformationByContactCode", "AppStartBO", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetContactInformationByContactCode");
            }
            finally
            {
                GEPServiceManager.DisposeService(objPartnerServiceChannel, scope);
            }
            return contactPreferences;
        }

        public List<User> GetUserDetailsByContactCodes(string contactCodes, bool bChkActiveFlag)
        {
            List<User> lstUsers = new List<User>();
            IPartnerServiceChannel objPartnerServiceChannel = null;
            OperationContextScope scope = null;
            GEPBaseProxy gepBaseProxy = new GEPBaseProxy();
            try
            {
                objPartnerServiceChannel = gepBaseProxy.ConfigureChannel<IPartnerServiceChannel>(CloudConfig.PartnerServiceURL, UserExecutionContext, ref scope);

                MessageHeader<string> objMhgAuth = new MessageHeader<string>(GetToken());
                System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                lstUsers = objPartnerServiceChannel.GetUserDetailsByContactCodes(contactCodes, bChkActiveFlag);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(log, "Error occured in GetUserDetailsByContactCodes Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetUserDetailsByContactCodes", "GetUserDetailsByContactCodes", "AppStartBO", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetUserDetailsByContactCodes");
            }
            finally
            {
                GEPServiceManager.DisposeService(objPartnerServiceChannel, scope);
            }
            return lstUsers;
        }

        public UserContext GetUserContextDetailsByContactCode(long contactCode)
        {
            IPartnerServiceChannel objPartnerServiceChannel = null;
            OperationContextScope scope = null;
            GEPBaseProxy gepBaseProxy = new GEPBaseProxy();
            try
            {
                objPartnerServiceChannel = gepBaseProxy.ConfigureChannel<IPartnerServiceChannel>(CloudConfig.PartnerServiceURL, UserExecutionContext, ref scope);

                MessageHeader<string> objMhgAuth = new MessageHeader<string>(GetToken());
                System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                return objPartnerServiceChannel.GetUserContextDetailsByContactCode(contactCode);

            }
            catch (Exception ex)
            {
                LogHelper.LogError(log, "Error occured in GetUserContextDetailsByContactCode Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetUserContextDetailsByContactCode", "GetUserContextDetailsByContactCode", "AppStartBO", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetUserContextDetailsByContactCode");
            }
            finally
            {
                GEPServiceManager.DisposeService(objPartnerServiceChannel, scope);
            }
        }

        public List<ContactORGMapping> GetContactORGMapping(ContactORGMapping objContact)
        {
            IPartnerServiceChannel objPartnerServiceChannel = null;
            OperationContextScope scope = null;
            GEPBaseProxy gepBaseProxy = new GEPBaseProxy();
            try
            {
                objPartnerServiceChannel = gepBaseProxy.ConfigureChannel<IPartnerServiceChannel>(CloudConfig.PartnerServiceURL, UserExecutionContext, ref scope);

                MessageHeader<string> objMhgAuth = new MessageHeader<string>(GetToken());
                System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                using (scope)
                    return objPartnerServiceChannel.GetContactORGMapping(objContact);

            }
            catch (Exception ex)
            {
                LogHelper.LogError(log, "Error occured in GetContactORGMapping Method for objContact =" + JSONHelper.ToJSON(objContact), ex);
                var objCustomFault = new CustomFault(ex.Message, "GetContactORGMapping", "GetContactORGMapping", "AppStartBO", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetContactORGMapping");
            }
            finally
            {
                GEPServiceManager.DisposeService(objPartnerServiceChannel, scope);
            }
        }

        public List<PartnerInfo> GetBuyerSuppliersAutoSuggest(long BuyerPartnerCode, string Status, string SearchText, int PageIndex, int PageSize, string OrgEntityCodes, long LOBEntityDetailCode)
        {
            IPartnerServiceChannel objPartnerServiceChannel = null;
            OperationContextScope scope = null;
            GEPBaseProxy gepBaseProxy = new GEPBaseProxy();
            try
            {
                objPartnerServiceChannel = gepBaseProxy.ConfigureChannel<IPartnerServiceChannel>(CloudConfig.PartnerServiceURL, UserExecutionContext, ref scope);

                MessageHeader<string> objMhgAuth = new MessageHeader<string>(GetToken());
                System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                using (scope)
                    return objPartnerServiceChannel.GetBuyerSuppliersAutoSuggest(BuyerPartnerCode, Status, SearchText, PageIndex, PageSize, OrgEntityCodes, LOBEntityDetailCode);

            }
            catch (Exception ex)
            {
                LogHelper.LogError(log, "Error occured in GetBuyerSuppliersAutoSuggest Method ", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetBuyerSuppliersAutoSuggest", "GetBuyerSuppliersAutoSuggest", "GetBuyerSuppliersAutoSuggest", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetBuyerSuppliersAutoSuggest");
            }
            finally
            {
                GEPServiceManager.DisposeService(objPartnerServiceChannel, scope);
            }
        }

        public Contact_ORG_Mapping GetContactOrgMappingDetails(ORGMaster_InputParams ORG_InputParams)
        {
            IPartnerServiceChannel objPartnerServiceChannel = null;
            OperationContextScope scope = null;
            GEPBaseProxy gepBaseProxy = new GEPBaseProxy();
            try
            {
                objPartnerServiceChannel = gepBaseProxy.ConfigureChannel<IPartnerServiceChannel>(CloudConfig.PartnerServiceURL, UserExecutionContext, ref scope);

                MessageHeader<string> objMhgAuth = new MessageHeader<string>(GetToken());
                System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                using (scope)
                    return objPartnerServiceChannel.GetContactOrgMappingDetails(ORG_InputParams);

            }
            catch (Exception ex)
            {
                LogHelper.LogError(log, "Error occured in GetContactOrgMappingDetails Method ", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetContactOrgMappingDetails", "GetContactOrgMappingDetails", "GetContactOrgMappingDetails", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetContactOrgMappingDetails");
            }
            finally
            {
                GEPServiceManager.DisposeService(objPartnerServiceChannel, scope);
            }
        }

        public List<UserLOBMapping> GetUserLOBDetailByContactCode(long contactCode)
        {
            IPartnerServiceChannel objPartnerServiceChannel = null;
            OperationContextScope scope = null;
            GEPBaseProxy gepBaseProxy = new GEPBaseProxy();
            try
            {
                objPartnerServiceChannel = gepBaseProxy.ConfigureChannel<IPartnerServiceChannel>(CloudConfig.PartnerServiceURL, UserExecutionContext, ref scope);

                MessageHeader<string> objMhgAuth = new MessageHeader<string>(GetToken());
                System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                return objPartnerServiceChannel.GetUserLOBDetailByContactCode(contactCode);

            }
            catch (Exception ex)
            {
                LogHelper.LogError(log, "Error occured in GetUserLOBDetailByContactCode Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetUserLOBDetailByContactCode", "GetUserLOBDetailByContactCode", "AppStartBO", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetUserLOBDetailByContactCode");
            }
            finally
            {
                GEPServiceManager.DisposeService(objPartnerServiceChannel, scope);
            }
        }

        public ContactPreferences GetContactPreferenceByContactCode(long PartnerCode, long ContactCode)
        {
            ContactPreferences contactPreferences = new ContactPreferences();
            IPartnerChannel objPartnerServiceChannel = null;
            OperationContextScope scope = null;
            GEPBaseProxy gepBaseProxy = new GEPBaseProxy();
            try
            {
                objPartnerServiceChannel = gepBaseProxy.ConfigureChannel<IPartnerChannel>(CloudConfig.PartnerServiceURL, UserExecutionContext, ref scope);

                MessageHeader<string> objMhgAuth = new MessageHeader<string>(GetToken());
                System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                contactPreferences = objPartnerServiceChannel.GetContactPreferenceByContactCode(PartnerCode, ContactCode);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(log, "Error occured in GetContactPreferenceByContactCode Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetContactPreferenceByContactCode", "GetContactPreferenceByContactCode", "AppStartBO", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetContactPreferenceByContactCode");
            }
            finally
            {
                GEPServiceManager.DisposeService(objPartnerServiceChannel, scope);
            }
            return contactPreferences;
        }

        public List<PASMaster> GetContactPASDefault(long ContactCode)
        {
            IPartnerChannel objPartnerServiceChannel = null;
            OperationContextScope scope = null;
            GEPBaseProxy gepBaseProxy = new GEPBaseProxy();
            try
            {
                objPartnerServiceChannel = gepBaseProxy.ConfigureChannel<IPartnerChannel>(CloudConfig.PartnerServiceURL, UserExecutionContext, ref scope);

                MessageHeader<string> objMhgAuth = new MessageHeader<string>(GetToken());
                System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                return objPartnerServiceChannel.GetContactPASDefault(ContactCode);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(log, "Error occured in GetContactPASDefault Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetContactPASDefault", "GetContactPASDefault", "AppStartBO", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetContactPASDefault");
            }
            finally
            {
                GEPServiceManager.DisposeService(objPartnerServiceChannel, scope);
            }
        }

        public List<Contact> GetContactsForGroup(string GroupId)
        {
            List<Contact> lstContacts = new List<Contact>();
            IPartnerChannel objPartnerServiceChannel = null;
            OperationContextScope scope = null;
            GEPBaseProxy gepBaseProxy = new GEPBaseProxy();
            try
            {
                objPartnerServiceChannel = gepBaseProxy.ConfigureChannel<IPartnerChannel>(CloudConfig.PartnerServiceURL, UserExecutionContext, ref scope);

                MessageHeader<string> objMhgAuth = new MessageHeader<string>(GetToken());
                System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                lstContacts = objPartnerServiceChannel.GetContactsForGroup(GroupId);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(log, "Error occured in GetContactsForGroup Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetContactsForGroup", "GetContactsForGroup", "AppStartBO", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetContactsForGroup");
            }
            finally
            {
                GEPServiceManager.DisposeService(objPartnerServiceChannel, scope);
            }
            return lstContacts;
        }

        public ContactAccountingInfo GetContactAccountingInfoByContactCode(long contactCode)
        {
            IPartnerServiceChannel objPartnerServiceChannel = null;
            OperationContextScope scope = null;
            GEPBaseProxy gepBaseProxy = new GEPBaseProxy();
            try
            {
                objPartnerServiceChannel = gepBaseProxy.ConfigureChannel<IPartnerServiceChannel>(CloudConfig.PartnerServiceURL, UserExecutionContext, ref scope);

                MessageHeader<string> objMhgAuth = new MessageHeader<string>(GetToken());
                System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                using (scope)
                    return objPartnerServiceChannel.GetContactAccountingInfoByContactCode(contactCode);

            }
            catch (Exception ex)
            {
                LogHelper.LogError(log, "Error occured in GetContactAccountingInfoByContactCode Method ", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetContactAccountingInfoByContactCode", "GetContactAccountingInfoByContactCode", "GetContactAccountingInfoByContactCode", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetContactAccountingInfoByContactCode");
            }
            finally
            {
                GEPServiceManager.DisposeService(objPartnerServiceChannel, scope);
            }
        }

        public List<User> GetSpecificUserApproversBasedOnActivityCode(string SearchText, int WFDocTypeId, int pageIndex = 1, int pageSize = 10, string ORGEntityCodes = "", decimal ThresholdAmt = 0, string PasCodes = "", string CurrencyCode = "")
        {
            IPartnerServiceChannel objPartnerServiceChannel = null;
            OperationContextScope scope = null;
            GEPBaseProxy gepBaseProxy = new GEPBaseProxy();
            List<User> lstUsers = new List<User>();
            try
            {
                objPartnerServiceChannel = gepBaseProxy.ConfigureChannel<IPartnerServiceChannel>(CloudConfig.PartnerServiceURL, UserExecutionContext, ref scope);

                MessageHeader<string> objMhgAuth = new MessageHeader<string>(GetToken());
                System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                if (objPartnerServiceChannel != null)
                {
                    if (WFDocTypeId == 7)
                        lstUsers = objPartnerServiceChannel.GetUsersBasedOnEntityCodesORPASCodes("10700001", SearchText, UserExecutionContext.BuyerPartnerCode, pageIndex, pageSize, ORGEntityCodes, ThresholdAmt, PasCodes, CurrencyCode);
                    else if (WFDocTypeId == 8)
                        lstUsers = objPartnerServiceChannel.GetUsersBasedOnEntityCodesORPASCodes("10700011", SearchText, UserExecutionContext.BuyerPartnerCode, pageIndex, pageSize, ORGEntityCodes, ThresholdAmt, PasCodes, CurrencyCode);
                    else if (WFDocTypeId == 9 || WFDocTypeId == 10)
                        lstUsers = objPartnerServiceChannel.GetUsersBasedOnEntityCodesORPASCodes("10700056", SearchText, UserExecutionContext.BuyerPartnerCode, pageIndex, pageSize, ORGEntityCodes, ThresholdAmt, PasCodes, CurrencyCode);
                    else if (WFDocTypeId == (int)Gep.Cumulus.CSM.Entities.Enums.WFDocTypeId.PaymentRequest)
                        lstUsers = objPartnerServiceChannel.GetUsersBasedOnEntityCodesORPASCodes("10700075", SearchText, UserExecutionContext.BuyerPartnerCode, pageIndex, pageSize, ORGEntityCodes, ThresholdAmt, PasCodes, CurrencyCode);
                    else
                        lstUsers = objPartnerServiceChannel.GetUsersBasedOnEntityCodesORPASCodes(string.Empty, SearchText, UserExecutionContext.BuyerPartnerCode, pageIndex, pageSize, ORGEntityCodes, ThresholdAmt, PasCodes, CurrencyCode);
                }
                return (lstUsers);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(log, "Error occured in GetSpecificUserApproversBasedOnActivityCode Method ", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetSpecificUserApproversBasedOnActivityCode", "GetSpecificUserApproversBasedOnActivityCode", "GetSpecificUserApproversBasedOnActivityCode", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetSpecificUserApproversBasedOnActivityCode");
            }
            finally
            {
                GEPServiceManager.DisposeService(objPartnerServiceChannel, scope);
            }
        }

        public PartnerLinkedLocationMapping GetLinkedLocationBySourceSystemValue(string SourceSystemValue)
        {
            IPartnerServiceChannel objPartnerServiceChannel = null;
            OperationContextScope scope = null;
            GEPBaseProxy gepBaseProxy = new GEPBaseProxy();
            try
            {
                objPartnerServiceChannel = gepBaseProxy.ConfigureChannel<IPartnerServiceChannel>(CloudConfig.PartnerServiceURL, UserExecutionContext, ref scope);

                MessageHeader<string> objMhgAuth = new MessageHeader<string>(GetToken());
                System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                using (scope)
                    return objPartnerServiceChannel.GetLinkedLocationBySourceSystemValue(SourceSystemValue);

            }
            catch (Exception ex)
            {
                LogHelper.LogError(log, "Error occured in GetLinkedLocationBySourceSystemValue Method ", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetLinkedLocationBySourceSystemValue", "GetLinkedLocationBySourceSystemValue", "GetLinkedLocationBySourceSystemValue", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetLinkedLocationBySourceSystemValue");
            }
            finally
            {
                GEPServiceManager.DisposeService(objPartnerServiceChannel, scope);
            }
        }

        public long GetContactCodeByClientContactCodeOrEmail(string clientContactCode, string clientEmailId, long PartnerCode)
        {
            long contactCode = 0;
            IPartnerChannel objPartnerServiceChannel = null;
            OperationContextScope scope = null;
            GEPBaseProxy gepBaseProxy = new GEPBaseProxy();
            try
            {
                objPartnerServiceChannel = gepBaseProxy.ConfigureChannel<IPartnerChannel>(CloudConfig.PartnerServiceURL, UserExecutionContext, ref scope);

                MessageHeader<string> objMhgAuth = new MessageHeader<string>(GetToken());
                System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                contactCode = objPartnerServiceChannel.GetContactCodeByClientContactCodeOrEmail(clientContactCode, clientEmailId, PartnerCode);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(log, "Error occured in GetContactCodeByClientContactCodeOrEmail Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetContactCodeByClientContactCodeOrEmail", "GetContactCodeByClientContactCodeOrEmail", "AppStartBO", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetContactCodeByClientContactCodeOrEmail");
            }
            finally
            {
                GEPServiceManager.DisposeService(objPartnerServiceChannel, scope);
            }
            return contactCode;
        }

        public void UpdateSplitDeatilsBasedOnHeaderEntity(List<DocumentAdditionalEntityInfo> DocumentAdditionalEntitiesInfoList, DataSet result)
        {
            foreach (var item in DocumentAdditionalEntitiesInfoList)
            {
                for (int intCount = 0; intCount < result.Tables[0].Rows.Count; intCount++)
                {
                    if (item.EntityId.Equals(result.Tables[0].Rows[intCount]["EntityTypeId"]))
                    {
                        result.Tables[0].Rows[intCount]["EntityCode"] = item.EntityCode;
                        result.Tables[0].Rows[intCount]["EntityDetailCode"] = item.EntityDetailCode;

                    }
                }
            }
        }

        public long GetPartnerCodeByClientPartnerCode(string clientPartnerCode, string sourceSystemName = "")
        {
            IPartnerServiceChannel objPartnerServiceChannel = null;
            OperationContextScope scope = null;
            GEPBaseProxy gepBaseProxy = new GEPBaseProxy();
            try
            {
                objPartnerServiceChannel = gepBaseProxy.ConfigureChannel<IPartnerServiceChannel>(CloudConfig.PartnerServiceURL, UserExecutionContext, ref scope);

                MessageHeader<string> objMhgAuth = new MessageHeader<string>(GetToken());
                System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                using (scope)
                    return objPartnerServiceChannel.GetPartnerCodeByClientPartnerCode(clientPartnerCode, sourceSystemName);

            }
            catch (Exception ex)
            {
                LogHelper.LogError(log, "Error occured in GetPartnerCodeByClientPartnerCode Method ", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetPartnerCodeByClientPartnerCode", "GetPartnerCodeByClientPartnerCode", "GetPartnerCodeByClientPartnerCode", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetPartnerCodeByClientPartnerCode");
            }
            finally
            {
                GEPServiceManager.DisposeService(objPartnerServiceChannel, scope);
            }
        }

        public PartnerDetails GetPartnerDetails(long PartnerCode, string CultureCode)
        {
            PartnerDetails partnerContactDetails = new PartnerDetails();
            IPartnerChannel objPartnerServiceChannel = null;
            OperationContextScope scope = null;
            GEPBaseProxy gepBaseProxy = new GEPBaseProxy();
            try
            {
                objPartnerServiceChannel = gepBaseProxy.ConfigureChannel<IPartnerChannel>(CloudConfig.PartnerServiceURL, UserExecutionContext, ref scope);

                MessageHeader<string> objMhgAuth = new MessageHeader<string>(GetToken());
                System.ServiceModel.Channels.MessageHeader authorization = objMhgAuth.GetUntypedHeader("Authorization", "Gep.Cumulus");
                OperationContext.Current.OutgoingMessageHeaders.Add(authorization);

                partnerContactDetails = objPartnerServiceChannel.GetPartnerDetails(PartnerCode, CultureCode);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(log, "Error occured in GetPartnerDetails Method", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetPartnerDetails", "GetPartnerDetails", "AppStartBO", ExceptionType.ApplicationException, "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while GetPartnerDetails");
            }
            finally
            {
                GEPServiceManager.DisposeService(objPartnerServiceChannel, scope);
            }
            return partnerContactDetails;
        }
           }
}
