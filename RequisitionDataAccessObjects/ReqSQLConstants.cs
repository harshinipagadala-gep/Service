using System.Diagnostics.CodeAnalysis;

namespace GEP.Cumulus.P2P.Req.DataAccessObjects.SQLServer
{
    [ExcludeFromCodeCoverage]
    internal static class ReqSqlConstants
    {
        #region Requisition

        #region Requisition Columns
        internal const string COL_BILLTOID = "BillToId";
        internal const string COL_BILLTOEMAIL = "BillToEmail";
        internal const string COL_BILLTOFAX = "BillToFax";
        internal const string COL_BILLTOADDRESS1 = "BillToAddress1";
        internal const string COL_BILLTOADDRESS2 = "BillToAddress2";
        internal const string COL_BILLTOADDRESS3 = "BillToAddress3";
        internal const string COL_BILLTOCOUNTRY = "BillToCountry";
        internal const string COL_BUSINESSUNITID = "BusinessUnitId";
        internal const string COL_CREATEDBYID = "CreatedById";
        internal const string COL_CREATEDBYFNAME = "CreatedByFirstName";
        internal const string COL_CREATEDBYLNAME = "CreatedByLastName";
        internal const string COL_CURRENCYSYMBOL = "CurrencySymbol";
        internal const string COL_DELIVERTOID = "DeliverToId";
        internal const string COL_DELTOADDRESS1 = "DeliverToAddress1";
        internal const string COL_DELTOADDRESS2 = "DeliverToAddress2";
        internal const string COL_DELTOADDRESS3 = "DeliverToAddress3";
        internal const string COL_DELIVERTOSTR = "DeliverToStr";
        internal const string COL_DOCUMENTCODE = "DocumentCode";
        internal const string COL_LASTMODIFIEDON = "LastModifiedOn";
        internal const string COL_NUMBER = "Number";
        internal const string COL_OBOID = "OBOId";
        internal const string COL_OBO_FNAME = "OBOFirstName";
        internal const string COL_OBO_LNAME = "OBOLastName";
        internal const string COL_SHIPTOID = "ShipToId";
        internal const string COL_SHIPTOADDRESS1 = "ShipToAddress1";
        internal const string COL_SHIPTOADDRESS2 = "ShipToAddress2";
        internal const string COL_SHIPTOADDRESS3 = "ShipToAddress3";
        internal const string COL_SHIPTOCITY = "ShipToCity";
        internal const string COL_SHIPTOZIP = "ShipToZip";
        internal const string COL_SHIPTOSTATE = "ShipToState";
        internal const string COL_SHIPTOCOUNTRY = "ShipToCountry";
        internal const string COL_SHIPTOCOUNTRYCODE = "ShipToCountryCode";
        internal const string COL_ORDLOCADDRESS1 = "OrderLocAddress1";
        internal const string COL_ORDLOCADDRESS2 = "OrderLocAddress2";
        internal const string COL_ORDLOCADDRESS3 = "OrderLocAddress3";
        internal const string COL_ORDLOCCITY = "OrderLocCity";
        internal const string COL_ORDLOCZIP = "OrderLocZip";
        internal const string COL_ORDLOCSTATE = "OrderLocState";
        internal const string COL_ORDLOCCOUNTRY = "OrderLocCountry";
        internal const string COL_SHIPLOCADDRESS1 = "ShipFromLocAddress1";
        internal const string COL_SHIPLOCADDRESS2 = "ShipFromLocAddress2";
        internal const string COL_SHIPLOCADDRESS3 = "ShipFromLocAddress3";
        internal const string COL_SHIPLOCCITY = "ShipFromLocCity";
        internal const string COL_SHIPLOCZIP = "ShipFromLocZip";
        internal const string COL_SHIPLOCSTATE = "ShipFromLocState";
        internal const string COL_SHIPLOCCOUNTRY = "ShipFromLocCountry";
        internal const string COL_SOURCEID = "SourceId";
        internal const string COL_WORKORDER = "WorkOrder";
        internal const string COL_HEADERENTITYID = "HeaderEntityId";
        internal const string COL_ENTITYNAME = "EntityName";
        internal const string COL_LASTMODIFIEDBYID = "LastModifiedById";
        internal const string COL_LASTMODIFIEDBYFNAME = "LastModifiedByFirstName";
        internal const string COL_LASTMODIFIEDBYLNAME = "LastModifiedByLastName";
        internal const string COL_MANUFACTURER = "Manufacturer";
        internal const string COL_NEEDBYDATE = "NeedByDate";
        internal const string COL_OTHERCHARGES = "OtherCharges";
        internal const string COL_PARTNERINTERFACECODE = "PartnerInterfaceCode";
        internal const string COL_REQUESTEDDATE = "RequestedDate";
        internal const string COL_TAXPERCENT = "TaxPercent";
        internal const string COL_TAXTYPEID = "TaxTypeId";
        internal const string COL_FIELDCONFIGID = "FieldConfigId";
        internal const string COL_REQSPLITITEMENTITYID = "RequisitionSplitItemEntityId";
        internal const string COL_GLNAME = "GLName";
        internal const string COL_ACCESSTYPE = "AccessType";
        internal const string COL_OBJECTID = "ObjectId";
        internal const string COL_OBJECTTYPE = "ObjectType";
        internal const string COL_COMMENTID = "CommentId";
        internal const string COL_FILEURL = "FileURL";

        internal const string COL_REQUISITION_ID = "RequisitionID";
        internal const string COL_REQUISITION_NAME = "RequisitionName";
        internal const string COL_REQUISITION_NUMBER = "RequisitionNumber";
        internal const string COL_REQUESTER_ID = "RequesterID";
        internal const string COL_REQUISITIONIDS = "RequisitionIDs";
        internal const string COL_LONGITUDE = "Longitude";
        internal const string COL_LATITUDE = "Latitude";
        internal const string COL_TAXJURISDICTION = "TaxJurisdiction";
        internal const string COL_ISADHOCSHIPTOLOCATION = "IsAdhocShipToLocation";

        internal const string COL_ITEMSPECIFICATION = "Itemspecification";
        internal const string COL_INTERNALPLANTMEMO = "InternalPlantMemo";
        internal const string COL_ITEMSTATUS = "ItemStatus";

        internal const string COL_ALLOWADVANCES = "AllowAdvances";
        internal const string COL_ADVANCEPERCENTAGE = "AdvancePercentage";
        internal const string COL_ADVANCEAMOUNT = "AdvanceAmount";
        internal const string COL_ADVANCERELEASEDATE = "AdvanceReleaseDate";
        internal const string COL_ISADVANCEREQUISITION = "IsAdvanceRequsition";
        internal const string COL_ExtendedStatus = "ExtendedStatus";
        internal const string COL_ITEMMASTERID = "ItemMasterId";
        internal const string COL_ISPREFERREDSUPPLIER = "IsPreferredSupplier";
        internal const string COL_ALLOWFLEXIBLEPRICE = "AllowFlexiblePrice";
        internal const string COL_CONTRACTREFERENCE = "ContractReference";

        internal const string COL_BUID = "BUID";
        internal const string COL_BUSINESSUNITNAME = "BusinessUnitName";
        internal const string COL_REQUISITION_STATUS = "RequisitionStatus";
        internal const string COL_REQUISITION_AMOUNT = "RequisitionAmount";
        internal const string COL_REQUISITION_ITEM_TOTAL = "RequisitionItemTotal";
        internal const string COL_REQUISITION_TAX = "RequisitionTax";
        internal const string COL_REQUISITION_TRNMODE = "TrasmissionMode";
        internal const string COL_REQUISITION_TRNVALUE = "TransmissionValue";
        internal const string COL_SPLITAMOUNT = "SplitAmount";

        internal const string COL_REQUESTER_NAME = "RequesterName";
        internal const string COL_APPROVE_URL = "ApproveURL";
        internal const string COL_REJECT_URL = "RejectURL";
        internal const string COL_TOTAL_RECORDS = "TotalRecords";
        internal const string COL_DOCUMENT_TYPE = "DocumentType";
        internal const string COL_DOCUMENT_CREATED = "DateCreated";

        internal const string COL_APPROVER_ID = "ApproverId";
        internal const string COL_PROXYAPPROVERID = "ProxyApproverId";
        internal const string COL_APPROVALTYPE = "ApprovalType";
        internal const string COL_APPROVER_TYPE = "ApproverType";
        internal const string COL_TRACK_DATE = "StatusDate";
        internal const string COL_TRACK_STATUS = "ApprovalStatus";
        internal const string COL_TRACK_ISDELETED = "IsDeleted";
        internal const string COL_REQUISITION_INSTANCEID = "InstanceID";
        internal const string COL_REQUISITION_SOURCE = "RequisitionSource";
        internal const string COL_REQSTATUS = "ReqStatus";
        internal const string COL_ERROR_MESSAGE = "ErrorMessage";
        internal const string COL_REQENTITYID = "ReqEntityId";
        internal const string COL_PARENTDOCUMENTITEMTOTAL = "ParentDocumentItemTotal";

        internal const string COL_ONBEHALFOF = "OnBehalfOf";
        internal const string COL_CostApprover = "CostApprover";
        internal const string COL_OnHeldBy = "OnHeldBy";
        internal const string COL_ONBEHALFOFNAME = "OnBehalfOfName";
        internal const string COL_ONBEHALFOFEMAIL = "OnBehalfOfEmail";
        internal const string COL_IS_CATALOG_ITEMS_EXISTS = "IsCatalogItemsExists";
        internal const string COL_IS_PUNCHOUT_ITEMS_EXISTS = "IsPunchoutItemExists";
        internal const string COL_REQUESTER_EMAIL_ID = "RequesterEmailID";
        internal const string COL_CATALOGITEMPARTNERID = "CatalogItemPartnerId";
        internal const string COL_LEGALCOMPANYNAME = "LegalCompanyName";
        internal const string COL_CATALOGITEMID = "CatalogItemId";

        internal const string COL_NOOFSENDFORBIDDDINGITEMS = "SentforBiddingItems";
        internal const string COL_NOOFPARTIALLYORDEREDITEMS = "PartiallyOrderedItems";
        internal const string COL_NOOFFULLYORDEREDITEMS = "FullyOrderedItems";
        internal const string COL_NOOFUNORDEREDITEMS = "UnOrderedItems";
        internal const string COL_NOOFNONSUPPORTEDITEMS = "NonSupportedItems";
        internal const string COL_REQUISITIONTOTALCHANGE = "RequisitionTotalChange";
        internal const string COL_CO_REQUESTER_ID = "CORequesterID";
        internal const string COL_CO_REQUESTER_NAME = "CORequesterName";
        internal const string COL_CO_REQUESTER_EMAIL_ID = "CORequesterEmailID";
        internal const string COL_BUDGETORYSTATUS = "BudgetoryStatus";
        internal const string COL_REQ_UPLOADDETAIL_ID = "RequisitionUploadLogID";

        internal const string COL_BUYERASSIGNEE = "BuyerAssignee";
        internal const string COL_BUYERASSIGNEENAME = "buyerAssigneeName";
        internal const string COL_REQ_PROCESSEDXMLRESULT = "ProcessedXMLResult";
        internal const string COL_REQ_REQUESTTYPE = "RequestType";
        internal const string COL_REQ_ERROR = "ERROR";
        internal const string COL_REQ_ERRORTRACE = "ERRORTRACE";
        internal const string COL_VENDORNUMBER = "VendorNumber";
        internal const string COL_ISOBOMANAGER = "IsOBOManager";

        internal const string COL_VALUE = "Value";
        internal const string COL_RISKSCORE = "RiskScore";
        internal const string COL_RISKCATEGORY = "RiskCategory";
        internal const string COL_GROUPID = "GroupId";

        internal const string COL_PARTNERCOUNT = "PartnerCount";
        internal const string COL_REQUISITIONPREVIOUSAMOUNT = "RequisitionPreviousAmount";
        internal const int COL_REQ_DOCUMENTTYPECODE = 7;
        internal const string COL_CONTACT_DEFAULT_ROLENAME = "ContactDefaultRoleName";

        internal const string COL_INTERFACE_STATUS_ID = "InterfaceStatusId";
        #endregion

        #region Requisition StoreProcedures
        internal const string usp_P2P_INV_GetDocumentBasicDetailsDocumentnumber = "usp_P2P_PO_GetDocumentBasicDetailsDocumentnumberForInvoice";
        internal const string USP_P2P_REQ_DELETELINEITEMS = "usp_P2P_REQ_DeleteLineItems";
        internal const string USP_P2P_REQ_SAVEREQUISITIONUPLOADLOG = "usp_P2P_REQ_SaveRequisitionUploadLog";
        internal const string USP_P2P_REQ_GETREQUISITIONUPLOADLOG = "usp_P2P_REQ_GetRequisitionUploadLog";

        internal const string USP_P2P_REQ_PRORATELINEITEMTAXANDSHIPPING = "usp_P2P_REQ_ProrateLineItemTaxandShipping";
        internal const string USP_P2P_PO_PRORATELINEITEMTAXANDSHIPPING = "usp_P2P_PO_ProrateLineItemTaxandShipping";

        internal const string USP_P2P_GETALLCUSTOMATTRIBSBYDOCUMENTCODE = "usp_P2P_GetAllCustomAttribsByDocumentCode";
        internal const string USP_P2P_REQ_GETALLDISPLAYDETAILS = "usp_P2P_REQ_GetAllDisplayDetails";
        internal const string USP_P2P_REQ_GETALLREQUISITIONSITEMSINFO = "usp_P2P_REQ_GetAllRequisitionsItemsInfo";
        internal const string USP_P2P_REQ_GETREQUISITIONLINEITEMSBYREQUISITIONID = "usp_P2P_REQ_GetRequisitionLineItemsByRequisitionId";
        internal const string USP_P2P_REQ_UPDATEREQUSITIONSTATUSBYID = "usp_P2P_REQ_UpdateRequsitionStatusById";
        internal const string USP_P2P_REQ_SAVEREQUISITIONAPPROVERDETAILS = "usp_P2P_REQ_SaveRequisitionApproverDetails";
        internal const string USP_P2P_REQ_SAVEREQUISITIONTRACKSTATUSDETAILS = "usp_P2P_REQ_SaveRequisitionTrackStatusDetails";
        internal const string USP_P2P_REQ_GETP2PLANDINGPAGE = "usp_P2P_REQ_GetP2PLandingPage";
        internal const string USP_P2P_REQ_GETREQUISITIONBASICDETAILSBYID = "usp_P2P_REQ_GetRequisitionBasicDetailsById";
        internal const string USP_P2P_REQ_GETREQUISITIONENTITYDETAILSBYID = "usp_P2P_REQ_GetRequisitionEntityDetailsById";
        internal const string USP_P2P_REQ_GENERATEDEFAULTREQUISITIONNAME = "usp_P2P_REQ_GenerateDefaultRequisitionName";
        internal const string USP_P2P_REQ_SAVEREQUISITION = "usp_P2P_REQ_SaveRequisition";
        internal const string USP_P2P_REQ_SAVEREQUISITIONITEM = "usp_P2P_REQ_SaveRequisitionItem";
        internal const string USP_P2P_REQ_SAVEREQITEMADDITIONALDETAILS = "usp_P2P_REQ_SaveReqItemsAdditionalDetails";
        internal const string USP_P2P_REQ_SAVEREQADDITIONALENTITYDETAILS = "usp_P2P_REQ_SaveReqAdditionalEntityDetails";
        internal const string USP_P2P_REQ_GETALLPARTNERSBYID = "usp_P2P_REQ_GetAllPartnersById";
        internal const string USP_P2P_REQ_SAVEREQUISITIONITEMPARTNERS = "usp_P2P_REQ_SaveRequisitionItemPartners";
        internal const string USP_P2P_REQ_SAVEREQUISITIONITEMSHIPPING = "usp_P2P_REQ_SaveRequisitionItemShippingDetails";
        internal const string USP_P2P_REQ_SAVEREQUISITIONITEMOTHER = "usp_P2P_REQ_SaveRequisitionItemOthers";
        internal const string USP_P2P_REQ_GETTRACKSTATUSDETAILBYID = "usp_P2P_REQ_GetTrackStatusDetailsByID";
        // internal const string USP_P2P_REQ_GETDOCUMENTCODE_BY_DOCUMENTID = "usp_P2P_REQ_GetDocumentCodeByDocumentId";
        internal const string USP_P2P_VALIDATEBUDGETACCOUNTINGDETAILS = "usp_P2P_ValidateBudgetAccountingDetails";
        internal const string USP_P2P_VALIDATE_BY_DOCUMENT_CODE = "usp_P2P_ValidateByDocumentCode";
        internal const string USP_P2P_REQ_GETDOCUMENTEXTENDEDSTATUS = "usp_P2P_REQ_GetDocumentExtendedStatus";
        internal const string USP_P2P_REQ_UPDATEREQUISITIONSTATUSBYORDERITEMID = "usp_P2P_REQ_UpdateRequisitionStatusByOrderItemId";
        internal const string USP_P2P_PO_CALCULATEREQSTATUSBYORDERID = "usp_P2P_PO_CalculateReqStatusByOrderId";
        internal const string USP_P2P_PO_DELETEORDERREQMAPPING = "usp_P2P_PO_DeleteOrderReqMapping";
        internal const string USP_P2P_REQ_SAVEBUSINESSUNIT = "usp_P2P_REQ_SaveBusinessUnit";
        internal const string USP_P2P_REQ_UPDATEREQUISITIONEXTENDEDSTATUS = "usp_P2P_REQ_UpdateRequisitionExtendedStatus";
        internal const string USP_P2P_REQ_GETBUSINESSUNIT = "usp_P2P_REQ_GetBusinessUnit";
        internal const string USP_P2P_REQ_GETALLREQUISITIONFORLEFTPANEL = "usp_P2P_REQ_GetAllRequisitionForLeftPanel";
        internal const string USP_P2P_REQ_ADDTEMPLATEITEMSINREQ = "usp_P2P_REQ_AddTemplateItemsInReq";
        internal const string USP_P2P_GETREQUISITIONADDITIONALDETAILS = "usp_P2P_GetRequisitionAdditionalDetails";
        internal const string USP_P2P_GETORDERADDITIONALDETAILS = "usp_P2P_GetOrderAdditionalDetails";
        internal const string USP_P2P_REQ_GETREQUISITIONACCOUNTINGDETAILSBYITEMID = "USP_P2P_REQ_GetRequisitionAccountingDetailsByItemId";
        internal const string USP_P2P_REQ_SAVEREQUISITIONACCOUNTINGDETAILS = "usp_P2P_REQ_SaveRequisitionAccountingDetails";
        internal const string USP_P2P_REQ_SAVEREQUISITIONACCOUNTINGDETAILSV2 = "usp_P2P_REQ_SaveRequisitionAccountingDetailsV2";
        internal const string USP_P2P_GETINVOICEADDITIONALDETAILS = "usp_P2P_GetInvoiceAdditionalDetails";
        internal const string USP_P2P_GETRECEIPTADDITIONALDETAILS = "usp_P2P_GetReceiptAdditionalDetails";
        internal const string USP_P2P_INV_SAVEINVOICEACCOUNTINGDETAILS = "usp_P2P_INV_SaveInvoiceAccountingDetails";
        internal const string USP_P2P_GETALLSPLITACCOUNTINGFIELDSWITHDEFAULTVALUES = "usp_P2P_GetAllSplitAccountingFieldsWithDefaultValues";
        internal const string USP_P2P_REQ_GETREQUISITIONADVPAYMENTACCOUNTINGDETAILSBYITEMID = "USP_P2P_REQ_GetRequisitionAdvPaymentAccountingDetailsByItemId";
        // internal const string USP_P2P_REQ_GETDOCUMENTIDBYDOCUMENTCODE = "usp_P2P_REQ_GetDocumentIdByDocumentCode";
        internal const string USP_P2P_REQ_GETDOCUMENTIDBYDOCUMENTCODE = "usp_P2P_REQ_GetDocumentIdByDocumentCode";
        internal const string USP_P2P_GETIRADDITIONALDETAILS = "usp_P2P_GetIRAdditionalDetails";
        internal const string USP_P2P_REQ_COPYREQUISITIONTOREQUISITION = "usp_P2P_REQ_CopyRequisitiontoRequisition";
        // internal const string USP_P2P_REQ_GETREQUISITIONLINEITEMSBYDOCUMENTCODE = "usp_P2P_REQ_GetRequisitionLineItemsByDocumentCode";
        internal const string USP_P2P_CHECKVIEWACCESSOFENTITY = "usp_P2P_CheckViewAccessOfEntity";
        internal const string USP_P2P_REQ_GETALLCATEGORIESBYREQID = "usp_P2P_REQ_GetAllCategoriesByReqId";
        internal const string USP_P2P_PR_GETALLCATEGORIESBYPAYMENTREQUESTID = "usp_P2P_PR_GetAllCategoriesByPaymentRequestId";
        internal const string USP_P2P_REQ_GETREQUISITION_ENTITIES = "usp_P2P_REQ_GetRequisitionEntities";
        internal const string USP_P2P_REQ_UPDATETAXONHEADERSHIPTO = "usp_P2P_REQ_UpdateTaxOnHeaderShipTo";
        internal const string USP_P2P_REQ_CALCULATE_AND_UPDATELINEITEMTAX = "usp_P2P_REQ_CalculateAndUpdateLineItemTax";
        internal const string USP_P2P_REQ_DELETEALLSPLITSBYDOCUMENTID = "usp_P2P_REQ_DeleteAllSplitsByDocumentId";
        internal const string USP_P2P_REQ_DELETESPLITSBYITEMID = "usp_P2P_REQ_DeleteSplitsByItemId";
        internal const string USP_P2P_PO_GETORDERDETAILSFOREXPORTPDFBYID = "usp_P2P_PO_GetOrderDetailsForExportPDFById";
        internal const string USP_P2P_REQ_GETOBOUSERBYDOCUMENTCODE = "usp_P2P_REQ_GetOBOUserByDocumentCode";
        internal const string USP_P2P_REQ_GETREQUISITIONBYIDFORNOTIFICATION = "usp_P2P_REQ_GetRequisitionByIdForNotification";
        internal const string USP_P2P_REQ_GETVALIDATIONERRORCODEBYID = "usp_P2P_REQ_GetValidationErrorCodeById";
        internal const string USP_P2P_REQ_SAVEREQUISITIONACCOUNTINGAPPLYTOALL = "usp_P2P_REQ_SaveRequisitionAccountingApplyToAll";
        internal const string USP_P2P_REQ_PRORATEHEADERTAXANDSHIPPING = "usp_P2P_REQ_ProrateHeaderTaxAndShipping";
        internal const string USP_P2P_REQ_GETDOCUMENTBASICDETAILSDOCUMENTNUMBER = "usp_P2P_REQ_GetDocumentBasicDetailsDocumentnumber";
        internal const string USP_P2P_REQ_UPDATEREQITEMONPARTNERCHANGE = "usp_P2P_REQ_UpdateReqItemOnPartnerChange";
        internal const string USP_P2P_REQ_GETPREFERREDPARTNERBYREQITEMID = "usp_P2P_REQ_getPreferredPartnerByReqItemId";
        internal const string USP_P2P_REQ_GETPREFERREDPARTNERBYCATALOGITEMID = "usp_P2P_REQ_getPreferredPartnerByCatalogItemId";
        internal const string USP_P2P_REQ_GETALLITEMIDSBYREQID = "usp_P2P_REQ_GetAllItemIdsByReqId";
        internal const string USP_P2P_REQ_REQUISITIONCATALOGITEMACCESS = "usp_P2P_REQ_RequisitionCatalogItemAccess";
        internal const string USP_P2P_REQ_REQUISITIONOBOUSERCATALOGITEMACCESS = "usp_P2P_REQ_RequisitionOBOUserCatalogItemAccess";
        internal const string USP_P2P_REQ_UPDATEBILLTOLOCATION = "usp_P2P_REQ_UpdateBillToLocation";
        internal const string USP_P2P_REQ_GETALLREQUISITIONDETAILSBYREQUISITIONID = "usp_P2P_REQ_GetAllRequisitionDetailsByRequisitionId";
        internal const string USP_P2P_GETAPPROVEDREJECTEDDOCSINFO = "usp_P2P_GetApprovedRejectedDocsInfo";
        internal const string USP_P2P_REQ_GETREQUISITIONCAPITALCODECOUNTBYID = "usp_P2P_REQ_GetRequisitionCapitalCodeCountById";
        internal const string USP_P2P_REQ_VALIDATEINTERFACEDOCUMENT = "usp_P2P_REQ_ValidateInterfaceDocument";
        internal const string USP_P2P_INV_UPDATERELEVANTIRFORINVOICE = "USP_P2P_INV_UpdateRelevantIRForInvoice";
        internal const string USP_P2P_PO_GETREQUISITIONDETAILSFOREXPORTPDFBYID = "usp_P2P_REQ_GetRequisitionDetailsForExportPDFById";
        internal const string USP_P2P_GETACESSCONTROLENTITYFROMSPLITS = "usp_P2P_GetAcessControlEntityFromSplits";
        internal const string USP_P2P_REQ_UPDATELINETYPEBYPURCHASETYPE = "usp_P2P_REQ_UpdateLineTypeByPurchaseType";
        internal const string USP_P2P_REQ_SAVEADVANCEDPAYMENTITEM = "usp_P2P_REQ_SaveAdvancedPaymentItem";
        internal const string USP_P2P_REQ_GETREQHEADERDETAILSBYID_FOR_INTERFACE = "usp_P2P_REQ_GetHeaderDetailsByIdForInterface";
        internal const string USP_P2P_REQ_GETREQLINEITEMS_FOR_INTERFACE = "usp_P2P_REQ_GetLineItemsForInterface";
        internal const string USP_P2P_REQ_UPDATEINTERFACESTATUS = "usp_P2P_REQ_UpdateInterfaceStatus";
        internal const string usp_DM_GetAutoSaveDocument = "usp_DM_GetAutoSaveDocument";
        internal const string usp_DM_AutoSaveDocument = "usp_DM_AutoSaveDocument";
        internal const string usp_DM_DeleteAutoSaveDocument = "usp_DM_DeleteAutoSaveDocument";
        internal const string USP_P2P_REQ_GETREQUISITIONDETAILSFORRFXTOPO = "usp_P2P_REQ_GetRequisitionDetailsForRFXToPO";
        internal const string USP_P2P_GetContractItemsByContractNumber = "USP_P2P_GetContractItemsByContractNumber";
        internal const string USP_P2P_REQ_ValidateContractNumber = "USP_P2P_REQ_ValidateContractNumber";
        internal const string USP_P2P_REQ_GETSAVEDVIEWS_WORKBENCH = "usp_P2P_GetSavedViews";
        internal const string USP_P2P_REQ_INSERTUPDATESAVEDVIEWS_WORKBENCH = "usp_P2P_InsertUpdateSavedViewInfo";
        internal const string USP_P2P_REQ_DELETEAVEDVIEWS_WORKBENCH = "usp_p2p_deletesavedviewinfo";
        internal const string USP_P2P_REQ_UPDATEREQUISITIONITEMAUTOSOURCEPROCESSFLAG = "USP_P2P_REQ_UpdateRequisitionItemAutoSourceProcessFlag";
        internal const string USP_P2P_PO_SAVEREQUISITIONORDERMAPPING = "USP_P2P_PO_SaveRequisitionOrderMapping";
        internal const string USP_P2P_REQ_SAVEREQUISITIONNOTESORATTACHMENTS = "usp_P2P_REQ_SaveRequisitionNotesOrAttachments";
        internal const string USP_P2P_REQ_GETREQUISITIONNOTESORATTACHMENTS = "usp_P2P_REQ_GetRequisitionNotesOrAttachments";
        internal const string USP_P2P_REQ_DELETEREQUISITIONNOTESORATTACHMENTS = "usp_P2P_REQ_DeleteRequisitionNotesOrAttachments";
        internal const string USP_P2P_GETCATEGORYTYPES = "usp_p2p_getcategorytypes";
        internal const string USP_P2P_REQ_ASSIGNBUYERTOREQUISITIONITEMS = "usp_P2P_REQ_AssignBuyerToRequisitionItems";
        internal const string USP_P2P_REQ_GETLISTOFASSIGNBUYERS = "usp_REQ_GetListofAssignBuyers";
        internal const string USP_P2P_REQ_GETLINEITEMSFORWORKBENCH = "usp_P2P_REQ_GetLineItemsForWorkBench";
        internal const string USP_P2P_REQ_PRORATETAX = "usp_P2P_REQ_ProrateTax";
        internal const string USP_P2P_INV_GETREQUISITIONHEADERTAXES = "usp_P2P_REQ_GetRequisitionHeaderTaxes";
        internal const string USP_P2P_INV_UPDATEREQUISITIONHEADERTAXES = "usp_P2P_REQ_UpdateRequisitionHeaderTaxes";
        internal const string USP_P2P_REQ_VALIDATEBLANKETITEMNUMBER = "USP_P2P_REQ_ValidateBlanketItemNumber";

        internal const string USP_P2P_REQ_UPDATEBUYERTOREQUISITIONITEMS = "USP_P2P_REQ_UpdateBuyerToRequisitionItems";
        internal const string USP_P2P_REQ_GETPOITEMSFORWORKBENCH = "usp_P2P_REQ_GetPOItemsForWorkBench";
        internal const string USP_P2P_REQ_CHANGEREQUISITIONREQUEST = "usp_P2P_REQ_ChangeRequisitionRequest";
        internal const string USP_P2P_REQ_UPDATERFXANDPOMAPPING = "usp_P2P_REQ_UpdateRfxAndPoMapping";
        internal const string USP_P2P_REQ_GETREVISIONNUMBERBYDOCUMENTCODE = "usp_P2P_REQ_GetRevisionNumberByDocumentCode";
        internal const string USP_P2P_COPYREQUISITIONEXISTINGTONEW = "usp_P2P_CopyRequisitionExistingToNew";
        internal const string USP_P2P_REQ_REVOKECHANGEREQUISITION = "usp_P2P_REQ_RevokeChangeRequisition";
        internal const string USP_P2P_REQ_VALIDATEREQWORKBENCHITEMS = "USP_P2P_REQ_ValidateReqWorkbenchItems";
        internal const string USP_P2P_REQ_SAVEREQUISITIONCHARGEACCOUNTINGDETAILS = "usp_P2P_REQ_SaveRequisitionChargeAccountingDetails";
        internal const string USP_P2P_REQ_GETLINEITEMCHARGES = "usp_P2P_REQ_GetLineItemCharges";
        internal const string USP_P2P_DELETEREQUISITIONTEMCHARGE = "usp_P2P_DeleteRequisitiontemCharge";
        internal const string USP_P2P_REQ_SAVEREQUISITIONCHARGEDEFAULTACCOUNTING = "usp_P2P_REQ_SaveRequisitionChargeDefaultAccounting";
        internal const string USP_P2P_REQ_SYNCCHANGEREQUISITION = "usp_P2P_REQ_SyncChangeRequisition";
        internal const string USP_P2P_REQ_WITHDRAWCHANGEREQUISITION = "USP_P2P_REQ_WithDrawChangeRequisition";
        internal const string USP_P2P_REQ_CheckBiddingInProgress = "usp_P2P_REQ_CheckBiddingInProgress";
        internal const string USP_P2P_REQ_VALIDATEDOCUMENTBEFORENEXTACTION = "usp_P2p_Req_ValidateDocumentBeforeNextAction";
        internal const string USP_P2P_REQ_SAVEBUYERASSIGNEE = "usp_p2p_Req_SaveBuyerAssignee";
        internal const string USP_P2P_REQ_SAVEREQUISITIONDEATILS = "usp_P2P_REQ_SaveRequisitionDetails";
        internal const string USP_P2P_REQ_SAVEREQUISITIONENTITYDETAILS = "usp_P2P_REQ_SaveRequisitionEntityDetails";
        internal const string USP_P2P_REQ_SAVEREQITEMSHIPPING = "usp_P2P_REQ_SaveReqItemShippingDetails";
        internal const string USP_P2P_REQ_GETREQUISITIONDETAILSFOREXTERNALWORKFLOWPROCESS = "usp_P2P_REQ_GetRequisitionDetailsForExternalWorkFlowProcess";
        internal const string USP_P2P_VALIDATEITEMDETAILSTOBEDERIVEDFROMINTERFACE = "usp_P2P_ValidateItemDetailsToBeDerivedFromInterface";
        internal const string USP_P2P_DERIVEITEMDETAILS = "usp_P2P_DeriveItemDetails";
        internal const string USP_P2P_SAVEFLIPPEDQUESTIONSRESPONSES = "usp_P2P_SaveFlippedQuestionResponses";
        internal const string USP_P2P_GETACCOUNTINGDATABYDOCUMENTCODES = "usp_P2P_GetAccountingDataByDocumentCodes";
        internal const string usp_P2P_REQ_UpdateRequsitionItemStatusFromInterface = "usp_P2P_REQ_UpdateRequsitionItemStatusFromInterface";
        internal const string USP_P2P_REQ_UpdateRequisitionItemFlipType = "USP_P2P_REQ_UpdateRequisitionItemFlipType";
        internal const string USP_P2P_REQ_ResetRequisitionItemFlipType = "USP_P2P_REQ_ResetRequisitionItemFlipType";
        internal const string USP_P2P_REQ_CheckWithDrawApprovedRequisition = "usp_P2P_REQ_CheckWithDrawApprovedRequisition";
        internal const string USP_P2P_REQ_GETREQUISIONSFORINTERFACE = "usp_P2P_REQ_GetRequisitionForInterface";
        internal const string USP_P2P_REQ_VALIDATEINTERFACELINESTATUS = "USP_P2P_REQ_ValidateInterfaceLineStatus";
        internal const string USP_P2P_REQ_UPDATEINTERFACEREQUSITIONITEMSTATUS = "usp_P2P_REQ_UpdateInterfaceRequsitionItemStatus";
        internal const string USP_P2P_REQ_SAVEORDERREQUISITIONSMAPPING = "usp_P2P_REQ_SaveOrderRequisitionsMapping";
        internal const string USP_P2P_REQ_ValidateRequisitionData = "usp_P2P_REQ_ValidateRequisitionData";
        internal const string USP_P2P_REQ_VALIDATEERRORBASEDINTERFACEDOCUMENT = "usp_P2P_REQ_ValidateErrorBasedInterfaceDocument";
        internal const string USP_P2P_REQ_GETALLPASCATEGORIES = "usp_P2P_REQ_GetPASCategoryList";
        internal const string USP_P2P_REQ_GetRequisitionDetailsFromCatalog = "usp_P2P_REQ_GetRequisitionDetailsFromCatalog";
        internal const string USP_REQ_GETPARTNERSDOCUMETINTERFACEINFO = "usp_REQ_GetPartnersDocumetInterfaceInfo";
        internal const string USP_P2P_VALIDATEREQITEMSFOREXCEPTIONHANDLING = "usp_P2P_ValidateReqItemsForExceptionHandling";
        internal const string USP_PRN_GETUSERSBASEDONUSERDETAILSFORPAGINATION = "usp_PRN_GetUsersBasedOnUserDetailsForPagination";
        internal const string USP_P2P_REQ_GETDOCUMENTLOBBYDOCUMENTCODE = "usp_P2P_REQ_GetDocumentLOBByDocumentCode";
        internal const string USP_P2P_REQ_UPDATEREQUSITIONPREVIOUSAMOUNT = "usp_P2P_REQ_UpdateRequsitionPreviousAmount";
        internal const string USP_P2P_REQ_GetOrderingLocationsWithNonOrgEntities = "usp_P2P_REQ_GetOrderingLocationsWithNonOrgEntities";
        internal const string USP_QB_SAVEQUESTIONRESPONSEATTACHMENT = "usp_QB_SaveQuestionResponseAttachment";
        internal const string USP_P2P_GETCATEGORYHIRARCHYFORDEFAULTCATEGORY = "usp_P2P_GetCategoryHirarchyForDefaultCategory";
        internal const string USP_P2P_REQ_GETALLBUYERSUPPLIERSAUTOSUGGEST = "usp_P2P_REQ_GetAllBuyerSuppliersAutoSuggest";
        internal const string USP_P2P_GETALLADDITIONALFIELDVALUES = "usp_P2P_GetAllAdditionalFieldValues";
        internal const string USP_P2P_REQ_SAVEREQUISITIONITEMFIELDS = "usp_P2P_REQ_SaveRequisitionItemFields";
        internal const string USP_P2P_GETADDITIONALFIELDSMASTERDATA = "usp_P2P_GetAdditionalFieldsMasterData";
        internal const string USP_P2P_GETADDITIONALFIELDSDATABYFIELDCONTROLTYPE = "usp_P2P_GetAdditionalFieldsDataByFieldControlType";
        internal const string USP_REQ_UPDATEPRICEDETAILOFCATALOGLINEITEMS = "usp_REQ_UpdatePriceDetailOfCatalogLineItems";
        internal const string USP_P2P_REQ_SAVEREQUISITIONHEADERADDITIONALFIELDS = "usp_P2P_REQ_SaveRequisitionHeaderAdditionalFields";
        internal const string USP_P2P_REQ_SAVEREQUISITIONITEMSINCOTERMDETAILS = "usp_P2P_REQ_SaveRequisitionItemsIncoTermDetails";
        internal const string USP_REQ_GETALLSUPPLIERLOCBYLOBAndOrgEntity = "usp_P2P_REQ_GetAllSupplierLocationsByOrgEntity";
        internal const string USP_P2P_REQ_UPDATEMUTIPLEREQUISITIONITEMSSTATUSES = "usp_P2P_REQ_UpdateMutipleRequisitionItemsStatuses";
        internal const string USP_P2P_GETALLADDITIONALFIELDDATA = "usp_P2P_GetAllAdditionalFieldData";
        internal const string USP_P2P_REQ_UPDATEREQUISITIONITEMTAXJURISDICTION = "USP_P2P_REQ_UpdateRequisitionItemTaxJurisdiction";
        internal const string USP_P2P_REQ_GETUSERBASEDONENTITYDETAILSCODE = "usp_P2P_REQ_GetUsersBasedOnEntityDetailsCode";
        internal const string USP_P2P_REQ_UpdateREQUSITIONITEMSTATUS = "usp_P2P_REQ_UpdateRequsitionItemStatus";
        internal const string usp_P2P_REQ_GetRequisitionCurrency = "usp_P2P_REQ_GetRequisitionCurrency";
        internal const string USP_P2P_REQ_GETP2PLINEITEMIDBYPARTENRANDCURRENCYCODE = "usp_P2P_REQ_GetP2PLineitemIdByPartnerAndCurrencyCode";
        internal const string USP_P2P_REQ_GETENTITYDETAILSBYSEARCHRESULTS = "usp_P2P_REQ_GetEntityDetailsBySearchResults";
        #endregion
        #region Notes And Attachements
        internal const string COL_NOTES_ATTACH_ID = "NotesOrAttachmentId";
        internal const string COL_NOTES_ATTACH_DESC = "NoteOrAttachmentDescription";
        internal const string COL_NOTES_ATTACH_NAME = "NoteOrAttachmentName";
        internal const string COL_NOTES_ATTACH_TYPE = "NoteOrAttachmentType";
        internal const string COL_NOTES_ATTACH_ACCESSTYPE = "AccessTypeId";
        internal const string COL_NOTES_ATTACH_MODIFIEDDATE = "ModifiedDate";
        internal const string COL_NOTES_ATTACH_ITEMID = "RequisitionItemId";
        internal const string COL_NOTES_ATTACH_REQID = "RequisitionId";
        internal const string COL_NOTES_ATTACH_CATEGORYTYPEID = "CategoryTypeId";
        internal const string COL_NOTES_ATTACH_CAT_DESC = "CategoryTypeDescription";
        internal const string COL_NOTES_ATTACH_CAT_ID = "CategoryTypeId";
        internal const string COL_NOTES_ATTACH_FILESIZE = "FileSize";
        internal const string COL_MAX_FILE_SIZE_IN_MB = "MaxFileSizeInMB";
        internal const string COL_FILE_TYPE_FILTER = "FileTypeFilter";
        internal const string COL_DOES_VALIDATE_TEXT = "DoesValidateText";
        internal const string COL_TEXT_FORMAT_TYPE = "TextFormatType";
        internal const string COL_TEXT_FORMAT_RANGE_MIN = "TextFormatRangeMin";
        internal const string COL_TEXT_FORMAT_RANGE_MAX = "TextFormatRangeMax";
        internal const string COL_TEXT_INVALID_FORMAT_ERROR_MESSAGE = "TextInvalidFormatErrorMessage";
        internal const string COL_TEXT_FORMULA = "Formula";
        #endregion

