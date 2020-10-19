using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using UpkManager.Domain.Constants;
using UpkManager.Domain.Helpers;
using UpkManager.Domain.Models.UpkFile.Tables;


namespace UpkManager.Domain.Models.UpkFile.Properties
{

    public class DomainPropertyNameArray : DomainPropertyValueBase
    {

        #region Constructor

        public DomainPropertyNameArray()
        {
            // NameIndexValue = new DomainNameTableIndex();
            NameArray = new List<DomainString>();
        }

        #endregion Constructor

        #region Properties

        private List<DomainString> NameArray;

        #endregion Properties

        #region Domain Properties

        public override PropertyTypes PropertyType => PropertyTypes.NameArray;

        public override object PropertyValue => NameArray;

        public override string PropertyString => String.Join(", ", NameArray);

        #endregion Domain Properties

        #region Domain Methods

        public override async Task ReadPropertyValue(ByteArrayReader reader, int size, DomainHeader header)
        {
            await Task.Run(async () =>
            {
                int length = reader.ReadInt32();
                for (int i = 0; i < length; i++)
                {
                    DomainString newstring = new DomainString();
                    await newstring.ReadString(reader);
                    NameArray.Add(newstring);
                }
            });
        }
        public override void SetPropertyValue(object value)
        {
            if (!(value is DomainString)) return;

            NameArray[0] = (DomainString)value;
        }

        #endregion Domain Methods

        #region DomainUpkBuilderBase Implementation

        public override int GetBuilderSize()
        {
            int currsize = 4;
            foreach (DomainString domainstring in NameArray)
                currsize += domainstring.GetBuilderSize();
            BuilderSize = currsize;

            return BuilderSize;
        }

        public override async Task WriteBuffer(ByteArrayWriter Writer, int CurrentOffset)
        {
            Writer.WriteInt32(NameArray.Count);
            foreach (DomainString domainstring in NameArray)
                domainstring.WriteBuffer(Writer, CurrentOffset);
        }

        #endregion DomainUpkBuilderBase Implementation

        public override string ToString()
        {
            return String.Join(", ", NameArray);
        }
    }

}
