using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Script.Services;
using System.Xml.Linq;
namespace ItemLoader
{
    /// <summary>
    /// Summary description for load_items1
    /// </summary>
    [WebService(Namespace = "/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class load_items1 : System.Web.Services.WebService
    {
        [WebMethod]
        public List<string> upload()
        {
            List<string> item_id_list = new List<string>();
            HttpRequest req = HttpContext.Current.Request;
            if (req.Files.Count == 1)
            {
                HttpPostedFile file = HttpContext.Current.Request.Files["xml"];
                var stream = file.InputStream;
                var xml = XDocument.Load(stream);

                foreach (XElement tag in xml.Descendants())
                {
                    if (tag.Name == "Items")
                    {
                        string TemplateID = tag.Attribute("TemplateID").Value;
                        string ParentID = tag.Attribute("ParentID").Value;
                        foreach (XElement item in tag.Descendants())
                        {
                            if (item.Name == "Item")
                            {
                                using (new Sitecore.SecurityModel.SecurityDisabler())
                                {
                                    Sitecore.Data.Database master = Sitecore.Data.Database.GetDatabase("master");
                                    Sitecore.Data.Items.Item parent = master.GetItem(new Sitecore.Data.ID(ParentID));
                                    Sitecore.Data.Items.TemplateItem template = master.GetTemplate(new Sitecore.Data.ID(TemplateID));

                                    string ItemName = item.Attribute("Name").Value;

                                    Sitecore.Data.Items.Item newItem = parent.Add(ItemName, template);

                                    newItem.Editing.BeginEdit();
                                    try
                                    {

                                        foreach (XElement field in item.Descendants())
                                        {
                                            if (field.Name == "Field")
                                            {
                                                string name = field.Attribute("Name").Value;
                                                string value = field.Attribute("Value").Value;
                                                newItem.Fields[name].Value = value;
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        newItem.Editing.EndEdit();
                                    }
                                    item_id_list.Add(newItem.ID.ToString());
                                }
                            }
                        }
                    }
                }
                
            }
            return item_id_list;
        }

        [WebMethod]
        public List<string> delete()
        {
            List<string> item_id_list = new List<string>();
            HttpRequest req = HttpContext.Current.Request;
            if (req.Files.Count == 1)
            {
                HttpPostedFile file = HttpContext.Current.Request.Files["xml"];
                var stream = file.InputStream;
                var xml = XDocument.Load(stream);

                using (new Sitecore.SecurityModel.SecurityDisabler())
                {

                    Sitecore.Data.Database master = Sitecore.Data.Database.GetDatabase("master");
                    foreach (XElement tag in xml.Descendants())
                    {
                        if (tag.Name == "{/}string")
                        {   
                            Sitecore.Data.ID itemID = Sitecore.Data.ID.Parse(tag.Value);
                            Sitecore.Data.Items.Item item = master.GetItem(itemID);
                            item.Delete();
                            item_id_list.Add(item.ID.ToString());
                        }
                    }
                }
            }
            return item_id_list;
        }
    }
}
