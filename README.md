sc_testing_webservice
=====================
This is an ASP.net web service that I created to interact with the Sitecore API in a way that makes sense to me.
For automated testing purposes, we need the ability to create and delete large numbers of content items.   So 
this script has the following methods to facilitate that:

* upload - give an uploaded XML file of item data, this method creates those items and returns the generated IDs
* create_media_item - Given an uploaded media file, and a second uploaded file which is XML representing the media
  item's metadata, this method creates the media item with the metadata
* get_items - Returns the metadata for multiple items, given a colon-separated list of IDs
* get_item - Returns the metadata for a single item, given the ID.
* spiderman - Obviously it was named during a moment of levity.  There's a semi-funny story surrounding the naming of
  this method.
* delete_item - Deletes a single item, given the ID.
* delete - Deletes a list of items, given the IDs in XML
* publish - This just tells Sitecore to publish.  This is a lot faster than publishing through the UI.
* media_item_info - I'm not sure what this is.  I'll put and say it "returns the media item info" :)
