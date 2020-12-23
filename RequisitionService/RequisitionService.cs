using Gep.Cumulus.CSM.BaseService;
using Gep.Cumulus.CSM.Config;
using Gep.Cumulus.CSM.Entities;
using Gep.Cumulus.CSM.Extensions;
using Gep.Cumulus.ExceptionManager;
using Gep.Cumulus.Partner.Entities;
using GEP.Cumulus.DocumentIntegration.Entities;
using GEP.Cumulus.Documents.Entities;
using GEP.Cumulus.Logging;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.Cumulus.P2P.BusinessObjects;
using GEP.Cumulus.P2P.Req.BusinessObjects;
using GEP.Cumulus.P2P.Req.ServiceContracts;
using GEP.Cumulus.Web.Utils;
using GEP.NewP2PEntities.FileManagerEntities;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using SMARTFaultException = Gep.Cumulus.ExceptionManager;
[assembly: CLSCompliant(true)]
namespace GEP.Cumulus.P2P.Req.Service
{
    [ExcludeFromCodeCoverage]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class RequisitionService : GepService, GEP.Cumulus.P2P.Req.ServiceContracts.IRequisitionService
    {
        private static readonly ILog Log = Logger.GetLog(MethodBase.GetCurrentMethod().DeclaringType);

        public string JWTToken { get; set; }



        public RequisitionService(GepConfig config) : base(config)
        {

        }

        public Requisition GetRequisitionBasicDetailsById(long requisitionId, long userId, int typeOfUser = 0)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return (Requisition)GetRequisitionDocumentManager().GetBasicDetailsById(P2PDocumentType.Requisition, requisitionId, userId, typeOfUser);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionId: " + requisitionId, "----userId: " + userId, "----TypeOfUser: " + typeOfUser);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition GetRequisitionBasicDetailsById Method Buyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                UtilsManager.ThrowHttpException(ex.Message);

                var objCustomFault = new CustomFault(ex.Message, "GetRequisitionBasicDetailsById", "GetRequisitionBasicDetailsById",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     requisitionId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error occured while getting Requisition Basic details for requisitionId = " +
                                                      requisitionId.ToString(CultureInfo.InvariantCulture));
            }
        }
        public Requisition GetRequisitionBasicAndValidationDetailsById(long requisitionId, long userId, int typeOfUser, bool filterByBU = true, bool isFunctionalAdmin = false)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return (Requisition)GetRequisitionDocumentManager().GetBasicAndValidationDetailsById(P2PDocumentType.Requisition, requisitionId, userId, typeOfUser, filterByBU, isFunctionalAdmin);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionId: " + requisitionId, "----UserId: " + userId, "----IsFunctionalAdmin: " + isFunctionalAdmin);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition GetRequisitionBasicAndValidationDetailsById Method Buyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                UtilsManager.ThrowHttpException(ex.Message);

                var objCustomFault = new CustomFault(ex.Message, "GetRequisitionBasicAndValidationDetailsById", "GetRequisitionBasicAndValidationDetailsById",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     requisitionId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error occured while getting Requisition Basic and Validation details for requisitionId = " +
                                                      requisitionId.ToString(CultureInfo.InvariantCulture));
            }
        }

        public List<P2PDocumentValidationInfo> GetRequisitionValidationDetailsById(long requisitionId, bool isOnSubmit = false)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionDocumentManager().GetValidationDetailsById(P2PDocumentType.Requisition, requisitionId, isOnSubmit);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionId: " + requisitionId, "----IsOnSubmit: " + isOnSubmit);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition GetRequisitionValidationDetailsById MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "GetRequisitionValidationDetailsById", "GetRequisitionValidationDetailsById",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     requisitionId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error occured while getting Requisition validation details for requisitionId = " +
                                                      requisitionId.ToString(CultureInfo.InvariantCulture));
            }
        }

        public ICollection<RequisitionItem> GetRequisitionLineItemBasicDetails(long requisitionId, ItemType itemType, int startIndex, int pageSize, string sortBy, string sortOrder, int typeOfUser = 0)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionDocumentManager()
                       .GetLineItemBasicDetails(P2PDocumentType.Requisition, requisitionId, itemType, startIndex, pageSize,
                                                sortBy, sortOrder, typeOfUser).Cast<RequisitionItem>().ToList();
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionId: " + requisitionId, "----TypeOfUser: " + typeOfUser);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition GetRequisitionLineItemBasicDetails MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "GetRequisitionLineItemBasicDetails", "GetRequisitionLineItemBasicDetails",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     requisitionId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error occured while getting Requisition line item Basic details for requisitionId = " +
                                                      requisitionId.ToString(CultureInfo.InvariantCulture));
            }
        }

        public string GenerateDefaultRequisitionName(long userId, long preDocumentId, long LOBEntityDetailCode)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionDocumentManager().GenerateDefaultName(P2PDocumentType.Requisition, userId, preDocumentId, LOBEntityDetailCode);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("UserId: " + userId, "----PreDocumentId: " + preDocumentId);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition GenerateDefaultRequisitionName MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "GenerateDefaultRequisitionName", "GenerateDefaultRequisitionName",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     preDocumentId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error occured While Generate Default Requisition Name for PreDocumentId= " +
                                                      preDocumentId.ToString(CultureInfo.InvariantCulture));
            }
        }

        public long SaveRequisition(Requisition objRequisition, bool isShipToChanged)
        {
            string finalJsonObj = string.Empty;
            if (ReferenceEquals(null, objRequisition))
                throw new ArgumentNullException("objRequisition");
            try
            {
                long reqId = GetRequisitionDocumentManager().Save(P2PDocumentType.Requisition, objRequisition, isShipToChanged);
                return reqId;
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionId: " + objRequisition != null ? objRequisition.DocumentCode : 0, "----IsShipToChanged: " + isShipToChanged);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition SaveRequisition MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "SaveRequisition", "SaveRequisition",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     objRequisition.DocumentId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while Saving Requisition Basic details for DocumentId = " +
                                                      objRequisition.DocumentId.ToString(CultureInfo.InvariantCulture));
            }

        }

        public long SaveRequisitionItem(RequisitionItem objRequisitionItem, bool isFunctionalAdmin = false)
        {
            string finalJsonObj = string.Empty;
            if (ReferenceEquals(null, objRequisitionItem))
                throw new ArgumentNullException("objRequisitionItem");
            try
            {
                return GetRequisitionDocumentManager().SaveItem(P2PDocumentType.Requisition, objRequisitionItem);
            }
            catch (Exception ex)
            {

                finalJsonObj = string.Concat("DocumentItemId: " + objRequisitionItem != null ? objRequisitionItem.DocumentItemId : 0, "----IsFunctionalAdmin: " + isFunctionalAdmin);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition SaveRequisitionItem MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "SaveRequisitionItem", "SaveRequisitionItem",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     objRequisitionItem.DocumentItemId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while Saving Requisition LineItem for DocumentItemId = " +
                                                      objRequisitionItem.DocumentItemId.ToString(CultureInfo.InvariantCulture));

            }

        }

        public long SaveRequisitionItemPartnerDetails(RequisitionItem objRequisitionItem)
        {
            string finalJsonObj = string.Empty;
            if (ReferenceEquals(null, objRequisitionItem))
                throw new ArgumentNullException("objRequisitionItem");
            try
            {
                return GetRequisitionDocumentManager().SaveItemPartnerDetails(P2PDocumentType.Requisition, objRequisitionItem);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("P2PLineItemId: " + objRequisitionItem != null ? objRequisitionItem.P2PLineItemId : 0);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition SaveRequisitionItemPartnerDetails MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "SaveRequisitionItemPartnerDetails", "SaveRequisitionItemPartnerDetails",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     objRequisitionItem.P2PLineItemId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while Saving Item Partner deatils in requisition :" +
                                                      objRequisitionItem.P2PLineItemId.ToString(CultureInfo.InvariantCulture));
            }
        }

        public long SaveRequisitionItemShippingDetails(long reqLineItemShippingId, long requisitionItemId, string shippingMethod, int shiptoLocationId, int delivertoLocationId, decimal quantity, decimal totalQuantity, long userid, int precessionValue, int maxPrecessionforTotal, int maxPrecessionForTaxesAndCharges, bool prorateLineItemTax = true, string deliverTo = "")
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionDocumentManager().SaveItemShippingDetails(P2PDocumentType.Requisition, reqLineItemShippingId, requisitionItemId, shippingMethod, shiptoLocationId, delivertoLocationId, quantity, totalQuantity, userid, deliverTo, precessionValue, maxPrecessionforTotal, maxPrecessionForTaxesAndCharges, prorateLineItemTax);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("ReqLineItemShippingId: " + reqLineItemShippingId, "----RequisitionItemId: " + requisitionItemId, "----ShippingMethod: " + shippingMethod, "----ShiptoLocationId: " + shiptoLocationId, "----DelivertoLocationId: " + delivertoLocationId);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition SaveRequisitionItemShippingDetails MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "SaveRequisitionItemShippingDetails", "SaveRequisitionItemShippingDetails",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     requisitionItemId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while saving item shipping deatils for requisition :" +
                                                      requisitionItemId.ToString(CultureInfo.InvariantCulture));
            }
        }

        public long SaveRequisitionItemOtherDetails(RequisitionItem objRequisitionItem, bool allowTaxCodewithAmount, string supplierStatusForValidation)
        {
            string finalJsonObj = string.Empty;
            if (ReferenceEquals(null, objRequisitionItem))
                throw new ArgumentNullException("objRequisitionItem");
            try
            {
                return GetRequisitionDocumentManager().SaveItemOtherDetails(P2PDocumentType.Requisition, objRequisitionItem, allowTaxCodewithAmount, supplierStatusForValidation);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("DocumentItemId: " + objRequisitionItem != null ? objRequisitionItem.DocumentItemId : 0, "----AllowTaxCodewithAmount: " + allowTaxCodewithAmount, "----SupplierStatusForValidation: " + supplierStatusForValidation);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition SaveRequisitionItemOtherDetails MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "SaveRequisitionItemOtherDetails", "SaveRequisitionItemOtherDetails",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     objRequisitionItem.P2PLineItemId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while saving item other deatils for requisition :" +
                                                      objRequisitionItem.P2PLineItemId.ToString(CultureInfo.InvariantCulture));
            }
        }

        public long SaveReqItemWithAdditionalDetails(RequisitionItem objRequisitionItem, bool isTaxExempt, long LOBEntityDetailCode, bool allowPeriodUpdate = true, bool isFunctionalAdmin = false)
        {
            string finalJsonObj = string.Empty;
            if (ReferenceEquals(null, objRequisitionItem))
                throw new ArgumentNullException("objRequisitionItem");
            try
            {
                return GetRequisitionDocumentManager().SaveItemWithAdditionalDetails(P2PDocumentType.Requisition, objRequisitionItem, LOBEntityDetailCode, isTaxExempt, true, true, true, allowPeriodUpdate, isFunctionalAdmin);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("DocumentItemId: " + objRequisitionItem != null ? objRequisitionItem.DocumentItemId : 0, "----IsTaxExempt: " + isTaxExempt, "----IsFunctionalAdmin: " + isFunctionalAdmin);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition SaveReqItemWithAdditionalDetails MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "SaveReqItemWithAdditionalDetails", "SaveReqItemWithAdditionalDetails",
                                                    "Requsition", ExceptionType.ApplicationException,
                                                    objRequisitionItem.P2PLineItemId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while saving item Additional deatils for lineitemid :" +
                                                      objRequisitionItem.P2PLineItemId.ToString(CultureInfo.InvariantCulture));
            }
        }

        public void AddRequisitionInfoToPortal(Requisition objRequisition)
        {
            string finalJsonObj = string.Empty;
            try
            {
                GetRequisitionDocumentManager().AddInfoToPortal(P2PDocumentType.Requisition, objRequisition);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionId: " + objRequisition != null ? objRequisition.DocumentCode : 0);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition AddRequisitionInfoToPortal MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "AddRequisitionInfoToPortal", "AddRequisitionInfoToPortal",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     objRequisition.DocumentCode.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error occured while Add Requisition Info To Portal for requisitionId = " +
                                                      objRequisition.DocumentCode.ToString(CultureInfo.InvariantCulture));
            }
        }

        public bool UpdateRequisitionApprovalStatusById(long requisitionId, Documents.Entities.DocumentStatus approvalStatus)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionDocumentManager().UpdateApprovalStatusById(P2PDocumentType.Requisition, requisitionId, approvalStatus);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionId: " + requisitionId);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition UpdateRequisitionApprovalStatusById MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "UpdateRequisitionApprovalStatusById", "UpdateRequisitionApprovalStatusById",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     requisitionId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error occured while Update Requisition Approval Status By Id for requisitionId = " +
                                                      requisitionId.ToString(CultureInfo.InvariantCulture));
            }
        }

        public bool SaveRequisitionApproverDetails(long requisitionId, int approverId, Documents.Entities.DocumentStatus approvalStatus,
                                               string approveUri, string rejectUri, string instanceId)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionDocumentManager().SaveApproverDetails(P2PDocumentType.Requisition, requisitionId, approverId, approvalStatus, approveUri, rejectUri, instanceId);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionId: " + requisitionId, "----ApproverId: " + approverId, "----ApproveUri: " + approveUri, "----RejectUri: " + rejectUri, "----InstanceId: " + instanceId);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition SaveRequisitionApproverDetails MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "SaveRequisitionApproverDetails", "SaveRequisitionApproverDetails",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     requisitionId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error occured while Save Requisition Approver Details for requisitionId = " +
                                                      requisitionId.ToString(CultureInfo.InvariantCulture));
            }
        }

        public ICollection<KeyValuePair<decimal, string>> GetAllPartnersOfRequisitionById(long requisitionId, string documentIds = "", string buIds = "")
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionDocumentManager().GetAllPartnersById(P2PDocumentType.Requisition, requisitionId, documentIds, buIds);

            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionId: " + requisitionId, "----DocumentIds: " + documentIds, "----BUIds: " + buIds);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition GetAllPartnersOfRequisitionById MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "GetAllPartnersOfRequisitionById", "GetAllPartnersOfRequisitionById",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     requisitionId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while Getting Partner deatils :" +
                                                      requisitionId.ToString(CultureInfo.InvariantCulture));
            }
        }

        public ICollection<DocumentTrackStatusDetail> GetTrackDetailsofRequisitionById(long requisitionId)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionDocumentManager()
                        .GetTrackDetailsofDocumentById(P2PDocumentType.Requisition, requisitionId);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionId: " + requisitionId);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition GetTrackDetailsofRequisitionById MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "GetTrackDetailsofRequisitionById", "GetTrackDetailsofRequisitionById",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     requisitionId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while Getting Track Details for requisitionId :" +
                                                      requisitionId.ToString(CultureInfo.InvariantCulture));
            }
        }

        public ICollection<RequisitionItem> GetAllLineItemsByRequisitionId(long requisitionId, ItemType itemType, int pageIndex, int pageSize, int typeOfUser)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionDocumentManager()
                        .GetAllLineItemsByDocumentId(P2PDocumentType.Requisition, itemType, requisitionId, pageIndex, pageSize, typeOfUser).Cast<RequisitionItem>().ToList();
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionId: " + requisitionId, "----ItemType: " + itemType, "----TypeOfUser: " + typeOfUser);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition GetAllLineItemsByRequisitionId MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "GetAllLineItemsByRequisitionId", "GetAllLineItemsByRequisitionId",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     requisitionId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while Getting all line items for requisitionid :" +
                                                      requisitionId.ToString(CultureInfo.InvariantCulture));
            }
        }

        public bool DeleteLineItemByIds(string lineItemIds, int precessionValue, int maxPrecessionValueforTotal, int maxPrecessionValueForTaxesAndCharges, bool isAdvance)
        {
            string finalJsonObj = string.Empty;
            if (ReferenceEquals(null, lineItemIds))
                throw new ArgumentNullException("lineItemIds");
            try
            {
                return GetRequisitionDocumentManager()
                        .DeleteLineItemByIds(P2PDocumentType.Requisition, lineItemIds, precessionValue, maxPrecessionValueforTotal, maxPrecessionValueForTaxesAndCharges, isAdvance);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("LineItemIds: " + lineItemIds, "----IsAdvance: " + isAdvance);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition DeleteLineItemByIds MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "DeleteLineItemByIds", "DeleteLineItemByIds",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     lineItemIds.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while deleting line items for lineItemIds :" +
                                                      lineItemIds.ToString(CultureInfo.InvariantCulture));

            }
        }
        public bool UpdateItemQuantity(long lineItemId, decimal quantity, int itemSource, int banding, decimal maxOrderQuantity, decimal minOrderQuantity)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionDocumentManager()
                       .UpdateItemQuantity(P2PDocumentType.Requisition, lineItemId, quantity, itemSource, banding, maxOrderQuantity, minOrderQuantity);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("LineItemId: " + lineItemId, "----Quantity: " + quantity);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition UpdateItemQuantity MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "UpdateItemQuantity", "UpdateItemQuantity",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     lineItemId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while Updating quantity for lineItemId :" +
                                                      lineItemId.ToString(CultureInfo.InvariantCulture));

            }
        }

        public bool SaveRequisitionTrackStatusDetails(long requisitionId, string instanceId, long approverId,
                                                      string approverName, string approverType, string approveUri,
                                                      string rejectUri, DateTime statusDate,
                                                      RequisitionTrackStatus approvalStatus, bool isDeleted)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionDocumentManager().SaveTrackStatusDetails(P2PDocumentType.Requisition, requisitionId, instanceId, approverId, approverName, approverType, approveUri,
                                                 rejectUri, statusDate, approvalStatus, isDeleted);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionId: " + requisitionId, "----ApproverId: " + approverId, "----ApproveUri: " + approveUri, "----RejectUri: " + rejectUri, "----InstanceId: " + instanceId);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition SaveRequisitionTrackStatusDetails MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "SaveRequisitionTrackStatusDetails", "SaveRequisitionTrackStatusDetails",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     requisitionId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error occured while Save Requisition Approver Details for requisitionId = " +
                                                      requisitionId.ToString(CultureInfo.InvariantCulture));
            }

        }

        public RequisitionItem GetPartnerDetailsByLiId(long lineItemId)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return (RequisitionItem)GetRequisitionDocumentManager()
                    .GetPartnerDetailsByLiId(P2PDocumentType.Requisition, lineItemId);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("LineItemId: " + lineItemId);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition GetPartnerDetailsByLiId MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "GetPartnerDetailsByLiId", "GetPartnerDetailsByLiId",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     lineItemId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while Getting Item Partner deatils by Liid :" +
                                                      lineItemId.ToString(CultureInfo.InvariantCulture));
            }
        }

        public ICollection<DocumentItemShippingDetail> GetShippingSplitDetailsByLiId(long lineItemId)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionDocumentManager()
                    .GetShippingSplitDetailsByLiId(P2PDocumentType.Requisition, lineItemId).Cast<DocumentItemShippingDetail>().ToList();
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("LineItemId: " + lineItemId);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition GetShippingSplitDetailsByLiId MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "GetShippingSplitDetailsByLiId", "GetShippingSplitDetailsByLiId",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     lineItemId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while Getting Shipping deatils by liid for requisition :" +
                                                      lineItemId.ToString(CultureInfo.InvariantCulture));
            }
        }

        public RequisitionItem GetOtherItemDetailsByLiId(long lineItemId)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return (RequisitionItem)GetRequisitionDocumentManager()
                    .GetOtherItemDetailsByLiId(P2PDocumentType.Requisition, lineItemId);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("LineItemId: " + lineItemId);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition GetOtherItemDetailsByLiId MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "GetOtherItemDetailsByLiId", "GetOtherItemDetailsByLiId",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     lineItemId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while Getting Item Other deatils by Liid :" +
                                                      lineItemId.ToString(CultureInfo.InvariantCulture));
            }
        }

        public long SaveCatalogRequisition(long userId, long reqId, string requisitionName, string requisitionNumber, long oboId = 0)
        {
            string finalJsonObj = string.Empty;
            long result = 0;
            try
            {
                result = GetRequisitionManager().SaveCatalogRequisition(userId, reqId, requisitionName, requisitionNumber, oboId, true);
            }
            catch (CommunicationException commFaultEx)
            {
                LogHelper.LogError(Log, "Error occured in SaveCatalogRequisition method", commFaultEx);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("UserId: " + userId, "----ReqId: " + reqId, "----RequisitionName: " + requisitionName, "----RequisitionNumber: " + requisitionNumber);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition SaveCatalogRequisition MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

            }
            return result;
        }


        //public long GetDocumentCodeByRequisitionId(long requisitionId)
        //{
        //    long documentCode = 0;
        //    try
        //    {
        //        documentCode = ReqServiceBase.GetRequisitionDocumentManager()
        //            .GetDocumentCodeByDocumentId(P2PDocumentType.Requisition, requisitionId);
        //    }
        //    catch (CommunicationException commFaultEx)
        //    {
        //        LogHelper.LogError(Log, "Error occured in GetDocumentCodeByRequisitionId method", commFaultEx);
        //    }
        //    catch (Exception ex)
        //    {

        //        LogHelper.LogError(Log, "Error occured in GetDocumentCodeByRequisitionId method", ex);
        //    }

        //    return documentCode;
        //}

        public ICollection<string> ValidateDocumentByDocumentCode(long documentCode, long LOBEntityDetailCode)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionDocumentManager()
                    .ValidateDocumentByDocumentCode(P2PDocumentType.Requisition, documentCode, LOBEntityDetailCode);
            }
            catch (Exception ex)
            {

                finalJsonObj = string.Concat("DocumentCode: " + documentCode);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition ValidateDocumentByDocumentCode MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "ValidateDocumentByDocumentCode", "ValidateDocumentByDocumentCode",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     documentCode.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while Getting all line items for documentCode :" +
                                                      documentCode.ToString(CultureInfo.InvariantCulture));
            }

        }

        public bool SaveRequisitionBusinessUnit(long documentCode, long buId)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionManager().SaveRequisitionBusinessUnit(documentCode, buId);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("DocumentCode: " + documentCode, "----BuId: " + buId);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition SaveRequisitionBusinessUnit MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "SaveRequisitionBusinessUnit", "SaveRequisitionBusinessUnit",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     documentCode.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while Saving Business Unit for documentCode :" +
                                                      documentCode.ToString(CultureInfo.InvariantCulture));
            }
        }

        public bool SaveDocumentBusinessUnit(long documentCode)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionManager().SaveDocumentBusinessUnit(documentCode);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("DocumentCode: " + documentCode);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition SaveDocumentBusinessUnit MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "SaveDocumentBusinessUnit", "SaveDocumentBusinessUnit",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     documentCode.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while Saving Document Business Unit for documentCode :" +
                                                      documentCode.ToString(CultureInfo.InvariantCulture));
            }
        }

        public ICollection<P2PDocument> GetAllRequisitionsForLeftPanel(long requisitionId, long userId, int pageIndex, int pageSize, string currencyCode, long orgEntityDetailCode, int purchaseTypeId)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionDocumentManager().GetAllDocumentForLeftPanel(P2PDocumentType.Requisition, 0, requisitionId, userId, pageIndex, pageSize, currencyCode, orgEntityDetailCode, purchaseTypeId);
            }
            catch (Exception ex)
            {

                finalJsonObj = string.Concat("RequisitionId: " + requisitionId, "----UserId: " + userId, "----OrgEntityDetailCode: " + orgEntityDetailCode);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition GetAllRequisitionsForLeftPanel MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "GetAllRequisitionsForLeftPanel", "GetAllRequisitionsForLeftPanel",
                                                     "Requisition", ExceptionType.ApplicationException,
                                                     requisitionId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while Getting All Requisition for left panel for partnerCode:" +
                                                      requisitionId.ToString(CultureInfo.InvariantCulture));
            }
        }

        public long UpdateProcurementStatusByReqItemId(long requisitionItemId)
        {
            string finalJsonObj = string.Empty;
            long documentStatus = 0;
            try
            {
                documentStatus = GetRequisitionManager()
                    .UpdateProcurementStatusByReqItemId(requisitionItemId);
            }
            catch (CommunicationException commFaultEx)
            {
                LogHelper.LogError(Log, "Error occured in UpdateProcurementStatusByReqItemId method", commFaultEx);
            }
            catch (Exception ex)
            {

                finalJsonObj = string.Concat("RequisitionItemId: " + requisitionItemId);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition UpdateProcurementStatusByReqItemId MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
            }

            return documentStatus;
        }

        public long SaveRequisitionfromInterface(long buyerPartnerCode, int partnerInterfaceId, BZRequisition objRequisition)
        {
            try
            {
                SetUserContext(buyerPartnerCode);
                //long documentId = ReqServiceBase.GetRequisitionDocumentManager().SaveP2PDocumentfromInterface(P2PDocumentType.Requisition, objRequisition.Requisition, partnerInterfaceId);
                long documentId = GetRequisitionInterfaceManager().SaveP2PDocumentfromInterface(P2PDocumentType.Requisition, objRequisition.Requisition, partnerInterfaceId);

                if (documentId <= 0)
                    throw new Exception("ApplicationException : Unable to save requisition");
                else
                    return documentId;

            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in SaveRequisitionfromInterface Method for requisition name : " + objRequisition.Requisition.DocumentName, ex);

                string exMsg = "Error while saving requisition from interface : " + objRequisition.Requisition != null ? objRequisition.Requisition.DocumentName : string.Empty + ". Error details: " + ex.Message;

                if (ex.Message.Contains("Validation"))
                    exMsg = "{ValidationException}" + ex.Message + "{/ValidationException}";
                else
                    exMsg = "{ApplicationException}" + exMsg + "{/ApplicationException}";

                var objCustomFault = new CustomFault(ex.Message, "SaveRequisitionfromInterface", "SaveRequisitionfromInterface",
                                                    "Requisition", ExceptionType.ApplicationException,
                                                   (objRequisition.Requisition != null ? objRequisition.Requisition.DocumentName : string.Empty), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while Saving Requisition from Interface");
            }
        }

        public bool AddTemplateItemInReq(long documentCode, string templateIds, List<KeyValuePair<long, decimal>> items, int itemType, string buIds)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionManager().AddTemplateItemInReq(documentCode, templateIds, items, itemType, buIds);
            }
            catch (CommunicationException commFaultEx)
            {
                LogHelper.LogError(Log, "Error occured in AddTemplateItemInReq method", commFaultEx);
                var objCustomFault = new CustomFault(commFaultEx.Message, "AddTemplateItemInReq", "AddTemplateItemInReq",
                                                    "Requisition", ExceptionType.ApplicationException,
                                                    documentCode.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while Template items in Requisition.");
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("DocumentCode: " + documentCode, "----TemplateIds: " + templateIds, "----ItemType: " + itemType, "----BuIds: " + buIds);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition AddTemplateItemInReq MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "AddTemplateItemInReq", "AddTemplateItemInReq",
                                                    "Requisition", ExceptionType.ApplicationException,
                                                   documentCode.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while Template items in Requisition.");
            }
        }

        public List<SplitAccountingFields> GetAllSplitAccountingFields(P2PDocumentType docType, LevelType levelType, int structureId = 0, long LOBId = 0, long ACEEntityDetailCode = 0)
        {
            try
            {
                return GetBO<P2PDocumentManager>().GetAllSplitAccountingFields(docType, levelType, structureId, LOBId, ACEEntityDetailCode);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetAllSplitAccountingFields method", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetAllSplitAccountingFields", "GetAllSplitAccountingFields",
                                                    "Requisition", ExceptionType.ApplicationException, docType.ToString(CultureInfo.InstalledUICulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while GetAllSplitAccountingFields in Requisition.");
            }
        }

        public List<RequisitionSplitItems> GetRequisitionAccountingDetailsByItemId(long requisitionItemId, int pageIndex, int pageSize, int itemType, long LOBId)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionManager().GetRequisitionAccountingDetailsByItemId(requisitionItemId, pageIndex,
                                                                                               pageSize, itemType, LOBId);

            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionItemId: " + requisitionItemId, "----LOBId: " + LOBId, "----ItemType: " + itemType);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition GetRequisitionAccountingDetailsByItemId MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "GetRequisitionAccountingDetailsByItemId", "GetRequisitionAccountingDetailsByItemId",
                                                    "Requisition", ExceptionType.ApplicationException, requisitionItemId.ToString(CultureInfo.InstalledUICulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while GetRequisitionAccountingDetailsByItemId in Requisition.");
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public long SaveRequisitionAccountingDetails(List<RequisitionSplitItems> requisitionSplitItems, List<DocumentSplitItemEntity> requisitionSplitItemEntities, decimal lineItemQuantity, bool updateTaxes, long LOBId)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionManager().SaveRequisitionAccountingDetails(requisitionSplitItems, requisitionSplitItemEntities, lineItemQuantity, updateTaxes, LOBId);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("LOBId: " + LOBId, "----LineItemQuantity: " + lineItemQuantity, "----UpdateTaxes: " + updateTaxes);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition SaveRequisitionAccountingDetails MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "SaveRequisitionAccountingDetails", "SaveRequisitionAccountingDetails",
                                                    "Requisition", ExceptionType.ApplicationException, requisitionSplitItems[0].DocumentItemId.ToString(CultureInfo.InstalledUICulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while SaveRequisitionAccountingDetails in Requisition.");
            }
        }

        public bool CheckAccountingSplitValidations(long requisitionId)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionManager().CheckAccountingSplitValidations(requisitionId);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionId: " + requisitionId);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition CheckAccountingSplitValidations MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "CheckAccountingSplitValidations", "CheckAccountingSplitValidations",
                                                    "Requisition", ExceptionType.ApplicationException, requisitionId.ToString(CultureInfo.InstalledUICulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while CheckAccountingSplitValidations in Requisition.");
            }
        }

        public bool DeleteRequisitionByDocumentCode(long documentCode)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionDocumentManager().DeleteDocumentByDocumentCode(P2PDocumentType.Requisition, documentCode);
            }
            catch (CommunicationException commFaultEx)
            {
                LogHelper.LogError(Log, "Error occured in DeleteRequisitionByDocumentCode method", commFaultEx);
                var objCustomFault = new CustomFault(commFaultEx.Message, "DeleteRequisitionByDocumentCode", "DeleteRequisitionByDocumentCode",
                                                    "Requisition", ExceptionType.ApplicationException,
                                                    documentCode.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while deleting Requisition.");
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("DocumentCode: " + documentCode);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition DeleteRequisitionByDocumentCode MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "DeleteRequisitionByDocumentCode", "DeleteRequisitionByDocumentCode",
                                                    "Requisition", ExceptionType.ApplicationException,
                                                   documentCode.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while deleting Requisition.");
            }
        }

        public bool UpdateLineStatusForRequisition(long RequisitionId,
                                                    StockReservationStatus LineStatus,
                                                    bool IsUpdateAllItems,
                                                    List<P2P.BusinessEntities.LineStatusRequisition> Items)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetNewRequisitionManager().UpdateLineStatusForRequisition(RequisitionId, LineStatus, IsUpdateAllItems, Items);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("DocumentCode: " + RequisitionId);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition UpdateLineStatusForRequisition MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "UpdateLineStatusForRequisition", "UpdateLineStatusForRequisition",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     RequisitionId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while Auto creating order" +
                                                      RequisitionId.ToString(CultureInfo.InvariantCulture));
            }
        }

        public void UpdateLineStatusForRequisitionFromInterface(List<RequisitionLineStatusUpdateDetails> reqDetails)
        {
            string finalJsonObj = string.Empty;
            try
            {
                GetRequisitionInterfaceManager().UpdateLineStatusForRequisitionFromInterface(reqDetails);
            }
            catch (Exception ex)
            {
                //finalJsonObj = string.Concat("DocumentCode: " + RequisitionId);
                LogHelper.LogError(Log, "Error occured in Requisition UpdateLineStatusForRequisitionFromInterface InnerExceptionMessage = {0}", ex);
                var objCustomFault = new CustomFault(ex.Message, "UpdateLineStatusForRequisitionFromInterface", "UpdateLineStatusForRequisitionFromInterface",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     "", false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while UpdateLineStatusForRequisitionFromInterface " +
                                                      "");
            }
        }


        public List<long> AutoCreateOrder(long documentCode)
        {
            string finalJsonObj = string.Empty;
            try
            {
                var context = this.GetCallerContext();
                var helper = new RequisitionServiceHelper(context);
                this.JWTToken = helper.GetToken();

                var executionHelper = new BusinessObjects.RESTAPIHelper.ExecutionHelper(context, this.Config, this.JWTToken);
                if (executionHelper.Check(16, BusinessObjects.RESTAPIHelper.ExecutionHelper.WebAPIType.Order))
                {
                    var orderHelper = new BusinessObjects.RESTAPIHelper.OrderHelper(context, JWTToken);
                    return orderHelper.GetSettingsAndAutoCreateOrder(documentCode);
                }
                else
                {
                    Proxy.ProxyOrderService proxyOrderService = new Proxy.ProxyOrderService(this.GetCallerContext(), this.JWTToken);
                    return proxyOrderService.GetSettingsAndAutoCreateOrder(documentCode);
                }          
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("DocumentCode: " + documentCode);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition AutoCreateOrder MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "AutoCreateOrder", "AutoCreateOrder",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     documentCode.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while Auto creating order" +
                                                      documentCode.ToString(CultureInfo.InvariantCulture));
            }
        }

        public long CopyRequisitionToRequisition(long newrequisitionId, string requisitionIds, string buIds, long LOBId)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionManager().CopyRequisitionToRequisition(newrequisitionId, requisitionIds, buIds, LOBId);

            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("NewrequisitionId: " + newrequisitionId, "----RequisitionIds: " + requisitionIds, "----BuIds: " + buIds, "----LOBId: " + LOBId);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition CopyRequisitionToRequisition MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "CopyRequisitionToRequisition", "CopyRequisitionToRequisition",
                                                    "Requisition", ExceptionType.ApplicationException, newrequisitionId.ToString(CultureInfo.InstalledUICulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while CopyRequisitionToRequisition in Requisition.");
            }
        }


        public string GetDocumentDetailsByDocumentCode(long documentCode)
        {
            string finalJsonObj = string.Empty;
            try
            {
                finalJsonObj = JSONHelper.ToJSON(GetRequisitionManager().GetDocumentDetailsByDocumentCode(documentCode));
                GetRequisitionDocumentManager().SyncChangeRequisition(documentCode);
                return finalJsonObj;

            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("DocumentCode: " + documentCode);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition GetDocumentDetailsByDocumentCode MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "GetDocumentIntegrationDetailsByDocumentCode", "GetDocumentIntegrationDetailsByDocumentCode",
                                                    "documentCode", ExceptionType.ApplicationException, documentCode.ToString(CultureInfo.InstalledUICulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while GetDocumentIntegrationDetailsByDocumentCode in Requisition.");
            }
        }


        private void SetUserContext(long buyerPartnerCode)
        {
            var _userExecutionContext = new UserExecutionContext
            {
                ClientName = Gep.Cumulus.CSM.Entities.CommonConstants.BUYERSQLCONN,
                Product = GEPSuite.eInterface,
                EntityType = "Basic Setting",
                EntityId = 8888,
                LoggerCode = "EP101",
                Culture = "en-US",
                CompanyName = Gep.Cumulus.CSM.Entities.CommonConstants.BUYERSQLCONN,
                BuyerPartnerCode = buyerPartnerCode
            };

            var objMessageHeader = new MessageHeader<UserExecutionContext>(_userExecutionContext);
            MessageHeader messageHeader = objMessageHeader.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
            OperationContext.Current.IncomingMessageHeaders.Add(messageHeader);

        }
        public List<SplitAccountingFields> GetAllSplitAccountingFieldsWithDefaultValues(P2PDocumentType docType, LevelType levelType, long documentCode, long OnBehalfOfId, long documentItemId = 0, long lOBEntityDetailCode = 0, List<DocumentAdditionalEntityInfo> lstHeaderEntityDetails = null, PreferenceLOBType preferenceLOBType = PreferenceLOBType.Serve)
        {
            string finalJsonObj = string.Empty;
            try
            {
                //if(docType != P2PDocumentType.Order)
                return GetBO<P2PDocumentManager>().GetAllAccountingFieldsWithDefaultValues(docType, levelType, OnBehalfOfId, documentCode, lstHeaderEntityDetails, null, false, documentItemId, lOBEntityDetailCode, preferenceLOBType);
                //else
                //    return ReqServiceBase.GetRequisitionDocumentManager().GetAllSplitAccountingFieldsWithDefaultValues(docType, levelType, OnBehalfOfId, documentCode);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("DocumentCode: " + documentCode, "----OnBehalfOfId: " + OnBehalfOfId, "----DocumentItemId: " + documentItemId, "----lOBEntityDetailCode: " + lOBEntityDetailCode);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition GetAllSplitAccountingFieldsWithDefaultValues MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "GetAllSplitAccountingFieldsWithDefaultValues", "GetAllSplitAccountingFieldsWithDefaultValues",
                                                    "Requisition", ExceptionType.ApplicationException, docType.ToString(CultureInfo.InstalledUICulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while GetAllSplitAccountingFieldsWithDefaultValues in Requisition.");
            }
        }

        public List<ADRSplit> GetAllSplitAccountingFieldsWithDefaultValuesForADR(P2PDocumentType docType, LevelType levelType, long documentCode, long OnBehalfOfId, List<long> documentItemId = null, long lOBEntityDetailCode = 0, List<DocumentAdditionalEntityInfo> lstHeaderEntityDetails = null, PreferenceLOBType preferenceLOBType = PreferenceLOBType.Serve, ADRIdentifier identifier = ADRIdentifier.None, object document = null)
        {
            string finalJsonObj = string.Empty;
            try
            {
                //if(docType != P2PDocumentType.Order)
                return GetBO<P2PDocumentManager>().GetAllAccountingFieldsWithDefaultValues(docType, levelType, OnBehalfOfId, documentCode, lstHeaderEntityDetails, null, false, documentItemId, lOBEntityDetailCode, preferenceLOBType, identifier, document);
                //else
                //    return ReqServiceBase.GetRequisitionDocumentManager().GetAllSplitAccountingFieldsWithDefaultValues(docType, levelType, OnBehalfOfId, documentCode);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("DocumentCode: " + documentCode, "----OnBehalfOfId: " + OnBehalfOfId, "----DocumentItemId: " + documentItemId, "----lOBEntityDetailCode: " + lOBEntityDetailCode);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition GetAllSplitAccountingFieldsWithDefaultValues MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "GetAllSplitAccountingFieldsWithDefaultValues", "GetAllSplitAccountingFieldsWithDefaultValues",
                                                    "Requisition", ExceptionType.ApplicationException, docType.ToString(CultureInfo.InstalledUICulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while GetAllSplitAccountingFieldsWithDefaultValues in Requisition.");
            }
        }

        public void SendNotificationForApprovedRequisition(long requisition, string queryString)
        {
            string finalJsonObj = string.Empty;
            try
            {
                GetRequisitionEmailNotificationManager().SendNotificationForApprovedRequisition(requisition);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionId: " + requisition);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition GetAllSplitAccountingFieldsWithDefaultValues MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "GetAllSplitAccountingFieldsWithDefaultValues", "GetAllSplitAccountingFieldsWithDefaultValues",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     requisition.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error occured while Get All SplitAccounting Fields With Default Values for requisitionId = " +
                                                      requisition.ToString(CultureInfo.InvariantCulture));
            }
        }

        public void SendNotificationForRejectedRequisition(long requisition, ApproverDetails rejector, List<ApproverDetails> prevApprovers, string queryString)
        {
            string finalJsonObj = string.Empty;
            try
            {
                GetRequisitionEmailNotificationManager().SendNotificationForRejectedRequisition(requisition, rejector, prevApprovers, queryString);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionId: " + requisition);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition SendNotificationForRejectedRequisition MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "SendNotificationForRejectedRequisition", "SendNotificationForRejectedRequisition",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     requisition.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error occured while Send Notification For Rejected Requisition for requisitionId = " +
                                                      requisition.ToString(CultureInfo.InvariantCulture));
            }
        }

        public bool UpdateSendforBiddingDocumentStatus(long documentCode)
        {
            string finalJsonObj = string.Empty;
            try
            {
                var result = GetRequisitionManager().UpdateSendforBiddingDocumentStatus(documentCode);
                var updateStatus = GetNewRequisitionManager().UpdateLineStatusForRequisition(documentCode, (StockReservationStatus)(DocumentStatus.SentForBidding), true, null);
                return result;
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("DocumentCode: " + documentCode);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition UpdateSendforBiddingDocumentStatus MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "UpdateSendforBiddingDocumentStatus", "UpdateSendforBiddingDocumentStatus",
                                                    "Requisition", ExceptionType.ApplicationException, documentCode.ToString(CultureInfo.InstalledUICulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while UpdateSendforBiddingDocumentStatus in Requisition.");
            }
        }

        public List<long> GetAllCategoriesByReqId(long documentId)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionManager().GetAllCategoriesByReqId(documentId);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("DocumentId: " + documentId);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition GetAllCategoriesByReqId MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "GetAllCategoriesByReqId", "GetAllCategoriesByReqId",
                                                    "Requisition", ExceptionType.ApplicationException, documentId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while GetAllCategoriesByReqId in Requisition.");
            }
        }

        public KeyValuePair<long, decimal> GetAllEntitiesByReqId(long documentId, int entityTypeId)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionManager().GetAllEntitiesByReqId(documentId, entityTypeId);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("DocumentId: " + documentId, "----EntityTypeId: " + entityTypeId);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition GetAllEntitiesByReqId MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "GetAllEntitiesByReqId", "GetAllEntitiesByReqId",
                                                    "Requisition", ExceptionType.ApplicationException, documentId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while GetAllEntitiesByReqId in Requisition.");
            }
        }
        public bool DeleteAllSplitsByDocumentId(long documentId, long ContactCode, long LOBId)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionManager().DeleteAllSplitsByDocumentId(documentId, ContactCode, LOBId);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("DocumentId: " + documentId, "----LOBId: " + LOBId, "----ContactCode: " + ContactCode);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition DeleteAllSplitsByDocumentId MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "DeleteAllSplitsByDocumentId", "GetAllEntitiesByReqId",
                                                    "Requisition", ExceptionType.ApplicationException, documentId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while DeleteAllSplitsByDocumentId in Requisition.");
            }
        }

        public void SendNotificationForRequisitionApproval(long documentCode, List<ApproverDetails> lstPendingApprover, List<ApproverDetails> lstPastApprover, string eventName, DocumentStatus documentStatus, string approvalType)
        {
            string finalJsonObj = string.Empty;
            try
            {
                GetRequisitionEmailNotificationManager().SendNotificationForRequisitionApproval(documentCode, lstPendingApprover, lstPastApprover, eventName, documentStatus, approvalType);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("DocumentCode: " + documentCode);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition SendNotificationForRequisitionApproval MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "SendNotificationForRequisitionApproval", "SendNotificationForRequisitionApproval",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     documentCode.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error occured while Send Notification For Requisition Approval for DocumentCode = " +
                                                      documentCode.ToString(CultureInfo.InvariantCulture));
            }
        }

        public void SendNotificationForSkipApproval(long documentCode, List<ApproverDetails> lstSkippedApprovers, List<ApproverDetails> lstFinalApprovers)
        {
            string finalJsonObj = string.Empty;
            try
            {
                GetRequisitionEmailNotificationManager().SendNotificationForSkipApproval(documentCode, lstSkippedApprovers, lstFinalApprovers);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("DocumentCode: " + documentCode);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition SendNotificationForSkipApproval MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "SendNotificationForSkipApproval", "SendNotificationForSkipApproval",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     documentCode.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error occured while Send Notification For Requisition Approval for DocumentCode = " +
                                                      documentCode.ToString(CultureInfo.InvariantCulture));
            }
        }

        public long SaveRequisitionAccountingApplyToAll(long requisitionId, List<DocumentSplitItemEntity> requisitionSplitItemEntities)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionDocumentManager().SaveAccountingApplyToAll(P2PDocumentType.Requisition, requisitionId, requisitionSplitItemEntities);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionId: " + requisitionId);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition SaveRequisitionAccountingApplyToAll MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "SaveRequisitionAccountingApplyToAll", "SaveRequisitionAccountingApplyToAll",
                                                    "Requisition", ExceptionType.ApplicationException, requisitionId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while SaveRequisitionAccountingApplyToAll in Requisition.");
            }
        }

        public bool copyLineItem(long requisitionItemId, long requisitionId, int txtNumberOfCopies, long LOBId)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionManager().copyLineItem(requisitionItemId, requisitionId, txtNumberOfCopies, LOBId);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionItemId: " + requisitionItemId, "----RequisitionId: " + requisitionId, "----LOBId: " + LOBId);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition copyLineItem MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "copyLineItem", "copyLineItem",
                                                    "Requisition", ExceptionType.ApplicationException, requisitionId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while copyLineItem in Requisition.");
            }
        }

        public bool UpdateReqItemOnPartnerChange(long requisitionItemId, long partnerCode, long LOBId)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionManager().UpdateReqItemOnPartnerChange(requisitionItemId, partnerCode, LOBId);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionItemId: " + requisitionItemId, "----PartnerCode: " + partnerCode, "----LOBId: " + LOBId);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition UpdateReqItemOnPartnerChange MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "UpdateReqItemOnPartnerChange", "UpdateReqItemOnPartnerChange",
                                                    "RequisitionItemId", ExceptionType.ApplicationException, requisitionItemId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while UpdateReqItemOnPartnerChange in Requisition.");
            }
        }

        public bool GetRequisitionItemAccountingStatus(long requisitionItemId)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionManager().GetRequisitionItemAccountingStatus(requisitionItemId);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionItemId: " + requisitionItemId);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition GetRequisitionItemAccountingStatus MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "GetRequisitionItemAccountingStatus", "GetRequisitionItemAccountingStatus",
                                                    "Requisition", ExceptionType.ApplicationException, requisitionItemId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while GetRequisitionItemAccountingStatus in Requisition.");
            }
        }

        public List<PartnerInfo> GetPreferredPartnerByReqItemId(long DocumentItemId, int pageIndex, int pageSize, string partnerName, out long partnerCode)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionManager().GetPreferredPartnerByReqItemId(DocumentItemId, pageIndex, pageSize, partnerName, out partnerCode);

            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("DocumentItemId: " + DocumentItemId, "----pageIndex: " + pageIndex, "----pageSize: " + pageSize, "----partnerName: " + partnerName);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition GetPreferredPartnerByReqItemId MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "GetPreferredPartnerByReqItemId", "GetPreferredPartnerByReqItemId",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     DocumentItemId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while Getting Partner deatils :" +
                                                      DocumentItemId.ToString(CultureInfo.InvariantCulture));
            }
        }


        public List<ItemPartnerInfo> GetPreferredPartnerByCatalogItemId(long DocumentItemId, int pageIndex, int pageSize, string partnerName, string currencyCode, long entityDetailCode, out long partnerCode, string buList = "")
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionManager().GetPreferredPartnerByCatalogItemId(DocumentItemId, pageIndex, pageSize, partnerName, currencyCode, entityDetailCode, out partnerCode, buList);

            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("DocumentItemId: " + DocumentItemId, "----pageIndex: " + pageIndex, "----pageSize: " + pageSize, "----partnerName: " + partnerName, "----currencyCode: " + currencyCode, "----entityDetailCode: " + entityDetailCode);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition GetPreferredPartnerByCatalogItemId MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "GetPreferredPartnerByCatalogItemId", "GetPreferredPartnerByCatalogItemId",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     DocumentItemId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while Getting Partner deatils :" +
                                                      DocumentItemId.ToString(CultureInfo.InvariantCulture));
            }
        }



        public bool FinalizeComments(long documentCode, bool isIndexingRequired = true)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionDocumentManager().FinalizeComments(P2PDocumentType.Requisition, documentCode, isIndexingRequired);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("DocumentCode: " + documentCode);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition FinalizeComments MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "FinalizeComments", "FinalizeComments",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     documentCode.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error occured while Finalize Comments for DocumentCode = " +
                                                      documentCode.ToString(CultureInfo.InvariantCulture));
            }
        }
        public bool CheckRequisitionCatalogItemAccess(long newrequisitionId, string requisitionIds)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionManager().CheckRequisitionCatalogItemAccess(newrequisitionId, requisitionIds);

            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("NewrequisitionId: " + newrequisitionId, "----RequisitionIds: " + requisitionIds);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition CheckRequisitionCatalogItemAccess MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "CheckRequisitionCatalogItemAccess", "CheckRequisitionCatalogItemAccess",
                                                    "Requisition", ExceptionType.ApplicationException, newrequisitionId.ToString(CultureInfo.InstalledUICulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while CheckRequisitionCatalogItemAccess in Requisition.");
            }
        }

        public bool CheckOBOUserCatalogItemAccess(long requisitionId, long requesterId, bool delItems = false)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionManager().CheckOBOUserCatalogItemAccess(requisitionId, requesterId, delItems);

            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionId: " + requisitionId, "----RequesterId: " + requesterId, "----delItems: " + delItems);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition CheckOBOUserCatalogItemAccess MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "CheckOBOUserCatalogItemAccess", "CheckOBOUserCatalogItemAccess",
                                                    "Requisition", ExceptionType.ApplicationException, requisitionId.ToString(CultureInfo.InstalledUICulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while CheckOBOUserCatalogItemAccess in Requisition.");
            }
        }

        public bool UpdateTaxOnLineItem(long requisitionItemId, ICollection<Taxes> lstTaxes, long LOBEntityDetailCode)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionManager().UpdateTaxOnLineItem(requisitionItemId, lstTaxes, LOBEntityDetailCode);

            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionItemId: " + requisitionItemId);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition UpdateTaxOnLineItem MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "UpdateTaxOnLineItem", "UpdateTaxOnLineItem",
                                                    "RequisitionItemId", ExceptionType.ApplicationException, requisitionItemId.ToString(CultureInfo.InstalledUICulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while UpdateTaxOnLineItem in Requisition.");
            }
        }

        /// <summary>
        /// Delete requisition items on BU change
        /// </summary>
        /// <param name="requisitionId">requisitionId</param>
        /// <param name="buList"></param>
        /// <param name="LOBId"></param>
        /// <returns>bool</returns>
        public bool DeleteRequisitionItemsOnBUChange(long requisitionId, string buList, long LOBId)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionDocumentManager().DeleteLineItemsBasedOnBUChange(P2PDocumentType.Requisition, requisitionId, buList, LOBId);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionId: " + requisitionId, "----BuList: " + buList);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition DeleteRequisitionItemsOnBUChange MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "DeleteRequisitionItemsOnBUChange", "DeleteRequisitionItemsOnBUChange",
                                                    "RequisitionId", ExceptionType.ApplicationException, requisitionId.ToString(CultureInfo.InstalledUICulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while DeleteRequisitionItemsOnBUChange in Requisition.");
            }
        }

        public Requisition GetAllRequisitionDetailsByRequisitionId(long requisitionId)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionDocumentManager().GetAllRequisitionDetailsByRequisitionId(requisitionId);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionId: " + requisitionId);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition GetAllRequisitionDetailsByRequisitionId MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                UtilsManager.ThrowHttpException(ex.Message);

                var objCustomFault = new CustomFault(ex.Message, "GetRequisitionBasicDetailsById", "GetRequisitionBasicDetailsById",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     requisitionId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error occured while getting Requisition Basic details for requisitionId = " +
                                                      requisitionId.ToString(CultureInfo.InvariantCulture));
            }
        }

        public bool GetRequisitionCapitalCodeCountById(long requisitionId)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionManager().GetRequisitionCapitalCodeCountById(requisitionId);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionId: " + requisitionId);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition GetRequisitionCapitalCodeCountById MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "GetRequisitionItemAccountingStatus", "GetRequisitionItemAccountingStatus",
                                                    "Requisition", ExceptionType.ApplicationException, requisitionId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while GetRequisitionItemAccountingStatus in Requisition.");
            }
        }

        public Dictionary<string, string> SentRequisitionForApproval(long contactCode, long documentCode, decimal documentAmount, int documentTypeId, string fromCurrency, string toCurrency, bool isOperationalBudgetEnabled, long headerOrgEntityCode, bool isBypassOperationalBudget = false)
        {
            string finalJsonObj = string.Empty;
            Dictionary<string, string> result;
            try
            {
                DocumentStatus documentStatus = GetRequisitionDocumentManager().GetDocumentStatus(documentCode);
                if (documentStatus == DocumentStatus.ApprovalPending || documentStatus == DocumentStatus.Ordered || documentStatus == DocumentStatus.PartiallyOrdered || documentStatus == DocumentStatus.SentForBidding || documentStatus == DocumentStatus.PartiallySourced || documentStatus == DocumentStatus.Approved || documentStatus == DocumentStatus.Accepted)
                {
                    result = new Dictionary<string, string>();
                    result.Add("SendForApprovalResult", "P2P_Req_Submit_Error");
                    return result;
                }

                var manager = GetRequisitionDocumentManager();
                var currentPrincipal = System.Threading.Thread.CurrentPrincipal;
                Task.Factory.StartNew((scope) =>
                {
                    System.Threading.Thread.CurrentPrincipal = currentPrincipal;
                    ((RequisitionDocumentManager)scope).FinalizeComments(P2PDocumentType.Requisition, documentCode, false);
                }, manager);
                result = GetRequisitionManager().SentRequisitionForApproval(contactCode, documentCode, documentAmount, documentTypeId, fromCurrency, toCurrency, isOperationalBudgetEnabled, headerOrgEntityCode, isBypassOperationalBudget: isBypassOperationalBudget);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("DocumentCode: " + documentCode, "----contactCodeontactCode: " + contactCode, "----headerOrgEntityCode: " + headerOrgEntityCode);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition SentRequisitionForApproval MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "SentRequisitionForApproval", "SentRequisitionForApproval",
                                                    "Order", ExceptionType.ApplicationException, documentCode.ToString(), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while SentRequisitionForApproval method in Requisition.");
            }
            return result;
        }

        public List<string> ValidateInterfaceRequisition(long buyerPartnerCode, Requisition objRequisition)
        {
            try
            {
                SetUserExecutionContext(buyerPartnerCode);
                if (!ReferenceEquals(null, objRequisition))
                    return GetRequisitionInterfaceManager().ValidateInterfaceRequisition(objRequisition);
                else
                    throw new ArgumentNullException("objRequisition");


            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in ValidateInterfaceRequisition method", ex);
                var objCustomFault = new CustomFault(ex.Message, "ValidateInterfaceRequisition", "ValidateInterfaceRequisition",
                                                    "Requisition", ExceptionType.ApplicationException, objRequisition.DocumentNumber.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while ValidateInterfaceRequisition in Requisition.");
            }
        }

        private void SetUserExecutionContext(long buyerPartnerCode)
        {
            var _userExecutionContext = new UserExecutionContext
            {
                ClientName = Gep.Cumulus.CSM.Entities.CommonConstants.BUYERSQLCONN,
                Product = GEPSuite.eInterface,
                EntityType = "Basic Setting",
                EntityId = 8888,
                LoggerCode = "EP101",
                Culture = "en-US",
                CompanyName = Gep.Cumulus.CSM.Entities.CommonConstants.BUYERSQLCONN,
                BuyerPartnerCode = buyerPartnerCode
            };

            var objMessageHeader = new MessageHeader<UserExecutionContext>(_userExecutionContext);
            MessageHeader messageHeader = objMessageHeader.GetUntypedHeader("GepCustomHeader", "Gep.Cumulus");
            OperationContext.Current.IncomingMessageHeaders.Add(messageHeader);
        }

        public Dictionary<string, string> SaveOfflineApprovalDetails(long contactCode, long documentCode, decimal documentAmount, string fromCurrency, string toCurrency, WorkflowInputEntities workflowEntity, long headerOrgEntityCode)
        {
            string finalJsonObj = string.Empty;
            Dictionary<string, string> result;
            try
            {
                GetRequisitionDocumentManager().FinalizeComments(P2PDocumentType.Requisition, documentCode);
                result = GetRequisitionManager().SaveOfflineApprovalDetails(contactCode, documentCode, documentAmount, fromCurrency, toCurrency, workflowEntity, headerOrgEntityCode);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("DocumentCode: " + documentCode, "----contactCodeontactCode: " + contactCode, "----headerOrgEntityCode: " + headerOrgEntityCode);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition SaveOfflineApprovalDetails MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "SendManageDocumentForApproval", "SendManageDocumentForApproval",
                                                    "Order", ExceptionType.ApplicationException, documentCode.ToString(), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error while SendManageDocumentForApproval method in Requisition.");
            }
            return result;
        }
        public bool SaveContractInformation(long requisitionItemId, string extContractRef)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionManager().SaveContractInformation(requisitionItemId, extContractRef);

            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionItemId: " + requisitionItemId, "----extContractRef: " + extContractRef);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition SaveContractInformation MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "SaveContractInformation", "SaveContractInformation",
                                                    "RequisitionItemId", ExceptionType.ApplicationException, requisitionItemId.ToString(CultureInfo.InstalledUICulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while SaveContractInformation in Requisition.");
            }
        }

        public bool UpdateDocumentStatus(long documentCode, DocumentStatus docuemntStatus, bool isStockRequisition = false)
        {
            bool result;
            try
            {
                result = GetRequisitionDocumentManager().UpdateDocumentStatus(P2PDocumentType.Requisition, documentCode, docuemntStatus, 0);

                var enableStockReservation = Convert.ToBoolean(GetRequisitionCommonManager().GetSettingsValueByKey(P2PDocumentType.None, "EnableStockReservationViaExternalInventoryIntegration", GetCallerContext().ContactCode, 107));

                if (enableStockReservation)
                {
                    var reqLineStatusRequisitions = new List<LineStatusRequisition>();
                    if (isStockRequisition == true)
                    {
                        var nonStockReqItems = GetNewRequisitionManager().GetRequisitionLineItemsByRequisitionId(documentCode)?.items?.Where(x => x.isProcurable.id == 0).ToList();

                        foreach (NewP2PEntities.RequisitionItem objReqItem in nonStockReqItems)
                        {
                            var objRequisitionLineStatus = new LineStatusRequisition
                            {
                                LineNumber = objReqItem.lineNumber,
                                LineStatus = StockReservationStatus.Orderd

                            };
                            reqLineStatusRequisitions.Add(objRequisitionLineStatus);
                        }
                    }

                    GetNewRequisitionManager().UpdateLineStatusForRequisition(documentCode, StockReservationStatus.Orderd, !isStockRequisition, reqLineStatusRequisitions);
                }
            }
            catch (Exception ex)
            {
                var finalJsonObj = string.Concat("DocumentCode: " + documentCode, "----docuemntStatus: " + (int)docuemntStatus);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition UpdateDocumentStatus MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "UpdateDocumentStatus", "UpdateDocumentStatus",
                                                    "UpdateDocumentStatus", ExceptionType.ApplicationException, documentCode.ToString(CultureInfo.InstalledUICulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while UpdateDocumentStatus in Requisition.");
            }
            return result;
        }

        public List<PartnerDetails> GetPartnerDetailsAndOrderingLocationByOrderId(long RequisitionId)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionManager().GetPartnerDetailsAndOrderingLocationByOrderId(RequisitionId);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionId: " + RequisitionId);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition GetPartnerDetailsAndOrderingLocationByOrderId MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "GetPartnerDetailsAndOrderingLocationByOrderId", "GetPartnerDetailsAndOrderingLocationByOrderId",
                                                    "GetPartnerDetailsAndOrderingLocationByOrderId", ExceptionType.ApplicationException, RequisitionId.ToString(CultureInfo.InstalledUICulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while GetPartnerDetailsAndOrderingLocationByOrderId in Requisition.");
            }
        }

        public FileDetails RequisitionExportById(long requisitionId, long contactCode, int userType, string accessType)
        {
            
            try
            {
                return GetRequisitionExportManager().RequisitionExportById(requisitionId, contactCode, userType, accessType);
            }
            catch (Exception ex)
            {
                string finalJsonObj = string.Concat("RequisitionId: " + requisitionId);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition RequisitionExportById MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                throw;
            }
        }

        public string GetDownloadPDFURL(long fileId)
        {
            string finalJsonObj = string.Empty;
            string strFileURI = null;
            try
            {
                strFileURI = GetRequisitionDocumentManager().GetDownloadPDFURL(fileId);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("FileId: " + fileId);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition GetDownloadPDFURL MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                throw;
            }

            return strFileURI;
        }

        public string RequisitionPrintById(long requisitionId, long contactCode, int userType, string accessType)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionExportManager().RequisitionPrintById(requisitionId, contactCode, userType, accessType);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionId: " + requisitionId);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition RequisitionPrintById MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                throw;
            }
        }

        /// <summary>
        /// Get List of Purchase Request Form Questionnaire sets
        /// </summary>
        /// <param name="requisitionItemId">requisitionItemId</param>
        /// <returns>set of Questionnaire in List</returns>
        public List<Questionnaire> GetAllQuestionnaire(long requisitionItemId)
        {
            return GetRequisitionManager().GetAllQuestionnaire(requisitionItemId);
        }

        /// <summary>
        ///  /// Update , Save and Delete RequisitionItems based on PunchoutCarRecID and RequisitionID
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="PartnerCode"></param>
        /// <param name="BuyerPartnerCode"></param>
        /// <param name="lstP2PItem"></param>
        /// <param name="precessionValue"></param>
        /// <param name="PartnerConfigurationId"></param>
        /// <param name="DocumentCode"></param>
        /// <param name="DocumentType"></param>
        /// <param name="PunchoutCartReqId"></param>
        /// <returns></returns>
        public Int64 SaveReqCartItemsFromInterface(Int64 UserId, Int64 PartnerCode, Int64 BuyerPartnerCode, List<P2P.BusinessEntities.RequisitionItem> lstP2PItem, int precessionValue, int PartnerConfigurationId, string DocumentCode, Int64 PunchoutCartReqId, decimal Tax = 0, decimal Shipping = 0, decimal AdditionalCharges = 0)
        {
            string finalJsonObj = string.Empty;
            try
            {
                SetUserExecutionContext(BuyerPartnerCode);

                List<RequisitionItem> lstRequisitionItem = new List<RequisitionItem>();
                lstRequisitionItem = lstP2PItem;
                List<P2PItem> lstp2pitems = lstRequisitionItem.Cast<P2PItem>().ToList();

                return GetRequisitionInterfaceManager().SaveReqCartItemsFromInterface(UserId, PartnerCode, lstp2pitems, precessionValue, PartnerConfigurationId, DocumentCode, PunchoutCartReqId, Tax, Shipping, AdditionalCharges);

            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("DocumentCode: " + DocumentCode, "----UserId: " + UserId);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition SaveReqCartItemsFromInterface MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                var objCustomFault = new CustomFault(ex.Message, "SaveReqCartItemsFromInterface", "SaveReqCartItemsFromInterface",
                                                    "Requisition", ExceptionType.ApplicationException, DocumentCode, false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while SaveReqCartItemsFromInterface in Requisition.");
            }
        }


        /// <summary>
        /// Return DataTable rows of shiptpolocation details with id, name, address(concatenated), TotalTaxPercentage
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public DataTable GetListofShipToLocDetails(string searchText, int pageIndex, int pageSize, bool getByID, int shipToLocID, long lOBEntityDetailCode, long entityDetailCode)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionManager().GetListofShipToLocDetails(searchText, pageIndex, pageSize, getByID, shipToLocID, lOBEntityDetailCode, entityDetailCode);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("SearchText: " + searchText, "----ShipToLocID: " + shipToLocID);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition GetListofShipToLocDetails MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                throw;
            }
        }

        public DataTable GetListofBillToLocDetails(string searchText, int pageIndex, int pageSize, long entityDetailCode, bool getDefault, long lOBEntityDetailCode = 0)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionManager().GetListofBillToLocDetails(searchText, pageIndex, pageSize, entityDetailCode, getDefault, lOBEntityDetailCode);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("SearchText: " + searchText, "----EntityDetailCode: " + entityDetailCode);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition GetListofBillToLocDetails MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                throw;
            }
        }

        public DataTable CheckCatalogItemAccessForContactCode(long requisitionId, long requesterId)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionManager().CheckCatalogItemAccessForContactCode(requisitionId, requesterId);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionId: " + requisitionId, "----RequesterId: " + requesterId);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition CheckCatalogItemAccessForContactCode MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                throw;
            }
        }





        /// <summary>
        /// To get requisition details by documentCode
        /// </summary>
        /// <param name="clientPartnerId">Client Partner Code to identify buyer.</param>
        /// <param name="documentCode">Document Code</param>
        /// <returns></returns>
        public BZRequisition GetRequisitionDetailsById(long buyerPartnerCode, long documentCode)
        {
            try
            {
                SetUserExecutionContext(buyerPartnerCode);
                return GetRequisitionManager().GetRequisitionDetailsById(documentCode);
                //return new BZRequisition();
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occurred in GetRequisitionDetailsById method for Requisition Number : " + documentCode.ToString(CultureInfo.InvariantCulture), ex);
                LogHelper.LogError(Log, "Error occurred in GetRequisitionDetailsById method for Requisition Number : " + documentCode.ToString(CultureInfo.InvariantCulture), ex);

                string exMsg = "Error while get requisition details from interface : " + documentCode.ToString(CultureInfo.InvariantCulture) + ". Error details: " + ex.Message;

                if (ex.Message.Contains("Validation"))
                    exMsg = "{ValidationException}" + ex.Message + "{/ValidationException}";
                else
                    exMsg = "{ApplicationException}" + exMsg + "{/ApplicationException}";

                var objCustomFault = new CustomFault(ex.Message, "GetRequisitionDetailsById", "GetRequisitionDetailsById",
                                                     "Requisition", ExceptionType.ApplicationException,
                                                     documentCode.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      exMsg);
            }
        }


        public DataTable CheckCatalogItemsAccessForContactCode(long requesterId, string catalogItems)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionManager().CheckCatalogItemsAccessForContactCode(requesterId, catalogItems);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequesterId: " + requesterId, "----catalogItems: " + catalogItems);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition CheckCatalogItemsAccessForContactCode MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                throw;
            }
        }
        public bool UpdateRequisitionLineStatusonRFXCreateorUpdate(long documentCode, List<long> p2pLineItemId, DocumentType docType, bool IsDocumentDeleted = false)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionManager().UpdateRequisitionLineStatusonRFXCreateorUpdate(documentCode, p2pLineItemId, docType, IsDocumentDeleted);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("DocumentCode: " + documentCode, "----docType: " + docType, "----IsDocumentDeleted: " + IsDocumentDeleted);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition UpdateRequisitionLineStatusonRFXCreateorUpdate MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                throw;
            }
        }

        public List<Taxes> GetRequisitioneHeaderTaxes(long requisitionId, int pageIndex, int pageSize)
        {
            try
            {
                return GetRequisitionManager().GetRequisitioneHeaderTaxes(requisitionId, pageIndex, pageSize);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetRequisitioneHeaderTaxes method", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetRequisitioneHeaderTaxes", "GetRequisitioneHeaderTaxes",
                                                    "Requisition", ExceptionType.ApplicationException, requisitionId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while GetRequisitioneHeaderTaxes in Requisition.");
            }

        }


        public bool UpdateRequisitionHeaderTaxes(ICollection<Taxes> taxes, long requisitionId, bool updateLineTax = false, int accuredTax = 1)
        {
            try
            {
                return GetRequisitionManager().UpdateRequisitionHeaderTaxes(taxes, requisitionId, updateLineTax, accuredTax);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in requisition UpdateRequisitionHeaderTaxes method for requisitionId=" + requisitionId, ex);
                var objCustomFault = new CustomFault(ex.Message, "UpdateRequisitionHeaderTaxes", "UpdateRequisitionHeaderTaxes",
                                                     "Requisition", ExceptionType.ApplicationException,
                                                     requisitionId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while Updating Requisition Header Taxes :" +
                                                      requisitionId);
            }
        }
        public ICollection<RequisitionItem> GetRequisitionItemsDispatchMode(long documentCode)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionManager().GetRequisitionItemsDispatchMode(documentCode).Cast<RequisitionItem>().ToList();
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("documentCode: " + documentCode);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition GetRequisitionItemsDispatchMode MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                throw;
            }
        }

        public long SaveChangeRequisitionRequest(int requisitionSource, long documentCode, string documentName, string documentNumber, bool isFunctionalAdmin = false, bool documentActive = false)
        {
            long result;
            string finalJsonObj = string.Empty;
            try
            {
                result = GetRequisitionManager().SaveChangeRequisitionRequest(requisitionSource, documentCode, documentName, documentNumber, DocumentSourceType.None, "", false, false, isFunctionalAdmin, documentActive);
                if (result > 0)
                    GetRequisitionDocumentManager().FinalizeComments(P2PDocumentType.Requisition, documentCode);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("requisitionSource: " + requisitionSource, "----documentCode: " + documentCode
                                   , "----documentName: " + documentName, "----documentNumber: " + documentNumber);
                LogHelper.LogError(Log, string.Format("Error occured in SaveChangeRequisitionRequest method for Buyer Partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                throw;
            }
            return result;
        }

        public int CopyRequisition(long SourceRequisitionId, long DestinationRequisitionId, long ContactCode, int PrecessionValue, bool isAccountChecked = false, bool isCommentsChecked = false, bool isNotesAndAttachmentChecked = false, bool isAddNonCatalogItems = true, bool isCheckReqUpdate = false, bool IsCopyEntireReq = false, bool isNewNotesAndAttachmentChecked = false)
        {
            try
            {
                return GetRequisitionManager().CopyRequisition(SourceRequisitionId, DestinationRequisitionId, ContactCode, PrecessionValue, isAccountChecked, isCommentsChecked, isNotesAndAttachmentChecked, isAddNonCatalogItems, isCheckReqUpdate, IsCopyEntireReq, isNewNotesAndAttachmentChecked);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in Requisitionservice CopyRequisition method", ex);
                var objCustomFault = new CustomFault(ex.Message, "CopyRequisition", "CopyRequisition",
                                                    "CopyRequisition", ExceptionType.ApplicationException,
                                                   SourceRequisitionId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error occured while Copy Requisition.");
            }
        }
        public bool CheckBiddingInProgress(long documentId)
        {
            try
            {
                return GetRequisitionManager().CheckBiddingInProgress(documentId);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in requisition CheckBiddingInProgress method for documentId=" + documentId, ex);
                var objCustomFault = new CustomFault(ex.Message, "CheckBiddingInProgress", "CheckBiddingInProgress",
                                                     "Requisition", ExceptionType.ApplicationException,
                                                     documentId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while Updating Requisition Header Taxes :" +
                                                      documentId);
            }
        }
        public List<long> AutoCreateWorkBenchOrder(long documentId, int processFlag, bool isautosubmit)
        {
            try
            {
                return GetAutoSourcingManager().AutoSourcing(documentId, processFlag, isautosubmit);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in requisition AutoSourcing method for documentId=" + documentId, ex);
                var objCustomFault = new CustomFault(ex.Message, "AutoSourcing", "AutoSourcing",
                                                     "Requisition", ExceptionType.ApplicationException,
                                                     documentId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while Auto Create Work Bench Order" +
                                                      documentId);
            }
        }

        public DataSet GetBuyerAssigneeDetails(long ContactCode, string SearchText, int StartIndex, int Size)
        {
            try
            {
                return GetRequisitionManager().GetBuyerAssigneeDetails(ContactCode, SearchText, StartIndex, Size);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetBuyerAssigneeDetails method", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetBuyerAssigneeDetails", "GetBuyerAssigneeDetails",
                                                    "ContactCode", ExceptionType.ApplicationException, ContactCode.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while GetBuyerAssigneeDetails in Requisition.");
            }

        }
        public long CancelChangeRequisition(long documentCode, long userId, int requisitionSource)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionManager().CancelChangeRequisition(documentCode, userId, requisitionSource);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("documentCode: " + documentCode);
                LogHelper.LogError(Log, string.Format("Error occured in CancelChangeRequisition Method in RequisitionService Document Code = {0},  User ID = {1},  Requisition Source = {2}, InnerExceptionMessage = {3},  ParameterValues = {4}.", documentCode, userId, requisitionSource, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                throw;
            }
        }
        public bool validateDocumentBeforeNextAction(long DocumentId)
        {
            try
            {
                return GetRequisitionDocumentManager().validateDocumentBeforeNextAction(P2PDocumentType.Requisition, DocumentId);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in validateDocumentBeforeNextAction method", ex);
                var objCustomFault = new CustomFault(ex.Message, "validateDocumentBeforeNextAction", "validateDocumentBeforeNextAction",
                                                    "ContactCode", ExceptionType.ApplicationException, DocumentId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while validateDocumentBeforeNextAction in Requisition.");
            }

        }

        public Dictionary<string, int> GetRequisitionPunchoutItemCount(long RequisitionId)
        {
            try
            {
                return GetRequisitionManager().GetRequisitionPunchoutItemCount(RequisitionId);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetRequisitionPunchoutItemCount method", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetRequisitionPunchoutItemCount", "GetRequisitionPunchoutItemCount",
                                                    "RequisitionId", ExceptionType.ApplicationException, RequisitionId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while GetRequisitionPunchoutItemCount in Requisition.");
            }
        }
        public void SyncChangeRequisition(long RequisitionId)
        {
            try
            {
                GetRequisitionDocumentManager().SyncChangeRequisition(RequisitionId);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in SyncChangeRequisition method", ex);
                var objCustomFault = new CustomFault(ex.Message, "SyncChangeRequisition", "SyncChangeRequisition",
                                                    "RequisitionId", ExceptionType.ApplicationException, RequisitionId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while SyncChangeRequisition in Requisition.");
            }
        }

        public void SendNotificationForRequisitionReview(long documentCode, List<ReviewerDetails> lstPendingReviewers, List<ReviewerDetails> lstPastReviewer, string eventName, DocumentStatus documentStatus, string reviewType)
        {
            string finalJsonObj = string.Empty;
            try
            {                
                GetRequisitionEmailNotificationManager().SendNotificationForRequisitionReview(documentCode, lstPendingReviewers, lstPastReviewer, eventName, documentStatus, reviewType);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("DocumentCode: " + documentCode);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition SendNotificationForRequisitionReview MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "SendNotificationForRequisitionReview", "SendNotificationForRequisitionApproval",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     documentCode.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error occured while Send Notification For Requisition Review for DocumentCode = " +
                                                      documentCode.ToString(CultureInfo.InvariantCulture));
            }
        }

        public void SendNotificationForReviewRejectedRequisition(long requisition, ReviewerDetails rejector, List<ReviewerDetails> prevReviewers, string queryString)
        {
            string finalJsonObj = string.Empty;
            try
            {
                GetRequisitionEmailNotificationManager().SendNotificationForReviewRejectedRequisition(requisition, rejector, prevReviewers, queryString);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionId: " + requisition);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition SendNotificationForReviewRejectedRequisition MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "SendNotificationForReviewRejectedRequisition", "SendNotificationForReviewRejectedRequisition",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     requisition.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error occured while Send Notification For Rejected Review Requisition for requisitionId = " +
                                                      requisition.ToString(CultureInfo.InvariantCulture));

            }
        }

        public void SendReviewedRequisitionForApproval(Requisition requisition)
        {
            string finalJsonObj = string.Empty;
            try
            {
                GetRequisitionManager().SendReviewedRequisitionForApproval(requisition);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionId: " + requisition);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition SendReviewedRequisitionForApproval MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "SendReviewedRequisitionForApproval", "SendReviewedRequisitionForApproval", "Requsition", ExceptionType.ApplicationException, requisition.DocumentCode.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error occured while Sending Reviewed Requisition for approval, requisitionId = " + requisition.DocumentCode.ToString(CultureInfo.InvariantCulture));
            }
        }

        public void SendNotificationForReviewAcceptedRequisition(long requisitionId, ReviewerDetails acceptor, string queryString)
        {
            string finalJsonObj = string.Empty;

            try
            {
                GetRequisitionEmailNotificationManager().SendNotificationForReviewAcceptedRequisition(requisitionId, acceptor, queryString);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionId: " + requisitionId);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition SendNotificationForReviewAcceptedRequisition MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "SendNotificationForReviewAcceptedRequisition", "SendNotificationForReviewAcceptedRequisition", "Requsition", ExceptionType.ApplicationException,
                                                     requisitionId.ToString(CultureInfo.InvariantCulture), false);

                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault, "Error occured while Sending Notification For Accepted Requisition for requisitionId = " + requisitionId.ToString(CultureInfo.InvariantCulture));
            }
        }

        public bool SendNotificationForSkipOrOffLineRequisitionApproval(long documentCode, List<ApproverDetails> lstApprovers, int skipType = 0, bool isOffLine = false, long actionarId = 0)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionEmailNotificationManager().SendNotificationForSkipOrOffLineRequisitionApproval(documentCode, lstApprovers, skipType, isOffLine, actionarId);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionId: " + documentCode);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition SendNotificationForSkipOrOffLineRequisitionApproval MethodBuyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);

                var objCustomFault = new CustomFault(ex.Message, "SendNotificationForSkipOrOffLineRequisitionApproval", "SendNotificationForSkipOrOffLineRequisitionApproval",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     documentCode.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error occured while Send Notification For Rejected Review Requisition for requisitionId = " +
                                                      documentCode.ToString(CultureInfo.InvariantCulture));

            }
        }

        public DocumentIntegrationResults CreateRequisitionFromS2C(DocumentIntegrationEntity objDocumentIntegrationEntity)
        {
            return GetNewRequisitionManager().CreateRequisitionFromS2C(objDocumentIntegrationEntity);
        }


        public List<long> GetRequisitionListForInterfaces(string docType, int docCount, int sourceSystemId)
        {
            try
            {
                return GetRequisitionInterfaceManager().GetRequisitionListForInterfaces(docType, docCount, sourceSystemId);
            }
            catch (Exception ex)
            {
                var objCustomFault = new CustomFault(ex.Message, "GetRequisitionListForInterfaces", "GetRequisitionListForInterfaces",
                                                     "Requisition", ExceptionType.ApplicationException, null, false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while GetRequisitionListForInterfaces");
            }
        }

        public DataSet ValidateInterfaceLineStatus(long buyerPartnerCode, DataTable dtRequisitionDetail)
        {
            try
            {
                if (dtRequisitionDetail != null && dtRequisitionDetail.Rows.Count > 0)
                    return GetRequisitionInterfaceManager().ValidateInterfaceLineStatus(buyerPartnerCode, dtRequisitionDetail);
                else
                    throw new ArgumentNullException("dtRequisitionDetail");
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in ValidateInterfaceLineStatus method", ex);
                var objCustomFault = new CustomFault(ex.Message, "ValidateInterfaceLineStatus", "ValidateInterfaceLineStatus",
                                                    "Requisition", ExceptionType.ApplicationException, buyerPartnerCode.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while ValidateInterfaceLineStatus in Requisition.");
            }
        }

        public void UpdateRequisitionLineStatus(long requisitionId, long buyerPartnerCode)
        {
            try
            {
                SetUserExecutionContext(buyerPartnerCode);
                if (requisitionId != 0 && buyerPartnerCode > 0)
                    GetRequisitionManager().UpdateRequisitionLineStatus(requisitionId, buyerPartnerCode);
                else
                    throw new ArgumentNullException("dtRequisitionDetail");
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in UpdateRequisitionLineStatus method", ex);
                var objCustomFault = new CustomFault(ex.Message, "UpdateRequisitionLineStatus", "UpdateRequisitionLineStatus",
                                                    "Requisition", ExceptionType.ApplicationException, buyerPartnerCode.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while UpdateRequisitionLineStatus in Requisition.");
            }
        }

        public bool PushingDataToEventHub(long Documentcode)
        {
            try
            {
                return GetRequisitionManager().PushingDataToEventHub(Documentcode);
            }
            catch (Exception ex)
            {
                var objCustomFault = new CustomFault(ex.Message, "PushingDataToEventHub", "PushingDataToEventHub",
                                                     "Requisition", ExceptionType.ApplicationException, null, false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while PushingDataToEventHub");
            }
        }

        public RequisitionItemCustomAttributes GetCutsomAttributesForLines(List<long> itemIds, int sourceDocType, int targetDocType, string level)
        {
            try
            {
                return GetRequisitionManager().GetCutsomAttributesForLines(itemIds, sourceDocType, targetDocType, level);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetCutsomAttributesForLines method", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetCutsomAttributesForLines", "GetCutsomAttributesForLines",
                                                    "Requisition", ExceptionType.ApplicationException, null, false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while GetCutsomAttributesForLines in Requisition.");
            }
        }

        public List<RequisitionItem> GetLineItemBasicDetailsForInterface(long documentCode)
        {
            try
            {
                return GetRequisitionInterfaceManager().GetLineItemBasicDetailsForInterface(documentCode);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetLineItemBasicDetailsForInterface method in RequisitionService ", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetLineItemBasicDetailsForInterface", "GetLineItemBasicDetailsForInterface",
                                                    "Requisition", ExceptionType.ApplicationException, null, false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while GetLineItemBasicDetailsForInterface in RequisitionService.");
            }

        }
        public RiskFormDetails GetRiskFormQuestionScore()
        {
            try
            {
                return GetRequisitionManager().GetRiskFormQuestionScore();
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetRiskFormQuestionScore method in RequisitionService ", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetRiskFormQuestionScore", "GetRiskFormQuestionScore",
                                                    "Requisition", ExceptionType.ApplicationException, null, false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while GetRiskFormQuestionScore in RequisitionService.");
            }

        }
        public RiskFormDetails GetRiskFormHeaderInstructionsText()
        {
            try
            {
                return GetRequisitionManager().GetRiskFormHeaderInstructionsText();
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in GetRiskFormHeaderInstructionsText method in RequisitionService ", ex);
                var objCustomFault = new CustomFault(ex.Message, "GetRiskFormHeaderInstructionsText", "GetRiskFormHeaderInstructionsText",
                                                    "Requisition", ExceptionType.ApplicationException, null, false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error while GetRiskFormHeaderInstructionsText in RequisitionService.");
            }

        }

        public Requisition GetRequisitionPartialDetailsById(long requisitionId)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetRequisitionManager().GetRequisitionPartialDetailsById(requisitionId);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionId: " + requisitionId);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition GetRequisitionPartialDetailsById Method Buyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                UtilsManager.ThrowHttpException(ex.Message);

                var objCustomFault = new CustomFault(ex.Message, "GetRequisitionPartialDetailsById", "GetRequisitionPartialDetailsById",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     requisitionId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error occured while getting Requisition Basic details for requisitionId = " +
                                                      requisitionId.ToString(CultureInfo.InvariantCulture));
            }
        }
        public bool ConsumeReleaseCapitalBudget(long requisitionId, DocumentStatus documentStatus, bool isReConsume)
        {
            string finalJsonObj = string.Empty;
            bool result = false;
            try
            {
                if(documentStatus== DocumentStatus.Accepted || documentStatus == DocumentStatus.Approved)
                    result = GetCapitalBudgetManager().ConsumeCapitalBudget(requisitionId, isReConsume);
                else
                    result = GetCapitalBudgetManager().ReleaseBudget(requisitionId, 0);

            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("RequisitionId: " + requisitionId);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition ConsumeReleaseCapitalBudget Method Buyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                UtilsManager.ThrowHttpException(ex.Message);

                var objCustomFault = new CustomFault(ex.Message, "ConsumeReleaseCapitalBudget", "ConsumeReleaseCapitalBudget",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     requisitionId.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error occured while Consume Capital Budget for requisitionId = " +
                                                      requisitionId.ToString(CultureInfo.InvariantCulture));
            }
            return result;
        }

        public bool ReleaseBudget(long documentCode, long parentDocumentCode = 0)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetCapitalBudgetManager().ReleaseBudget(documentCode, parentDocumentCode);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("documentCode: " + documentCode);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition ReleaseBudget Method Buyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                UtilsManager.ThrowHttpException(ex.Message);

                var objCustomFault = new CustomFault(ex.Message, "ReleaseBudget", "ReleaseBudget",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     documentCode.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error occured while Consume Release Budget for requisitionId = " +
                                                      documentCode.ToString(CultureInfo.InvariantCulture));
            }
        }
        public Boolean CapitalBudgetValidation(long documentCode, Boolean isReConsume = false)
        {
            string finalJsonObj = string.Empty;
            try
            {
                return GetCapitalBudgetManager().CapitalBudgetValidation(documentCode,isReConsume);
            }
            catch (Exception ex)
            {
                finalJsonObj = string.Concat("documentCode: " + documentCode);
                LogHelper.LogError(Log, string.Format("Error occured in Requisition Capital Budget Validation Method Buyer partner code = {0} , Contact Code = {1},  InnerExceptionMessage = {2},  ParameterValues = {3}.", GetCallerContext().BuyerPartnerCode, GetCallerContext().ContactCode, ex.InnerException != null ? ex.InnerException.Message : "null", finalJsonObj), ex);
                UtilsManager.ThrowHttpException(ex.Message);

                var objCustomFault = new CustomFault(ex.Message, "CapitalBudgetValidation", "CapitalBudgetValidation",
                                                     "Requsition", ExceptionType.ApplicationException,
                                                     documentCode.ToString(CultureInfo.InvariantCulture), false);
                throw new SMARTFaultException.FaultException<CustomFault>(objCustomFault,
                                                      "Error occured while Consume Capital Budget Validation for requisitionId = " +
                                                      documentCode.ToString(CultureInfo.InvariantCulture));
            }
        }
       
        public Requisition TestJWT(long requisitionId, long userId, int typeOfUser = 0)
        {
            Requisition result = null;
            string finalJsonObj = string.Empty;
            try
            {
                LogNewRelicAppForJWTTokenTracking(0, "Before GetRequisitionDocumentManager_WithJWT", "TestJWT");
                var mgr = GetRequisitionDocumentManager_WithJWT();
                LogNewRelicAppForJWTTokenTracking(0, "After GetRequisitionDocumentManager_WithJWT", "TestJWT");

                try
                {
                    result = mgr.GetAllRequisitionDetailsByRequisitionId(requisitionId);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(Log, "Error occured in TestJWT Method : " + ex.Message, ex);
                    LogNewRelicAppForJWTTokenTracking(0, "Error occured in TestJWT method", "TestJWT");
                }

                return result;                
            }
            catch (Exception ex)
            {
                LogHelper.LogError(Log, "Error occured in TestJWT Method : " + ex.Message, ex);
                throw;                
            }
        }

        #region Get Managers
        private NewRequisitionManager GetNewRequisitionManager()
        {
            var context = this.GetCallerContext();
            try
            {
                var helper = new RequisitionServiceHelper(context);
                this.JWTToken = helper.GetToken();
            }
            catch(Exception ex)
            {
                LogNewRelicAppForJWTTokenTracking(context.BuyerPartnerCode, "Error in GetNewRequisitionManager : " + ex.Message, "GetRequisitionDocumentManager_WithJWT");
            }

            var bo = new NewRequisitionManager(JWTToken, context);
            bo.UserContext = context;
            bo.GepConfiguration = this.Config;
            return bo;
        }

        private RequisitionDocumentManager GetRequisitionDocumentManager()
        {
            var context = this.GetCallerContext();

            try
            {
                var helper = new RequisitionServiceHelper(context);
                this.JWTToken = helper.GetToken();
            }
            catch (Exception ex)
            {
                LogNewRelicAppForJWTTokenTracking(context.BuyerPartnerCode, "Error in GetNewRequisitionManager : " + ex.Message, "GetRequisitionDocumentManager_WithJWT");
            }

            var bo = new RequisitionDocumentManager(JWTToken, context);
            bo.UserContext = context;
            bo.GepConfiguration = this.Config;
            return bo;
        }        

        private RequisitionDocumentManager GetRequisitionDocumentManager_WithJWT()
        {            
            var context = this.GetCallerContext();
            var helper = new RequisitionServiceHelper(context);
            this.JWTToken = helper.GetToken();

            if (string.IsNullOrEmpty(this.JWTToken))
            {
                LogNewRelicAppForJWTTokenTracking(context.BuyerPartnerCode, "After setting JWT token in GetRequisitionDocumentManager_WithJWT, Token not found", "GetRequisitionDocumentManager_WithJWT");
            }
            else
            {
                LogNewRelicAppForJWTTokenTracking(context.BuyerPartnerCode, "After setting JWT token in GetRequisitionDocumentManager_WithJWT, Found Token", "GetRequisitionDocumentManager_WithJWT");
            }

            var bo = new RequisitionDocumentManager(JWTToken, context);
            bo.UserContext = context;
            bo.GepConfiguration = this.Config;
            LogNewRelicAppForJWTTokenTracking(context.BuyerPartnerCode, "Before return statement, LN:2434", "GetRequisitionDocumentManager_WithJWT");
            return bo;
        }

        private RequisitionManager GetRequisitionManager()
        {
            var context = this.GetCallerContext();

            try
            {
                var helper = new RequisitionServiceHelper(context);
                this.JWTToken = helper.GetToken();
            }
            catch (Exception ex)
            {
                LogNewRelicAppForJWTTokenTracking(context.BuyerPartnerCode, "Error in GetNewRequisitionManager : " + ex.Message, "GetRequisitionManager");
            }

            var bo = new RequisitionManager(JWTToken, context);
            bo.UserContext = context;
            bo.GepConfiguration = this.Config;
            return bo;
        }

        private RequisitionEmailNotificationManager GetRequisitionEmailNotificationManager()
        {
            var context = this.GetCallerContext();

            try
            {
                var helper = new RequisitionServiceHelper(context);
                this.JWTToken = helper.GetToken();
            }
            catch (Exception ex)
            {
                LogNewRelicAppForJWTTokenTracking(context.BuyerPartnerCode, "Error in GetNewRequisitionManager : " + ex.Message, "GetRequisitionEmailNotificationManager");
            }

            LogNewRelicAppForJWTTokenTracking(context.BuyerPartnerCode, "Start GetRequisitionEmailNotificationManager is called. LN2484", "GetRequisitionEmailNotificationManager");

            var bo = new RequisitionEmailNotificationManager(JWTToken, context);
            bo.UserContext = context;
            bo.GepConfiguration = this.Config;

            LogNewRelicAppForJWTTokenTracking(context.BuyerPartnerCode, "End GetRequisitionEmailNotificationManager is called. LN2490", "GetRequisitionEmailNotificationManager");

            return bo;
        }

        private RequisitionCommonManager GetRequisitionCommonManager()
        {
            var context = this.GetCallerContext();
            var bo = new RequisitionCommonManager(JWTToken, context);

            try
            {
                var helper = new RequisitionServiceHelper(context);
                this.JWTToken = helper.GetToken();
            }
            catch (Exception ex)
            {
                LogNewRelicAppForJWTTokenTracking(context.BuyerPartnerCode, "Error in GetNewRequisitionManager : " + ex.Message, "GetRequisitionCommonManager");
            }

            bo.UserContext = context;
            bo.GepConfiguration = this.Config;
            return bo;
        }

        private BusinessObjects.RequisitionExportManager GetRequisitionExportManager()
        {
            var context = this.GetCallerContext();

            try
            {
                var helper = new RequisitionServiceHelper(context);
                this.JWTToken = helper.GetToken();
            }
            catch (Exception ex)
            {
                LogNewRelicAppForJWTTokenTracking(context.BuyerPartnerCode, "Error in GetNewRequisitionManager : " + ex.Message, "GetRequisitionExportManager");
            }

            var bo = new BusinessObjects.RequisitionExportManager(JWTToken, context);
            bo.UserContext = context;
            bo.GepConfiguration = this.Config;
            return bo;
        }

        private AutoSourcingManager GetAutoSourcingManager()
        {
            var context = this.GetCallerContext();

            try
            {
                var helper = new RequisitionServiceHelper(context);
                this.JWTToken = helper.GetToken();
            }
            catch (Exception ex)
            {
                LogNewRelicAppForJWTTokenTracking(context.BuyerPartnerCode, "Error in GetNewRequisitionManager : " + ex.Message, "GetAutoSourcingManager");
            }

            var bo = new AutoSourcingManager(JWTToken, context);
            bo.UserContext = context;
            bo.GepConfiguration = this.Config;
            return bo;
        }

        private RequisitionInterfaceManager GetRequisitionInterfaceManager()
        {
            var context = this.GetCallerContext();

            try
            {
                var helper = new RequisitionServiceHelper(context);
                this.JWTToken = helper.GetToken();
            }
            catch (Exception ex)
            {
                LogNewRelicAppForJWTTokenTracking(context.BuyerPartnerCode, "Error in GetNewRequisitionManager : " + ex.Message, "GetRequisitionInterfaceManager");
            }

            var bo = new RequisitionInterfaceManager(JWTToken);
            bo.UserContext = context;
            bo.GepConfiguration = this.Config;
            return bo;
        }

        private RequisitionRuleEngineManager GetRequisitionRuleEngineManager()
        {
            var context = this.GetCallerContext();

            try
            {
                var helper = new RequisitionServiceHelper(context);
                this.JWTToken = helper.GetToken();
            }
            catch (Exception ex)
            {
                LogNewRelicAppForJWTTokenTracking(context.BuyerPartnerCode, "Error : " + ex.Message, "GetRequisitionRuleEngineManager");
            }

            var bo = new RequisitionRuleEngineManager(JWTToken);
            bo.UserContext = context;
            bo.GepConfiguration = this.Config;
            return bo;
        }

        private CapitalBudgetManager GetCapitalBudgetManager()
        {
            var context = this.GetCallerContext();

            try
            {
                var helper = new RequisitionServiceHelper(context);
                this.JWTToken = helper.GetToken();
            }
            catch (Exception ex)
            {
                LogNewRelicAppForJWTTokenTracking(context.BuyerPartnerCode, "Error : " + ex.Message, "GetCapitalBudgetManager");
            }

            var bo = new CapitalBudgetManager(JWTToken, context);
            bo.UserContext = context;
            bo.GepConfiguration = this.Config;
            return bo;
        }

        private void LogNewRelicAppForJWTTokenTracking(long buyerPartnerCode, string message, string method)
        {
            var eventAttributes = new Dictionary<string, object>();
            eventAttributes.Add("buyerPartnerCode", buyerPartnerCode);
            eventAttributes.Add("message", message);
            eventAttributes.Add("method", method);
            NewRelic.Api.Agent.NewRelic.RecordCustomEvent("RequisitionServiceJWTTokenTesting", eventAttributes);
        }        
        #endregion
    }
}
