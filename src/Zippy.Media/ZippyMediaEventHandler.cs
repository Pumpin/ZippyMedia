using System;
using Umbraco.Core;
using Umbraco.Web.Trees;
using MenuItem = Umbraco.Web.Models.Trees.MenuItem;

namespace Zippy.Media
{
    public class ZippyMediaEventHandler : ApplicationEventHandler
    {
        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            TreeControllerBase.MenuRendering += TreeControllerBase_MenuRendering;
        }

        private void TreeControllerBase_MenuRendering(TreeControllerBase sender, MenuRenderingEventArgs e)
        {
            var selectedMedia = ApplicationContext.Current.Services.MediaService.GetById(Convert.ToInt32(e.NodeId));
            string mediaType = selectedMedia != null ? selectedMedia.ContentType.Alias : string.Empty;
            
            if (sender.TreeAlias == "media" && mediaType.Equals("Folder"))
            {
                var menuItem = new MenuItem("uploadZipFile", "Upload and Unpack ZIP");
                menuItem.Icon = "compress";
                menuItem.SeperatorBefore = true;
                menuItem.LaunchDialogView("/App_Plugins/zippyMedia/Views/Upload-Zip.html", "Upload and Unpack ZIP archive");
                
                e.Menu.Items.Add(menuItem);       
            }


            if (sender.TreeAlias == "media" && Convert.ToInt32(e.NodeId) == Constants.System.Root)
            {
                var menuItem = new MenuItem("uploadZipFile", "Upload and Unpack ZIP");
                menuItem.Icon = "compress";
                menuItem.SeperatorBefore = true;
                menuItem.LaunchDialogView("/App_Plugins/zippyMedia/Views/Upload-Zip.html", "Upload and Unpack ZIP archive");

                e.Menu.Items.Add(menuItem);  

            }



            //Upload from server AKA ftp upload comming soon 
            //if (sender.TreeAlias == "media" && mediaType.Equals("Folder") && sender.Security.CurrentUser.UserType.Alias.Equals("admin"))
            //{
            //    var menuItem = new MenuItem("unpackZipFileFromServer", "Unpack ZIP from server");
            //    menuItem.Icon = "server";
            //    menuItem.SeperatorBefore = false;
            //    menuItem.LaunchDialogView("/App_Plugins/zippyMedia/Views/Unpack-Zip.html", "Unpack ZIP archive from server");

            //    e.Menu.Items.Add(menuItem);
            //}

            //if (sender.TreeAlias == "media" && Convert.ToInt32(e.NodeId) == Constants.System.Root && sender.Security.CurrentUser.UserType.Alias.Equals("admin"))
            //{
            //    var menuItem = new MenuItem("unpackZipFileFromServer", "Unpack ZIP from server");
            //    menuItem.Icon = "server";
            //    menuItem.SeperatorBefore = false;
            //    menuItem.LaunchDialogView("/App_Plugins/zippyMedia/Views/Unpack-Zip.html", "Unpack ZIP archive from server");

            //    e.Menu.Items.Add(menuItem);

            //}
            
        }
    }
}
