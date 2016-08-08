/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Windows.Forms;

namespace Chummer
{
	public sealed class XmlManager
	{
		/// <summary>
		/// Used to cache XML files so that they do not need to be loaded and translated each time an object wants the file.
		/// </summary>
		private class XmlReference
		{
			private DateTime _datDate = new DateTime();
			private string _strFileName = "";
			private XmlDocument _objXmlDocument = new XmlDocument();

			/// <summary>
			/// Date/Time stamp on the XML file.
			/// </summary>
			public DateTime FileDate
			{
				get
				{
					return _datDate;
				}
				set
				{
					_datDate = value;
				}
			}

			/// <summary>
			/// Name of the XML file.
			/// </summary>
			public string FileName
			{
				get
				{
					return _strFileName;
				}
				set
				{
					_strFileName = value;
				}
			}

			/// <summary>
			/// XmlDocument that is created by merging the base data file and data translation file. Does not include custom content since this must be loaded each time.
			/// </summary>
			public XmlDocument XmlContent
			{
				get
				{
					return _objXmlDocument;
				}
				set
				{
					_objXmlDocument = value;
				}
			}
		}
		
		static readonly XmlManager _objInstance = new XmlManager();
		static private readonly List<XmlReference> _lstXmlDocuments = new List<XmlReference>();

		#region Constructor and Instance
		static XmlManager()
		{
			LanguageManager.Instance.Load(GlobalOptions.Instance.Language, null);
		}

		XmlManager()
		{
		}

		/// <summary>
		/// Glboal XmlManager instance.
		/// </summary>
		public static XmlManager Instance
		{
			get
			{
				return _objInstance;
			}
		}
		#endregion

