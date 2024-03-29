﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Entities.Users;
using System.Collections;
using DotNetNuke.Services.Tokens;
using DotNetNuke.UI.WebControls;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Roles;
using DotNetNuke.Security.Roles.Internal;
using DotNetNuke.Entities.Groups;
using DotNetNuke.Common;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;
namespace DotNetNuke.Modules.Groups.Controls {
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:GroupListControl runat=server></{0}:GroupListControl>")]
    public class GroupListControl : WebControl {
        [DefaultValue(""), PersistenceMode(PersistenceMode.InnerProperty)]
        public String ItemTemplate { get; set; }

        [DefaultValue(""), PersistenceMode(PersistenceMode.InnerProperty)]
        public String HeaderTemplate { get; set; }

        [DefaultValue(""), PersistenceMode(PersistenceMode.InnerProperty)]
        public String FooterTemplate { get; set; }

        [DefaultValue(""), PersistenceMode(PersistenceMode.InnerProperty)]
        public String RowHeaderTemplate { get; set; }

        [DefaultValue(""), PersistenceMode(PersistenceMode.InnerProperty)]
        public String RowFooterTemplate { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public PortalSettings PortalSettings {
            get {
                return PortalController.GetCurrentPortalSettings();
            }
        }
        [DefaultValue(1)]
        public int ItemsPerRow { get; set; }
        [DefaultValue(0)]
        public int RoleGroupId { get; set; }
        [DefaultValue(20)]
        public int PageSize { get; set; }
        [DefaultValue(0)]
        public int CurrentIndex { get; set; }
        [DefaultValue(-1)]
        public int TabId { get; set; }
        public UserInfo currentUser;
        public int GroupViewTabId { get; set; }
        protected override void OnInit(EventArgs e) {
            base.OnInit(e);
            currentUser = UserController.GetCurrentUserInfo();

        }


        protected override void Render(HtmlTextWriter output) {


            RoleController rc = new RoleController();
            IList<RoleInfo> roles = TestableRoleController.Instance.GetRoles(PortalSettings.PortalId, 
                                                (grp) => grp.SecurityMode != SecurityMode.SecurityRole 
                                                            && grp.RoleGroupID == RoleGroupId 
                                                            && grp.Status == RoleStatus.Approved
                                                            && (grp.IsPublic || currentUser.IsInRole(grp.RoleName))
                                        );

            decimal pages = (decimal)roles.Count / (decimal)PageSize;

            output.Write(HeaderTemplate);
            string resxPath = "~/desktopmodules/SocialGroups/App_LocalResources/SharedResources.resx";
            ItemTemplate = ItemTemplate.Replace("{resx:posts}", Localization.GetString("posts", resxPath));
            ItemTemplate = ItemTemplate.Replace("{resx:members}", Localization.GetString("members", resxPath));
            ItemTemplate = ItemTemplate.Replace("{resx:photos}", Localization.GetString("photos", resxPath));
            ItemTemplate = ItemTemplate.Replace("{resx:documents}", Localization.GetString("documents", resxPath));
           
            ItemTemplate = ItemTemplate.Replace("{resx:Join}", Localization.GetString("Join", resxPath));
            ItemTemplate = ItemTemplate.Replace("{resx:Pending}", Localization.GetString("Pending", resxPath));
            ItemTemplate = ItemTemplate.Replace("{resx:LeaveGroup}", Localization.GetString("LeaveGroup", resxPath));

            if (roles.Count == 0) {
                output.Write(String.Format("<div class=\"dnnFormMessage dnnFormInfo\"><span>{0}</span></div>", Localization.GetString("NoGroupsFound", resxPath)));
        
            }
            int rowItem = 0;
            if (!String.IsNullOrEmpty(HttpContext.Current.Request.QueryString["page"]))
            {
                CurrentIndex = Convert.ToInt32(HttpContext.Current.Request.QueryString["page"].ToString());
                CurrentIndex = CurrentIndex - 1;
            }
            int recordStart = (CurrentIndex * PageSize);
            if (CurrentIndex == 0)
            {
                recordStart = 0;
            }
            for (int x = recordStart; x < (recordStart + PageSize); x++)
            {
                if (x > roles.Count-1)
                {
                    break;
                }
                var role = roles[x];
                string rowTemplate = ItemTemplate;
                if (rowItem == 0)
                {
                    output.Write(RowHeaderTemplate);
                }
                var groupParser = new Components.GroupViewParser(PortalSettings, role, currentUser, rowTemplate, GroupViewTabId);
                output.Write(groupParser.ParseView());

                rowItem += 1;
                if (rowItem == ItemsPerRow)
                {
                    output.Write(RowFooterTemplate);
                    rowItem = 0;
                }
            }
           
            if (rowItem > 0) {
                output.Write(RowFooterTemplate);
            }

            output.Write(FooterTemplate);
            int TotalPages = Convert.ToInt32(System.Math.Ceiling(pages));
       
       
            if (TotalPages == 0)
            {
                TotalPages = 1;
            }
            string sUrlFormat = "<a href=\"{0}\" class=\"{1}\">{2}</a>";
            string[] currParams = new string[] { };

            StringBuilder sb = new StringBuilder();
            if (TotalPages > 1)
            {
              
                for (int x = 1; x <= TotalPages; x++)
                {
                    string[] @params = new string[] { };
                    if (currParams.Length > 0 & x > 1)
                    {
                        @params = Utilities.AddParams("page=" + x.ToString(), currParams);
                    } else if (currParams.Length > 0 & x == 1)
                    {
                        @params = currParams;
                    } else if (x > 1)
                    {
                        @params = new string[] { "page=" + x.ToString() };
                    }
                    string sUrl = Utilities.NavigateUrl(TabId, @params);
                    string cssClass = "pagerItem";
                    if (x-1 == CurrentIndex)
                    {
                        cssClass = "pagerItemSelected";
                    }
                    sb.AppendFormat(sUrlFormat, sUrl, cssClass, x.ToString());
                }
                
            }
            output.Write("<div class=\"dnnClear groupPager\">");
            output.Write(sb.ToString());
            output.Write("</div>");

        }



    }

}