        #region Requisition LineItem Columns
        internal const string COL_REQUISITION_ITEM_ID = "RequisitionItemID";
        internal const string COL_P2P_LINE_ITEM_ID = "P2PLineItemID";
        internal const string COL_SHORT_NAME = "ShortName";
        internal const string COL_DESCRIPTION = "Description";
        internal const string COL_UNIT_PRICE = "UnitPrice";
        internal const string COL_PERCENTAGE = "Percentage";
        internal const string COL_QUANTITY = "Quantity";
        internal const string COL_UOM = "UOM";
        internal const string COL_STANDARD_UOM = "StandardUOM";
        internal const string COL_UOM_DESC = "UOMDescription";
        internal const string COL_DATE_REQUESTED = "DateRequested";
        internal const string COL_DATE_NEEDED = "DateNeeded";
        internal const string COL_MANUFACTURER_NAME = "ManufacturerName";
        internal const string COL_MANUFACTURER_PART_NUMBER = "ManufacturerPartNumber";
        internal const string COL_CATEGORY_ID = "CategoryID";
        internal const string COL_CATEGORY_NAME = "CategoryName";
        internal const string COL_ITEM_TYPE_ID = "ItemTypeID";
        internal const string COL_ITEM_EXTENDED_TYPE = "ItemExtendedType";
        internal const string COL_EFFORTS = "Efforts";
        internal const string COL_TOTAL_ITEMS_COUNT = "TotalItemCount";
        internal const string COL_LINE_ITEM_TAX = "Tax";
        internal const string COL_INVOICETOTAL_TAX = "InvoiceTotalTax";
        internal const string COL_START_DATE = "StartDate";
        internal const string COL_END_DATE = "EndDate";
        internal const string COL_DOCUMENTDATE = "DocumentProcessDate";
        internal const string COL_TOTAL_AMOUNT = "TotalAmount";
        internal const string COL_ITEM_TOTAL_AMOUNT = "ItemTotalAmount";
        internal const string COL_CREATED_BY_NAME = "CreatedByName";
        internal const string COL_PARTNER_NAME = "PartnerName";
        internal const string COL_Partner_Contact_Name = "PartnerContactName";
        internal const string COL_SOURCE_TYPE = "SourceType";
        internal const string COL_REQITEM_SHIPPING_ID = "ReqLineItemShippingID";
        internal const string COL_ITEM_CODE = "ItemCode";
        internal const string COL_IS_PROCURABLE = "IsProcurable";
        internal const string COL_ITEM_STATUS = "ItemStatus";
        internal const string COL_INVOICING_STATUS = "InvoicingStatus";
        internal const string COL_RECEIVING_STATUS = "ReceivingStatus";
        internal const string COL_ACCOUNTING_STATUS = "AccountingStatus";
        internal const string COL_ORDERENTITYID = "OrderEntityId";
        internal const string COL_BLANKET_DOCUMENTCODE = "BlanketDocumentCode";
        internal const string COL_BLANKET_STARTDATE = "BlanketStartDate";
        internal const string COL_BLANKET_ENDDATE = "BlanketEndDate";
        internal const string COL_BLANKET_AMOUNT = "BlanketValue";
        internal const string COL_RELEASE_ORDER_COUNT = "ReleaseOrderCount";
        internal const string COL_BLANKET_DOCUMENTNUMBER = "BlanketDocumentNumber";
        internal const string COL_CONSUMED_AMOUNT = "ConsumedAmount";
        internal const string COL_PARTNER_STATUSCODE = "PartnerStatusCode";
        internal const string COL_BILLABLE = "Billable";
        internal const string COL_INVENTORYTYPE = "InventoryType";
        internal const string COL_PROCUREMENTSTATUS = "ProcurementStatus";
        internal const string COL_CAPITALIZED = "Capitalized";
        internal const string COL_ORDERTAX = "OrderTax";
        internal const string COL_ORDERSHIPPING = "OrderShipping";
        internal const string COL_ORDERADDITIONALCHARGES = "OrderAdditionalCharges";
        internal const string COL_INVOICEORDERTAXDIFFERENCE = "InvoiceOrderTaxDifference";
        internal const string COL_TAXEXEMPTEDINVOICETOTAL = "TaxexemptionInvoiceTotal";
        internal const string COL_INVOICINGTAX = "InvoicingTax";
        internal const string COL_USEDTAX = "UsedTax";
        internal const string COL_ACCOUNTNUMBER = "AccountNumber";
        internal const string COL_TYPEOFITEM = "TypeOfItem";
        internal const string COL_PARTNERCONTACTNAME = "PartnerContactName";
        internal const string COL_PARTNERCONTACTID = "PartnerContactId";
        internal const string COL_ORDERLOCATIONID = "OrderLocationId";
        internal const string COL_ORDERLOCATIONNAME = "OrderLocationName";
        internal const string COL_ORDERLOCATIONADDRESS = "OrderLocationAddress";
        internal const string COL_ORDERLOCATIONCODE = "OrderLocationCode";
        internal const string COL_SHIPFROMLOCATIONID = "ShipFromLocationId";
        internal const string COL_SHIPFROMLOCATIONNAME = "ShipFromLocationName";
        internal const string COL_SHIPFROMLOCATIONADDRESS = "ShipFromLocationAddress";
        internal const string COL_SHIPFROMLOCATIONCODE = "ShipFromLocationCode";
        internal const string COL_REQITEMTOTAL = "ReqItemTotal";
        internal const string COL_REQTOTAL = "ReqTotal";
        internal const string COL_REQITEMVALUE = "ReqItemValue";
        internal const string COL_PARENTPOITEMTOTAL = "ParentPOItemTotal";
        internal const string COL_PARENTPOITEMVALUE = "ParentPOItemValue";
        internal const string COL_DISPATCHMODE = "DispatchMode";
        internal const string COL_REQ_ITEM_LINENUMBER = "ReqItemLineNumber";
        internal const string COL_PROMISEDDATE = "PromisedDate";
        internal const string COL_ESTIMATEDDELIVERYDATE = "EstimatedDeliveryDate";
        internal const string COL_LEADTIME = "LeadTime";
        internal const string COL_CREATEDON = "CreatedOn";

        internal const string COL_OVERALLITEMLIMIT = "OverallItemLimit";
        internal const string COL_OVERALLLIMITSPLITITEM = "OverallLimitSplitItem";
        internal const string COL_SPLIT_OVERALLSPLITITEM_TOTAL = "OverallLimitSplitTotal";
        internal const string COL_ORDERTOTAL = "OrderItemTotal";
        internal const string COL_ISOVERALLLIMITALLOWED = "IsOverallLimitAllowed";
        internal const string COL_OVERALLLIMIT = "OverAllLimit";
        internal const string COL_BUYERCONTACTCODE = "BuyerContactCode";
        internal const string COL_ERRORSTRING = "ErrorString";
        internal const string COL_ISERSENABLED = "IsERSEnabled";
        internal const string COL_SOURCEQUESTIONID = "SourceQuestionId";
        internal const string COL_TARGETQUESTIONID = "TargetQuestionId";
        internal const string COL_SOURCEDOCUMENTTYPE = "SourceDocumentType";
        internal const string COL_TARGETDOCUMENTTYPE = "TargetDocumentType";
        internal const string COL_SOURCECHOICEID = "SourceChoiceId";
        internal const string COL_TARGETCHOICEID = "TargetChoiceId";
        internal const string COL_ISADDEDFROMREQUISTION = "IsAddedFromRequistion";
        internal const string COL_PREVORDEROVERALLTOTALAMOUNT = "PrevOrderOverallTotalAmount";

        internal const string COL_SHIPFROMLOCADDRESS1 = "ShipFromLocAddress1";
        internal const string COL_SHIPFROMLOCADDRESS2 = "ShipFromLocAddress2";
        internal const string COL_SHIPFROMLOCADDRESS3 = "ShipFromLocAddress3";
        internal const string COL_SHIPFROMLOCCITY = "ShipFromLocCity";
        internal const string COL_SHIPFROMLOCZIP = "ShipFromLocZip";
        internal const string COL_SHIPFROMLOCSTATE = "ShipFromLocState";
        internal const string COL_SHIPFROMLOCCOUNTRY = "ShipFromLocCountry";
        internal const string COL_RFXFlipType = "RFXFlipType";
        internal const string COL_IsTaxUserEdited = "IsTaxUserEdited";
        // Adding of incoterm fields
        internal const string COL_INCOTERMID = "IncoTermId";
        internal const string COL_INCOTERMCODE = "IncoTermCode";
        internal const string COL_INCOTERMLOCATION = "IncoTermLocation";
        internal const string COL_INCOTERMDESCRIPTION = "IncoTermDescription";
        internal const string COL_CONVERSIONFACTOR = "ConversionFactor";
        internal const string COL_CLIENTPASCODE = "ClientPASCode";


        #endregion

        #region Requisition LineItem StoreProcedures
        internal const string USP_P2P_REQ_GETREQUISITIONLINEITEMS = "usp_P2P_REQ_GetRequisitionLineItems";
        internal const string USP_P2P_REQ_UPDATEREQUISITIONSEARCHINDEXKEY = "usp_P2P_REQ_UpdateRequisitionSearchIndexKey";
        internal const string USP_P2P_REQ_GETALLLINEITEMSBYID = "usp_P2P_REQ_GetAllLineItemsById";
        internal const string USP_P2P_REQ_DELETELINEITEMBYID = "usp_P2P_REQ_DeleteLineItemById";
        internal const string USP_P2P_REQ_UPDATEITEMQUANTITY = "usp_P2P_REQ_UpdateItemQuantity";
        internal const string USP_P2P_REQ_GETPARTNERDETAILSBYLIID = "usp_P2P_REQ_GetPartnerDetailsByLiId";
        internal const string USP_P2P_REQ_GETSHIPPINGSPLITDETAILSBYLIID = "usp_P2P_REQ_GetShippingSplitDetailsByLiId";
        internal const string USP_P2P_REQ_GETOTHERITEMDETAILSBYLIID = "usp_P2P_REQ_GetOtherItemDetailsByLiId";
        internal const string USP_P2P_SAVECATALOGREQUISITION = "usp_P2P_SaveCatalogRequisition";
        internal const string USP_P2P_UPDATEPROCUREMENTSTATUSBYREQITEMID = "usp_P2P_REQ_UpdateProcurementStatusByReqItemId";
        internal const string USP_P2P_SAVECATALOGORDER = "usp_P2P_SaveCatalogOrder";
        internal const string USP_P2P_REQ_GETLINEITEMTAXDETAILS = "usp_P2P_REQ_GetLineItemTaxDetails";
        internal const string USP_P2P_REQ_GETREQUISITIONITEMACCOUNTINGSTATUS = "usp_P2P_REQ_GetRequisitionItemAccountingStatus";
        internal const string USP_P2P_REQ_COPYLINEITEM = "usp_P2P_REQ_CopyLineItem";
        internal const string USP_P2P_REQ_UPDATETAXONLINEITEM = "usp_P2P_REQ_UpdateTaxOnLineItem";
        internal const string USP_P2P_REQ_UPDATEORGENTITIESFROMCATALOG = "usp_P2P_REQ_UpdateOrgEntitiesFromCatalog";
        internal const string USP_P2P_REQ_UPDATEORGENTITIESFROMCATALOGBYITEMID = "usp_P2P_REQ_UpdateOrgEntitiesFromCatalogByItemId";
        internal const string USP_P2P_REQ_DELETELINEITEMSBYORGENTITYCODE = "usp_P2P_REQ_DeleteLineItemsByOrgEntityCode";
        internal const string USP_P2P_REQ_DELETELINEITEMSONBUCHANGE = "usp_P2P_REQ_DeleteLineItemsOnBUChange";
        internal const string USP_P2P_REQ_SAVECONTRACTINFORMATION = "usp_P2P_REQ_SaveContractInformation";
        internal const string USP_P2P_DELETELINEITEMSBYEXTENDEDTYPEIDS = "USP_P2P_REQ_DeleteLineItemsByExtendedTypeIds";
        internal const string USP_P2P_REQ_GETALLQUESTIONNAIRE = "usp_P2P_REQ_GetAllQuestionnaire";
        internal const string USP_P2P_PO_GETALLQUESTIONNAIRE = "usp_P2P_PO_GetAllQuestionnaire";
        internal const string USP_P2P_UPDATEREQLINESTATUSONRFXCREATEORUPDATE = "usp_P2P_REQ_UpdateReqLineStatusonRFXCreateorUpdate";
        internal const string USP_P2P_REQ_SAVEITEM = "usp_P2P_SaveRequisitionItem";
        internal const string USP_P2P_REQ_SAVESRFQUESTIONSFORREQITEMID = "usp_P2P_REQ_SaveSRFQuestionsforReqItemId";
        internal const string USP_P2P_REQ_GETCUTSOMATTRIBUTESFORLINES = "usp_P2P_REQ_GetCutsomAttributesForLines";
        internal const string USP_P2P_REQ_GETORDERSFORREQUISITIONLINE = "usp_P2P_REQ_GetOrdersForRequisitionLine";
        internal const string USP_P2P_REQ_GETAllAuthorsAndRequestersForFilters = "USP_P2P_REQ_GetAllAuthorsAndRequestersForFilters";
        internal const string USP_P2P_REQ_GetDocumentDetailsBySelectedReqWorkbenchItems = "USP_P2P_REQ_GetDocumentDetailsBySelectedReqWorkbenchItems";
        internal const string USP_REQ_GETPARTNERSOURCESYSTEMDETAILSBYREQID = "USP_REQ_GetPartnerSourceSystemDetailsByReqId";
        internal const string usp_P2P_REQ_GetRequisitionID = "usp_P2P_REQ_GetRequisitionID";
        internal const string USP_P2P_REQ_GETPARTNERCONTACTSOFROLESBYLOCATIONID = "USP_P2P_REQ_GetPartnerContactsOfRolesByLocationId";
        #endregion

        #region Requisition TableTypes
        internal const string TVP_P2P_REQUISITIONITEM = "tvp_P2P_RequisitionItem";
        internal const string TVP_P2P_SETTINGVALUES = "tvp_P2P_SettingValues";
        internal const string TVP_P2P_LINEITEMSTAXANDSHIPPING = "tvp_P2P_LineItemsTaxandShipping";
        internal const string TVP_P2P_NOTESORATTACHMENTS = "tvp_P2P_NotesOrAttachmentsIds";
        internal const string TVP_P2P_REQUISITIONITEMBUYERUPDATE = "tvp_P2P_RequisitionItemBuyerUpdate";
        internal const string tvp_Item_ItemStatus = "tvp_Item_ItemStatus";
        internal const string TVP_P2P_REQUISITIONLINEITEM = "tvp_P2P_RequisitionLineItem";
        internal const string TVP_P2P_REQUISITIONITEMDETAILS = "tvp_P2P_RequisitionItemDetails";
        internal const string TVP_Basic_Setting = "tvp_BasicSettings";
        internal const string TVP_CatalogLineitem = "tvp_CatalogLineitem";
        internal const string TVP_ES_DOCCODETYPEMAPPING = "tvp_ES_DocCodeTypeMapping";
        internal const string TVP_P2P_REQUISITIONITEMINCOTERMDETAILS = "tvp_P2P_RequisitionItemIncoTermDetails";
        internal const string TVP_P2P_REQUISITIONITEMFLIPTYPEUPDATE = "tvp_P2P_RequisitionItemFlipTypeUpdate";
        internal const string TVP_P2P_KEYVALUEIDNAME = "tvp_P2P_KeyValueIdName";
        internal const string TVP_P2P_REQ_BUDGETALLOCATION = "tvp_P2P_REQ_BudgetAllocation";
        public const string TVP_P2P_DOCUMENTADDITIONALENTITYINFO = "tvp_P2P_DocumentAdditionalEntityInfo";
        internal const string tvp_CurrencyExchangeRates = "tvp_CurrencyExchangeRates";
        #endregion

        #region Requestera and OBU Data
        internal const string COL_REQUESTER_FIRSTNAME = "RequesterFirstName";
        internal const string COL_REQUESTER_LASTNAME = "RequesterLastName";
        internal const string COL_REQUESTER_EMAILADDRESS = "RequesterEmailAddress";
        internal const string COL_REQUESTER_CONTACTCODE = "RequesterContactCode";
        internal const string COL_OBU_FIRSTNAME = "OBUFirstName";
        internal const string COL_OBU_LAST_NAME = "OBULastName";
        internal const string COL_OBU_EMAILADDRESS = "OBUEmailAddress";
        internal const string COL_OBU_CONTACTCODE = "OBUContactCode";
        internal const string USP_P2P_REQ_UPDATEREQUISITIONSTATUSFROMINTERFACE = "usp_P2P_REQ_UpdateRequsitionStatusFromInterface";
        internal const string USP_P2P_REQ_GETNOTIFICATIONDETAILSFORINTERFACE = "usp_P2P_REQ_GetNotificationDetailsForInterface";


        #endregion


        #endregion

        #region Order

        #region Order Columns
        internal const string COL_ORDER_ID = "OrderID";
        internal const string COL_ORDER_NAME = "OrderName";
        internal const string COL_ORDER_NUMBER = "OrderNumber";
        internal const string COL_ORDER_STATUS = "OrderStatus";
        internal const string COL_ORDER_SOURCE = "OrderSource";
        internal const string COL_ORDER_LOCATIONID = "OrderLocationID";
        internal const string COL_MATCHTYPE = "MatchType";
        internal const string COL_ISHEADERCONTRACTVISIBLE = "IsHeaderContractVisible";
        internal const string COL_ISOVERALLLIMITENABLED = "IsOverallLimitEnabled";
        internal const string COL_LineStatus = "LineStatus";
        internal const string COL_ITEMTOTAL = "ItemTotal";
        internal const string COL_ORDERAMOUNT = "OrderAmount";
        internal const string COL_ORDERSUBMITTEDBY = "SubmittedBy";
        internal const string COL_EXTENDEDSTATUS = "ExtendedStatus";
        internal const string COL_DATEACKNOWLEDGED = "DateAcknowledged";
        internal const string COL_ORDER_CONTACTCODE = "OrderContactCode";
        internal const string COL_ORDER_ORDERCONTACTEMAIL = "OrderContactEmail";
        internal const string COL_ORDER_ORDERCONTACTCODE = "OrderContactCode";
        internal const string COL_ORDER_ISINTERNALITEMEXIST = "IsInternalItemExist";
        internal const string COL_CLOSING_ORDER_STATUS = "ClosingOrderStatus";
        internal const string COL_TRASMISSIONMODE = "TrasmissionMode";
        internal const string COL_TRANSMISSIONVALUE = "TransmissionValue";
        internal const string COL_ACKNOWELEDGED_BY = "AcknowledgedBy";
        internal const string COL_ORDER_TYPE = "OrderType";
        internal const string COL_PARENTITEM_UNITPRICE = "ParentItemUnitPrice";
        internal const string COL_PARENTITEM_DATENEEDED = "ParentItemDateNeeded";
        internal const string COL_POSIGNATORYCODE = "POSignatoryCode";
        internal const string COL_POSIGNATORYNAME = "POSignatoryName";
        internal const string COL_PARENTDOCUMENTCODE = "ParentDocumentCode";
        internal const string COL_REVISIONNUMBER = "RevisionNumber";
        internal const string COL_ISCLOSEFORRECEIVING = "IsCloseForReceiving";
        internal const string COL_ISCLOSEFORINVOICING = "IsCloseForInvoicing";
        internal const string COL_ISENFORCELINEREFERENCE = "IsEnforceLineReference";
        internal const string COL_MATERIAL_RECEIVING_TOLERANCE = "MaterialReceivingTolerance";
        internal const string COL_SERVICE_RECEIVING_TOLERANCE = "ServiceReceivingTolerance";
        internal const string COL_ITEM_RECEIVING_TOLERANCE = "ItemReceivingTolerance";
        internal const string COL_ISRESENT = "RESENT";
        internal const string COL_ISORDERANDINVOICECONTACTSAME = "IsOrderAndInvoiceContactSame";
        internal const string COL_CURRENTDATEMINUSINVOICEDATE = "CurrentDateMinusInvoiceDate";
        internal const string COL_ISORDERANDINVOICECURRENCYSAME = "IsOrderAndInvoiceCurrencySame";
        internal const string COL_IS_ALL_NEGATIVE_ITEMS = "IsAllNegativeItems";
        internal const string COL_ENTITYDETAILCODECCPGPC = "EntityDetailCode";
        internal const string COL_ENTITYDESCRIPTIONCCPGPC = "EntityDescription";
        internal const string COL_ISANYITEMINTHREEWAYMATCH = "IsAnyItemInThreeWayMatch";
        internal const string COL_ServiceConfirmationRecevingStatus = "ServiceConfirmationRecevingStatus";
        internal const string COL_INVOICEADVANCEAMOUNT = "InvoiceAdvanceAmount";
        internal const string COL_INVOICEADJUSTMENTAMOUNT = "InvoiceAdjustmentAmount";
        internal const string COL_ORDERADVANCEAMOUNT = "OrderAdvanceAmount";
        internal const string COL_ADJUSTEDINVOICETOTAL = "AdjustedInvoiceTotal";
        internal const string COL_AvailableADVANCEAMOUNT = "AvailableAdvanceAmount";
        internal const string COL_ISADVANCED = "isAdvanced";
        internal const string COL_QUANTITYVARIANCE = "QuantityVariance";
        internal const string COL_UNITPRICEVARIANCE = "UnitPriceVariance";
        internal const string COL_SHIPPINGCHARGESVARIANCE = "ShippingChargesVariance";
        internal const string COL_TAXVARIANCE = "TaxVariance";
        internal const string COL_ADDITIONALCHARGESVARIANCE = "AdditionalChargesVariance";
        internal const string COL_ORDERAMOUNTVARIANCE = "OrderAmountVariance";
        internal const string COL_LINEITEMVARIANCE = "LineItemVariance";
        internal const string COL_PO_SPLIT_ITEM_ENTITYID = "OrderSplitItemEntityId";
        internal const string COL_OLDORDERAMOUNT = "OldOrderAmount";
        internal const string COL_IS_PCARDSUPPORTEDFORORDER = "IsPCardSupportedForOrder";
        internal const string COL_SC_NUMBER = "ServiceConfirmationNumber";
        internal const string COL_IS_PARTNERCONTACTACTIVE = "IsPartnerContactActive";
        internal const string COL_PARENTORDERPAYMENTTERMID = "ParentOrderPaymentTermId";
        internal const string COL_PARENTORDERPAYMENTTERMS = "ParentOrderPaymentTerms";
        internal const string COL_ERS = "ERS";
        internal const string COL_EXCHANGE_RATE = "ExchangeRate";
        internal const string COL_ISMAINTAINREVISIONNUMBER = "isMaintainRevisionNumber";
        internal const string COL_ISINTERNALCHANGEORDERFINALIZE = "isInternalChangeOrderFinalize";
        internal const string COL_PARENTDOCUMENTSTATUSID = "ParentDocumentStatus";
        internal const string COL_ORDER_NOTIFICATION_TO_CONTACTCODE = "NotificationToContactCode";
        internal const string COL_ORDER_NOTIFICATION_TO_EMAILID = "NotificationContactToEmail";
        internal const string COL_ORDER_NOTIFICATION_CC_CONTACTCODE = "NotificationCcContactCode";
        internal const string COL_ORDER_NOTIFICATION_CC_EMAILID = "NotificationContactCcEmail";
        internal const string COL_CHANGE_ORDER_CREATOR = "ChangeOrderCreator";
        internal const string COL_DOCUMENT_STAKEHOLDER_ID = "DocumentStakeholderId";
        internal const string COL_STAKEHOLDER_DOCUMENT_STATUS = "StakeholderDocumentStatus";
        internal const string COL_STAKEHOLDER_TYPE_INFO = "StakeholderType";
        internal const string COL_CONTACT_NAME = "ContactName";
        internal const string COL_PROXY_CONTACT_CODE = "ProxyContactCode";
        internal const string COL_ISALLOWCREATEASN = "IsAllowCreateASN";
        internal const string COL_ERROR_MESSAGE_KEY = "ErrorMessageKey";
        internal const string COL_ERROR_MESSAGE_VALUE = "ErrorMessageValue";
        internal const string usp_P2P_PO_UpdateInterfaceDocumentStatus = "usp_P2P_PO_UpdateInterfaceDocumentStatus";
        internal const string COL_ORDEREXTENDEDSTATUS = "OrderExtendedStatus";
        internal const string COL_TYPE = "Type";
        internal const string COL_CHANGEREQUISITIONDOCUMENTCODE = "ChangeRequisitionDocumentCode";
        internal const string COL_ORDERTOTALCHANGE = "OrderTotalChange";
        internal const string COL_ProcurementProfileId = "ProcurementProfileId";
        internal const string COL_ORDERAUTHOR = "OrderAuthor";
        internal const string COL_ORDERAUTHORNAME = "OrderAuthorName";
        internal const string COL_ORDERAUTHOREMAIL = "OrderAuthorEmail";
        internal const string COL_PURCHASETYPEFORORDER = "PurchaseType";
        internal const string COL_ContractLineReference = "ContractLineReference";
        internal const string COL_ISPARENTORDERITEM = "IsParentOrderItem";
        internal const string COL_PO_P2P_ITEM_ID = "P2PLineItemID";
        internal const string COL_PO_STARTDATE = "StartDate";
        internal const string COL_PO_ENDDATE = "EndDate";
        #endregion

        #region Order StoredProcedures
        internal const string USP_P2P_PO_GETALLDISPLAYDETAILS = "usp_P2P_PO_GetAllDisplayDetails";
        internal const string USP_P2P_GETCOSTCENTERPLANTCODEPURCGROUP = "usp_P2P_GetCostCenterPlantCodePurcDept";