		#region Methods
		/// <summary>
		/// Load the selected XML file and its associated custom file.
		/// </summary>
		/// <param name="strFileName">Name of the XML file to load.</param>
		public XmlDocument Load(string strFileName)
		{
			string strPath = Path.Combine(Application.StartupPath, "data", strFileName);
			DateTime datDate = File.GetLastWriteTime(strPath);

			// Look to see if this XmlDocument is already loaded.
			bool blnFound = false;
			XmlReference objReference = new XmlReference();
			foreach (XmlReference objCurrentReference in _lstXmlDocuments)
			{
				if (objCurrentReference.FileName == strFileName)
				{
					objReference = objCurrentReference;
					blnFound = true;
					break;
				}
			}

			bool blnLoadFile = false;
			if (!blnFound)
			{
				// The file was not found in the reference list, so it must be loaded.
				blnLoadFile = true;
				_lstXmlDocuments.Add(objReference);
			}
			else
			{
				// The file was found in the List, so check the last write time.
				if (datDate != objReference.FileDate)
				{
					// The last write time does not match, so it must be reloaded.
					blnLoadFile = true;
				}
			}

			// Create a new document that everything will be merged into.
			XmlDocument objDoc = new XmlDocument();
			// write the root chummer node.
			XmlNode objCont = objDoc.CreateElement("chummer");
			objDoc.AppendChild(objCont);

			XmlDocument objXmlFile = new XmlDocument();
			XmlNodeList objList;

			if (blnLoadFile)
			{
				// Load the base file and retrieve all of the child nodes.
				objXmlFile.Load(strPath);
				objList = objXmlFile.SelectNodes("/chummer/*");
				foreach (XmlNode objNode in objList)
				{
					// Append the entire child node to the new document.
					XmlNode objImported = objDoc.ImportNode(objNode, true);
					objDoc.DocumentElement.AppendChild(objImported);
				}

				// Load any override data files the user might have. Do not attempt this if we're loading the Improvements file.
				if (strFileName != "improvements.xml")
				{
					string strFilePath = Path.Combine(Application.StartupPath, "data");
					foreach (string strFile in Directory.GetFiles(strFilePath, "override*_" + strFileName))
					{
						objXmlFile.Load(strFile);
						objList = objXmlFile.SelectNodes("/chummer/*");
						foreach (XmlNode objNode in objList)
						{
							foreach (XmlNode objType in objNode.ChildNodes)
							{
								if (objType["id"] != null)
								{
									XmlNode objItem = objDoc.SelectSingleNode("/chummer/" + objNode.Name + "/" + objType.Name + "[id = \"" + objType["id"].InnerText.Replace("&amp;", "&") + "\"]");
									if (objItem != null)
										objItem.InnerXml = objType.InnerXml;
								}
								else if (objType["name"] != null)
								{
									XmlNode objItem = objDoc.SelectSingleNode("/chummer/" + objNode.Name + "/" + objType.Name + "[name = \"" + objType["name"].InnerText.Replace("&amp;", "&") + "\"]");
									if (objItem != null)
										objItem.InnerXml = objType.InnerXml;
								}
							}
						}
					}
				}

				// Load the translation file for the current base data file if the selected language is not en-us.
				if (GlobalOptions.Instance.Language != "en-us")
				{
					// Everything is stored in the selected language file to make translations easier, keep all of the language-specific information together, and not require users to download 27 individual files.
					// The structure is similar to the base data file, but the root node is instead a child /chummer node with a file attribute to indicate the XML file it translates.
					if (LanguageManager.Instance.DataDoc != null)
					{
						foreach (XmlNode objNode in LanguageManager.Instance.DataDoc.SelectNodes("/chummer/chummer[@file = \"" + strFileName + "\"]"))
						{
							foreach (XmlNode objType in objNode.ChildNodes)
							{
								foreach (XmlNode objChild in objType.ChildNodes)
								{
									// If this is a translatable item, find the proper node and add/update this information.
									if (objChild["translate"] != null)
									{
										XmlNode objItem = objDoc.SelectSingleNode("/chummer/" + objType.Name + "/" + objChild.Name + "[name = \"" + objChild["name"].InnerXml.Replace("&amp;", "&") + "\"]");
										if (objItem != null)
											objItem.InnerXml += "<translate>" + objChild["translate"].InnerXml + "</translate>";
									}
									if (objChild["page"] != null)
									{
										XmlNode objItem = objDoc.SelectSingleNode("/chummer/" + objType.Name + "/" + objChild.Name + "[name = \"" + objChild["name"].InnerXml.Replace("&amp;", "&") + "\"]");
										if (objItem != null)
											objItem.InnerXml += "<altpage>" + objChild["page"].InnerXml + "</altpage>";
									}
									if (objChild["code"] != null)
									{
										XmlNode objItem = objDoc.SelectSingleNode("/chummer/" + objType.Name + "/" + objChild.Name + "[name = \"" + objChild["name"].InnerXml.Replace("&amp;", "&") + "\"]");
										if (objItem != null)
											objItem.InnerXml += "<altcode>" + objChild["code"].InnerXml + "</altcode>";
									}
									if (objChild["advantage"] != null)
									{
										XmlNode objItem = objDoc.SelectSingleNode("/chummer/" + objType.Name + "/" + objChild.Name + "[name = \"" + objChild["name"].InnerXml.Replace("&amp;", "&") + "\"]");
										if (objItem != null)
											objItem.InnerXml += "<altadvantage>" + objChild["advantage"].InnerXml + "</altadvantage>";
									}
									if (objChild["disadvantage"] != null)
									{
										XmlNode objItem = objDoc.SelectSingleNode("/chummer/" + objType.Name + "/" + objChild.Name + "[name = \"" + objChild["name"].InnerXml.Replace("&amp;", "&") + "\"]");
										if (objItem != null)
											objItem.InnerXml += "<altdisadvantage>" + objChild["disadvantage"].InnerXml + "</altdisadvantage>";
									}
									if (objChild.Attributes != null)
									{
										// Handle Category name translations.
										if (objChild.Attributes["translate"] != null)
										{
											XmlNode objItem = objDoc.SelectSingleNode("/chummer/" + objType.Name + "/" + objChild.Name + "[. = \"" + objChild.InnerXml.Replace("&amp;", "&") + "\"]");
											if (objItem != null)
											{
												XmlElement objElement = (XmlElement)objItem;
												objElement.SetAttribute("translate", objChild.Attributes["translate"].InnerXml);
											}
										}
									}

									// Check for Skill Specialization information.
									if (strFileName == "skills.xml")
									{
										if (objChild["specs"] != null)
										{
											foreach (XmlNode objSpec in objChild.SelectNodes("specs/spec"))
											{
												if (objSpec.Attributes["translate"] != null)
												{
													XmlNode objItem = objDoc.SelectSingleNode("/chummer/" + objType.Name + "/skill[name = \"" + objChild["name"].InnerXml + "\"]/specs/spec[. = \"" + objSpec.InnerXml + "\"]");
													if (objItem != null)
													{
														XmlElement objElement = (XmlElement)objItem;
														objElement.SetAttribute("translate", objSpec.Attributes["translate"].InnerXml);
													}
												}
											}
										}
									}

									// Check for Metavariant information.
									if (strFileName == "metatypes.xml")
									{
										if (objChild["metavariants"] != null)
										{
											foreach (XmlNode objMetavariant in objChild.SelectNodes("metavariants/metavariant"))
											{
												if (objMetavariant["translate"] != null)
												{
													XmlNode objItem = objDoc.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + objChild["name"].InnerXml + "\"]/metavariants/metavariant[name = \"" + objMetavariant["name"].InnerXml + "\"]");
													if (objItem != null)
														objItem.InnerXml += "<translate>" + objMetavariant["translate"].InnerXml + "</translate>";
												}
												if (objMetavariant["altpage"] != null)
												{
													XmlNode objItem = objDoc.SelectSingleNode("/chummer/metatypes/metatype[name = \"" + objChild["name"].InnerXml + "\"]/metavariants/metavariant[name = \"" + objMetavariant["name"].InnerXml + "\"]");
													if (objItem != null)
														objItem.InnerXml += "<altpage>" + objMetavariant["page"].InnerXml + "</altpage>";
												}
											}
										}
									}

									// Check for Martial Art Advantage information.
									if (strFileName == "martialarts.xml")
									{
										if (objChild["advantages"] != null)
										{
                                            foreach (XmlNode objAdvantage in objChild.SelectNodes("techniques/technique"))
											{
												if (objAdvantage.Attributes["translate"] != null)
												{
                                                    XmlNode objItem = objDoc.SelectSingleNode("/chummer/martialarts/martialart[name = \"" + objChild["name"].InnerXml + "\"]/techniques/technique[. = \"" + objAdvantage.InnerXml + "\"]");
													if (objItem != null)
													{
														XmlElement objElement = (XmlElement)objItem;
														objElement.SetAttribute("translate", objAdvantage.Attributes["translate"].InnerXml);
													}
												}
											}
										}
									}

									// Check for Mentor Spirit/Paragon choice information.
									if (strFileName == "mentors.xml" || strFileName == "paragons.xml")
									{
										if (objChild["choices"] != null)
										{
											foreach (XmlNode objChoice in objChild.SelectNodes("choices/choice"))
											{
												if (objChoice["translate"] != null)
												{
													XmlNode objItem = objDoc.SelectSingleNode("/chummer/mentors/mentor[name = \"" + objChild["name"].InnerXml + "\"]/choices/choice[name = \"" + objChoice["name"].InnerXml + "\"]");
													if (objItem != null)
														objItem.InnerXml += "<translate>" + objChoice["translate"].InnerXml + "</translate>";
												}
											}
										}
									}
								}
							}
						}
					}
				}

				// Cache the merged document and its relevant information.
				objReference.FileDate = datDate;
				objReference.FileName = strFileName;
				objReference.XmlContent = objDoc;
			}
			else
			{
				// Pull the document from cache.
				objDoc = objReference.XmlContent;
			}

