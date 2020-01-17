using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Specialized;
using FileDateTimeDetails;
using System.Data.SqlClient;

namespace FileDateTimeDetails
{
    public partial class FileDateTimeDetails : Form
    {
        public FileDateTimeDetails()
        {
            InitializeComponent();
        }

        private void FileDateTimeDetails_Load(object sender, EventArgs e)
        {
            ReadFileDetails();
        }

        public void ReadFileDetails()
        {
            // string xmlString = System.IO.File.ReadAllText("../../FileData.xml");
            List<Data> listOfSystemDetails = new List<Data>();
            List<Data> listOfFileDetailsToSave = new List<Data>();
            XmlRootAttribute xRoot = new XmlRootAttribute();
            try
            {
                xRoot.ElementName = "DateTimeDetails";
                xRoot.IsNullable = true;
                XmlSerializer deserializer = new XmlSerializer(typeof(DateTimeDetails), xRoot);
                //TextReader reader = new StreamReader(@"..\..\FileData.xml");
                TextReader reader = new StreamReader(ConfigurationSettings.AppSettings["XMLFileDataPath"]);

                object obj = deserializer.Deserialize(reader);
                DateTimeDetails XmlData = (DateTimeDetails)obj;
                reader.Close();

                //Loop through each record
                for (int i = 0; i < XmlData.data.Count; i++)
                {
                    try
                    {
                        DirectoryInfo d = new DirectoryInfo(XmlData.data[i].FolderName);
                        string path = "//" + XmlData.data[i].UNCPath + @"/" + XmlData.data[i].FolderName + @"/" + XmlData.data[i].FileName;
                        if (File.Exists(path))
                        {
                            //FileInfo[] Files = d.GetFiles();
                            //foreach (FileInfo file in Files)
                            //{
                                Data obj1 = new Data();
                                String modification = File.GetLastWriteTime(path).ToString();
                                obj1.FolderName = XmlData.data[i].FolderName;
                                obj1.FileName = XmlData.data[i].FileName;
                                obj1.LastUpdatedDate = modification;
                                obj1.UNCPath = XmlData.data[i].UNCPath;
                                obj1.ErrorMessage = "";
                                listOfFileDetailsToSave.Add(obj1);
                            // }
                        }
                        else
                        {
                            Data objData = new Data();
                            objData.FolderName = XmlData.data[i].FolderName;
                            objData.FileName = XmlData.data[i].FileName;
                            objData.LastUpdatedDate = "";
                            objData.UNCPath = XmlData.data[i].UNCPath;
                            objData.ErrorMessage = "File Not Exists in Given Path";
                            listOfFileDetailsToSave.Add(objData);
                        }
                    }
                    catch (Exception ex)
                    {
                        Data objData = new Data();
                        objData.FolderName = XmlData.data[i].FolderName;
                        objData.FileName = XmlData.data[i].FileName;
                        objData.LastUpdatedDate = "";
                        objData.UNCPath = XmlData.data[i].UNCPath;
                        objData.ErrorMessage = "Error while reading file details in given path";
                        listOfFileDetailsToSave.Add(objData);
                    }
                }
                string xmlString = Serialize(listOfFileDetailsToSave);
                var connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("USP_DateTimeDetails", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@InputXMLString", SqlDbType.VarChar).Value = xmlString;
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch(Exception e)
            {

            }
        }


        public static string Serialize<T>(T dataToSerialize)
        {
            try
            {

                XmlSerializer serializer = new XmlSerializer(typeof(T));
                StringBuilder builder = new StringBuilder();
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = true;
                using (XmlWriter stringWriter = XmlWriter.Create(builder, settings))
                {
                    serializer.Serialize(stringWriter, dataToSerialize);
                    return builder.ToString();
                }

            }
            catch(Exception e)
            {
                throw;
            }
        }


    }
}
