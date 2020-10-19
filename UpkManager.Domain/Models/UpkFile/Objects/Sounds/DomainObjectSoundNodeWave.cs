using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using UpkManager.Domain.Constants;
using UpkManager.Domain.Helpers;
using UpkManager.Domain.Models.UpkFile.Properties;
using UpkManager.Domain.Models.UpkFile.Tables;


namespace UpkManager.Domain.Models.UpkFile.Objects.Sounds
{

    public class DomainObjectSoundNodeWave : DomainObjectCompressionBase
    {

        #region Constructor

        public DomainObjectSoundNodeWave()
        {
            Sounds = new List<byte[]>();
        }

        #endregion Constructor

        #region Properties

        public List<byte[]> Sounds { get; }

        public override bool IsExportable => true;

        public override ViewableTypes Viewable => ViewableTypes.Sound;

        public override ObjectTypes ObjectType => ObjectTypes.SoundNodeWave;

        public override string FileExtension => ".ogg";

        public override string FileTypeDesc => "Ogg Vorbis";

        #endregion Properties

        #region Domain Methods

        public override async Task ReadDomainObject(ByteArrayReader reader, DomainHeader header, DomainExportTableEntry export, bool skipProperties, bool skipParse)
        {
            await base.ReadDomainObject(reader, header, export, skipProperties, skipParse);

            if (skipParse) return;

            bool done = false;

            do
            {
                await ProcessCompressedBulkData(reader, async bulkChunk =>
                {
                    byte[] ogg = (await bulkChunk.DecompressChunk(0))?.GetBytes();

                    if (ogg == null || ogg.Length == 0)
                    {
                        done = true;

                        return;
                    }

                    Sounds.Add(ogg);
                });
            } while (!done);
        }

        public override async Task SetObject(string filename, List<DomainNameTableEntry> nameTable, object configuration)
        {
            filename = @"C:\Users\User\BnS\modding\export_new\bns\CookedPC\00054840\pomf.ogg";
            Sounds[0] = File.ReadAllBytes(filename);

            DomainPropertyFloatValue duration = PropertyHeader.GetProperty("Duration").FirstOrDefault()?.Value as DomainPropertyFloatValue;
            DomainPropertyIntValue numchannels = PropertyHeader.GetProperty("NumChannels").FirstOrDefault()?.Value as DomainPropertyIntValue;
            DomainPropertyIntValue samplerate = PropertyHeader.GetProperty("SampleRate").FirstOrDefault()?.Value as DomainPropertyIntValue;
            DomainPropertyIntValue sampledatasize = PropertyHeader.GetProperty("SampleDataSize").FirstOrDefault()?.Value as DomainPropertyIntValue;

            using (var vorbis = new NVorbis.VorbisReader(filename))
            {
                var channels = vorbis.Channels;
                var sampleRate = vorbis.SampleRate;

                // OPTIONALLY: get a TimeSpan indicating the total length of the Vorbis stream
                var totalTime = vorbis.TotalTime;

                // create a buffer for reading samples
                var readBuffer = new float[channels * sampleRate / 5];  // 200ms

                // get the initial position (obviously the start)
                var position = System.TimeSpan.Zero;
                System.Diagnostics.Debug.WriteLine("Hm");

                duration?.SetPropertyValue((float)totalTime.TotalSeconds);
                numchannels?.SetPropertyValue(channels);
                samplerate?.SetPropertyValue(sampleRate);
                sampledatasize?.SetPropertyValue(vorbis.TotalSamples * 2);
                System.Diagnostics.Debug.WriteLine("Hm");
            }

            // sizeX?.SetPropertyValue(skipFirstMip ? width * 2 : width);
            // sizeY?.SetPropertyValue(skipFirstMip ? height * 2 : height);
        }
        public override async Task WriteBuffer(ByteArrayWriter Writer, int CurrentOffset)
        {
            await PropertyHeader.WriteBuffer(Writer, CurrentOffset);

            await base.WriteBuffer(Writer, CurrentOffset);

            for (int i = 0; i < Sounds.Count; ++i)
            {
                await CompressedChunks[i].WriteCompressedChunk(Writer, CurrentOffset);
            }
        }
        public override int GetBuilderSize()
        {
            BuilderSize = PropertyHeader.GetBuilderSize()
                        + base.GetBuilderSize()
                        + sizeof(int);

            foreach (byte[] Sound in Sounds)
            {
                BulkDataCompressionTypes flags = Sound == null || Sound.Length == 0 ? BulkDataCompressionTypes.Unused | BulkDataCompressionTypes.StoreInSeparatefile : BulkDataCompressionTypes.LZO_ENC;

                BuilderSize += Task.Run(() => ProcessUncompressedBulkData(ByteArrayReader.CreateNew(Sound, 0), flags)).Result
                            + sizeof(int) * 2;
            }

            return BuilderSize;
        }

        public override async Task SaveObject(string filename, object configuration)
        {
            if (!Sounds.Any()) return;

            await Task.Run(() => File.WriteAllBytes(filename, Sounds[0]));

            if (Sounds.Count == 1) return;

            string name = Path.GetFileNameWithoutExtension(filename);
            string ext = Path.GetExtension(filename);

            for (int i = 1; i < Sounds.Count; ++i)
            {
                string soundFilename = Path.Combine(Path.GetDirectoryName(filename), $"{name}_{i}{ext}");

                int i1 = i;

                await Task.Run(() => File.WriteAllBytes(soundFilename, Sounds[i1]));
            }
        }

        public override Stream GetObjectStream()
        {
            return Sounds.Any() ? new MemoryStream(Sounds[0]) : null;
        }

        #endregion Domain Methods

    }

}
