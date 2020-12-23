using Gep.Cumulus.CSM.BaseDataAccessObjects;
using GEP.Cumulus.P2P.BusinessEntities;
using System;
using System.Collections.Generic;
using System.Data;

namespace GEP.Cumulus.P2P.Req.DataAccessObjects
{
    public interface IRequisitionInterfaceDAO : IBaseDAO
    {

        /// <summary>
        /// Validates Interface Kube Status
        /// </summary>
        /// <param name="buyerPartnerCode"></param>
        /// <param name="dtRequisitionDetail"></param>
        /// <returns></returns>
        DataSet ValidateInterfaceLineStatus(long buyerPartnerCode, DataTable dtRequisitionDetail);

        /// <summary>
        /// Updates Line Status For Requisition From Interface
        /// </summary>
        /// <param name="RequisitionId"></param>
        /// <param name="LineStatus"></param>
        /// <param name="IsUpdateAllItems"></param>
        /// <param name="Items"></param>
        /// <param name="StockReservationNumber"></param>
        /// <returns></returns>
        bool UpdateLineStatusForRequisitionFromInterface(long RequisitionId, BusinessEntities.StockReservationStatus LineStatus, bool IsUpdateAllItems, List<BusinessEntities.LineStatusRequisition> Items, string StockReservationNumber);

        /// <summary>
        /// Gets Requisition List For Interfaces
        /// </summary>
        /// <param name="docType"></param>
        /// <param name="docCount"></param>
        /// <param name="sourceSystemId"></param>
        /// <returns></returns>
        List<long> GetRequisitionListForInterfaces(string docType, int docCount, int sourceSystemId);

        /// <summary>
        /// Validates Interface Requisition
        /// </summary>
        /// <param name="objRequisition"></param>
        /// <param name="dctSettings"></param>
        /// <param name="IsOrderingLocationMandatory"></param>
        /// <param name="IsDefaultOrderingLocation"></param>
        /// <returns></returns>
        List<string> ValidateInterfaceRequisition(Requisition objRequisition, Dictionary<string, string> dctSettings, bool IsOrderingLocationMandatory = false, bool IsDefaultOrderingLocation = false);

        /// <summary>
        /// Prorate HeaderTax AndS hipping
        /// </summary>
        /// <param name="objRequisition"></param>
        //void ProrateHeaderTaxAndShipping(Requisition objRequisition);

        /// <summary>
        /// Validates Requisition Items For Exception Handling
        /// </summary>
        /// <param name="dtItemDetails"></param>
        /// <returns></returns>
        DataTable ValidateReqItemsForExceptionHandling(DataTable dtItemDetails);

        /// <summary>
        /// Validates Item Details To Be Derived From Interface
        /// </summary>
        /// <param name="itemNumber"></param>
        /// <param name="partnerSourceSystemValue"></param>
        /// <param name="uom"></param>
        /// <returns></returns>
        DataSet ValidateItemDetailsToBeDerivedFromInterface(string itemNumber, string partnerSourceSystemValue, string uom);

        /// <summary>
        /// Validate Ship To Bill To From Interface
        /// </summary>
        /// <param name="objRequisition"></param>
        /// <param name="shipToLocSetting"></param>
        /// <param name="billToLocSetting"></param>
        /// <param name="deliverToFreeText"></param>
        /// <param name="LobentitydetailCode"></param>
        /// <param name="IsDefaultBillToLocation"></param>
        /// <param name="entityDetailCode"></param>
        /// <returns></returns>
        DataSet ValidateShipToBillToFromInterface(Requisition objRequisition, bool shipToLocSetting, bool billToLocSetting, bool deliverToFreeText, long LobentitydetailCode, bool IsDefaultBillToLocation, long entityDetailCode);

        /// <summary>
        /// Gets the Line Item Basic Details For Interface
        /// </summary>
        /// <param name="documentCode"></param>
        /// <returns></returns>
        List<RequisitionItem> GetLineItemBasicDetailsForInterface(long documentCode);

        /// <summary>
        /// Validates Error Based Interface Requisition
        /// </summary>
        /// <param name="objRequisition"></param>
        /// <param name="dctSettings"></param>
        /// <returns></returns>
        List<string> ValidateErrorBasedInterfaceRequisition(Requisition objRequisition, Dictionary<string, string> dctSettings);

