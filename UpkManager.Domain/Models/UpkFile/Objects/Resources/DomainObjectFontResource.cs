using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using UpkManager.Domain.Constants;
using UpkManager.Domain.Helpers;
using UpkManager.Domain.Models.UpkFile.Properties;
using UpkManager.Domain.Models.UpkFile.Tables;

namespace UpkManager.Domain.Models.UpkFile.Objects.Resources
{

    public class DomainObjectFontResource : DomainObjectBase
    {

        #region Constructor

        public DomainObjectFontResource()
        {
            Font = null;
            FontLength = null;
            FontBytes = null;
        }

        #endregion Constructor

        #region Properties

        public byte[] Font { get; set; }

        public byte[] FontLength;

        public byte[] FontBytes;

        private bool Modified = false;

        public override bool IsExportable => true;

        public override ViewableTypes Viewable => ViewableTypes.Font;

        public override ObjectTypes ObjectType => ObjectTypes.FontResource;

        public override string FileExtension => ".ttf";

        public override string FileTypeDesc => "TrueType/OpenType font";

        #endregion Properties

        #region Domain Methods

        public override async Task ReadDomainObject(ByteArrayReader reader, DomainHeader header, DomainExportTableEntry export, bool skipProperties, bool skipParse)
        {
            await base.ReadDomainObject(reader, header, export, skipProperties, skipParse);

            if (skipParse) return;

            FontBytes = (byte[])this.PropertyHeader.GetProperty("Buffer").FirstOrDefault().Value.PropertyValue;

            FontLength = FontBytes.Take(4).ToArray();
            Font = FontBytes.Skip(4).ToArray();

            if (this.Modified)
                Debug.WriteLine("hi1");
        }

        public override async Task SaveObject(string filename, object configuration)
        {
            if (Font == null) return;

            await Task.Run(() => File.WriteAllBytes(filename, Font));
        }
        public override async Task SetObject(string filename, List<DomainNameTableEntry> nameTable, object configuration)
        {
            this.Modified = true;
            Font = File.ReadAllBytes(filename);

            FontLength = BitConverter.GetBytes(Font.Length);

            ByteArrayReader FontReader = ByteArrayReader.CreateNew(FontLength.Concat(Font).ToArray(), 0);
            PropertyHeader.GetProperty("Buffer").FirstOrDefault().Value.SetPropertyValue(FontReader);
            PropertyHeader.GetProperty("Buffer").FirstOrDefault().Value.GetBuilderSize();

            // GlyphTypeface ttf = new GlyphTypeface(new Uri(filename));

            // PropertyHeader.GetProperty("FontFamilyNameArray").First().Value.SetPropertyValue(new DomainString("Lovely Creamy"));
            // PropertyHeader.GetProperty("FontStyleNameArray").First().Value.SetPropertyValue(new DomainString("Lovely Creamy"));
            // PropertyHeader.GetProperty("FontFullNameArray").First().Value.SetPropertyValue(new DomainString("Lovely Creamy"));

            var FontFamilyName = (DomainPropertyNameArray)PropertyHeader.GetProperty("FontFamilyNameArray").First().Value;
            var FontStyleName = (DomainPropertyNameArray)PropertyHeader.GetProperty("FontStyleNameArray").First().Value;
            var FontFullName = (DomainPropertyNameArray)PropertyHeader.GetProperty("FontFullNameArray").First().Value;
            // string FontFamilyName = ttf.Win32FamilyNames.Values.FirstOrDefault();
            // string FontStyleNamee = ttf.Win32FaceNames.Values.FirstOrDefault();

            Debug.WriteLine("hi2");
        }
        //public override int GetBuilderSize()
        //{
        //    BuilderSize = PropertyHeader.GetBuilderSize()
        //                + base.GetBuilderSize()
        //                + sizeof(int);
        //    return base.GetBuilderSize();
        //}
        public override async Task WriteBuffer(ByteArrayWriter Writer, int CurrentOffset)
        {
            if (this.Modified)
                Debug.WriteLine("saving modified");

            // await PropertyHeader.WriteBuffer(Writer, CurrentOffset);
            // GetBuilderSize();

            await base.WriteBuffer(Writer, CurrentOffset);


            Debug.WriteLine("hi3");
        }

        public override Stream GetObjectStream()
        {
            return Font != null ? new MemoryStream(Font) : null;
        }

        #endregion Domain Methods

    }

}