        internal const string USP_P2P_PO_GETORDERBASICDETAILSBYID = "usp_P2P_PO_GetOrderBasicDetailsById";
        internal const string USP_P2P_PO_GETORDERBASICDETAILSBYIDFORITEMS = "usp_P2P_PO_GetOrderBasicDetailsByIdForLineItems";
        internal const string USP_P2P_PO_GETORDERDETAILSFORAPPROVAL = "usp_P2P_PO_GetOrderDetailsForApproval";
        internal const string USP_P2P_PO_GENERATEDEFAULTORDERNAME = "usp_P2P_PO_GenerateDefaultOrderName";
        internal const string USP_P2P_PO_CREATREQUISTIONORDER = "usp_P2P_PO_CreateRequistionOrder";
        internal const string USP_P2P_PO_GETORDERLINEITEMS = "usp_P2P_PO_GetOrderLineItems";
        internal const string USP_P2P_PO_GETORDERLINEITEMS_FOR_INTERFACE = "usp_P2P_PO_GetOrderLineItemsForInterface";
        internal const string USP_P2P_PO_GETREQUISITIONNUMBERFROMORDERID = "usp_P2P_PO_GetRequisitionNumberFromOrderId";
        internal const string USP_P2P_PO_GETALLORDERSFORLEFTPANEL = "usp_P2P_PO_GetAllOrdersForLeftPanel";
        internal const string USP_P2P_PO_GETALLREQUISITIONSFORORDERLEFTPANEL = "usp_P2P_PO_GetAllRequisitionsForOrderLeftPanel";
        internal const string USP_P2P_PO_SAVEORDERITEMPARTNERS = "usp_P2P_PO_SaveOrderItemPartners";
        internal const string USP_P2P_PO_SAVEORDERITEMSHIPPING = "usp_P2P_PO_SaveOrderItemShippingDetails";
        internal const string USP_P2P_PO_SAVEORDERITEMSHIPPING_TUNED = "usp_P2P_PO_SaveOrderItemShippingDetailsTuned";
        internal const string USP_P2P_PO_SAVEORDER = "usp_P2P_PO_SaveOrder";
        internal const string USP_P2P_GETREQUISITIONITEMSCOUNTBYPARTNERCODE = "usp_P2P_GetRequisitionItemsCountByPartnerCode";
        internal const string USP_P2P_PO_UPDATEORDERSTATUS = "usp_P2P_PO_UpdateOrderStatus";
        internal const string USP_P2P_PO_ADDREQUISITIONSTOORDER = "USP_P2P_PO_AddRequisitionsToOrder";
        internal const string USP_P2P_PO_VALIDATEDOCUMENTBEFORENEXTACTION = "usp_P2P_PO_ValidateDocumentBeforeNextAction";
        internal const string USP_P2P_PO_GETINVOICINGSTATUSBYID = "usp_P2P_PO_GetInvoicingStatusById";
        internal const string USP_P2P_PO_CALCULATEPORECEIVINGSTATUS = "usp_P2P_PO_CalculatePOReceivingStatus";
        internal const string USP_P2P_PO_RECALCULATEPORECEIVINGSTATUSBYORDERID = "usp_P2P_PO_UpdateChangeOrderReceivingStatus";
        internal const string USP_P2P_PO_RECALCULATEPOINVOICINGSTATUSBYORDERID = "usp_P2P_PO_UpdateChangeOrderInvoicingStatus";
        internal const string USP_P2P_PO_GETORDERINFOBYDOCUMENTCODE = "usp_P2P_PO_GetOrderInfoByDocumentCode";
        internal const string USP_P2P_PO_GETCHANGEORDERGDETAILBYORDERID = "usp_P2P_PO_GetChangeOrderDetailByOrderId";
        internal const string USP_P2P_PO_GETORDERINVOICINGSTATUS = "usp_P2P_PO_GetOrderInvoicingStatus";
        internal const string USP_P2P_PO_CHANGEORDERREQUEST = "usp_P2P_PO_ChangeOrderRequest";
        internal const string USP_P2P_PO_CANCELCHANGEORDER = "usp_P2P_PO_CancelChangeOrder";
        internal const string USP_P2P_PO_GETALLINVOICESANDRECEIPTS = "usp_P2P_PO_GetAllInvoicesAndReceipts";
        internal const string USP_P2P_PO_GETPRIVOUSORDERVERSION_AND_REMOVEORDERITEMS = "usp_P2P_PO_GetPreviousOrderVersion";
        internal const string USP_P2P_PO_REJECTCHANGEREQUEST = "usp_P2P_PO_RejectChangeRequest";
        internal const string USP_P2P_PO_COMPARECHANGEREQUESTTOORDER = "usp_P2P_PO_CompareChangeRequestToOrder";
        internal const string USP_P2P_PO_CHECKORDERVERSIONEXISTS = "usp_P2P_PO_CheckOrderVersionExists";
        internal const string USP_P2P_PO_SAVEORDERITEMSTATUS = "usp_P2P_PO_SaveOrderItemStatus";
        internal const string USP_P2P_PO_GETALLDOCUMENTSFORCANCELEDORDER = "usp_P2P_PO_GetAllDocForCanceledOrder";
        internal const string USP_P2P_PO_UPDATEORDERCANCELLEDDATE = "usp_P2P_PO_UpdateOrderCancelledDate";
        internal const string USP_P2P_PO_SAVEORDERACCOUNTINGDETAILS = "usp_P2P_PO_SaveOrderAccountingDetails";
        internal const string USP_P2P_PO_SAVEORDERACCOUNTINGDETAILS2 = "usp_P2P_PO_SaveOrderAccountingDetailsV2";
        internal const string USP_P2P_PO_SAVEORDERCHARGEACCOUNTINGDETAILSV2 = "usp_P2P_PO_SaveOrderChargeAccountingDetailsV2";
        internal const string USP_P2P_PO_GETORDERACCOUNTINGDETAILSBYITEMID = "USP_P2P_PO_GetOrderAccountingDetailsByItemId";
        //  internal const string USP_P2P_PO_GETDOCUMENTIDBYDOCUMENTCODE = "usp_P2P_PO_GetDocumentIdByDocumentCode";
        internal const string USP_P2P_DELETE_REQUISITIONBY_DOCUMENTCODE = "usp_P2P_DeleteRequisitionByDocumentCode";
        internal const string USP_P2P_DELETE_CREDITMEMOBY_DOCUMENTCODE = "usp_P2P_DeleteCreditMemoByDocumentCode";
        // internal const string USP_P2P_PO_GENERATEDEFAULTORDERNAMEFORFLIPTOORDER = "usp_P2P_PO_GenerateDefaultOrderNameforFliptoOrder"; 
        internal const string USP_P2P_PO_GENERATEDEFAULTORDERNAMEFORFLIPTOORDER = "usp_P2P_PO_GenerateDefaultOrderNameforFliptoOrder";
        internal const string USP_P2P_PO_GETALLCATEGORIESBYORDERID = "usp_P2P_PO_GetAllCategoriesByOrderId";
        internal const string USP_P2P_PO_GETORDER_ENTITIES = "usp_P2P_PO_GetOrderEntities";
        internal const string USP_P2P_PO_GETALLCHANGEORDERSBYINVOICEORIR = "usp_P2P_PO_GetAllChangeOrdersByInvoiceorIR";
        internal const string USP_P2P_GETRELATEDDOCUMENTS = "usp_P2P_GetRelatedDocuments";
        internal const string USP_P2P_GETRELATEDDOCUMENTSDETAILS = "usp_P2P_GetRelatedDocumentsDetails";
        internal const string USP_P2P_PO_GETORDERIDBYORDERNUMBERANDSTATUS = "usp_P2P_PO_GetOrderIdByOrderNumberAndStatus";
        internal const string USP_P2P_GENERATELOCATIONCODE = "usp_P2P_GenerateLocationCode";
        internal const string USP_P2P_PO_GETORDERENTITYDETAILSBYID = "usp_P2P_PO_GetOrderEntityDetailsById";
        internal const string USP_P2P_PO_SAVEORDERADDITIONALENTITYDETAILS = "usp_P2P_PO_SaveOrderAdditionalEntityDetails";
        internal const string USP_P2P_PO_GETORDERDETAILSFORCOMPARISIONBYID = "usp_P2P_PO_GetOrderDetailsForComparisionById";
        internal const string USP_P2P_PO_GETORDERITEMDETAILSFORCOMPARISIONBYID = "usp_P2P_PO_GetOrderItemDetailsForComparisionById";
        internal const string USP_P2P_PO_COMPAREACCOUNTINGDETAILSFORAPPROVAL = "usp_P2P_PO_CompareAccountingDetailsForApproval";
        internal const string USP_P2P_PO_UPDATEINTERFACESTATUS = "usp_P2P_PO_UpdateInterfaceStatus";
        internal const string USP_P2P_PO_CREATERELEASEORDER = "usp_P2P_PO_CreateReleaseOrder";
        internal const string USP_P2P_PO_GETLINEITEMTAXDETAILS = "usp_P2P_PO_GetLineItemTaxDetails";
        internal const string USP_P2P_PO_GETLINEITEMTAXDETAILS_FORLISTOFDOCUMENTITEMIDS = "usp_P2P_PO_GetLineItemTaxDetails_ForListOfDocumentItemIds";
        internal const string USP_P2P_PO_PRORATELINEITEMTAX = "usp_P2P_PO_ProrateLineItemTax";
        internal const string USP_P2P_PO_PRORATESHIPPINGANDFREIGHT = "usp_P2P_PO_ProrateShippingAndFreight";
        internal const string USP_P2P_PO_UPDATETAXONHEADERSHIPTO = "usp_P2P_PO_UpdateTaxOnHeaderShipTo";
        internal const string USP_P2P_PO_CALCULATE_AND_UPDATELINEITEMTAX = "usp_P2P_PO_CalculateAndUpdateLineItemTax";
        internal const string USP_P2P_PO_GETMODIFIEDITEMSINCHANGEORDER = "usp_P2P_PO_GetModifiedItemsInChangeOrder";
        internal const string USP_P2P_PO_GETORDERSFORINTERFACE = "usp_P2P_PO_GetOrdersForInterface";
        internal const string USP_P2P_PO_GETORDERITEMIDBYP2PLIID = "usp_P2P_PO_GetOrderItemIdByP2PLiId";
        internal const string USP_P2P_PO_GETCATALOGNONCATALOGCHECK = "usp_P2P_PO_GetCatalogNonCatalogCheck";
        internal const string USP_P2P_PO_UPDATECONSUMEDAMOUNTFORBLANKETORDER = "usp_P2P_PO_UpdateConsumedAmountForBlanketOrder";
        internal const string USP_P2P_PO_PRORATEHEADERTAXANDSHIPPING = "usp_P2P_PO_ProrateHeaderTaxAndShipping";
        internal const string USP_P2P_PO_GETVALIDATIONERRORCODEBYID = "usp_P2P_PO_GetValidationErrorCodeById";
        internal const string USP_P2P_PO_SAVEORDERACCOUNTINGAPPLYTOALL = "usp_P2P_PO_SaveOrderAccountingApplyToAll";
        internal const string USP_P2P_PO_GETALLACTIVEPAYMENTTERMS = "usp_P2P_PO_GetAllActivePaymentTerms";
        internal const string USP_P2P_GETPAYMENTTERMSBYID = "usp_P2P_GetPaymentTermsById";
        internal const string USP_P2P_PO_GETGLDETAILS = "usp_P2P_PO_GetGLDetails";
        internal const string USP_P2P_PO_GETORGENTITYDETAILS = "usp_P2P_PO_GetOrgEntityDetails";
        internal const string USP_P2P_GETINTERFACECONFIGURATIONDETAILS = "usp_P2P_GetInterfaceConfigurationDetails";
        internal const string USP_P2P_PO_VALIDATEINTERFACEDOCUMENT = "usp_P2P_PO_ValidateInterfaceDocument";
        internal const string USP_P2P_PO_UPDATEEDOCUMENTITEMRECEIVEINGSTATUS = "usp_P2P_PO_UpdateDocumentItemReceivingStatus";
        internal const string USP_P2P_GETWORKFLOWDOCTYPEFORDOCUMENT = "usp_P2P_GetWorkflowDocTypeForDocument";
        internal const string USP_P2P_PO_GETALLORDERSFORAUTOCLOSE = "usp_P2P_PO_GetAllOrdersForAutoClose";
        internal const string USP_P2P_PO_UPDATEORDER_REQ_MAPPING = "usp_P2P_PO_UpdateOrderReqMapping";
        internal const string USP_P2P_PO_DELETE_ALL_LINEITEMS_BY_ORDERID = "usp_P2P_PO_DeleteAllLineItemsByOrderId";
        internal const string USP_P2P_PO_COPY_ORDERS_TO_ORDER = "usp_P2P_PO_CopyOrdersToOrder";
        internal const string USP_P2P_PO_GETALLITEMIDSBYORDID = "usp_P2P_PO_GetAllItemIdsByOrdId";
        internal const string USP_P2P_PO_GETORDERHEADERDETAILSBYID = "usp_P2P_PO_GetOrderHeaderDetailsById";
        internal const string USP_P2P_PO_GETORDERHEADERDETAILSBYID_FOR_INTERFACE = "usp_P2P_PO_GetOrderHeaderDetailsByIdForInterface";
        internal const string USP_P2P_PO_ORDERCATALOGITEMACCESS = "usp_P2P_PO_OrderCatalogItemAccess";
        internal const string USP_P2P_PO_ADDTEMPLATEITEMSINORDER = "usp_P2P_PO_AddTemplateItemInOrder";
        internal const string USP_P2P_PO_GETVALIDATIONFORCLOSINGORDER = "usp_P2P_PO_GetValidationForClosingOrder";
        internal const string USP_P2P_PO_UPDATECLOSINGORDERSTATUS = "usp_P2P_PO_UpdateClosingOrderStatus";
        internal const string USP_P2P_PO_CHECKCREATEINVOICEFORORDER = "usp_P2P_PO_CheckCreateInvoiceForOrder";
        internal const string USP_P2P_PO_VALIDATEBLANKETORDER = "usp_P2P_PO_ValidateBlanketOrder";
        internal const string USP_P2P_SAVEPOTRANSMISSIONMODE = "usp_P2P_SavePOTransmissionMode";
        internal const string USP_P2P_PO_UPDATEORGENTITIESFROMCATALOG = "usp_P2P_PO_UpdateOrgEntitiesFromCatalog";
        internal const string USP_P2P_COPYORDEREXISTINGTONEW = "usp_P2P_CopyOrderExistingToNew";
        internal const string USP_P2P_PO_UPDATECONSUMEDAMOUNT = "usp_P2P_PO_UpdateConsumedAmount";
        internal const string USP_P2P_PO_UPDATEBILLTOLOCATION = "usp_P2P_PO_UpdateBillToLocation";
        internal const string USP_P2P_PO_DELETESPLITSBYITEMID = "usp_P2P_PO_DeleteSplitsByItemId";
        internal const string USP_P2P_PO_DELETECHARGEANDSPLITSITEMSBYITEMCHARGEID = "usp_P2P_PO_DeleteChargeAndSplitsItemsByItemChargeId";
        internal const string USP_P2P_REQ_DELETECHARGEANDSPLITSITEMSBYITEMCHARGEID = "usp_P2P_REQ_DeleteChargeAndSplitsItemsByItemChargeId";
        internal const string USP_P2P_PO_CALCULATEINTERFACEITEMSPLITDETAILS = "usp_P2P_PO_CalculateInterfaceItemSplitDetails";
        internal const string USP_P2P_CM_CALCULATEINTERFACEITEMSPLITDETAILS = "usp_P2P_CM_CalculateInterfaceItemSplitDetails";
        internal const string USP_P2P_PO_DELETELINEITEMSBYORGENTITYCODE = "usp_P2P_PO_DeleteLineItemsByOrgEntityCode";
        internal const string USP_P2P_PO_GETALLORDERDETAILSBYORDERID = "usp_P2P_PO_GetAllOrderDetailsByOrderId";
        internal const string USP_P2P_PO_GETALLORDERDETAILSBYORDERIDFORVIEWCHANGES = "usp_P2P_PO_GetAllOrderDetailsByOrderIdForViewChanges";
        internal const string USP_P2P_PO_DELETELINEITEMSONBUCHANGE = "usp_P2P_PO_DeleteLineItemsOnBUChange";
        internal const string USP_WF_GETMANAGERFORORGENTITY = "usp_WF_GetManagerForORGEntity";
        internal const string USP_P2P_PO_UPDATEORGENTITIESFROMCATALOGBYITEMID = "usp_P2P_PO_UpdateOrgEntitiesFromCatalogByItemId";
        internal const string USP_P2P_PO_GETORDERCAPITALCODECOUNTBYID = "usp_P2P_PO_GetOrderCapitalCodeCountById";
        internal const string USP_P2P_PO_CONFIRMINGORDERREQUEST = "usp_P2P_PO_ConfirmingOrderRequest";
        internal const string USP_P2P_PO_SAVECONTRACTINFORMATION = "usp_P2P_PO_SaveContractInformation";
        internal const string USP_P2P_PO_GETORDERHEADERDETAILBYID = "usp_P2P_PO_GetOrderHeaderDetailById";
        internal const string USP_P2P_PO_GETORDERITEMSCHANGED = "usp_P2P_PO_GetOrderItemsChanged";
        internal const string USP_P2P_PO_GETALLCHANGEORDERSBYORDER = "usp_P2P_PO_GetAllChangeOrdersByOrder";
        internal const string USP_P2P_PO_GETALLUSER = "usp_P2P_PO_GetAllUser";
        internal const string USP_P2P_PO_UPDATEACCRUALDASHBOARD = "usp_P2P_PO_UpdateAccrualDashBoard";
        internal const string USP_P2P_PO_GETORDERLISTFORCLOSINGORDER = "usp_P2P_PO_GetOrderListForClosingOrder";
        internal const string USP_P2P_INV_SAVECONTRACTINFORMATION = "usp_P2P_INV_SaveContractInformation";
        internal const string USP_P2P_PO_GETCONTRACTNUMBERBYORDERID = "usp_P2P_PO_GetContractNumberByOrderId";
        internal const string USP_P2P_REQ_CALCULATEINTERFACEITEMSPLITDETAILS = "usp_P2P_REQ_CalculateInterfaceItemSplitDetails";
        internal const string USP_P2P_PO_UPDATELINEITEMSCLOSEORDERSTATUS = "usp_P2P_PO_UpdateLineItemsCloseOrderStatus";
        internal const string USP_P2P_REQ_GETALLPARTNERCODEANDORDERINGLOCATIONID = "usp_P2P_REQ_GetAllPartnerCodeAndOrderinglocationId";
        internal const string USP_P2P_REQ_GETALLPARTNERCODEORDERINGLOCATIONIDANDSPENDCONTROLITEMID = "usp_P2P_REQ_GetAllPartnerCodeOrderinglocationIdAndSpendControlItemId";
        internal const string USP_P2P_REQ_GETLISTERRORCODESBYORDERIDS = "usp_P2P_REQ_GetListErrorCodesByOrderIds";
        internal const string USP_P2P_REQ_GETALLPARTNERCODEANDORDERINGLOCATION = "usp_P2P_REQ_GetAllPartnerCodeAndOrderinglocation";
        internal const string USP_P2P_PO_GETORDERSBYPARTNERCODE = "usp_P2P_PO_GetOrdersByPartnerCode";
        internal const string USP_P2P_INV_UPDATEDISPATCHHISTORY = "usp_P2P_PO_UpdateDispatchHistory";
        internal const string USP_P2P_PO_GETDISPATCHDETAILS = "usp_p2p_PO_GetDispatchDetails";
        internal const string USP_P2P_PO_INSERTUPDATELINEITEMTAXES = "usp_P2P_PO_InsertUpdateLineItemTaxes";
        internal const string USP_P2P_VALIDATESHIPTOBILLTOFROMINTERFACE = "usp_P2P_ValidateShipToBillToFromInterface";
        internal const string USP_P2P_REQ_INSERTUPDATELINEITEMTAXES = "usp_P2P_REQ_InsertUpdateLineItemTaxes";
        internal const string USP_P2P_PO_GETSPLITDETAILS = "usp_P2P_PO_GetSplitDetails";
        internal const string USP_P2P_CM_GETSPLITDETAILS = "usp_P2P_CM_GetSplitDetails";
        internal const string USP_P2P_REQ_GETSPLITDETAILS = "usp_P2P_REQ_GetSplitDetails";
        internal const string USP_P2P_PO_GETREVISIONNUMBERBYDOCUMENTCODE = "usp_P2P_PO_GetRevisionNumberByDocumentCode";
        internal const string USP_P2P_PO_GETREVISIONNUMBERFORRO = "usp_P2P_PO_GetRevisionNumberForRO";
        internal const string USP_P2P_REC_GETREVISIONNUMBERBYDOCUMENTCODE = "usp_P2P_REC_GetRevisionNumberByDocumentCode";
        internal const string USP_P2P_REC_GETRECEIPTIDBYRETURNNOTEID = "usp_P2P_REC_GetReceiptIdByReturnNoteId";
        internal const string USP_P2P_PO_GETREQUISITIONNUMBERSBYORDERID = "usp_P2P_PO_GetRequisitionNumbersByOrderId";
        internal const string USP_P2P_PO_UPDATEORDER_CREDITMEMO_MAPPING = "usp_P2P_PO_UpdateOrderCreditMemoMapping";
        internal const string USP_P2P_PO_GETALLITEMIDSBYORDERID = "usp_P2P_PO_GetAllItemIdsByOrderdId";
        internal const string USP_P2P_PO_DELETELINEITEMSBYEXTENDEDTYPEIDS = "USP_P2P_PO_DeleteLineItemsByExtendedTypeIds";
        internal const string USP_P2P_PO_UPDATELINETYPEBYPURCHASETYPE = "usp_P2P_PO_UpdateLineTypeByPurchaseType";
        internal const string USP_P2P_UPDATE_BULK_DOCUMENT_STATUS = "usp_P2P_UpdateBulkMatchingStatus";
        internal const string USP_P2P_PO_GetFOBDETAILSFORINTERFACE = "usp_P2P_GetFOBDetailsForInterface";
        internal const string USP_P2P_INV_GETDISPATCHDETAILS = "usp_p2p_INV_GetDispatchDetails";
        internal const string USP_P2P_PO_VALIDATEPOFOREDITSHIPTOONCO = "usp_p2p_PO_ValidatePOForEditShipToOnCO";
        internal const string USP_P2P_PO_GETORDERREPORTCONFIGURATION = "usp_P2P_PO_GetOrderReportConfiguration";
        internal const string USP_P2P_PO_GETORDERDETAILSFOREXCELREPORTBYPARTNER = "usp_P2P_PO_GetOrderDetailsForExcelReportByPartner";
        internal const string USP_P2P_PO_GETALLPOSWITHOUTINVOICEORRECEIPT = "usp_P2P_PO_GetALLPOsWithoutInvoiceOrReceipt";
        internal const string USP_P2P_FLIPBLANKETACCOUNTINGDETAILSTOORDER = "usp_P2P_FlipBlanketAccountingDetailstoOrder";
        internal const string USP_P2P_SAVEINTERFACEDOCUMENTS = "USP_P2P_SaveInterfaceDocuments";
        internal const string USP_P2P_REC_CREATERECEIPTRETURNNOTECOPY = "usp_P2P_REC_CreateReceiptReturnNoteCopy";
        internal const string USP_P2P_REC_UPDATERECEIPTRETURNNOTEDOCUMENTSTATUS = "usp_P2P_REC_UpdateReceiptReturnNoteDocumentStatus";
        internal const string USP_P2P_PR_DELETELINEITEMSONBUCHANGE = "usp_P2P_PR_DeleteLineItemsOnBUChange";
        internal const string usp_P2P_REC_GetAllReturnNotesByReceiptId = "usp_P2P_REC_GetAllReturnNotesByReceiptId ";
        internal const string USP_P2P_PO_GETORDERVERSIONDETAILS = "usp_P2P_PO_GetOrderVersionDetails";
        internal const string USP_P2P_REC_REVOKERECEIPTRETURNNOTE = "usp_P2P_REC_RevokeReceiptReturnNote";
        internal const string USP_P2P_REC_GETNOTIFICATIONDETAILSBYRETURNNOTEID = "usp_P2P_REC_GetNotificationDetailsByReturnNoteId";
        internal const string usp_P2P_PO_UPDATEPAYMENTTERMS = "usp_P2P_PO_UpdatePaymentTerms";
        internal const string usp_P2P_PO_GETORDERSDETAILTOSENDNOTIFICATION = "usp_P2P_PO_GetOrdersDetailToSendNotification";
        internal const string USP_P2P_PO_GETORDERNOTESORATTACHMENTS = "usp_P2P_PO_GetOrderNotesOrAttachments";
        internal const string USP_P2P_PO_SAVEITEMORDERNOTESORATTACHMENTS = "usp_P2P_PO_SaveOrderNotesOrAttachments";
        internal const string USP_P2P_PO_DELETEORDERNOTESORATTACHMENTS = "usp_P2P_PO_DeleteOrderNotesOrAttachments";

        internal const string USP_P2P_GETCUSTOMATTRIBUTESWITHORDERID = "USP_P2P_GETCUSTOMATTRIBUTESWITHORDERID";
        internal const string USP_P2P_GETALLQUESTIONNAIREBYFORMCODES = "usp_P2P_GetAllQuestionnaireByFormCodes";
        internal const string usp_P2P_PO_GETORDERDETAILTOSENDNOTIFICATION = "usp_P2P_PO_GetOrderDetailToSendNotification";
        internal const string USP_P2P_VALIDATEASL = "usp_P2P_ValidateASL";
        internal const string USP_P2P_PO_UPDATEORDERMATCHTYPE = "usp_P2P_PO_UpdateOrderMatchType";
        internal const string USP_DM_GETDOCUMENTSTAKEHOLDERDETAILS = "usp_DM_GetDocumentStakeholderDetails";
        internal const string USP_P2P_PO_GETTAXDETAILS = "usp_P2P_PO_GetTaxDetails";
        internal const string USP_P2P_PO_UPDATEBULKLINEITEMSTATUS = "usp_P2P_PO_UpdateBulkLineItemStatus";
        internal const string USP_P2P_PO_GETBLANKETDETAILS = "usp_P2P_PO_GetBlanketDetails";
        internal const string USP_P2P_GETBUYERASSIGNEE_DETAILS = "usp_P2P_GetBuyerAssignee_Details";
        internal const string USP_P2P_PO_GETAllORDERSMANAGEMASSUPDATE = "usp_P2P_PO_GetAllOrdersManageMassUpdate";
        internal const string USP_P2P_PO_GETAllORDERITEMSMANAGEMASSUPDATE = "usp_P2P_PO_GetAllOrderItemsManageMassUpdate";
        internal const string USP_P2P_INV_CALCULATEINTERFACEITEMSPLITDETAILS = "usp_P2P_INV_CalculateInterfaceItemSplitDetails";
        internal const string USP_P2P_GETQUESTIONNAIREROWCHOICES = "usp_P2P_GetQuestionnaireRowChoices";
        internal const string USP_P2P_ASN_GETASNIDBYASNNUMBERANDSTATUS = "usp_P2P_ASN_GetAsnIdByASNNumberAndStatus";
        internal const string USP_P2P_GETCUSTOMATTRIBUTESROWMAPPINGBYSOURCEDESTINATION = "usp_P2P_GetCustomAttributesRowMappingBySourceDestination";
        internal const string USP_P2P_GETCLIENTPARTNERCODE = "usp_p2p_Getclientpartnercode";
        internal const string USP_P2P_PO_CHECKACTIVEPROCESSINGINVOICES = "usp_P2P_PO_CheckActiveProcessingInvoices";
        internal const string USP_P2P_PO_GETPARTNERRECONMATCHTYPEIDBYPARTNERCODE = "usp_P2P_PO_GetPartnerReconMatchTypeIdByPartnerCode";
        internal const string USP_P2P_PO_GETPARTNERFOBDETAILS = "usp_P2P_PO_GetPartnerFOBDetails";
        internal const string USP_P2P_PO_SAVESRFQUESTIONNAIRECODEFORITEMID = "usp_P2P_PO_SaveSRFQuestionnaireCodeforItemId";
        internal const string USP_P2P_SAVEFLIPTOORDERHEADERENTITIESANDLINESHIPPINGDETAILS = "usp_P2P_SaveFlipToOrderHeaderEntitiesAndLineShippingDetails";
        internal const string USP_P2P_SAVESRFQUESTIONNAIRECODEFORITEMID = "usp_P2P_PO_SaveSRFQuestionnaireCodeforItemId";
        internal const string USP_P2P_PO_SAVESRFCUSTOMATTRFORCHANGEORDER = "usp_P2P_PO_SaveSRFCustomAttrForChangeOrder";
        internal const string USP_ORG_GETDOCLOBCONFIGURATION = "usp_ORG_GetDocLOBConfiguration";
        internal const string USP_P2P_PO_CHECKORDERSERVICEXIST = "usp_P2P_PO_CheckOrderServiceMapping";
        internal const string USP_P2P_PO_GETVALIDATEACTIVEDDOCUMENT = "usp_P2P_PO_GetValidateActiveDocument";
        internal const string USP_P2P_REQ_GETRISKFORMQUESTIONSCORE = "USP_P2P_REQ_GetRiskFormQuestionScore";
        internal const string USP_P2P_REQ_GETRISKFORMHEADERINSTRUCTIONSTEXT = "USP_P2P_REQ_GetRiskFormHeaderInstructionsText";
        internal const string USP_REQ_GetPriceDetailOfCatalogLineItems = "usp_REQ_GetPriceDetailOfCatalogLineItems";
        internal const string USP_REQ_GETCATALOGLINEITEMDETAILS = "usp_REQ_GetCatalogLineItemDetails";


        #endregion

        #region Order LineItem Columns
        internal const string COL_ORDER_ITEM_ID = "OrderItemID";
        internal const string COL_POITEM_SHIPPING_ID = "OrderLineItemShippingID";
        internal const string COL_POITEM_INVOICED_QUANTITY = "InvoicedQuantity";
        internal const string COL_POITEM_RECEIVED_QUANTITY = "ReceivedQuantity";
        internal const string COL_ACTIVEITEMCOUNT = "ActiveItemCount";
        internal const string COL_ITEMLINENUMBER = "ItemLineNumber";
        internal const string COL_INTERNALLINENUMBER = "InternalLineNumber";
        internal const string COL_LINENUMBER = "LineNumber";
        internal const string COL_SUPPLIERPARTID = "SupplierPartId";
        internal const string COL_CATALOGITEMNUMBER = "CatalogItemNumber";
        internal const string COL_ITEMNUMBER = "ItemNumber";
        internal const string COL_SUPPLIERAUXILIARYPARTID = "SupplierPartAuxiliaryId";
        internal const string COL_POITEM_INVOICED_UNITPRICE = "InvoicedUnitPrice";
        internal const string COL_POITEM_INVOICED_STARTDATE = "InvoicedStartDate";
        internal const string COL_LINEITEMNAME = "LineItemName";
        internal const string COL_REQUESTEDQUANTITY = "RequestedQuantity";
        internal const string COL_ORDEREDQUANTITY = "OrderedQuantity";
        internal const string COL_ORDEREDAMOUNT = "OrderedAmount";
        internal const string COL_ISNOTIFICATION = "isNotification";
        internal const string COL_INVOICECOUNT = "InvoiceCount";
        internal const string COL_INVOICETOTALQUANTITY = "InvoiceTotalQuantity";
        internal const string COL_ORDERINVOICETOTALAMOUNT = "InvoiceTotalAmount";
        internal const string COL_CREDITMEMOAMOUNT = "CreditMemoAmount";
        internal const string COL_CREDITMEMOQUANTITY = "CreditMemoQuantity";
        internal const string COL_RECEIPTSCOUNT = "ReceiptsCount";
        internal const string COL_RECEIPTTOTALACCEPTEDQUANTITY = "ReceiptTotalAcceptedQuantity";
        internal const string COL_RECEIPTTOTALRECEIVEDAMOUNT = "ReceiptTotalReceivedAmount";
        internal const string COL_RETURNNOTECOUNT = "ReturnNoteCount";
        internal const string COL_RETURNNOTETOTALRETURNEDQUANTITY = "ReturnNoteTotalReturnedQuantity";
        internal const string COL_ITEMHISTORYICONTYPE = "ItemHistoryIconType";
        internal const string COL_ISCONTRACTED = "IsContracted";
        internal const string COL_ISBLANKET = "IsBlanket";
        internal const string COL_ITEMMATCHTYPE = "MatchType";
        internal const string COL_ALLOWEDITANDINSPECT = "AllowEditInspect";
        internal const string COL_PARTNERCONFIGURATIONID = "PartnerConfigurationId";
        internal const string COL_PUNCHOUTCARTREQID = "PunchoutCartReqId";
        internal const string COL_P2PItemId = "P2PItemId";
        internal const string COL_ISQUESTIONNAIREERROR = "IsQuestionnaireError";

        internal const string COL_Evaluator = "Evaluator";
        internal const string COL_EvaluatorName = "EvaluatorName";
        internal const string COL_TOTALCHARGE = "TotalCharge";
        internal const string COL_CONTRACTITEMID = "ContractItemId";
        internal const string COL_CONTRACTITEMDESCRIPTION = "ContractItemDescription";
        internal const string COL_ITEMERS = "ERS";
        internal const string COL_TOTALCOMPLETION = "TotalCompletion";
        internal const string COL_TOTALRECEIVEDAMOUNT = "TotalReceivedAmount";
        internal const string COL_OLD_ORDERITEMID = "OldOrderItemId";
        internal const string COL_TAXRATE = "TaxRate";
        internal const string COL_TAXERRORMESSAGE = "TaxErrorMessage";
        internal const string COL_POITEM_CREDIT_QUANTITY = "CreditQuantity";
        internal const string COL_POITEM_CREDIT_UNIT_PRICE = "CreditUnitPrice";
        internal const string COL_ISALLOWRECIEPTS = "IsAllowReceipts";
        internal const string COL_LOBId = "LOBId";
        #endregion

        #region Order LineItem StoredProcedures
        internal const string USP_P2P_PO_GETALLLINEITEMSBYID = "usp_P2P_PO_GetAllLineItemsById";
        internal const string USP_P2P_PO_GETREQUESTERSFORORDERBYID = "usp_P2P_PO_GetRequestersForOrderById";
        internal const string USP_P2P_PO_GETORDERFORCANCELORDERNOTIFICATION = "usp_P2P_PO_GetOrderForCancelOrderNotification";
        internal const string USP_P2P_PO_GETALLINVOICESFORCANCELORDERNOTIFICATION = "usp_P2P_PO_GetAllInvoicesForCancelOrderNotification";
        internal const string USP_P2P_PO_GETPARTNERDETAILSBYLIID = "usp_P2P_PO_GetPartnerDetailsByLiId";
        internal const string USP_P2P_PO_GETSHIPPINGSPLITDETAILSBYLIID = "usp_P2P_PO_GetShippingSplitDetailsByLiId";
        internal const string USP_P2P_PO_GETOTHERITEMDETAILSBYLIID = "usp_P2P_PO_GetOtherItemDetailsByLiId";
        internal const string USP_P2P_PO_SAVEORDERITEM = "usp_P2P_PO_SaveOrderItem";
        internal const string USP_P2P_PO_SAVEESTIMATEDDATESORDERITEMS = "usp_P2P_PO_SaveTempLineItems";
        internal const string USP_P2P_PO_UPDATEPROMISEDATE = "usp_P2P_PO_UpdatePromiseDate";
        internal const string USP_P2P_PO_DELETELINEITEMBYID = "usp_P2P_PO_DeleteLineItemById";
        internal const string USP_P2P_PO_GETALLMANUALLINEITEMSBYID = "usp_P2P_PO_GetAllManualLineItemsById";
        internal const string USP_P2P_PO_UPDATELINEITEMSTATUS = "usp_P2P_PO_UpdateLineItemStatus";
        internal const string USP_P2P_PO_GETALLORDERSBYREQUISITIONID = "usp_P2P_PO_GetAllOrdersByRequisitionId";
        internal const string USP_P2P_PO_GETDOCUMENTBASICDETAILSDOCUMENTNUMBER = "usp_P2P_PO_GetDocumentBasicDetailsDocumentnumber";
        internal const string USP_P2P_PO_GETORDERFORNOTIFICATION = "usp_P2P_PO_GetOrderForNotification";
        internal const string USP_P2P_PO_GETORDERFYINOTIFICATION_ADHOCAPPROVAL = "usp_P2P_PO_GetOrderFYINotification_AdhocApproval";
        internal const string USP_P2P_PO_GETORDERITEMACCOUNTINGSTATUS = "usp_P2P_PO_GetOrderItemAccountingStatus";
        internal const string USP_P2P_PO_UPDATEPOITEMONPARTNERCHANGE = "usp_P2P_PO_UpdatePOItemOnPartnerChange";
        internal const string USP_P2P_PO_GETPREFERREDPARTNERBYORDERID = "usp_P2P_PO_GetPreferredPartnerByOrderId";
        internal const string USP_P2P_PO_GETORDERINFOFORNOTIFICATION = "usp_P2P_PO_GetOrderInfoForNotification";
        internal const string USP_P2P_PO_RESETORDERITEMNUMBERSEQUENCE = "usp_P2P_PO_ResetOrderItemNumberSequence";
        internal const string USP_P2P_PO_SAVEORDERITEMOTHER = "usp_P2P_PO_SaveOrderItemOther";
        internal const string USP_P2P_PO_UPDATETAXONLINEITEM = "usp_P2P_PO_UpdateTaxOnLineItem";
        internal const string USP_P2P_PO_UPDATETAXONHEADERWITHOUTTAXMASTER = "usp_P2P_PO_UpdateTaxOnHeaderWithoutTaxMaster";
        internal const string USP_P2P_PO_GETALLDOCUMENTITEMSCHECKEDSTATUS = "usp_P2P_PO_GetAllDocumentItemsCheckedStatus";
        internal const string USP_P2P_PO_UPDATEORDERITEMCHECKEDSTATUS = "usp_P2P_PO_UpdateOrderItemCheckedStatus";
        internal const string USP_P2P_PO_UPDATELINEITEMSHIPTOLOCATION = "usp_P2P_PO_UpdateLineItemShipToLocation";
        internal const string USP_P2P_PO_GETALLORDERREQUISITIONITEMSBYREQUISITIONID = "usp_P2P_PO_GetAllOrderRequisitionItemsByRequisitionId";
        internal const string USP_P2P_REC_GETORDERDEATILSFORRECEIPTINTERFACE = "usp_P2P_REC_GetOrderDeatilsForReceiptInterface";
        internal const string USP_P2P_REC_GETRECEIPTDEATILSFORRRTURNNOTE = "usp_P2P_REC_GetReceiptDeatilsForReturnNote";
        internal const string USP_P2P_PO_SAVEORDERADVANCEDITEM = "usp_P2P_PO_SaveAdvancedPaymentItem";
        internal const string USP_P2P_PO_GETORDERADVANCEDACCOUNTINGDETAILSBYITEMID = "USP_P2P_PO_GetOrderAdvancedAccountingDetailsByItemId";
        internal const string USP_P2P_PO_SAVE_ADV_ORDER_DEFAULT_ACCOUNTING = "usp_P2P_PO_SaveAdvancePaymentDefaultAccounting";
        internal const string USP_P2P_SAVEITEMNEEDBYDATE = "usp_P2P_saveItemNeedByDate";
        internal const string USP_P2P_CHECKCANCELADVITEMVALIDATION = "usp_P2P_CheckCancelAdvItemValidation";
        internal const string USP_P2P_PO_GetFOBPODetailsId = "usp_P2P_PO_GetFOBPODetailsById";
        internal const string USP_P2P_VALIDATEITEMMANDATORYQUESTIONRESPONSES = "usp_P2P_ValidateItemMandatoryQuestionResponses";
        internal const string USP_P2P_GETALLTOLERANCEDETAILS = "usp_P2P_GetAllToleranceDetails";
        internal const string USP_P2P_SAVEFLIPTOORDERSPLITSANDSHIPPINGDETAILS = "usp_P2P_SaveFlipToOrderSplitsAndShippingDetails";
        internal const string USP_P2P_REC_CANCELLINEITEM = "usp_P2P_REC_CancelLineItem";
        internal const string USP_P2P_REQITEMSHIPPINGDETAILS = "usp_P2P_SaveRequisitionItemShippingDetails";
        internal const string USP_P2P_REQITEMTAXDETAILS = "usp_P2P_SaveRequisitionItemTaxDetails";
        internal const string USP_P2P_REQ_VALIDATELINEITEMSONBUCHANGE = "usp_P2P_REQ_ValidateLineItemsOnBuChange";
        internal const string USP_P2P_PO_GetAllCancelOrderItemId = "USP_P2P_PO_GetAllCancelOrderItemId";
        internal const string USP_P2P_PO_GETSCORDERITEMSBYID = "usp_P2P_PO_GetSCOrderItemsById";
        internal const string USP_REQ_GETALLUSERSBYACTIVITYCODE = "usp_REQ_GetAllUsersByActivityCode";


        #endregion

        #region Order Tolerance Columns
        internal const string COL_TOLERANCE_ID = "ToleranceId";
        internal const string COL_ALLOW_TOLERANCE = "AllowTolerance";
        internal const string COL_ALLOW_TOLERANCE_EDIT = "AllowToleranceEdit";
        internal const string COL_ALLOW_TOLERANCE_FOR_BLANK = "AllowToleranceForBlank";
        internal const string COL_ITEM_TOTAL_PERCENT_TOLERANCE = "ItemTotalPercentTolerance";
        internal const string COL_ITEM_TOTAL_VALUE_TOLERANCE = "ItemTotalValueTolerance";
        internal const string COL_TOTAL_AMOUNT_PERCENT_TOLERANCE = "TotalAmountPercentTolerance";
        internal const string COL_TOTAL_AMOUNT_VALUE_TOLERANCE = "TotalAmountValueTolerance";
        internal const string COL_TAX_PERCENT_TOLERANCE = "TaxPercentTolerance";
        internal const string COL_TAX_VALUE_TOLERANCE = "TaxValueTolerance";
        internal const string COL_SHIPPING_PERCENT_TOLERANCE = "ShippingPercentTolerance";
        internal const string COL_SHIPPING_VALUE_TOLERANCE = "ShippingValueTolerance";
        internal const string COL_CHARGES_PERCENT_TOLERANCE = "ChargesPercentTolerance";
        internal const string COL_CHARGES_VALUE_TOLERANCE = "ChargesValueTolerance";
        internal const string COL_QUANTITY_PERCENT_TOLERANCE = "QuantityPercentTolerance";
        internal const string COL_QUANTITY_VALUE_TOLERANCE = "QuantityValueTolerance";
        internal const string COL_UNIT_PRICE_PERCENT_TOLERANCE = "UnitPricePercentTolerance";
        internal const string COL_UNIT_PRICE_VALUE_TOLERANCE = "UnitPriceValueTolerance";
        internal const string COL_PERCENTTOLERANCE = "PercentTolerance";
        internal const string COL_VALUETOLERANCE = "ValueTolerance";
        internal const string COL_REQACCESS = "REQACCESS";
        internal const string COL_RECACCESS = "RECACCESS";
        internal const string COL_LINEITEMSTATUS = "LineItemStatus";
        internal const string COL_INVOICEPERCENTVALUE = "InvoicePercentValue";
        internal const string COL_ORDERPERCENTVALUE = "OrderPercentValue";

        #endregion

        #region Order Tolerance StoredProcedures
        internal const string USP_P2P_PO_GETORDERTOLERANCEDETAILSBYID = "usp_P2P_PO_GetOrderToleranceDetailsById";
        internal const string USP_P2P_PO_SAVETOLERANCEDETAILS = "usp_P2P_PO_SaveToleranceDetails";
        #endregion

        #region Order SavePoTermsMapping StoredProcedures
        internal const string USP_P2P_PO_SAVEPOTERMSMAPPING = "usp_P2P_PO_SavePoTermsMapping";
        #endregion
        #region Order TableTypes
        internal const string TVP_P2P_ORDERITEM = "tvp_P2P_OrderItem";
        internal const string TVP_P2P_PO_ITEMS = "tvp_P2P_PO_Items";
        internal const string TVP_P2P_ORDERITEMS = "tvp_P2P_OrderItems";
        internal const string TVP_P2P_REQITEMS = "tvp_P2P_RequisitionItems";
        internal const string TVP_ITEMPARTNERDEATAILS = "tvp_ItemPartnerDeatails";
        internal const string TVP_P2P_ORDERITEMSHIPPINGDETAILS = "tvp_P2P_ItemShippingDetails";
        internal const string TVP_P2P_ITEMOTHERDETAILS = "tvp_P2P_ItemOtherDetails";
        internal const string TVP_P2P_ORDERTEMPITEMS = "tvp_P2P_PO_TempLineItems";
        internal const string TVP_P2P_ORDERNOTESORATTACHMENTS = "tvp_P2P_OrderItemsNotesOrAttachments";
        internal const string TVP_LONG = "tvp_Long";
        internal const string TVP_P2P_DocumentItems = "tvp_P2P_DocumentItems";
        internal const string TVP_P2P_Document = "tvp_P2P_Document";
        internal const string TVP_P2P_PartnerItems = "tvp_P2P_PartnerItems";


        internal const string TVP_P2P_REQITEMSHIPPING = "tvp_P2P_RequisitionItemShipping";
        internal const string TVP_P2P_REQTAXDETAILS = "tvp_P2P_RequisitionItemTax";
        internal const string TVP_P2P_FOBHEADERENTITYDETAILS = "tvp_P2P_FOBHeaderEntityDetails";
        internal const string TVP_P2P_SRFQUESTIONNAIRE = "tvp_P2P_SRFQuestionnaire";


        #endregion

        #region Interface Order TableTypes
        internal const string TVP_P2P_HEADERBILLTO = "tvp_P2P_HeaderBillTo";
        internal const string TVP_P2P_HEADERSHIPTO = "tvp_P2P_HeaderShipTo";
        internal const string TVP_P2P_LINELEVELSHIPTO = "tvp_P2P_LinelevelShipTo";
        internal const string TVP_P2P_HEADERDELIVERTO = "tvp_P2P_HeaderDeliverTo";
        internal const string TVP_P2P_LINELEVELDELIVERTO = "tvp_P2P_LinelevelDeliverTo";



        #endregion

        #region Order DispatchHistory
        internal const string COL_ACTION = "ActionInfo";
        internal const string COL_PERFORMEDBY = "PerformedBy";
        internal const string COL_DATEPERFORMED = "DatePerformed";
        internal const string COL_ADDITIONALINFO = "AdditionalInfo";
        #endregion

        internal const string COL_TAXES = "Taxes";
        internal const string COL_SHIPPINGCHARGES = "Shipping Charges";
        internal const string COL_OTHER_CHARGES = "Other Charges";
        #endregion

        #region Common Entity
        #region Common Columns
        internal const string COL_LOCATIONID = "LocationId";
        internal const string COL_APPROVAL_STATUS = "ApprovalStatus";
        internal const string COL_PROCUREMENT_STATUS = "ProcurementStatus";
        internal const string COL_PARTNER_CODE = "PartnerCode";
        internal const string COL_PARTNER_CONTACT_CODE = "PartnerContactCode";
        internal const string COL_PARTNER_CONTACT_EMAIL = "PartnerContactEmail";
        internal const string COL_PARTNER_CONTACTID = "PartnerContactId";
        internal const string COL_PARTNER_CONTACT_NUMBER = "PartnerContactNumber";

