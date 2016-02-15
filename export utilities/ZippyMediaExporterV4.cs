using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;


/// <summary>
/// Remember to backup first
/// Will export your media items from a Umbraco version 4.11
/// Best solution is to upgrade you older version to version 4.11. 
/// TIP: just use umbracos config files thourout the process cause the script uses sql to get media informations. 
/// See https://github.com/pumpin/zippymedia for more info.
/// </summary>
public class ZippyMediaExporterV4
{

    /// <summary>
    /// Please Set the connection string here. 
    /// </summary>
    public string ConnString = "";


    /// <summary>
    /// Exports the entire media lib via sql instead of the umbraco api. 
    /// During the Export the script will try to write to the response stream if it for some reason cant export the media item. (one reason could be that the file dosnt exist)
    /// When done you wil have a folder struckture tha matches your folder naming in umbraco, with your media items. 
    /// Remeber to set the connection string.
    /// </summary>
    /// <param name="exportFolder">Override the output folder</param>
    public void Export(string exportFolder = "/ZippyExportedMediaLib/")
    {
        string _virtualPath = HttpContext.Current.Server.MapPath(exportFolder);
        var zipRootDir = new DirectoryInfo(_virtualPath);

        var rootItems = GetRootMediaItems();

        foreach (DataRow rootRow in rootItems.Rows)
        {

            if (IsFolder(rootRow["id"].ToString()))
            {
                //Creating folders at root
                var mediaRootDir = zipRootDir.CreateSubdirectory(rootRow["text"].ToString());
                TraverseChildren(rootRow["id"].ToString(), mediaRootDir);

            }
            else
            {
                //Creating media items at root
                string mediaItemPath = "";
                using (SqlConnection con = new SqlConnection(ConnString))
                {
                    con.Open();
                    using (SqlCommand command = new SqlCommand("select contentNodeId, propertytypeid, dataNvarchar from cmsPropertyData where contentnodeid in(select id from umbracoNode where nodeObjectType = 'B796F64C-1F99-4FFB-B886-4BF4BC011A9C') and dataNvarchar like '%/media%' and contentNodeId =" + rootRow["id"], con))
                    {

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                mediaItemPath = reader.GetString(2);
                            }
                        }

                    }
                }


                string fileName = rootRow["text"].ToString();
                string zipFolderPath = zipRootDir.FullName;
                string fileExtension = Path.GetExtension(mediaItemPath);

                string destFilePath = zipFolderPath + @"\" + fileName + fileExtension;

                try
                {
                    string sourceFilePath = HttpContext.Current.Server.MapPath(mediaItemPath);
                    System.IO.File.Copy(sourceFilePath, destFilePath, true);

                }
                catch (Exception exception)
                {
                    HttpContext.Current.Response.Write("------Error Start----");
                    HttpContext.Current.Response.Write("<br> item id found in db : ");
                    HttpContext.Current.Response.Write(rootRow["id"].ToString());
                    HttpContext.Current.Response.Write("<br> item name found ind db : ");
                    HttpContext.Current.Response.Write(rootRow["text"].ToString());
                    HttpContext.Current.Response.Write("<br> destenation path : ");
                    HttpContext.Current.Response.Write(destFilePath);
                    HttpContext.Current.Response.Write("<br><a download='" + rootRow["text"].ToString() + "' href='" + mediaItemPath + "'>Help link</a> : ");

                    HttpContext.Current.Response.Write("------Error End----<br>");
                    HttpContext.Current.Response.Flush();
                }
            }
        }
    }


    private DataTable GetRootMediaItems()
    {
        var dt = new DataTable();
        using (SqlConnection con = new SqlConnection(ConnString))
        {
            con.Open();
            //B796F64C-1F99-4FFB-B886-4BF4BC011A9C is the guid for media item
            using (SqlCommand command = new SqlCommand("select * from umbracoNode where nodeObjectType = 'B796F64C-1F99-4FFB-B886-4BF4BC011A9C' and trashed = 0 and parentId = -1", con))
            {
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                adapter.Fill(dt);
            }
        }
        return dt;
    }

    private void TraverseChildren(string mediaRootId, DirectoryInfo zipRootDir)
    {
        var children = GetChildren(mediaRootId);
        foreach (DataRow child in children.Rows)
        {
            if (IsFolder(child["id"].ToString()))
            {
                var mediaRootDir = zipRootDir.CreateSubdirectory(child["text"].ToString());
                TraverseChildren(child["id"].ToString(), mediaRootDir);
            }
            else
            {
                string mediaItemPath = "";
                using (SqlConnection con = new SqlConnection(ConnString))
                {
                    con.Open();
                    using (SqlCommand command = new SqlCommand("select contentNodeId, propertytypeid, dataNvarchar from cmsPropertyData where contentnodeid in(select id from umbracoNode where nodeObjectType = 'B796F64C-1F99-4FFB-B886-4BF4BC011A9C') and dataNvarchar like '%/media%' and contentNodeId =" + child["id"], con))
                    {

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                mediaItemPath = reader.GetString(2);
                            }
                        }

                    }
                }


                string fileName = child["text"].ToString();
                string zipFolderPath = zipRootDir.FullName;
                string fileExtension = Path.GetExtension(mediaItemPath);

                string destFilePath = zipFolderPath + @"\" + fileName + fileExtension;

                try
                {
                    //try to copy the file over
                    string sourceFilePath = HttpContext.Current.Server.MapPath(mediaItemPath);
                    System.IO.File.Copy(sourceFilePath, destFilePath, true);

                }
                catch (Exception exception)
                {
                    HttpContext.Current.Response.Write("------Error Start----");
                    HttpContext.Current.Response.Write("<br> item id found in db : ");
                    HttpContext.Current.Response.Write(child["id"].ToString());
                    HttpContext.Current.Response.Write("<br> item name found ind db : ");
                    HttpContext.Current.Response.Write(child["text"].ToString());
                    HttpContext.Current.Response.Write("<br> destenation path : ");
                    HttpContext.Current.Response.Write(destFilePath);
                    HttpContext.Current.Response.Write("<br> looking up via umbraco media api : ");
                    HttpContext.Current.Response.Write("<br><a  download='" + child["text"].ToString() + "'  href='" + mediaItemPath + "'>Help link</a> : ");

                    HttpContext.Current.Response.Write("------Error End----<br>");
                    HttpContext.Current.Response.Flush();
                }

            }
        }

    }

    private DataTable GetChildren(string id)
    {
        var dt = new DataTable();
        using (SqlConnection con = new SqlConnection(ConnString))
        {
            con.Open();
            using (SqlCommand command = new SqlCommand("select * from umbracoNode where nodeObjectType = 'B796F64C-1F99-4FFB-B886-4BF4BC011A9C' and trashed = 0 and parentId =" + id, con))
            {
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                adapter.Fill(dt);
            }
        }
        return dt;
    }

    private bool IsFolder(string id)
    {

        object noOfRecords = null;
        using (SqlConnection con = new SqlConnection(ConnString))
        {
            con.Open();
            using (SqlCommand command = new SqlCommand("select contentNodeId, propertytypeid, dataNvarchar from cmsPropertyData where contentnodeid in(select id from umbracoNode where nodeObjectType = 'B796F64C-1F99-4FFB-B886-4BF4BC011A9C') and dataNvarchar like '%/media%' and contentNodeId =" + id, con))
            {
                noOfRecords = command.ExecuteScalar();

            }
        }
        return noOfRecords == null;
    }


}
