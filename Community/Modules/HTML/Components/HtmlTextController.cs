#region Copyright
// 
// DotNetNukeŽ - http://www.dotnetnuke.com
// Copyright (c) 2002-2012
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using DotNetNuke.Security.Roles.Internal;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Messaging.Data;
using DotNetNuke.Services.Search;
using DotNetNuke.Services.Social.Notifications;
using DotNetNuke.Services.Tokens;
using DotNetNuke.Services.Exceptions;

namespace DotNetNuke.Modules.Html
{
    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Modules.Html
    /// Project:    DotNetNuke
    /// Class:      HtmlTextController
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   The HtmlTextController is the Controller class for managing HtmlText information the HtmlText module
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// </history>
    /// -----------------------------------------------------------------------------
    public class HtmlTextController : ISearchable, IPortable, IUpgradeable
    {
        private const int MAX_DESCRIPTION_LENGTH = 100;
        private const string PortalRootToken = "{{PortalRoot}}";

        #region Private Methods

        private static void AddHtmlNotification(string subject, string body, UserInfo user)
        {
            var notificationType = NotificationsController.Instance.GetNotificationType("HtmlNotification");
            var portalSettings = PortalController.GetCurrentPortalSettings();
            var sender = UserController.GetUserById(portalSettings.PortalId, portalSettings.AdministratorId);

            var notification = new Notification {NotificationTypeID = notificationType.NotificationTypeId, Subject = subject, Body = body, IncludeDismissAction = true, SenderUserID = sender.UserID};
            NotificationsController.Instance.SendNotification(notification, portalSettings.PortalId, null, new List<UserInfo> { user });
        }

