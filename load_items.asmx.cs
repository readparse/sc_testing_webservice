using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Script.Services;
using System.Xml.Linq;
using System.Collections.ObjectModel;
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
        Sitecore.Data.Database master = Sitecore.Data.Database.GetDatabase("master");
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
                                                
                                                
                                                if (field.Attributes().Any(p => p.Name == "Type")) {
                                                    string type = field.Attribute("Type").Value;
                                                    if (type == "Date")
                                                    {
                                                        value = Sitecore.DateUtil.ToIsoDate(DateTime.Parse(value));
                                                    }
                                                }

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
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public Collection<Item> get_items()
        {
            Collection<Item> items = new Collection<Item>();
            HttpRequest req = HttpContext.Current.Request;
            string list = req.Params["list"];
            string[] id_list = list.Split(':');
            foreach (string id in id_list)
            {
                var i = new Item();
                i.id = id;
                i.load();
                items.Add(i);
            }
            return items;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public Item get_item()
        {
            HttpRequest req = HttpContext.Current.Request;
            string id = req.Params["id"];
            Item item = new Item();
            item.id = id;
            item.load();
            return item;
        }

        [WebMethod]
        public int spiderman()
        {
            HttpRequest req = HttpContext.Current.Request;
            string list = req.Params["list"];
            string[] id_list = list.Split(':');
            //return id_list.Count();
            int count = 0;
            using (new Sitecore.SecurityModel.SecurityDisabler())
            {
                foreach (string id in id_list)
                {
                    var item = master.GetItem(new Sitecore.Data.ID(id));
                    item.Delete();
                    count++;
                }
            }
            return count;
            
        }

        [WebMethod]
        public bool delete_item()
        {
            HttpRequest req = HttpContext.Current.Request;
            string id = req.Params["id"];
            using (new Sitecore.SecurityModel.SecurityDisabler())
            {
                var item = master.GetItem(new Sitecore.Data.ID(id));
                item.Delete();
            }
            return true;
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
                            try
                            {
                                Sitecore.Data.Items.Item item = master.GetItem(itemID);
                                item.Delete();
                                item_id_list.Add(item.ID.ToString());
                            }
                            catch(Exception e)
                            {
                                //swallow
                            }
                        }
                    }
                }
            }
            return item_id_list;
        }
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public bool publish()
        {
            Sitecore.Data.Database master = Sitecore.Data.Database.GetDatabase("master");
            Sitecore.Data.Database web = Sitecore.Data.Database.GetDatabase("web");
            var options = new Sitecore.Publishing.PublishOptions(master, web, Sitecore.Publishing.PublishMode.Smart, Sitecore.Globalization.Language.Parse("en"), System.DateTime.Now);
            var publisher = new Sitecore.Publishing.Publisher(options);

            publisher.Options.RootItem = master.GetItem("/sitecore/content/home");
            publisher.Options.Deep = true;
            publisher.Publish();
            return true;
        }
    }
    public class Item
    {
        public string id { get; set; }
        public string name { get; set; }
        public Collection<Field> fields = new Collection<Field>();

        public void load()
        {
            Sitecore.Data.Database master = Sitecore.Data.Database.GetDatabase("master");
            Sitecore.Data.Items.Item item = master.GetItem( new Sitecore.Data.ID(this.id) );
            this.name = item.Name;
            foreach (Sitecore.Data.Fields.Field field in item.Fields)
            {
                Field ff = new Field(field.Name, field.Value, field.Type);
                this.fields.Add(ff);
            }
        }
    }
    
    
    public class Field
    {
        public string name { get; set; }
        public string value { get; set; }
        public string type { get; set; }

        public Field(string name, string value, string type)
        {
            this.name = name;
            this.value = value;
            this.type = type;
        }
        public Field(string name, string value)
        {
            this.name = name;
            this.value = value;
        }
        public Field(string name)
        {
            this.name = name;
        }
        public Field()
        {

        }

    }
}
