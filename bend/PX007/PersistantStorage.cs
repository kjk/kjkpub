using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace Bend
{
    public class PersistantStorage
    {
        static PersistantStorage singletonPersistantStorageObject;
        const string settingsFileName = "Settings.xml";
        
        #region Member Data
        public string[] mruFile;
        public double mainWindowTop;
        public double mainWindowLeft;
        public double mainWindowWidth;
        public double mainWindowHeight;

        // JSBeautifier Options
        public bool JSBeautifyPreserveLine;
        public int  JSBeautifyIndent;
        public bool JSBeautifyUseSpaces;
        public bool JSBeautifyUseTabs;

        // Text Editor Options
        public int TextIndent;
        public bool TextUseSpaces;
        public bool TextUseTabs;
        public bool TextFormatControlCharacters;
        public bool TextFormatHyperLinks;
        public bool TextFormatEmailLinks;
        public bool TextShowFormatting;
        public bool TextWordWrap;
        #endregion

        private PersistantStorage()
        {
            // Prevent object construction and default file creation
            mruFile = null;
            mainWindowTop = System.Windows.SystemParameters.PrimaryScreenHeight / 2 - 300;
            mainWindowLeft = System.Windows.SystemParameters.PrimaryScreenWidth / 2 - 400;
            mainWindowWidth = 800.0;
            mainWindowHeight = 600.0;

            JSBeautifyPreserveLine = false;
            JSBeautifyIndent = 4;
            JSBeautifyUseSpaces = true;
            JSBeautifyUseTabs = false;

            TextIndent = 4;
            TextUseSpaces = true;
            TextUseTabs = false;
            TextFormatControlCharacters = true;
            TextFormatHyperLinks = true;
            TextFormatEmailLinks = true;
            TextShowFormatting = false;
            TextWordWrap = false;
        }

        static PersistantStorage()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(PersistantStorage));
                String filePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\";
                FileStream fs = new FileStream(filePath + settingsFileName, FileMode.Open);
                singletonPersistantStorageObject = (PersistantStorage)serializer.Deserialize(fs);
            }
            catch
            {
                singletonPersistantStorageObject = new PersistantStorage();
            }
        }

        public static PersistantStorage StorageObject
        {
            get
            {
                return singletonPersistantStorageObject;
            }
        }

        ~PersistantStorage()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(this.GetType());
                String filePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\";
                TextWriter writer = new StreamWriter(filePath + settingsFileName);
                serializer.Serialize(writer, this);
                writer.Close();
            }
            catch
            {
            }
        }        
    }    
}
