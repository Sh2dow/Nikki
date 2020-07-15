﻿using System;
using System.IO;
using System.Collections.Generic;
using Nikki.Core;
using Nikki.Utils;
using Nikki.Reflection.Enum;
using CoreExtensions.IO;
using CoreExtensions.Management;



namespace Nikki.Support.Underground2.Framework
{
	internal class DatabaseLoader : IDisposable
	{
		private readonly Options _options = Options.Default;
		private readonly Datamap _db;
		private readonly Logger _logger;

		#if DEBUG
		private Dictionary<uint, List<long>> _offsets = new Dictionary<uint, List<long>>();
		#endif

		private Block acideffects;
		private Block acidemitters;
		private Block caranimations;
		private Block carskins;
		private Block cartypeinfos;
		private Block dbmodelparts;
		private Block gcareer;
		private Block fngroups;
		private Block materials;
		private Block presetrides;
		private Block slottypes;
		private Block strblocks;
		private Block suninfos;
		private Block tpkblocks;
		private Block tracks;

		public DatabaseLoader(Options options, Datamap db)
		{
			this._options = options;
			this._db = db;
			this._logger = new Logger("MainLog.txt", "Nikki.dll : Underground2 DatabaseLoader", true);
			this.materials = new Block(BinBlockID.Materials);
			this.tpkblocks = new Block(BinBlockID.TPKBlocks);
			this.cartypeinfos = new Block(BinBlockID.CarTypeInfos);
			this.presetrides = new Block(BinBlockID.PresetRides);
			this.dbmodelparts = new Block(BinBlockID.DBCarParts);
			this.suninfos = new Block(BinBlockID.SunInfos);
			this.tracks = new Block(BinBlockID.Tracks);
			this.fngroups = new Block(BinBlockID.FEngFiles);
			this.strblocks = new Block(BinBlockID.STRBlocks);
			this.slottypes = new Block(BinBlockID.SlotTypes);
			this.acideffects = new Block(BinBlockID.AcidEffects);
			this.acidemitters = new Block(BinBlockID.AcidEmitters);
			this.gcareer = new Block(BinBlockID.GCareer);
			this.carskins = new Block(BinBlockID.CarSkins);
			this.caranimations = new Block(BinBlockID.CarInfoAnimHookup);
		}

		public void Invoke()
		{
			var info = new FileInfo(this._options.File);
			if (!info.Exists) return;

			var comp = this.NeedsDecompression();
			if (!comp && info.Length > (1 << 26)) this.ReadFromStream();
			else this.ReadFromBuffer(comp);

			#if DEBUG
			foreach (var pair in this._offsets)
			{

				this._logger.WriteLine($"File: {this._options.File}");
				this._logger.Write($"0x{pair.Key:X8} | ");

				foreach (var off in pair.Value)
				{

					this._logger.Write($" ---> 0x{off:X8}");

				}

				this._logger.WriteLine(String.Empty);

			}
			#endif

			ForcedX.GCCollect();
		}

		private bool NeedsDecompression()
		{
			var array = new byte[4];
			using var fs = new FileStream(this._options.File, FileMode.Open, FileAccess.Read);
			fs.Read(array, 0, 4);
			var type = BitConverter.ToInt32(array, 0);
			return Enum.IsDefined(typeof(LZCompressionType), type);
		}

		private void ReadFromStream()
		{
			using var br = new BinaryReader(File.Open(this._options.File, FileMode.Open, FileAccess.Read));
			this.Disassemble(br);
		}

		private void ReadFromBuffer(bool compressed)
		{
			var buffer = File.ReadAllBytes(this._options.File);
			if (compressed) buffer = Interop.Decompress(buffer);
			using var ms = new MemoryStream(buffer);
			using var br = new BinaryReader(ms);
			this.Disassemble(br);
		}

		public void Disassemble(BinaryReader br)
		{
			try
			{

				this.ReadBlockOffsets(br);

				this._db.STRBlocks.Disassemble(br, this.strblocks);
				this._db.Materials.Disassemble(br, this.materials);
				this._db.TPKBlocks.Disassemble(br, this.tpkblocks);
				this._db.CarTypeInfos.Disassemble(br, this.cartypeinfos);
				this._db.DBModelParts.Disassemble(br, this.dbmodelparts);
				this._db.Tracks.Disassemble(br, this.tracks);
				this._db.SunInfos.Disassemble(br, this.suninfos);
				this._db.PresetRides.Disassemble(br, this.presetrides);
				this._db.FNGroups.Disassemble(br, this.fngroups);
				this._db.SlotTypes.Disassemble(br, this.slottypes);
				this._db.SlotOverrides.Disassemble(br, this.slottypes);
				this._db.AcidEffects.Disassemble(br, this.acideffects);
				this._db.AcidEmitters.Disassemble(br, this.acidemitters);
				this.ProcessCarAnimations(br);
				this.ProcessCarSkins(br);

			}
			catch (Exception e)
			{
			
				this._logger.WriteException(e, br.BaseStream);
			
			}
		}

