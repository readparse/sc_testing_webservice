using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Script.Services;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using Sitecore.Exceptions;

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
        HttpRequest request = HttpContext.Current.Request;
        [WebMethod]
        public List<string> media_item_info()
        {
            List<string> outlist = new List<string>();
            string id = request.Params["id"];
            outlist.Add(id);
            Sitecore.Data.Items.Item item = master.GetItem(new Sitecore.Data.ID(id));
            Sitecore.Data.Items.MediaItem media_item = new Sitecore.Data.Items.MediaItem(item);
            Sitecore.Resources.Media.Media media = Sitecore.Resources.Media.MediaManager.GetMedia(media_item);
            Sitecore.Resources.Media.MediaData media_data = media.MediaData;
            
            return outlist;
        }
        
        [WebMethod]
        /*
        The following method takes a sitecore media library destination path, a title, and a media file as
        upload parameters.  It creates the media item and the media file, in the specified destination path,
        creating that path if it does not exist.
        */
        public Item create_media_item()
        {
            List<HttpPostedFile> files = new List<HttpPostedFile>();

            HttpPostedFile media_file = request.Files["media_file"];

            var stream = media_file.InputStream;
            using (new Sitecore.SecurityModel.SecurityDisabler())
            {
                /*
                any dots in the filename must be replaced.  TODO: there are other characters that also will not work,
                so we will have to deal with that at some point.  By default, item names must pass the following regex:
                ^[\w\*\$][\w\s\-\$]*(\(\d{1,}\)){0,1}$
                Sitecore also doesn't allow, by default, any of the following characters:
                    /  :  ?  "  <  >  |  [  ]
                */
                string itemName = media_file.FileName.Replace(".", "_").Replace("&", "_and_");

                // set the options for the media creation
                Sitecore.Resources.Media.MediaCreatorOptions options = new Sitecore.Resources.Media.MediaCreatorOptions();
                options.Database = master;
                options.Destination = request.Params["destination"] + "/" + itemName;

                //create the item.  Then get that Item's "MediaItem", and then get the MediaItem's "Media". 
                //From the "Media" object, get the file extension, so we can strip if off the end of the item name
                //We do this to avoid downloads like "foo.xls.xls"
                Sitecore.Data.Items.Item item = Sitecore.Resources.Media.MediaManager.Creator.CreateFromStream(stream, media_file.FileName, options);
                Sitecore.Data.Items.MediaItem media_item = new Sitecore.Data.Items.MediaItem(item);
                Sitecore.Resources.Media.Media media = Sitecore.Resources.Media.MediaManager.GetMedia(media_item);
                string new_name = itemName.Replace("_" + media.Extension, "");

                // now edit the Title field and the item's name
                item.Editing.BeginEdit();
                item.Fields["Title"].Value = request.Params["title"];
                item.Name = new_name;
                item.Editing.EndEdit();

                var i = new Item();
                i.id = item.ID.ToString();
                i.load();
                return i;
            }
        }

        [WebMethod]
        public List<string> upload()
        {
            List<string> item_id_list = new List<string>();
            if (request.Files.Count == 1)
            {
                HttpPostedFile file = request.Files["xml"];
                var stream = file.InputStream;
                var xml = XDocument.Load(stream);
                foreach (XElement tag in xml.Descendants())
                {
                    if (tag.Name == "Items")
                    {
                        string TemplateID = tag.Attribute("TemplateID").Value;
                        string ParentID = tag.Attribute("ParentID").Value;
                        string domainUser = tag.Attribute("User").Value;
                        foreach (XElement item in tag.Descendants())
                        {
                            if (item.Name == "Item")
                            {
                                if (Sitecore.Security.Accounts.User.Exists(domainUser))
                                {
                                    Sitecore.Security.Accounts.User user = Sitecore.Security.Accounts.User.FromName(domainUser, false);
                                    using (new Sitecore.Security.Accounts.UserSwitcher(user))
                                    {
                                        Sitecore.Data.Database master = Sitecore.Data.Database.GetDatabase("master");
                                        Sitecore.Data.Items.Item parent = master.GetItem(new Sitecore.Data.ID(ParentID));
                                        Sitecore.Data.Items.TemplateItem template = master.GetTemplate(new Sitecore.Data.ID(TemplateID));

                                        string ItemName = item.Attribute("Name").Value;

                                        Sitecore.Data.Items.Item newItem;
                                        try
                                        {
                                             newItem = parent.Add(ItemName, template);
                                        }
                                        catch(AccessDeniedException e)
                                        {
                                            return item_id_list;
                                        }

                                        newItem.Editing.BeginEdit();
                                        try
                                        {

                                            foreach (XElement field in item.Descendants())
                                            {
                                                if (field.Name == "Field")
                                                {

                                                    string name = field.Attribute("Name").Value;
                                                    string value = field.Attribute("Value").Value;


                                                    if (field.Attributes().Any(p => p.Name == "Type"))
                                                    {
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
                
            }
            return item_id_list;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public Collection<Item> get_items()
        {
            Collection<Item> items = new Collection<Item>();
            string list = request.Params["list"];
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
            string id = request.Params["id"];
            Item item = new Item();
            item.id = id;
            item.load();
            return item;
        }

        [WebMethod]
        public int spiderman()
        {
            string list = request.Params["list"];
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
            string id = request.Params["id"];
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
            if (request.Files.Count == 1)
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
        public string url { get; set; }
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
            this.url = Sitecore.Links.LinkManager.GetItemUrl(item);
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
