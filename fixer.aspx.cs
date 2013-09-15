using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AttachmentFixer
{
    public partial class fixer : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Lookup_Click(object sender, EventArgs e)
        {
            ListBox1.Items.Clear();
            Label1.Text = "";
            string start_item_path = start_item.Text.ToString();
            if (start_item_path != "")
            {
                Sitecore.Data.Database master = Sitecore.Data.Database.GetDatabase("master");
                Sitecore.Data.Items.Item products = master.GetItem(start_item_path);
                foreach (Sitecore.Data.Items.Item child in products.Children)
                {

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
                }

            }
        }
    }
}