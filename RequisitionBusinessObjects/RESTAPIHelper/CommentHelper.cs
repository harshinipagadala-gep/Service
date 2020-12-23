using Gep.Cumulus.CSM.Entities;
using GEP.Cumulus.P2P.Req.BusinessObjects.Entities;
using GEP.SMART.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper
{
    [ExcludeFromCodeCoverage]
    public class CommentHelper
    {
        private readonly string serviceURL;
        private readonly string serviceURLV2;
        private string appName = Environment.GetEnvironmentVariable("NewRelic.AppName") ?? "Requisition"; 
        private string useCase = "NewRequisitionManager-CommentHelper";
        GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI webAPI;

        public Gep.Cumulus.CSM.Entities.UserExecutionContext UserContext { get; set; }
        Req.BusinessObjects.RESTAPIHelper.RequestHeaders requestHeaders;

        public CommentHelper(Gep.Cumulus.CSM.Entities.UserExecutionContext userExecutionContext, string jwtToken)
        {
            serviceURL = string.Concat(MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL), ServiceURLs.CommentsServiceURL);
            serviceURLV2 = string.Concat(MultiRegionConfig.GetConfig(CloudConfig.APIMBaseURL), "/CommentsControl/api/v2/Comments/");
            this.UserContext = userExecutionContext;
            requestHeaders = new Req.BusinessObjects.RESTAPIHelper.RequestHeaders();
            requestHeaders.Set(UserContext, jwtToken);
            webAPI = new GEP.Cumulus.P2P.Req.BusinessObjects.RESTAPIHelper.WebAPI(requestHeaders, appName, useCase);
        }

        public List<CommentGroup> GetCommentsWithAttachments(List<CommentsGroupRequestModel> commentsGroupRequestModel)
        {       
            
            var commentsRequestModel = new CommentsRequestModel(commentsGroupRequestModel);
            List<CommentGroup> listCommentGroup = null;

            try
            {
                var result = webAPI.ExecutePost(serviceURL + "GetCommentsWithAttachments", commentsRequestModel);
                listCommentGroup = JsonConvert.DeserializeObject<List<CommentGroup>>(result);
            }
            catch (Exception httpException)
            {
                throw httpException;
            }
            return listCommentGroup;
        }

        public CommentsGroup SaveComment(dynamic commentsGroupRequestModel)
        {
            CommentsGroup commentsGroup = null;
            try
            {
                var result = webAPI.ExecutePost(serviceURLV2 + "SaveComment", commentsGroupRequestModel);
                var jsonResult = JsonConvert.DeserializeObject<dynamic>(result);
                commentsGroup = MapSaveComment(jsonResult);
            }
            catch (Exception httpException)
            {
                throw httpException;
            }
            return commentsGroup;
        }

        public bool SaveCommentAttachments(dynamic commentsGroupRequestModel)
        {
            bool result = false;
            try
            {
                var jsonResult = webAPI.ExecutePost(serviceURLV2 + "SaveCommentAttachments", commentsGroupRequestModel);
                result = JsonConvert.DeserializeObject<bool>(jsonResult);
            }
            catch (Exception httpException)
            {
                throw httpException;
            }
            return result;
        }

        public bool FinalizeComments(dynamic finalizeCommentsRequest)
        {
            bool result = false;
            try
            {
                var jsonResult = webAPI.ExecutePost(serviceURL + "FinalizeComments", finalizeCommentsRequest);
                result = JsonConvert.DeserializeObject<bool>(jsonResult);
            }
            catch (Exception httpException)
            {
                throw httpException;
            }
            return result;
        }

        public List<Gep.Cumulus.CSM.Entities.CommentsGroup> Map(List<CommentGroup> input)
        {
            List<Gep.Cumulus.CSM.Entities.CommentsGroup> output = new List<Gep.Cumulus.CSM.Entities.CommentsGroup>();

            if (input != null && input.Count > 0)
            {
                var lstResultComments = new List<Gep.Cumulus.CSM.Entities.Comments>();
                foreach (var comments in input)
                {
                    if (comments.Comment != null && comments.Comment.Count > 0)
                    {
                        foreach (var cmt in comments.Comment)
                        {
                            var commentData = new Gep.Cumulus.CSM.Entities.Comments()
                            {
                                CommentText = cmt.CommentText,
                                CommentID = cmt.CommentID,
                                CreatedBy = cmt.CreatedBy,
                                CommentGroupID = cmt.CommentGroupID,
                                FirstName = cmt.FirstName,
                                AccessType = cmt.AccessType,
                                ComapanyName = cmt.ComapanyName,
                                CommentType = cmt.CommentType,
                                DateCreated = cmt.DateCreated,
                                IsDeleteEnable = cmt.IsDeleteEnable,
                                IsDeleted = cmt.IsDeleted,
                                IsSubmitted = cmt.IsSubmitted,
                                ParentCommentID = cmt.ParentCommentID
                            };
                            if (cmt.CommentAttachment != null)
                            {
                                commentData.CommentAttachment = new List<Gep.Cumulus.CSM.Entities.CommentAttachment>();
                                foreach (var attachment in cmt.CommentAttachment)
                                {
                                    commentData.CommentAttachment.Add(new Gep.Cumulus.CSM.Entities.CommentAttachment()
                                    {
                                        CreatedBy = attachment.CreatedBy,
                                        DateCreated = attachment.DateCreated,
                                        FileID = attachment.FileID,
                                        FileName = attachment.FileName
                                    });
                                }
                            }

                            lstResultComments.Add(commentData);
                        }
                    }

                    output.Add(new Gep.Cumulus.CSM.Entities.CommentsGroup()
                    {
                        CommentGroupID = comments.CommentGroupID,
                        Comment = lstResultComments,
                        CreatedBy = comments.CreatedBy,
                        DateCreated = comments.DateCreated,
                        GroupText = comments.GroupText,
                        IsDeleted = comments.IsDeleted,
                        ObjectID = comments.ObjectID,
                        ObjectType = comments.ObjectType,
                        TotalComments = comments.TotalComments
                    });
                }

            }

            return output;
        }

        public CommentsGroup MapSaveComment(dynamic input)
        {
            CommentsGroup commentsGroup = new CommentsGroup()
            {
                CommentGroupID = input.CommentGroupID,
                Comment = new List<Comments>() {
                    new Comments()
                    {
                        CommentID = input.CommentID
                    }
                }
            };
            return commentsGroup;
        }
    }
}
