// .NET
using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Windows;
using System.Xml;

namespace XUtils.Xml
{
	// serialization 'Class' <-> 'Xml ...'
	internal class XmlSerialization
	{
		public enum EnumParams { None, Mini }

		// --- 'Class' -> 'Xml...' ---

		#if __ANDROID__
		#else
		  // 'Class' -> jako 'Xml' do 'Directory + File'
		  public static void ClassToXmlFile<T>(string fileDir, string fileName, T t, EnumParams param = EnumParams.None) where T : class, new()
		  {
			try
			{
				if (String.IsNullOrEmpty(fileDir) == false) { XUtils.DirsAndFiles.Dirs.CreateIfNotExist(fileDir); fileName = Path.Combine(fileDir, fileName); };
				using(StreamWriter streamWriter = new StreamWriter(fileName)) { ClassToXmlTextStream<T>(streamWriter, t, param); }
			} catch(Exception e) { ShowException ("ClassToXmlFile<T>", e.ToString ()); }
		  }
		#endif

		// 'Class' -> jako 'Xml' do 'String'
		public static string ClassToXmlString<T>(T t, EnumParams param = EnumParams.None) where T : class, new()
		{
			try
			{
				StringBuilder sb = new StringBuilder();
				ClassToXmlTextStream<T>(new StringWriter(sb), t, param);
				return sb.ToString();
			} catch(Exception e) { ShowException ("ClassToXmlString<T>", e.ToString ()); return String.Empty; }
		}

		// 'Class' -> jako 'Xml' do 'Stream'
		public static void ClassToXmlTextStream<T>(TextWriter textWriterIn, T t, EnumParams param = EnumParams.None) where T : class, new()
		{
			try
			{
				if (param == EnumParams.Mini)
				{
					XmlWriterSettings settings = new XmlWriterSettings() {  Indent = false /* mezery */, OmitXmlDeclaration = true, NewLineChars = "" /* konce radku */ };
					using (XmlWriter xmlWriter = XmlWriter.Create(textWriterIn, settings))
					{
						XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
						// save data from given class to XML file
						XmlSerializerNamespaces emptyNs = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
						xmlSerializer.Serialize(xmlWriter, t, emptyNs);
					}
				}
				else
				{
					XmlWriterSettings settings = new XmlWriterSettings() { Indent = true /* mezery */, NewLineChars = "\n" /* konce radku */ };
					//using (TextWriter textWriter = textWriterIn)
					using (XmlWriter xmlWriter = XmlWriter.Create(textWriterIn, settings))
					{	// create 'XmlSerializer' for given class type
						XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
						// save data from given class to XML file
						//xmlSerializer.Serialize(textWriter, t);
						xmlSerializer.Serialize(xmlWriter, t);
					}
				}
			}
			catch (Exception e) { ShowException("ClassToXmlTextStream<T>", e.ToString()); }
		}

		// --- 'Xml...' -> 'Class' ---

		#if __ANDROID__
		#else
		  // 'XmFile' -> 'Class'
		  public static T XmlFileToClass<T>(string fileDir, string fileName) where T : class, new()
		  {
			try
			{
				if (String.IsNullOrEmpty(fileDir) == false) { XUtils.DirsAndFiles.Dirs.CreateIfNotExist(fileDir); fileName = Path.Combine(fileDir, fileName); };
				return XmlTextStreamToClass<T>(new StreamReader(fileName));
			} catch(Exception e) { ShowException("XmlFileToClass<T>", e.ToString()); return default(T); }
		  }
		#endif

		// 'XmlString' -> 'Class'
		public static T XmlStringToClass<T>(string xmlString) where T : class, new()
		{
			try { return XmlTextStreamToClass<T>(new StringReader(xmlString)); }
			catch(Exception e) { ShowException("XmlStringToClass<T>", e.ToString()); return default(T); }
		}

		// 'XmStream' -> 'Class'
		public static T XmlTextStreamToClass<T>(TextReader textReaderIn, bool showInvalidItems = false) // where T : class, new()
		{
			try
			{
				//XmlReaderSettings settings = new XmlReaderSettings() { Indent = true, OmitXmlDeclaration = true };
				//using (XmlReader xmlReader = XmlReader.Create(textReaderIn, settings))

				using (TextReader textReader = textReaderIn)
				{	// create 'XmlSerializer' for given class type
					XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
					#if SILVERLIGHT || __ANDROID__
					#else
					if (showInvalidItems == true)
					{
						xmlSerializer.UnknownNode += new XmlNodeEventHandler(XmlSerializerUnknownNode);
						xmlSerializer.UnknownElement += new XmlElementEventHandler(XmlSerializerUnknownElement);
						xmlSerializer.UnknownAttribute += new XmlAttributeEventHandler(XmlSerializerUnknownAttribute);
					}
					#endif
					// read data from XML file to class
					return (T)xmlSerializer.Deserialize(textReader);
				}
			}
			catch (Exception e) { ShowException("XmlTextStreamToClass<T>", e.ToString()); return default(T); }
		}

        // --- ---

        #if SILVERLIGHT || __ANDROID__
        #else
		  private static void XmlSerializerUnknownNode(object sender, XmlNodeEventArgs e)
		  {
			string title = "!!! Invalid / Unknown node was detected in XML file !!!";
			//string text = "\t" + e.n.Name + " " + e.Attr.InnerText + 
			//	"\n\tLineNumber: " + (e.LineNumber-1) +
			//	"\n\tLinePosition: " + e.LinePosition;
			MessageBox.Show("text", title);
		  }
		  // show information with invalid / unknown element
		  private static void XmlSerializerUnknownElement(object sender, XmlElementEventArgs e)
		  {
			string title = "!!! Invalid / Unknown element was detected in XML file !!!";
			string text = "\t" + e.Element.Name + " " + e.Element.InnerText +
				"\n\tLineNumber: " + (e.LineNumber - 1) +
					"\n\tLinePosition: " + e.LinePosition;
			MessageBox.Show(text, title);
		  }
		  private static void XmlSerializerUnknownAttribute(object sender, XmlAttributeEventArgs e)
		  {
			string title = "!!! Invalid / Unknown attribute was detected in XML file !!!";
			string text = "\t" + e.Attr.Name + " " + e.Attr.InnerText +
				"\n\tLineNumber: " + (e.LineNumber - 1) +
					"\n\tLinePosition: " + e.LinePosition;
			MessageBox.Show(text, title);
		  }
        #endif

        public static bool ShowExceptions { get; set; } = false;
		public static void ShowException(string methodStr, string eStr)
		{
			#if __ANDROID__
			#else
			  if (ShowExceptions) { MessageBox.Show("Method: " + methodStr + "\n\nException: " + eStr, "!!! Exception - XmlSerialization !!!", MessageBoxButton.OK); }
			#endif
		}
	}
}
