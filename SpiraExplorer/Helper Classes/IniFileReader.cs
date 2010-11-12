using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Xml;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio
{
	public partial class Connect
	{
		internal class SettingsClass
		{
			enum IniItemTypeEnum
			{
				GetKeys = 0,
				GetValues = 1,
				GetKeysAndValues = 2
			}

			public class XMLFileReader
			{
				private XmlDocument m_XmlDoc;
				private ArrayList unattachedComments = new ArrayList();
				private StringCollection sections = new StringCollection();
				private bool m_CaseSensitive = false;
				private String m_SaveFilename;

				public XMLFileReader(String SettingsFile)
				{
					this.m_SaveFilename = SettingsFile;
					initXMLFileReader();
				}
				public XMLFileReader(String SettingsFile, bool IsCaseSensitive)
				{
					this.m_SaveFilename = SettingsFile;
					initXMLFileReader();
				}

				private void initXMLFileReader()
				{
					m_XmlDoc = new XmlDocument();
					try
					{
						m_XmlDoc.Load(this.OutputFilename);
						this.UpdateSections();
					}
					catch
					{
						m_XmlDoc.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?><sections></sections>");
						FileInfo fi = new FileInfo(this.OutputFilename);
						if (fi.Exists)
						{
							//Error was thrown. Delete the file and create the new one.
							fi.Delete();
						}
						if (!Directory.Exists(this.m_SaveFilename))
							Directory.CreateDirectory(Path.GetDirectoryName(this.m_SaveFilename));
						m_XmlDoc.Save(this.OutputFilename);
					}

				}

				public bool CaseSensitive
				{
					get
					{
						return m_CaseSensitive;
					}
				}

				private String SetNameCase(String aName)
				{
					if (CaseSensitive)
					{
						return aName;
					}
					else
					{
						return aName.ToLower();
					}
				}

				private XmlElement GetRoot()
				{
					return m_XmlDoc.DocumentElement;
				}

				private XmlElement GetLastSection()
				{
					if (sections.Count == 0)
					{
						return GetRoot();
					}
					else
					{
						return GetSection(sections[sections.Count - 1]);
					}
				}

				private XmlElement GetSection(String sectionName)
				{
					if ((sectionName != null) && (sectionName != ""))
					{
						sectionName = SetNameCase(sectionName);
						return ((XmlElement)m_XmlDoc.SelectSingleNode("//section[@name='" + sectionName + "']"));
					}
					return null;
				}

				private XmlElement GetItem(String sectionName, String keyName)
				{
					if ((keyName != null) && (keyName != ""))
					{
						keyName = SetNameCase(keyName);
						XmlElement section = GetSection(sectionName);
						if (section != null)
						{
							return (XmlElement)section.SelectSingleNode("item[@key='" + keyName + "']");
						}
					}
					return null;
				}

				public bool SetSection(String oldSection, String newSection)
				{
					if ((newSection != null) && (newSection != ""))
					{
						XmlElement section = GetSection(oldSection);
						if (section != null)
						{
							section.SetAttribute("name", SetNameCase(newSection));
							UpdateSections();
							return true;
						}
					}
					return false;
				}

				public bool SetValue(String sectionName, String keyName, String newValue)
				{
					XmlElement item = null;
					XmlElement section = null;
					section = GetSection(sectionName);
					if (section == null)
					{
						if (CreateSection(sectionName))
						{
							section = GetSection(sectionName);
							// exit if keyName is null or blank
							if ((keyName == null) || (keyName == ""))
							{
								return true;
							}
						}
						else
						{
							// can't create section
							return false;
						}
					}
					if (keyName == null)
					{
						// delete the section
						return DeleteSection(sectionName);
					}
					item = GetItem(sectionName, keyName);
					if (item != null)
					{
						if (newValue == null)
						{
							// delete this item
							return DeleteItem(sectionName, keyName);
						}
						else
						{
							// add or update the value attribute
							item.SetAttribute("value", newValue);
							return true;
						}
					}
					else
					{
						// try to create the item
						if ((keyName != "") && (newValue != null))
						{
							// construct a new item (blank values are OK)
							item = m_XmlDoc.CreateElement("item");
							item.SetAttribute("key", SetNameCase(keyName));
							item.SetAttribute("value", newValue);
							section.AppendChild(item);
							return true;
						}
					}
					return false;
				}

				private bool DeleteSection(String sectionName)
				{
					XmlElement section = null;
					if ((section = GetSection(sectionName)) != null)
					{
						section.ParentNode.RemoveChild(section);
						this.UpdateSections();
						return true;
					}
					return false;
				}

				private bool DeleteItem(String sectionName, String keyName)
				{
					XmlElement item = null;
					if ((item = GetItem(sectionName, keyName)) != null)
					{
						item.ParentNode.RemoveChild(item);
						return true;
					}
					return false;
				}

				public bool SetKey(String sectionName, String keyName, String newValue)
				{
					XmlElement item = GetItem(sectionName, keyName);
					if (item != null)
					{
						item.SetAttribute("key", SetNameCase(newValue));
						return true;
					}
					return false;
				}

				public String GetValue(String sectionName, String keyName)
				{
					XmlNode N = GetItem(sectionName, keyName);
					if (N != null)
					{
						return (N.Attributes.GetNamedItem("value").Value);
					}
					return null;
				}

				private void UpdateSections()
				{
					sections = new StringCollection();
					foreach (XmlElement N in m_XmlDoc.SelectNodes("sections/section"))
					{
						sections.Add(N.GetAttribute("name"));
					}
				}

				public StringCollection AllSections
				{
					get
					{
						return sections;
					}
				}

				private StringCollection GetItemsInSection(String sectionName, IniItemTypeEnum itemType)
				{
					XmlNodeList nodes = null;
					StringCollection items = new StringCollection();
					XmlNode section = GetSection(sectionName);
					if (section == null)
					{
						return null;
					}
					else
					{
						nodes = section.SelectNodes("item");
						if (nodes.Count > 0)
						{
							foreach (XmlNode N in nodes)
							{
								switch (itemType)
								{
									case IniItemTypeEnum.GetKeys:
										items.Add(N.Attributes.GetNamedItem("key").Value);
										break;
									case IniItemTypeEnum.GetValues:
										items.Add(N.Attributes.GetNamedItem("value").Value);
										break;
									case IniItemTypeEnum.GetKeysAndValues:
										items.Add(N.Attributes.GetNamedItem("key").Value + "=" +
										   N.Attributes.GetNamedItem("value").Value);
										break;
								}
							}
						}
						return items;
					}
				}

				public StringCollection AllKeysInSection(String sectionName)
				{
					return (GetItemsInSection(sectionName, IniItemTypeEnum.GetKeys));
				}

				public StringCollection AllValuesInSection(String sectionName)
				{
					return (GetItemsInSection(sectionName, IniItemTypeEnum.GetValues));
				}

				public StringCollection AllItemsInSection(String sectionName)
				{
					return (GetItemsInSection(sectionName, IniItemTypeEnum.GetKeysAndValues));
				}

				public string GetCustomAttribute(string sectionName, string keyName, string attributeName)
				{
					if ((attributeName != null) && (attributeName != ""))
					{
						XmlElement N = GetItem(sectionName, keyName);
						if (N != null)
						{
							attributeName = SetNameCase(attributeName);
							return N.GetAttribute(attributeName);
						}
					}
					return null;
				}

				public bool SetCustomAttribute(string sectionName, string keyName, string attributeName, string attributeValue)
				{
					if (attributeName != "")
					{
						XmlElement N = GetItem(sectionName, keyName);
						if (N != null)
						{
							try
							{
								if (attributeValue == null)
								{
									// delete the attribute
									N.RemoveAttribute(attributeName);
									return true;
								}
								else
								{
									attributeName = SetNameCase(attributeName);
									N.SetAttribute(attributeName, attributeValue);
									return true;
								}
							}
							catch { }
						}
					}
					return false;
				}

				private bool CreateSection(String sectionName)
				{
					if ((sectionName != null) && (sectionName != ""))
					{
						sectionName = SetNameCase(sectionName);
						try
						{
							XmlElement N = m_XmlDoc.CreateElement("section");
							XmlAttribute Natt = m_XmlDoc.CreateAttribute("name");
							Natt.Value = SetNameCase(sectionName);
							N.Attributes.SetNamedItem(Natt);
							m_XmlDoc.DocumentElement.AppendChild(N);
							sections.Add(Natt.Value);
							return true;
						}
						catch
						{
							return false;
						}
					}
					return false;
				}

				private bool CreateItem(String sectionName, String keyName, String newValue)
				{
					XmlElement item = null;
					XmlElement section = null;
					try
					{
						if ((section = GetSection(sectionName)) != null)
						{
							item = m_XmlDoc.CreateElement("item");
							item.SetAttribute("key", keyName);
							item.SetAttribute("newValue", newValue);
							section.AppendChild(item);
							return true;
						}
						return false;
					}
					catch
					{
						return false;
					}
				}

				private void ParseLineXml(String s, XmlDocument doc)
				{
					s.TrimStart();
					String key, value;
					XmlElement N;
					XmlAttribute Natt;
					if (s.Length == 0)
					{
						return;
					}
					switch (s.Substring(0, 1))
					{
						case "[":
							// this is a section
							// trim the first and last characters
							s = s.TrimStart('[');
							s = s.TrimEnd(']');
							// create a new section element
							CreateSection(s);
							break;
						case ";":
							// new comment
							N = doc.CreateElement("comment"); // + commentCount.ToString());					
							//commentCount++;
							N.InnerText = s.Substring(1);
							GetLastSection().AppendChild(N);
							break;
						default:
							// split the string on the "=" sign, if present
							if (s.IndexOf('=') > 0)
							{
								String[] parts = s.Split('=');
								key = parts[0].Trim();
								value = parts[1].Trim();
							}
							else
							{
								key = s;
								value = "";
							}
							N = doc.CreateElement("item");
							Natt = doc.CreateAttribute("key");
							Natt.Value = SetNameCase(key);
							N.Attributes.SetNamedItem(Natt);
							Natt = doc.CreateAttribute("value");
							Natt.Value = value;
							N.Attributes.SetNamedItem(Natt);
							GetLastSection().AppendChild(N);
							break;
					}


				}

				public String OutputFilename
				{
					get
					{
						return m_SaveFilename;
					}
				}

				public void Save()
				{
					if ((OutputFilename != null) && (m_XmlDoc != null))
					{
						FileInfo fi = new FileInfo(OutputFilename);
						if (!fi.Directory.Exists)
						{
							return;
						}
						if (fi.Exists)
						{
							fi.Delete();
							m_XmlDoc.Save(OutputFilename);
						}
					}
				}

				public XmlDocument XmlDoc
				{
					get
					{
						return m_XmlDoc;
					}
				}

				public String XML
				{
					get
					{
						StringBuilder sb = new StringBuilder();
						StringWriter sw = new StringWriter(sb);
						XmlTextWriter xw = new XmlTextWriter(sw);
						xw.Indentation = 3;
						xw.Formatting = Formatting.Indented;
						m_XmlDoc.WriteContentTo(xw);
						xw.Close();
						sw.Close();
						return sb.ToString();
					}
				}

			}

		}
	}
}