        internal const string COL_PARTNER_CONTACT_FAX = "PartnerContactFax";
        internal const string COL_PARTNER_CONTACT_PHONE = "PartnerContactPhone";
        internal const string COL_PARTNER_CONTACT_ADDRESS1 = "PCAaddress1";
        internal const string COL_PARTNER_CONTACT_ADDRESS2 = "PCAaddress2";
        internal const string COL_PARTNER_CONTACT_ADDRESS3 = "PCAaddress3";
        internal const string COL_PARTNER_CONTACT_CITY = "PCACity";
        internal const string COL_PARTNER_CONTACT_STATE = "PCAState";
        internal const string COL_PARTNER_CONTACT_ZIP = "PCAZip";
        internal const string COL_PARTNER_CONTACT_COUNTRY = "PCACountry";
        internal const string COL_PARTNER_CONTACT_COUNTRYCODE = "PCACountryCode";
        internal const string COL_PARTNER_CONTACT_STATECODE = "PCAStateCode";
        internal const string COL_PARTNER_INVOICE_NUMBER = "PartnerInvoiceNumber";
        internal const string COL_COMPANY_NAME = "CompanyName";
        internal const string COL_CONTACT_CODE = "ContactCode";
        internal const string COL_EMAIL_ID = "EmailId";
        internal const string COL_PHONE_NO = "PhoneNo";
        internal const string COL_EXTENSION_NO1 = "ExtensionNo1";
        internal const string COL_EXTENSION_NO2 = "ExtensionNo2";
        internal const string COL_MOBILE_NO = "MobileNo";
        internal const string COL_PHONE_NO1 = "PhoneNo1";
        internal const string COL_PHONE_NO2 = "PhoneNo2";
        internal const string COL_CURRENCY = "CurrencyCode";
        internal const string COL_ALLOCATEDBUDGET = "AllocatedBudget";
        internal const string COL_SHIPTOLOCATION_ID = "ShiptoLocationID";
        internal const string COL_DELIVERTOLOCATION_ID = "DelivertoLocationId";
        internal const string COL_BILLTOLOCATIONID = "BilltoLocationID";
        internal const string COL_PAYMENTTERMS = "PaymentTerms";
        internal const string COL_PAYMENTTERMSID = "PaymentTermsId";
        internal const string COL_TAX = "Tax";
        internal const string COL_UseTax = "Tax";
        internal const string COL_ISTAXEXEMPT = "IsTaxExempt";
        internal const string COL_SHIPPING = "Shipping";
        internal const string COL_ADDITIONAL_CHARGES = "AdditionalCharges";
        internal const string COL_IS_DELETED = "IsDeleted";
    internal const string COL_SOURCEDOCUMENTTYPEID = "SourceDocumentTypeId";
    internal const string COL_DATE_CREATED = "DateCreated";
        internal const string COL_DATE_MODIFIED = "DateModified";
        internal const string COL_DATE_SUBMITTED = "DateSubmitted";
        internal const string COL_SUBMITTED_BY = "SubmittedBy";
        internal const string COL_SUBMITTED_ON = "SubmittedOn";
        internal const string COL_CREATED_BY = "CreatedBy";
        internal const string COL_MODIFIED_BY = "ModifiedBy";
        internal const string COL_BANDING = "Banding";
        internal const string COL_MAX_ORDER_QUANTITY = "MaximumOrderQuantity";
        internal const string COL_MIN_ORDER_QUANTITY = "MinimumOrderQuantity";
        internal const string COL_APPROVER_NAME = "ApproverName";
        internal const string COL_DOCUMENT_SOURCE = "DocumentSource";
        internal const string COL_ITEM_TOTAL = "ItemTotal";
        internal const string COL_SHIPPING_CHARGES = "ShippingCharges";
        internal const string COL_ItemImageFileId = "FileId";
        internal const string COL_ItemImageFileName = "FileName";
        internal const string COL_ItemImageUri = "FileUri";
        internal const string COL_BUCODE = "BUCode";
        internal const string COL_UNSPSCKEY = "UNSPSCKey";
        internal const string COL_SPLITITEMID = "SplitItemId";
        internal const string COL_ACCRUEUSETAX = "AccrueUseTax";
        internal const string COL_AMOUNT = "Amount";
        internal const string COL_BUYERCOMPANYNAME = "BuyerCompanyName";
        internal const string COL_LINKEDDOCUMENTCODE = "LinkedDocumentCode";
        internal const string COL_REQUESTERID = "RequesterID";
        internal const string COL_PAYMENTTERMID = "PaymentTermId";
        internal const string COL_PAYMENTTERMNAME = "PaymentTermName";
        internal const string COL_PAYMENTTERMCODE = "PaymentTermCode";
        internal const string COL_PAYMENTTERMDISCOUNT = "Discount";
        internal const string COL_PAYMENTTERMDISCOUNTDAYS = "DiscountDays";
        internal const string COL_PAYMENTTERMNOOFDAYS = "NoOfDays";
        internal const string COL_P2PITEMID = "P2PItemId";
        internal const string COL_CONTRACTNO = "ContractNo";
        internal const string COL_CONTRACTSTARTDATE = "ContractStartDate";
        internal const string COL_CONTRACTENDDATE = "ContractEndDate";
        internal const string COL_CONTRACTDATEEXPIRY = "ContractDateExpiry";
        internal const string COL_CONTRACTVALUE = "ContractValue";
        internal const string COL_CONTRACTVOLUME = "ContractVolume";
        internal const string COL_CONTRACTNAME = "ContractName";
        internal const string COL_CONTRACTSTATUS = "ContractStatus";
        internal const string COL_COMMENTCOUNT = "CommentCount";
        internal const string COL_EXCEPTIONS = "Exceptions";
        internal const string COL_IMAGEID = "ImageId";
        internal const string COL_CLIENTCATEGORYID = "ClientCategoryId";
        internal const string COL_POCONTACTCODE = "POContactCode";
        internal const string COL_CLIENT_PARTNERCODE = "ClientPartnerCode";
        internal const string COL_PARTNERSTATUSDISPLAYNAME = "PartnerStatusDisplayName";
        internal const string COL_ISURGENT = "IsUrgent";
        internal const string COL_DELIVERTO = "DeliverTo";
        internal const string COL_ENTITYDETAILCODE = "EntityDetailCode";
        internal const string COL_FULLNAME = "FullName";
        internal const string COL_NEXTLINENUMBER = "NextLineNumber";
        internal const string COL_CANCELLED_DATE = "CancelledDate";
        internal const string COL_CLOSED_DATE = "ClosedDate";
        internal const string COL_IS_RESOLVED = "IsResolved";
        internal const string COL_CONTACT_NUMBER = "ContactNumber";
        internal const string COL_CREATED_DATE = "CreatedDate";
        internal const string COL_ISREASSIGNED = "IsReassigned";
        internal const string COL_ISREVIEW = "IsReview";
        internal const string COL_IS_PCARDSUPPORTED = "IsPCardSupported";
        internal const string COL_ISCATALOGITEM = "IsCatalogItem";
        internal const string COL_SUPPLIERPARTNUMBER = "SupplierPartNumber";
        internal const string COL_PARENTDOCUMENTNUMBER = "ParentDocumentNumber";
        internal const string COL_ADDRESSCODE = "AddressCode";
        internal const string COL_PHONENO1 = "PhoneNo1";
        internal const string COL_EXTENSIONNO1 = "ExtensionNo1";
        internal const string COL_PHONENO2 = "PhoneNo2";
        internal const string COL_EXTENSIONNO2 = "ExtensionNo2";
        internal const string COL_FAXNO = "FaxNo";
        internal const string COL_SRCITEMID = "SrcItemId";
        internal const string COL_TGTITEMID = "TgtItemId";
        internal const string COL_GTIN = "GTIN";
        internal const string COL_ContractItemLineNumber = "ContractItemLineNumber";
        internal const string COL_ITEMADVANCEAMOUNT = "ItemAdvanceAmount";
        internal const string COL_ADVANCETOTAL = "AdvanceTotalAmount";
        internal const string COL_RECOUPMENTPERCENTAGE = "RecoupmentPercentage";
        internal const string COL_ADJUSTMENTTYPE = "AdjustmentType";
        internal const string COL_TOTALRECOUPMENTAMOUNT = "TotalRecoupmentamount";
        internal const string COL_REMAININGADVANCEAMOUNT = "RemainingAdvanceAmount";
        internal const string COL_REMAININGADVADJAMOUNT = "RemainingAdvAdjAmount";
        internal const string COL_RECOUPMENTRATE = "RecoupmentRate";
        internal const string COL_OVERALLTOTALAMOUNT = "OverallTotalAmount";
        internal const string COL_UPDATEDBY = "UpdatedBy";
        internal const string COL_UPDATEDON = "UpdatedOn";
        internal const string COL_UPDATEDVIA = "UpdatedVia";
        internal const string COL_ROLE = "Role";
        internal const string COL_ORG_ENTITYCODE = "OrgEntityCode";
        internal const string COL_USERDEFINEDAPPROVALCOUNT = "CountOfApprovers";
        internal const string COL_ItemPASCodes = "ItemPASCodes";
        internal const string COL_DEFAULTCURRENCYCODE = "DefaultCurrencyCode";
        internal const string COL_ENTITYID = "EntityId";
        internal const string COL_SOURCEENTITYID = "SourceEntityId";
        internal const string COL_ENTITYDISPLAYNAME = "EntityDisplayName";
        internal const string COL_ENTITYCODE = "EntityCode";
        internal const string COL_ENTITYDESCRIPTION = "EntityDescription";
        internal const string COL_ISACTIVE = "IsActive";
        internal const string COL_PARENTENTITYDETAILCODE = "ParentEntityDetailCode";
        internal const string COL_PARENT_ENTITY_DETAIL_CODESTRING = "ParentEntityDetailCodeString";
        internal const string COL_ENTITYKEY = "EntityKey";
        internal const string COL_ENTITYDEFAULTNAME = "EntityDefaultName";
        internal const string COL_ISLOB = "IsLOB";
        internal const string COL_ISBRIDGE = "IsBridge";
        internal const string COL_ISORGENTITY = "IsOrgEntity";
        internal const string COL_Extended_Error_Code = "ExtendedErrorCode";
        internal const string COL_FIELDFORASSET = "FileIdForAsset";
        internal const string COL_FILENAMEFORASSET = "FileNameForAsset";
        internal const string COL_PASNAME = "PASName";
        internal const string COL_CLIENT_PASCODE = "Client_PASCode";
        internal const string COL_PASCODE = "PASCode";
        internal const string COL_TAXINTEGRATION = "TaxIntegration";
        internal const string COL_DIVISION = "Division";
        internal const string COL_INVOICESOURCEID = "InvoiceSourceId";
        internal const string COL_TOTALCOUNT = "TotalCount";

        internal const string COL_ORGMANAGERMAPPINGID = "OrgManagerMappingId";

        internal const string COL_PRICETYPEID = "PriceTypeId";
        internal const string COL_PRICETYPENAME = "PriceTypeName";
        internal const string COL_PRICETYPECODE = "PriceTypeCode";
        internal const string COL_JOBTITLEID = "JobTitleId";
        internal const string COL_JOBTITLENAME = "JobTitleName";
        internal const string COL_CREATEDATE = "CreateDate";
        internal const string COL_UPDATEDDATE = "UpdatedDate";
        internal const string COL_CONTINGENTWORKERID = "ContingentWorkerId";
        internal const string COL_MARGIN = "Margin";
        internal const string COL_BASERATE = "BaseRate";
        internal const string COL_CONTINGENTWORKERNAME = "ContingentWorkerName";
        internal const string COL_ITEMTYPEID = "ItemTypeId";
        internal const string COL_REPORTINGMANAGERID = "ReportingManagerId";
        internal const string COL_REPORTINGMANAGERNAME = "ReportingManagerName";
        internal const string COL_CONTINGENTWORKERFIRSTNAME = "ContingentWorkerFirstName";
        internal const string COL_CONTINGENTWORKERLASTNAME = "ContingentWorkerLastName";
        internal const string COL_REPORTINGMANAGERFIRSTNAME = "ReportingManagerFirstName";
        internal const string COL_RREPORTINGMANAGERLASTNAME = "ReportingManagerLastName";
        internal const string COL_ISSTOCKREQUISITION = "IsStockRequisition";
        internal const string COL_STOCKRESERVATIONNUMBER = "StockReservationNumber";
        internal const string COL_SMARTFORMID = "SmartFormId";
        internal const string COL_SPENDCONTROLDOCUMENTCODE = "SpendControlDocumentCode";
        internal const string COL_SPENDCONTROLDOCUMENTITEMID = "SpendControlDocumentItemId";
        internal const string COL_SPENDCONTROLDOCUMENTNUMBER = "SpendControlDocumentNumber";
        internal const string COL_SPENDCONTROLDOCUMENTNAME = "SpendControlDocumentName";
        internal const string COL_SPENDCONTROLDOCUMENTITEMREFERENCENUMBER = "SpendControlDocumentItemReferenceNumber";
        internal const string COL_CATEGORYHIERARCHY = "CategoryHierarchy";
        internal const string COL_ENABLERISKFORM = "EnableRiskForm";
        internal const string COL_DOCUMENTNUMBER = "DocumentNumber";
        internal const string COL_ADDITIONALFIELDIDS = "AdditionalFieldIds";
    #endregion

    #region Common Entity StoreProcedures
    internal const string USP_P2P_GETALLSUPPLIERLOCBYLOBAndOrgEntity = "usp_P2P_GetAllSupplierLocationsByLOBandOrgEntity";
        internal const string USP_P2P_GETALLSUPPLIERSBYLOBAndOrgEntity = "usp_P2P_GetAllSupplierContactsByLOBandOrgEntity";
        internal const string USP_P2P_GETALLCATEGORIESBYLOB = "usp_P2P_GetAllCategoriesByLOB";
        internal const string USP_P2P_GETALLUOMS = "usp_P2P_GetAllUOMs";
        internal const string USP_P2P_GETALLSHIPPINGMETHODS = "usp_P2P_GetAllShippingMethods";
        internal const string USP_P2P_GETDEFAULTSHIPPINGMETHOD = "usp_P2P_GetDefaultShippingMethod";
        internal const string USP_P2P_GETALLSHIPTOLOCATIONS = "usp_P2P_GetAllShipToLocations";
        internal const string USP_P2P_GETDEFAULTSHIPTOLOCATIONSBYLOBBU = "usp_P2P_GetDefaultShipToLocationsByLOBBU";
        internal const string USP_P2P_GETSHIPTOLOCATIONBYID = "usp_P2P_GetShipToLocationById";
        internal const string USP_P2P_GETALLBILLTOLOCATIONS = "usp_P2P_GetAllBillToLocations";
        internal const string USP_WF_CHECKORIGINALAPPROVERNOTIFICATIONSTATUS = "usp_WF_CheckOriginalApproverNotificationStatus";
        internal const string USP_WF_GETDOCUMENTLISTFORDELEGATEAPPROVERS = "usp_WF_GetDocumentListForDelegateApprovers";
        internal const string USP_P2P_GETBILLTOLOCATIONBYID = "usp_P2P_GetBillToLocationById";
        internal const string USP_P2P_GETALLDOCUMENTS = "usp_P2P_GetAllDocuments";
        internal const string USP_P2P_GETSHIPTOLOCATIONIDBYADDRESS = "usp_P2P_GetShipToLocationIdByAddress";
        internal const string USP_P2P_INSERTSHIPTOLOCATION = "usp_P2P_InsertShiptoLocation";
        internal const string USP_P2P_INSERTSHIPTOLOCATIONFROMINTERFACE = "usp_P2P_InsertShiptoLocationFromInterface";
        internal const string USP_BZ_INSERTPARTNERINTERFACE_DOCUMENT_MAPPING = "usp_BZ_InsertPartnerInterfaceDocumentMapping";
        internal const string USP_P2P_VALIDATEINTERNALCATALOGITEMS = "usp_P2P_ValidateInternalCatalogItems";
        internal const string USP_P2P_UPDATESHIPTOLOCATION = "usp_P2P_UpdateShiptoLocation";
        internal const string USP_P2P_VALIDATEPAYMENTTERMS = "usp_P2P_ValidatePaymentTerms";
        internal const string USP_P2P_SAVEFLIPPEDQUESTIONSRESPONSESFROMINTERFACE = "usp_P2P_SaveFlippedQuestionResponsesFromInterface";
        internal const string USP_P2P_GETBILLTOLOCBYNAME = "usp_P2P_GetBillToLocationByName";
        internal const string USP_P2P_GETBILLTOLOCATIONIDBYADDRESS = "usp_P2P_GetBillToLocationIdByAddress";
        internal const string USP_P2P_SAVEBILLTOLOCATION = "usp_P2P_SaveBillToLocation";
        internal const string USP_P2P_GETALLSHIPTOLOCATIONBYNUMBER = "usp_P2P_GetAllShipToLocationByNumber";
        internal const string USP_P2P_UPDATEBASECURRENCY = "usp_P2P_UpdateBaseCurrency";
        internal const string USP_P2P_REQ_UPDATEBASECURRENCY = "usp_P2P_REQ_UpdateBaseCurrency";
        internal const string USP_P2P_GETDEFAULTBILLTOLOCATION = "usp_P2P_GetDefaultBillToLocation";
        internal const string USP_P2P_VALIDATEALLACCOUNTINGCODECOMBINATION = "usp_P2P_ValidateAllAccountingCodeCombination";
        internal const string USP_P2P_GETALLBILLTOLOCATIONBYNUMBER = "usp_P2P_GetAllBillToLocationByNumber";
        internal const string USP_P2P_GETALLERPORDERTYPES = "usp_P2P_GetAllERPOrderTypes";
        internal const string USP_P2P_GETRELATEDDOCUMENTSFORCOMMENTS = "usp_P2P_GetRelatedDocumentsForComments";
        internal const string USP_P2P_GETALLBUDGETID = "usp_P2P_GetAllBudgetIds";
        internal const string USP_P2P_GETDOCUMENTLISTFORAUTOACCEPTIR = "usp_P2P_GetDocumentListForAutoAcceptIR";
        internal const string USP_P2P_CHECKHASRULEFORUSERDEFINEDAPPROVER = "usp_P2P_CheckHasRuleForUserDefinedApprover";
        internal const string USP_P2P_CHECKDATAPRESENTFORSTATICAPPROVAL = "usp_P2P_CheckDataPresentForStaticApproval";
        internal const string USP_P2P_GETRULEIDSMARKEDOFFLINEFORSTATICAPPROVAL = "usp_P2P_GetRuleIdsMarkedOfflineForStaticApproval";
        internal const string USP_WF_CHECKORIGINALREVIEWERNOTIFICATIONSTATUS = "usp_WF_CheckOriginalReviewerNotificationStatus";

        internal const string USP_P2P_GETRELATEDLINEITEMSFORCOMMENTS = "usp_P2P_GetRelatedLineItemsForComments";
        internal const string USP_P2P_GETAVAILABLEFUNDS = "usp_p2p_GetAvailableFunds";
        internal const string USP_P2P_GETALLUOMSBYCATALOGITEMID = "usp_P2P_GetAllUOMsByCatalogItemId";
        internal const string USP_P2P_GETCONTRACTDATABYCATALOGITEMID = "usp_P2P_GetContractDataByCatalogItemId";
        internal const string USP_P2P_CATALOGCONTRACTEXPIRY = "usp_P2P_CatalogContractExpiry";
        internal const string USP_P2P_GETDELIVERTOLOCATIONBYID = "usp_P2P_GetDeliverToLocationById";
        internal const string USP_P2P_GETALLDELIVERTOLOCATIONS = "usp_P2P_GetAllDeliverToLocations";
        internal const string USP_P2P_GETDELIVERTOLOCATIONBYNUMBER = "usp_P2P_GetDeliverToLocationByNumber";
        internal const string USP_BZ_GETPARTNERCODEBYCONFIGURATIONID = "usp_BZ_GetPartnerCodeByConfigurationId";
        internal const string USP_P2P_GETFIRSTACCOUNTINGSPLITREQUESTER = "usp_P2P_GetFirstAccountingSplitRequester";
        internal const string USP_P2P_GETORGENTITYMAPPEDWITHCATALOGITEM = "USP_P2P_GetOrgEntityMappedWithCatalogItem";
        internal const string USP_P2P_SAVEADHOCUSERS = "usp_P2P_SaveAdhocUsers";
        internal const string USP_P2P_APPLYDATATOALLSELECTEDLINEITEMS = "usp_P2P_SelectiveApplyToAll";
        internal const string USP_P2P_GETBUBASEDREQUESTER = "usp_P2P_GetBUBasedRequester";
        internal const string USP_P2P_GETCONTACTDETAILSBYCONTACTCODE = "usp_P2P_GetContactDetailsByContactCode";
        internal const string USP_P2P_REQSELECTIVEAPPLYTOALLBYRATIO = "usp_P2P_REQSelectiveApplyToAllByRatio";
        internal const string USP_P2P_POSELECTIVEAPPLYTOALLBYRATIO = "usp_P2P_POSelectiveApplyToAllByRatio";
        internal const string USP_P2P_INVOICESELECTIVEAPPLYTOALLBYRATIO = "usp_P2P_InvoiceSelectiveApplyToAllByRatio";
        internal const string USP_P2P_GETPARTNERDETAILSFORINTERFACE = "usp_P2P_GetPartnerDetailsForInterface";
        internal const string USP_WF_GETDOCUMENTSFORDELEGATION = "usp_WF_GetDocumentsForDelegation";
        internal const string USP_WF_SAVEDOCUMENTDELEGATIONREQUEST = "usp_WF_SaveDocumentDelegationRequest";
        internal const string USP_WF_RESETDOCUMENTDELEGATIONREQUEST = "usp_WF_ResetDocumentDelegationRequest";
        internal const string USP_WF_SETDOCUMENTDELEGATIONREQUEST = "usp_WF_SetDocumentDelegationRequest";
        internal const string USP_P2P_GETPARTERCONTACTSBYLOCATIONID = "usp_P2P_GetPartercontactsByLocationId";
        internal const string USP_P2P_GETPARTNERANDCONTACTCODEDETAILS = "usp_P2P_GetPartnerAndContactCodeDetails";
        internal const string USP_PRN_GETPARTNERCONATACTBYPARTNERCODEANDLOCATIONCODE = "usp_PRN_GetPartnerConatactByPartnerCodeAndLocationCode";
        internal const string USP_P2P_GETALLSHIPTOLOCATIONBYNAMES = "usp_P2P_GetAllShipToLocationByNames";
        internal const string USP_P2P_GETALLDELIVERTOLOCATIONBYNAMES = "usp_P2P_GetAllDeliverToLocationByNames";
        internal const string USP_P2P_GETALLBILLTOLOCATIONBYNAMES = "usp_P2P_GetAllBillToLocationByNames";
        internal const string USP_P2P_GETALLUOMBYUOMS = "usp_P2P_GetAllUOMByUOMs";
        internal const string USP_P2P_GETACTIVEPAYMENTTERMS = "usp_P2P_GetActivePaymentTerms";
        internal const string USP_P2P_GETUOMDETAILSBYSTANDARDUOM = "usp_P2P_GetUOMDetailsByStandardUOM";
        internal const string USP_P2P_GETPURCHASETYPES = "usp_P2P_GetPurchaseTypes";
        internal const string USP_P2P_REQ_GETPURCHASETYPES = "usp_P2P_REQ_GetPurchaseTypes";
        internal const string USP_P2P_GETPURCHASETYPEITEMEXTENDEDTYPEMAPPING = "usp_P2P_GetPurchaseTypeItemExtendedTypeMapping";
        internal const string usp_P2P_REQ_GetPurchaseTypeFeatures = "usp_P2P_REQ_GetPurchaseTypeFeatures";
        internal const string USP_P2P_GETITEMIDMAPPING = "usp_P2P_GetItemIdMapping";
        internal const string USP_P2P_GETALLSPLITITEMIDSBYDOCUMENTID = "usp_P2P_GetAllSplitItemIdsByDocumentId";
        internal const string USP_P2P_GETITEMFORMMAPPINGBYCATALOGID = "usp_P2P_GetItemFormMappingByCatalogId";
        internal const string USP_P2P_GETALLITEMIDSBYDOCUMENTID = "usp_P2P_GetAllItemIdsByDocumentId";
        internal const string USP_P2P_CUSTOMATTRFORMID = "usp_P2P_CustomAttrFormId";
        internal const string USP_P2P_UPDATEALLPERIODONSPLITSBYNEEDBYDATE = "usp_P2P_UpdateAllPeriodonSplitsbyNeedbyDate";
        internal const string USP_P2P_GetListOfShipToLocations = "usp_P2P_GetListOfShipToLocations";
        internal const string USP_P2P_GetListOfBillToLocations = "usp_P2P_GetListOfBillToLocations";
        internal const string USP_P2P_GetListOfCatalogItemIdsNotAllowedAccess = "usp_P2P_GetListOfCatalogItemIdsNotAllowedAccess";
        internal const string USP_P2P_UPDATEPERIODONSPLITSBYNEEDBYDATE = "usp_P2P_UpdatePeriodonSplitsbyNeedbyDate";
        internal const string USP_P2P_UPDATEACCOUNTINGSTATUS = "usp_P2P_UpdateAccountingStatus";
        internal const string USP_P2P_GETIDENTIFICATIONNUMBERVALUES = "usp_P2P_GetIdentificationValues";
        internal const string USP_P2P_GETUSERCONFIGURATIONS = "USP_P2P_GetUserConfigurations";
        internal const string USP_P2P_SAVEUSERCONFIGURATIONS = "USP_P2P_SaveUserConfigurations";
        internal const string USP_P2P_GetListOfCatalogItemIdsNotAllowedAccessLatestNow = "[USP_P2P_GetListOfCatalogItemsIdsNotAllowedAccess]";
        internal const string USP_P2P_REMOVECONTRACTNOONBUCHANGE = "usp_P2P_RemoveContractNoOnBUChange";
        internal const string USP_P2P_PO_GETALLSHIPTOLOCATIONSMASTERLIST = "usp_P2P_GetAllShiptoLocationsMasterList";
        internal const string USP_P2P_ValidateDocumentCheck = "usp_P2P_ValidateDocumentCheck";
        internal const string USP_P2P_GETSHIPTOLOCATIONBYDEFAULTMAPPEDENTITY = "usp_P2P_GetShipToLocationByDefaultMappedEntity";
        internal const string USP_P2P_GETALLADDITIONALFIELDS = "USP_P2P_GetAllEntityFieldsDetail";
        internal const string USP_P2P_GETLOBBYDOCUMENTCODE = "usp_P2P_GetLOBByDocumentCode";
        internal const string USP_GET_ENTITY_SEARCH_RESULTS = "usp_P2P_GetEntityDetailsBySearchResults";
        internal const string USP_REQ_GETMAPPEDQUESTIONS = "USP_REQ_GetMappedQuestions";
        internal const string USP_P2P_GETALLINVOICEACTIONS = "usp_P2P_GetAllInvoiceActions";
        internal const string USP_P2P_GETORGENTITIES = "usp_P2P_GetOrgEntities";
        internal const string USP_P2P_GETALLPARTNERLOCATIONIDENFITICATIONDETAILS = "usp_P2P_GetAllPartnerLocationIdenfiticationDetails";
        internal const string USP_P2P_REQ_GETREQUISITIONDETAILSFORBULKUPLOADREQLINES = "usp_P2P_REQ_GetRequisitionDetailsForBulkUploadReqLines";
        internal const string USP_P2P_REQ_VALIDATEITEMNUMBER = "usp_P2P_REQ_ValidateItemNumber";
        internal const string USP_P2P_GETTAXINTEGRATIONBYMAPPEDENTITY = "usp_P2P_GetTaxintegrationByMappedEntity";
        internal const string USP_P2P_INSERTDELIVERTOLOCATION = "usp_P2P_InsertDelivertoLocation";
        internal const string USP_P2P_GETPRICETYPE = "usp_P2P_GetPriceType";
        internal const string USP_P2P_GETJOBTITLE = "usp_P2P_GetJobTitle";
        internal const string USP_P2P_CM_UPDATEERPCREDITMEMONUMBER = "usp_P2P_CM_UpdateERPCreditMemoNumber";
        internal const string USP_P2P_CM_GETCREDITMEMOITEMS = "USP_P2P_CM_GETCREDITMEMOITEMS";
        internal const string USP_P2P_CM_GETINVOICEITEMS = "USP_P2P_CM_GETINVOICEITEMS";
        internal const string USP_P2P_CM_GETINVOICESPLITITEMS = "USP_P2P_CM_GETINVOICESPLITITEMS";
        internal const string USP_P2P_CM_GETDOCUMENTSPLITITEMENTITY = "USP_P2P_CM_GETDOCUMENTSPLITITEMENTITY";
        internal const string USP_P2P_GETCURRENCYEXCHANGERATEDETAILSFROMSOURCETOTARGET = "usp_CurrencyExchangeRateFromSourceToTarget";
        internal const string usp_P2P_GetAdditionalFieldsByDocumentType = "usp_P2P_GetAdditionalFieldsByDocumentType";

        #region Combination Code
        internal const string USP_SaveAccountingCombinationCode = "usp_SaveAccountingCombinationCode";
        internal const string USP_SAVEACCOUNTINGCOMBINATIONCODEFORINTERFACE = "usp_SaveAccountingCombinationCodeForInterface";
        internal const string USP_GetAllCombinationCode = "usp_GetAllAccountingCombinationCodes";
        internal const string USP_UpdateLastReadTime = "usp_UpdateLastReadTime";
        internal const string USP_ValidateOrgEntity = "usp_ValidateOrgEntity";
        #endregion

        #endregion

        #region Common Entity Columns

        // UOM
        internal const string COL_UOM_CODE = "UOMCode";
        internal const string COL_UOM_DESCRIPTION = "UOMDescription";
        internal const string COL_UOM_ALLOWDECIMAL = "AllowDecimal";
        internal const string COL_ERPORDERTYPEID = "ERPOrderTypeID";
        internal const string COL_ERPORDERTYPE = "ERPOrderType";
        internal const string COL_WORKORDERNO = "WorkOrderNo";
        internal const string COL_CAPITALCODE = "CapitalCode";
        internal const string COL_EXTCONTRACTREF = "ExtContractRef";
        internal const string COL_BUDGETID = "BudgetId";
        internal const string COL_BUDGETIDDESC = "BudgetIdDesc";
        internal const string COL_INVOICEACTIONID = "InvoiceActionId";
        internal const string COL_ACTIONNAME = "Action";

        // ShippingMethod
        internal const string COL_SHIPPINGMETHOD_NAME = "ShippingMethodName";
        internal const string COL_SHIPPINGMETHOD = "ShippingMethod";

        //ShipToLocations
        internal const string COL_SHIPTOLOC_ID = "ShiptoLocationID";
        internal const string COL_SHIPTOLOC_NAME = "ShiptoLocationName";
        internal const string COL_SHIPTOLOC_IsDefault = "IsDefault";
        internal const string COL_SHIPTOLOC_NUMBER = "ShiptoLocationNumber";
        internal const string COL_SHIPTOLOC_ISADHOC = "IsAdhoc";
        internal const string COL_SHIPTOLOC_ALLOWFORFUTUREREFERENCE = "AllowForFutureReference";
        internal const string COL_SHIPTOLOC_CONTACTPERSON = "ContactPerson";
        internal const string COL_ADDRESS1 = "AddressLine1";
        internal const string COL_ADDRESS2 = "AddressLine2";
        internal const string COL_ADDRESS3 = "AddressLine3";
        internal const string COL_CITY = "City";
        internal const string COL_STATE = "State";
        internal const string COL_ZIP = "Zip";
        internal const string COL_COUNTRY = "Country";
        internal const string COL_COUNTRYCODE = "CountryCode";
        internal const string COL_STATECODE = "StateCode";
        internal const string COL_STATENAME = "StateName";
        internal const string COL_SHIPTO_MAPPINGENTITY = "MappingEntity";
        internal const string COL_SHIPTO_COUNTRYNAME = "CountryName";

        internal const string COL_ExternalIntegrationConfig = "ExternalIntegrationConfig";
        //Bill to location
        internal const string COL_BILLTOLOC_ID = "BilltoLocationID";
        internal const string COL_BILLTOLOC_NAME = "BilltoLocationName";
        internal const string COL_BILLTOLOC_NUMBER = "BilltoLocationNumber";
        internal const string COL_ISDEFAULT = "IsDefault";
        internal const string COL_ISDELETE = "IsDeleted";
        internal const string COL_ADDRESSID = "AddressID";
        internal const string BILLTO_COL_ADDRESS1 = "BillToAddressLine1";
        internal const string BILLTO_COL_ADDRESS2 = "BillToAddressLine2";
        internal const string BILLTO_COL_ADDRESS3 = "BillToAddressLine3";
        internal const string BILLTO_COL_CITY = "BillToCity";
        internal const string BILLTO_COL_STATE = "BillToState";
        internal const string BILLTO_COL_STATECODE = "BillToStateCode";
        internal const string BILLTO_COL_COUNTRYCODE = "BillToCountryCode";
        internal const string BILLTO_COL_COUNTRYNAME = "BillToCountryName";
        internal const string BILLTO_COL_ZIP = "BillToZip";
        internal const string BILLTO_COL_FAXNO = "BillToFaxNo";
        internal const string BILLTO_COL_EmailAddress = "BillToEmailAddress";
        //Ordering Location
        internal const string OL_COL_LocationId = "LocationId";
        internal const string OL_COL_LocationCode = "ClientLocationCode";
        internal const string OL_COL_LocationName = "LocationName";
        internal const string OL_COL_ADDRESS1 = "OLAddressLine1";
        internal const string OL_COL_ADDRESS2 = "OLAddressLine2";
        internal const string OL_COL_ADDRESS3 = "OLAddressLine3";
        internal const string OL_COL_CITY = "OLCity";
        internal const string OL_COL_STATE = "OLState";
        internal const string OL_COL_ZIP = "OLZip";
        internal const string OL_COL_COUNTRY = "OLCountry";
        internal const string OL_COL_COUNTRYCODE = "OLCountryCode";
        internal const string OL_COL_STATECODE = "OLStateCode";
        internal const string OL_COL_STATENAME = "OLState";
        //remit To Location
        internal const string RL_COL_LocationId = "RemitTolocationID";
        internal const string RL_COL_LocationName = "RemitToLocationName";
        internal const string RL_COL_ADDRESS1 = "RemitToAddressLine1";
        internal const string RL_COL_ADDRESS2 = "RemitToAddressLine2";
        internal const string RL_COL_ADDRESS3 = "RemitToAddressLine3";
        internal const string RL_COL_CITY = "RemitToCity";
        internal const string RL_COL_STATE = "RemitToState";
        internal const string RL_COL_STATENAME = "RemitToStateName";
        internal const string RL_COL_ZIP = "RemitToZip";
        internal const string RL_COL_COUNTRY = "RemitToCountry";
        // Deliver to location 
        internal const string COL_DELIVERTOLOC_ID = "DelivertoLocationID";
        internal const string COL_DELIVERTOLOC_NAME = "DelivertoLocationName";
        internal const string COL_DELIVERTOLOC_NUMBER = "DelivertoLocationNumber";
        internal const string DEL_COL_ADDRESS1 = "DeliverToAddressLine1";
        internal const string DEL_COL_ADDRESS2 = "DeliverToAddressLine2";
        internal const string DEL_COL_ADDRESS3 = "DeliverToAddressLine3";
        internal const string DEL_COL_CITY = "DeliverToCity";
        internal const string DEL_COL_STATE = "DeliverToState";
        internal const string DEL_COL_ZIP = "DeliverToZip";
        internal const string DEL_COL_COUNTRYCODE = "DeliverToCountryCode";
        internal const string DEL_COL_COUNTRY = "DeliverToCountry";
        internal const string DEL_COL_STATECODE = "DeliverToStateCode";
        // Remit to Location
        internal const string COL_REMITTOLOCATIONNAME = "LocationName";
        internal const string COL_COUNTRYID = "CountryId";
        internal const string COL_COUNTRYNAME = "CountryName";
        internal const string COL_STATECOUNTRYID = "StatecountryId";
        internal const string COL_STATEABBREVATIONCODE = "StateAbbrevationCode";
        internal const string COL_CLIENTLOCATIONCODE = "ClientLocationCode";
        internal const string COL_REMITTOLOCATIONCODE = "RemittoLocationCode";
        internal const string COL_REMITTOLOCATION = "RemittoLocation";
        //Landing Page Columns
        internal const string COL_DOCUMENT_CODE = "DocumentCode";
        internal const string COL_DOCUMENT_NAME = "DocumentName";
        internal const string COL_DOCUMENT_NUMBER = "DocumentNumber";
        internal const string COL_DOCUMENT_STATUS = "DocumentStatus";
        internal const string COL_TOTAL_ITEM_RECORD = "TotalItemRecord";

        //TermsAndConditions
        internal const string COL_TERMS_AND_CONDITIONS = "TermsAndConditionId";
        //P2P Item Columns
        internal const string COL_DOC_AUTHORNAME = "AuthorName";
        internal const string COL_DOC_DOCUMENTAMOUNT = "DocumentAmount";
        internal const string COL_DOC_TYPEOFDOCUMENT = "TypeOfDocument";
        internal const string COL_DOC_DATEOFAPPROVAL = "DateofApproval";
        internal const string COL_DOC_APPROVEDDATE = "ApprovedDate";
        internal const string COL_DOCUMENT_ITEM_ID = "DocumentItemId";
        internal const string COL_ISCHECKED = "IsChecked";
        internal const string COL_DOCUMENT_SPLIT_ITEM_ID = "DocumentSplitItemId";
        internal const string COL_ITEMCURRENCY = "ItemCurrencyCode";
        // Delegation
        internal const string COL_ENABLEORIGINALAPPROVERNOTIFICATIONS = "EnableOriginalApproverNotifications";
        internal const string COL_ADDTASKFORORIGINALAPPROVER = "AddTaskForOriginalApprover";
        internal const string COL_DATEFROM = "DateFrom";
        internal const string COL_DATETO = "DateTo";
        internal const string COL_DOCTYPEID = "DocTypeId";
        internal const string COL_WFDOCTYPEID = "WFDocTypeId";

        //P2P Document Columns
        internal const string COL_INTERFACESTATUS = "InterfaceStatus";
        internal const string COL_INTERFACECOMMENT = "InterfaceComment";
        internal const string COL_INTERFACEDATE = "InterfaceDate";
        internal const string COL_ERPRequisitionNumber = "ERPRequisitionNumber";

        //Users column
        internal const string COL_CONTACTCODE = "ContactCode";
        internal const string COL_FIRSTNAME = "FirstName";
        internal const string COL_LASTNAME = "LastName";
        internal const string COL_EMAILADDRESS = "EmailAddress";
        internal const string COL_LOBEntityDetailCode = "LOBEntityDetailCode";
        internal const string COL_PreferenceLOBType = "PreferenceLOBType";
        internal const string COL_BUENTITYDETAILCODE = "BUEntityDetailCode";
        internal const string COL_BUISDEFAULT = "BUIsdefault";
        internal const string COL_BUENTITYDESCRIPTION = "BUEntityDescription";
        internal const string COL_BUENTITYCODE = "BUEntityCode";
        internal const string COL_BU_ENTITY_DISPLAY_NAME = "BUEntityDisplayName";

