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
using System.IO;
using System.Xml.XPath;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Framework.Providers;

#endregion

namespace DotNetNuke.Services.Installer.Installers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ScriptInstaller installs Script Components to a DotNetNuke site
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	08/07/2007  created
    /// </history>
    /// -----------------------------------------------------------------------------
    public class ScriptInstaller : FileInstaller
    {
		#region "Private Members"

        private readonly SortedList<Version, InstallFile> _InstallScripts = new SortedList<Version, InstallFile>();
        private readonly SortedList<Version, InstallFile> _UnInstallScripts = new SortedList<Version, InstallFile>();
        private InstallFile _InstallScript;
        private InstallFile _UpgradeScript;

		#endregion

		#region "Protected Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the base Install Script (if present)
        /// </summary>
        /// <value>An InstallFile</value>
        /// <history>
        /// 	[cnurse]	05/20/2008  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected InstallFile InstallScript
        {
            get
            {
                return _InstallScript;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the collection of Install Scripts
        /// </summary>
        /// <value>A List(Of InstallFile)</value>
        /// <history>
        /// 	[cnurse]	08/07/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected SortedList<Version, InstallFile> InstallScripts
        {
            get
            {
                return _InstallScripts;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the collection of UnInstall Scripts
        /// </summary>
        /// <value>A List(Of InstallFile)</value>
        /// <history>
        /// 	[cnurse]	08/07/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected SortedList<Version, InstallFile> UnInstallScripts
        {
            get
            {
                return _UnInstallScripts;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Collection Node ("scripts")
        /// </summary>
        /// <value>A String</value>
        /// <history>
        /// 	[cnurse]	08/07/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override string CollectionNodeName
        {
            get
            {
                return "scripts";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Item Node ("script")
        /// </summary>
        /// <value>A String</value>
        /// <history>
        /// 	[cnurse]	08/07/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override string ItemNodeName
        {
            get
            {
                return "script";
            }
        }

        protected ProviderConfiguration ProviderConfiguration
        {
            get
            {
                return ProviderConfiguration.GetProviderConfiguration("data");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Upgrade Script (if present)
        /// </summary>
        /// <value>An InstallFile</value>
        /// <history>
        /// 	[cnurse]	07/14/2009  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected InstallFile UpgradeScript
        {
            get
            {
                return _UpgradeScript;
            }
        }
		
		#endregion

		#region "Public Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a list of allowable file extensions (in addition to the Host's List)
        /// </summary>
        /// <value>A String</value>
        /// <history>
        /// 	[cnurse]	03/28/2008  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public override string AllowableFiles
        {
            get
            {
                return "*dataprovider, sql";
            }
        }
		
		#endregion

		#region "Private Methods"


        private bool ExecuteSql(InstallFile scriptFile, bool useTransaction)
        {
            bool bSuccess = true;

            Log.AddInfo(string.Format(Util.SQL_BeginFile, scriptFile.Name));

            //read script file for installation
            string strScript = FileSystemUtils.ReadFile(PhysicalBasePath + scriptFile.FullName);

            //This check needs to be included because the unicode Byte Order mark results in an extra character at the start of the file
            //The extra character - '?' - causes an error with the database.
            if (strScript.StartsWith("?"))
            {
                strScript = strScript.Substring(1);
            }
            string strSQLExceptions = Null.NullString;
            strSQLExceptions = DataProvider.Instance().ExecuteScript(strScript);
            if (!String.IsNullOrEmpty(strSQLExceptions))
            {
                if (Package.InstallerInfo.IsLegacyMode)
                {
                    Log.AddWarning(string.Format(Util.SQL_Exceptions, Environment.NewLine, strSQLExceptions));
                }
                else
                {
                    Log.AddFailure(string.Format(Util.SQL_Exceptions, Environment.NewLine, strSQLExceptions));
                    bSuccess = false;
                }
            }
            Log.AddInfo(string.Format(Util.SQL_EndFile, scriptFile.Name));
            return bSuccess;
        }
		
		#endregion

		#region "Protected Methods"

        private bool InstallScriptFile(InstallFile scriptFile)
        {
			//Call base InstallFile method to copy file
            bool bSuccess = InstallFile(scriptFile);

            //Process the file if it is an Install Script
            string fileExtension = Path.GetExtension(scriptFile.Name.ToLower()).Substring(1);
            if (bSuccess && ProviderConfiguration.DefaultProvider.ToLower() == fileExtension)
            {
                Log.AddInfo(Util.SQL_Executing + scriptFile.Name);
                //bSuccess = ExecuteSql(scriptFile, True)
                bSuccess = ExecuteSql(scriptFile, false);
            }
            return bSuccess;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a flag that determines what type of file this installer supports
        /// </summary>
        /// <param name="type">The type of file being processed</param>
        /// <history>
        /// 	[cnurse]	08/07/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override bool IsCorrectType(InstallFileType type)
        {
            return (type == InstallFileType.Script);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ProcessFile method determines what to do with parsed "file" node
        /// </summary>
        /// <param name="file">The file represented by the node</param>
        /// <param name="nav">The XPathNavigator representing the node</param>
        /// <history>
        /// 	[cnurse]	08/07/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void ProcessFile(InstallFile file, XPathNavigator nav)
        {
            string type = nav.GetAttribute("type", "");
            if (file != null && IsCorrectType(file.Type))
            {
                if (file.Name.ToLower().StartsWith("install."))
                {
					//This is the initial script when installing
                    _InstallScript = file;
                }
                else if (file.Name.ToLower().StartsWith("upgrade."))
                {
                    _UpgradeScript = file;
                }
                else if (type.ToLower() == "install")
                {
					//These are the Install/Upgrade scripts
                    InstallScripts[file.Version] = file;
                }
                else
                {
					//These are the Uninstall scripts
                    UnInstallScripts[file.Version] = file;
                }
            }
			
            //Call base method to set up for file processing
            base.ProcessFile(file, nav);
        }

        protected override void UnInstallFile(InstallFile scriptFile)
        {
			//Process the file if it is an UnInstall Script
            if (UnInstallScripts.ContainsValue(scriptFile) && ProviderConfiguration.DefaultProvider.ToLower() == Path.GetExtension(scriptFile.Name.ToLower()).Substring(1))
            {
                if (scriptFile.Name.ToLower().StartsWith("uninstall."))
                {
					//Install Script
                    Log.AddInfo(Util.SQL_Executing + scriptFile.Name);
                    ExecuteSql(scriptFile, false);
                }
            }
			
            //Call base method to delete file
            base.UnInstallFile(scriptFile);
        }

		#endregion

		#region "Public Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Commit method finalises the Install and commits any pending changes.
        /// </summary>
        /// <remarks>In the case of Files this is not neccessary</remarks>
        /// <history>
        /// 	[cnurse]	08/07/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public override void Commit()
        {
            base.Commit();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Install method installs the script component
        /// </summary>
        /// <history>
        /// 	[cnurse]	08/07/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public override void Install()
        {
            Log.AddInfo(Util.SQL_Begin);
            try
            {
                bool bSuccess = true;
                Version installedVersion = Package.InstalledVersion;

                //First process InstallScript
                if (installedVersion == new Version(0, 0, 0))
                {
                    if (InstallScript != null)
                    {
                        bSuccess = InstallScriptFile(InstallScript);
                        installedVersion = InstallScript.Version;
                    }
                }
				
                //Then process remain Install/Upgrade Scripts
                if (bSuccess)
                {
                    foreach (InstallFile file in InstallScripts.Values)
                    {
                        if (file.Version > installedVersion)
                        {
                            bSuccess = InstallScriptFile(file);
                            if (!bSuccess)
                            {
                                break;
                            }
                        }
                    }
                }
				
                //Next process UpgradeScript - this script always runs if present
                if (UpgradeScript != null)
                {
                    bSuccess = InstallScriptFile(UpgradeScript);
                    installedVersion = UpgradeScript.Version;
                }
				
                //Then process uninstallScripts - these need to be copied but not executed
                if (bSuccess)
                {
                    foreach (InstallFile file in UnInstallScripts.Values)
                    {
                        bSuccess = InstallFile(file);
                        if (!bSuccess)
                        {
                            break;
                        }
                    }
                }
                Completed = bSuccess;
            }
            catch (Exception ex)
            {
                Log.AddFailure(ex);
            }
            Log.AddInfo(Util.SQL_End);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Rollback method undoes the installation of the script component in the event 
        /// that one of the other components fails
        /// </summary>
        /// <history>
        /// 	[cnurse]	08/07/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public override void Rollback()
        {
            base.Rollback();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UnInstall method uninstalls the script component
        /// </summary>
        /// <history>
        /// 	[cnurse]	08/07/2007  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public override void UnInstall()
        {
            Log.AddInfo(Util.SQL_BeginUnInstall);

            //Call the base method
            base.UnInstall();

            Log.AddInfo(Util.SQL_EndUnInstall);
        }
		
		#endregion
    }
}
