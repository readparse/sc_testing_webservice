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

        }

        protected void Go_Click(object sender, EventArgs e)
        {
           

            Label1.Text = "";
            Sitecore.Data.Database master = Sitecore.Data.Database.GetDatabase("master");
            //string connection_string = Sitecore.Configuration.Settings.GetConnectionString("master");
            //SqlConnection connection = new SqlConnection(connection_string);
            //connection.Open();
            //string sql = "INSERT INTO [Blobs]( [Id], [BlobId], [Index], [Created], [Data] ) VALUES(   NewId(), NewId(), @index, @created, @data)";

            string text = TextArea1.InnerText.ToString();
            string[] lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            foreach (string item_id in lines) 
            {
                if (item_id != "") 
                {
               
                  
                  //Label1.Text += "<div>-" + item_id + "-</div>";
                  Sitecore.Data.Items.Item item = master.GetItem(new Sitecore.Data.ID(item_id));
                  if (item != null)
                  {
                      string file_path = item.Fields["File Path"].ToString();
                      string blob = item.Fields["Blob"].ToString();
                      if (file_path != "")  // This media item has a "File Path" value.  That means the data is stored on the filesystem.
                      {
                          Sitecore.Data.Items.MediaItem media_item = new Sitecore.Data.Items.MediaItem(item);
                          string size = media_item.Size.ToString();
                          System.IO.Stream stream = media_item.GetMediaStream();
                          string bytes = stream.Length.ToString();
                          string type = stream.GetType().ToString();
                          /*
                          var command = new SqlCommand(sql, connection);
                          command.Parameters.AddWithValue("@index", 0);
                          command.Parameters.AddWithValue("@created", DateTime.UtcNow);
                          command.Parameters.Add("@data", System.Data.SqlDbType.Image, (Int32)stream.Length).Value = stream;
                          command.ExecuteNonQuery();
                          */


                          item.Editing.BeginEdit();
                          item.Fields["File Path"].Value = "";
                          item.Fields["Blob"].SetBlobStream((System.IO.Stream)stream);
                          item.Editing.EndEdit();

                          Label1.Text += "<div>" + item.Name.ToString() + ": <b>migrated</b></div>";
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
                
                /*
                //Sitecore.Collections.FieldCollection fields = child.Fields;
                string blob = child.Fields["Blob"].ToString();
                string path = child.Fields["File Path"].ToString();
                //Label1.Text += "<div>" + child.Name.ToString() + "</div>";
                if (blob != "")
                {
                    ListBox1.Items.Add(new ListItem(child.Name.ToString() + " has a blob (" + blob + ")"));
                    Label1.Text += "<div>" + child.Name.ToString() + " has a blob (<span class=blob>" + blob + "</span>)</div>";
                }
                else if (path != "")
                {
                    ListBox1.Items.Add(new ListItem(child.Name.ToString() + " has a file path (" + path + ")"));
                    Label1.Text += "<div>" + child.Name.ToString() + " has a file path (<span class=path>" + path + "</span>)</div>";
                }
                else
                {
                    ListBox1.Items.Add(new ListItem(child.Name.ToString() + " nas neither"));
                    Label1.Text += "<div>" + child.Name.ToString() + " has neither</div>";
                }
                 */
            }
            //connection.Close();    
        }
        
    }
}