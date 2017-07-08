<%@ WebService Language="C#" Class="translation" %>

using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml;

[WebService(Namespace = "http://www.dndjunkie.com/")]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]
public class translation : System.Web.Services.WebService
{
	private const int MESSAGE_SUCCESS = 0;
	private const int MESSAGE_UNAUTHORIZED = 1;
	private const int MESSAGE_INVALID_FILE = 2;
	
	public translation()
	{
	}
	
	public const string CONNECTION_STRING = "Server=10.10.10.3;Database=DNDJunkie;User ID=dndjunkie;Password=hxcr5k2;Trusted_Connection=False;";

	[WebMethod(Description = "Check to see if the user can upload a Language file.")]
	public bool CanUploadLanguage(string strUserName)
	{
		SqlConnection objConnection = new SqlConnection(CONNECTION_STRING);
		objConnection.Open();

		SqlCommand objCommand = new SqlCommand();
		objCommand.Connection = objConnection;

		SqlDataReader objReader;

		// Make sure the user is allowed to upload the file.
		if (strUserName != "Nebular")
		{
			objCommand.CommandText = "SELECT * FROM tblOmaeUserLanguagePermission WHERE strUserName = '" + SQLSafe(strUserName) + "'";
			objReader = objCommand.ExecuteReader();
			if (!objReader.Read())
			{
				objReader.Close();
				return false;
			}
			else
			{
				objReader.Close();
				return true;
			}
		}
		else
			return true;
	}

	[WebMethod(Description = "Upload a Language file.")]
	public int UploadLanguage(string strUserName, string strFileName, byte[] bytFile)
	{
		SqlConnection objConnection = new SqlConnection(CONNECTION_STRING);
		objConnection.Open();

		SqlCommand objCommand = new SqlCommand();
		objCommand.Connection = objConnection;

		SqlDataReader objReader;

		// Make sure the user is allowed to upload the file.
		if (strUserName != "Nebular")
		{
			objCommand.CommandText = "SELECT * FROM tblOmaeUserLanguagePermission WHERE strUserName = '" + SQLSafe(strUserName) + "' AND strFileName = '" + SQLSafe(strFileName) + "'";
			objReader = objCommand.ExecuteReader();
			if (!objReader.Read())
			{
				objReader.Close();
				return MESSAGE_UNAUTHORIZED;
			}
			else
				objReader.Close();
		}

		// Determine the current version number from the database.
		int intCurrentVersion = -1000;
		string strDescription = "";
		objCommand.CommandText = "SELECT * FROM tblLanguage WHERE strFile = '" + SQLSafe(strFileName) + "'";
		objReader = objCommand.ExecuteReader();
		if (objReader.Read())
		{
			intCurrentVersion = Convert.ToInt32(objReader["intVersion"].ToString());
			strDescription = objReader["strDescription"].ToString();
		}
		else
		{
			objReader.Close();
			return MESSAGE_INVALID_FILE;
		}
		objReader.Close();

		// Change the version number of the file that was just uploaded.
		intCurrentVersion++;

		MemoryStream objStream = new MemoryStream();
		objStream.Write(bytFile, 0, bytFile.Length);
		objStream.Position = 0;
		
		XmlDocument objDocument = XmlDocumentFromStream(objStream);
		objDocument.SelectSingleNode("/chummer/version").InnerText = intCurrentVersion.ToString();

		// Write the updated file to the file system.
		string strSavePath = Path.Combine(Path.Combine(Server.MapPath("/dev/chummer"), "lang"), FileSafe(strFileName));
		FileStream objFileStream = new FileStream(strSavePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
		XmlTextWriter objWriter = new XmlTextWriter(objFileStream, System.Text.Encoding.UTF8);
		objWriter.Formatting = Formatting.Indented;
		objWriter.Indentation = 1;
		objWriter.IndentChar = '\t';
		objDocument.WriteContentTo(objWriter);
		objWriter.Close();
		
		// Update the language manifest.
		string strManifestPath = Path.Combine(Server.MapPath("/dev/chummer"), "manifestlang.xml");
		objDocument = new XmlDocument();
		objDocument.Load(strManifestPath);
		XmlNode objNode = objDocument.SelectSingleNode("/manifest/file[name = 'lang/" + strFileName + "']");
		if (objNode != null)
		{
			objDocument.SelectSingleNode("/manifest/file[name = 'lang/" + strFileName + "']/version").InnerText = intCurrentVersion.ToString();
		}
		else
		{
			XmlNode objFile = objDocument.CreateElement("file");
			XmlNode objName = objDocument.CreateElement("name");
			objName.InnerText = "lang/" + strFileName;
			XmlNode objType = objDocument.CreateElement("type");
			objType.InnerText = "Language File";
			XmlNode objVersion = objDocument.CreateElement("version");
			objVersion.InnerText = intCurrentVersion.ToString();
			XmlNode objDescription = objDocument.CreateElement("description");
			objDescription.InnerText = strDescription;
			XmlNode objNotes = objDocument.CreateElement("notes");
			objNotes.InnerText = strDescription;
			objFile.AppendChild(objName);
			objFile.AppendChild(objType);
			objFile.AppendChild(objVersion);
			objFile.AppendChild(objDescription);
			objFile.AppendChild(objNotes);
			objDocument.DocumentElement.AppendChild(objFile);
		}
		
		// Save the updated language manifest.
		objFileStream = new FileStream(strManifestPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
		objWriter = new XmlTextWriter(objFileStream, System.Text.Encoding.UTF8);
		objWriter.Formatting = Formatting.Indented;
		objWriter.Indentation = 1;
		objWriter.IndentChar = '\t';
		objDocument.WriteContentTo(objWriter);
		objWriter.Close();
		
		// Update the database version number.
		objCommand.CommandText = "UPDATE tblLanguage SET intVersion = " + intCurrentVersion.ToString() + " WHERE strFile = '" + SQLSafe(strFileName) + "'";
		objCommand.ExecuteNonQuery();

		return MESSAGE_SUCCESS;
	}

	private string SQLSafe(string strValue)
	{
		string strReturn = strValue;
		strReturn = strReturn.Replace("'", "''");
		return strReturn;
	}
	
	private string FileSafe(string strValue)
	{
		string strReturn = strValue;
		strReturn = strReturn.Replace(" ", "_");
		strReturn = strReturn.Replace("\\", "");
		strReturn = strReturn.Replace("/", "");
		strReturn = strReturn.Replace(":", "");
		strReturn = strReturn.Replace("*", "");
		strReturn = strReturn.Replace("?", "");
		strReturn = strReturn.Replace("<", "");
		strReturn = strReturn.Replace(">", "");
		strReturn = strReturn.Replace("|", "");
		return strReturn;
	}
	
	/// <summary>
	/// Write the contents of a MemoryStream to an XmlDocument.
	/// </summary>
	/// <param name="objStream">MemoryStream to read.</param>
	private XmlDocument XmlDocumentFromStream(MemoryStream objStream)
	{
		string strXml = "";
		objStream.Position = 0;
		StreamReader objReader = new StreamReader(objStream);
		strXml = objReader.ReadToEnd();

		XmlDocument objXmlDocument = new XmlDocument();
		objXmlDocument.LoadXml(strXml);

		return objXmlDocument;
	}
} 