        internal const string COL_PENDING_VALUE = "PendingValue";
        internal const string COL_UTILIZED_VALUE = "UtilizedValue";
        internal const string COL_UTILIZED_LINE_VALUE = "UtilizedLineValue";
        internal const string COL_UTILIZED_QUANTITY_VALUE = "UtilizedQuantityValue";
        internal const string COL_CONTRACT_LINE_VALUE = "Total";
        internal const string COL_CONTRACT_LINE_UNIT_PRICE = "UnitPrice";
        internal const string COL_CONTRACT_LINE_QUANTITY = "Quantity";
        internal const string COL_CONTRACT_LINE_ITEM_NO = "ContractLineItemNo";
        internal const string COL_COMPLIANCE_TYPE = "ComplianceType";
        internal const string COL_TARGET_CURRENCYCODE = "TargetCurrencyCode";
        internal const string COL_BASE_CURRENCYCODE = "BaseCurrencyCode";

        internal const string COL_PARTNERRECONMATCHTYPEID = "PartnerReconMatchTypeId";
        internal const string COL_CONFIGTYPE = "ConfigType";
        internal const string COL_CONFIGDETAILS = "ConfigDetails";
        internal const string COL_SELECTED = "Selected";
        internal const string COL_INCLUDED = "Included";
        internal const string COL_EXCLUDED = "Excluded";
        internal const string COL_REMITTOLOCATIONID = "RemitToLocationId";
        internal const string COL_ORDERTOLOCATIONID = "OrderToLocationId";
        internal const string COL_PARTNERSOURCESYSTEMVALUE = "PartnerSourceSystemValue";
        internal const string COL_REMITTOLOCATION_CODE = "RemitToLocationAddressCode";
        internal const string COL_ORDERTOLOCATION_CODE = "OrderingLocationAddressCode";
        internal const string COL_SHIPFFROMLOCATION_CODE = "ShipFromLocationAddressCode";

        internal const string COL_SELECTEDITEMS = "selectedItems";
        internal const string COL_PURCHASETYPEDESC = "PurchaseTypeDesc";
        internal const string COL_PROGRAMDESC = "ProgramDesc";

        internal const string OLD_DOCUMENT_STATUS = "OldDocumentStatus";
        internal const string NEW_DOCUMENT_STATUS = "NewDocumentStatus";


        #region Combination Code Column
        internal const string COL_CombinationCode = "Code";
        internal const string COL_DateModified = "DateModified";
        internal const string COL_CCStructureId = "CCStructureId";
        internal const string COL_STRUCTURENAME = "StructureName";
        #endregion

        #endregion


        #region Table Types

        internal const string TVP_P2P_OrgEntity = "TVP_P2P_OrgEntity";
        internal const string TVP_P2P_INTERNALCATALOGITEMS = "tvp_P2P_InternalCatalogItems";
        internal const string TVP_P2P_USERLIST = "tvp_P2P_UserList";
        internal const string TVP_TAXCODES = "tvp_TaxCodes";
        internal const string TVP_ORG_ENTITYCODES = "tvp_ORG_EntityCodes";
        internal const string TVP_ADDRESS = "tvp_Address";
        internal const string TVP_WF_DOCUMENTDELEGATION = "tvp_WF_DocumentDelegation";
        internal const string TVP_P2P_STANDARDUOMLIST = "tvp_P2P_StandardUOMList";
        internal const string TVP_P2P_IDS = "tvp_P2P_Ids";
        internal const string TVP_P2P_FORMCODEOBJECTINSTANCEID = "tvp_P2P_FormCodeObjectInstanceId";
        internal const string TVP_ORG_ENTITYIDS = "tvp_ORGEntityCodesList";
        internal const string TVP_P2P_KEYVALUE = "tvp_P2P_KeyValue";
        internal const string TVP_OrgEntity = "tvp_OrgEntity";
        internal const string TVP_P2P_SourceEntityDetails = "tvp_P2P_SourceEntityDetails";
        internal const string TVP_P2P_TargetEntityDetails = "tvp_P2P_TargetEntityDetails";
        internal const string TVP_P2P_INVOICEDOCUMENTUPDATE = "tvp_P2P_InvoiceDocumentUpdate";
        internal const string TVP_P2P_CUSTOMATTRIBUTEQUESTIONTEXT = "tvp_P2P_CustomAttributeQuestionText";
        internal const string TVP_PRN_PARTNERCODE = "tvp_PRN_PartnerCode";
        internal const string PASCODESTABLE = "PASCodesTable";
        internal const string TVP_P2P_REQ_BLANKETMAPPING = "tvp_P2P_Req_BlanketMapping";
        internal const string TVP_ADDITIONALFIELDPARENTDETAILS = "tvp_AdditionalFieldParentDetails";
    internal const string TVP_ADDITIONALFIELDSOURCEDOCUMENTDETAILS = "tvp_AdditionalFieldSourceDocumentDetails";

    #endregion

    #endregion

    #region Invoice
    #region Invoice Columns
        internal const string COL_INVOICE_ID = "InvoiceId";
        internal const string COL_INVOICE_NAME = "DocumentName";
        internal const string COL_INVOICE_NUMBER = "DocumentNumber";
        internal const string COL_INVOICE_PARTNERINVOICENUMBER = "PartnerInvoiceNumber";
        internal const string COL_INVOICE_PARTNERINVOICEDATE = "PartnerInvoiceDate";
        internal const string COL_BU = "BU";
        internal const string COL_INVOICE_REMITTOLOCATIONID = "RemittoLocationId";
        internal const string COL_INVOICE_SHIPTOLOCATIONID = "ShipToLocationId";
        internal const string COL_MATCHSTATUS = "MatchStatus";
        internal const string COL_MATCHLEVEL = "MatchLevel";
        internal const string COL_EXCEPTIONTYPEID = "ExceptionTypeId";
        internal const string COL_MATCHNAME = "MatchName";
        internal const string COL_MATCHDISPLAYNAME = "DisplayName";
        internal const string COL_INV_NAME = "InvoiceName";
        internal const string COL_INV_NUMBER = "InvoiceNumber";
        internal const string COL_ORDER_DOCUMENTCODE = "OrderDocumentCode";
        internal const string COL_ORDER_CREATED_DATE = "OrderCreatedDate";
        internal const string COL_INVOICESOURCE = "InvoiceSource";
        internal const string COL_INVOICEORIGIN = "InvoiceOrigin";
        internal const string COL_DATERECEIVED = "DateReceived";
        internal const string COL_USETAX = "UseTax";
        internal const string COL_ISINVSPLITDELETED = "IsInvSplitDeleted";
        internal const string COL_ORDERCONTACT = "OrderContact";
        internal const string COL_ORDERP2PLINEITEMID = "OrderP2PLineItemId";
        internal const string COL_MATERIAL_ITEM_COUNT = "MaterialItemCount";
        internal const string COL_SERVICE_ITEM_COUNT = "ServiceItemCount";
        internal const string COL_ADVANCE_ITEM_COUNT = "AdvanceItemCount";
        internal const string COL_CHARGE_ITEM_COUNT = "ChargeItemCount";
        internal const string COL_SUBLINEITEMCOUNT = "SublineItemCount";
        internal const string COL_INVOICEDOCSTATUS = "InvoiceDocStatusInfo";
        internal const string COL_INVOICE_CREATED_FROM = "InvoiceCreatedFrom";
        internal const string COL_ORDER_PARTNER_CODE = "OrderPartnerCode";
        internal const string COL_ORDER_PARTNER_NAME = "OrderPartnerName";
        internal const string COL_BUYERITEMNUMBER = "BuyerItemNumber";
        internal const string COL_INVOICETOTALAMOUNT = "InvoiceTotalAmount";
        internal const string COL_ORDER_SOURCESYSTEMID = "OrderSourceSystemId";
        internal const string COL_INVOICE_ERRORSTATUSID = "ErrorstatusId";
        internal const string COL_INVOICE_ERRORENUM = "EnumName";
        internal const string COL_INVOICE_ERRORDISPLAYNAME = "DisplayName";
        internal const string TVP_P2P_INVOICEERROR = "tvp_P2P_InvoiceError";
        internal const string COL_ERROR_JSON = "ErrorJson";
        internal const string COL_ORDER_PAYMENTTERMID = "OrderPaymentTermId";
        internal const string COL_ORDER_CURRENCY = "OrderCurrencyCode";
        internal const string COL_INVOICE_STATUS = "InvoiceStatus";
        internal const string COL_INVOICE_ORDER_REQUESTER = "OrderRequester";
        internal const string COL_INVOICE_ITEMTOTAL = "InvoiceItemTotal";
        internal const string COL_INVOICE_TAX = "InvoiceTax";
        internal const string COL_INVOICE_SHIPPINGCHARGES = "InvoiceShippingCharges";
        internal const string COL_INVOICE_ADDITIONALCHARGES = "InvoiceAdditionalCharges";
        internal const string COL_INVOICE_TOTALAMOUNT = "InvoiceTotalAmount";
        internal const string COL_INVOICE_USETAX = "InvoiceUseTax";
        internal const string COL_INVOICEORDERCONTACT = "InvoiceOrderContact";
        internal const string COL_INVOICECURRENCY = "InvoiceCurrency";
        internal const string COL_INVOICEORDERCURRENCY = "InvoiceOrderCurrency";
        internal const string COL_CLIENTCONTACTCODE = "ClientContactcode";
        internal const string COL_ERP_PROCESSSTATUS = "ERPProcessStatus";
        internal const string COL_INVOICE_DATECREATED = "DateCreated";
        internal const string COL_INVOICE_SECONDLEVELACKNOWLEDGEMENT = "SecondLevelAcknowledgement";
        internal const string COL_INVOICE_SHOWINTERNALCANCELACTION = "ShowInternalCancelAction";
        internal const string COL_EXCEPTIONDESCRIPTION = "Description";
        internal const string COL_HEADERTAXCODECOUNT = "HeaderTaxCodeCount";
        internal const string COL_TAXCODES = "TaxCodes";
        internal const string COL_INVOICE_FileExtension = "FileExtension";
        internal const string COL_INV_SPLIT_ITEM_ENTITYID = "InvoiceSplitItemEntityId";
        internal const string COL_INV_REJECTIONCOMMENTS = "RejectionComments";
        internal const string COL_INV_REVIEWER_CONTACT_CODE = "ReviewerContactCode";
        internal const string COL_INV_REVIEWER_NAME = "ReviewerName";
        internal const string COL_PARTNER_INVOICE_Date = "PartnerInvoiceDate";
        internal const string COL_INVOICE_SHIPTOLOCATIONNUMBER = "ShiptoLocationNumber";
        internal const string COL_INVOICE_AMOUNT = "InvoiceAmount";
        internal const string COL_INVOICE_INVOICENUMBER = "InvoiceNumber";
        internal const string COL_INVOICE_CREATEDBYNAME = "CreatedByName";
        internal const string COL_INVOICE_PARTNERCONTACTNAME = "PartnerContactName";
        internal const string COL_INVOICE_TotalAmout = "TotalAmout";
        internal const string COL_INVOICE_TotalCharge = "TotalCharge";
        internal const string COL_INVOICE_TotalAllowance = "TotalAllowance";
        internal const string COL_INV_USETAX = "UseTax";
        internal const string COL_INV_SHORTNAME = "ShortName";
        internal const string COL_INV_Quantity = "Quantity";
        internal const string COL_INV_UseTax = "UseTax";
        internal const string COL_INV_SHIPTOZIPCODE = "ZipCode";
        internal const string COL_INV_SHIPTOSTATECODE = "ShipToStateCode";
        internal const string COL_INV_SHIPTOCOUNTRYCODE = "ShipToCountryCode";
        internal const string COL_INV_SHIPTOCOUNTRY = "ShipToCountry";
        internal const string COL_INV_PARTNERPAYMENTTYPE = "PartnerPaymentType";
        internal const string COL_ORDERPURCHASETYPE = "OrderPurchaseType";
        internal const string COL_ORDERLINKEDDOCUMENTTYPECODE = "OrderLinkedDocumentTypeCode";
        internal const string COL_INVOICECREATOR = "InvoiceCreator";
        internal const string COL_ISMULTIDOCINVOICE = "IsMultiDocInvoice";
        #endregion

        #region Invoice Upload info
        internal const string COL_ID = "Id";
        internal const string COL_USERCONTEXT = "UserContext";
        internal const string COL_USEREMAIL = "UserEmail";
        internal const string COL_UPLOADEDFILEID = "UploadedFileId";
        internal const string COL_UPLOADEDFILENAME = "UploadedFileName";
        internal const string COL_UPLOADEDFILETYPE = "UploadedFileType";
        internal const string COL_UPLOADEDFILEURI = "UploadedFileUri";
        internal const string COL_PROCESSEDFILEID = "ProcessedFileId";
        internal const string COL_PROCESSEDFILENAME = "ProcessedFileName";
        internal const string COL_PROCESSEDFILETYPE = "ProcessedFileType";
        internal const string COL_PROCESSEDFILEURI = "ProcessedFileUri";
        internal const string COL_PROCESSEDRESULT = "ProcessedResult";
        internal const string COL_PROCESSEDRESULTXML = "ProcessedResultXml";
        internal const string COL_TOTAL = "Total";
        internal const string COL_PROCESSED = "Processed";
        internal const string COL_UNPROCESSED = "UnProcessed";
        internal const string COL_PROCESSEDCOUNT = "ProcessedCount";
        internal const string COL_USERNAME = "Username";
        internal const string COL_REQUESTEDTIME = "RequestedTime";
        internal const string COL_COMPLETIONTIME = "CompletionTime";
        internal const string COL_ERRORNAME = "ErrorName";
        internal const string COL_DISPLAYDOCUMENTSTATUSDESC = "DisplayDocumentStatusDESC";
        internal const string COL_EXCELFILETYPE = "ExcelFileType";
        #endregion

        #region InvoiceRemittanceDetails
        internal const string COL_REMITTANCE_ID = "Id";
        internal const string COL_PAYMENT_DATE = "PaymentDate";
        internal const string COL_PAYMENT_REF_NO = "PaymentReferenceNumber";
        internal const string COL_PAYMENT_REMITTANCE_ID = "PaymentRemittanceId";
        internal const string COL_PAYMENT_METHOD = "PaymentMethod";
        internal const string COL_NET_AMOUNT = "NetAmount";
        internal const string COL_IRTYPEID = "IRTypeId";
        internal const string COL_GROSS_AMOUNT = "GrossAmount";
        internal const string COL_DISCOUNT_AMOUNT = "DiscountAmount";
        internal const string COL_ADJUSTMENT_AMOUNT = "AdjustmentAmount";
        internal const string COL_REMITTANCE_STATUS = "RemittanceStatus";
        internal const string COL_REMITTANCE_ISCREDITMEMO = "IsCreditMemo";
        internal const int COL_HEDDERlEVEL = 0;
        internal const int COL_ITEMLEVEL = 1;


        #endregion

        #region Invoice LineItem Columns
        internal const string COL_INVOICE_ITEM_ID = "InvoiceItemId";
        internal const string COL_INVOICE_POITEMQUANTITY = "POItemQuantity";
        internal const string COL_INVOICE_POITEMUNITPRICE = "POItemUnitPrice";
        internal const string COL_INVOICE_LINEITEMSHIPPINGID = "InvoiceLineItemShippingID";
        internal const string COL_INVOICE_INVOICEAMOUNT = "InvoicingAmount";
        internal const string COL_INVOICE_REMAININGINVOICEAMOUNT = "RemainingInvoiceAmount";
        internal const string COL_LINEAMOUNTDIFF = "LineAmountDiff";
        internal const string COL_INVOICE_MATCHSTATUS = "MatchStatus";
        internal const string COL_PARTNERITEMNUMBER = "PartnerItemNumber";
        internal const string COL_CONTRACTNUMBER = "ContractNumber";
        internal const string COL_INVOICE_ITEMMATCHTYPE = "ItemMatchType";
        internal const string COL_ORDER_START_DATE = "OrderStartDate";
        internal const string COL_POITEMLINENUMBER = "POItemLineNumber";
        internal const string COL_LINETAXCODECOUNT = "LineTaxCodeCount";
        internal const string COL_ITEMCONTROLTYPE = "ItemControlType";
        #endregion

        #region Dynamic Discount Columns
        internal const string COL_DISCOUNT = "Discount($)";
        internal const string COL_DISCOUNTPERCENTAGE = "Discount(%)";
        internal const string COL_INVOICE_PAYDATE = "InvoicePayDate";
        internal const string COL_INVOICE_PAY_AMOUNT = "InvoicePayAmount";
        internal const string COL_INVITATION_ID = "InvitationId";
        internal const string COL_DYNAMICDISCOUNT_STATUS = "Status";
        internal const string COL_DYNAMICDISCOUNT_STATUS_DISPLAY_NAME = "DynamicDiscountStatusDisplayName";
        internal const string COL_DYNAMICDISCOUNT_PAYDATE = "DynamicDiscountPayDate";
        internal const string COL_DYNAMICDISCOUNT_PAY_AMOUNT = "DynamicDiscountPayAmount";
        internal const string COL_DYNAMICDISCOUNT_STARTDATE = "StartDate";
        internal const string COL_DYNAMICDISCOUNT_ENDDATE = "EndDate";
        internal const string COL_DYNAMICDISCOUNT_DISCOUNTDETAILSKEY = "DiscountDetailsKey";
        internal const string COL_DYNAMICDISCOUNT_DISCOUNTDETAILSVALUE = "DiscountDetailsValue";
        internal const string COL_CREATEDBY = "CreatedBy";
        #endregion

        #region Program Columns
        internal const string COL_PROGRAM_ID = "ProgramId";
        internal const string COL_PROGRAM_NAME = "ProgramName";
        internal const string COL_ENFORCEMENTID = "EnforcementId";
        internal const string COL_DISPLAYNAME = "DisplayName";
        internal const string COL_PARENTID = "ParentId";
        internal const string COL_OWNER = "Owner";
        internal const string COL_AUTHOR = "Author";
        internal const string COL_SPONSOR = "Sponsor";
        internal const string COL_PROGRAM_DESCRIPTION = "Description";
        internal const string COL_BUDGET = "Budget";
        internal const string COL_COMMITTEDFUNDS = "CommittedFunds";
        internal const string COL_OBLIGATEDFUNDS = "ObligatedFunds";
        internal const string COL_EXPENSEDFUNDS = "ExpensedFunds";
        internal const string COL_AVAILABLEFUNDS = "AvailableFunds";
        internal const string COL_DATEACTIVATED = "DateActivated";
        internal const string COL_DATEINACTIVATED = "DateInActivated";
        internal const string COL_DATEDELETED = "DateDeleted";
        internal const string COL_DATECLOSED = "DateClosed";
        internal const string COL_DOCUMENT_COUNT = "DocumentCount";
        internal const string COL_ADJUSTMENT_NUMBER = "AdjustmentNumber";
        internal const string COL_FORMID = "FormId";
        internal const string COL_ITEMFORMID = "ItemFormId";
        internal const string COL_SPLITFORMID = "SplitFormId";
        internal const string COL_LEVEL = "Level";
        internal const string COL_ISSUBMIT = "IsSubmit";
        #endregion

        #region Invoice Tax Master Info Columns
        internal const string COL_JURISDICTION_CODE = "JurisdictionCode";
        internal const string COL_JURISDICTION_NAME = "JurisdictionName";
        #endregion

        #region Invoice StoredProcedures
        internal const string USP_P2P_INV_GETALLDISPLAYDERAILS = "usp_P2P_INV_GetAllDisplayDetails";
        internal const string USP_P2P_INV_GETINVOICEBASICDETAILSBYID = "usp_P2P_INV_GetInvoiceBasicDetailsById";
        internal const string USP_P2P_INV_GETBASICDETAILSBYIDFORINTERFACE = "usp_P2P_INV_GetBasicDetailsByIdForInterface";
        internal const string USP_P2P_INV_GETINVOICEBUYERANDPARTNERDETAILS = "usp_P2P_INV_GetInvoiceBuyerAndPartnerDetails";
        internal const string USP_P2P_INV_GETALLLINEITEMSBYID = "usp_P2P_INV_GetAllLineItemsById";
        internal const string USP_P2P_INV_GETALLLINEITEMSBYIDFORINTERFACE = "usp_P2P_INV_GetAllLineItemsByIdForInterface";
        internal const string USP_P2P_INV_GETPARTNERDETAILSBYLIID = "usp_P2P_INV_GetPartnerDetailsByLiId";
        internal const string USP_P2P_INV_GETSHIPPINGSPLITDETAILSBYLIID = "usp_P2P_INV_GetShippingSplitDetailsByLiId";
        internal const string USP_P2P_INV_GETSHIPPINGSPLITDETAILSBYINVID = "usp_P2P_INV_GetShippingSplitDetailsByInvId";
        internal const string USP_P2P_INV_GETOTHERITEMDETAILSBYLIID = "usp_P2P_INV_GetOtherItemDetailsByLiId";
        internal const string USP_P2P_INV_SAVEINVOICEITEM = "usp_P2P_INV_SaveInvoiceItem";
        internal const string USP_P2P_INV_GETINVOICEINFOFORNOTIFICATIONS = "usp_P2P_INV_GetInvoiceInfoForNotifications";
        internal const string USP_P2P_INV_GETINVOICEENTITYDETAILSBYID = "usp_P2P_INV_GetInvoiceEntityDetailsById";
        internal const string USP_P2P_INV_GETINVOICEACENTITYDETAILSBYID = "usp_P2P_INV_GetInvoiceACEntityDetailsById";
        internal const string USP_P2P_INV_GETDOCUMENTDETAILSBYNUMBER = "usp_P2P_INV_GetDocumentDetailsByNumber";
        internal const string USP_P2P_INV_GETALLDOCUMENTITEMSCHECKEDSTATUS = "usp_P2P_INV_GetAllDocumentItemsCheckedStatus";
        internal const string USP_P2P_INV_GETINVOICERELATEDLOCATIONDETAILS = "usp_P2P_INV_GetInvoiceRelatedLocationDetails";
        internal const string USP_P2P_INV_GETALLMATCHSTATUSBYID = "usp_P2P_INV_GetAllMatchStatusById";

        internal const string USP_P2P_INV_SAVEINVOICEITEMPARTNERS = "usp_P2P_INV_SaveInvoiceItemPartners";
        internal const string USP_P2P_INV_SAVEINVOICEITEMSHIPPING = "usp_P2P_INV_SaveInvoiceItemShippingDetails";
        internal const string USP_P2P_INV_GENERATEDEFAULTINVOICENAME = "usp_P2P_INV_GenerateDefaultInvoiceName";
        internal const string USP_P2P_INV_GETALLLINEITEMSBYINVOICEIDFORLEFTPANEL = "usp_P2P_INV_GetAllLineItemsByInvoiceIdForLeftPanel";
        internal const string USP_P2P_INV_DELETELINEITEMBYID = "usp_P2P_INV_DeleteLineItembyId";
        internal const string USP_P2P_INV_SAVEINVOICE = "usp_P2P_INV_SaveInvoice";
        internal const string USP_P2P_INV_SAVEINVOICEADDITIONALENTITYDETAILS = "usp_P2P_INV_SaveInvoiceAdditionalEntityDetails";
        internal const string USP_P2P_INV_UPDATERECEIVEDATE = "usp_P2P_INV_UpdateReceiveDate";
        internal const string USP_P2P_INV_CREATEINVOICEORDER = "usp_P2P_INV_CreateInvoiceOrder";
        internal const string USP_P2P_INV_CREATEINVOICEFROMPAYMENTREQUEST = "usp_P2P_INV_CreateInvoiceFromPaymentRequest";
        internal const string USP_P2P_INV_CREATEINVOICEFROMSERVICECONFIRMATION = "usp_P2P_INV_CreateInvoiceFromServiceConfirmation";
        internal const string USP_P2P_INV_CREATEINVOICESCANIMAGE = "usp_P2P_INV_CreateInvoiceScanImage";
        internal const string USP_P2P_INV_VALIDATEINTERFACEINVOICESTATUS = "usp_P2P_INV_ValidateInterfaceInvoiceStatus";
        internal const string USP_P2P_INV_VALIDATEINTERFACEINVOICESTATUSBATCH = "usp_P2P_INV_ValidateInterfaceInvoiceStatusBatch";
        internal const string USP_P2P_INV_UPDATEINVOICEMATCHINGSTATUSFORINTERFACE = "usp_P2P_INV_UpdateInvoiceMatchingStatusForInterface";
        internal const string USP_P2P_INV_GETINVOICEDETAILSFORINTERFACE = "usp_P2P_INV_GetInvoiceDetailsForInterface";
        internal const string USP_P2P_INV_DELETESPLITITEMSBYITEMID = "usp_P2P_Inv_DeleteSplitItemsByItemId";
        internal const string USP_P2P_INV_GETALLORDERITEMSBYORDERID = "usp_P2P_INV_GetAllOrderItemsByOrderId";
        internal const string USP_P2P_REQ_DELETESPLITITEMSBYITEMID = "usp_P2P_REQ_DeleteSplitItemsByItemId";

        internal const string USP_P2P_INV_GETINVOICEORDERID = "usp_P2P_INV_GetInvoiceOrderId";
        internal const string USP_P2P_INV_GETIRBYINVOICEID = "usp_P2P_INV_GetIRByInvoiceId";
        internal const string USP_P2P_INV_COPYCANCELEDINVOICE = "usp_P2P_INV_CopyCanceledInvoice";
        internal const string USP_P2P_INV_VALIDATEPARTNERINVOICENUMBER = "usp_P2P_INV_ValidatePartnerInvoiceNumber";
        internal const string USP_P2P_INV_GETBASICDETAILBYSUPPLIERINVOICENUMBER = "usp_P2P_INV_GetBasicDetailBySupplierInvoiceNumber";


        internal const string USP_P2P_INV_GETINVOICEDETAILSBYINVOICENUMBERANDSTATUS = "usp_P2P_INV_GetInvoiceDetailsByInvoiceNumberAndStatus";
        internal const string USP_P2P_INV_UPDATEINTERFACESTATUS = "usp_P2P_INV_UpdateInterfaceStatus";
        internal const string USP_P2P_INV_UPDATEINVOICEPAYMENTDETAILS = "usp_P2P_INV_UpdateInvoicePaymentDetails";
        internal const string USP_P2P_INV_UPDATEEXTENDEDSTATUS = "usp_P2P_INV_UpdateExtendedStatus";
        internal const string USP_P2P_INV_GETREMITTANCEDETAILSBYINVOICEID = "usp_P2P_INV_GetRemittanceDetailsByInvoiceID";
        internal const string USP_P2P_INV_GETREMITTANCEDETAILSHISTORYBYINVOICEID = "usp_P2P_INV_GetRemittanceDetailsHistoryByInvoiceID";
        internal const string USP_P2P_INV_GETREMITTANCEDETAILSBYPAYMENTREFERENCENUMBER = "usp_P2P_INV_GetRemittanceDetailsByPaymentReferenceNumber";

        internal const string USP_P2P_INV_VALIDATEDOCUMENT = "usp_P2P_INV_ValidateDocument";
        internal const string USP_P2P_INV_VALIDATECANCELINVOICE = "usp_P2P_INV_ValidateCancelInvoice";
        internal const string USP_P2P_INV_GETINVOICEIDBYPARTNERINVOICENUMBER = "usp_P2P_INV_GetInvoiceIdByPartnerInvoiceNumber";
        internal const string USP_P2P_INV_COPYORDERITEMSTOINVOICE = "usp_P2P_INV_CopyOrderItemsToInvoice";
        internal const string USP_P2P_INV_GETSCANNEDIMAGEIDSBYINVOICEID = "usp_P2P_INV_GetScannedImageIdsByInvoiceId";
        internal const string USP_P2P_INV_GETORDERDETAILSFORINVOICE = "usp_P2P_INV_GetOrderDetailsForInvoice";
        internal const string USP_P2P_INV_GETALLORDERFORINVOICES = "usp_P2P_INV_GetAllOrderForInvoices";
        internal const string USP_P2P_INV_GETINVOICELINEITEMTAXES = "usp_P2P_INV_GetInvoiceLineItemTaxes";
        internal const string USP_P2P_INV_UPDATEINVOICELINEITEMTAXES = "usp_P2P_INV_UpdateInvoiceLineItemTaxes";
        internal const string USP_P2P_INV_INSERTUPDATELINEITEMTAXES = "usp_P2P_INV_InsertUpdateLineItemTaxes";
        internal const string USP_P2P_INV_GETINVOICEHEADERANDITEMTAXBYITEMID = "usp_P2P_INV_GetInvoiceHeaderAndItemTaxByItemId";
        internal const string USP_P2P_INV_DELETELINEITEMTAXBYID = "usp_P2P_INV_DeleteLineItemTaxById";
        internal const string USP_P2P_INV_CALCULATE_AND_UPDATELINEITEMTAX = "usp_P2P_INV_CalculateAndUpdateLineItemTax";
        internal const string USP_P2P_INV_CALCULATELINEITEMTAX = "usp_P2P_INV_CalculateLineItemTax";
        internal const string USP_P2P_INV_GETALLITEMSFORUSETAX = "usp_P2P_INV_GetAllItemsForUseTax";
        internal const string USP_P2P_INV_GETLINEITEMTAXESFORUSETAX = "usp_P2P_INV_GetLineItemTaxesForUseTax";
        internal const string USP_P2P_INV_GETINVSPLITSQTYFORUSETAX = "usp_P2P_INV_GetInvSplitsQtyForUseTax";
        internal const string USP_P2P_INV_UPDATEUSETAX = "usp_P2P_INV_UpdateUseTax";
        internal const string USP_P2P_INV_FLIP_ACCOUNTING_SPLIT_FROM_PO_TO_INVOICE = "usp_P2P_INV_FlipAccountingSplitFromPOToInvoice";
        internal const string USP_P2P_INV_FLIP_ACCOUNTING_SPLIT_FROM_SC_TO_INVOICE = "usp_P2P_INV_FlipAccountingSplitFromSCToInvoice";
        internal const string USP_P2P_INV_UPDATESPLITACCOUNTINGDETAILS = "usp_P2P_INV_UpdateSplitAccountingDetails";
        internal const string USP_P2P_INV_UPDATEORDERINVOICEMAPPING = "usp_P2P_INV_UpdateOrderInvoiceMapping";
        internal const string USP_P2P_INV_SAVEINVOICEITEMOTHER = "usp_P2P_INV_SaveInvoiceItemOther";
        internal const string USP_P2P_INV_PRORATESHIPPINGANDFREIGHT = "usp_P2P_INV_ProrateShippingAndFreight";
        internal const string USP_P2P_INV_SAVEACCRUEDTAXDETAILS = "usp_P2P_INV_SaveAccruedTaxDetails";
        internal const string USP_P2P_INV_GETINVOICESFORINTERFACE = "usp_P2P_INV_GetInvoicesForInterface";
        internal const string USP_P2P_INV_GETINVOICEFORNOTIFICATION = "usp_P2P_INV_GetInvoiceForNotification";
        internal const string USP_P2P_INV_SAVEINVOICEDEFAULTACCOUNTING = "usp_P2P_INV_SaveInvoiceDefaultAccounting";
        internal const string USP_P2P_INV_GETVALIDATIONERRORCODEBYID = "usp_P2P_INV_GetValidationErrorCodeById";
        internal const string USP_P2P_INV_PRORATEHEADERTAXANDSHIPPING = "usp_P2P_INV_ProrateHeaderTaxAndShipping";
        internal const string USP_P2P_INV_VALIDATEINTERFACEREMITTANCEDTLS = "usp_P2P_INV_ValidateInterfaceRemittanceDtls";
        internal const string USP_P2P_INV_SAVEINVOICEACCOUNTINGAPPLYTOALL = "usp_P2P_INV_SaveInvoiceAccountingApplyToAll";
        internal const string USP_P2P_INV_GETALLITEMIDSBYINVOICEID = "usp_P2P_INV_GetAllItemIdsByInvoiceId";
        internal const string USP_P2P_INV_GETINVOICEDETAILSFOREXPORTPDFBYID = "usp_P2P_Invoice_GetInvoiceDetailsForExportPDFById";
        internal const string USP_P2P_INV_DELETELINEITEMSBYDOCUMENTCODE = "usp_P2P_INV_DeleteLineItemsByDocumentCode";
        internal const string USP_P2P_INV_UPDATEBILLTOLOCATION = "usp_P2P_INV_UpdateBillToLocation";
        internal const string USP_P2P_INV_PRORATETAX = "usp_P2P_INV_ProrateTax";
        internal const string USP_P2P_INV_GETALLINVOICEDETAILSBYINVOICEID = "usp_P2P_INV_GetAllInvoiceDetailsByInvoiceId";
        internal const string USP_P2P_IR_GETALLIRDETAILSBYIRID = "usp_P2P_IR_GetAllIRDetailsByIRId";
        internal const string USP_P2P_INV_DELETELINEITEMSBYORGENTITYCODE = "usp_P2P_INV_DeleteLineItemsByOrgEntityCode";
        internal const string USP_P2P_INV_SAVEACCOUNTINGFROMPO = "usp_P2P_INV_SaveAccountingFromPO";
        internal const string USP_P2P_INV_DELETELINEITEMSONBUCHANGE = "usp_P2P_INV_DeleteLineItemsOnBUChange";
        internal const string USP_P2P_INV_UPDATEINVOICEITEMCHECKEDSTATUS = "usp_P2P_INV_UpdateInvoiceItemCheckedStatus";
        internal const string USP_P2P_INV_DELETEUNCHECKEDINVOICEITEMS = "usp_P2P_INV_DeleteUnCheckedInvoiceItems";
        internal const string USP_P2P_INV_VALIDATEINVOICEPAYMENTUPDATE = "usp_P2P_INV_ValidateInvoicePaymentUpdate";
        internal const string USP_P2P_INV_VALIDATEINTERFACEDOCUMENT = "usp_P2P_INV_ValidateInterfaceDocument";
        internal const string USP_P2P_INV_VALIDATESUPPLIERINTERFACEDOCUMENT = "usp_P2P_INV_ValidateSupplierInterfaceDocument";
        internal const string USP_P2P_INV_VALIDATEBUYERINTERFACEDOCUMENT = "usp_P2P_INV_ValidateBuyerInterfaceDocument";
        internal const string USP_P2P_INV_UPDATEORGENTITIESFROMCATALOGBYITEMID = "usp_P2P_INV_UpdateOrgEntitiesFromCatalogByItemId";
        internal const string USP_P2P_INV_GETINVOICECAPITALCODECOUNTBYID = "usp_P2P_INV_GetInvoiceCapitalCodeCountById";
        internal const string USP_P2P_INV_PRORATEHEADERUSETAX = "usp_P2P_INV_ProrateHeaderUseTax";
        internal const string USP_P2P_INV_GETINVDETAILSBYPARTNERINVNOANDLOCCODE = "usp_P2P_INV_GetInvDetailsByPartnerInvNoAndLocCode";
        internal const string USP_P2P_INV_UPDATEIRDETAILSBYINVOICEID = "UpdateIRDetailsByInvoiceId";
        internal const string USP_P2P_INV_GETCONTRACTUTILIZATIONBYID = "usp_P2P_INV_GetContractUtilizationbyId";
        internal const string USP_P2P_INV_SAVEEXCEPTIONFOROVERRIDE = "usp_P2P_INV_SaveExceptionForOverride";
        internal const string USP_P2P_INV_GETOVERRIDEEXCEPTION = "USP_P2P_INV_GetOverrideException";
        internal const string USP_P2P_INV_GETALLOVERRIDEEXCEPTION = "USP_P2P_INV_GetALLOverrideException";
        internal const string USP_P2P_INV_GETALLMATCHINGSTATUS = "usp_P2P_INV_GetAllMatchingStatus";
        internal const string USP_P2P_INV_GETALLERRORSTATUS = "usp_P2P_INV_GetAllErrorStatus";
        internal const string USP_P2P_INV_SAVEINVOICEERRORDETAILS = "usp_P2P_INV_SaveInvoiceErrorDetails";
        internal const string USP_P2P_INV_UPDATEPROCEEDFORPAYMENT = "usp_P2P_INV_UpdateProceedForPayment";
        internal const string USP_P2P_INV_VALIDATEERRORCODEBUYERINTERFACEDOCUMENT = "usp_P2P_INV_ValidateErrorCodeBuyerInterfaceDocument";
        internal const string UPS_P2P_INV_UPDATEPROCEEDFORPAYMENTINBULK = "usp_P2P_INV_UpdateProceedForPaymentInBulk";
        internal const string USP_P2P_INV_VALIDATEPARTNERINVOICENUMBERBYORDER = "usp_P2P_INV_ValidatePartnerInvoiceNumberByOrder";
        internal const string USP_P2P_INV_GETINVOICERESPONSEDETAILS = "usp_P2P_INV_GetInvoiceResponseDetails";
        internal const string USP_P2P_INV_GETALLERRORSBYINVOICEID = "usp_P2P_INV_GetAllErrorsByInvoiceId";
        internal const string USP_P2P_INV_SAVEITEMEXCEPTIONFOROVERRIDE = "usp_P2P_INV_SaveItemExceptionForOverride";
        internal const string USP_P2P_INV_SAVEITEMEXCEPTIONSFOROVERRIDE = "usp_P2P_INV_SaveItemExceptionsForOverride";
        internal const string USP_P2P_INV_SAVEEXCEPTIONSFOROVERRIDE = "usp_P2P_INV_SaveExceptionsForOverride";
        internal const string USP_P2P_INV_GETPAYMENTREQUESTDETAILSFORNOTIFICATION = "usp_P2P_PR_GetPaymentRequestDetailsForNotification";
        internal const string USP_P2P_INV_FLIP_ACCOUNTING_SPLIT_FROM_PR_TO_INVOICE = "usp_P2P_INV_FlipAccountingSplitFromPRToInvoice";
        internal const string USP_P2P_INV_SAVEINVOICEUPLOADINFO = "usp_P2P_INV_SaveInvoiceUploadInfo";
        internal const string USP_P2P_INV_GETINVOICEUPLOADINFO = "usp_P2P_INV_GetInvoiceUploadInfo";
        internal const string USP_P2P_INV_UPDATETAXONHEADERSHIPTO = "usp_P2P_INV_UpdateTaxOnHeaderShipTo";
        internal const string USP_P2P_INV_PRORATELINEITEMTAX = "usp_P2P_INV_ProrateLineItemTax";
        internal const string USP_P2P_INV_UPDATELINEITEMTAX = "usp_P2P_INV_UpdateLineItemTax";
        internal const string USP_P2P_INV_GETLINEITEMTAXDETAILS_FORLISTOFDOCUMENTITEMIDS = "usp_P2P_INV_GetLineItemTaxDetails_ForListOfDocumentItemIds";
        internal const string USP_P2P_INV_GETTAXDETAILFORLINEITEM = "usp_P2P_INV_GetTaxDetailForLineItem";