		private void ReadBlockOffsets(BinaryReader br)
		{
			while (br.BaseStream.Position < br.BaseStream.Length)
			{

				var off = br.BaseStream.Position;
				var id = br.ReadEnum<BinBlockID>();
				var size = br.ReadInt32();

				#if DEBUG
				if (!Enum.IsDefined(typeof(BinBlockID), (uint)id))
				{

					Console.WriteLine("Located unknown data block. Please send MailLog file to the developer!!!");

					if (this._offsets.TryGetValue((uint)id, out var list))
					{

						list.Add(off);

					}
					else
					{

						list = new List<long>() { off };
						this._offsets[(uint)id] = list;

					}

				}
				#endif

				switch (id)
				{
					case BinBlockID.FX:
						throw new NotSupportedException("FX Effects files are not supported");

					case BinBlockID.ABKC:
						throw new NotSupportedException("ABKC Sound files are not supported");

					case BinBlockID.LOCH:
						throw new NotSupportedException("LOCH Localization files are not supported");

					case BinBlockID.VPAK:
						throw new NotSupportedException("VPAK Vault files are not supported");

					case BinBlockID.MOIR:
						throw new NotSupportedException("MOIR Sound files are not supported");

					case BinBlockID.MEMO:
						throw new NotSupportedException("Memory Data files are not supported");

					case BinBlockID.MVhd:
						throw new NotSupportedException("VP6 Encoded files are not supported");

					case BinBlockID.Gnsu:
						throw new NotSupportedException("GNSU Sound files are not supported");

					case BinBlockID.Materials:
						this.materials.Offsets.Add(off);
						goto default;

					case BinBlockID.TPKBlocks:
					case BinBlockID.TPKSettings:
						this.tpkblocks.Offsets.Add(off);
						goto default;

					case BinBlockID.CarTypeInfos:
						this.cartypeinfos.Offsets.Add(off);
						goto default;

					case BinBlockID.PresetRides:
						this.presetrides.Offsets.Add(off);
						goto default;

					case BinBlockID.GCareer:
						this.gcareer.Offsets.Add(off);
						goto default;

					case BinBlockID.DBCarParts:
						this.dbmodelparts.Offsets.Add(off);
						goto default;

					case BinBlockID.SunInfos:
						this.suninfos.Offsets.Add(off);
						goto default;

					case BinBlockID.Tracks:
						this.tracks.Offsets.Add(off);
						goto default;

					case BinBlockID.AcidEffects:
						this.acideffects.Offsets.Add(off);
						goto default;

					case BinBlockID.AcidEmitters:
						this.acidemitters.Offsets.Add(off);
						goto default;

					case BinBlockID.STRBlocks:
						this.strblocks.Offsets.Add(off);
						goto default;

					case BinBlockID.FEngFiles:
					case BinBlockID.FNGCompress:
						this.fngroups.Offsets.Add(off);
						goto default;

					case BinBlockID.SlotTypes:
						this.slottypes.Offsets.Add(off);
						goto default;

					case BinBlockID.CarInfoAnimHookup:
						this.caranimations.Offsets.Add(off);
						goto default;

					case BinBlockID.CarSkins:
						this.carskins.Offsets.Add(off);
						goto default;

					default:
						br.BaseStream.Position += size;
						break;

				}

			}
		}

		private void ProcessCarAnimations(BinaryReader br)
		{
			var manager = this._db.SlotTypes;
			if (manager == null) return;
			
			for (int loop = 0; loop < this.caranimations.Offsets.Count; ++loop)
			{

				br.BaseStream.Position = this.caranimations.Offsets[loop] + 4;
				var size = br.ReadInt32();

				if (size < manager.Count)
				{

					throw new InvalidDataException("CarAnimHookup block has invalid or corrupted data");

				}

				for (int i = 0; i < manager.Count; ++i)
				{

					manager[i].PrimaryAnimation = br.ReadEnum<Shared.Class.SlotType.CarAnimLocation>();
					manager[i].SecondaryAnimation = br.ReadEnum<Shared.Class.SlotType.CarAnimLocation>();

				}

			}
		}

		private void ProcessCarSkins(BinaryReader br)
		{
			var manager = this._db.CarTypeInfos;
			if (manager == null) return;

			for (int loop = 0; loop < this.carskins.Offsets.Count; ++loop)
			{

				br.BaseStream.Position = this.carskins.Offsets[loop] + 4;
				var size = br.ReadInt32();

				for (int i = 0; i < size >> 6; ++i)
				{

					var id = br.ReadInt32();
					br.BaseStream.Position -= 4;

					var car = manager[id];
					if (!(car is null)) car.SetCarSkins(br);

				}

			}
		}

		public void Dispose() => this._logger.Dispose();
	}
}
