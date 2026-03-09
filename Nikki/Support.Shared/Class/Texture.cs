using System;
using System.IO;
using System.ComponentModel;
using Nikki.Core;
using Nikki.Utils;
using Nikki.Utils.DDS;
using Nikki.Reflection.Enum;
using Nikki.Reflection.Abstract;
using Nikki.Reflection.Interface;
using Nikki.Reflection.Attributes;
using CoreExtensions.IO;



namespace Nikki.Support.Shared.Class
{
    /// <summary>
    /// <see cref="Texture"/> is a collection of dds image data used by the game.
    /// </summary>
    public abstract class Texture : Collectable, IAssembly
    {
        #region Private Fields

        private byte[] _data;
        private int _decodedSize;
        private string _lazySourceFile;
        private long _lazyBaseOffset;
        private readonly object _dataSync = new object();

		#endregion

		#region Shared Enums

		/// <summary>
		/// Enum of alpha usage types for textures.
		/// </summary>
		public enum TextureAlphaUsageType : byte
        {
            /// <summary>
            /// 
            /// </summary>
            TEXUSAGE_NONE = 0,

            /// <summary>
            /// 
            /// </summary>
            TEXUSAGE_PUNCHTHRU = 1,

            /// <summary>
            /// 
            /// </summary>
            TEXUSAGE_MODULATED = 2,
        }

        /// <summary>
        /// Enum of alpha blend types for textures.
        /// </summary>
        public enum TextureAlphaBlendType : sbyte
        {
            /// <summary>
            /// 
            /// </summary>
            TEXBLEND_DEFAULT = -1,

            /// <summary>
            /// 
            /// </summary>
            TEXBLEND_SRCCOPY = 0,

            /// <summary>
            /// 
            /// </summary>
            TEXBLEND_BLEND = 1,

            /// <summary>
            /// 
            /// </summary>
            TEXBLEND_ADDATIVE = 2,

            /// <summary>
            /// 
            /// </summary>
            TEXBLEND_SUBTRACTIVE = 3,

            /// <summary>
            /// 
            /// </summary>
            TEXBLEND_OVERBRIGHT = 4,

            /// <summary>
            /// 
            /// </summary>
            TEXBLEND_DEST_BLEND = 5,

            /// <summary>
            /// 
            /// </summary>
            TEXBLEND_DEST_ADDATIVE = 6,

            /// <summary>
            /// 
            /// </summary>
            TEXBLEND_DEST_SUBTRACTIVE = 7,

            /// <summary>
            /// 
            /// </summary>
            TEXBLEND_DEST_OVERBRIGHT = 8,
        }

        /// <summary>
        /// Enum of possible mipmap bias types.
        /// </summary>
        public enum TextureMipmapBiasType : byte
        {
            /// <summary>
            /// 
            /// </summary>
            TEXBIAS_DEFAULT = 0,

            /// <summary>
            /// 
            /// </summary>
            TEXBIAS_ADS = 1,

            /// <summary>
            /// 
            /// </summary>
            TEXBIAS_ARC = 2,

            /// <summary>
            /// 
            /// </summary>
            TEXBIAS_OBJ = 3,

            /// <summary>
            /// 
            /// </summary>
            TEXBIAS_ORG = 4,

            /// <summary>
            /// 
            /// </summary>
            TEXBIAS_SGN = 5,

            /// <summary>
            /// 
            /// </summary>
            TEXBIAS_TRN = 6,

            /// <summary>
            /// 
            /// </summary>
            TEXBIAS_PARTICLES = 7,

            /// <summary>
            /// 
            /// </summary>
            TEXBIAS_NUM = 8,
        }

        /// <summary>
        /// Enum of texture scroll types.
        /// </summary>
        public enum TextureScrollType : byte
        {
            /// <summary>
            /// 
            /// </summary>
            TEXSCROLL_NONE = 0,

            /// <summary>
            /// 
            /// </summary>
            TEXSCROLL_SMOOTH = 1,

            /// <summary>
            /// 
            /// </summary>
            TEXSCROLL_SNAP = 2,

            /// <summary>
            /// 
            /// </summary>
            TEXSCROLL_OFFSETSCALE = 3,
        }

