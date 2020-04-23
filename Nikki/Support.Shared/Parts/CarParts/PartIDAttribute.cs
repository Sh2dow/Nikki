﻿using System;
using System.IO;
using System.Collections.Generic;
using Nikki.Reflection.Enum;



namespace Nikki.Support.Shared.Parts.CarParts
{
	/// <summary>
	/// A <see cref="CPAttribute"/> with unknown byte and part ID values.
	/// </summary>
	public class PartIDAttribute : CPAttribute
	{
		/// <summary>
		/// <see cref="eCarPartAttribType"/> type of this <see cref="PartIDAttribute"/>.
		/// </summary>
		public override eCarPartAttribType AttribType => eCarPartAttribType.CarPartID;

		/// <summary>
		/// Unknown byte value.
		/// </summary>
		public byte Unknown { get; set; }

		/// <summary>
		/// Part ID of this <see cref="PartIDAttribute"/>.
		/// </summary>
		public byte ID { get; set; }

		/// <summary>
		/// Initializes new instance of <see cref="PartIDAttribute"/>.
		/// </summary>
		public PartIDAttribute() { }

		/// <summary>
		/// Initializes new instance of <see cref="PartIDAttribute"/> by reading data using 
		/// <see cref="BinaryReader"/> provided.
		/// </summary>
		/// <param name="br"><see cref="BinaryReader"/> to read with.</param>
		/// <param name="key">Key of the attribute's group.</param>
		public PartIDAttribute(BinaryReader br, uint key)
		{
			this.Key = key;
			this.Disassemble(br, null);
		}

		/// <summary>
		/// Disassembles byte array into <see cref="PartIDAttribute"/> using <see cref="BinaryReader"/> 
		/// provided.
		/// </summary>
		/// <param name="br"><see cref="BinaryReader"/> to read with.</param>
		/// <param name="str_reader"><see cref="BinaryReader"/> to read strings with. 
		/// Since it is an Integer Attribute, this value can be <see langword="null"/>.</param>
		public override void Disassemble(BinaryReader br, BinaryReader str_reader)
		{
			this.Unknown = br.ReadByte();
			this.ID = br.ReadByte();
			br.ReadUInt16();
		}

		/// <summary>
		/// Assembles <see cref="PartIDAttribute"/> and writes it using <see cref="BinaryWriter"/> 
		/// provided.
		/// </summary>
		/// <param name="bw"><see cref="BinaryWriter"/> to write with.</param>
		/// <param name="string_dict">Dictionary of string HashCodes and their offsets. 
		/// Since it is an Integer Attribute, this value can be <see langword="null"/>.</param>
		public override void Assemble(BinaryWriter bw, Dictionary<int, int> string_dict)
		{
			bw.Write(this.Key);
			bw.Write(this.Unknown);
			bw.Write(this.ID);
			bw.Write((ushort)0);
		}

		/// <summary>
		/// Returns attribute part label and its type as a string value.
		/// </summary>
		/// <returns>String value.</returns>
		public override string ToString() => base.ToString();

		/// <summary>
		/// Determines whether this instance and a specified object, which must also be a
		/// <see cref="PartIDAttribute"/> object, have the same value.
		/// </summary>
		/// <param name="obj">The <see cref="PartIDAttribute"/> to compare to this instance.</param>
		/// <returns>True if obj is a <see cref="PartIDAttribute"/> and its value is the same as 
		/// this instance; false otherwise. If obj is null, the method returns false.
		/// </returns>
		public override bool Equals(object obj) =>
			obj is PartIDAttribute && this == (PartIDAttribute)obj;

		/// <summary>
		/// Returns the hash code for this <see cref="PartIDAttribute"/>.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode() => 
			Tuple.Create(this.Key, this.ID, this.Unknown).GetHashCode();

		/// <summary>
		/// Determines whether two specified <see cref="PartIDAttribute"/> have the same value.
		/// </summary>
		/// <param name="at1">The first <see cref="PartIDAttribute"/> to compare, or null.</param>
		/// <param name="at2">The second <see cref="PartIDAttribute"/> to compare, or null.</param>
		/// <returns>True if the value of c1 is the same as the value of c2; false otherwise.</returns>
		public static bool operator ==(PartIDAttribute at1, PartIDAttribute at2) =>
			at1 is null ? at2 is null : at2 is null ? false
			: (at1.Key == at2.Key && at1.ID == at2.ID && at1.Unknown == at2.Unknown);

		/// <summary>
		/// Determines whether two specified <see cref="PartIDAttribute"/> have different values.
		/// </summary>
		/// <param name="at1">The first <see cref="PartIDAttribute"/> to compare, or null.</param>
		/// <param name="at2">The second <see cref="PartIDAttribute"/> to compare, or null.</param>
		/// <returns>True if the value of c1 is different from the value of c2; false otherwise.</returns>
		public static bool operator !=(PartIDAttribute at1, PartIDAttribute at2) => !(at1 == at2);
	}
}
