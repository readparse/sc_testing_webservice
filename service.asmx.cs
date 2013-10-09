using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Services;
using System.Web.Script.Services;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using Sitecore.Exceptions;


namespace SitecoreTestingService
{
    /// <summary>
    /// Summary description for service
    /// </summary>
    [WebService(Namespace = "/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class service : System.Web.Services.WebService
    {
        Sitecore.Data.Database master = Sitecore.Data.Database.GetDatabase("master");
        HttpRequest req = HttpContext.Current.Request;
        [WebMethod]
        public List<string> media_item_info()
        {
            List<string> outlist = new List<string>();
            string id = req.Params["id"];
            outlist.Add(id);
            Sitecore.Data.Items.Item item = master.GetItem(new Sitecore.Data.ID(id));
            Sitecore.Data.Items.MediaItem media_item = new Sitecore.Data.Items.MediaItem(item);
            Sitecore.Resources.Media.Media media = Sitecore.Resources.Media.MediaManager.GetMedia(media_item);
            Sitecore.Resources.Media.MediaData media_data = media.MediaData;

            return outlist;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string create_media_item()
        {
            CreatedMediaItem output = new CreatedMediaItem();
            List<HttpPostedFile> files = new List<HttpPostedFile>();
            if (req.Files.Count > 0)
            {
                HttpPostedFile media_file = req.Files["media_file"];
                output.fileName = media_file.FileName;

                var stream = media_file.InputStream;
                using (new Sitecore.SecurityModel.SecurityDisabler())
                {
                    //using (new Sitecore.Data.DatabaseCacheDisabler())
                    //{

                    Sitecore.Resources.Media.MediaCreatorOptions options = new Sitecore.Resources.Media.MediaCreatorOptions();
                    options.Database = master;
                    string itemName = media_file.FileName.Replace(".", "_").Replace("&", "_and_");
                    output.itemName = itemName;
                    options.Destination = req.Params["destination"] + "/" + itemName;

                    //create the item
                    Sitecore.Data.Items.Item media_item = Sitecore.Resources.Media.MediaManager.Creator.CreateFromStream(stream, media_file.FileName, options);
                    output.ID = media_item.ID.ToString();

                    // change the template
                    //string TemplateID = "{16692733-9A61-45E6-B0D4-4C0C06F8DD3C}";
                    //Sitecore.Data.Items.TemplateItem template = master.GetTemplate(new Sitecore.Data.ID(TemplateID));
                    //media_item.ChangeTemplate(template);

                    // edit the fields
                    media_item.Editing.BeginEdit();
                    media_item.Fields["Title"].Value = req.Params["title"];
                    media_item.Editing.EndEdit();

                    //}
                }
            }
            return new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(output);


            /*
            using (new Sitecore.SecurityModel.SecurityDisabler())
            {
                
                string ParentID = "{C2902D58-D0D2-4B6C-B93E-87208CFC2C89}";
                string TemplateID = "{962B53C4-F93B-4DF9-9821-415C867B8903}";
                Sitecore.Data.Items.TemplateItem template = master.GetTemplate(new Sitecore.Data.ID(TemplateID));
                Sitecore.Data.Items.Item parent = master.GetItem(new Sitecore.Data.ID(ParentID));
                Sitecore.Data.Items.Item newitem = parent.Add("foo", template);
                return newitem.ID.ToString();
                
            }
            */
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string upload()
        {
            List<string> item_id_list = new List<string>();
            if (req.Files.Count == 1)
            {
                HttpPostedFile file = req.Files[0];
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
                                        ItemName = Regex.Replace(ItemName, "[^a-zA-Z0-9]+", " ");
                                        try
                                        {
                                            newItem = parent.Add(ItemName.Trim(), template);
                                        }
                                        catch (AccessDeniedException e)
                                        {
                                            return "Failure"; // item_id_list;
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
            return new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(item_id_list);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public Collection<Item> get_items()
        {
            Collection<Item> items = new Collection<Item>();
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
            string id = req.Params["id"];
            Item item = new Item();
            item.id = id;
            item.load();
            return item;
        }
        
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public Item get_item_bypath()
        {
            Sitecore.Data.Items.Item myItem = master.Items[req.Params["path"]];
            return myItem;
        }
        
        [WebMethod]
        public int spiderman()
        {
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
            if (req.Files.Count > 0)
            {
                HttpPostedFile file = HttpContext.Current.Request.Files[0];
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
								foreach (Sitecore.Data.Items.Item child in master.GetItem(itemID).Children)
								{
									foreach (ItemLink link in linkDatabase.GetReferrers(child))
									{
										Item sourceItem = link.GetSourceItem();
										if (sourceItem != null)
										{
											foreach (Item item in sourceItem.Versions.GetVersions(true))
											{
												RemoveLink(item, link);
											}
										}
									}
								}
								foreach (ItemLink link in linkDatabase.GetReferrers(Sitecore.Data.Items.Item item = master.GetItem(itemID)))
								{
									Item sourceItem = link.GetSourceItem();
									if (sourceItem != null)
									{
										foreach (Item item in sourceItem.Versions.GetVersions(true))
										{
											RemoveLink(item, link);
										}
									}
								}
							}
							catch
							{}
                            try
                            {
                                Sitecore.Data.Items.Item item = master.GetItem(itemID);
                                item.Delete();
                                item_id_list.Add(item.ID.ToString());
                            }
                            catch (Exception e)
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

                publisher.Options.RootItem = master.GetItem("/sitecore/content/healthtrust");
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
            Sitecore.Data.Items.Item item = master.GetItem(new Sitecore.Data.ID(this.id));
            this.name = item.Name;
            foreach (Sitecore.Data.Fields.Field field in item.Fields)
            {
                Field ff = new Field(field.Name, field.Value, field.Type);
                this.fields.Add(ff);
            }
        }
    }

    public class CreatedMediaItem
    {
        public string fileName { get; set; }
        public string itemName { get; set; }
        public string ID { get; set; }
    }

    [Serializable]
    public class CreatedItem
    {
        public List<string> ID { get; set; }
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