        /// <summary>
        /// Type of tileable UV.
        /// </summary>
        public enum TextureTileableType : byte
        {
            /// <summary>
            /// 
            /// </summary>
            NONE = 0,

            /// <summary>
            /// 
            /// </summary>
            HORIZONTAL = 1,

            /// <summary>
            /// 
            /// </summary>
            VERTICAL = 2,

            /// <summary>
            /// 
            /// </summary>
            ALL_SIDES = 3,
        }

        #endregion

        #region Main Properties

        /// <summary>
        /// Collection name of the variable.
        /// </summary>
        public override string CollectionName { get; set; }

        /// <summary>
        /// Game to which the class belongs to.
        /// </summary>
        public override GameINT GameINT => GameINT.None;

        /// <summary>
        /// Game string to which the class belongs to.
        /// </summary>
        public override string GameSTR => GameINT.None.ToString();

        /// <summary>
        /// Binary memory hash of the collection name.
        /// </summary>
        public virtual uint BinKey => this.CollectionName.BinHash();

        /// <summary>
        /// Vault memory hash of the collection name.
        /// </summary>
        public virtual uint VltKey => this.CollectionName.VltHash();

        /// <summary>
        /// Represents data offset of the block in Global data.
        /// </summary>
        [MemoryCastable()]
        [Browsable(false)]
        public int Offset { get; set; } = 0;

        /// <summary>
        /// Represents data size of the block in Global data.
        /// </summary>
        [MemoryCastable()]
        [Browsable(false)]
        public int Size { get; protected set; } = 0;

        /// <summary>
        /// Represents palette offset of the block in Global data.
        /// </summary>
        [MemoryCastable()]
        [Browsable(false)]
        public int PaletteOffset { get; set; } = 0;

        /// <summary>
        /// Represents palette size of the block in Global data.
        /// </summary>
        [MemoryCastable()]
        [Browsable(false)]
        public int PaletteSize { get; protected set; } = 0;

        /// <summary>
        /// Compression type of the texture.
        /// </summary>
        [Category("Primary")]
        public abstract TextureCompressionType Compression { get; }

        /// <summary>
        /// Determines whether this <see cref="Texture"/> has palette.
        /// </summary>
        [Category("Primary")]
        public bool HasPalette => this.Compression switch
        {
            TextureCompressionType.TEXCOMP_4BIT => true,
            TextureCompressionType.TEXCOMP_4BIT_IA8 => true,
            TextureCompressionType.TEXCOMP_4BIT_RGB16_A8 => true,
            TextureCompressionType.TEXCOMP_4BIT_RGB24_A8 => true,
            TextureCompressionType.TEXCOMP_8BIT => true,
            TextureCompressionType.TEXCOMP_8BIT_16 => true,
            TextureCompressionType.TEXCOMP_8BIT_64 => true,
            TextureCompressionType.TEXCOMP_8BIT_IA8 => true,
            TextureCompressionType.TEXCOMP_8BIT_RGB16_A8 => true,
            TextureCompressionType.TEXCOMP_8BIT_RGB24_A8 => true,
            TextureCompressionType.TEXCOMP_16BIT => true,
            TextureCompressionType.TEXCOMP_16BIT_1555 => true,
            TextureCompressionType.TEXCOMP_16BIT_3555 => true,
            TextureCompressionType.TEXCOMP_16BIT_565 => true,
            _ => false
        };

        #endregion

        #region Public Properties

        /// <summary>
        /// Represents height in pixels of the texture.
        /// </summary>
        [MemoryCastable()]
        [Category("Primary")]
        public short Width { get; protected set; }

        /// <summary>
        /// Represents height in pixels of the texture.
        /// </summary>
        [MemoryCastable()]
        [Category("Primary")]
        public short Height { get; protected set; }

        /// <summary>
        /// Represents base 2 value of the width of the texture.
        /// </summary>
        [Browsable(false)]
        public byte Log_2_Width => (byte)Math.Log(this.Width, 2);

        /// <summary>
        /// Represents base 2 value of the height of the texture.
        /// </summary>
        [Browsable(false)]
        public byte Log_2_Height => (byte)Math.Log(this.Height, 2);