        internal const string USP_P2P_INV_EXPORTINVOICETOEXCEL = "usp_P2P_INV_ExportInvoiceToExcel";
        internal const string USP_P2P_INV_EXPORTINVOICEITEMSTOEXCEL = "usp_P2P_INV_ExportInvoiceItemsToExcel";
        internal const string USP_P2P_INV_GETINVOICEUPLOADRESULTINFO = "usp_P2P_INV_GetInvoiceUploadResultInfo";
        internal const string USP_P2P_INV_GETINVOICEMASTERTEMPLATEINFO = "usp_P2P_INV_GetInvoiceMasterTemplateInfo";
        internal const string USP_P2P_GETPROCESSEDINVOICEDETAILSFORINTERFACE = "usp_P2P_GetProcessedInvoiceDetailsForInterface";
        internal const string USP_P2P_INV_GETMATCHSTATUSBYINVOICEID = "usp_P2P_INV_GetMatchStatusByInvoiceId";
        internal const string USP_P2P_INV_GETORDERITEMSFORMAPPING = "usp_P2P_INV_GetOrderItemsForMapping";
        internal const string USP_P2P_INV_UPDATECANCELINVOICEDETAILS = "usp_P2P_INV_UpdateCancelInvoiceDetails";
        internal const string USP_P2P_INV_UPDATELINETYPEBYPURCHASETYPE = "usp_P2P_INV_UpdateLineTypeByPurchaseType";
        internal const string USP_P2P_INV_SAVEINVOICEITEMFROMSUPPLIER = "usp_P2P_INV_SaveInvoiceItemfromsupplier";
        internal const string USP_P2P_INV_VALIDATEORDEREXISTSSTANDARDINVOICE = "usp_P2P_INV_ValidateOrderExistsStandardInvoice";
        internal const string USP_P2P_INV_SAVEAINVOICEDVANCEITEM = "usp_P2P_INV_SaveInvoiceAdvanceItem";
        internal const string USP_P2P_INV_GETINVOICEHEADERTAXES = "usp_P2P_INV_GetInvoiceHeaderTaxes";
        internal const string USP_P2P_INV_UPDATEINVOICEHEADERTAXES = "USP_P2P_INV_UPDATEINVOICEHEADERTAXES";
        internal const string USP_P2P_INV_GETINVOICETAXMASTERINFO = "usp_P2P_INV_GetInvoiceTaxMasterInfo";
        internal const string USP_P2P_INV_SAVEINVOICEACCOUNTINGDETAILSV2 = "usp_P2P_INV_SaveInvoiceAccountingDetailsV2";
        internal const string USP_P2P_INV_SAVEREVIEWERASSTAKEHOLDER = "usp_P2P_INV_SaveReviewerAsStakeholder";
        internal const string USP_P2P_INVV2_SAVEINVOICEITEM = "usp_P2P_INVV2_SaveInvoiceItem";
        internal const string USP_P2P_INV_GetInvoiceV2EntityDetailsById = "usp_P2P_INV_GetInvoiceV2EntityDetailsById";
        internal const string USP_P2P_IR_SAVEIRITEMOTHER_V2 = "usp_P2P_IR_SaveIRItemOtherV2";
        internal const string USP_P2P_INV_SAVEINVOICEACCOUNTING = "usp_P2P_INV_SaveInvoiceAccounting";

        internal const string USP_P2P_INVOICE_GETALLCHANGEINVOICEBYINVOICE = "usp_P2P_invoice_GetAllChangeInvoiceByInvoice";
        internal const string USP_P2P_INVOICE_GETINVOICEHEADERDETAILBYID = "usp_P2P_Invoice_GetInvoiceHeaderDetailById";
        internal const string USP_P2P_INV_GETALLINVOICEDETAILSBYINVOICEIDFORVIEWCHANGES = "usp_P2P_INV_GetAllInvoiceDetailsByInvoiceIdForViewChanges";
        internal const string USP_P2P_INV_GETINVOICEITEMSCHANGED = "usp_P2P_INV_GetInvoiceItemsChanged";
        internal const string USP_P2P_INV_VALIDATEINTERFACEUPDATEDOCUMENT = "usp_P2P_INV_ValidateInterfaceUpdateDocument";

        internal const string USP_PRN_GETPARTNERIDENTIFICATIONDETAILS = "usp_PRN_GetPartnerIdentificationDetails";

        #endregion

        #region Invoice Types
        internal const string TVP_P2P_INVOICEITEMS = "tvp_P2P_InvoiceItems";
        internal const string TVP_P2P_INVOICEITEMTAXES = "tvp_P2P_InvoiceItemTaxes";
        internal const string TVP_P2P_INV_ERRORDETAILS = "tvp_P2P_INV_ErrorDetails";
        internal const string TVP_P2P_INV_MATCHLIST = "tvp_P2P_INV_MatchList";
        internal const string TVP_P2P_INV_DETAILS = "tvp_P2P_Inv_Details";
        internal const string TVP_P2P_INV_INVOICEITEMS = "tvp_P2P_INV_InvoiceItems";
        internal const string tvp_P2P_INV_ORDERIDLIST = "tvp_P2P_INV_OrderIdList";
        internal const string TVP_P2P_INVOICEITEMSHIPPINGDETAILS = "tvp_P2P_INV_ItemShippingDetails";
        internal const string TVP_P2P_IR_ITEMOTHERDETAILS = "tvp_P2P_IR_ItemOtherDetails";
        internal const string TVP_P2P_ExceptionOverrides = "tvp_P2P_ExceptionOverrides";
        #endregion
        #endregion

        #region Invoice Reconciliation
        internal const string USP_P2P_IR_CREATEIRFROMINVOICE = "usp_P2P_IR_CreateIRfromInvoice";
        internal const string USP_P2P_IR_GENERATEDEFAULTIRNAME = "usp_P2P_IR_GenerateDefaultIRName";
        internal const string USP_P2P_IR_GETIRBASICDETAILSBYID = "usp_P2P_IR_GetIRBasicDetailsById";
        internal const string USP_P2P_IR_GETALLLINEITEMSBYID = "usp_P2P_IR_GetAllLineItemsById";
        internal const string USP_P2P_IR_GETALLLINEITEMSFORLEFTPANEL = "usp_P2P_IR_GetAllLineItemsForLeftPanel";
        internal const string USP_P2P_IR_SAVEITEMSTATUS = "usp_P2P_IR_SaveItemStatus";
        internal const string USP_P2P_IR_CALCULATEIRSTATUS = "usp_P2P_IR_CalculateIRStatus";
        internal const string USP_P2P_IR_GETIRINFOFORNOTIFICATION = "usp_P2P_IR_GetIRInfoForNotification";
        internal const string USP_P2P_IR_GETIRDETAILSBYORDERID = "usp_P2P_IR_GetIRDetailsByOrderId";
        internal const string USP_P2P_IR_GETPREVIOUSORDERDETAILSFORIR = "usp_P2P_IR_GetPreviousOrderDetailForIR";
        internal const string USP_P2P_IR_UPDATECHANGEORDERITEM = "usp_P2P_IR_UpdateChangeOrderItem";
        internal const string USP_P2P_IR_GETRECEIVERDETAILSOFITEM = "usp_P2P_IR_GetReceiverDetailsOfItem";
        internal const string USP_P2P_IR_GETNEXTACTIONONIR = "usp_P2P_IR_GetNextActionOnIR";
        internal const string USP_P2P_IR_SAVEIRORDERDETAILS = "usp_P2P_IR_SaveIROrderDetails";
        internal const string USP_P2P_IR_GETIRLINEITEMTAXES = "usp_P2P_IR_GetIRLineItemTaxes";
        internal const string USP_P2P_UPDATEINVOICEORDERMAPPING = "usp_P2P_UpdateInvoiceOrderMapping";
        internal const string USP_P2P_IR_GETORDERITEMSFOREXCEPTIONPOPUP = "usp_P2P_IR_GetOrderItemsforExceptionPopup";
        internal const string USP_P2P_IR_RESETPOINVOICEITEMMAPPING = "usp_P2P_IR_ResetPOInvoiceItemMapping";
        internal const string USP_P2P_IR_APPROVALSUBTYPE = "usp_P2P_IR_ApprovalSubType";
        internal const string USP_P2P_IR_GETREQUESTERFORIRAPPROVAL = "usp_P2P_IR_GetRequesterForIRApproval";
        internal const string USP_P2P_IR_GETNEXTACCEPTORFORIRACCEPTANCE = "usp_P2P_IR_GetNextAcceptorForIRAcceptance";
        internal const string USP_P2P_IR_GETCOUNTACCEPTANCECYCLEFORIR = "usp_P2P_IR_GetCountAcceptanceCycleForIR";
        internal const string USP_P2P_IR_GETIRDETAILSFORIRAPPROVAL = "usp_P2P_IR_GetIRDetailsForApproval";
        internal const string USP_P2P_IR_GETALLCATEGORIESBYIRID = "usp_P2P_IR_GetAllCategoriesByIRId";
        internal const string USP_P2P_IR_GETIRENTITIES = "usp_P2P_IR_GetIREntities";
        internal const string USP_P2P_IR_UPDATE_IR_ACCOUNTING_WITH_PO = "usp_P2P_IR_UpdateIRAccountingWithPO";
        internal const string USP_P2P_IR_GETIRENTITYDETAILSBYID = "usp_P2P_IR_GetIREntityDetailsById";
        internal const string USP_P2P_IR_GETIRDETAILSFOREXPORTPDFBYID = "usp_P2P_IR_GetIRDetailsForExportPDFById";
        internal const string USP_P2P_IR_GETIRDETAILSFORNOTIFICATION = "usp_P2P_IR_GetIRDetailsForNotification";
        internal const string USP_P2P_IR_GETVALIDATIONERRORCODEBYID = "usp_P2P_IR_GetValidationErrorCodeById";
        internal const string USP_P2P_IR_SAVEIRBASICDETAILS = "usp_P2P_IR_SaveIRBasicDetails";
        internal const string USP_P2P_IR_GETIRTYPES = "usp_P2P_IR_GetIRTypes";
        internal const string USP_P2P_IR_SAVEIRACCOUNTINGAPPLYTOALL = "usp_P2P_IR_SaveIRAccountingApplyToAll";
        internal const string USP_P2P_IR_GETALLITEMIDSBYIRID = "usp_P2P_IR_GetAllItemIdsByIRId";
        internal const string USP_P2P_IR_DELETEIRBYDOCUMENTID = "usp_P2P_IR_DeleteIRbyDocumentId";
        internal const string USP_DM_REMOVEDOCUMENTBUBYCONTACTCODESDOCUMENTCODE = "usp_DM_RemoveDocumentBUByContactCodesDocumentCode";
        internal const string USP_P2P_IR_SAVEACCEPTANCELOG = "usp_P2P_IR_SaveAcceptanceLog";
        internal const string USP_P2P_IR_UPDATEACCEPTANCELOG = "usp_P2P_IR_UpdateAcceptanceLog";
        internal const string USP_P2P_IR_COUNTREMAININGACCEPTANCEREQUIRED = "usp_P2P_IR_CountRemainingAcceptanceRequired";
        internal const string USP_P2P_IR_UPDATEIRSUBTYPE = "usp_P2P_IR_UpdateIRSubType";
        internal const string USP_P2P_IR_GETACCEPTORSLIST = "usp_P2P_IR_GetAcceptorsList";
        internal const string USP_P2P_IR_GETACCEPTANCETRACKSTATUSDETAILS = "usp_P2P_IR_GetAcceptanceTrackStatusDetails";
        internal const string USP_P2P_IR_GETREQUESTERFORDOCUMENT = "usp_P2P_IR_GetRequesterForDocument";
        internal const string USP_P2P_IR_GETBUYERFORDOCUMENT = "usp_P2P_IR_GetBuyerForDocument";
        internal const string USP_P2P_IR_GETIRSTATUS = "usp_P2P_IR_GetIRStatus";
        internal const string USP_P2P_IR_DISABLEIRACCEPTANCEORDER = "usp_P2P_IR_DisableIRAcceptanceOrder";
        internal const string USP_P2P_IR_GETMATCHSTATUSBYIRID = "usp_P2P_IR_GetMatchStatusByIRId";
        internal const string USP_P2P_IR_GETINVOICEIDBYIRID = "USP_P2P_IR_GetInvoiceIdByIRId";
        internal const string USP_P2P_GETVAT = "USP_P2P_GETVAT";
        internal const string USP_P2P_SaveAndGetVATForCreditMemo = "USP_P2P_SaveAndGetVATForCreditMemo";
        internal const string usp_P2P_CM_GetstructureIdByCREDITMEMOId = "usp_P2P_CM_GetStructureIdByCreditMemoId";
        internal const string USP_P2P_IR_GETALLDISPLAYDETAILS = "usp_P2P_IR_GetAllDisplayDetails";
        internal const string USP_P2P_IR_DISABLEACCEPTANCEINSTANCE = "usp_P2P_IR_DisableAcceptanceInstance";

        #endregion

        #region IR Columns
        internal const string COL_IRDOCUMENT_ID = "DocumentId";
        internal const string COL_IRDOCUMENT_NAME = "IRDocumentName";
        internal const string COL_IRDOCUMENT_NUMBER = "IRDocumentNumber";
        internal const string COL_ADDRESS1BILL = "AddressLine1Bill";
        internal const string COL_ADDRESS2BILL = "AddressLine2Bill";
        internal const string COL_ADDRESS3BILL = "AddressLine3Bill";
        internal const string COL_CITYBILL = "CityBill";
        internal const string COL_STATEBILL = "StateBill";
        internal const string COL_ZIPBILL = "ZipBill";
        internal const string COL_STAKEHOLDER_NAME = "StakeHolderName";
        internal const string COL_STAKEHOLDER_TYPE = "StakeHolderType";
        internal const string COL_IRENTITYID = "IREntityId";

        internal const string COL_PARTNERCOMPANY_NAME = "PartnerCompanyName";
        internal const string COL_IRAMOUNT = "IRamount";
        internal const string COL_ORDERCONTACTEMAILID = "OrderContactEmailId";
        internal const string COL_IRCONTACTEMAILID = "IRContactEmailId";
        internal const string COL_IRCONTACTNAME = "IRContactName";
        internal const string COL_IRCONTACTCODE = "IRContactCode";
        internal const string COL_AUTOACKNOWLEDGED_BY_PARTNER = "AutoAcknowledgedByPartner";
        internal const string COL_ACTIONPERFORMED = "ActionPerformed";
        internal const string COL_ACCEPTANCELOGID = "AcceptanceLogId";
        internal const string COL_ACTIONERID = "ActionerId";
        internal const string COL_NAME = "Name";
        internal const string COL_DESIGNATION = "Designation";
        internal const string COL_STATUS = "Status";
        #endregion
        internal const string COL_IRDOCUMENT_CODE = "IRDocumentcode";

        #region Matching

        #region Match Columns
        internal const string COL_DOCUMENT_ID = "DocumentId";
        internal const string COL_PARTNER_STATUS = "PartnerStatus";
        internal const string COL_ITEM_ID = "ItemId";
        internal const string COL_ITEMVALUE = "ItemValue";
        internal const string COL_DOCUMENTTYPE_CODE = "DocumentTypeCode";
        internal const string COL_TITLE = "Title";
        internal const string COL_ORDER_VALUE = "OrderValue";
        internal const string COL_INVOICE_VALUE = "InvoiceValue";
        internal const string COL_ACCEPTED_VALUE = "AcceptedValue";
        internal const string COL_RECEIVED_VALUE = "ReceivedValue";
        internal const string COL_TOTAL_INVOICE_VALUE = "TotalInvoiceValue";
        internal const string COL_MATCH_STATUS_ID = "MatchStatusId";
        internal const string COL_ALLOWSTATUSUPDATE = "AllowStatusUpdate";
        internal const string COL_MATCH_MATCHINGDISPLAYNAME = "DisplayName";
        internal const string COL_ORIGINALQUANTITY = "OriginalQuantity";
        internal const string COL_MATCH_STATUS_IDS = "MatchStatusIds";
        #endregion

        #region Match StoreProcedures
        internal const string USP_P2P_INV_GETALLDOCUMENTFORMATCHING = "usp_P2P_INV_GetAllDocumentForMatching";
        internal const string USP_P2P_INV_GETALLITEMSFORMATCHING = "usp_P2P_INV_GetAllItemsForMatching";
        internal const string USP_P2P_INV_SAVEITEMMATCHING = "usp_P2P_INV_SaveItemMatching";
        internal const string USP_P2P_INV_SAVEDOCUMENTMATCHING = "usp_P2P_INV_SaveDocumentMatching";
        internal const string USP_P2P_INV_GET_EXCEPTION_DETAILS_BY_CODE = "usp_P2P_INV_getExceptionDetailsByCode";
        internal const string USP_P2P_INV_GETLASTINVOICEID = "usp_P2P_INV_GetLastInvoiceId";
        internal const string USP_P2P_INV_GETALLORDERFORMATCHING = "usp_P2P_INV_GetAllOrderForMatching";
        internal const string USP_P2P_GETDOCUMENTSBYPARTNERCODEFORMATCHING = "usp_P2P_GetDocumentsByPartnerCodeForMatching";
        internal const string USP_P2P_INV_GETALLCHARGELINESFORMATCHING = "usp_P2P_INV_GetAllChargeLinesForMatching";
        internal const string USP_P2P_INV_SAVEINVOICEEXTENDEDITEMMATCHING = "usp_P2P_INV_SaveInvoiceExtendedItemMatching";
        #endregion
        #endregion

        #region Receipt

        #region Receipt Columns
        internal const string COL_RECEIPT_ID = "ReceiptId";
        internal const string COL_RECEIPT_NAME = "RecepitName";
        internal const string COL_RECEIPT_RECEIVED_QUANTITY = "ReceivedQuantity";
        internal const string COL_RECEIPT_ACCEPTED_QUANTITY = "AcceptedQuantity";
        internal const string COL_RECEIPT_ITEM_ID = "ReceiptItemId";
        internal const string COL_RECEIPT_ACCEPTED_PREVIOUS_QUANTITY = "PreviouslyAcceptedQuantity";
        internal const string COL_RECEIPT_ACCEPTED_PREVIOUS_AMOUNT = "PreviouslyAcceptedAmount";
        internal const string COL_RECEIPT_ACCEPTED_LATER_QUANTITY = "AcceptedLaterQuantity";
        internal const string COL_RECEIPT_POITEMQUANTITY = "POItemQuantity";
        internal const string COL_RECEIPT_POITEMUNITPRICE = "POItemUnitPrice";
        internal const string COL_RECEIVERID = "ReceiverId";
        internal const string COL_RECEIVERNAME = "ReceiverName";
        internal const string COL_RECEIVEREMAILID = "ReceiverEmailId";
        internal const string COL_RECEIPT_STATUS = "ReceiptStatus";
        internal const string COL_RECEIPT_RETURNED_PREVIOUS_QUANTITY = "PreviouslyReturnedQuantity";
        internal const string COL_RECEIPT_RECEIVED_AMOUNT = "ReceivedAmount";
        internal const string COL_RECEIPT_ITEM_NUMBER = "ReceiptItemNumber";
        internal const string COL_RETURNNOTE_RECEIPT_ACCEPTED_QUANTITY = "ReceiptAcceptedQuantity";

        internal const string COL_ASSETTAGID = "AssetTagId";
        internal const string COL_SERIALNUMBER = "SerialNumber";
        internal const string COL_ASSETKEY = "AssetKey";
        internal const string COL_ASSETLOCATIONID = "AssetLocationId";
        internal const string COL_ASSETLOCATIONTYPE = "AssetLocationType";
        internal const string COL_ASSETLOCATION = "AssetLocation";
        internal const string COL_ASSETLOCATIONCODE = "AssetLocationCode";
        internal const string COL_REMAININGQUANTITY = "RemainingQuantity";
        internal const string COL_ASSETRETURN = "AssetReturn";
        internal const string COL_ASSETTAGSCOUNT = "AssetTagsCount";
        internal const string COL_RECEIVEDDATE = "ReceivedDate";
        internal const string COL_CANCELLEDQUANTITY = "CancelledQuantity";

        internal const string COL_ERRORCODE = "ErrorCode";
        internal const string COL_ERRORVALIDATIONCODE = "ErrorValidationCode";
        internal const string COL_RECEIPT_ISRETURNNOTE = "IsReturnNote";
        internal const string COL_RECEIPT_RETURNNOTETYPE = "ReturnNoteType";
        internal const string COL_RECEIPT_PARTNERRMANUMBER = "PartnerRMANumber";
        internal const string COL_RECEIPT_RETURNEDQUANTITY = "ReturnedQuantity";
        internal const string COL_RECEIPT_ITEM_REASONFORRETURN = "ReasonForReturn";
        internal const string COL_RECEIPT_ITEM_REASONID = "ReasonID";
        internal const string COL_RECEIPT_RECEIPTNUMBER = "ReceiptNumber";
        internal const string COL_RECEIPT_RECEIPTNAME = "ReceiptName";
        internal const string COL_RECEIPT_RECEIPTSOURCE = "ReceiptSource";
        internal const string COL_RECEIPT_HASRETURNNOTE = "HasReturnNote";
        internal const string COL_RECEIPT_TYPE = "ReceiptType";


        internal const string COL_RECEIPT_REASONID = "ReasonId";
        internal const string COL_RECEIPT_REASONFORRETURN = "ReasonForReturn";
        internal const string COL_RECEIPT_CONTACTNAME = "ReceiptContactName";
        internal const string COL_RECEIPT_CONTACT_EMAIL = "ReceiptContactEmail";
        internal const string COL_RECEIPT_CONTACTID = "ReceiptContactId";
        internal const string COL_RECEIPT_ISINTERFACEUPDATE = "IsInterfaceUpdate";
        internal const string COL_RECEIPT_ITEM_REASONFORRETURNCODE = "ReasonForReturnCode";
        internal const string COL_RECEIPT_EXCESS_RECEIPT_ITEMIDS = "ExcessReceiptItemIds";
        internal const string COL_RECEIPTITEMLINENUMBER = "ReceiptLineNumber";
        internal const string COL_RETURN_NOTE_ID = "ReturnNoteId";
        internal const string COL_RETURN_NOTE_NUMBER = "ReturnNoteNumber";
        #endregion


        #region Receipt StoreProcedures
        internal const string USP_P2P_REC_GETALLORDERFORRECEIPTLEFTPANEL = "usp_P2P_REC_GetAllOrderForReceiptLeftPanel";
        internal const string USP_P2P_REC_GETALLREQUISITIONFORLEFTPANEL = "usp_P2P_REC_GetAllRequisitionForLeftPanel";
        internal const string USP_P2P_REC_GETRECEIPTBASICDETAILSBYID = "usp_P2P_REC_GetReceiptBasicDetailsByID";
        internal const string USP_P2P_REC_GETRECEIPTBASICDETAILSBYID_FOR_INTERFACE = "usp_P2P_REC_GetReceiptBasicDetailsByIDForInterface";
        internal const string USP_P2P_REC_GETALLLINEITEMSBYID = "usp_P2P_REC_GetAllLineItemsById";
        internal const string USP_P2P_REC_GETALLLINEITEMSBYID_FOR_INTERFACE = "usp_P2P_REC_GetAllLineItemsByIdForInterface";
        internal const string USP_P2P_REC_CREATERECEIPTFROMORDER = "usp_P2P_REC_CreateReceiptFromOrder";
        internal const string USP_P2P_REC_GENERATEDEFAULTRECEIPTNAME = "usp_P2P_REC_GenerateDefaultReceiptName";
        internal const string USP_P2P_REC_SAVERECEIPT = "usp_P2P_REC_SaveReceipt";
        internal const string USP_P2P_REC_SAVE_RECEIPT_ITEM = "usp_P2P_REC_SaveReceiptItem";
        internal const string USP_P2P_REC_DELETELINEITEMBYID = "usp_P2P_REC_DeleteLineItemById";
        internal const string USP_P2P_REC_VALIDATE_DOCUMENT_BY_CODE = "USP_P2P_REC_Validate_Document_By_Code";
        internal const string USP_P2P_REC_CALCULATE_STATUS = "usp_P2P_REC_CalculateStatus";
        internal const string USP_P2P_REC_UPDATEITEMSFROMIR = "usp_P2P_REC_UpdateItemsFromIR";
        internal const string USP_P2P_REC_GETRECEIPTITEMSASSETTAGSBYRECID = "usp_P2P_REC_GetReceiptItemsAssetTagsByRecId";
        internal const string USP_P2P_REC_VALIDATERECEIPTASSETTAGS = "USP_P2P_REC_ValidateReceiptAssetTags";
        internal const string USP_P2P_REC_GETRECEIPTLINEITEMDEATILSBYID = "usp_P2P_REC_GetReceiptLineItemDeatilsById";
        internal const string USP_P2P_REC_SAVERECEIPTITEMSASSETTAGS = "usp_P2P_REC_SaveReceiptItemsAssetTags";
        internal const string USP_P2P_REC_DELETE_ALL_ASSETTAG_OF_RECEIPT_ITEM = "usp_P2P_REC_DeleteAllAssetTagOfReceiptItem";
        internal const string USP_P2P_REC_GETDOCUMENTBASICDETAILSDOCUMENTNUMBER = "usp_P2P_REC_GetDocumentBasicDetailsDocumentnumber";
        internal const string USP_P2P_REC_GETRECEIPTIDBYNUMBERANDSTATUS = "usp_P2P_REC_GetReceiptIdByReceiptNumberAndStatus";
        internal const string USP_P2P_REC_UPDATEINTERFACESTATUS = "usp_P2P_REC_UpdateInterfaceStatus";
        internal const string USP_P2P_REC_GETRECEIPTSFORINTERFACE = "usp_P2P_REC_GetReceiptsForInterface";
        internal const string USP_P2P_REC_VALIDATEINTERFACEDOCUMENT = "usp_P2P_REC_ValidateInterfaceDocument";
        internal const string USP_P2P_REC_GETALLITEMIDSBYRECEIPTID = "usp_P2P_REC_GetAllItemIdsByReceiptId";
        internal const string USP_P2P_RET_CREATERETURNNOTEFROMRECEIPT = "usp_P2P_RET_CreateReturnNotefromReceipt";
        internal const string USP_P2P_RET_GETRETURNNOTEADDITIONALDETAILS = "usp_P2P_GetReturnNoteAdditionalDetails";
        internal const string USP_P2P_REC_UPDATEQUANTITY = "usp_P2P_REC_UpdateQuantity";
        internal const string USP_P2P_RET_GETREASONFORRETURN = "usp_P2P_RET_GetReasonForReturn";
        internal const string USP_P2P_REC_VALIDATEDOCUMENT = "usp_P2P_REC_ValidateDocument";
        internal const string USP_P2P_REC_GETDETAILSFOREXPORTPDFBYID = "usp_P2P_REC_GetDetailsForExportPDFById";
        internal const string USP_P2P_REC_GETALLDOCUMENTITEMSCHECKEDSTATUS = "usp_P2P_REC_GetAllDocumentItemsCheckedStatus";
        internal const string USP_P2P_REC_GETALLLEFTPANELLINEITEMSBYID = "usp_P2P_REC_GetAllLeftPanelLineItemsById";
        internal const string USP_P2P_PO_VALIDATERECEIPTCREATION = "usp_P2P_PO_ValidateReceiptCreation";
        internal const string USP_P2P_REC_VALIDATEERTFORSELECTEDITEMS = "usp_P2P_REC_ValidateERTForSelectedItems";
        internal const string USP_P2P_GETNOTIFICAITONDETAILSFOREXTERNALRECEIPTS = "usp_P2P_GetNotificaitonDetailsForExternalReceipts";
        internal const string USP_P2P_RET_VALIDATEINTERFACEDOCUMENT = "usp_P2P_RET_ValidateInterfaceDocument";
        internal const string USP_P2P_REC_ISRETURNNOTE = "usp_P2P_REC_IsReturnNote";
        internal const string USP_P2P_REC_SAVEASSETORSERIALEXCELUPLOADREQUETDETAILS = "usp_P2P_REC_SaveAssetOrSerialExcelUploadInfo";
        internal const string USP_P2P_REC_GETRECEIPTASSETORSERIALUPLOADINFO = "usp_P2P_REC_GetReceiptAssetOrSerialUploadInfo";
        internal const string USP_P2P_REC_SAVEASSETORSERIALEXCELDETAILS = "usp_P2P_REC_SaveAssetOrSerialExcelDetails";
        internal const string USP_P2P_REC_GETRECEIPTITEMSASSETTAGSBYRECIDFROMSTG = "usp_P2P_REC_GetReceiptItemsAssetTagsfromStg";
        internal const string USP_P2P_REC_SAVEBULKRECEIPTASSETTAGSFORSTGTABLE = "usp_P2P_REC_SaveBulkReceiptItemsAssetTags_Staging";
        internal const string USP_P2P_REC_SAVEBULKRECEIPTASSETTAGS = "usp_P2P_REC_SaveBulkReceiptItemsAssetTags";
        internal const string USP_P2P_REC_GETITEMACCEPTEDQUANTITY = "usp_P2P_REC_GetItemAcceptedQuantity";
        internal const string USP_P2P_REC_GETALLSHIPTOLOCATIONFORASSETTAGSBULKUPLOAD = "usp_P2P_REC_GetAllShipToLocationsForAssetTagsBulkUpload";
        internal const string USP_P2P_REC_GETITEMDETAILSFORMASTERDATA = "usp_P2P_REC_GetItemDetailsForMasterData";
        internal const string USP_P2P_REC_GETRECEIPTENTITYDETAILSBYID = "usp_P2P_REC_GetReceiptEntityDetailsById";
        internal const string USP_P2P_REC_VALIDATEDOCUMENTSTATUS = "usp_P2P_REC_ValidateDocumentStatus";
        internal const string USP_P2P_REC_CHECKISERSENABLEDPO = "usp_P2P_REC_CheckIsERSEnabledPO";

        #endregion

        #region Receipts Items Asset Tags
        internal const string TVP_P2P_REC_ASSETTAGS = "tvp_P2P_REC_AssetTags";
        internal const string TVP_LOBENTTTYDETAILCODES = "tvp_LOBEntityDetailCodes";
        internal const string TVP_P2P_REC_ASSETTAGSFORSTGTABLE = "tvp_P2P_REC_AssetTagsForStgTable";
        #endregion

        #region Return Note
        #region Return Note Stored Procedures
        internal const string USP_P2P_RET_GETRETURNNOTEITEMSBYRETURNNOTEID = "usp_P2P_RET_GetReturnNoteItemsByReturnNoteId";
        internal const string USP_P2P_RET_SAVERETURNNOTEITEMS = "usp_P2P_RET_SaveReturnNoteItems";
        internal const string USP_P2P_RET_DELETEDOCUMENTBYDOCUMENTCODE = "usp_P2P_RET_DeleteDocumentByDocumentCode";
        internal const string USP_P2P_REC_UPDATERECEIPTITEMCHECKEDSTATUS = "usp_P2P_REC_UpdateReceiptItemCheckedStatus";
        internal const string USP_P2P_RET_UPDATERETURNNOTEITEMREASON = "usp_P2P_RET_UpdateReturnNoteItemReason";
        internal const string USP_P2P_REC_GETRETURNNOTEDETAILSFORINTERFACE = "usp_P2P_REC_GetReturnNoteDetailsForInterface";
        #endregion Return Note Table Types
        internal const string TVP_P2P_RET_RETURNNOTEITEMS = "tvp_P2P_RET_ReturnNoteItems";
        internal const string TVP_P2P_DOCUMENTITEMCHKSTATUS = "tvp_P2P_DocumentItemChkStatus";
        internal const string TVP_P2P_RET_RETURNNOTEITEMSSTATUS = "tvp_P2P_RET_ReturnNoteItemsStatus";

        #endregion
        #endregion

        #region Split Accounting Fields
        #region  Split Accounting Columns

        internal const string COL_PARENTENTITYCODE = "ParentEntityCode";
        internal const string COL_SPLITACCOUNTINGFIELD_CONFIGID = "SplitAccountingFieldConfigId";
        internal const string COL_FIELD_NAME = "FieldName";
        internal const string COL_FIELD_CONTROL_TYPE = "FieldControlType";
        internal const string COL_FIELD_ORDER = "FieldOrder";
        internal const string COL_PARENT_FIELD_CONFIG_ID = "ParentFieldConfigId";
        internal const string COL_ISMANDATORY = "IsMandatory";
        internal const string COL_ENTITY_TYPE_ID = "EntityTypeId";
        internal const string COL_ERROR_CODE = "ErrorCode";
        internal const string COL_CATALOG_ITEM_CONTROL_TYPE = "CatalogItemControlType";
        internal const string COL_DATA_DISPLAY_STYLE = "DataDisplayStyle";
        internal const string COL_AUTO_SUGGEST_URL_ID = "AutoSuggestURLId";
        internal const string COL_IS_ACCOUNTING_ENTITY = "IsAccountingEntity";
        internal const string COL_REQUISITION_SPLIT_ITEM_ID = "RequisitionSplitItemId";
        internal const string COL_ORDER_SPLIT_ITEM_ID = "OrderSplitItemId";
        internal const string COL_PAYMENTREQUEST_SPLIT_ITEM_ID = "PaymentRequestSplitItemId";
        internal const string COL_STRUCTUREID = "StructureId";
        internal const string COL_ENABLESHOWLOOKUP = "EnableShowLookup";

        internal const string COL_SPLIT_TYPE = "SplitType";
        internal const string COL_SPLIT_ITEM_TOTAL = "SplitItemTotal";
        internal const string COL_SPLIT_ACCOUNTING_FIELD_CONFIG_ID = "SplitAccountingFieldConfigId";
        internal const string COL_SPLIT_ACCOUNTING_FIELD_VALUE = "SplitAccountingFieldValue";
        internal const string COL_ENTITY_DISPLAY_NAME = "EntityDisplayName";
        internal const string COL_ENTITY_DISPLAY_NAMES = "EntityDisplayNames";
        internal const string COL_ENTITY_CODE = "EntityCode";
        internal const string COL_ENTITY_ID = "EntityId";
        internal const string COL_CODE_COMBINATION_ORDER = "CodeCombinationOrder";
        internal const string COL_INVOICE_SPLIT_ITEM_ID = "InvoiceSplitItemId";
        internal const string COL_ACCOUNTINGSTATUS = "AccountingStatus";
        internal const string COL_SPLIT_ITEM_ERRORCODE = "ErrorCode";
        internal const string COL_SPLIT_ITEM_DISPLAYNAME = "DisplayName";
        internal const string COL_SPLIT_ITEM_PARENNTENTITYDETAILCODE = "ParentEntityDetailCode";
        internal const string COL_SPLIT_ITEM_ENTITYDETAILCODE = "EntityDetailCode";
        internal const string COL_LEVEL_TYPE = "LevelType";
        internal const string COL_LEVELNO = "LevelNo";
        internal const string COL_PARENT_ENTITY_TYPE = "ParentEntityType";
        internal const string COL_MAPPING_TYPE = "MappingType";
        internal const string COL_ENTITY_TYPE = "EntityType";
        internal const string COL_TAB_ID = "TabIndexId";
        internal const string COL_POPULATE_DEFAULT = "PopulateDefault";
        internal const string COL_SIGNATORS_NAME = "SignatorsName";
        internal const string COL_LOBENTITYDETAILCODE = "LOBEntityDetailCode";
        internal const string COL_AEENTITYDETAILCODE = "AEEntityDetailCode";
        internal const string COL_ORDERTOLERANCEDETAILS = "OrderToleranceDetails";
        internal const string COL_UPDATED_BY = "UpdatedBy";
        internal const string COL_SPLIT_UPDATED_ON = "UpdatedOn";
        internal const string COL_SPLIT_UPDATED_VIA = "UpdatedVia";
        internal const string COL_SPLIT_Number = "SplitNumber";
        #endregion

        #region Derive Header entity 
        internal const string USP_P2P_GETSPLITACCOUNTINGFIELDSWITHDEFAULTVALUESFORINTERFACE = "usp_P2P_GetSplitAccountingFieldsWithDefaultValuesForInterface";
        internal const string TVP_P2P_HEADERENTITYDETAILS = "tvp_P2P_HeaderEntityDetails";
        internal const string COL_LOBENTITYCODE = "LOBEntityCode";
        #endregion

        #region Requisition work bench Saved views
        internal const string COL_REQ_SAVEDVIEW_ID = "SavedViewInfoId";
        internal const string COL_REQ_SAVEDVIEW_NAME = "ViewName";
        internal const string COL_REQ_SAVEDVIEW_COLUMNLIST = "ColumnList";
        internal const string COL_REQ_SAVEDVIEW_FILTERS = "Filters";
        internal const string COL_REQ_SAVEDVIEW_SORTCOLUMN = "SortColumn";
        internal const string COL_REQ_SAVEDVIEW_SORTORDER = "SortOrder";
        internal const string COL_REQ_SAVEDVIEW_GROUPCOLUMN = "GroupColumn";
        internal const string COL_REQ_SAVEDVIEW_ISDEFAULTVIEW = "IsDefaultView";
        internal const string COL_REQ_SAVEDVIEW_ISSYSTEMDEFAULT = "IsSystemDefault";
        internal const string COL_REQ_SAVEDVIEW_LOBID = "LobId";
        #endregion

        #region Split Accounting TableTypes
        internal const string TVP_CHARGEITEMID = "tvp_ChargeItemId";
        internal const string TVP_P2P_ORDERHEADERCHARGEITEM = "tvp_P2P_OrderHeaderChargeItem";
        internal const string TVP_P2P_ORDERLINELEVELCHARGEITEM = "tvp_P2P_OrderLineLevelChargeItem";