        /// <summary>
        /// Gets Split Details
        /// </summary>
        /// <param name="RequisitionItem"></param>
        /// <param name="contactCode"></param>
        /// <param name="lobEntityDetailCode"></param>
        /// <param name="EntityCode"></param>
        /// <returns></returns>
        DataSet GetSplitsDetails(List<RequisitionItem> RequisitionItem, long contactCode, long lobEntityDetailCode, string EntityCode = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ClientLocationCode"></param>
        /// <param name="PartnerCode"></param>
        /// <param name="headerEntities"></param>
        /// <param name="IsDefaultOrderingLocation"></param>
        /// <returns></returns>
        //long GetOrderLocationIdByClientLocationCode(string ClientLocationCode, long PartnerCode, string headerEntities, bool IsDefaultOrderingLocation);

        /// <summary>
        /// Gets Requisition Header Details By Id For Interface
        /// </summary>
        /// <param name="requisitionId"></param>
        /// <param name="deliverToFreeText"></param>
        /// <returns></returns>
        BZRequisition GetRequisitionHeaderDetailsByIdForInterface(long requisitionId, bool deliverToFreeText = false);

        /// <summary>
        /// Delete Charge And Splits Items By Item ChargeId
        /// </summary>
        /// <param name="lstItemCharge"></param>
        /// <param name="ChangeorderItemId"></param>
        /// <returns></returns>
        bool DeleteChargeAndSplitsItemsByItemChargeId(List<ItemCharge> lstItemCharge, long ChangeorderItemId);

        /// <summary>
        /// Delete Splits By ItemId
        /// </summary>
        /// <param name="RequisitionItemId"></param>
        /// <param name="documentId"></param>
        /// <returns></returns>
        bool DeleteSplitsByItemId(long RequisitionItemId, long documentId);

        /// <summary>
        /// Save Requisition Additional Details From Interface
        /// </summary>
        /// <param name="documentCode"></param>
        void SaveRequisitionAdditionalDetailsFromInterface(long documentCode);

        /// <summary>
        /// Get Requisition Accounting Details By ItemId
        /// </summary>
        /// <param name="requisitionItemId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="itemType"></param>
        /// <param name="precessionValue"></param>
        /// <param name="precissionTotal"></param>
        /// <param name="precessionValueForTaxAndCharges"></param>
        /// <returns></returns>
        //List<RequisitionSplitItems> GetRequisitionAccountingDetailsByItemId(long requisitionItemId, int pageIndex, int pageSize, int itemType, int precessionValue = 0, int precissionTotal = 0, int precessionValueForTaxAndCharges = 0);

        /// <summary>
        /// Proprate Line Item Tax and Shipping
        /// </summary>
        /// <param name="objItems"></param>
        /// <param name="Tax"></param>
        /// <param name="shipping"></param>
        /// <param name="AdditionalCharges"></param>
        /// <param name="PunchoutCartReqId"></param>
        /// <param name="precessionValue"></param>
        /// <param name="maxPrecessionforTotal"></param>
        /// <param name="maxPrecessionForTaxesAndCharges"></param>
        /// <returns></returns>
        DataSet ProprateLineItemTaxandShipping(List<P2PItem> objItems, decimal Tax, decimal shipping, decimal AdditionalCharges, Int64 PunchoutCartReqId, int precessionValue = 0, int maxPrecessionforTotal = 0, int maxPrecessionForTaxesAndCharges = 0);
        void SaveAdditionalFieldAttributes(long documentID, long documentItemID, List<P2PAdditionalFieldAtrribute> lstAdditionalFieldAttributues, string PurchaseTypeDescription);
        void SaveRequisitionItemAdditionalDetailsFromInterface(List<RequisitionItem> requisitionItems,int SpendControlType=0);

        /// <summary>
        /// Update Requisition Status For Stock Reservation
        /// </summary>
        /// <param name="requisitionId"></param>
        /// <returns></returns>
        //bool UpdateRequisitionStatusForStockReservation(long requisitionId);

        /// <summary>
        /// Update Requisition Notification Details
        /// </summary>
        /// <param name="requisitionId"></param>
        /// <returns></returns>
        //RequisitionLineStatusUpdateDetails UpdateRequisitionNotificationDetails(long requisitionId);


    }
}