        /// <summary>
        /// Represents number of mipmaps in the texture.
        /// </summary>
        [MemoryCastable()]
        [Category("Primary")]
        public byte Mipmaps { get; protected set; }

        /// <summary>
        /// Represents class key of the texture.
        /// </summary>
        [AccessModifiable()]
        [MemoryCastable()]
        [Category("Secondary")]
        public uint ClassKey { get; set; } = 0x001A93CF;

        /// <summary>
        /// Represents class name of the texture. Directly linked to <see cref="ClassKey"/>.
        /// </summary>
        [AccessModifiable()]
        [MemoryCastable()]
        [Category("Secondary")]
        public string ClassName
		{
            get => this.ClassKey.BinString(LookupReturn.EMPTY);
            set => this.ClassKey = value.BinHash();
		}

        /// <summary>
        /// Represents mipmap bias type of the texture.
        /// </summary>
        [AccessModifiable()]
        [MemoryCastable()]
        [Category("Secondary")]
        public TextureMipmapBiasType MipmapBiasType { get; set; }

        /// <summary>
        /// Represents mipmap bias type of the texture as a <see cref="Byte"/>. Directly linked to <see cref="MipmapBiasType"/>.
        /// </summary>
        [AccessModifiable()]
        [MemoryCastable()]
        [Category("Secondary")]
        public byte MipmapBiasInt
		{
            get => (byte)this.MipmapBiasType;
            set => this.MipmapBiasType = (TextureMipmapBiasType)value;
		}

        /// <summary>
        /// Represents bias level of the texture.
        /// </summary>
        [AccessModifiable()]
        [MemoryCastable()]
        [Category("Secondary")]
        public byte BiasLevel { get; set; }

        /// <summary>
        /// Represents alpha usage type of the texture.
        /// </summary>
        [AccessModifiable()]
        [MemoryCastable()]
        [Category("Secondary")]
        public TextureAlphaUsageType AlphaUsageType { get; set; } = TextureAlphaUsageType.TEXUSAGE_MODULATED;

        /// <summary>
        /// Represents alpha blend type of the texture.
        /// </summary>
        [AccessModifiable()]
        [MemoryCastable()]
        [Category("Secondary")]
        public TextureAlphaBlendType AlphaBlendType { get; set; } = TextureAlphaBlendType.TEXBLEND_BLEND;

        /// <summary>
        /// Represents type of applying alpha sort of the texture.
        /// </summary>
        [AccessModifiable()]
        [MemoryCastable()]
        [Category("Secondary")]
        public byte ApplyAlphaSort { get; set; }

        /// <summary>
        /// Represents scroll type of the texture.
        /// </summary>
        [AccessModifiable()]
        [MemoryCastable()]
        [Category("Secondary")]
        public TextureScrollType ScrollType { get; set; }

        /// <summary>
        /// Represents rendering order of the texture.
        /// </summary>
        [AccessModifiable()]
        [MemoryCastable()]
        [Category("Secondary")]
        public byte RenderingOrder { get; set; } = 5;

        /// <summary>
        /// Represents tileable level of the texture.
        /// </summary>
        [AccessModifiable()]
        [MemoryCastable()]
        [Category("Secondary")]
        public TextureTileableType TileableUV { get; set; }

        /// <summary>
        /// Represents offset S of the texture.
        /// </summary>
        [AccessModifiable()]
        [MemoryCastable()]
        [Category("Secondary")]
        public short OffsetS { get; set; }

        /// <summary>
        /// Represents offset T of the texture.
        /// </summary>
        [AccessModifiable()]
        [MemoryCastable()]
        [Category("Secondary")]
        public short OffsetT { get; set; }

        /// <summary>
        /// Represents scale S of the texture.
        /// </summary>
        [AccessModifiable()]
        [MemoryCastable()]
        [Category("Secondary")]
        public short ScaleS { get; set; }

        /// <summary>
        /// Represents scale T of the texture.
        /// </summary>
        [AccessModifiable()]
        [MemoryCastable()]
        [Category("Secondary")]
        public short ScaleT { get; set; }

