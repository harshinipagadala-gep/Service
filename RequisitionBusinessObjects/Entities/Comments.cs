using System;
using System.Collections.Generic;

namespace GEP.Cumulus.P2P.Req.BusinessObjects.Entities
{ 
    public class CurrencyAutoSuggestFilterModel
    {
        public CurrencyAutoSuggestFilterModel(string searchText, int noOfRecord)
        {
            this.SearchText = searchText;
            this.NoOfRecord = noOfRecord;
        }

        
        public string CultureCode { get; set; }

        
        public string SearchText { get; set; }

        
        public int NoOfRecord { get; set; }
    }


   
    public class CurrencyAutoSuggestDataModel
    {
        
        public int CurrencyId { get; set; }
        
        public string CurrencyCode { get; set; }
        
        public string Symbol { get; set; }
        
        public string CountryAbbrevationCode { get; set; }
        
        public string CurrencyName { get; set; }
        
        public bool IsActive { get; set; }
        
        public bool IsDefault { get; set; }
    }

    public class CurrencyDataModel
    {
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencyName { get; set; }
        public string Symbol { get; set; }
        public string CountryAbbrevationCode { get; set; }
        public int PreferenceOrder { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
        public int TotalRecords { get; set; }
    }

    public class CommentsGroupRequestModel
    {
        public long ObjectID { get; set; }
        public string ObjectType { get; set; }
        public string AccessType { get; set; }
    }

    public class CommentGroup
    {      
        public int CommentGroupID { get; set; }
        public string GroupText { get; set; }
        public bool IsDeleted { get; set; }
        public long CreatedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public string ObjectType { get; set; }
        public long ObjectID { get; set; }
        public List<Comment> Comment { get; set; }
        public int TotalComments { get; set; }
    }

    public class Comment
    {       
        public int CommentID { get; set; }
        public string CommentText { get; set; }
        public int CommentGroupID { get; set; }
        public bool IsDeleted { get; set; }
        public long CreatedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public string CommentType { get; set; }
        public int ParentCommentID { get; set; }
        public string AccessType { get; set; }
        public bool IsSubmitted { get; set; }
        public bool IsDeleteEnable { get; set; }
        public string FirstName { get; set; }
        public string ComapanyName { get; set; }
        public List<CommentAttachment> CommentAttachment { get; set; }
    }

    public class CommentAttachment
    {    
        public string FileName { get; set; }
        public long CreatedBy { get; set; }
        public DateTime DateCreated { get; set; }
        public long FileID { get; set; }
    }

    public class CommentsRequestModel
    {
        public CommentsRequestModel(List<CommentsGroupRequestModel> commentsGroupRequestModel)
        {
            this.CommentsGroupRequestModel = commentsGroupRequestModel;
        }

        public List<CommentsGroupRequestModel> CommentsGroupRequestModel { get; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}