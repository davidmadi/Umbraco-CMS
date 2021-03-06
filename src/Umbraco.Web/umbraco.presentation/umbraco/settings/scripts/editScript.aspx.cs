using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.IO;
using Umbraco.Core;
using Umbraco.Core.IO;
using umbraco.cms.presentation.Trees;
using System.Linq;
using umbraco.cms.helpers;
using umbraco.uicontrols;

namespace umbraco.cms.presentation.settings.scripts
{
    public partial class editScript : BasePages.UmbracoEnsuredPage
    {
        public editScript()
        {
            CurrentApp = BusinessLogic.DefaultApps.settings.ToString();

        }
        protected System.Web.UI.HtmlControls.HtmlForm Form1;
        protected uicontrols.UmbracoPanel Panel1;
        protected System.Web.UI.WebControls.TextBox NameTxt;
        protected uicontrols.Pane Pane7;

        protected System.Web.UI.WebControls.Literal lttPath;
        protected System.Web.UI.WebControls.Literal editorJs;
        protected umbraco.uicontrols.CodeArea editorSource;
        protected umbraco.uicontrols.PropertyPanel pp_name;
        protected umbraco.uicontrols.PropertyPanel pp_path;

        protected MenuIconI SaveButton;

        private string file;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            NameTxt.Text = file;

            string path = "";
            if (file.StartsWith("~/"))
                path = Umbraco.Core.IO.IOHelper.ResolveUrl(file);
            else
                path = Umbraco.Core.IO.IOHelper.ResolveUrl(Umbraco.Core.IO.SystemDirectories.Scripts + "/" + file);


            lttPath.Text = "<a target='_blank' href='" + path + "'>" + path + "</a>";

            var exts = UmbracoSettings.ScriptFileTypes.Split(',').ToList();
            if (Umbraco.Core.Configuration.UmbracoSettings.DefaultRenderingEngine == RenderingEngine.Mvc)
            {
                exts.Add("cshtml");
                exts.Add("vbhtml");
            }

            var dirs = Umbraco.Core.IO.SystemDirectories.Scripts;
            if (Umbraco.Core.Configuration.UmbracoSettings.DefaultRenderingEngine == RenderingEngine.Mvc)
                dirs += "," + Umbraco.Core.IO.SystemDirectories.MvcViews;

            // validate file
            Umbraco.Core.IO.IOHelper.ValidateEditPath(Umbraco.Core.IO.IOHelper.MapPath(path), dirs.Split(','));

            // validate extension
            Umbraco.Core.IO.IOHelper.ValidateFileExtension(Umbraco.Core.IO.IOHelper.MapPath(path), exts);


            StreamReader SR;
            string S;
            SR = File.OpenText(Umbraco.Core.IO.IOHelper.MapPath(path));
            S = SR.ReadToEnd();
            SR.Close();

            editorSource.Text = S;

            Panel1.Text = ui.Text("editscript", base.getUser());
            pp_name.Text = ui.Text("name", base.getUser());
            pp_path.Text = ui.Text("path", base.getUser());

            if (!IsPostBack)
            {
                string sPath = DeepLink.GetTreePathFromFilePath(file);
                ClientTools
                    .SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadScripts>().Tree.Alias)
                    .SyncTree(sPath, false);
            }
        }

        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);

            file = Request.QueryString["file"].TrimStart('/');

            //need to change the editor type if it is XML
            if (file.EndsWith("xml"))
                editorSource.CodeBase = uicontrols.CodeArea.EditorType.XML;
            else if (file.EndsWith("master"))
                editorSource.CodeBase = uicontrols.CodeArea.EditorType.HTML;

            SaveButton = Panel1.Menu.NewIcon();
            SaveButton.ImageURL = SystemDirectories.Umbraco + "/images/editor/save.gif";
            SaveButton.AltText = "Save File";
            SaveButton.ID = "save";

            if (editorSource.CodeBase == uicontrols.CodeArea.EditorType.HTML)
            {
                // Editing buttons
                Panel1.Menu.InsertSplitter();
                uicontrols.MenuIconI umbField = Panel1.Menu.NewIcon();
                umbField.ImageURL = UmbracoPath + "/images/editor/insField.gif";
                umbField.OnClickCommand = BasePages.ClientTools.Scripts.OpenModalWindow(IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/dialogs/umbracoField.aspx?objectId=" + editorSource.ClientID + "&tagName=UMBRACOGETDATA", ui.Text("template", "insertPageField"), 640, 550);
                umbField.AltText = ui.Text("template", "insertPageField");

                // TODO: Update icon
                uicontrols.MenuIconI umbDictionary = Panel1.Menu.NewIcon();
                umbDictionary.ImageURL = GlobalSettings.Path + "/images/editor/dictionaryItem.gif";
                umbDictionary.OnClickCommand = BasePages.ClientTools.Scripts.OpenModalWindow(IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/dialogs/umbracoField.aspx?objectId=" + editorSource.ClientID + "&tagName=UMBRACOGETDICTIONARY", ui.Text("template", "insertDictionaryItem"), 640, 550);
                umbDictionary.AltText = "Insert umbraco dictionary item";

                uicontrols.MenuIconI umbMacro = Panel1.Menu.NewIcon();
                umbMacro.ImageURL = UmbracoPath + "/images/editor/insMacro.gif";
                umbMacro.AltText = ui.Text("template", "insertMacro");
                umbMacro.OnClickCommand = BasePages.ClientTools.Scripts.OpenModalWindow(IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/dialogs/editMacro.aspx?objectId=" + editorSource.ClientID, ui.Text("template", "insertMacro"), 470, 530);

                // Help
                Panel1.Menu.InsertSplitter();

                uicontrols.MenuIconI helpIcon = Panel1.Menu.NewIcon();
                helpIcon.OnClickCommand = umbraco.BasePages.ClientTools.Scripts.OpenModalWindow(Umbraco.Core.IO.IOHelper.ResolveUrl(Umbraco.Core.IO.SystemDirectories.Umbraco) + "/settings/modals/showumbracotags.aspx?alias=", ui.Text("template", "quickGuide"), 600, 580);
                helpIcon.ImageURL = UmbracoPath + "/images/editor/help.png";
                helpIcon.AltText = ui.Text("template", "quickGuide");

            }

        }
        

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/codeEditorSave.asmx"));
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/legacyAjaxCalls.asmx"));
        }

    }
}