        /// <summary>
        /// Represents scroll time step of the texture.
        /// </summary>
        [AccessModifiable()]
        [MemoryCastable()]
        [Category("Secondary")]
        public short ScrollTimestep { get; set; }

        /// <summary>
        /// Represents scroll speed S of the texture.
        /// </summary>
        [AccessModifiable()]
        [MemoryCastable()]
        [Category("Secondary")]
        public short ScrollSpeedS { get; set; }

        /// <summary>
        /// Represents scroll speed T of the texture.
        /// </summary>
        [AccessModifiable()]
        [MemoryCastable()]
        [Category("Secondary")]
        public short ScrollSpeedT { get; set; }

        /// <summary>
        /// Represents flags of the texture.
        /// </summary>
        [AccessModifiable()]
        [MemoryCastable()]
        [Category("Secondary")]
        public byte Flags { get; set; }

        /// <summary>
        /// DDS data of this <see cref="Texture"/>.
        /// </summary>
        [Browsable(false)]
        public byte[] Data
		{
            get
			{
                this.EnsureDataLoaded();
                if (this._decodedSize == 0) return this._data;
                if (this._data is null) return null;
                return LZF.Decompress(this._data, this._decodedSize);
			}
            set
			{
                this._lazySourceFile = null;
                this._lazyBaseOffset = 0;

                if (value is null || value.Length == 0)
				{

                    this._decodedSize = 0;
                    this._data = value;

				}
                else
				{

                    this._decodedSize = value.Length;
                    this._data = LZF.Compress(value);

				}
			}
		}

        /// <summary>
        /// Length of decoded DDS data of this <see cref="Texture"/>.
        /// </summary>
        [Browsable(false)]
        public int DataLength => this._decodedSize;

        #endregion

        #region Methods

        /// <summary>
        /// Assembles <see cref="Texture"/> header into a byte array.
        /// </summary>
        /// <param name="bw"><see cref="BinaryWriter"/> to write <see cref="Texture"/> header with.</param>
        public abstract void Assemble(BinaryWriter bw);

        /// <summary>
        /// Disassembles array into <see cref="Texture"/> header properties.
        /// </summary>
        /// <param name="br"><see cref="BinaryReader"/> to read <see cref="Texture"/> header with.</param>
        public abstract void Disassemble(BinaryReader br);

        /// <summary>
        /// Serializes instance into a byte array and stores it in the file provided.
        /// </summary>
        /// <param name="bw"><see cref="BinaryWriter"/> to write data with.</param>
        public abstract void Serialize(BinaryWriter bw);

        /// <summary>
        /// Deserializes byte array into an instance by loading data from the file provided.
        /// </summary>
		/// <param name="br"><see cref="BinaryReader"/> to read data with.</param>
        public abstract void Deserialize(BinaryReader br);

        /// <summary>
        /// Gets .dds data along with the .dds header.
        /// </summary>
        /// <returns>.dds texture as a byte array.</returns>
        /// <param name="make_no_palette">True if palette should be decompressed into 
        /// 32 bpp DDS; false otherwise.</param>
        public abstract byte[] GetDDSArray(bool make_no_palette);

        /// <summary>
        /// Initializes all properties of the new <see cref="Texture"/>.
        /// </summary>
        /// <param name="filename">Filename of the .dds texture passed.</param>
        protected abstract void Initialize(string filename);

        /// <summary>
        /// Reads .dds data from the <see cref="TPKBlock"/>.
        /// </summary>
        /// <param name="br"><see cref="BinaryReader"/> to read data with.</param>
        /// <param name="forced">If forced, ignores internal offset and reads data 
        /// starting at the pointer passed.</param>
        public abstract void ReadData(BinaryReader br, bool forced);

        /// <summary>
        /// Reloads <see cref="Texture"/> properties based on the new file passed.
        /// </summary>
        /// <param name="filename">Filename of .dds texture passed.</param>
        public virtual void Reload(string filename) => this.Initialize(filename);