        internal const string TVP_P2P_REQUISITIONHEADERCHARGEITEM = "tvp_P2P_RequisitionHeaderChargeItem";
        internal const string TVP_P2P_REQUISITIONLINELEVELCHARGEITEM = "tvp_P2P_RequisitionLineLevelChargeItem";

        internal const string TVP_P2P_SPLITITEMS = "tvp_P2P_SplitItems";
        internal const string TVP_P2P_SPLITITEMSENTITIES = "tvp_P2P_SplitItemsEntities";
        internal const string TVP_P2P_INTERFACESPLITITEMS = "tvp_P2P_InterfaceSplitItems";
        internal const string TVP_P2P_INVOICEHEADERCHARGEITEM = "tvp_P2P_InvoiceHeaderChargeItem";
        internal const string TVP_P2P_INVOICELINELEVELCHARGEITEM = "tvp_P2P_InvoiceLineLevelChargeItem";
        internal const string TVP_P2P_INVOICELINELEVELMATCHSTATUS = "tvp_P2P_InvoiceLineLevelMatchStatus";
        internal const string TVP_P2P_DOCUMENTADDITIONALENTITY = "tvp_P2P_DocumentAdditionalEntity";
        internal const string TVP_P2P_DOCUMENTCODE = "tvp_P2P_DocumentCode";
        internal const string TVP_P2P_ENTITYDETAILCODES = "tvp_P2P_EntityDetailCodes";
        internal const string TVP_P2P_ORDERITEMTAXES = "tvp_P2P_OrderItemTaxes";
        internal const string TVP_P2P_CUSTOMATTRIBUTES = "tvp_P2P_CustomAttributes";
        //internal const string TVP_P2P_BLANKETITEMMAPPING = "tvp_P2P_BlanketItemMapping";
        internal const string TVP_P2P_REQUISITIONITEMTAXES = "tvp_P2P_RequisitionItemTaxes";
        internal const string TVP_P2P_PO_ExternalCodeCombination = "tvp_P2P_PO_ExternalCodeCombination";
        internal const string TVP_P2P_PO_UpdateLineAccountingStatus = "tvp_P2P_PO_UpdateLineAccountingStatus";
        internal const string USP_P2P_INV_GETSPLITDETAILS = "usp_P2P_INV_GetSplitDetails";
        internal const string USP_P2P_INV_GETINVOICEDETAILBYDOCUMENTCODE = "usp_P2P_INV_GetInvoiceDetailByDocumentCode";
        #endregion

        #region  Split Accounting Stored Procedures
        internal const string USP_P2P_GETALLSPLITACCOUNTINGFIELDCONFIGURATIONS = "usp_P2P_GetAllSplitAccountingFieldConfigurations";
        internal const string USP_P2P_INV_GETINVOICEACCOUNTINGDETAILSBYITEMID = "usp_P2P_INV_GetInvoiceAccountingDetailsByItemId";
        internal const string USP_P2P_REQ_CHECK_REQUISITION_ACCOUNTING_SPLIT_VALIDATIONS = "usp_P2P_REQ_CheckRequisitionAccountingSplitValidations";
        internal const string USP_P2P_PO_CHECKACCOUNTINGSPLITVALIDATIONS = "USP_P2P_PO_CheckAccountingSplitValidations";
        internal const string USP_P2P_GETALLSPLITACCOUNTINGDEFAULTVALUES = "usp_P2P_GetAllSplitAccountingDefaultValues";
        internal const string USP_P2P_REQ_SAVEREQUISITION_DEFAULT_ACCOUNTINGDETAILS = "usp_P2P_REQ_SaveRequisitionDefaulAccounting";
        internal const string USP_P2P_REQ_PRORATE_LINEITEM_TAX = "usp_P2P_REQ_ProrateLineItemTax";
        internal const string USP_P2P_REQ_PRORATESHIPPINGANDFREIGHT = "usp_P2P_REQ_ProrateShippingAndFreight";
        internal const string USP_P2P_PO_SAVEORDER_DEFAULT_ACCOUNTING = "usp_P2P_PO_SaveOrderDefaultAccounting";
        internal const string USP_P2P_PR_SAVEPAYMENTREQUEST_DEFAULT_ACCOUNTING = "usp_P2P_PR_SavePaymentRequestDefaultAccounting";
        internal const string USP_P2P_REQ_SAVE_REQ_ADVANCEDPAYMENT_DEFAULT_ACCOUNTINGDETAILS = "usp_P2P_REQ_SaveAdvancePaymentDefaulAccounting";
        internal const string USP_P2P_REQ_SAVE_INV_ADVANCEDPAYMENT_DEFAULT_ACCOUNTINGDETAILS = "usp_P2P_INV_SaveAdvancePaymentDefaultAccounting";
        internal const string USP_P2P_INV_GETALLSPLITACCOUNTINGCODESBYDOCUMENTTYPE = "USP_P2P_INV_GETALLSPLITACCOUNTINGCODESBYDOCUMENTTYPE";
        internal const string USP_P2P_REQ_GETALLSPLITACCOUNTINGCODESBYDOCUMENTTYPE = "USP_P2P_REQ_GETALLSPLITACCOUNTINGCODESBYDOCUMENTTYPE";
        internal const string USP_P2P_REQ_GETVALIDSPLITACCOUNTINGCODES = "usp_P2P_REQ_GetValidSplitAccountingCodes";
        internal const string USP_P2P_REQ_BULKCOPYREQUISITIONLINEITEMS = "usp_P2P_REQ_BulkCopyRequisitionLineItems";
        internal const string USP_P2P_REQ_WEBBULKCOPYREQUISITIONLINEITEMS = "usp_P2P_REQ_WebBulkCopyRequisitionLineItems";
        internal const string USP_P2P_REQ_DELETEREQUISITIONUPLOADSTAGINGDATA = "usp_P2P_REQ_DeleteRequisitionUploadStagingData";
        internal const string USP_P2P_GETALLSPLITITEMIDSBYDOCUMENTCODE = "usp_P2P_GetAllSplitItemIDsByDocumentCode";
        internal const string USP_P2P_INV_VALIDATEACCOUNTINGCODECOMBINATION = "usp_P2P_INV_ValidateAccountingCodeCombination";
        internal const string USP_P2P_INV_VALIDATECODECOMBINATIONSUCCESSBYINVOICEID = "usp_P2P_INV_ValidateCodeCombinationSuccessByInvoiceId";
        internal const string USP_P2P_REQ_GETALLREQUISITIONACCOUNTINGDETAILS = "usp_P2P_REQ_GetAllRequisitionAccountingDetails";
        internal const string USP_P2P_REQ_GETGROUPIDOFVABUSER = "USP_P2P_REQ_GetGroupIdOfVABUser";
        internal const string USP_P2P_REQ_GETREQUISITIONPARTIALDETAILSBYID = "USP_P2P_REQ_GetRequisitionPartialDetailsByID";
        internal const string USP_P2P_REQ_SAVEADDITIONALFIELDATTRIBUTEFROMINTERFACE = "usp_P2P_REQ_SaveAdditionalFieldAttributeFromInterface";
        internal const string USP_P2P_REQ_SAVEHEADERADDITIONALFIELDATTRIBUTEFROMINTERFACE = "usp_P2P_REQ_SaveHeaderAdditionalFieldAttributeFromInterface";
        internal const string USP_INT_GETORDERLOCATIONID = "usp_INT_GetOrderLocationId";
        internal const string USP_P2P_REQ_UPDATEDOCUMENTBUDGETORYSTATUS = "usp_P2P_REQ_UpdateDocumentBudgetoryStatus";
        internal const string USP_P2P_REQ_GETREQUISITIONDETAILSFORCAPITALBUDGET = "usp_P2P_REQ_GetRequisitionDetailsForCapitalBudget";
        internal const string USP_P2P_REQ_UPDATEREQUISITIONITEMSTATUSWORKBENCH = "USP_P2P_REQ_UpdateRequisitionItemStatusWorkBench";
        internal const string USP_P2P_REQ_GETREQUISITIONITEMRFXLINK = "usp_P2P_REQ_GetRequisitionItemRFxLink";

        #endregion
        #endregion

        #region My Task //Note : Need to remove once Localization is implemented
        internal const string SENT_FOR_APPROVAl = "Send for Approval";
        internal const string APPROVE = "Approve";
        internal const string REJECT = "Reject";
        internal const string FINALIZE = "Finalize";
        internal const string SUBMIT_TO_BUYER = "Submit to Buyer";
        internal const string ACKNOWLEDGE = "Acknowledge";
        internal const string SUBMIT_TO_PARTNER = "Submit to Partner";
        internal const string ACCEPT = "Accept";
        internal const string OVERRIDE = "Override";
        #endregion

        #region ScannedImage


        #region ScannedImage Procedure

        internal const string USP_P2P_SI_SAVESCANNEDIMAGEFILEMAPPING = "usp_P2P_SI_SaveScannedImageFileMapping";
        internal const string USP_P2P_SI_GETALLSCANNEDINVOICEFILEID = "usp_P2P_SI_GetAllScannedInvoiceFileId";
        internal const string USP_P2P_SI_GETINVOICESFORSCANNEDIMAGE = "usp_P2P_SI_GetInvoicesForScannedImage";
        internal const string USP_P2P_SI_GETSCANNEDIMAGEADDITIONALDETAILS = "usp_P2P_SI_GetScannedImageAdditionalDetails";
        internal const string USP_P2P_SI_UPDATESCANNEDIMAGEBYDOCUMENTCODE = "usp_P2P_SI_UpdateScannedImageByDocumentCode";
        internal const string USP_P2P_SI_GETIMAGEBYFILEID = "usp_P2P_SI_GetImageByFileId";
        internal const string USP_P2P_SI_GETIMAGEBYDOCUMENTCODE = "usp_P2P_SI_GetImageByDocumentCode";
        internal const string USP_P2P_SI_MAPSCANNEDIMAGETOINVOICE = "usp_P2P_SI_MapScannedImageToinvoice";
        internal const string USP_P2P_SI_UPDATESCANNEDIMAGEFORCREDITMEMO = "usp_P2P_SI_UpdateScannedImageForCreditMemo";
        internal const string USP_P2P_SI_GETALLSCANNEDCREDITMEMOFILEID = "usp_P2P_SI_GetAllScannedCreditMemoFileId";

        #endregion

        #endregion

        #region Tax and Charges

        #region Tax Columns

        internal const string COL_TAXID = "TaxId";
        internal const string COL_TAX_DESC = "TaxDescription";
        internal const string COL_TAX_TYPE = "TaxType";
        internal const string COL_TAX_MODE = "TaxMode";
        internal const string COL_TAX_VALUE = "TaxValue";
        internal const string COL_TAX_CODE = "TaxCode";
        internal const string COL_TAX_ISMANUAL = "IsManual";
        internal const string COL_TAX_ISACCRUETAX = "IsAccrueTax";
        internal const string COL_TAX_ISINTERFACETAX = "IsInterfaceTax";
        internal const string COL_TAX_PERCENTAGE = "TaxPercentage";
        internal const string COL_TAX_ISFLIPPEDFROMORDER = "IsFLippedFromOrder";
        internal const string COL_TAX_ISINCLUDESHIPPING = "IsIncludeShipping";

        #endregion

        #region Stored Procedures

        internal const string USP_P2P_REQ_CALCULATELINEITEMTAX = "usp_P2P_REQ_CalculateLineItemTax";
        internal const string USP_P2P_PO_CALCULATELINEITEMTAX = "usp_P2P_PO_CalculateLineItemTax";
        internal const string USP_P2P_INV_RECALCULATELINEITEMTAX = "usp_P2P_INV_ReCalculateLineItemTax";
        internal const string USP_P2P_REQ_GETITEMDISPATCHMODE = "usp_P2P_REQ_GetItemsDispatchMode";
        internal const string USP_P2P_SAVETAXMASTERFROMINTERFACE = "usp_P2P_SaveTaxMasterFromInterface";
        //internal const string USP_P2P_CALCULATELINEITEMTAX = "usp_P2P_CalculateLineItemTax";
        internal const string TVP_P2P_INVOICETAXES = "tvp_P2P_InvoiceTaxes";
        internal const string TVP_PTL_TAXMASTER = "tvp_PTL_TaxMaster";
        internal const string TVP_P2P_REQUISITIONTAXES = "tvp_P2P_RequisitionTaxes";
        internal const string TVP_P2P_TAXMASTER = "tvp_P2P_TaxMaster";
        internal const string TVP_P2P_TAXJURISDICTIONS = "tvp_P2P_TaxJurisdictions";
        internal const string TVP_P2P_JURISDICTIONORGENTITIES = "tvp_P2P_JurisdictionOrgEntities";
        internal const string TVP_P2P_JURISDICTIONSHIPTOLOCATIONS = "tvp_P2P_JurisdictionShipToLocations";
        #endregion

        #endregion

        #region Notification
        internal const string COL_ORDER_CONTACT_NAME = "OrderContactName";
        internal const string COL_BUYER_COMPANY_NAME = "BuyerCompanyName";
        internal const string COL_ORDER_CONTACT_EMAIL = "OrderContactEmail";
        internal const string COL_ORDER_CONTACT_PHONE = "OrderContactPhone";
        internal const string COL_ORDER_CONTACT_FAX = "OrderContactFax";
        internal const string COL_PARTNER_INVOICING_CONTACTNAME = "PartnerInvoicingContactName";
        internal const string COL_PARTNER_INVOICING_EMAIL = "PartnerInvoicingEmailId";
        internal const string COL_BUYER_EMAIL = "BuyerEmailId";
        internal const string COL_BUYER_CONTACT_NAME = "BuyerContactName";
        internal const string COL_COMMENT = "Comment";
        internal const string COL_DOCUMENTBUSE = "DocumentBUs";
        #endregion

        #region Notification lables
        internal const string NOTN_ParentOrderNumber = "[ParentOrderNumber]";
        internal const string NOTN_SupplierName = "[SupplierName]";
        internal const string NOTN_SupplierInvoiceNumber = "[SupplierInvoiceNumber]";
        internal const string NOTN_SupplierCompanyName = "[SupplierCompanyName]";
        internal const string NOTN_SupplierLegalCompanyName = "[SupplierLegalCompanyName]";
        internal const string NOTN_SupplierInvoicingContactName = "[SupplierInvoicingContactName]";
        internal const string NOTN_Supplier_Name = "[Supplier Name]";
        internal const string NOTN_SupplierUserName = "[SupplierUserName]";
        internal const string NOTN_SupplierUserEmailID = "[SupplierUserEmailID]";
        #endregion


        #region Credit Memo

        internal const string COL_INVOICEDQUANTITY = "InvoicedQuantity";
        internal const string COL_INVOICEDAMOUNT = "InvoicedAmount";
        internal const string COL_ORDEREDUNITPRICE = "OrderedUnitPrice";
        internal const string COL_CREDITMEMO_ID = "CreditMemoId";
        internal const string COL_CREDITATTRIBUTE = "CreditAttribute";

        internal const string TVP_P2P_CM_DETAILS = "tvp_P2P_CM_Details";

        #region Credit Memo StoredProcedures

        internal const string TVP_P2P_CREDITMEMOITEM = "tvp_P2P_CreditMemoItem";
        internal const string TVP_P2P_CREDITMEMOITEMFORINTERFACE = "tvp_P2P_CreditMemoItemForInterface";

        internal const string USP_P2P_CM_VALIDATEPARTNERMEMONUMBER = "usp_P2P_CM_ValidatePartnerMemoNumber";
        internal const string USP_P2P_CM_FLIPCREDITMEMOFROMINVOICE = "usp_P2P_CM_FlipCreditMemoFromInvoice";
        internal const string USP_P2P_CM_GENERATEDEFAULTCREDITMEMONAME = "usp_P2P_CM_GenerateDefaultCreditMemoName";
        internal const string USP_P2P_CM_UPDATERECEIVEDATE = "usp_P2P_CM_UpdateReceiveDate";
        internal const string USP_P2P_CM_FLIPACCOUNTINGSPLITFROMINVTOCM = "usp_P2P_CM_FlipAccountingSplitFromINVToCM";
        internal const string USP_P2P_GETCREDITMEMOADDITIONALDETAILS = "usp_P2P_GetCreditMemoAdditionalDetails";
        internal const string USP_P2P_GETALLDEFAULTENTITYVALUES = "usp_P2P_GetAllDefaultEntityValues";
        internal const string USP_P2P_GETCREDITMEMOQUANTITY = "usp_P2P_GetCreditMemoQuantity";
        internal const string USP_P2P_GETPOBasedCREDITMEMOQUANTITY = "usp_P2P_GetPOBasedCreditMemoQuantity";
        internal const string USP_P2P_GEINVOICEQUANTITYAMOUNT = "usp_P2P_GeInvoiceQuantityAmount";
        internal const string USP_P2P_GETORDERQUANTITYAMOUNT = "usp_P2P_GetOrderQuantityAmount";
        internal const string USP_P2P_CM_VALIDATEINTERFACEDOCUMENT = "usp_P2P_CM_ValidateInterfaceDocument";
        internal const string USP_P2P_CM_FLIPCREDITMEMOFROMORDER = "usp_P2P_CM_FlipCreditMemoFromOrder";
        internal const string USP_P2P_CM_SAVESTANDALONECREDITMEMOFROMINTERFACE = "usp_P2P_CM_SaveStandAloneCreditMemoFromInterface";
        internal const string USP_P2P_CM_VALIDATEBUYERINTERFACEDOCUMENT = "usp_P2P_CM_ValidateBuyerInterfaceDocument";
        internal const string USP_P2P_CM_FLIPACCOUNTINGSPLITFROMPOTOCM = "usp_P2P_CM_FlipAccountingSplitFromPOToCM";
        internal const string USP_P2P_CM_CREATECREDITMEMOSCANIMAGE = "usp_P2P_CM_CreateCreditMemoScanImage";
        internal const string USP_P2P_CM_GETSCANNEDIMAGEIDSBYCREDITMEMOID = "usp_P2P_CM_GetScannedImageIdsByCreditMemoId";
        internal const string USP_P2P_CM_VALIDATEINTERFACECREDITMEMOSTATUSBATCH = "usp_P2P_CM_ValidateInterfaceCreditMemoStatusBatch";
        internal const string USP_P2P_CM_GETCREDITMEMODETAILSFORINTERFACE = "usp_P2P_CM_GetCreditMemoDetailsForInterface";
        internal const string USP_P2P_CM_VALIDATEINTERFACESTANDALONECREDITMEMO = "usp_P2P_CM_ValidateInterfaceStandaloneCreditMemo";
        internal const string USP_P2P_CM_VALIDATEINTERFACESTANDALONEBUYERCREDITMEMO = "usp_P2P_CM_ValidateInterfaceStandaloneBuyerCreditMemo";

        internal const string USP_P2P_CM_GETCREDITMEMOITEMDETAILSFROMINTERFACE = "usp_P2P_CM_GetCreditMemoItemDetailsFromInterface";
        internal const string USP_P2P_CM_PRORATETAX = "usp_P2P_CM_ProrateTax";
        internal const string USP_P2P_CM_GETCMENTITYDETAILSBYID = "usp_P2P_CM_GetCMEntityDetailsById";
        internal const string USP_P2P_CM_SAVECREDITMEMOADDITIONALENTITYDETAILS = "usp_P2P_CM_SaveCreditMemoAdditionalEntityDetails";
        internal const string USP_P2P_CM_HEADERLEVELTAX = "usp_P2P_CM_Headerleveltax";
        internal const string USP_P2P_CM_PRORATEHEADERSHIPPINGANDADDITIONALCHARGES = "usp_P2P_CM_ProrateHeaderShippingAndAdditionalCharges";
        internal const string USP_P2P_CM_GETPARTNERDETAILSFORCREDITMEMO = "usp_P2P_CM_GetPartnerDetailsForCreditMemo";
        internal const string USP_P2P_CM_GETCREDITMEMOSPLITITEMENTITIES = "usp_P2P_CM_GetCreditMemoSplitItemEntities";
        #endregion

        #endregion


        #region Dynamic Discount sp's
        internal const string USP_P2P_DD_GETINVOICEDETAILSFORDYNAMICDISCOUNT = "usp_P2P_DD_GetInvoiceDetailsForDynamicDiscount";
        internal const string USP_P2P_DD_SAVEPARTNERINVITATION = "usp_P2P_DD_SavePartnerInvitation";
        internal const string USP_P2P_DD_SAVEPARTNERRESPONSE = "usp_P2P_DD_SavePartnerResponse";
        internal const string USP_P2P_DD_GETDYNAMICDISCOUNTDETAILS = "usp_P2P_DD_GetDynamicDiscountDetails";
        internal const string USP_P2P_DD_SAVEDYNAMICDISCOUNTDETAILS = "usp_P2P_DD_SaveDynamicDiscountDetails";
        #endregion

        #region Program sp's
        internal const string USP_P2P_PG_GETENFORCEMENTS = "usp_P2P_PG_GetEnforcements";
        internal const string USP_P2P_PG_SAVEPROGRAM = "usp_P2P_PG_SaveProgram";
        internal const string USP_P2P_PG_GETPROGRAMBASICDETAILSBYID = "usp_P2P_PG_GetProgramBasicDetailsById";
        internal const string USP_P2P_PG_GENERATEDEFAULTPROGRAMNAME = "usp_P2P_PG_GenerateDefaultProgramName";
        internal const string USP_P2P_PG_GETPARENTPROGRAMS = "usp_P2P_PG_GetParentPrograms";
        internal const string USP_P2P_PG_GETAllActivePROGRAMS = "usp_P2P_PG_GetAllActivePrograms";
        internal const string USP_P2P_PG_UPDATEPROGRAMDATES = "usp_P2P_PG_UpdateProgramDates";
        internal const string USP_P2P_PG_VALIDATEBUDGET = "usp_P2P_PG_ValidateBudget";
        internal const string USP_P2P_PG_GETPROGRAMADDITIONALDETAILS = "usp_P2P_PG_GetProgramAdditionalDetails";
        internal const string USP_P2P_PG_VALIDATECHILDPROGRAMSTATUS = "usp_P2P_PG_ValidateChildProgramStatus";
        internal const string USP_P2P_PG_SAVEDOCUMENTMAPPING = "usp_P2P_PG_SaveDocumentMapping";
        internal const string USP_P2P_PG_GETPROGRAMLINKEDDOCUMENTDETAILS = "usp_P2P_PG_GetProgramLinkedDocumentDetails";
        internal const string USP_P2P_PG_GETDOCUMENTCOUNT = "usp_P2P_PG_GetDocumentCount";
        internal const string USP_P2P_PG_TRANSFERFUNDS = "usp_P2P_PG_TransferFunds";
        internal const string USP_P2P_PG_SAVEFORMMAPPING = "usp_P2P_PG_SaveFormMapping";
        internal const string USP_P2P_PG_PROGRAM_SAVEADDITIONALENTITYDETAILS = "usp_P2P_PG_Program_SaveAdditionalEntityDetails";
        internal const string USP_P2P_PG_GETPROGRAMBYIDFORNOTIFICATION = "USP_P2P_PG_GETPROGRAMBYIDFORNOTIFICATION";
        internal const string USP_P2P_PG_GETPROGRAMENTITYDETAILSBYID = "USP_P2P_PG_GETPROGRAMENTITYDETAILSBYID";
        #endregion

        #region Operational Budget sp's
        internal const string USP_P2P_OB_GETFINANCIALCALENDARANDPERIOD = "usp_P2P_OB_GetFinancialCalendarAndPeriod";
        internal const string USP_P2P_OB_SAVEFINANCIALCALENDAR = "usp_P2P_OB_SaveFinancialCalendar";
        internal const string USP_P2P_OB_SAVECALENDARPERIOD = "usp_P2P_OB_SaveCalendarPeriod";
        internal const string USP_P2P_OB_VALIDATEPERIODGAPANDOVERLAP = "usp_P2P_OB_ValidatePeriodGapAndOverlap";
        internal const string USP_P2P_OB_GETLEGALENTITIES = "usp_P2P_OB_GetLegalEntities";
        internal const string USP_P2P_OB_GETBUDGETSEGMENTS = "usp_P2P_OB_GetBudgetSegments";
        internal const string USP_P2P_OB_GETBUDGETSTRUCTURE = "usp_P2P_OB_GetBudgetStructure";
        internal const string USP_P2P_OB_GETDIRECTPURCHASE = "usp_P2P_OB_GetDirectPurchase";
        internal const string USP_P2P_OB_GETSTATUS = "usp_P2P_OB_GetStatus";
        internal const string USP_P2P_OB_ADDBUDGETSTRUCTUREENTITYDETAILS = "usp_P2P_OB_SaveBudgetStructureEntityDetails";
        internal const string USP_P2P_OB_SAVEBUDGETALLOCATIONDETAILS = "usp_P2P_OB_SaveBudgetAllocationDetails";
        internal const string USP_P2P_OB_GETBUDGETUTILIZATION = "usp_P2P_OB_GetBudgetUtilization";
        internal const string USP_P2P_OB_GETALLCURRENCYANDEXCHANGERATE = "usp_P2P_OB_GetAllCurrencyAndExchangeRate";
        internal const string USP_P2P_OB_GETBUDGETENFORCEMENTRULES = "usp_P2P_OB_GetBudgetEnforcementRules";
        internal const string USP_P2P_OB_SAVEBUDGETENFORCEMENTRULES = "usp_P2P_OB_SaveBudgetEnforcementRules";

        internal const string USP_P2P_OB_GETPERIODDATAFORAUTOSUGGEST = "usp_P2P_OB_GetPeriodDataForAutoSuggest";
        internal const string USP_P2P_OB_SAVEBUDGETSTRUCTURE = "usp_P2P_OB_SaveBudgetStructure";
        internal const string USP_P2P_OB_UPDATEOPERATIONALBUDGETFUNDSFLOW = "usp_P2P_OB_UpdateOperationalBudgetFundsFlow";
        internal const string USP_P2P_OB_GETORGENTITYNAMESBYENTITYID = "usp_P2P_OB_GetOrgEntityNamesByEntityId";
        internal const string USP_P2P_OB_GETBUDGETALLOCATION = "usp_P2P_OB_GetBudgetAllocation";
        internal const string USP_P2P_OB_GETBUDGETSEGMENTDETAILS = "usp_P2P_OB_GetBudgetSegmentDetails";
        internal const string TVP_P2P_OB_ENFORCEMENTRULE = "tvp_P2P_OB_EnforcementRule";
        #endregion

        #region Operational Budget Columns
        internal const string COL_CALENDARID = "CalendarId";
        internal const string COL_CALENDARNAME = "CalendarName";
        internal const string COL_CALENDARCODE = "CalendarCode";
        internal const string COL_PERIODID = "PeriodId";
        internal const string COL_PERIODCODE = "PeriodCode";
        internal const string COL_PERIODNAME = "PeriodName";
        internal const string COL_STRUCTURE_ID = "StructureID";
        internal const string COL_LEGAL_ENTITY_ID = "EntityID";
        internal const string COL_ENTITY_DETAIL_CODE = "EntityDetailCode";
        internal const string COL_PARENT_ENTITY_DETAIL_CODE = "ParentEntityDetailCode";
        internal const string COL_LEGAL_ENTITY_DISPLAYNAME = "EntityDisplayName";
        internal const string COL_SEGMENTS = "Segments";
        internal const string COL_SEGMENTIDS = "SegmentIds";
        internal const string COL_ORGANIZATION = "Organization";
        internal const string COL_COMPANY = "Company";
        internal const string COL_REGION = "Region";
        internal const string COL_LOCATION = "Location";
        internal const string COL_COST_CENTER = "CostCenter";
        internal const string COL_PROFIT_CENTER = "ProfitCenter";
        internal const string COL_BUSINESS_UNIT = "BusinessUnit";
        internal const string COL_GROUP = "Groups";
        internal const string COL_PROJECT = "Project";
        internal const string COL_COMPANY_CODE = "CompanyCode";
        internal const string COL_OB_BUDGET = "Budget";
        internal const string COL_FUND = "Fund";
        internal const string COL_DIRECT_PURCHASE_ID = "DirectPurchaseId";
        internal const string COL_DIRECT_PURCHASE_NAME = "DirectPurchaseDisplayName";
        internal const string COL_OB_STATUS = "Status";
        internal const string COL_OB_STATUSID = "StatusID";
        internal const string COL_OB_STATUSDISPLAYNAME = "StatusDisplayName";
        internal const string COL_OB_ISDEFAULT = "IsDefault";
        internal const string COL_OB_STRUCTURECODE = "StructureCode";
        internal const string COL_OB_ISALLOCATED = "IsAllocated";
        internal const string COL_OB_IsBudgetMovementRestricted = "IsBudgetMovementRestricted";
        #endregion

        #region Operational Budget For Interface
        internal const string TVP_BZ_BUDGETALLOCATION = "tvp_BZ_BudgetAllocation";
        internal const string TVP_BZ_BUDGETALLOCATIONACCOUNTCODES = "tvp_BZ_BudgetAllocationAccountCodes";
        internal const string USP_P2P_OB_SAVEBUDGETALLOCATION = "usp_P2P_OB_SaveBudgetAllocation";
        internal const string USP_P2P_OB_GETBUDGETALLOCATIONFORINTERFACE = "usp_P2P_OB_GetBudgetAllocationForInterface";
        #endregion
        #region Source System Info sp's
        internal const string USP_P2P_GetSourceSystemInfo = "usp_P2P_GetSourceSystemInfo";
        internal const string USP_P2P_GetAllSourceSystemInfo = "usp_P2P_GetAllSourceSystemInfo";
        #endregion

        #region  Source System Info Columns
        internal const string COL_SOURCESYSTEMID = "SourceSystemId";
        internal const string COL_SOURCESYSTEMNAME = "SourceSystemName";
        #endregion

        #region Exception For Override
        internal const string COL_OVERRIDEFLAG = "OverrideFlag";
        internal const string COL_ISOVERRIDABLE = "IsOverridable";
        internal const string COL_OrderedUOM = "OrderedUOM";
        internal const string COL_InvoiceUOM = "InvoiceUOM";
        #endregion

        #region PaymentRequest

        #region PaymentRequest StoredProcedures
        internal const string USP_P2P_PR_SAVEPAYMENTREQUEST = "usp_P2P_PR_SavePaymentRequest";
        internal const string USP_P2P_PR_GENERATEDEFAULTPAYMENTREQUESTNAME = "usp_P2P_PR_GenerateDefaultPaymentRequestName";
        internal const string USP_P2P_PR_GETPAYMENTREQUESTADDITIONALDETAILS = "usp_P2P_PR_GetPaymentRequestAdditionalDetails";
        internal const string USP_P2P_PR_GETPAYMENTREQUESTBASICDETAILSBYID = "usp_P2P_PR_GetPaymentRequestBasicDetailsById";
        internal const string USP_P2P_GETALLPAYMENTREQUESTDETAILSBYPAYMENTREQUESTID = "usp_P2P_GetAllPaymentRequestDetailsByPaymentRequestId";
        internal const string USP_P2P_PR_GETPAYMENTREQUESTENTITYDETAILSBYID = "usp_P2P_PR_GetPaymentRequestEntityDetailsById";
        internal const string USP_P2P_PR_GETVALIDATIONERRORCODEBYID = "usp_P2P_PR_GetValidationErrorCodeById";
        internal const string USP_P2P_PR_DELETEALLLINEITEMSBYPAYMENTREQUESTID = "usp_P2P_PR_DeleteAllLineItemsByPaymentRequestId";
        internal const string USP_P2P_PR_SAVEPAYMENTREQUESTITEM = "usp_P2P_PR_SavePaymentRequestItem";
        internal const string USP_P2P_PR_GETPAYMENTREQUESTLINEITEMS = "usp_P2P_PR_GetPaymentRequestLineItems";
        internal const string USP_P2P_PR_DELETELINEITEMBYID = "usp_P2P_PR_DeleteLineItemById";
        internal const string USP_P2P_PR_GETALLLINEITEMSBYID = "usp_P2P_PR_GetAllLineItemsById";
        internal const string USP_P2P_PR_SAVEPAYMENTREQUESTITEMOTHER = "usp_P2P_PR_SavePaymentRequestItemOther";
        internal const string USP_P2P_PR_GETOTHERITEMDETAILSBYLIID = "usp_P2P_PR_GetOtherItemDetailsByLiId";
        internal const string USP_P2P_PR_GETPAYMENTREQUESTACCOUNTINGDETAILSBYITEMID = "USP_P2P_PR_GetPaymentRequestAccountingDetailsByItemId";
        internal const string USP_P2P_PR_GETPAYMENTREQUESTITEMACCOUNTINGSTATUS = "usp_P2P_PR_GetPaymentRequestItemAccountingStatus";
        internal const string USP_P2P_PR_SAVEPAYMENTREQUESTACCOUNTINGDETAILS = "usp_P2P_PR_SavePaymentRequestAccountingDetails";
        internal const string USP_P2P_PR_GETPAYMENTREQUEST_ENTITIES = "usp_P2P_PR_GetPaymentRequestEntities";
        internal const string USP_P2P_PR_SAVEPAYMENTREQUESTADDITIONALENTITYDETAILS = "usp_P2P_PR_SavePaymentRequestAdditionalEntityDetails";
        internal const string USP_P2P_PR_PRORATESHIPPINGANDFREIGHT = "usp_P2P_PR_ProrateShippingAndFreight";
        internal const string USP_P2P_PR_CALCULATELINEITEMTAX = "usp_P2P_PR_CalculateLineItemTax";
        internal const string USP_P2P_PR_UPDATETAXONHEADERSHIPTO = "usp_P2P_PR_UpdateTaxOnHeaderShipTo";
        internal const string USP_P2P_PR_PRORATELINEITEMTAX = "usp_P2P_PR_ProrateLineItemTax";
        internal const string USP_P2P_PR_CALCULATE_AND_UPDATELINEITEMTAX = "usp_P2P_PR_CalculateAndUpdateLineItemTax";
        internal const string USP_P2P_PR_GETLINEITEMTAXDETAILS_FORLISTOFDOCUMENTITEMIDS = "usp_P2P_PR_GetLineItemTaxDetails_ForListOfDocumentItemIds";
        internal const string USP_P2P_PR_UPDATELINEITEMSHIPTOLOCATION = "usp_P2P_PR_UpdateLineItemShipToLocation";
        internal const string USP_P2P_PR_UPDATETAXONLINEITEM = "usp_P2P_PR_UpdateTaxOnLineItem";
        internal const string USP_P2P_PR_SAVEPAYMENTREQUESTITEMSHIPPINGDETAILS = "usp_P2P_PR_SavePaymentRequestItemShippingDetails";
        internal const string USP_P2P_PR_GETPaymentRequestTYPES = "usp_P2p_PR_GetPaymentRequestTypes";
        internal const string USP_P2P_PR_SAVEPAYMENTREQUEST_DEFAULT_ACCOUNTINGDETAILSADR = "usp_P2P_PR_SavePaymentRequestDefaultAccountingForADR";
        #endregion

        #region Payment Request Columns
        internal const string COL_PAYMENTREQUEST_ID = "PaymentRequestID";
        internal const string COL_PAYMENTREQUEST_NAME = "PaymentRequestName";
        internal const string COL_PAYMENTREQUEST_NUMBER = "PaymentRequestNumber";
        internal const string COL_PAYMENTREQUEST_STATUS = "PaymentRequestStatus";
        internal const string COL_PAYMENTREQUEST_SOURCE = "PaymentRequestSource";
        internal const string COL_PAYMENTREQUEST_LOCATIONID = "PaymentRequestLocationID";
        internal const string COL_PAYMENTREQUEST_MATCHTYPE = "MatchType";
        internal const string COL_PAYMENTREQUEST_ITEMTOTAL = "ItemTotal";
        internal const string COL_PAYMENTREQUESTAMOUNT = "PaymentRequestAmount";
        internal const string COL_PAYMENTREQUESTSUBMITTEDBY = "SubmittedBy";
        internal const string COL_PAYMENTREQUEST_EXTENDEDSTATUS = "ExtendedStatus";
        internal const string COL_PAYMENTREQUEST_DATEACKNOWLEDGED = "DateAcknowledged";
        internal const string COL_PAYMENTREQUEST_CONTACTCODE = "PaymentRequestContactCode";
        internal const string COL_PAYMENTREQUEST_PAYMENTREQUESTCONTACTEMAIL = "PaymentRequestContactEmail";
        internal const string COL_PAYMENTREQUEST_PAYMENTREQUESTCONTACTCODE = "PaymentRequestContactCode";
        internal const string COL_PAYMENTREQUEST_ISINTERNALITEMEXIST = "IsInternalItemExist";
        internal const string COL_CLOSING_PAYMENTREQUEST_STATUS = "ClosingPaymentRequestStatus";
        //internal const string COL_TRASMISSIONMODE = "TrasmissionMode";
        //internal const string COL_TRANSMISSIONVALUE = "TransmissionValue";
        //internal const string COL_ACKNOWELEDGED_BY = "AcknowledgedBy";
        internal const string COL_PAYMENTREQUEST_TYPE = "PaymentRequestType";
        internal const string COL_PAYMENTREQUEST_PARENTITEM_UNITPRICE = "ParentItemUnitPrice";
        internal const string COL_PAYMENTREQUEST_PARENTITEM_DATENEEDED = "ParentItemDateNeeded";
        //internal const string COL_POSIGNATORYCODE = "POSignatoryCode";
        //internal const string COL_POSIGNATORYNAME = "POSignatoryName";
        internal const string COL_PAYMENTREQUEST_PARENTDOCUMENTCODE = "ParentDocumentCode";
        //internal const string COL_REVISIONNUMBER = "RevisionNumber";
        //internal const string COL_ISCLOSEFORRECEIVING = "IsCloseForReceiving";
        //internal const string COL_ISCLOSEFORINVOICING = "IsCloseForInvoicing";
        //internal const string COL_MATERIAL_RECEIVING_TOLERANCE = "MaterialReceivingTolerance";
        //internal const string COL_SERVICE_RECEIVING_TOLERANCE = "ServiceReceivingTolerance";
        //internal const string COL_ITEM_RECEIVING_TOLERANCE = "ItemReceivingTolerance";
        internal const string COL_PAYMENTREQUEST_CONTACT = "PaymentRequestContact";
        internal const string COL_PRITEM_SHIPPING_ID = "PaymentRequestLineItemShippingID";
        internal const string COL_PRTYPEID = "PRTypeId";
        internal const string COL_CREATED_ON = "CreatedOn";
        internal const string COL_PAYMENTREQUEST_REMITTOLOCATIONID = "RemittoLocationId";
        internal const string COL_CREATERCLIENTCONTACTCODE = "CreaterClientContactCode";
        internal const string COL_CREATEREMAILADDRESS = "CreaterEmailAddress";
        internal const string COL_OBOCLIENTCONTACTCODE = "OBOClientContactCode";
        #endregion