			// A new XmlDocument is created by loading the a copy of the cached one so that we don't stuff custom content into the cached copy
			// (which we don't want and also results in multiple copies of each custom item).
			XmlDocument objReturnDocument = new XmlDocument();
			objReturnDocument.LoadXml(objDoc.OuterXml);

			// Load any custom data files the user might have. Do not attempt this if we're loading the Improvements file.
			if (strFileName != "improvements.xml")
			{
				strPath = Path.Combine(Application.StartupPath, "data");
				foreach (string strFile in Directory.GetFiles(strPath, "custom*_" + strFileName))
				{
					objXmlFile.Load(strFile);
					objList = objXmlFile.SelectNodes("/chummer/*");
					foreach (XmlNode objNode in objList)
					{
						// Look for any items with a duplicate name and pluck them from the node so we don't end up with multiple items with the same name.
						List<XmlNode> lstDelete = new List<XmlNode>();
						foreach (XmlNode objChild in objNode.ChildNodes)
						{
							// Only do this if the child has the name field since this is what we must match on.
							if (objChild["name"] != null)
							{
								XmlNodeList objNodeList = objReturnDocument.SelectNodes("/chummer/" + objChild.ParentNode.Name + "/" + objChild.Name + "[name = \"" + objChild["name"].InnerText + "\"]");
								if (objNodeList.Count > 0)
								{
									lstDelete.Add(objChild);
								}
							}
						}
						// Remove the offending items from the node we're about to merge in.
						foreach (XmlNode objRemoveNode in lstDelete)
						{
							objNode.RemoveChild(objRemoveNode);
						}

						// Append the entire child node to the new document.
						XmlNode objImported = objReturnDocument.ImportNode(objNode, true);
						objReturnDocument.DocumentElement.AppendChild(objImported);
					}
				}
			}