        /// <summary>
        /// Writes texture as a DDS file to the writer provided.
        /// </summary>
        /// <param name="bw"><see cref="BinaryWriter"/> to write DDS data with.</param>
        /// <param name="makeNoPalette">True if palette should be expanded to RGBA; false otherwise.</param>
        public virtual void WriteDDS(BinaryWriter bw, bool makeNoPalette)
        {
            if (!makeNoPalette && !string.IsNullOrEmpty(this._lazySourceFile))
            {
                this.WriteDDSHeader(bw, this.Compression, this.PaletteSize + this.Size);
                var written = this.WriteRawTextureData(bw.BaseStream);
                var expected = this.ExpectedDdsPayloadLength(this.Compression);

                if (expected > written)
                {
                    bw.WriteBytes(0, expected - written);
                }

                return;
            }

            bw.Write(this.GetDDSArray(makeNoPalette));
            this.ReleaseCachedData();
        }

        /// <summary>
        /// Configures raw payload to be loaded on demand from the original source file.
        /// </summary>
        /// <param name="filename">Path of the source file.</param>
        /// <param name="baseOffset">Base offset of the texture data block.</param>
        public void SetLazySource(string filename, long baseOffset)
        {
            this._lazySourceFile = filename;
            this._lazyBaseOffset = baseOffset;
            this._data = null;
            this._decodedSize = this.PaletteSize + this.Size;
        }

        /// <summary>
        /// Drops the in-memory payload cache for textures that can be re-read from disk.
        /// </summary>
        public void ReleaseCachedData()
        {
            if (string.IsNullOrEmpty(this._lazySourceFile)) return;
            this._data = null;
        }