        #region Payment Request item Columns
        internal const string COL_PR_ITEM_ID = "PaymentRequestItemID";
        #endregion
        #endregion

        #region Buisness Details
        internal const string COL_BU_BusinessUnitCode = "BusinessUnitCode";
        internal const string COL_BU_BusinessUnitEntityCode = "BusinessUnitEntityCode";
        internal const string COL_BU_BusinessUnitName = "BusinessUnitName";
        #endregion

        #region Service Procument 
        internal const string COL_PURCHASETYPEID = "PurchaseTypeId";
        internal const string COL_PURCHASETYPE = "PurchaseType";
        internal const string COL_INVOICETYPE = "InvoiceType";
        internal const string COL_ITEMTYPE = "ItemType";
        internal const string COL_ISFLEXIBLECHARGE = "IsFlexibleCharge";
        internal const string COL_PURCHASE_DESCRIPTION = "Description";
        internal const string COL_CREDITMEMOTYPE = "CreditMemoType";
        internal const string COL_FULFILLMENTDOCUMENTTYPE = "FulfillmentDocumentType";
        internal const string COL_FEATUREID = "FeatureId";
        internal const string COL_FEATUREDESCRIPTION = "FeatureDescription";
        internal const string COL_ADDITIONALFIELDID = "AdditionalFieldId";
        internal const string COL_ADDITIONALFIELDNAME = "AdditionalFieldDisplayName";
        internal const string COL_ADDITIONALFIELDTRANSLATEDNAME = "AdditionalFieldTranslatedName";

        internal const string COL_ADDITIONALFIELDTYPEID = "AdditionalFieldId";
        internal const string COL_ADDITIONALFIELDCODE = "AdditionalFieldCode";
        internal const string COL_ADDITIONALFIELDVALUE = "AdditionalFieldValue";
        internal const string COL_ADDITIONALFIELDDETAILCODE = "AdditionalFieldDetailCode";

        internal const string COL_FieldSpecification = "FieldSpecification";
        internal const string COL_IsChildAdditionalField = "IsChildAdditionalField";
        internal const string COL_ParentlFieldId = "ParentlFieldId";

        internal const string COL_FIELDCONTROLTYPE = "FieldControlType";
        internal const string COL_FIELDORDER = "FieldOrder";
        internal const string COL_PARENTADDITIONALFIELDID = "ParentAdditionalFieldId";
        internal const string COL_LEVELTYPE = "LevelType";
        internal const string COL_ISMAPPEDTOORGENTITY = "IsMappedToOrgEntity";
        internal const string COL_ENABLESHOW_LOOKUP = "EnableShowLookup";
        internal const string COL_ISVISIBLEONEXPORTPDF = "IsVisibleOnExportPDF";
        internal const string COL_DataDisplayStyle = "DataDisplayStyle";
        internal const string COL_DocumentSpecification = "DocumentSpecification";
        internal const string COL_FLIPDOCUMENTTYPES = "FlipDocumentTypes";
        internal const string COL_ADDITIONALFIELD_NAME = "AdditionalFieldName";
        internal const string COL_ADDITIONALPARENTFIELDDETAILCODE = "AdditionalParentFieldDetailCode";
        internal const string COL_ADDITIONALPARENTFIELDCODE = "AdditionalParentFieldCode";
        internal const string COL_ADDITIONALPARENTFIELDDISPLAYNAME = "AdditionalParentFieldDisplayName";
        internal const string COL_IsDefault = "IsDefault";

        internal const string COL_PARENTADDITIONALFIELDTYPEID = "ParentFieldId";
        internal const string COL_PARENTADDITIONALFIELDDETAILCODE = "ParentFieldDetailCode";
        internal const string COL_CHILDFIELDID = "ChildFieldId";
        internal const string COL_CHILDFIELDDETAILCODE = "ChildFieldDetailCode";
        internal const string COL_CHILDADDITIONALFIELDCODE = "ChildAdditionalFieldCode";
        internal const string COL_CHILDADDITIONALFIELDDISPLAYNAME = "ChildAdditionalFieldDisplayName";
        internal const string COL_CHILDFIELDCONTROLTYPE = "ChildFieldControlType";
        internal const string COL_ISORGPARENT = "IsOrgParent";


        #endregion

        #region Service Confirmation
        #region ServiceConfirmation StoredProcedure
        internal const string USP_P2P_SC_GENERATEDEFAULTSERVICECONFIRMATIONNAME = "usp_P2P_SC_GenerateDefaultServiceConfirmName";
        internal const string USP_P2P_SC_GETSERVICECONFIRMATIONADDITIONALDETAILS = "usp_P2P_SC_GetServiceConfirmationAdditionalDetails";
        internal const string USP_P2P_SC_CREATESERVICECONFIRMATIONFROMORDER = "usp_P2P_SC_CreateServiceConfirmationFromOrder";
        internal const string USP_P2P_SC_GETSERVICECONFIRMATIONDETAILSFOREXPORTPDFBYID = "usp_P2P_SC_GetServiceConfirmationDetailsForExportPDFById";
        internal const string USP_P2P_SC_CHECKINVOICECREATEDFORSERVICECONFIRMATION = "usp_P2P_SC_CheckInvoiceCreatedForServiceConfirmation";
        internal const string USP_P2P_SC_GETSERVICECONFIRMATIONDETAILSFORNOTIFICATION = "usp_P2P_SC_GetServiceConfirmationDetailsForNotification";
        internal const string USP_P2P_PO_GetAllServiceconfirmation = "usp_P2P_PO_GetAllServiceconfirmation";
        #endregion
        #endregion

        #region Questionnaire
        internal const string COL_QUESTIONNAIRECODE = "QuestionnaireCode";
        internal const string COL_QUESTIONNAIRETITLE = "QuestionnaireTitle";
        internal const string COL_QUESTIONNAIREDESCRIPTION = "QuestionnaireDescription";
        internal const string COL_QUESTIONNAIREORDER = "QuestionnaireOrder";
        internal const string COL_ISSUPPLIERVISIBLE = "IsSupplierVisible";
        internal const string COL_ISINFORMATIVE = "IsInformative";
        internal const string COL_WEIGHTAGE = "Weightage";
        internal const string COL_QUESTIONTEXT = "QuestionText";
        internal const string COL_RESPONSEVALUE = "ResponseValue";
        internal const string COL_QUESTIONID = "QuestionId";
        internal const string COL_QUESTIONTYPEID = "QuestionTypeId";
        internal const string COL_SORTORDER = "SortOrder";
        internal const string COL_ASSESSEEID = "AssesseeId";
        internal const string COL_ASSESSORID = "AssessorId";
        internal const string COL_ASSESSORTYPE = "AssessorType";
        internal const string COL_COLUMNID = "ColumnId";
        internal const string COL_ROWID = "RowId";
        internal const string COL_OBJECTINSTANCEID = "ObjectInstanceId";
        internal const string COL_RESPONSEID = "ResponseId";
        internal const string COL_USERCOMMENTS = "UserComments";
        internal const string COL_TABID = "TabId";
        internal const string COL_ROWTEXT = "RowText";
        internal const string COL_COLUMNTEXT = "ColumnText";
        internal const string COL_ROWDESCRIPTION = "RowDescription";
        internal const string COL_COLUMNDESCRIPTION = "ColumnDescription";
        internal const string COL_CHILDQUESTIONSETCODE = "ChildQuestionSetCode";
        internal const string COL_DATETIMETYPE = "DateTimeType";
        internal const string COL_DATETIMEFORMAT = "DateTimeFormat";
        internal const string COL_ALLOW_SINGLE_SELECT = "AllowSingleSelect";
        internal const string COL_FIELD_TYPE_ID = "FieldTypeId";
        internal const string COL_FIELD_TYPE_NAME = "FieldTypeName";
        internal const string COL_FIELD_GET_SP_NAME = "FieldGetSPName";
        internal const string COL_LOCALIZATION_SUFIX = "LocalizationSufix";
        internal const string COL_ISAUTOSUGGEST = "IsAutoSuggest";
        internal const string COL_CONDITIONAL_ID = "ConditionalId";
        internal const string COL_HASCONDITIONALQUESTION = "HasConditionalQuestion";
        internal const string COL_COLUMNTYPE = "ColumnType";
        internal const string COL_NUMBER_OF_ATTACHMENTS = "NumberofAttachments";
        internal const string COL_IS_ALLOWATTACHMENT = "IsAllowAttachment";
        internal const string COL_IS_ATTACHMENTID = "AttachmentId";
        internal const string COL_FILENAME = "FileName";
        internal const string COL_UPLOADEDDATE = "UploadedDate";

        internal const string COL_COLUMN_ID = "ColumnId";
        internal const string COL_CELL_CHOICE_ID = "CellChoiceId";
        internal const string COL_ROW_ID = "RowId";
        internal const string COL_CHOICE_VALUE = "ChoiceValue";
        internal const string COL_CHOICE_SCORE = "ChoiceScore";
        internal const string COL_IS_DEFAULT = "IsDefault";
        internal const string COL_SECTION = "Section";
        internal const string COL_RESPONSEWEIGHT = "ResponseWeight";
        internal const string COL_RESPONSE = "Response";
        internal const string COL_RISKFORMHEADERINSTRUCTIONSTEXT = "RiskFormHeaderInstructionsText";
        //FileName
        #endregion

        #region FOBShippingDetails
        #region sp's
        internal const string USP_P2P_GETALLFOBCODES = "USP_P2P_GetAllFOBCodes";
        internal const string USP_P2P_GETALLFOBLOCATIONS = "USP_P2P_GetAllFOBLocations";
        internal const string USP_P2P_GETALLFREIGHTTERMS = "USP_P2P_GetAllFreightTerms";
        internal const string USP_P2P_GETALLTRANSITTYPE = "USP_P2P_GetAllTransitType";
        internal const string USP_P2P_GETALLCARRIERS = "USP_P2P_GetAllCarriers";
        internal const string USP_P2P_SAVEFOBSHIPPINGDETAILS = "USP_P2P_SaveFOBShippingDetails";
        internal const string USP_P2P_SAVESTANDARDANDPROCEDUREDETAILS = "USP_P2P_SaveStandardAndProcedureDetails";
        internal const string USP_P2P_PO_GETSTANDARDANDPROCEDUREDETAILS = "USP_P2P_PO_GetStandardAndProcedureDetails";
        #endregion
        // columns
        internal const string COL_FOBCODEID = "FOBCodeID";
        internal const string COL_FOBCODE = "FOBCode";
        internal const string COL_FOBCODEDESCRIPTION = "FOBCodeDescription";
        internal const string COL_FOBLOCATIONID = "FOBLocationID";
        internal const string COL_FOBLOCATIONCODE = "FOBLocationCode";
        internal const string COL_FOBLOCATIONDESCRIPTION = "FOBLocationDescription";
        internal const string COL_CARRIERSID = "CarriersID";
        internal const string COL_CARRIERSCODE = "CarriersCode";
        internal const string COL_CARRIERSDESCRIPTION = "CarriersDescription";
        internal const string COL_FREIGHTTERMSID = "FreightTermsID";
        internal const string COL_FREIGHTTERMSCODE = "FreightTermsCode";
        internal const string COL_FREIGHTTERMSDESCRIPTION = "FreightTermsDescription";
        internal const string COL_TRANSITTYPEID = "TransitTypeID";
        internal const string COL_TRANSITTYPECODE = "TransitTypeCode";
        internal const string COL_TRANSITTYPEDESCRIPTION = "TransitTypeDescription";
        internal const string TVP_STANDARDS_PROCEDURES = "tvp_Standards_Procedures";
        internal const string COL_FULLTEXTS = "FullTexts";
        internal const string COL_MANUFACTURER_SUPPLIERCODE = "ManufacturerSupplierCode";
        internal const string COL_MANUFACTURER_MODEL = "ManufacturerModel";
        #endregion


        #region "Custom Attributes"
        internal const string COL_QUESTIONSETCODE = "QUESTIONSETCODE";
        internal const string USP_BZ_GETQUESTIONSETCODEBYFORMCODE = "usp_BZ_GetQuestionSetCodeByFormCode";
        internal const string USP_QB_SAVEQUESTIONRESPONSE = "usp_QB_SaveQuestionResponse";
        internal const string USP_P2P_SAVECUSTOMATTRIBRESPONSE = "usp_P2P_SaveCustomAttribResponse";
        internal const string USP_P2P_GETQUESTIONRESPONSEBYFORMCODE = "usp_P2P_GetQuestionResponseByFormCode";
        internal const string USP_P2P_GETQUESTIONDETAILSBYFORMCODE = "usp_P2P_GetQuestionDetailsByFormCode";
        internal const string USP_P2P_GETQUESTIONRESPONSEBYQSETCODE = "usp_P2P_GetQuestionResponseByQSetCode";
        internal const string USP_P2P_GETCUSTOMFIELDDETAILFORINTERFACE = "usp_P2P_GetCustomFieldDetailForInterface";
        internal const string USP_P2P_VALIDATECUSTOMFIELDS = "USP_P2P_ValidateCustomFields";
        internal const string USP_P2P_REQ_CALCULATEANDUPDATERISKSCORE = "usp_P2P_REQ_CalculateAndUpdateRiskScore";
        #endregion "Custom Attributes"

        internal const string USP_P2P_GETPUNCHOUTITEMSBYPUNCHOUTCARTREQID = "usp_P2P_GetPunchoutItemsByPunchoutCartReqId";
        internal const string USP_P2P_GETALLEXTRINSICVALUESBYPARTNERCODE = "USP_P2P_GETALLEXTRINSICVALUESBYPARTNERCODE";

        internal const string usp_P2P_GetIRItemCharges = "usp_P2P_GetIRItemCharges";
        internal const string usp_P2P_GetRequisitionItemCharges = "usp_P2P_GetRequisitionItemCharges";


        #region export template 
        internal const string USP_GetDocumentExportTemplate = "usp_P2P_GetDocumentExportTemplate";
        internal const string USP_P2P_GetDivisionCodeEntityCodeEntityDetailMappingByDocumentCode = "usp_P2P_GetDivisionCodeEntityCodeEntityDetailMappingByDocumentCode";
        internal const string USP_P2P_GetPartnerContactCodeByDocumentCode = "usp_P2P_GetPartnerContactCodeByDocumentCode";

        internal const string COL_TemplateHTML = "TemplateHTML";
        internal const string COL_CultureCode = "CultureCode";
        internal const string COL_EntityId = "EntityId";
        internal const string COL_EntityDetailCode = "EntityDetailCode";
        internal const string COL_DivisionEntityCode = "DivisionEntityCode";
        internal const string COL_IsLandscape = "IsLandscape";
        internal const string COL_TimeZone = "TimeZone";
        internal const string COL_PartnerContactCode = "PartnerContactCode";
        internal const string COL_PartnerCode = "PartnerCode";

        #endregion

        internal const string usp_P2P_GetTaxItemsByEntityID = "usp_P2P_GetTaxItemsByEntityID";
        internal const string usp_P2P_GetTaxItemsByMappedEntity = "usp_P2P_GetTaxItemsByMappedEntity";

        #region VAT
        #region columns's
        internal const string COL_IDENTIFICATIONID = "IdentificationId";
        internal const string COL_IDENTIFICATIONVALUE = "IdentificationValueId";
        internal const string COL_IDENTIFICATIONNAME = "IdentificationName";
        internal const string COL_ISBUYERLOCATION = "IsBuyerLocation";
        internal const string COL_SUPPLIERVAT = "SupplierVAT";
        internal const string COL_RECEIPTID = "ReceiptId";
        internal const string COL_SERVICECONFIRMATIONID = "ServiceConfirmationId";
        internal const string COL_ASNCREATED = "ASNCreated";

        #endregion
        #endregion

        #region LOBEntity Configuration
        internal const string COL_FORMCODE = "FormCode";
        internal const string COL_IDENTIFICATIONTYPEID = "IdentificationTypeID";
        internal const string USP_P2P_GETLOBENTITYCONFIGURATIONBYLOBENTITYDETAILCODE = "usp_P2P_GetLOBEntityConfigurationByLOBEntityDetailCode";

        #endregion
        #region Item Charge  

        #region sp's       
        internal const string USP_P2P_GETORDERITEMCHARGES = "USP_P2P_GETORDERITEMCHARGES";
        internal const string USP_P2P_GETINVOICEITEMCHARGES = "USP_P2P_GETINVOICEITEMCHARGES";
        internal const string USP_P2P_GETDEFAULTCALCULATIONBASIS = "usp_P2P_GetDefaultCalculationBasis";
        internal const string USP_P2P_SAVEORDERITEMCHARGES = "usp_P2P_SaveOrderItemCharges";
        internal const string USP_P2P_SAVEINVOICEITEMCHARGES = "usp_P2P_SaveInvoiceItemCharges";
        internal const string USP_P2P_GETALLCHARGENAME = "usp_P2P_GetAllChargeName";
        internal const string USP_P2P_GETALLCHARGEATTRIBUTESETTINGS = "usp_P2P_GetAllChargeAttributeSettings";
        internal const string USP_P2P_GETCHARGEMASTERDETAILSBYCHARGEMASTERID = "usp_P2P_GetChargeMasterDetailsByChargeMasterId";
        internal const string USP_P2P_DELETEORDERITEMCHARGEBYID = "usp_P2P_DeleteOrderItemChargeById";
        internal const string USP_P2P_DELETEINVOICEITEMCHARGEBYID = "usp_P2P_DeleteInvoiceItemChargeById";
        internal const string USP_P2P_PO_SAVEORDERCHARGEDEFAULTACCOUNTING = "usp_P2P_PO_SaveOrderChargeDefaultAccounting";
        internal const string USP_P2P_INV_SAVEINVOICECHARGEDEFAULTACCOUNTING = "usp_P2P_INV_SaveInvoiceChargeDefaultAccounting";
        internal const string USP_P2P_PO_ExternalCodeCombination = "usp_P2P_PO_UpdateExternalErrorCode";
        internal const string USP_P2P_GETALLCHARGEMASTERDETAILSBYCHARGENAME = "usp_P2P_GetItemChargesByName";
        internal const string USP_P2P_SAVEALLORDERITEMCHARGES = "usp_P2P_SaveAllOrderItemCharges";
        internal const string TVP_P2P_ITEMCHARGES = "tvp_P2P_ItemCharges";
        internal const string USP_P2P_SAVEALLREQUISITIONITEMCHARGES = "usp_P2P_SaveAllRequisitionItemCharges";
        internal const string USP_P2P_DELETEREQUISITIONITEMCHARGEBYID = "usp_P2P_DeleteRequisitionItemChargeById";

        #endregion

        #region columns's  
        internal const string COL_ITEMCHARGEID = "ItemChargeId";
        internal const string COL_CHARGEMASTERID = "ChargeMasterId";
        internal const string COL_CHARGENAME = "ChargeName";
        internal const string COL_CHARGEDESCRIPTION = "ChargeDescription";
        internal const string COL_CALCULATIONBASISID = "CalculationBasisId";
        internal const string COL_CALCULATIONVALUE = "CalculationValue";
        internal const string COL_CHARGEAMOUNT = "ChargeAmount";
        internal const string COL_ISINCLUDEFORRETAINAGE = "IsIncludeForRetainage";
        internal const string COL_ISINCLUDEFORTAX = "IsIncludeForTax";
        internal const string COL_TOLERANCEPERCENTAGE = "TolerancePercentage";
        internal const string COL_ISALLOWANCE = "IsAllowance";
        internal const string COL_ISEDITABLEONINVOICE = "IsEditableOnInvoice";
        internal const string COL_CHARGETYPENAME = "ChargeTypeName";
        internal const string COL_CALCULATIONBASIS = "CalculationBasis";
        internal const string COL_CHARGEATTRIBUTESETTINGID = "ChargeAttributeSettingId";
        internal const string COL_ATTRIBUTENAME = "AttributeName";
        internal const string COL_ISEDITABLE = "IsEditable";
        internal const string COL_ISVISIBLE = "IsVisible";
        internal const string COL_CHARGETYPECODE = "ChargeTypeCode";
        internal const string COL_EDICODE = "EDICode";
        internal const string COL_DEFAULTVALUE = "DefaultValue";
        internal const string COL_TOTALALLOWANCE = "TotalAllowance";
        internal const string COL_DOCUMENTTOTAL = "DocumentTotal";
        internal const string COL_DOCUMENTTAX = "DocumentTax";
        internal const string COL_DOCUMENTADDITIONALCHARGE = "DocumentAdditionalCharge";
        internal const string PRM_P2P_TVP_ITEMCHARGES = "@Tvp_ItemCharges";
        internal const string COL_ISHEADERLEVELCHARGE = "IsHeaderLevelCharge";
        #endregion

        #endregion

        #region "Exchangerate"
        internal const string USP_P2P_PO_UpdateDocumentWithConversion = "usp_P2P_PO_UpdateDocumentWithConversion";
        internal const string USP_P2P_GETCURRENCYCONVERSIONRATE = "usp_P2P_GetCurrencyConversionRate";
        internal const string COL_FROMCURRENCY = "FromCurrency";
        internal const string COL_RETURNCURRENCY = "ReturnCurrency";
        internal const string COL_EXCHANGERATE = "ExchangeRate";
        internal const string COL_ISCONVERSIONAVAILABLE = "IsConversionAvailable";
        internal const string USP_CheckConversionOnFlip = "usp_CheckConversionOnFlip";
        internal const string COL_DocumentCodeFlip = "DocumentCode";
        internal const string COL_RequistionCurrencyFlip = "FromCurrency";
        internal const string COL_ToCurrencyFlip = "ToCurrency";
        #endregion "Exchangerate"
        #region "NotificationEntities"
        internal const string USP_P2P_GetAllEntityDetailsByDocumentCode = "usp_P2P_GetAllEntityDetailsByDocumentCode";
        #endregion

        #region ASN  
        internal const string COL_ASNID = "ASNID";
        internal const string COL_NOTESANDATTACHMENTS_ASNID = "ASNId";
        internal const string COL_SHIPPINGDATE = "ShippingDate";
        internal const string COL_EXPECTEDDELIVERYDATE = "ExpectedDeliveryDate";
        internal const string COL_TRACKINGNUMBER = "TrackingNumber";
        internal const string COL_ASSETINFORMATION = "AssetInformation";
        internal const string COL_ASNITEMID = "ASNItemID";
        internal const string COL_SENTQUANTITY = "SentQuantity";
        internal const string COL_ASNStatus = "ASNStatus";
        internal const string COL_SHIPTOLOCATIONID = "ShipToLocationID";
        internal const string COL_SHIPTOLOCATIONNAME = "ShipToLocationName";
        internal const string COL_SHIPTOLOCATIONADDRESS = "ShipToLocationAddress";
        internal const string COL_ASNNAME = "ASNName";
        internal const string COL_ASNNUMBER = "ASNNumber";
        internal const string COL_PARTNERASNNUMBER = "PartnerASNNumber";
        internal const string COL_CREATOR = "Creator";
        internal const string COL_CREATORID = "CreatorId";
        internal const string COL_ORDERID = "OrderId";
        internal const string COL_ORDERRECEIVEDDATE = "OrderReceivedOn";
        internal const string COL_ORDERACKNOWLEDGEDDATE = "OrderAcknowledgedOn";
        internal const string COL_SHIPPEDQUANTITY = "ShippedQuantity";
        internal const string COL_PREVIOUSLYACCEPTEDQUANTITY = "PreviouslyAcceptedQuantity";
        internal const string COL_PREVIOUSLYSHIPPEDQUANTITY = "PreviouslyShippedQuantity";
        internal const string COL_ISVALID = "IsValid";
        internal const string COL_LINEITEM = "LineItem";
        internal const string TVP_ASNITEM = "tvp_ASNItem";
        internal const string USP_P2P_SAVEASN = "usp_P2P_SaveASN";
        internal const string COL_PARTNERADDRESS = "PartnerAddress";
        internal const string COL_PARTNEREMAILADDRESS = "PartnerEmailAddress";
        internal const string COL_PARTNERPHONENUMBER = "PartnerPhoneNumber";
        internal const string COL_BUYERNAME = "BuyerName";
        internal const string COL_BUYERADDRESS = "BuyerAddress";
        internal const string COL_BUYEREMAILADDRESS = "BuyerEmailAddress";
        internal const string COL_BUYERPHONENUMBER = "BuyerPhoneNumber";
        internal const string COL_ORDERIDS = "OrderIds";
        internal const string USP_P2P_ASN_GETASNADDITIONALDETAILS = "usp_P2P_ASN_GetASNAdditionalDetails";
        internal const string USP_P2P_ASN_GENERATEDEFAULTASNNAME = "usp_P2P_ASN_GenerateDefaultASNName";
        internal const string USP_P2P_ASN_GETALLDISPLAYDETAILS = "usp_P2P_ASN_GetAllDisplayDetails";
        internal const string USP_P2P_ASN_VALIDATEASNCREATION = "usp_P2P_ASN_ValidateASNCreation";
        internal const string USP_P2P_ASN_VALIDATERECEIPTCREATIONFORASN = "usp_P2P_ASN_ValidateReceiptCreationForASN";
        internal const string USP_P2P_ASN_CreateASNFROMORDERS = "usp_P2P_ASN_CreateASNFromOrders";
        internal const string USP_P2P_ASN_GetASNDetailsForNotification = "usp_P2P_ASN_GetASNDetailsForNotification";
        internal const string USP_P2P_ASN_ValidateASNONSUBMIT = "usp_P2P_ASN_ValidateASNOnSubmit";
        internal const string USP_P2P_ASN_SAVENOTESANDATTACHMENTS = "usp_P2P_ASN_SaveNotesAndAttachments";
        internal const string USP_P2P_ASN_GETNOTESANDATTACHMENTS = "usp_P2P_ASN_GetNotesAndAttachments";
        internal const string USP_P2P_ASN_DELETENOTESANDATTACHMENTS = "usp_P2P_ASN_DeleteNotesAndAttachments";
        internal const string USP_DM_GETDOCUMENTBUS = "usp_DM_GetDocumentBUs";
        internal const string USP_P2P_ASN_CREATERECEIPTFROMASN = "usp_P2P_ASN_CreateReceiptFromASN";
        internal const string USP_P2P_ASN_ValidateChangeOrder = "usp_P2P_ASN_ValidateChangeOrder";
        internal const string USP_P2P_ASN_GETASNDETAILSBYID = "usp_P2P_ASN_GetAsnDetailsByID";
        internal const string USP_P2P_ASN_GETASNFORINTERFACE = "usp_P2P_ASN_GetAsnForInterface";
        internal const string USP_P2P_ASN_UPDATEINTERFACESTATUS = "usp_P2P_ASN_UpdateInterfaceStatus";
        internal const string USP_P2P_PO_UPDATEORDEREXTENDEDSTATUS = "usp_P2P_PO_UpdateOrderExtendedStatus";

        #endregion

        #region Escalation Details  
        internal const string TVP_DM_DOCUMENT_ESCALATION = "tvp_DM_DocumentEscalation";
        internal const string USP_UPDATE_DM_ESCALATION_LOG_TABLE = "usp_Update_DM_EscalationLogDtl";
        internal const string USP_WF_ESCALATE_DOCUMENT_TO_MANAGER = "usp_WF_EscalateDocumentToManager";
        #endregion
        #region RegistrationLocationDetails
        internal const string COL_COMPANYIDENTIFICATIONDISPLAYNAME = "CompanyIdentificationDisplayName";
        internal const string COL_COMPANYIDENTIFICATION = "CompanyIdentification";
        #endregion

        #region ADR
        internal const string COL_SourceCombinationId = "SourceCombinationId";
        internal const string COL_ADRDescription = "Description";
        internal const string COL_ADRDSourceDescription = "SourceDescription";
        internal const string COL_ADRDSourceid = "SourceId";
        internal const string COL_ADRDSourceValue = "SourceValue";
        internal const string COL_ADRDSourceMapPartId = "SourceMapPartId";
        internal const string COL_ADRENTITY_LEVEL = "level";
        internal const string COL_ADRMappingHeaderSetId = "MappingHeaderSetId";
        internal const string COL_ADRLOBID = "LOBID";
        internal const string COL_ADRSTRUCTUREID = "StructureID";
        internal const string COL_ADRSetDescription = "SetDescription";
        internal const string COL_ADROutPutObject = "OutPutObject";
        internal const string COL_ADRPrecedence = "Precedence";
        internal const string COL_ADRSetId = "SetId";
        internal const string COL_ADRTargetId = "TargetId";
        internal const string COL_ADRTargetDescription = "TargetDescription";
        internal const string COL_ADRTargetEntityDetailCode = "TargetEntityDetailCode";
        internal const string COL_ADRTargetValue = "TargetValue";
        internal const string COL_Identifier = "Identifier";
        internal const string COL_RESTURL = "RestUrl";
        internal const string COL_ISACCOUNTINGSEGMENT = "IsAccountingsegment";
        #endregion

        #region ADR
        #region SP
        internal const string USP_ADR_GETALLHEADERSET = "USP_ADR_GetAllHeaderSet";
        internal const string USP_ADR_GETALLDATASET = "USP_ADR_GetAllDataset";
        internal const string USP_P2P_GETALLADRSOURCES = "USP_P2P_GetAllADRSources";
        internal const string USP_ADR_GETENTITYDETAILSBYCODES = "USP_ADR_GetEntityDetailsByCodes";
        internal const string USP_ADR_GETENTITYDETAILSBYUSERDEFUALT = "USP_ADR_GetEntityDetailsByUserDefualt";
        internal const string USP_ADR_GETALLCATEGORYGLMAPPING = "USP_ADR_GetAllCategoryGLMapping";
        internal const string USP_ADR_GETPERIODDEFAULTDATA = "USP_ADR_GetPeriodDefaultData";
        internal const string USP_P2P_REQ_SAVEREQUISITION_DEFAULT_ACCOUNTINGDETAILSADR = "USP_P2P_REQ_SaveRequisitionDefaultAccountingForADR";
        internal const string USP_P2P_PO_SAVEORDER_DEFAULT_ACCOUNTINGDETAILSADR = "USP_P2P_PO_SaveOrderDefaultAccountingForADR";
        #endregion
        #region TVP
        internal const string TVP_ORG_ENTITYDETAILS = "tvp_ORG_EntityDetails";
        internal const string TVP_P2P_ADRSPLITITEMSENTITIES = "tvp_P2P_ADRSplitItemsEntities";
        internal const string TVP_P2P_INV_ORDERACCEPTANCELIST = "tvp_P2P_INV_OrderAcceptanceList";
        #endregion
        #endregion

        internal const string USP_P2P_GET_REQUISITION_ITEM_COUNT = "usp_P2P_Get_RequisitionItemCount";
        internal const string COL_TOTAL_PUNCHOUT_ITEMS_COUNT = "TotalPunchoutItemCount";
        internal const string USP_P2P_IR_GETACCEPTANCEINSTANCEID = "usp_P2P_IR_GetAcceptanceInstanceId";
        internal const string COL_PARTNERCOMPANYIDENTIFICATIONID = "PartnerCompanyIdentificationId";
        internal const string COL_COMPANYIDENTIFICATIONTYPEID = "CompanyIdentificationTypeId";
        internal const string USP_P2P_GETALLENTITYVALUEAMOUNT = "USP_P2P_GetAllEntityValueAmount";
        internal const string USP_P2P_INV_SAVEINVOICESOURCEDETAILS = "usp_P2P_INV_SaveInvoiceSourceDetails";
        internal const string USP_P2P_GETDOCUMENTANDCONTACTDETAILS = "usp_P2P_GetDocumentAndContactDetails";
        internal const string USP_P2P_GETDOCUMENTEXPORTTEMPLATELANGUAGE = "usp_P2P_GetDocumentExportTemplateLanguage";
        internal const string USP_P2P_GETSPLITITEMSENTITYCHANGE = "usp_P2P_GetSplitItemsEntityChange";
        internal const string USP_P2P_GETENTITYMANAGERMAPPINGBYSTRUCTUREID = "USP_P2P_GetEntityManagerMappingByStructureId";
        internal const string USP_P2P_PO_UPDATEDOCUMENTVALIDATIONERRORCODE = "usp_P2P_PO_UpdateDocumentWithValidationErrorCode";

        internal const string COL_POCUMULATIVEITEMCHARGES = "POCumulativeItemCharges";
        internal const string COL_POCUMULATIVEITEMVALUE = "POCumulativeItemValue";
        internal const string COL_REQCUMULATIVEITEMCHARGES = "ReqCumulativeItemCharges";
        internal const string COL_REQCUMULATIVEITEMVALUE = "ReqCumulativeItemValue";
        internal const string USP_P2P_PO_GETPARENTDOCFORCHANGEREQUEST = "USP_P2P_PO_GetParentDocForChangeRequest";

        #region Order Timesheet details
        internal const string COL_DraftTimeSheets = "DraftTimesheets";
        internal const string COL_ProcessedTimeSheets = "SubmittedTimesheets";
        internal const string USP_P2P_PO_GETORDERTIMESHEETDETAILS = "usp_P2P_PO_GetOrderTimesheetDetails";
        #endregion

        internal const string USP_P2P_PO_SAVEMULTIREQUISITIONSORDERMAPPING = "usp_P2P_PO_SaveMultiRequisitionsOrderMapping";
        internal const string USP_P2P_PO_RETRIGGERORDER = "usp_P2P_PO_RetriggerOrder";

        #region PO TermsAndConditions
        internal const string USP_P2P_PO_GETTERMSANDCONDISTIONSBYIDS = "usp_P2P_PO_GetTermsAndConditionsByIds";
        internal const string COL_TERM_CONDITION_ID = "TermConditionId";
        internal const string COL_TEMPLATE_NAME = "TemplateName";
        internal const string COL_TEMPLATE_DESCRIPTION = "TemplateDescription";
        internal const string COL_TERMS_CONDITION_TEXT = "TermsConditionText";
        internal const string COL_BU_ENTITY_CODE = "BUEntityCode";
        internal const string COL_UPDATED_BY_NAME = "UpdatedByName";
        #endregion

        internal const string USP_P2P_REQ_UPDATERISKSCOREANDCATEGORY = "usp_P2P_REQ_UpdateRiskScoreAndCategory";
        internal const string USP_P2P_PUSHREQUISITIONTOINTERFACE = "usp_P2P_PushRequisitionToInterface";
        internal const string USP_P2P_REQ_GETBUDGETDETAILS = "USP_P2P_REQ_GetBudgetDetails";
        internal const string TVP_P2P_BZ_REQUISITIONITEMADDITIONALFIELDS = "tvp_P2P_BZ_RequisitionItemAdditionalFields";
        internal const string TVP_P2P_BZ_REQUISITIONHEADERADDITIONALFIELDS = "tvp_P2P_BZ_RequisitionHeaderAdditionalFields";
        internal const string USP_P2P_REQ_SAVEREQITEMBLANKETMAPPING = "USP_P2P_REQ_SaveReqItemBlanketMapping";
        internal const string REQUISITIONITEMADDITIONALFIELDDETAILS = "RequisitionItemAdditionalFieldDetails";
        internal const string USP_P2P_REQ_GETORDERINGLOCATIONBYLOCATIONID = "usp_P2P_REQ_GetOrderingLocationByLocationID";
        internal const string USP_P2P_REQ_GETBLANKETDETAILSBYREQITEMID = "usp_P2P_REQ_GetBlanketDetailsByReqItemID";
        internal const string USP_P2P_REQ_GETALLCHILDADDITIONALFIELDWITHDEFAULTVALUES = "usp_P2P_Req_GetAllChildAdditionalFieldsWithDefaultValues";
        internal const string USP_P2P_REQ_GETTAXJURISDICTIONSBYSHIPTOLOCATIONID = "usp_P2P_Req_GetTaxJurisdcitionsByshiptoLocationId";
    internal const string USP_P2P_REQ_GETSOURCEDOCUMENTADDITIONALFIELDSCONFIG= "usp_P2P_Req_GetSourceDocumentAdditionalFieldsConfig";

        internal const string BLANKET_DOCUMENT_NUMBER = "BlanketDocumentNumber";
        internal const string BLANKET_AMOUNT_CONSUMED = "AmountConsumed";

        internal const string REQUISITIONSPLITITEMID = "RequisitionSplititemId";
        internal const string BUDGETID = "BudgetId";
        internal const string BUDGETENTITYALLOCATIONID = "BudgetEntityAllocationId";
        internal const string REQUISITIONID = "RequisitionId";
        internal const string BUDGETOWNERCONTACTCODE = "BudgetOwnerContactcode";

        internal const string JURISDICTIONCODE = "JurisdictionCode";
        internal const string JURISDICTIONNAME = "JurisdictionName";
        internal const string SHIPTOLOCATIONID = "ShiptoLocationID";
        internal const string JURISDICTIONID = "JurisdictionId";

        #region requesterchange
        internal const string USP_P2P_REQ_GETALLREQUISITIONSFORUTILITY = "usp_P2P_REQ_GetAllRequisitionsForUtility";
        internal const string USP_P2P_REQ_SAVEREQUISITIONFORREQUESTERCHANGE = "usp_P2P_REQ_SaveRequisitionForRequesterChange";
        internal const string USP_SEARCH_SAVEINDEXERQUEUEINGDETAILS = "usp_Search_SaveIndexerQueueingDetails";
        #endregion

        internal const string USP_P2P_REQ_GETACTIVECONTACTSMANAGERMAPPING = "usp_P2P_REQ_GetActiveContactsManagerMapping";
        internal const string COL_MANAGERCONTACTCODE = "ManagerContactCode";

        internal const string USP_P2P_REQ_GetAllLevelCategories = "usp_P2P_REQ_GetAllLevelCategories";

        internal const string USP_P2P_REQ_UPDATEEXTENDEDSTATUSFORHOLDREQUISITION = "usp_P2P_REQ_UpdateExtendedStatusForHoldRequisition";
    }
}