			return objReturnDocument;
		}

		/// <summary>
		/// Verify the contents of the language data translation file.
		/// </summary>
		/// <param name="strLanguage">Language to check.</param>
		/// <param name="lstBooks">List of books.</param>
		public void Verify(string strLanguage, List<string> lstBooks)
		{
			XmlDocument objLanguageDoc = new XmlDocument();
			string languageDirectoryPath = Path.Combine(Application.StartupPath, "lang");
            string strFilePath = Path.Combine(languageDirectoryPath, strLanguage + "_data.xml");
			objLanguageDoc.Load(strFilePath);

			string strLangPath = Path.Combine(languageDirectoryPath, "results_" + strLanguage + ".xml");
			FileStream objStream = new FileStream(strLangPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
			XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.Unicode);
			objWriter.Formatting = Formatting.Indented;
			objWriter.Indentation = 1;
			objWriter.IndentChar = '\t';

			objWriter.WriteStartDocument();
			// <results>
			objWriter.WriteStartElement("results");

			string strPath = Path.Combine(Application.StartupPath, "data");
			foreach (string strFile in Directory.GetFiles(strPath, "*.xml"))
			{
				string strFileName = Path.GetFileName(strFile);

				// Do not bother to check custom files.
				if (!strFileName.StartsWith("custom") && !strFile.StartsWith("override") && !strFile.Contains("packs.xml") && !strFile.Contains("ranges.xml"))
				{
					// Load the current English file.
					XmlDocument objEnglishDoc = new XmlDocument();
					objEnglishDoc = Load(strFileName);
					XmlNode objEnglishRoot = objEnglishDoc.SelectSingleNode("/chummer");

					// First pass: make sure the document exists.
					bool blnExists = false;
					XmlNode objLanguageRoot = objLanguageDoc.SelectSingleNode("/chummer/chummer[@file = \"" + strFileName + "\"]");
					if (objLanguageRoot != null)
						blnExists = true;

					// <file name="x" exists="y">
					objWriter.WriteStartElement("file");
					objWriter.WriteAttributeString("name", strFileName);
					objWriter.WriteAttributeString("exists", blnExists.ToString());

					if (blnExists)
					{
						foreach (XmlNode objType in objEnglishRoot.ChildNodes)
						{
							objWriter.WriteStartElement(objType.Name);
							foreach (XmlNode objChild in objType.ChildNodes)
							{
								// If the Node has a source element, check it and see if it's in the list of books that were specified.
								// This is done since not all of the books are available in every language or the user may only wish to verify the content of certain books.
								bool blnContinue = false;
								if (objChild["source"] != null)
								{
									foreach (string strBook in lstBooks)
									{
										if (strBook == objChild["source"].InnerText)
										{
											blnContinue = true;
											break;
										}
									}
								}
								else
									blnContinue = true;

								if (blnContinue)
								{
									if (objType.Name != "version" && !((objType.Name == "costs" || objType.Name == "safehousecosts") && strFile.EndsWith("lifestyles.xml")))
									{
										// Look for a matching entry in the Language file.
										if (objChild["name"] != null)
										{
											XmlNode objNode = objLanguageRoot.SelectSingleNode(objType.Name + "/" + objChild.Name + "[name = \"" + objChild["name"].InnerText + "\"]");
											if (objNode != null)
											{
												// A match was found, so see what elements, if any, are missing.
												bool blnTranslate = false;
												bool blnAltPage = false;
												bool blnAdvantage = false;
												bool blnDisadvantage = false;

												if (objChild.HasChildNodes)
												{
													if (objNode["translate"] != null)
														blnTranslate = true;

													// Do not mark page as missing if the original does not have it.
													if (objChild["page"] != null)
													{
														if (objNode["page"] != null)
															blnAltPage = true;
													}
													else
														blnAltPage = true;

													if (strFile.EndsWith("mentors.xml") || strFile.EndsWith("paragons.xml"))
													{
														if (objNode["advantage"] != null)
															blnAdvantage = true;
														if (objNode["disadvantage"] != null)
															blnDisadvantage = true;
													}
													else
													{
														blnAdvantage = true;
														blnDisadvantage = true;
													}
												}
												else
												{
													blnAltPage = true;
													if (objNode.Attributes["translate"] != null)
														blnTranslate = true;
												}

												// At least one pice of data was missing so write out the result node.
												if (!blnTranslate || !blnAltPage || !blnAdvantage || !blnDisadvantage)
												{
													// <results>
													objWriter.WriteStartElement(objChild.Name);
													objWriter.WriteAttributeString("exists", "True");
													objWriter.WriteElementString("name", objChild["name"].InnerText);
													if (!blnTranslate)
														objWriter.WriteElementString("missing", "translate");
													if (!blnAltPage)
														objWriter.WriteElementString("missing", "page");
													if (!blnAdvantage)
														objWriter.WriteElementString("missing", "advantage");
													if (!blnDisadvantage)
														objWriter.WriteElementString("missing", "disadvantage");
													// </results>
													objWriter.WriteEndElement();
												}
											}
											else
											{
												// No match was found, so write out that the data item is missing.
												// <result>
												objWriter.WriteStartElement(objChild.Name);
												objWriter.WriteAttributeString("exists", "False");
												objWriter.WriteElementString("name", objChild["name"].InnerText);
												// </result>
												objWriter.WriteEndElement();
											}

											if (strFileName == "metatypes.xml")
											{
												if (objChild["metavariants"] != null)
												{
													foreach (XmlNode objMetavariant in objChild.SelectNodes("metavariants/metavariant"))
													{
														XmlNode objTranslate = objLanguageRoot.SelectSingleNode("metatypes/metatype[name = \"" + objChild["name"].InnerText + "\"]/metavariants/metavariant[name = \"" + objMetavariant["name"].InnerText + "\"]");
														if (objTranslate != null)
														{
															bool blnTranslate = false;
															bool blnAltPage = false;

															if (objTranslate["translate"] != null)
																blnTranslate = true;
															if (objTranslate["page"] != null)
																blnAltPage = true;

															// Item exists, so make sure it has its translate attribute populated.
															if (!blnTranslate || !blnAltPage)
															{
																// <result>
																objWriter.WriteStartElement("metavariants");
																objWriter.WriteStartElement("metavariant");
																objWriter.WriteAttributeString("exists", "True");
																objWriter.WriteElementString("name", objMetavariant["name"].InnerText);
																if (!blnTranslate)
																	objWriter.WriteElementString("missing", "translate");
																if (!blnAltPage)
																	objWriter.WriteElementString("missing", "page");
																objWriter.WriteEndElement();
																// </result>
																objWriter.WriteEndElement();
															}
														}
														else
														{
															// <result>
															objWriter.WriteStartElement("metavariants");
															objWriter.WriteStartElement("metavariant");
															objWriter.WriteAttributeString("exists", "False");
															objWriter.WriteElementString("name", objMetavariant.InnerText);
															objWriter.WriteEndElement();
															// </result>
															objWriter.WriteEndElement();
														}
													}
												}
											}

											if (strFile == "martialarts.xml")
											{
												if (objChild["advantages"] != null)
												{
                                                    foreach (XmlNode objAdvantage in objChild.SelectNodes("techniques/technique"))
													{
                                                        XmlNode objTranslate = objLanguageRoot.SelectSingleNode("martialarts/martialart[name = \"" + objChild["name"].InnerText + "\"]/techniques/technique[. = \"" + objAdvantage.InnerText + "\"]");
														if (objTranslate != null)
														{
															// Item exists, so make sure it has its translate attribute populated.
															if (objTranslate.Attributes["translate"] == null)
															{
																// <result>
																objWriter.WriteStartElement("martialarts");
																objWriter.WriteStartElement("advantage");
																objWriter.WriteAttributeString("exists", "True");
																objWriter.WriteElementString("name", objAdvantage.InnerText);
																objWriter.WriteElementString("missing", "translate");
																objWriter.WriteEndElement();
																// </result>
																objWriter.WriteEndElement();
															}
														}
														else
														{
															// <result>
															objWriter.WriteStartElement("martialarts");
															objWriter.WriteStartElement("advantage");
															objWriter.WriteAttributeString("exists", "False");
															objWriter.WriteElementString("name", objAdvantage.InnerText);
															objWriter.WriteEndElement();
															// </result>
															objWriter.WriteEndElement();
														}
													}
												}
											}
										}
										else if (objChild.Name == "#comment")
										{
											//Ignore this node, as it's a comment node.
										}
										else if (objChild.InnerText != null)
										{
											// The item does not have a name which means it should have a translate CharacterAttribute instead.
											XmlNode objNode =
												objLanguageRoot.SelectSingleNode(objType.Name + "/" + objChild.Name + "[. = \"" + objChild.InnerText + "\"]");
											if (objNode != null)
											{
												// Make sure the translate attribute is populated.
												if (objNode.Attributes["translate"] == null)
												{
													// <result>
													objWriter.WriteStartElement(objChild.Name);
													objWriter.WriteAttributeString("exists", "True");
													objWriter.WriteElementString("name", objChild.InnerText);
													objWriter.WriteElementString("missing", "translate");
													// </result>
													objWriter.WriteEndElement();
												}
											}
											else
											{
												// No match was found, so write out that the data item is missing.
												// <result>
												objWriter.WriteStartElement(objChild.Name);
												objWriter.WriteAttributeString("exists", "False");
												objWriter.WriteElementString("name", objChild.InnerText);
												// </result>
												objWriter.WriteEndElement();
											}
										}
									}
								}
							}
							objWriter.WriteEndElement();
						}

						// Now loop through the translation file and determine if there are any entries in there that are not part of the base content.
						foreach (XmlNode objType in objLanguageRoot.ChildNodes)
						{
							foreach (XmlNode objChild in objType.ChildNodes)
							{
								// Look for a matching entry in the English file.
								if (objChild["name"] != null)
								{
									XmlNode objNode = objEnglishRoot.SelectSingleNode("/chummer/" + objType.Name + "/" + objChild.Name + "[name = \"" + objChild["name"].InnerText + "\"]");
									if (objNode == null)
									{
										// <noentry>
										objWriter.WriteStartElement("noentry");
										objWriter.WriteStartElement(objChild.Name);
										objWriter.WriteElementString("name", objChild["name"].InnerText);
										objWriter.WriteEndElement();
										// </noentry>
										objWriter.WriteEndElement();
									}
								}
							}
						}
					}

					// </file>
					objWriter.WriteEndElement();
				}
			}

			// </results>
			objWriter.WriteEndElement();
			objWriter.WriteEndDocument();
			objWriter.Close();
			objStream.Close();
		}
		#endregion
	}
}