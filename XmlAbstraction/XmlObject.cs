// <copyright file="XmlObject.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace XmlAbstraction
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;

    /// <summary>
    /// Class that allows Reading and Writing of XML Files.
    /// </summary>
    // Only the Save() method should do direct edits to the XDocument object of the class named "Doc".
    // The rest should just use the dictionaries for the changes to be applied to the xml in the Save()
    // method if the xml is not read-only. I did this to support read only memory access of xml.
    public class XmlObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlObject"/> class
        /// for reading xml data from memory.
        ///
        /// With this contstructor, the <see cref="XmlObject"/> returned will be read-only.
        /// </summary>
        /// <param name="xmlcontent">The xml data to load into memory.</param>
        public XmlObject(string xmlcontent)
            : this(":memory", xmlcontent)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlObject"/> class
        /// for reading and/or writing.
        ///
        /// If the file does not exists it will be created.
        /// </summary>
        /// <param name="xmlfilename">
        /// The name of the XML File to load into the XMLObject.
        /// </param>
        /// <param name="fallbackxmlcontent">
        /// The fallback content string to write into the fallback XML File
        /// if the file does not exist or if the file is empty.
        /// </param>
        public XmlObject(string xmlfilename, string fallbackxmlcontent)
            : this(xmlfilename, fallbackxmlcontent, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlObject"/> class
        /// for reading and/or writing.
        ///
        /// If the file does not exists it will be created.
        /// </summary>
        /// <param name="xmlfilename">
        /// The name of the XML File to load into the XMLObject.
        /// </param>
        /// <param name="fallbackxmlcontent">
        /// The fallback content string to write into the fallback XML File
        /// if the file does not exist or if the file is empty.
        /// </param>
        /// <param name="saveToCurrentDirectory">
        /// Controls weather to save the file to the xmlfilename param string if
        /// it is the full path or to the Current Directory if it supplies file name only.
        /// </param>
        public XmlObject(string xmlfilename, string fallbackxmlcontent, bool saveToCurrentDirectory)
        {
            this.ObjLock = new object();
            this.ElementsAdded = new Dictionary<string, XmlElementData>();
            this.ElementsEdits = new Dictionary<string, XmlElementData>();
            this.ElementAttributesDeleted = new Dictionary<string, XmlElementData>();
            this.ElementsDeleted = new List<string>();
            if (saveToCurrentDirectory)
            {
                var directory = new DirectoryInfo(xmlfilename);
                if (!directory.Parent.Exists)
                {
                    throw new DirectoryNotFoundException("Directory in filename was not found.");
                }

                if (!xmlfilename.Contains(Environment.CurrentDirectory) &&
                    directory.Parent.FullName == Environment.CurrentDirectory)
                {
                    xmlfilename = Environment.CurrentDirectory + Path.DirectorySeparatorChar + xmlfilename;
                }
            }

            if (!fallbackxmlcontent.Contains("<?xml version=\"1.0\" encoding=\"utf-8\" ?>"))
            {
                // insert root element at begginning of string data.
                fallbackxmlcontent = fallbackxmlcontent.Insert(0, "<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            }

            long fileSize = 0;
            this.CachedXmlfilename = xmlfilename;
            if (!xmlfilename.Equals(":memory"))
            {
                this.Exists = File.Exists(xmlfilename);
                this.HasChanged = !this.Exists;
                if (this.Exists)
                {
                    var fileinfo = new FileInfo(xmlfilename);
                    fileSize = fileinfo.Length;
                }
            }

            this.Doc = (fileSize > 0) ? XDocument.Load(xmlfilename) : XDocument.Parse(fallbackxmlcontent);
        }

        private object ObjLock { get; set; }

        private XDocument Doc { get; set; }

        private string CachedXmlfilename { get; set; }

        // Pending XML Element Addictions (excluding only adding attributes to an already existing element).
        private Dictionary<string, XmlElementData> ElementsAdded { get; set; }

        // Pending XML Element edits (any value edits, added attributes, or edited attributes).
        private Dictionary<string, XmlElementData> ElementsEdits { get; set; }

        // Pending XML Element Attribute Deletions. If Element was made at runtime and not in the xml file,
        // remove it from the _elements_changed Dictionary instead.
        private Dictionary<string, XmlElementData> ElementAttributesDeleted { get; set; }

        // Pending XML Element Deletions.
        private List<string> ElementsDeleted { get; set; }

        // Summary:
        //   Gets or sets a value indicating whether the XML file existed when this object was created.
        //
        //   This is just a property to minimize saving code on checking
        //   if the xml file changed externally.
        private bool Exists { get; set; } = false;

        // Summary:
        //   Gets a value indicating whether the XML file was externally edited.
        private bool HasChangedExternally
        {
            get
            {
                var outxmlData = new MemoryStream();
                this.Doc.Save(outxmlData);
                var outXmlBytes = outxmlData.ToArray();
                outxmlData.Dispose();

                // ensure file length is not 0.
                if (this.Exists != (
                    File.Exists(this.CachedXmlfilename) &&
                    Encoding.UTF8.GetString(File.ReadAllBytes(this.CachedXmlfilename)).Length > 0))
                {
                    // refresh Exists so it always works.
                    this.Exists = File.Exists(this.CachedXmlfilename);
                }

                var dataOnFile = this.Exists ? File.ReadAllBytes(this.CachedXmlfilename) : null;

                // cannot change externally if it does not exist on file yet.
                return dataOnFile == null ? false : !dataOnFile.SequenceEqual(outXmlBytes) ? true : false;
            }
        }

        // Summary:
        //   Gets or sets a value indicating whether the internal data in this class has changed, e.g.
        //   pending edits, deletions, or additions to the xml file.
        private bool HasChanged { get; set; } = false;

        /// <summary>
        /// Reopens from the file name used to construct the object,
        /// but only if it has changed. If the file was not saved it
        /// will be saved first.
        /// </summary>
        /// <exception cref="InvalidOperationException">Cannot reopen on read-only instances.</exception>
        public void ReopenFile()
        {
            if (!this.CachedXmlfilename.Equals(":memory"))
            {
                this.Save();
                this.Doc = XDocument.Load(this.CachedXmlfilename);
            }
            else
            {
                throw new InvalidOperationException("This instance is read-only.");
            }
        }

        /// <summary>
        /// Adds or edits an attribute in an Element and sets it's value in the <see cref="XmlObject"/>.
        ///
        /// This method can also remove the attribute by setting the value to null.
        ///
        /// If Element does not exist yet it will be created automatically with an
        /// empty value as well as making the attribute as if the Element was
        /// pre-added before calling this function.
        /// </summary>
        /// <exception cref="Exception">Attribute already exists in the xml file.</exception>
        /// <exception cref="InvalidOperationException">When called from a read-only instance.</exception>
        /// <param name="elementname">The name of the element to add a attribute to.</param>
        /// <param name="attributename">The name of the attribute to add.</param>
        /// <param name="attributevalue">The value of the attribute.</param>
        public void AddAttribute(string elementname, string attributename, object attributevalue)
        {
            if (!this.CachedXmlfilename.Equals(":memory"))
            {
                var elem = this.Doc.Root.Element(elementname);
                if (elem == null)
                {
                    this.Write(elementname, string.Empty);
                }

                if (this.ElementsAdded.ContainsKey(elementname))
                {
                    var xmleldata = this.ElementsAdded[elementname];
                    if (xmleldata.Attributes != null)
                    {
                        foreach (var attribute in xmleldata.Attributes)
                        {
                            if (attribute.AttributeName.Equals(attributename))
                            {
                                attribute.Value = attributevalue.ToString();
                            }
                        }
                    }

                    var xMLAttributeData = new XmlAttributeData
                    {
                        AttributeName = attributename,
                        Value = attributevalue.ToString(),
                    };
                    xmleldata.Attributes = xmleldata.Attributes ?? new List<XmlAttributeData>();
                    xmleldata.Attributes.Add(xMLAttributeData);
                }
                else if (this.ElementsEdits.ContainsKey(elementname))
                {
                    XmlAttributeData xMLAttributeData;
                    var edit = false;
                    var attributeIndex = 0;
                    var xmleldata = this.ElementsEdits[elementname];
                    foreach (var attribute in xmleldata.Attributes)
                    {
                        if (attribute.AttributeName.Equals(attributename))
                        {
                            edit = true;
                            attributeIndex = xmleldata.Attributes.IndexOf(attribute);
                        }
                    }

                    xMLAttributeData = new XmlAttributeData
                    {
                        AttributeName = attributename,
                        Value = attributevalue.ToString(),
                    };
                    xmleldata.Attributes = xmleldata.Attributes ?? new List<XmlAttributeData>();
                    if (!edit && attributevalue != null)
                    {
                        xmleldata.Attributes.Add(xMLAttributeData);
                    }
                    else
                    {
                        xMLAttributeData = xmleldata.Attributes[attributeIndex];
                        xMLAttributeData.Value = attributevalue.ToString();
                    }
                }
                else
                {
                    if (attributevalue != null)
                    {
                        if (elem.Attribute(attributename) != null)
                        {
                            throw new Exception("Attribute already exists.");
                        }
                        else
                        {
                            var xmleldata = new XmlElementData
                            {
                                Name = elementname,
                            };
                            var xMLAttributeData = new XmlAttributeData
                            {
                                AttributeName = attributename,
                                Value = attributevalue.ToString(),
                            };
                            xmleldata.Attributes = new List<XmlAttributeData>
                            {
                                xMLAttributeData,
                            };
                            this.ElementsEdits.Add(elementname, xmleldata);
                        }
                    }
                }

                this.HasChanged = true;
            }
            else
            {
                throw new InvalidOperationException("This instance is read-only.");
            }
        }

        /// <summary>
        /// Writes to an element or updates it based upon the element name
        /// it is in and the value to place in it.
        ///
        /// If Element does not exist yet it will be created automatically.
        /// </summary>
        /// <exception cref="InvalidOperationException">When called from a read-only instance.</exception>
        /// <param name="elementname">The name of the element to write to or create.</param>
        /// <param name="value">The value for the element.</param>
        public void Write(string elementname, string value)
        {
            if (!this.CachedXmlfilename.Equals(":memory"))
            {
                var elem = this.Doc.Root.Element(elementname);
                if (elem != null
                    || this.ElementsAdded.ContainsKey(elementname)
                    || this.ElementsEdits.ContainsKey(elementname))
                {
                    var xMLElementData = new XmlElementData
                    {
                        Attributes = this.ElementsAdded.ContainsKey(elementname)
                            ? this.ElementsAdded[elementname].Attributes
                            : (this.ElementsEdits.ContainsKey(elementname)
                                ? this.ElementsEdits[elementname].Attributes
                                : null),
                        Value = value,
                        Name = elementname,
                    };
                    if (this.ElementsAdded.ContainsKey(elementname))
                    {
                        // modify this key, do not put into _elements_edits dictonary.
                        this.ElementsAdded[elementname] = xMLElementData;
                    }
                    else
                    {
                        if (this.ElementsEdits.ContainsKey(elementname))
                        {
                            // edit the collection whenever this changes.
                            this.ElementsEdits[elementname] = xMLElementData;
                        }
                        else
                        {
                            this.ElementsEdits.Add(elementname, xMLElementData);
                        }
                    }

                    this.HasChanged = true;
                }
                else
                {
                    this.AddElement(elementname, value);
                }
            }
            else
            {
                throw new InvalidOperationException("This instance is read-only.");
            }
        }

        /// <summary>
        /// Writes to an attribute in an element or updates it based upon the element name
        /// it is in and the value to place in it.
        ///
        /// If Element does not exist yet it will be created automatically.
        /// </summary>
        /// <exception cref="Exception">Attribute already exists in the xml file.</exception>
        /// <exception cref="InvalidOperationException">When called from a read-only instance.</exception>
        /// <param name="elementname">
        /// The name of the element to create an attribute or set an
        /// attribute in or to create with the attribute.
        /// </param>
        /// <param name="attributename">The attribute name to change the value or to create.</param>
        /// <param name="attributevalue">The value of the attribute to use.</param>
        public void Write(string elementname, string attributename, string attributevalue)
        {
            if (!this.CachedXmlfilename.Equals(":memory"))
            {
                this.AddAttribute(elementname, attributename, attributevalue);
            }
            else
            {
                throw new InvalidOperationException("This instance is read-only.");
            }
        }

        /// <summary>
        /// Writes an array of elements to the parrent element or updates them based
        /// upon the element name.
        ///
        /// If Elements do not exist yet they will be created automatically.
        /// </summary>
        /// <exception cref="InvalidOperationException">When called from a read-only instance.</exception>
        /// <param name="parentelementname">parrent element name of the subelement.</param>
        /// <param name="elementname">The name to use when writing subelement(s).</param>
        /// <param name="values">The array of values to use for the subelement(s).</param>
        public void Write(string parentelementname, string elementname, string[] values)
        {
            if (!this.CachedXmlfilename.Equals(":memory"))
            {
                var elem = this.Doc.Root.Element(parentelementname);
                if (elem == null)
                {
                    this.Write(parentelementname, string.Empty);
                }

                var elem2 = this.Doc.Descendants(parentelementname);
                if (elem2.Count() > 0)
                {
                    // for Save() to work.
                    this.Delete(parentelementname);
                }

                var xmleldata = new XmlElementData
                {
                    Name = parentelementname,
                    Subelements = new List<XmlElementData>(),
                };
                foreach (var value in values)
                {
                    var xmlelsubelement = new XmlElementData
                    {
                        Name = elementname,
                        Value = value,
                    };
                    xmleldata.Subelements.Add(xmlelsubelement);
                }

                if (this.ElementsAdded.ContainsKey(parentelementname))
                {
                    this.ElementsAdded[parentelementname] = xmleldata;
                }
                else
                {
                    this.ElementsAdded.Add(parentelementname, xmleldata);
                }
            }
            else
            {
                throw new InvalidOperationException("This instance is read-only.");
            }
        }

        /// <summary>
        /// Reads and returns the value set for an particular XML Element.
        ///
        /// If Element does not exist yet it will be created automatically with an empty value.
        /// </summary>
        /// <exception cref="InvalidOperationException">When the Element does not exist in a read-only instance.</exception>
        /// <param name="elementname">The element name to read the value from.</param>
        /// <returns>The value of the input element or <see cref="string.Empty"/>.</returns>
        public string Read(string elementname)
        {
            var elem = this.Doc.Root.Element(elementname);
            if (elem != null
                || this.ElementsAdded.ContainsKey(elementname)
                || this.ElementsEdits.ContainsKey(elementname))
            {
                // do not dare to look in _elements_deleted.
                return elem != null
                    ? elem.Value
                    : (this.ElementsAdded.ContainsKey(elementname)
                        ? this.ElementsAdded[elementname].Value
                        : (this.ElementsEdits.ContainsKey(elementname)
                            ? this.ElementsEdits[elementname].Value
                            : string.Empty));
            }
            else
            {
                this.Write(elementname, string.Empty);
                return string.Empty;
            }
        }

        /// <summary>
        /// Reads and returns the value set for an particular XML Element attribute.
        ///
        /// If Element and the attribute does not exist yet it will be created automatically
        /// with an empty value.
        /// </summary>
        /// <exception cref="InvalidOperationException">When the Element does not exist in a read-only instance.</exception>
        /// <param name="elementname">The element name to get the value of a attribute.</param>
        /// <param name="attributename">The name of the attribute to get the value of.</param>
        /// <returns>The value of the input element or <see cref="string.Empty"/>.</returns>
        public string Read(string elementname, string attributename)
        {
            var elem = this.Doc.Root.Element(elementname);
            if (elem == null)
            {
                this.Write(elementname, attributename, string.Empty);
            }
            else if (elem != null)
            {
                var attribute = elem.Attribute(attributename);
                if (attribute != null)
                {
                    return attribute.Value;
                }
            }
            else if (this.ElementsAdded.ContainsKey(elementname))
            {
                foreach (var attribute in this.ElementsAdded[elementname].Attributes)
                {
                    if (attribute.AttributeName.Equals(attributename))
                    {
                        return attribute.Value;
                    }
                }
            }
            else if (this.ElementsEdits.ContainsKey(elementname))
            {
                foreach (var attribute in this.ElementsEdits[elementname].Attributes)
                {
                    if (attribute.AttributeName.Equals(attributename))
                    {
                        return attribute.Value;
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Reads and returns an array of values set for an particular XML Element's subelements.
        ///
        /// If Parent Element does not exist yet it will be created automatically
        /// with an empty value. In that case an empty string array is returned.
        /// </summary>
        /// <exception cref="InvalidOperationException">When the Element does not exist in a read-only instance.</exception>
        /// <param name="parentelementname">The name of the parrent element of the subelement(s).</param>
        /// <param name="elementname">The name of the subelements to get their values.</param>
        /// <param name="unused">
        /// A unused paramiter to avoid a compiler error from this overload.
        /// </param>
        /// <returns>
        /// A array of values or a empty array of strings if
        /// there is no subelements to this element.
        /// </returns>
        public string[] Read(string parentelementname, string elementname, object unused = null)
        {
            var elem = this.Doc.Descendants(parentelementname);
            var strarray = new string[] { };
            foreach (var element in elem)
            {
                var elements = element.Elements(elementname);
                var elemValues = new List<string>();
                foreach (var elemnt in elements)
                {
                    elemValues.Add(elemnt.Value);
                }

                strarray = elemValues.ToArray();
            }

            if (elem.Count() == 0)
            {
                this.Write(parentelementname, string.Empty);
            }

            return strarray;
        }

        /// <summary>
        /// Deletes an xml element using the element name.
        /// Can also delete not only the parrent element but also subelements with it.
        /// </summary>
        /// <exception cref="ArgumentException">elementname does not exist in the xml, in pending edits, or was already deleted.</exception>
        /// <exception cref="InvalidOperationException">When the object is a read-only instance.</exception>
        /// <param name="elementname">The element name of the element to delete.</param>
        public void Delete(string elementname)
        {
            if (!this.CachedXmlfilename.Equals(":memory"))
            {
                var elem = this.Doc.Root.Element(elementname);
                if (this.ElementsAdded.ContainsKey(elementname))
                {
                    this.ElementsAdded.Remove(elementname);
                }
                else if (this.ElementsEdits.ContainsKey(elementname))
                {
                    this.ElementsEdits.Remove(elementname);
                }
                else if (elem != null && !this.ElementsDeleted.Contains(elementname))
                {
                    this.ElementsDeleted.Add(elementname);
                }
                else
                {
                    throw new ArgumentException("elementname does not exist in the xml, in pending edits, or was already deleted.");
                }

                this.HasChanged = true;
            }
            else
            {
                throw new InvalidOperationException("This instance is read-only.");
            }
        }

        /// <summary>
        /// Removes an xml attribute using the element name and the name of the attribute.
        /// </summary>
        /// <exception cref="ArgumentException">elementname or attributename does not exist in the xml, in pending edits, or was already deleted.</exception>
        /// <exception cref="InvalidOperationException">When the object is a read-only instance.</exception>
        /// <param name="elementname">The element name that has the attribute to delete.</param>
        /// <param name="attributename">The name of the attribute to delete.</param>
        public void Delete(string elementname, string attributename)
        {
            if (!this.CachedXmlfilename.Equals(":memory"))
            {
                var elem = this.Doc.Root.Element(elementname);
                if (this.ElementsAdded.ContainsKey(elementname))
                {
                    foreach (var attribute in this.ElementsAdded[elementname].Attributes)
                    {
                        if (attribute.AttributeName.Equals(attributename))
                        {
                            this.ElementsAdded[elementname].Attributes.Remove(attribute);
                        }
                    }
                }
                else if (this.ElementsEdits.ContainsKey(elementname))
                {
                    foreach (var attribute in this.ElementsEdits[elementname].Attributes)
                    {
                        if (attribute.AttributeName.Equals(attributename))
                        {
                            this.ElementsEdits[elementname].Attributes.Remove(attribute);
                        }
                    }
                }
                else if (elem != null && elem.Attribute(attributename) != null
                    && !this.ElementAttributesDeleted.ContainsKey(elementname))
                {
                    var xmleldata = new XmlElementData
                    {
                        Name = elementname,
                        Attributes = new List<XmlAttributeData>(),
                    };
                    var xMLAttributeData = new XmlAttributeData
                    {
                        AttributeName = attributename,
                        Value = null,
                    };
                    xmleldata.Attributes.Add(xMLAttributeData);
                    this.ElementAttributesDeleted.Add(elementname, xmleldata);
                }
                else
                {
                    throw new ArgumentException("elementname or attributename does not exist in the xml, in pending edits, or was already deleted.");
                }

                this.HasChanged = true;
            }
            else
            {
                throw new InvalidOperationException("This instance is read-only.");
            }
        }

        /// <summary>
        /// Saves the underlying XML file if it changed.
        /// </summary>
        public void Save()
        {
            // do not save in memory xml. It should be read only.
            if (!this.CachedXmlfilename.Equals(":memory"))
            {
                lock (this.ObjLock)
                {
                    if (this.HasChangedExternally && this.Exists)
                    {
                        // reopen file to apply changes at runtime to it.
                        this.Doc = XDocument.Load(this.CachedXmlfilename);
                    }

                    if (this.HasChanged)
                    {
                        // start with deleted elements and attributes.
                        foreach (var attributes_deleted in this.ElementAttributesDeleted)
                        {
                            var elem = this.Doc.Root.Element(attributes_deleted.Key);

                            // remove attributes on to this element.
                            foreach (var attributes in attributes_deleted.Value.Attributes)
                            {
                                elem.SetAttributeValue(attributes.AttributeName, attributes.Value);
                            }
                        }

                        // hopefully this actually deletes the elements stored in this list.
                        foreach (var deleted_elements in this.ElementsDeleted)
                        {
                            var elem = this.Doc.Root.Element(deleted_elements);
                            elem.Remove();
                        }

                        foreach (var added_elements in this.ElementsAdded)
                        {
                            // add elements to doc.
                            this.Doc.Root.Add(new XElement(added_elements.Key, added_elements.Value.Value));
                            if (added_elements.Value.Attributes != null)
                            {
                                // add attributes added to this element.
                                var elem = this.Doc.Root.Element(added_elements.Key);
                                foreach (var attributes in added_elements.Value.Attributes)
                                {
                                    elem.SetAttributeValue(attributes.AttributeName, attributes.Value);
                                }

                                // add subelements and their attributes.
                                if (added_elements.Value.Subelements != null)
                                {
                                    foreach (var element in added_elements.Value.Subelements)
                                    {
                                        this.SaveAddedSubelements(elem, element);
                                    }
                                }
                            }
                        }

                        foreach (var edited_elements in this.ElementsEdits)
                        {
                            var elem = this.Doc.Root.Element(edited_elements.Key);
                            elem.Value = edited_elements.Value.Value;
                            if (edited_elements.Value.Attributes != null)
                            {
                                // add/edit attributes added/edited to this element.
                                foreach (var attributes in edited_elements.Value.Attributes)
                                {
                                    elem.SetAttributeValue(attributes.AttributeName, attributes.Value);
                                }
                            }
                        }

                        this.ElementsAdded.Clear();
                        this.ElementsEdits.Clear();
                        this.ElementAttributesDeleted.Clear();
                        this.ElementsDeleted.Clear();

                        // apply changes.
                        this.Doc.Save(this.CachedXmlfilename);

                        // avoid unneeded writes if nothing changed after this.
                        this.HasChanged = false;
                    }
                }
            }
        }

        // Summary:
        //   Adds an Element to the XmlObject but verifies it does not exist first.
        //
        // Exceptions:
        //   System.Exception:
        //     Thrown if element already exists in the XMLObject.
        //   System.ObjectDisposedException
        //     XMLOblect is disposed.
        private void AddElement(string elementname, string value)
        {
            var elem = this.Doc.Root.Element(elementname);
            if (elem == null)
            {
                var xMLElementData = new XmlElementData
                {
                    Attributes = null,
                    Value = value,
                };
                if (!this.ElementsAdded.ContainsKey(elementname))
                {
                    this.ElementsAdded.Add(elementname, xMLElementData);
                }
                else
                {
                    this.ElementsAdded[elementname] = xMLElementData;
                }

                this.HasChanged = true;
            }
            else
            {
                throw new Exception("Element already exists.");
            }
        }

        // Summary:
        //   Writes Added subelements to the XML file.
        private void SaveAddedSubelements(XElement xElement, XmlElementData elemdata)
        {
            if (elemdata.Name != string.Empty)
            {
                var elem = new XElement(elemdata.Name, elemdata.Value);
                xElement.Add(elem);
                if (elemdata.Attributes != null)
                {
                    foreach (var attributes in elemdata.Attributes)
                    {
                        elem.SetAttributeValue(attributes.AttributeName, attributes.Value);
                    }
                }

                if (elemdata.Subelements != null)
                {
                    // recursively add each subelement of these subelements.
                    foreach (var element in elemdata.Subelements)
                    {
                        this.SaveAddedSubelements(elem, element);
                    }
                }
            }
        }

        private class XmlAttributeData
        {
            internal string AttributeName { get; set; } = string.Empty;

            internal string Value { get; set; } = string.Empty;
        }

        private class XmlElementData
        {
            internal string Name { get; set; } = string.Empty;

            internal List<XmlElementData> Subelements { get; set; } = null;

            internal List<XmlAttributeData> Attributes { get; set; } = new List<XmlAttributeData>();

            internal string Value { get; set; } = string.Empty;
        }
    }
}