        private void ClearModuleSettings(ModuleInfo objModule)
        {
            var moduleController = new ModuleController();
            if (objModule.ModuleDefinition.FriendlyName == "Text/HTML")
            {
                moduleController.DeleteModuleSetting(objModule.ModuleID, "WorkFlowID");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   CreateUserNotifications creates HtmlTextUser records and optionally sends email notifications to participants in a Workflow
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="objHtmlText">An HtmlTextInfo object</param>
        /// <history>
        /// </history>
        /// -----------------------------------------------------------------------------
        private void CreateUserNotifications(HtmlTextInfo objHtmlText)
        {
            var _htmlTextUserController = new HtmlTextUserController();
            HtmlTextUserInfo _htmlTextUser = null;
            UserInfo _user = null;

            // clean up old user notification records
            _htmlTextUserController.DeleteHtmlTextUsers();

            // ensure we have latest htmltext object loaded
            objHtmlText = GetHtmlText(objHtmlText.ModuleID, objHtmlText.ItemID);

            // build collection of users to notify
            var objWorkflow = new WorkflowStateController();
            var arrUsers = new ArrayList();

            // if not published
            if (objHtmlText.IsPublished == false)
            {
                arrUsers.Add(objHtmlText.CreatedByUserID); // include content owner 
            }

            // if not draft and not published
            if (objHtmlText.StateID != objWorkflow.GetFirstWorkflowStateID(objHtmlText.WorkflowID) && objHtmlText.IsPublished == false)
            {
                // get users from permissions for state
                var objRoles = new RoleController();
                foreach (WorkflowStatePermissionInfo permission in
                    WorkflowStatePermissionController.GetWorkflowStatePermissions(objHtmlText.StateID))
                {
                    if (permission.AllowAccess)
                    {
                        if (Null.IsNull(permission.UserID))
                        {
                            int roleId = permission.RoleID;
                            RoleInfo objRole = TestableRoleController.Instance.GetRole(objHtmlText.PortalID, r => r.RoleID == roleId);
                            if ((objRole != null))
                            {
                                foreach (UserRoleInfo objUserRole in objRoles.GetUserRoles(objHtmlText.PortalID, null, objRole.RoleName))
                                {
                                    if (!arrUsers.Contains(objUserRole.UserID))
                                    {
                                        arrUsers.Add(objUserRole.UserID);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!arrUsers.Contains(permission.UserID))
                            {
                                arrUsers.Add(permission.UserID);
                            }
                        }
                    }
                }
            }

            // process notifications
            if (arrUsers.Count > 0 || (objHtmlText.IsPublished && objHtmlText.Notify))
            {
                // get tabid from module 
                var objModules = new ModuleController();
                ModuleInfo objModule = objModules.GetModule(objHtmlText.ModuleID);

                PortalSettings objPortalSettings = PortalController.GetCurrentPortalSettings();
                if (objPortalSettings != null)
                {
                    string strResourceFile = string.Format("{0}/DesktopModules/{1}/{2}/{3}",
                                                           Globals.ApplicationPath,
                                                           objModule.DesktopModule.FolderName,
                                                           Localization.LocalResourceDirectory,
                                                           Localization.LocalSharedResourceFile);
                    string strSubject = Localization.GetString("NotificationSubject", strResourceFile);
                    string strBody = Localization.GetString("NotificationBody", strResourceFile);
                    strBody = strBody.Replace("[URL]", Globals.NavigateURL(objModule.TabID));
                    strBody = strBody.Replace("[STATE]", objHtmlText.StateName);

                    // process user notification collection

                    foreach (int intUserID in arrUsers)
                    {
                        // create user notification record 
                        _htmlTextUser = new HtmlTextUserInfo();
                        _htmlTextUser.ItemID = objHtmlText.ItemID;
                        _htmlTextUser.StateID = objHtmlText.StateID;
                        _htmlTextUser.ModuleID = objHtmlText.ModuleID;
                        _htmlTextUser.TabID = objModule.TabID;
                        _htmlTextUser.UserID = intUserID;
                        _htmlTextUserController.AddHtmlTextUser(_htmlTextUser);

                        // send an email notification to a user if the state indicates to do so
                        if (objHtmlText.Notify)
                        {
                            _user = UserController.GetUserById(objHtmlText.PortalID, intUserID);
                            if (_user != null)
                            {
                                AddHtmlNotification(strSubject, strBody, _user);
                            }
                        }
                    }

                    // if published and the published state specifies to notify members of the workflow
                    if (objHtmlText.IsPublished && objHtmlText.Notify)
                    {
                        // send email notification to the author
                        _user = UserController.GetUserById(objHtmlText.PortalID, objHtmlText.CreatedByUserID);
                        if (_user != null)
                        {
                            try
                            {
                                Services.Mail.Mail.SendEmail(objPortalSettings.Email, objPortalSettings.Email, strSubject, strBody);
                            }
                            catch (Exception exc)
                            {
                                Exceptions.LogException(exc);
                            }
                        }
                    }
                }
            }
        }

        private string DeTokeniseLinks(string content, int portalId)
        {

            var portalController = new PortalController();
            var portal = portalController.GetPortal(portalId);
            var portalRoot = UrlUtils.Combine(Globals.ApplicationPath, portal.HomeDirectory);
            if (!portalRoot.StartsWith("/"))
            {
                portalRoot = "/" + portalRoot;
            }
            content = content.Replace(PortalRootToken, portalRoot);

            return content;
        }

        private string TokeniseLinks(string content, int portalId)
        {
            //Replace any relative portal root reference by a token "{{PortalRoot}}"
            var portalController = new PortalController();
            var portal = portalController.GetPortal(portalId);
            var portalRoot = UrlUtils.Combine(Globals.ApplicationPath, portal.HomeDirectory);
            if (!portalRoot.StartsWith("/"))
            {
                portalRoot = "/" + portalRoot;
            }
            Regex exp = new Regex(portalRoot, RegexOptions.IgnoreCase);
            content = exp.Replace(content, PortalRootToken);

            return content;
        }

        #endregion

        #region Public Methods
        
        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   DeleteHtmlText deletes an HtmlTextInfo object for the Module and Item
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "ModuleID">The ID of the Module</param>
        /// <param name = "ItemID">The ID of the Item</param>
        /// <history>
        /// </history>
        /// -----------------------------------------------------------------------------
        public void DeleteHtmlText(int ModuleID, int ItemID)
        {
            DataProvider.Instance().DeleteHtmlText(ModuleID, ItemID);

            // refresh output cache
            ModuleController.SynchronizeModule(ModuleID);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   FormatHtmlText formats HtmlText content for display in the browser
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="ModuleId">The Module ID</param>
        /// <param name = "Content">The HtmlText Content</param>
        /// <param name = "Settings">A Hashtable of Module Settings</param>
        /// <history>
        /// </history>
        /// -----------------------------------------------------------------------------
        public static string FormatHtmlText(int ModuleId, string Content, Hashtable Settings)
        {
            PortalSettings objPortalSettings = PortalController.GetCurrentPortalSettings();

            // token replace
            bool blnReplaceTokens = false;
            if (!string.IsNullOrEmpty(Convert.ToString(Settings["HtmlText_ReplaceTokens"])))
            {
                blnReplaceTokens = Convert.ToBoolean(Settings["HtmlText_ReplaceTokens"]);
            }
            if (blnReplaceTokens)
            {
                var tr = new TokenReplace();
                tr.AccessingUser = UserController.GetCurrentUserInfo();
                tr.DebugMessages = objPortalSettings.UserMode != PortalSettings.Mode.View;
                tr.ModuleId = ModuleId;
                Content = tr.ReplaceEnvironmentTokens(Content);
            }

            // Html decode content
            Content = HttpUtility.HtmlDecode(Content);

            // manage relative paths
            Content = ManageRelativePaths(Content, objPortalSettings.HomeDirectory, "src", objPortalSettings.PortalId);
            Content = ManageRelativePaths(Content, objPortalSettings.HomeDirectory, "background", objPortalSettings.PortalId);

            return Content;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   GetAllHtmlText gets a collection of HtmlTextInfo objects for the Module and Workflow
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "ModuleID">The ID of the Module</param>
        /// <history>
        /// </history>
        /// -----------------------------------------------------------------------------
        public List<HtmlTextInfo> GetAllHtmlText(int ModuleID)
        {
            return CBO.FillCollection<HtmlTextInfo>(DataProvider.Instance().GetAllHtmlText(ModuleID));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   GetHtmlText gets the HtmlTextInfo object for the Module, Item, and Workflow
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "ModuleID">The ID of the Module</param>
        /// <param name = "ItemID">The ID of the Item</param>
        /// <history>
        /// </history>
        /// -----------------------------------------------------------------------------
        public HtmlTextInfo GetHtmlText(int ModuleID, int ItemID)
        {
            return (HtmlTextInfo) (CBO.FillObject(DataProvider.Instance().GetHtmlText(ModuleID, ItemID), typeof (HtmlTextInfo)));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   GetTopHtmlText gets the most recent HtmlTextInfo object for the Module, Workflow, and State
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "ModuleID">The ID of the Module</param>
        /// <param name = "IsPublished">Whether the content has been published or not</param>
        /// <param name="WorkflowID">The Workflow ID</param>
        /// <history>
        /// </history>
        /// -----------------------------------------------------------------------------
        public HtmlTextInfo GetTopHtmlText(int ModuleID, bool IsPublished, int WorkflowID)
        {
            var objHtmlText = (HtmlTextInfo) (CBO.FillObject(DataProvider.Instance().GetTopHtmlText(ModuleID, IsPublished), typeof (HtmlTextInfo)));
            if (objHtmlText != null)
            {
                // check if workflow has changed
                if (IsPublished == false && objHtmlText.WorkflowID != WorkflowID)
                {
                    // get proper state for workflow
                    var objWorkflow = new WorkflowStateController();
                    objHtmlText.WorkflowID = WorkflowID;
                    objHtmlText.WorkflowName = "[REPAIR_WORKFLOW]";
                    if (objHtmlText.IsPublished)
                    {
                        objHtmlText.StateID = objWorkflow.GetLastWorkflowStateID(WorkflowID);
                    }
                    else
                    {
                        objHtmlText.StateID = objWorkflow.GetFirstWorkflowStateID(WorkflowID);
                    }
                    // update object
                    UpdateHtmlText(objHtmlText, GetMaximumVersionHistory(objHtmlText.PortalID));
                    // get object again
                    objHtmlText = (HtmlTextInfo) (CBO.FillObject(DataProvider.Instance().GetTopHtmlText(ModuleID, IsPublished), typeof (HtmlTextInfo)));
                }
            }
            return objHtmlText;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   GetWorkFlow retrieves the currently active Workflow for the Portal
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "ModuleId">The ID of the Module</param>
        /// <param name="TabId">The Tab ID</param>
        /// <param name = "PortalId">The ID of the Portal</param>
        /// <history>
        /// </history>
        /// -----------------------------------------------------------------------------
        public KeyValuePair<string, int> GetWorkflow(int ModuleId, int TabId, int PortalId)
        {
            int workFlowId = Null.NullInteger;
            string workFlowType = Null.NullString;

            // get from module settings
            var moduleController = new ModuleController();
            Hashtable settings = moduleController.GetModuleSettings(ModuleId);
            if (settings["WorkflowID"] != null)
            {
                workFlowId = Convert.ToInt32(settings["WorkflowID"]);
                workFlowType = "Module";
            }
            if (workFlowId == Null.NullInteger)
            {
                // if undefined at module level, get from tab settings
                settings = new TabController().GetTabSettings(TabId);
                if (settings["WorkflowID"] != null)
                {
                    workFlowId = Convert.ToInt32(settings["WorkflowID"]);
                    workFlowType = "Page";
                }
            }

            if (workFlowId == Null.NullInteger)
            {
                // if undefined at tab level, get from portal settings
                workFlowId = int.Parse(PortalController.GetPortalSetting("WorkflowID", PortalId, "-1"));
                workFlowType = "Site";
            }

            // if undefined at portal level, set portal default
            if (workFlowId == Null.NullInteger)
            {
                var objWorkflow = new WorkflowStateController();
                ArrayList arrWorkflows = objWorkflow.GetWorkflows(PortalId);
                foreach (WorkflowStateInfo objState in arrWorkflows)
                {
                    // use direct publish as default
                    if (Null.IsNull(objState.PortalID) && objState.WorkflowName == "Direct Publish")
                    {
                        workFlowId = objState.WorkflowID;
                        workFlowType = "Module";
                    }
                }
            }

            return new KeyValuePair<string, int>(workFlowType, workFlowId);
        }

        public static string ManageRelativePaths(string strHTML, string strUploadDirectory, string strToken, int intPortalID)
        {
            int P = 0;
            int R = 0;
            int S = 0;
            int tLen = 0;
            string strURL = null;
            var sbBuff = new StringBuilder("");

            if (!string.IsNullOrEmpty(strHTML))
            {
                tLen = strToken.Length + 2;
                string uploadDirectory = strUploadDirectory.ToLower();

                //find position of first occurrance:
                P = strHTML.IndexOf(strToken + "=\"", StringComparison.InvariantCultureIgnoreCase);
                while (P != -1)
                {
                    sbBuff.Append(strHTML.Substring(S, P - S + tLen));
                    //keep charactes left of URL
                    S = P + tLen;
                    //save startpos of URL
                    R = strHTML.IndexOf("\"", S);
                    //end of URL
                    if (R >= 0)
                    {
                        strURL = strHTML.Substring(S, R - S).ToLower();
                    }
                    else
                    {
                        strURL = strHTML.Substring(S).ToLower();
                    }

                    // if we are linking internally
                    if (strURL.Contains("://") == false)
                    {
                        // remove the leading portion of the path if the URL contains the upload directory structure
                        string strDirectory = uploadDirectory;
                        if (!strDirectory.EndsWith("/"))
                        {
                            strDirectory += "/";
                        }
                        if (strURL.IndexOf(strDirectory) != -1)
                        {
                            S = S + strURL.IndexOf(strDirectory) + strDirectory.Length;
                            strURL = strURL.Substring(strURL.IndexOf(strDirectory) + strDirectory.Length);
                        }
                        // add upload directory
                        if (strURL.StartsWith("/") == false)
                        {
                            sbBuff.Append(uploadDirectory);
                        }
                    }
                    //find position of next occurrance
                    P = strHTML.IndexOf(strToken + "=\"", S + strURL.Length + 2, StringComparison.InvariantCultureIgnoreCase);
                }

                if (S > -1)
                {
                    sbBuff.Append(strHTML.Substring(S));
                }
                //append characters of last URL and behind
            }

            return sbBuff.ToString();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   UpdateHtmlText creates a new HtmlTextInfo object or updates an existing HtmlTextInfo object
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "htmlContent">An HtmlTextInfo object</param>
        /// <param name = "MaximumVersionHistory">The maximum number of versions to retain</param>
        /// <history>
        /// </history>
        /// -----------------------------------------------------------------------------
        public void UpdateHtmlText(HtmlTextInfo htmlContent, int MaximumVersionHistory)
        {
            var _workflowStateController = new WorkflowStateController();
            bool blnCreateNewVersion = false;

            // determine if we are creating a new version of content or updating an existing version
            if (htmlContent.ItemID != -1)
            {
                if (htmlContent.WorkflowName != "[REPAIR_WORKFLOW]")
                {
                    HtmlTextInfo objContent = GetTopHtmlText(htmlContent.ModuleID, false, htmlContent.WorkflowID);
                    if (objContent != null)
                    {
                        if (objContent.StateID == _workflowStateController.GetLastWorkflowStateID(htmlContent.WorkflowID))
                        {
                            blnCreateNewVersion = true;
                        }
                    }
                }
            }
            else
            {
                blnCreateNewVersion = true;
            }

            // determine if content is published
            if (htmlContent.StateID == _workflowStateController.GetLastWorkflowStateID(htmlContent.WorkflowID))
            {
                htmlContent.IsPublished = true;
            }
            else
            {
                htmlContent.IsPublished = false;
            }

            if (blnCreateNewVersion)
            {
                // add content
                htmlContent.ItemID = DataProvider.Instance().AddHtmlText(htmlContent.ModuleID,
                                                                         htmlContent.Content,
																		 htmlContent.Summary,
                                                                         htmlContent.StateID,
                                                                         htmlContent.IsPublished,
                                                                         UserController.GetCurrentUserInfo().UserID,
                                                                         MaximumVersionHistory);
            }
            else
            {
                // update content
				DataProvider.Instance().UpdateHtmlText(htmlContent.ItemID, htmlContent.Content, htmlContent.Summary, htmlContent.StateID, htmlContent.IsPublished, UserController.GetCurrentUserInfo().UserID);
            }

            // add log history
            var logInfo = new HtmlTextLogInfo();
            logInfo.ItemID = htmlContent.ItemID;
            logInfo.StateID = htmlContent.StateID;
            logInfo.Approved = htmlContent.Approved;
            logInfo.Comment = htmlContent.Comment;
            var objLogs = new HtmlTextLogController();
            objLogs.AddHtmlTextLog(logInfo);

            // create user notifications
            CreateUserNotifications(htmlContent);

            // refresh output cache
            ModuleController.SynchronizeModule(htmlContent.ModuleID);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   UpdateWorkFlow updates the currently active Workflow
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="WorkFlowType">The type of workflow (Module | Page | Site)</param>
        /// <param name = "WorkflowID">The ID of the Workflow</param>
        /// <param name="ObjectID">The ID of the object to apply the update to (depends on WorkFlowType)</param>
        /// <param name="ReplaceExistingSettings">Should existing settings be overwritten?</param>
        /// <history>
        /// </history>
        /// -----------------------------------------------------------------------------
        public void UpdateWorkflow(int ObjectID, string WorkFlowType, int WorkflowID, bool ReplaceExistingSettings)
        {
            var tabController = new TabController();
            var moduleController = new ModuleController();

            switch (WorkFlowType)
            {
                case "Module":
                    moduleController.UpdateModuleSetting(ObjectID, "WorkflowID", WorkflowID.ToString());
                    break;
                case "Page":
                    tabController.UpdateTabSetting(ObjectID, "WorkflowID", WorkflowID.ToString());
                    if (ReplaceExistingSettings)
                    {
                        //Get All Modules on the current Tab
                        foreach (var kvp in moduleController.GetTabModules(ObjectID))
                        {
                            ClearModuleSettings(kvp.Value);
                        }
                    }
                    break;
                case "Site":
                    PortalController.UpdatePortalSetting(ObjectID, "WorkflowID", WorkflowID.ToString());
                    if (ReplaceExistingSettings)
                    {
                        //Get All Tabs aon the Site
                        foreach (var kvp in tabController.GetTabsByPortal(ObjectID))
                        {
                            tabController.DeleteTabSetting(kvp.Value.TabID, "WorkFlowID");
                        }
                        //Get All Modules in the current Site
                        foreach (ModuleInfo objModule in moduleController.GetModules(ObjectID))
                        {
                            ClearModuleSettings(objModule);
                        }
                    }
                    break;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   GetMaximumVersionHistory retrieves the maximum number of versions to store for a module
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "PortalID">The ID of the Portal</param>
        /// <history>
        /// </history>
        /// -----------------------------------------------------------------------------
        public int GetMaximumVersionHistory(int PortalID)
        {
            int intMaximumVersionHistory = -1;

            // get from portal settings
            intMaximumVersionHistory = int.Parse(PortalController.GetPortalSetting("MaximumVersionHistory", PortalID, "-1"));

            // if undefined at portal level, set portal default
            if (intMaximumVersionHistory == -1)
            {
                intMaximumVersionHistory = 5;
                // default
                PortalController.UpdatePortalSetting(PortalID, "MaximumVersionHistory", intMaximumVersionHistory.ToString());
            }

            return intMaximumVersionHistory;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   UpdateWorkFlowID updates the currently active WorkflowID for the Portal
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "PortalID">The ID of the Portal</param>
        /// <param name = "MaximumVersionHistory">The MaximumVersionHistory</param>
        /// <history>
        /// </history>
        /// -----------------------------------------------------------------------------
        public void UpdateMaximumVersionHistory(int PortalID, int MaximumVersionHistory)
        {
            // data integrity check
            if (MaximumVersionHistory < 0)
            {
                MaximumVersionHistory = 5;
                // default
            }

            // save portal setting
            PortalSettings objPortalSettings = PortalController.GetCurrentPortalSettings();
            if (PortalSecurity.IsInRole(objPortalSettings.AdministratorRoleName))
            {
                PortalController.UpdatePortalSetting(PortalID, "MaximumVersionHistory", MaximumVersionHistory.ToString());
            }
        }

        #endregion

        #region Optional Interfaces

        #region IPortable Members

         /// -----------------------------------------------------------------------------
        /// <summary>
        ///   ExportModule implements the IPortable ExportModule Interface
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "moduleId">The Id of the module to be exported</param>
        /// <history>
        /// </history>
        /// -----------------------------------------------------------------------------
        public string ExportModule(int moduleId)
        {
            string xml = "";

            var moduleController = new ModuleController();
            ModuleInfo module = moduleController.GetModule(moduleId);
            int workflowID = GetWorkflow(moduleId, module.TabID, module.PortalID).Value;

            HtmlTextInfo content = GetTopHtmlText(moduleId, true, workflowID);
            if ((content != null))
            {
                xml += "<htmltext>";
                xml += "<content>" + XmlUtils.XMLEncode(TokeniseLinks(content.Content, module.PortalID)) + "</content>";
                xml += "</htmltext>";
            }

            return xml;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   ImportModule implements the IPortable ImportModule Interface
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "ModuleID">The ID of the Module being imported</param>
        /// <param name = "Content">The Content being imported</param>
        /// <param name = "Version">The Version of the Module Content being imported</param>
        /// <param name = "UserId">The UserID of the User importing the Content</param>
        /// <history>
        /// </history>
        /// -----------------------------------------------------------------------------
        public void ImportModule(int ModuleID, string Content, string Version, int UserId)
        {
            var moduleController = new ModuleController();
            ModuleInfo module = moduleController.GetModule(ModuleID);
            var workflowStateController = new WorkflowStateController();
            int workflowID = GetWorkflow(ModuleID, module.TabID, module.PortalID).Value;
            XmlNode xml = Globals.GetContent(Content, "htmltext");

            var htmlContent = new HtmlTextInfo();
            htmlContent.ModuleID = ModuleID;
            // convert Version to System.Version
            var objVersion = new Version(Version);
            if (objVersion >= new Version(5, 1, 0))
            {
                // current module content
                htmlContent.Content = DeTokeniseLinks(xml.SelectSingleNode("content").InnerText, module.PortalID);
            }
            else
            {
                // legacy module content
                htmlContent.Content = DeTokeniseLinks(xml.SelectSingleNode("desktophtml").InnerText, module.PortalID);
            }
            htmlContent.WorkflowID = workflowID;
            htmlContent.StateID = workflowStateController.GetFirstWorkflowStateID(workflowID);
            // import
            UpdateHtmlText(htmlContent, GetMaximumVersionHistory(module.PortalID));
        }

        #endregion

        #region ISearchable Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   GetSearchItems implements the ISearchable Interface
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "ModInfo">The ModuleInfo for the module to be Indexed</param>
        /// <history>
        /// </history>
        /// -----------------------------------------------------------------------------
        public SearchItemInfoCollection GetSearchItems(ModuleInfo ModInfo)
        {
            var objWorkflow = new WorkflowStateController();
            int WorkflowID = GetWorkflow(ModInfo.ModuleID, ModInfo.TabID, ModInfo.PortalID).Value;
            var SearchItemCollection = new SearchItemInfoCollection();
            HtmlTextInfo objContent = GetTopHtmlText(ModInfo.ModuleID, true, WorkflowID);

            if (objContent != null)
            {
                //content is encoded in the Database so Decode before Indexing
                string strContent = HttpUtility.HtmlDecode(objContent.Content);

                //Get the description string
                string strDescription = HtmlUtils.Shorten(HtmlUtils.Clean(strContent, false), MAX_DESCRIPTION_LENGTH, "...");

                var SearchItem = new SearchItemInfo(ModInfo.ModuleTitle,
                                                    strDescription,
                                                    objContent.LastModifiedByUserID,
                                                    objContent.LastModifiedOnDate,
                                                    ModInfo.ModuleID,
                                                    "",
                                                    strContent,
                                                    "",
                                                    Null.NullInteger);
                SearchItemCollection.Add(SearchItem);
            }

            return SearchItemCollection;
        }

        #endregion

        #region IUpgradeable Members

        public string UpgradeModule(string Version)
        {
            switch (Version)
            {
                case "05.01.02":
                    //remove the Code SubDirectory
                    Config.RemoveCodeSubDirectory("HTML");

                    //Once the web.config entry is done we can safely remove the HTML folder
                    var arrPaths = new string[1];
                    arrPaths[0] = "App_Code\\HTML\\";
                    FileSystemUtils.DeleteFiles(arrPaths);
                    break;
                case "06.00.00":
                    DesktopModuleInfo desktopModule = DesktopModuleController.GetDesktopModuleByModuleName("DNN_HTML", Null.NullInteger);
                    desktopModule.Category = "Common";
                    DesktopModuleController.SaveDesktopModule(desktopModule, false, false);
                    break;

                case "06.02.00":
                    AddNotificationTypes();
                    break;
            }

           return string.Empty;
        }

        private void AddNotificationTypes()
        {
            var type = new NotificationType { Name = "HtmlNotification", Description = "Html Module Notification" };
            if (NotificationsController.Instance.GetNotificationType(type.Name) == null)
            {
                NotificationsController.Instance.CreateNotificationType(type);
            }
        }

        #endregion

        #endregion
    }
}