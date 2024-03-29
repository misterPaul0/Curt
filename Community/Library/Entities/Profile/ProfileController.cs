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
#region Usings

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Profile;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Log.EventLog;

#endregion

namespace DotNetNuke.Entities.Profile
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Profile
    /// Class:      ProfileController
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ProfileController class provides Business Layer methods for profiles and
    /// for profile property Definitions
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class ProfileController
    {
        #region Private Members

        private static readonly DataProvider _dataProvider = DataProvider.Instance();
        private static readonly ProfileProvider _profileProvider = ProfileProvider.Instance();
        private static int _orderCounter;

        #endregion

        #region Private Methdods

        private static void AddDefaultDefinition(int portalId, string category, string name, string strType, int length, UserVisibilityMode defaultVisibility, Dictionary<string, ListEntryInfo> types)
        {
            _orderCounter += 2;
            AddDefaultDefinition(portalId, category, name, strType, length, _orderCounter, defaultVisibility, types);
        }

        internal static void AddDefaultDefinition(int portalId, string category, string name, string type, int length, int viewOrder, UserVisibilityMode defaultVisibility,
                                                  Dictionary<string, ListEntryInfo> types)
        {
            ListEntryInfo typeInfo = types["DataType:" + type] ?? types["DataType:Unknown"];
            var propertyDefinition = new ProfilePropertyDefinition(portalId)
                                         {
                                             DataType = typeInfo.EntryID,
                                             DefaultValue = "",
                                             ModuleDefId = Null.NullInteger,
                                             PropertyCategory = category,
                                             PropertyName = name,
                                             Required = false,
                                             ViewOrder = viewOrder,
                                             Visible = true,
                                             Length = length,
                                             DefaultVisibility = defaultVisibility
                                         };
            AddPropertyDefinition(propertyDefinition);
        }

        private static ProfilePropertyDefinition FillPropertyDefinitionInfo(IDataReader dr)
        {
            ProfilePropertyDefinition definition = null;
            try
            {
                definition = FillPropertyDefinitionInfo(dr, true);
            }
			catch (Exception ex)
			{
				DnnLog.Error(ex);
			}
            finally
            {
                CBO.CloseDataReader(dr, true);
            }
            return definition;
        }

        private static ProfilePropertyDefinition FillPropertyDefinitionInfo(IDataReader dr, bool checkForOpenDataReader)
        {
            ProfilePropertyDefinition definition = null;

            //read datareader
            bool canContinue = true;
            if (checkForOpenDataReader)
            {
                canContinue = false;
                if (dr.Read())
                {
                    canContinue = true;
                }
            }
            if (canContinue)
            {
                int portalid = 0;
                portalid = Convert.ToInt32(Null.SetNull(dr["PortalId"], portalid));
                definition = new ProfilePropertyDefinition(portalid);
                definition.PropertyDefinitionId = Convert.ToInt32(Null.SetNull(dr["PropertyDefinitionId"], definition.PropertyDefinitionId));
                definition.ModuleDefId = Convert.ToInt32(Null.SetNull(dr["ModuleDefId"], definition.ModuleDefId));
                definition.DataType = Convert.ToInt32(Null.SetNull(dr["DataType"], definition.DataType));
                definition.DefaultValue = Convert.ToString(Null.SetNull(dr["DefaultValue"], definition.DefaultValue));
                definition.PropertyCategory = Convert.ToString(Null.SetNull(dr["PropertyCategory"], definition.PropertyCategory));
                definition.PropertyName = Convert.ToString(Null.SetNull(dr["PropertyName"], definition.PropertyName));
                definition.Length = Convert.ToInt32(Null.SetNull(dr["Length"], definition.Length));
                if (dr.GetSchemaTable().Select("ColumnName = 'ReadOnly'").Length > 0)
                {
                    definition.ReadOnly = Convert.ToBoolean(Null.SetNull(dr["ReadOnly"], definition.ReadOnly));
                }
                definition.Required = Convert.ToBoolean(Null.SetNull(dr["Required"], definition.Required));
                definition.ValidationExpression = Convert.ToString(Null.SetNull(dr["ValidationExpression"], definition.ValidationExpression));
                definition.ViewOrder = Convert.ToInt32(Null.SetNull(dr["ViewOrder"], definition.ViewOrder));
                definition.Visible = Convert.ToBoolean(Null.SetNull(dr["Visible"], definition.Visible));
                definition.DefaultVisibility = (UserVisibilityMode) Convert.ToInt32(Null.SetNull(dr["DefaultVisibility"], definition.DefaultVisibility));
                definition.ProfileVisibility = new ProfileVisibility
                                                   {
                                                       VisibilityMode = definition.DefaultVisibility
                                                   };
                definition.Deleted = Convert.ToBoolean(Null.SetNull(dr["Deleted"], definition.Deleted));
            }
            return definition;
        }

        private static List<ProfilePropertyDefinition> FillPropertyDefinitionInfoCollection(IDataReader dr)
        {
            var arr = new List<ProfilePropertyDefinition>();
            try
            {
                while (dr.Read())
                {
                    //fill business object
                    ProfilePropertyDefinition definition = FillPropertyDefinitionInfo(dr, false);
                    //add to collection
                    arr.Add(definition);
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
            finally
            {
				//close datareader
                CBO.CloseDataReader(dr, true);
            }
            return arr;
        }

        private static int GetEffectivePortalId(int portalId)
        {
            return PortalController.GetEffectivePortalId(portalId);
        }

        private static IEnumerable<ProfilePropertyDefinition> GetPropertyDefinitions(int portalId)
        {
			//Get the Cache Key
            string key = string.Format(DataCache.ProfileDefinitionsCacheKey, portalId);

            //Try fetching the List from the Cache
            var definitions = (List<ProfilePropertyDefinition>) DataCache.GetCache(key);
            if (definitions == null)
            {
                //definitions caching settings
                Int32 timeOut = DataCache.ProfileDefinitionsCacheTimeOut*Convert.ToInt32(Host.Host.PerformanceSetting);

                //Get the List from the database
                definitions = FillPropertyDefinitionInfoCollection(_dataProvider.GetPropertyDefinitionsByPortal(portalId));

                //Cache the List
                if (timeOut > 0)
                {
                    DataCache.SetCache(key, definitions, TimeSpan.FromMinutes(timeOut));
                }
            }
            return definitions;
        }

        #endregion

        #region Public Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Adds the default property definitions for a portal
        /// </summary>
        /// <param name="portalId">Id of the Portal</param>
        /// -----------------------------------------------------------------------------
        public static void AddDefaultDefinitions(int portalId)
        {
            portalId = GetEffectivePortalId(portalId); 
            
            _orderCounter = 1;
            var listController = new ListController();
            Dictionary<string, ListEntryInfo> dataTypes = listController.GetListEntryInfoDictionary("DataType");

            AddDefaultDefinition(portalId, "Name", "Prefix", "Text", 50, UserVisibilityMode.AllUsers, dataTypes);
            AddDefaultDefinition(portalId, "Name", "FirstName", "Text", 50, UserVisibilityMode.AllUsers, dataTypes);
            AddDefaultDefinition(portalId, "Name", "MiddleName", "Text", 50, UserVisibilityMode.AllUsers, dataTypes);
            AddDefaultDefinition(portalId, "Name", "LastName", "Text", 50, UserVisibilityMode.AllUsers, dataTypes);
            AddDefaultDefinition(portalId, "Name", "Suffix", "Text", 50, UserVisibilityMode.AllUsers, dataTypes);
            AddDefaultDefinition(portalId, "Address", "Unit", "Text", 50, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(portalId, "Address", "Street", "Text", 50, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(portalId, "Address", "City", "Text", 50, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(portalId, "Address", "Region", "Region", 0, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(portalId, "Address", "Country", "Country", 0, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(portalId, "Address", "PostalCode", "Text", 50, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(portalId, "Contact Info", "Telephone", "Text", 50, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(portalId, "Contact Info", "Cell", "Text", 50, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(portalId, "Contact Info", "Fax", "Text", 50, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(portalId, "Contact Info", "Website", "Text", 50, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(portalId, "Contact Info", "IM", "Text", 50, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(portalId, "Preferences", "Biography", "RichText", 0, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(portalId, "Preferences", "TimeZone", "TimeZone", 0, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(portalId, "Preferences", "PreferredTimeZone", "TimeZoneInfo", 0, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(portalId, "Preferences", "PreferredLocale", "Locale", 0, UserVisibilityMode.AdminOnly, dataTypes);
            AddDefaultDefinition(portalId, "Preferences", "Photo", "Image", 0, UserVisibilityMode.AllUsers, dataTypes);

            //6.0 requires the old TimeZone property to be marked as Deleted
            ProfilePropertyDefinition pdf = GetPropertyDefinitionByName(portalId, "TimeZone");
            if(pdf != null)
            {
                DeletePropertyDefinition(pdf);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Adds a Property Defintion to the Data Store
        /// </summary>
        /// <param name="definition">An ProfilePropertyDefinition object</param>
        /// <returns>The Id of the definition (or if negative the errorcode of the error)</returns>
        /// -----------------------------------------------------------------------------
        public static int AddPropertyDefinition(ProfilePropertyDefinition definition)
        {
            int portalId = GetEffectivePortalId(definition.PortalId);
            if (definition.Required)
            {
                definition.Visible = true;
            }
            int intDefinition = _dataProvider.AddPropertyDefinition(portalId,
                                                               definition.ModuleDefId,
                                                               definition.DataType,
                                                               definition.DefaultValue,
                                                               definition.PropertyCategory,
                                                               definition.PropertyName,
                                                               definition.ReadOnly,
                                                               definition.Required,
                                                               definition.ValidationExpression,
                                                               definition.ViewOrder,
                                                               definition.Visible,
                                                               definition.Length,
                                                               (int) definition.DefaultVisibility,
                                                               UserController.GetCurrentUserInfo().UserID);
            var objEventLog = new EventLogController();
            objEventLog.AddLog(definition, PortalController.GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.PROFILEPROPERTY_CREATED);
            ClearProfileDefinitionCache(definition.PortalId);
            return intDefinition;
        }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Clears the Profile Definitions Cache
		/// </summary>
		/// <param name="portalId">Id of the Portal</param>
		/// -----------------------------------------------------------------------------
        public static void ClearProfileDefinitionCache(int portalId)
        {
            DataCache.ClearDefinitionsCache(GetEffectivePortalId(portalId));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Deletes a Property Defintion from the Data Store
        /// </summary>
        /// <param name="definition">The ProfilePropertyDefinition object to delete</param>
        /// -----------------------------------------------------------------------------
        public static void DeletePropertyDefinition(ProfilePropertyDefinition definition)
        {
            _dataProvider.DeletePropertyDefinition(definition.PropertyDefinitionId);
            var objEventLog = new EventLogController();
            objEventLog.AddLog(definition, PortalController.GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.PROFILEPROPERTY_DELETED);
            ClearProfileDefinitionCache(definition.PortalId);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a Property Defintion from the Data Store by id
        /// </summary>
        /// <param name="definitionId">The id of the ProfilePropertyDefinition object to retrieve</param>
        /// <param name="portalId">Portal Id.</param>
        /// <returns>The ProfilePropertyDefinition object</returns>
        /// -----------------------------------------------------------------------------
        public static ProfilePropertyDefinition GetPropertyDefinition(int definitionId, int portalId)
        {
            bool bFound = Null.NullBoolean;
            ProfilePropertyDefinition definition = null;
            foreach (ProfilePropertyDefinition def in GetPropertyDefinitions(GetEffectivePortalId(portalId)))
            {
                if (def.PropertyDefinitionId == definitionId)
                {
                    definition = def;
                    bFound = true;
                    break;
                }
            }
            if (!bFound)
            {
				//Try Database
                definition = FillPropertyDefinitionInfo(_dataProvider.GetPropertyDefinition(definitionId));
            }
            return definition;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a Property Defintion from the Data Store by name
        /// </summary>
        /// <param name="portalId">The id of the Portal</param>
        /// <param name="name">The name of the ProfilePropertyDefinition object to retrieve</param>
        /// <returns>The ProfilePropertyDefinition object</returns>
        /// -----------------------------------------------------------------------------
        public static ProfilePropertyDefinition GetPropertyDefinitionByName(int portalId, string name)
        {
            portalId = GetEffectivePortalId(portalId);

            bool bFound = Null.NullBoolean;
            ProfilePropertyDefinition definition = null;
            foreach (ProfilePropertyDefinition def in GetPropertyDefinitions(portalId))
            {
                if (def.PropertyName == name)
                {
                    definition = def;
                    bFound = true;
                    break;
                }
            }
            if (!bFound)
            {
				//Try Database
                definition = FillPropertyDefinitionInfo(_dataProvider.GetPropertyDefinitionByName(portalId, name));
            }
            return definition;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a collection of Property Defintions from the Data Store by category
        /// </summary>
        /// <param name="portalId">The id of the Portal</param>
        /// <param name="category">The category of the Property Defintions to retrieve</param>
        /// <returns>A ProfilePropertyDefinitionCollection object</returns>
        /// -----------------------------------------------------------------------------
        public static ProfilePropertyDefinitionCollection GetPropertyDefinitionsByCategory(int portalId, string category)
        {
            portalId = GetEffectivePortalId(portalId); 
            
            var definitions = new ProfilePropertyDefinitionCollection();
            foreach (ProfilePropertyDefinition definition in GetPropertyDefinitions(portalId))
            {
                if (definition.PropertyCategory == category)
                {
                    definitions.Add(definition);
                }
            }
            return definitions;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a collection of Property Defintions from the Data Store by portal
        /// </summary>
        /// <param name="portalId">The id of the Portal</param>
        /// <returns>A ProfilePropertyDefinitionCollection object</returns>
        /// -----------------------------------------------------------------------------
        public static ProfilePropertyDefinitionCollection GetPropertyDefinitionsByPortal(int portalId)
        {
            return GetPropertyDefinitionsByPortal(portalId, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a collection of Property Defintions from the Data Store by portal
        /// </summary>
        /// <param name="portalId">The id of the Portal</param>
        /// <param name="clone">Whether to use a clone object.</param>
        /// <returns>A ProfilePropertyDefinitionCollection object</returns>
        /// -----------------------------------------------------------------------------
        public static ProfilePropertyDefinitionCollection GetPropertyDefinitionsByPortal(int portalId, bool clone)
        {
            return GetPropertyDefinitionsByPortal(portalId, clone, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a collection of Property Defintions from the Data Store by portal
        /// </summary>
        /// <param name="portalId">The id of the Portal</param>
        /// <param name="clone">Whether to use a clone object.</param>
        /// <param name="includeDeleted">Whether to include deleted profile properties.</param>
        /// <returns>A ProfilePropertyDefinitionCollection object</returns>
        /// -----------------------------------------------------------------------------
        public static ProfilePropertyDefinitionCollection GetPropertyDefinitionsByPortal(int portalId, bool clone, bool includeDeleted)
        {
            portalId = GetEffectivePortalId(portalId);

            var definitions = new ProfilePropertyDefinitionCollection();
            foreach (ProfilePropertyDefinition definition in GetPropertyDefinitions(portalId))
            {
                if (!definition.Deleted || includeDeleted)
                {
                    definitions.Add(clone ? definition.Clone() : definition);
                }
            }
            return definitions;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Profile Information for the User
        /// </summary>
        /// <remarks></remarks>
        /// <param name="user">The user whose Profile information we are retrieving.</param>
        /// -----------------------------------------------------------------------------
        public static void GetUserProfile(ref UserInfo user)
        {
            int portalId = user.PortalID;
            user.PortalID = GetEffectivePortalId(portalId);

            _profileProvider.GetUserProfile(ref user);
            user.PortalID = portalId;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Updates a Property Defintion in the Data Store
        /// </summary>
        /// <param name="definition">The ProfilePropertyDefinition object to update</param>
        /// -----------------------------------------------------------------------------
        public static void UpdatePropertyDefinition(ProfilePropertyDefinition definition)
        {
            
            if (definition.Required)
            {
                definition.Visible = true;
            }
            _dataProvider.UpdatePropertyDefinition(definition.PropertyDefinitionId,
                                              definition.DataType,
                                              definition.DefaultValue,
                                              definition.PropertyCategory,
                                              definition.PropertyName,
                                              definition.ReadOnly,
                                              definition.Required,
                                              definition.ValidationExpression,
                                              definition.ViewOrder,
                                              definition.Visible,
                                              definition.Length,
                                              (int) definition.DefaultVisibility,
                                              UserController.GetCurrentUserInfo().UserID);
            var objEventLog = new EventLogController();
            objEventLog.AddLog(definition, PortalController.GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.PROFILEPROPERTY_UPDATED);
            ClearProfileDefinitionCache(definition.PortalId);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Updates a User's Profile
        /// </summary>
        /// <param name="user">The use to update</param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static void UpdateUserProfile(UserInfo user)
        {
            int portalId = GetEffectivePortalId(user.PortalID);
            user.PortalID = portalId;
           
            //Update the User Profile
            if (user.Profile.IsDirty)
            {
                _profileProvider.UpdateUserProfile(user);
            }

            //Remove the UserInfo from the Cache, as it has been modified
            DataCache.ClearUserCache(user.PortalID, user.Username);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Updates a User's Profile
        /// </summary>
        /// <param name="user">The use to update</param>
        /// <param name="profileProperties">The collection of profile properties</param>
        /// <returns>The updated User</returns>
        /// -----------------------------------------------------------------------------
        public static UserInfo UpdateUserProfile(UserInfo user, ProfilePropertyDefinitionCollection profileProperties)
        {
            int portalId = GetEffectivePortalId(user.PortalID);
            user.PortalID = portalId;
            
            bool updateUser = Null.NullBoolean;
            //Iterate through the Definitions
            if (profileProperties != null)
            {
                foreach (ProfilePropertyDefinition propertyDefinition in profileProperties)
                {
                    string propertyName = propertyDefinition.PropertyName;
                    string propertyValue = propertyDefinition.PropertyValue;
                    if (propertyDefinition.IsDirty)
                    {
                        user.Profile.SetProfileProperty(propertyName, propertyValue);
                        if (propertyName.ToLower() == "firstname" || propertyName.ToLower() == "lastname")
                        {
                            updateUser = true;
                        }
                    }
                }
                UpdateUserProfile(user);
                if (updateUser)
                {
                    UserController.UpdateUser(portalId, user);
                }
            }
            return user;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Validates the Profile properties for the User (determines if all required properties
        /// have been set)
        /// </summary>
        /// <param name="portalId">The Id of the portal.</param>
        /// <param name="objProfile">The profile.</param>
        /// -----------------------------------------------------------------------------
        public static bool ValidateProfile(int portalId, UserProfile objProfile)
        {
            bool isValid = true;
            foreach (ProfilePropertyDefinition propertyDefinition in objProfile.ProfileProperties)
            {
                if (propertyDefinition.Required && string.IsNullOrEmpty(propertyDefinition.PropertyValue))
                {
                    isValid = false;
                    break;
                }
            }
            return isValid;
        }

        #endregion

        #region Obsolete Methods

        [Obsolete("This method has been deprecated.  Please use GetPropertyDefinition(ByVal definitionId As Integer, ByVal portalId As Integer) instead")]
        public static ProfilePropertyDefinition GetPropertyDefinition(int definitionId)
        {
            return (ProfilePropertyDefinition) CBO.FillObject(_dataProvider.GetPropertyDefinition(definitionId), typeof (ProfilePropertyDefinition));
        }

        #endregion
    }
}
