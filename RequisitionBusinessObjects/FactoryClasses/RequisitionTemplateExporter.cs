using Gep.Cumulus.CSM.Entities;
using GEP.Cumulus.P2P.BusinessEntities;
using GEP.NewP2PEntities;
using GEP.NewPlatformEntities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExcelLib = Aspose.Cells;

namespace GEP.Cumulus.P2P.Req.BusinessObjects.FactoryClasses
{
    public class RequisitionTemplateExporter : RequisitionTemplateGenerator
    {
        private long documentCode = 0;
        private NewRequisitionManager RequisitionManger = null;
        public RequisitionTemplateExporter(UserExecutionContext userContext, Int64 documentCode, string documentNumber, NewRequisitionManager ReqManger, GEP.NewP2PEntities.FileManagerEntities.ReqTemplateFileResponse reqTemplateResponse, string jwtToken)
            : base(userContext, documentCode, documentNumber, ReqManger, jwtToken, RequisitionExcelTemplateHandler.ExportTemplate)
        {

            this.documentCode = documentCode;
            RequisitionManger = ReqManger;
            GenerateTemplate();
            ExportLineItems();
            reqTemplateResponse.FileId = UploadFiletoBlobContainerAndGetFileId();
        }

        public void ExportLineItems()
        {
            string manufacturerDetails = string.Empty;
            if (wsRequisitionLines != null)
            {
                //Call service Method
                var req = RequisitionManger.GetRequisitionLineItemsByRequisitionId(documentCode);
                if (req != null)
                {
                    if (req.items != null && req.items.Count() > 0)
                    {
                        int RequisitionLinesRowIndex = (int)RequisitionExcelTemplateRowStartIndex.RequisitionLinesRowStartIndex;
                        RequisitionTaxLinesRowIndex = (int)RequisitionExcelTemplateRowStartIndex.RequisitionLinesRowTaxCodesStartIndex;
                        foreach (NewP2PEntities.RequisitionItem objReqItem in req.items)
                        {
                            //Get the Excel Row
                            ExcelLib.Row excelRowLine = wsRequisitionLines.Cells.Rows[RequisitionLinesRowIndex + 1];
                            if (excelRowLine != null)
                            {
                                //Write Data into Rows
                                excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.Operation).PutValue("Update");
                                excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.LineReferenceNumber).PutValue(objReqItem.lineNumber.ToString());
                                if (objReqItem.shipTo != null)
                                    excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.ShipToLocation).PutValue(objReqItem.shipTo.name);
                                if (objReqItem.type != null)
                                {
                                    if (objReqItem.type.id == (int)ItemExtendedType.Material)
                                        excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.ItemType).PutValue(objReqItem.type.name);
                                    else if (objReqItem.type.id == (int)ItemExtendedType.Fixed)
                                        excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.ItemType).PutValue("Fixed Service");
                                    else if (objReqItem.type.id == (int)ItemExtendedType.Variable)
                                        excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.ItemType).PutValue("Variable Service");
                                }
                                if (objReqItem.matching != null)
                                {
                                    if (objReqItem.matching.name.Equals("P2P_CMN_TwoWayMatch"))
                                        excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.MatchType).PutValue("2-Way");
                                    else if (objReqItem.matching.name.Equals("P2P_CMN_ThreeWayMatch"))
                                        excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.MatchType).PutValue("3-Way");
                                    else
                                        excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.MatchType).PutValue(" ");
                                }
                                if (objReqItem.buyerItemNumber != null)
                                    excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.ItemNumber).PutValue(objReqItem.buyerItemNumber);
                                if (objReqItem.partnerItemNumber != null)
                                    excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.SupplierItemNumber).PutValue(objReqItem.partnerItemNumber);
                                if (objReqItem.contractNumber != null)
                                    excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.ContractNumber).PutValue(objReqItem.contractNumber);
                                if (objReqItem.ContractItemId != null)
                                    excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.ContractItemLineRef).PutValue(objReqItem.ContractItemId);
                                //excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.BlanketNumber).PutValue(objReqItem.BlanketNumber.ToString());
                                if (objReqItem.description != null)
                                    excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.Description).PutValue(objReqItem.description);
                                if (objReqItem.category != null)
                                    excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.Category).PutValue(objReqItem.category.name);
                                excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.QuantityEffort).PutValue(objReqItem.quantity.ToString());
                                if (objReqItem.uom != null)
                                    excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.UnitofMeasure).PutValue(objReqItem.uom.name);
                                excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.UnitPrice).PutValue(objReqItem.unitPrice.ToString());
                                if (objReqItem.currencyCode != null)
                                    excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.Currency).PutValue(objReqItem.currencyCode);
                                excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.OtherCharges).PutValue(objReqItem.otherCharges.ToString());
                                excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.ShippingAndFreight).PutValue(objReqItem.shippingCharges.ToString());
                                if (objReqItem.needByDate.HasValue)
                                    excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.NeedByDate).PutValue(objReqItem.needByDate.Value.ToShortDateString());
                                if (objReqItem.startDate.HasValue)
                                    excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.ServiceStartDate).PutValue(objReqItem.startDate.Value.ToShortDateString());
                                if (objReqItem.endDate.HasValue)
                                    excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.ServiceEndDate).PutValue(objReqItem.endDate.Value.ToShortDateString());
                                if (objReqItem.partner != null)
                                    excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.Supplier).PutValue(objReqItem.partner.name);
                                if (objReqItem.orderingLocation != null)
                                    excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.SupplierOrderingLocation).PutValue(objReqItem.orderingLocation.code);
                                if (objReqItem.partnerContact != null)
                                    excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.SupplierOrderingContact).PutValue(objReqItem.partnerContactEmail);
                                //if (objReqItem.ShipFromLocation != null)
                                //    excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.ShipFromLocation).PutValue(objReqItem.ShipFromLocation.name);
                                ExportShipFrom(excelRowLine, objReqItem);
                                if (objReqItem.manufacturer == null && objReqItem.ManufacturerModel == null && objReqItem.manufacturerPartNumber == null)
                                    manufacturerDetails = "";
                                else
                                    manufacturerDetails = GetManufacturerDetails(objReqItem.manufacturer, objReqItem.manufacturerPartNumber, objReqItem.ManufacturerModel);

                                excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.ManufacturerDetails).PutValue(manufacturerDetails);

                                if (objReqItem.isTaxExempt)
                                {
                                    excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.TaxExempt).PutValue("Yes");
                                }
                                else
                                {
                                    excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.TaxExempt).PutValue("No");
                                }

                                if (objReqItem.TrasmissionMode > 0)
                                {
                                    excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.DispatchMode).PutValue(GetTrasmissionModeText((POTransmissionMode)objReqItem.TrasmissionMode));

                                    excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.DispatchInfo).PutValue(objReqItem.TransmissionValue);

                                }
                                if (objReqItem.deliverToStr != null)
                                    excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.DeliverTo).PutValue(objReqItem.deliverToStr);

                                ExportSingleSplitAccountingData(objReqItem, excelRowLine);
                                if (wsTaxCodes != null)
                                {
                                    if (objReqItem.taxItems.Count > 0 || !objReqItem.isTaxExempt)
                                        ExporTaxCodesData(objReqItem, excelRowLine);
                                }
                                RequisitionLinesRowIndex++;
                            }
                        }

                        ExportMultipleSplitAccountingDataList(req.items);
                    }
                }
            }
            //workbook.Save(@"C:\TEMP\EXPORTTEST.xlsx");
        }
        private string GetManufacturerDetails(string manufacturerName, string manufacturerPartName, string manufacturerModel)
        {
            StringBuilder manufacturerDetails = new StringBuilder();
            List<string> tmpName = new List<string>();
            List<string> tmpPartName = new List<string>();
            List<string> tmpModel = new List<string>();
            int count = 0;

            if (!String.IsNullOrEmpty(manufacturerName))
            {
                tmpName = manufacturerName.Split('|').ToList();
                count = tmpName.Count;
            }

            if (!String.IsNullOrEmpty(manufacturerPartName))
            {
                tmpPartName = manufacturerPartName.Split('|').ToList();
                count = tmpPartName.Count;
            }
            if (!String.IsNullOrEmpty(manufacturerModel))
            {
                tmpModel = manufacturerModel.Split('|').ToList();
                count = tmpModel.Count;
            }

            for (var i = 0; i < count; i++)
            {
                manufacturerDetails.Append(tmpName.Count == 0 ? ":" : tmpName[i] + ":");
                manufacturerDetails.Append(tmpPartName.Count == 0 ? ":" : tmpPartName[i] + ":");
                manufacturerDetails.Append(tmpModel.Count == 0 ? "" : tmpModel[i]);
                if (i != count - 1)
                    manufacturerDetails.Append("|");
            }

            return manufacturerDetails.ToString();
        }
        private void ExportSingleSplitAccountingData(NewP2PEntities.RequisitionItem objReqItem, ExcelLib.Row excelRowLine)
        {
            if (objReqItem.splits != null && objReqItem.splits.Count() > 0)
            {
                //if only one split write into Requistion Lines Tab else write into Accounting Splits Tab
                if (objReqItem.splits.Count() == 1 && objReqItem.splits[0] != null)
                {
                    ExportAccountingData(excelRowLine, htItemsTabAccountingColumnIndex, objReqItem.splits[0]);
                }
            }
        }
        private void ExportMultipleSplitAccountingDataList(List<NewP2PEntities.RequisitionItem> lstReqItems)
        {
            var reqItemList = lstReqItems.Where(x => x.splits != null && x.splits.Count() > 1).OrderBy(x => x.lineNumber);

            if (reqItemList != null && reqItemList.Count() > 0)
            {
                int AccountignLinesRowStartIndex = (int)RequisitionExcelTemplateRowStartIndex.RequisitionAccountingSplitRowStartIndex;
                foreach (NewP2PEntities.RequisitionItem objReqItem in reqItemList)
                {
                    //Get the Excel Row
                    if (objReqItem.splits != null && objReqItem.splits.Count() > 0)
                    {
                        foreach (ReqAccountingSplit objSplit in objReqItem.splits)
                        {
                            ExcelLib.Row excelRowLine = wsAccountingSplits.Cells.Rows[AccountignLinesRowStartIndex + 1];
                            if (excelRowLine != null)
                            {
                                excelRowLine.GetCellByIndex((int)RequisitionAccountingSplitColumn.LineReferenceNumber).PutValue(objReqItem.lineNumber.ToString());
                                if (objSplit.SplitType == 0)
                                {
                                    if (objReqItem.type.id == (int)ItemExtendedType.Material)
                                    {
                                    excelRowLine.GetCellByIndex(SplitTypeColumnIndex).PutValue("Quantity");
                                    excelRowLine.GetCellByIndex(SplitValueColumnIndex).PutValue(objSplit.quantity.ToString());
                                    }
                                    else
                                    {
                                    excelRowLine.GetCellByIndex(SplitTypeColumnIndex).PutValue("Amount");
                                    excelRowLine.GetCellByIndex(SplitValueColumnIndex).PutValue(objSplit.quantity.ToString());
                                    }
                                }
                                else
                                {
                                    excelRowLine.GetCellByIndex(SplitTypeColumnIndex).PutValue("Percentage");
                                    excelRowLine.GetCellByIndex(SplitValueColumnIndex).PutValue(objSplit.percentage.ToString());
                                }

                                ExportAccountingData(excelRowLine, htAccountingTabAccountingColumnIndex, objSplit);
                                AccountignLinesRowStartIndex++;
                            }
                        }
                    }
                }
            }
        }

        private void ExportAccountingData(ExcelLib.Row excelRowLine, Hashtable htAccountingColumnIndex, ReqAccountingSplit objSplit)
        {
            if (objSplit != null)
            {
                if (objSplit.gLCode != null)
                {
                    if (htAccountingColumnIndex.ContainsKey(objSplit.gLCode.title))
                        excelRowLine[(int)htAccountingColumnIndex[objSplit.gLCode.title]].PutValue(objSplit.gLCode.name);
                }
                //if (objSplit.requester != null)
                //{
                //    if(htAccountingColumnIndex.ContainsKey("Requester"))
                //        excelRowLine[(int)htAccountingColumnIndex["Requester"]].PutValue(objSplit.requester.name);
                //}
                //if (objSplit.period != null)
                //{
                //    if (htAccountingColumnIndex.ContainsKey("Period"))
                //        excelRowLine[(int)htAccountingColumnIndex["Period"]].PutValue(objSplit.period.name);
                //}

                for (int cnt = 0; cnt < objSplit.SplitEntities.Count; cnt++)
                {
                    if (objSplit.SplitEntities[cnt] != null)
                    {
                        int columnIndex = Convert.ToInt32(htAccountingColumnIndex[objSplit.SplitEntities[cnt].title]);
                        if (columnIndex > 0)
                            excelRowLine[columnIndex].PutValue(objSplit.SplitEntities[cnt].entityCode);
                    }

                }

                /*Below code is commented because we are implementing Dynamic spliit entities(REQ-4496)
                if (objSplit.splitEntity1 != null)
                {
                    int columnIndex1 = Convert.ToInt32(htAccountingColumnIndex[objSplit.splitEntity1.title]);
                    if (columnIndex1 > 0)
                        excelRowLine[columnIndex1].PutValue(objSplit.splitEntity1.entityCode);
                }
                if (objSplit.splitEntity2 != null)
                {
                    int columnIndex2 = Convert.ToInt32(htAccountingColumnIndex[objSplit.splitEntity2.title]);
                    if (columnIndex2 > 0)
                        excelRowLine[columnIndex2].PutValue(objSplit.splitEntity2.entityCode);
                }
                if (objSplit.splitEntity3 != null)
                {
                    int columnIndex3 = Convert.ToInt32(htAccountingColumnIndex[objSplit.splitEntity3.title]);
                    if (columnIndex3 > 0)
                        excelRowLine[columnIndex3].PutValue(objSplit.splitEntity3.entityCode);
                }
                if (objSplit.splitEntity4 != null)
                {
                    int columnIndex4 = Convert.ToInt32(htAccountingColumnIndex[objSplit.splitEntity4.title]);
                    if (columnIndex4 > 0)
                        excelRowLine[columnIndex4].PutValue(objSplit.splitEntity4.entityCode);
                }
                if (objSplit.splitEntity5 != null)
                {
                    int columnIndex5 = Convert.ToInt32(htAccountingColumnIndex[objSplit.splitEntity5.title]);
                    if (columnIndex5 > 0)
                        excelRowLine[columnIndex5].PutValue(objSplit.splitEntity5.entityCode);
                }
                if (objSplit.splitEntity6 != null)
                {
                    int columnIndex6 = Convert.ToInt32(htAccountingColumnIndex[objSplit.splitEntity6.title]);
                    if (columnIndex6 > 0)
                        excelRowLine[columnIndex6].PutValue(objSplit.splitEntity6.entityCode);
                }
                if (objSplit.splitEntity7 != null)
                {
                    int columnIndex7 = Convert.ToInt32(htAccountingColumnIndex[objSplit.splitEntity7.title]);
                    if (columnIndex7 > 0)
                        excelRowLine[columnIndex7].PutValue(objSplit.splitEntity7.entityCode);
                }
                if (objSplit.splitEntity8 != null)
                {
                    int columnIndex8 = Convert.ToInt32(htAccountingColumnIndex[objSplit.splitEntity8.title]);
                    if (columnIndex8 > 0)
                        excelRowLine[columnIndex8].PutValue(objSplit.splitEntity8.entityCode);
                }
                if (objSplit.splitEntity9 != null)
                {
                    int columnIndex9 = Convert.ToInt32(htAccountingColumnIndex[objSplit.splitEntity9.title]);
                    if (columnIndex9 > 0)
                        excelRowLine[columnIndex9].PutValue(objSplit.splitEntity9.entityCode);
                }
                if (objSplit.splitEntity10 != null)
                {
                    int columnIndex10 = Convert.ToInt32(htAccountingColumnIndex[objSplit.splitEntity10.title]);
                    if (columnIndex10 > 0)
                        excelRowLine[columnIndex10].PutValue(objSplit.splitEntity10.entityCode);
                }
                if (objSplit.splitEntity11 != null)
                {
                    int columnIndex11 = Convert.ToInt32(htAccountingColumnIndex[objSplit.splitEntity11.title]);
                    if (columnIndex11 > 0)
                        excelRowLine[columnIndex11].PutValue(objSplit.splitEntity11.entityCode);
                }
                if (objSplit.splitEntity12 != null)
                {
                    int columnIndex12 = Convert.ToInt32(htAccountingColumnIndex[objSplit.splitEntity12.title]);
                    if (columnIndex12 > 0)
                        excelRowLine[columnIndex12].PutValue(objSplit.splitEntity12.entityCode);
                }
                if (objSplit.splitEntity13 != null)
                {
                    int columnIndex13 = Convert.ToInt32(htAccountingColumnIndex[objSplit.splitEntity13.title]);
                    if (columnIndex13 > 0)
                        excelRowLine[columnIndex13].PutValue(objSplit.splitEntity13.entityCode);
                }
                if (objSplit.splitEntity14 != null)
                {
                    int columnIndex14 = Convert.ToInt32(htAccountingColumnIndex[objSplit.splitEntity14.title]);
                    if (columnIndex14 > 0)
                        excelRowLine[columnIndex14].PutValue(objSplit.splitEntity14.entityCode);
                }
                if (objSplit.splitEntity15 != null)
                {
                    int columnIndex15 = Convert.ToInt32(htAccountingColumnIndex[objSplit.splitEntity15.title]);
                    if (columnIndex15 > 0)
                        excelRowLine[columnIndex15].PutValue(objSplit.splitEntity15.entityCode);
                }
                if (objSplit.splitEntity16 != null)
                {
                    int columnIndex16 = Convert.ToInt32(htAccountingColumnIndex[objSplit.splitEntity16.title]);
                    if (columnIndex16 > 0)
                        excelRowLine[columnIndex16].PutValue(objSplit.splitEntity16.entityCode);
                }
                if (objSplit.splitEntity17 != null)
                {
                    int columnIndex17 = Convert.ToInt32(htAccountingColumnIndex[objSplit.splitEntity17.title]);
                    if (columnIndex17 > 0)
                        excelRowLine[columnIndex17].PutValue(objSplit.splitEntity17.entityCode);
                }
                if (objSplit.splitEntity18 != null)
                {
                    int columnIndex18 = Convert.ToInt32(htAccountingColumnIndex[objSplit.splitEntity18.title]);
                    if (columnIndex18 > 0)
                        excelRowLine[columnIndex18].PutValue(objSplit.splitEntity18.entityCode);
                }
                if (objSplit.splitEntity19 != null)
                {
                    int columnIndex19 = Convert.ToInt32(htAccountingColumnIndex[objSplit.splitEntity19.title]);
                    if (columnIndex19 > 0)
                        excelRowLine[columnIndex19].PutValue(objSplit.splitEntity19.entityCode);
                }
                if (objSplit.splitEntity20 != null)
                {
                    int columnIndex20 = Convert.ToInt32(htAccountingColumnIndex[objSplit.splitEntity20.title]);
                    if (columnIndex20 > 0)
                        excelRowLine[columnIndex20].PutValue(objSplit.splitEntity20.entityCode);
                }
                */
            }
        }

        private void ExportShipFrom(ExcelLib.Row excelRowLine, NewP2PEntities.RequisitionItem objReqItem)
        {
            if (objReqItem != null)
            {
                var IsShipFromLocationVisible = Convert.ToString(commonManager.GetSettingsValueByKey(P2PDocumentType.None, "IsShipFromLocationVisible", this.UserContext.ContactCode, (int)SubAppCodes.P2P, "", LOBEntityDetailCode));
                if (IsShipFromLocationVisible.ToLower() == "true")
                {
                    if (objReqItem.ShipFromLocation != null)
                    {
                        excelRowLine.GetCellByIndex((int)RequisitionExcelItemColumn.ShipFromLocation).PutValue(objReqItem.ShipFromLocation.name);
                    }
                }
            }
        }

        private void ExporTaxCodesData(NewP2PEntities.RequisitionItem objReqItem, ExcelLib.Row excelRowLine)
        {
            foreach (var objtax in objReqItem.taxItems)
            {
                ExporTaxCodes(objReqItem, false, objtax);
            }

        }

        private void ExporTaxCodes(NewP2PEntities.RequisitionItem objReqItem, Boolean isTaxExempt, Tax objtax)
        {
            ExcelLib.Row excelTaxRowLine = wsTaxCodes.Cells.Rows[RequisitionTaxLinesRowIndex + 1];
            if (excelTaxRowLine != null)
            {
                //Write Data into Rows
                excelTaxRowLine.GetCellByIndex((int)RequisitionLinesRowTaxCodesColumn.LineReferenceNumber).PutValue(objReqItem.lineNumber.ToString());
                excelTaxRowLine.GetCellByIndex((int)RequisitionLinesRowTaxCodesColumn.TaxCode).PutValue(objtax.code);
                excelTaxRowLine.GetCellByIndex((int)RequisitionLinesRowTaxCodesColumn.TaxDescription).PutValue(objtax.description);

                RequisitionTaxLinesRowIndex = RequisitionTaxLinesRowIndex + 1;
            }
        }

        private String GetTrasmissionModeText(POTransmissionMode transmissionMode)
        {
            string transmissionModeString = "";

            switch (transmissionMode)
            {
                case POTransmissionMode.Portal:
                    transmissionModeString = "Portal";
                    break;
                case POTransmissionMode.Email:
                    transmissionModeString = "Direct Email";
                    break;
                case POTransmissionMode.CxmlEDI:
                    transmissionModeString = "EDI/cXML";
                    break;
                case POTransmissionMode.CallAndSubmit:
                    transmissionModeString = "Call & Submit";
                    break;
            }
            return transmissionModeString;
        }

    }
}
