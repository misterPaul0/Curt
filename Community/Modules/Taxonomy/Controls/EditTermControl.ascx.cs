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
using System.Linq;
using System.Web.UI;

using DotNetNuke.Entities.Content.Taxonomy;


#endregion

namespace DotNetNuke.Modules.Taxonomy.Views.Controls
{

    public partial class EditTermControl : UserControl
    {

        #region Public Properties

        public string LocalResourceFile { get; set; }

        #endregion

        #region Public Methods

        public void BindTerm(Term term, IEnumerable<Term> terms, bool isHeirarchical, bool loadFromControl, bool editEnabled)
        {
            //nameValidator.Text = Services.Localization.Localization.GetString("TermName.Required", SharedResourceFile);

            if (loadFromControl)
            {
                term.Name = nameTextBox.Text;
                term.Description = descriptionTextBox.Text;
                if (isHeirarchical && !string.IsNullOrEmpty(parentTermCombo.SelectedValue))
                {
                    term.ParentTermId = Int32.Parse(parentTermCombo.SelectedValue);
                }
            }
            else
            {
                nameTextBox.Text = term.Name;
                descriptionTextBox.Text = term.Description;

                //Remove this term (and its descendants) from the collection, so we don't get wierd heirarchies
                var termsList = (from t in terms where !(t.Left >= term.Left && t.Right <= term.Right) select t).ToList();

                parentTermCombo.DataSource = termsList;
                parentTermCombo.DataBind();

                if (term.ParentTermId.HasValue && parentTermCombo.FindItemByValue(term.ParentTermId.ToString()) != null)
                {
                    parentTermCombo.FindItemByValue(term.ParentTermId.ToString()).Selected = true;
                }

                divParentTerm.Visible = isHeirarchical && termsList.Count > 0;
                nameTextBox.Enabled = editEnabled;
                descriptionTextBox.Enabled = editEnabled;
                parentTermCombo.Enabled = editEnabled;
            }
        }

        #endregion

    }
}