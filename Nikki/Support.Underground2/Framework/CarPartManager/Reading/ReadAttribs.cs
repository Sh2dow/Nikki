﻿using System.IO;
using Nikki.Core;
using Nikki.Reflection.ID;
using Nikki.Reflection.Enum;
using Nikki.Support.Shared.Parts.CarParts;



namespace Nikki.Support.Underground2.Framework
{
	public static partial class CarPartManager
	{
		private static CPAttribute[] ReadAttribs(BinaryReader br, BinaryReader str)
		{
			var offset = br.BaseStream.Position + 8;
			if (br.ReadUInt32() != CarParts.DBCARPART_ATTRIBS) return null;
			var size = br.ReadInt32();
			var result = new CPAttribute[size / 8]; // set initial capacity

			int count = 0;
			while (br.BaseStream.Position < offset + size)
			{
				var key = br.ReadUInt32();
				if (!Map.CarPartKeys.TryGetValue(key, out var type))
					type = eCarPartAttribType.Integer;
				result[count] = type switch
				{
					eCarPartAttribType.Boolean => new BoolAttribute(br, key),
					eCarPartAttribType.CarPartID => new PartIDAttribute(br, key),
					eCarPartAttribType.Floating => new FloatAttribute(br, key),
					eCarPartAttribType.String => new StringAttribute(br, str, key),
					eCarPartAttribType.TwoString => new TwoStringAttribute(br, str, key),
					_ => new IntAttribute(br, key),
				};
				++count;
			}
			return result;
		}
	}
}