        /// <summary>
        /// Casts all attributes from this object to another one.
        /// </summary>
        /// <param name="CName">CollectionName of the new created object.</param>
        /// <returns>Memory casted copy of the object.</returns>
        public override Collectable MemoryCast(string CName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns LZF compressed buffer of this <see cref="Texture"/>.
        /// </summary>
        /// <returns>LZF compressed data buffer.</returns>
        protected byte[] GetCompressedBuffer()
		{
            this.EnsureDataLoaded();
            return this._data;
		}

        /// <summary>
        /// Copies data buffer from one texture to another.
        /// </summary>
        /// <param name="from">Texture to copy data from.</param>
        /// <param name="to">Texture to copy data into.</param>
        protected static void CopyMemory(Texture from, Texture to)
		{
            to._decodedSize = from._decodedSize;
            to._lazySourceFile = from._lazySourceFile;
            to._lazyBaseOffset = from._lazyBaseOffset;

            if (from._data is null)
            {
                to._data = null;
                return;
            }

            to._data = new byte[from._data.Length];
            Array.Copy(from._data, to._data, to._data.Length);
		}

        private void EnsureDataLoaded()
        {
            if (this._data != null || this._decodedSize == 0 || string.IsNullOrEmpty(this._lazySourceFile)) return;

            lock (this._dataSync)
            {
                if (this._data != null || string.IsNullOrEmpty(this._lazySourceFile)) return;

                int total = this.PaletteSize + this.Size;
                var data = new byte[total];

                using var br = new BinaryReader(File.Open(this._lazySourceFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                var offset = this._lazyBaseOffset;
                br.BaseStream.Position = offset + this.PaletteOffset;
                Array.Copy(br.ReadBytes(this.PaletteSize), 0, data, 0, this.PaletteSize);
                br.BaseStream.Position = offset + this.Offset;
                Array.Copy(br.ReadBytes(this.Size), 0, data, this.PaletteSize, this.Size);

                this._data = LZF.Compress(data);
            }
        }

        private int WriteRawTextureData(Stream output)
        {
            using var input = File.Open(this._lazySourceFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var written = 0;
            written += CopySegment(input, output, this._lazyBaseOffset + this.PaletteOffset, this.PaletteSize);
            written += CopySegment(input, output, this._lazyBaseOffset + this.Offset, this.Size);
            return written;
        }

        private static int CopySegment(Stream input, Stream output, long offset, int count)
        {
            if (count <= 0) return 0;

            input.Position = offset;
            var buffer = new byte[Math.Min(count, 1 << 20)];
            var remaining = count;
            var written = 0;

            while (remaining > 0)
            {
                var read = input.Read(buffer, 0, Math.Min(buffer.Length, remaining));
                if (read <= 0) throw new EndOfStreamException("Unexpected end of texture source stream.");
                output.Write(buffer, 0, read);
                remaining -= read;
                written += read;
            }

            return written;
        }

        private void WriteDDSHeader(BinaryWriter bw, TextureCompressionType compression, int dataLength)
        {
            var flags = DDS_HEADER_FLAGS.TEXTURE | DDS_HEADER_FLAGS.MIPMAP;
            flags |= IsCompressed(compression) ? DDS_HEADER_FLAGS.LINEARSIZE : DDS_HEADER_FLAGS.PITCH;

            bw.Write(DDS_MAIN.MAGIC);
            bw.Write(0x7C);
            bw.WriteEnum(flags);
            bw.Write((int)this.Height);
            bw.Write((int)this.Width);
            bw.Write(PitchLinearSize(compression));
            bw.Write(1);
            bw.Write((int)this.Mipmaps);
            bw.WriteBytes(0, 0x2C);
            WritePixelFormat(bw, compression);
            bw.WriteEnum(DDS_SURFACE.SURFACE_FLAGS_ALL);
            bw.WriteBytes(0, 0x10);
        }

        private int PitchLinearSize(TextureCompressionType compression)
        {
            return compression switch
            {
                TextureCompressionType.TEXCOMP_DXTC1 => Math.Max(1, (this.Width + 3) / 4) * Math.Max(1, (this.Height + 3) / 4) * 8,
                TextureCompressionType.TEXCOMP_DXTC3 => Math.Max(1, (this.Width + 3) / 4) * Math.Max(1, (this.Height + 3) / 4) * 16,
                TextureCompressionType.TEXCOMP_DXTC5 => Math.Max(1, (this.Width + 3) / 4) * Math.Max(1, (this.Height + 3) / 4) * 16,
                TextureCompressionType.TEXCOMP_4BIT => (this.Width * 4 + 7) / 8,
                TextureCompressionType.TEXCOMP_4BIT_IA8 => (this.Width * 4 + 7) / 8,
                TextureCompressionType.TEXCOMP_4BIT_RGB16_A8 => (this.Width * 4 + 7) / 8,
                TextureCompressionType.TEXCOMP_4BIT_RGB24_A8 => (this.Width * 4 + 7) / 8,
                TextureCompressionType.TEXCOMP_8BIT => (this.Width * 8 + 7) / 8,
                TextureCompressionType.TEXCOMP_8BIT_16 => (this.Width * 8 + 7) / 8,
                TextureCompressionType.TEXCOMP_8BIT_64 => (this.Width * 8 + 7) / 8,
                TextureCompressionType.TEXCOMP_8BIT_IA8 => (this.Width * 8 + 7) / 8,
                TextureCompressionType.TEXCOMP_8BIT_RGB16_A8 => (this.Width * 8 + 7) / 8,
                TextureCompressionType.TEXCOMP_8BIT_RGB24_A8 => (this.Width * 8 + 7) / 8,
                TextureCompressionType.TEXCOMP_16BIT => (this.Width * 16 + 7) / 8,
                TextureCompressionType.TEXCOMP_16BIT_1555 => (this.Width * 16 + 7) / 8,
                TextureCompressionType.TEXCOMP_16BIT_3555 => (this.Width * 16 + 7) / 8,
                TextureCompressionType.TEXCOMP_16BIT_565 => (this.Width * 16 + 7) / 8,
                TextureCompressionType.TEXCOMP_24BIT => (this.Width * 24 + 7) / 8,
                _ => (this.Width * 32 + 7) / 8,
            };
        }

        private static bool IsCompressed(TextureCompressionType compression) =>
            compression == TextureCompressionType.TEXCOMP_DXTC1 ||
            compression == TextureCompressionType.TEXCOMP_DXTC3 ||
            compression == TextureCompressionType.TEXCOMP_DXTC5;

        private int ExpectedDdsPayloadLength(TextureCompressionType compression)
        {
            var payload = this.PaletteSize;
            var width = Math.Max(1, (int)this.Width);
            var height = Math.Max(1, (int)this.Height);
            var levels = Math.Max(1, (int)this.Mipmaps);

            for (var level = 0; level < levels; level++)
            {
                payload += compression switch
                {
                    TextureCompressionType.TEXCOMP_DXTC1 =>
                        Math.Max(1, (width + 3) / 4) * Math.Max(1, (height + 3) / 4) * 8,
                    TextureCompressionType.TEXCOMP_DXTC3 or TextureCompressionType.TEXCOMP_DXTC5 =>
                        Math.Max(1, (width + 3) / 4) * Math.Max(1, (height + 3) / 4) * 16,
                    TextureCompressionType.TEXCOMP_4BIT or
                    TextureCompressionType.TEXCOMP_4BIT_IA8 or
                    TextureCompressionType.TEXCOMP_4BIT_RGB16_A8 or
                    TextureCompressionType.TEXCOMP_4BIT_RGB24_A8 =>
                        (width * height + 1) / 2,
                    TextureCompressionType.TEXCOMP_8BIT or
                    TextureCompressionType.TEXCOMP_8BIT_16 or
                    TextureCompressionType.TEXCOMP_8BIT_64 or
                    TextureCompressionType.TEXCOMP_8BIT_IA8 or
                    TextureCompressionType.TEXCOMP_8BIT_RGB16_A8 or
                    TextureCompressionType.TEXCOMP_8BIT_RGB24_A8 =>
                        width * height,
                    TextureCompressionType.TEXCOMP_16BIT or
                    TextureCompressionType.TEXCOMP_16BIT_1555 or
                    TextureCompressionType.TEXCOMP_16BIT_3555 or
                    TextureCompressionType.TEXCOMP_16BIT_565 =>
                        width * height * 2,
                    TextureCompressionType.TEXCOMP_24BIT =>
                        width * height * 3,
                    _ =>
                        width * height * 4,
                };

                width = Math.Max(1, width >> 1);
                height = Math.Max(1, height >> 1);
            }

            return payload;
        }

        private static void WritePixelFormat(BinaryWriter bw, TextureCompressionType compression)
        {
            var format = new DDS_PIXELFORMAT();

            switch (compression)
            {
                case TextureCompressionType.TEXCOMP_DXTC1:
                    DDS_CONST.DDSPF_DXT1(format);
                    break;
                case TextureCompressionType.TEXCOMP_DXTC3:
                    DDS_CONST.DDSPF_DXT3(format);
                    break;
                case TextureCompressionType.TEXCOMP_DXTC5:
                    DDS_CONST.DDSPF_DXT5(format);
                    break;
                case TextureCompressionType.TEXCOMP_4BIT:
                    DDS_CONST.DDSPF_PAL4(format);
                    break;
                case TextureCompressionType.TEXCOMP_4BIT_IA8:
                case TextureCompressionType.TEXCOMP_4BIT_RGB16_A8:
                case TextureCompressionType.TEXCOMP_4BIT_RGB24_A8:
                    DDS_CONST.DDSPF_PAL4A(format);
                    break;
                case TextureCompressionType.TEXCOMP_8BIT:
                case TextureCompressionType.TEXCOMP_8BIT_16:
                case TextureCompressionType.TEXCOMP_8BIT_64:
                    DDS_CONST.DDSPF_PAL8(format);
                    break;
                case TextureCompressionType.TEXCOMP_8BIT_IA8:
                case TextureCompressionType.TEXCOMP_8BIT_RGB16_A8:
                case TextureCompressionType.TEXCOMP_8BIT_RGB24_A8:
                    DDS_CONST.DDSPF_PAL8A(format);
                    break;
                default:
                    DDS_CONST.DDSPF_A8R8G8B8(format);
                    break;
            }

            bw.Write(format.dwSize);
            bw.Write(format.dwFlags);
            bw.Write(format.dwFourCC);
            bw.Write(format.dwRGBBitCount);
            bw.Write(format.dwRBitMask);
            bw.Write(format.dwGBitMask);
            bw.Write(format.dwBBitMask);
            bw.Write(format.dwABitMask);
        }

        #endregion
    }
}
