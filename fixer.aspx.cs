using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using Sitecore.Data.SqlServer;

namespace AttachmentFixer
{
    public partial class fixer : System.Web.UI.Page
    {
        
        protected void Page_Load(object sender, EventArgs e)
        {
            Sitecore.Data.Database master = Sitecore.Data.Database.GetDatabase("master");
            string WebRoot = Request.ServerVariables.Get("APPL_PHYSICAL_PATH");
            Label1.Text = WebRoot;
        }

        protected void Go_Click(object sender, EventArgs e)
        {

            using (new Sitecore.SecurityModel.SecurityDisabler())
            {
                Label1.Text = "";
                Sitecore.Data.Database master = Sitecore.Data.Database.GetDatabase("master");
                string DataRoot = Sitecore.Configuration.Settings.DataFolder;
                string MediaFiles = Sitecore.Configuration.Settings.GetSetting("Media.FileFolder").Replace('/', '\\');
                //string MediaFileRoot = DataRoot + "..\\Website" + MediaFiles;
                
                string text = TextArea1.InnerText.ToString();
                string[] lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                foreach (string item_id in lines)
                {
                    if (item_id != "")
                    {

                        Sitecore.Data.Items.Item item = master.GetItem(new Sitecore.Data.ID(item_id));
                        if (item != null)
                        {
                            string file_path = item.Fields["File Path"].ToString();
                            string blob = item.Fields["Blob"].ToString();
                            if (file_path != "")  // This media item has a "File Path" value.  That means the data is stored on the filesystem.
                            {
                                Sitecore.Data.Items.MediaItem media_item = new Sitecore.Data.Items.MediaItem(item);
                                string size = media_item.Size.ToString();

                                string WebRoot = Request.ServerVariables.Get("APPL_PHYSICAL_PATH");
                                string MediaPath = WebRoot + "\\" + media_item.FilePath;
                                try
                                {
                                    if (System.IO.File.Exists(MediaPath))
                                    {
                                        System.IO.Stream stream = media_item.GetMediaStream();

                                        string bytes = stream.Length.ToString();
                                        string type = stream.GetType().ToString();

                                        item.Editing.BeginEdit();
                                        item.Fields["File Path"].Value = "";
                                        item.Fields["Blob"].SetBlobStream((System.IO.Stream)stream);
                                        item.Editing.EndEdit();

                                        Label1.Text += "<div>" + item.Name.ToString() + ": <b>migrated</b></div>";
                                    }
                                    else
                                    {
                                        Label1.Text += "<div>" + item.Name.ToString() + ": <b>does not exist</b> (" + MediaPath + ")</div>";
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Label1.Text += "<div>Caught Exception " + ex.Message + " on item " + item.Name.ToString() + "</div>";
                                }

                            }
                            else if (blob != "") // This media item has a value in the "Blob" field, so it's stored in the database.
                            {
                                Sitecore.Data.Items.MediaItem media_item = new Sitecore.Data.Items.MediaItem(item);
                                System.IO.Stream stream = media_item.GetMediaStream();
                                var bytes = stream.Length.ToString();
                                string type = stream.GetType().ToString();
                                //Sitecore.Data.SqlServer.SqlServerDataProvider provider = new Sitecore.Data.SqlServer.SqlServerDataProvider();

                                Label1.Text += "<div>" + item.Name.ToString() + ": <b>" + type + "</b></div>";

                            }

                        }
                        else
                        {
                            Label1.Text += "<div>No item found for " + item_id + "</div>";
                        }
                    }
                }
            }
        }
    }
}