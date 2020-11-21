﻿using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;
using Nikki.Core;
using Nikki.Utils;
using Nikki.Reflection.Enum;
using Nikki.Reflection.Abstract;
using Nikki.Reflection.Exception;
using Nikki.Reflection.Attributes;
using Nikki.Reflection.Enum.PartID;
using Nikki.Support.Underground1.Class;
using Nikki.Support.Shared.Parts.CarParts;
using Nikki.Support.Underground1.Attributes;
using CoreExtensions.Conversions;



namespace Nikki.Support.Underground1.Parts.CarParts
{
	/// <summary>
	/// A unit CarPart attribute of <see cref="DBModelPart"/>.
	/// </summary>
	[DebuggerDisplay("PartName: {PartName} | AttribCount: {Attributes.Count} | GroupID: {CarPartGroupID}")]
	public class RealCarPart : Shared.Parts.CarParts.RealCarPart
	{
		/// <summary>
		/// Name of this <see cref="RealCarPart"/>.
		/// </summary>
		public override string PartName => this.ToString();

		/// <summary>
		/// <see cref="DBModelPart"/> to which this instance belongs to.
		/// </summary>
		[Browsable(false)]
		public override Shared.Class.DBModelPart Model { get; set; }

		/// <summary>
		/// Collection of <see cref="CPAttribute"/> of this <see cref="RealCarPart"/>.
		/// </summary>
		[Browsable(false)]
		public override List<CPAttribute> Attributes { get; }

		/// <summary>
		/// Debug name of this <see cref="RealCarPart"/>.
		/// </summary>
		[AccessModifiable()]
		public string DebugName { get; set; }

		/// <summary>
		/// Part label of the car part.
		/// </summary>
		[AccessModifiable()]
		public string PartLabel { get; set; } = String.Empty;

		/// <summary>
		/// Brand label of the car part.
		/// </summary>
		[AccessModifiable()]
		public string BrandLabel { get; set; } = String.Empty;

		/// <summary>
		/// Car Part ID Group to which this part belongs to.
		/// </summary>
		[AccessModifiable()]
		public PartUnderground1 CarPartGroupID { get; set; } = PartUnderground1.INVALID;

		/// <summary>
		/// Upgrade group ID of this <see cref="RealCarPart"/>.
		/// </summary>
		[AccessModifiable()]
		public byte UpgradeGroupID { get; set; }

		/// <summary>
		/// Upgrade style of this <see cref="RealCarPart"/>.
		/// </summary>
		[AccessModifiable()]
		public byte UpgradeStyle { get; set; }

		/// <summary>
		/// Geometry Lod A label of the part.
		/// </summary>
		[AccessModifiable()]
		public string GeometryLodA { get; set; }

		/// <summary>
		/// Geometry Lod B label of the part.
		/// </summary>
		[AccessModifiable()]
		public string GeometryLodB { get; set; }

		/// <summary>
		/// Geometry Lod C label of the part.
		/// </summary>
		[AccessModifiable()]
		public string GeometryLodC { get; set; }

		/// <summary>
		/// Geometry Lod D label of the part.
		/// </summary>
		[AccessModifiable()]
		public string GeometryLodD { get; set; }

		/// <summary>
		/// Initialize new instance of <see cref="RealCarPart"/>.
		/// </summary>
		public RealCarPart() => this.Attributes = new List<CPAttribute>();

		/// <summary>
		/// Initializes new instance of <see cref="RealCarPart"/>.
		/// </summary>
		/// <param name="model"><see cref="DBModelPart"/> to which this instance belongs to.</param>
		public RealCarPart(DBModelPart model) : this() { this.Model = model; }

		/// <summary>
		/// Initializes new instance of <see cref="RealCarPart"/>.
		/// </summary>
		/// <param name="capacity">Initial capacity of the attribute list.</param>
		public RealCarPart(int capacity) => this.Attributes = new List<CPAttribute>(capacity);

		/// <summary>
		/// Initializes new instance of <see cref="RealCarPart"/>.
		/// </summary>
		/// <param name="model"><see cref="DBModelPart"/> to which this instance belongs to.</param>
		/// <param name="capacity">Initial capacity of the attribute list.</param>
		public RealCarPart(DBModelPart model, int capacity) : this(capacity) { this.Model = model; }

		/// <summary>
		/// Returns PartName, Attributes count and CarPartGroupID as a string value.
		/// </summary>
		/// <returns>String value.</returns>
		public override string ToString() =>
			String.IsNullOrEmpty(this.PartLabel) ? "REAL_CAR_PART" : this.PartLabel;

		/// <summary>
		/// Returns the hash code for this <see cref="RealCarPart"/>.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode()
		{
			int result = (int)this.PartName.BinHash();

			foreach (var attribute in this.Attributes)
			{

				result = HashCode.Combine(result, attribute.GetHashCode());

			}

			result = HashCode.Combine(result, this.CarPartGroupID);
			result = HashCode.Combine(result, this.DebugName);
			result = HashCode.Combine(result, this.UpgradeGroupID);
			result = HashCode.Combine(result, this.BrandLabel);
			result = HashCode.Combine(result, this.UpgradeStyle);
			result = HashCode.Combine(result, this.GeometryLodA);
			result = HashCode.Combine(result, this.GeometryLodB);
			result = HashCode.Combine(result, this.GeometryLodC);
			result = HashCode.Combine(result, this.GeometryLodD);

			return result;
		}

		/// <summary>
		/// Gets <see cref="CPAttribute"/> with the key provided.
		/// </summary>
		/// <param name="key">Key of a <see cref="CPAttribute"/> to find.</param>
		/// <returns>A <see cref="CPAttribute"/> with key provided.</returns>
		public override CPAttribute GetAttribute(uint key) => this.Attributes.Find(_ => _.Key == key);

		/// <summary>
		/// Gets <see cref="CPAttribute"/> with the label provided.
		/// </summary>
		/// <param name="label">Label of a <see cref="CPAttribute"/> to find.</param>
		/// <returns>A <see cref="CPAttribute"/> with label provided.</returns>
		public override CPAttribute GetAttribute(string label) => this.GetAttribute(label.BinHash());

		/// <summary>
		/// Gets <see cref="CPAttribute"/> at index specified.
		/// </summary>
		/// <param name="index">Index in the list of <see cref="CPAttribute"/>.</param>
		/// <returns>A <see cref="CPAttribute"/> at index specified.</returns>
		public override CPAttribute GetAttribute(int index) =>
			(index >= 0 && index < this.Length) ? this.Attributes[index] : null;

		/// <summary>
		/// Adds <see cref="CPAttribute"/> with key provided.
		/// </summary>
		/// <param name="key">Key of the new <see cref="CPAttribute"/>.</param>
		public override void AddAttribute(uint key)
		{
			if (!Map.CarPartKeys.TryGetValue(key, out var type))
			{

				throw new MappingFailException($"Attribute of key type 0x{key:X8} is not a valid attribute");

			}

			CPAttribute attribute = type switch
			{
				CarPartAttribType.Boolean => new BoolAttribute(eBoolean.False),
				CarPartAttribType.Floating => new FloatAttribute((float)0),
				CarPartAttribType.String => new StringAttribute(String.Empty),
				CarPartAttribType.Key => new KeyAttribute(String.Empty),
				_ => new IntAttribute((int)0)
			};

			attribute.Key = key;
			this.Attributes.Add(attribute);
		}

		/// <summary>
		/// Adds <see cref="CPAttribute"/> with label provided.
		/// </summary>
		/// <param name="label">Label of the new <see cref="CPAttribute"/>.</param>
		public override void AddAttribute(string label) =>
			this.AddAttribute(label.BinHash());

		/// <summary>
		/// Removes <see cref="CPAttribute"/> with key provided.
		/// </summary>
		/// <param name="key">Key of the <see cref="CPAttribute"/> to remove.</param>
		public override void RemoveAttribute(uint key)
		{
			if (!this.Attributes.RemoveWith(_ => _.Key == key))
			{

				throw new InfoAccessException($"0x{key:X8}");

			}
		}

		/// <summary>
		/// Removes <see cref="CPAttribute"/> with key provided.
		/// </summary>
		/// <param name="label">Label of the <see cref="CPAttribute"/> to remove.</param>
		public override void RemoveAttribute(string label) =>
			this.RemoveAttribute(label.BinHash());

		/// <summary>
		/// Clones <see cref="CPAttribute"/> with key provided.
		/// </summary>
		/// <param name="newkey">Key of the new <see cref="CPAttribute"/>.</param>
		/// <param name="copykey">Key of the <see cref="CPAttribute"/> to clone.</param>
		public override void CloneAttribute(uint newkey, uint copykey)
		{
			var attribute = this.GetAttribute(copykey);

			if (attribute == null)
			{

				throw new InfoAccessException($"0x{copykey:X8}");

			}

			if (!Map.CarPartKeys.TryGetValue(newkey, out var type))
			{

				throw new MappingFailException($"Attribute of key type 0x{newkey:X8} is not a valid attribute");

			}

			var result = (CPAttribute)attribute.PlainCopy();
			result = result.ConvertTo(type);
			result.Key = newkey;
			this.Attributes.Add(result);
		}

		/// <summary>
		/// Clones <see cref="CPAttribute"/> with label provided.
		/// </summary>
		/// <param name="newkey">Key of the new <see cref="CPAttribute"/>.</param>
		/// <param name="copylabel">Label of the <see cref="CPAttribute"/> to clone.</param>
		public override void CloneAttribute(uint newkey, string copylabel) =>
			this.CloneAttribute(newkey, copylabel.BinHash());

		/// <summary>
		/// Clones <see cref="CPAttribute"/> with key provided.
		/// </summary>
		/// <param name="newlabel">Label of the new <see cref="CPAttribute"/>.</param>
		/// <param name="copykey">Key of the <see cref="CPAttribute"/> to clone.</param>
		public override void CloneAttribute(string newlabel, uint copykey) =>
			this.CloneAttribute(newlabel.BinHash(), copykey);

		/// <summary>
		/// Clones <see cref="CPAttribute"/> with label provided.
		/// </summary>
		/// <param name="newlabel">Label of the new <see cref="CPAttribute"/>.</param>
		/// <param name="copylabel">Label of the <see cref="CPAttribute"/> to clone.</param>
		public override void CloneAttribute(string newlabel, string copylabel) =>
			this.CloneAttribute(newlabel.BinHash(), copylabel.BinHash());

		/// <summary>
		/// Adds a custom attribute type to this part.
		/// </summary>
		/// <param name="name">Name of custom attribute to add.</param>
		/// <param name="type">Type of custom attribute to add.</param>
		public override void AddCustomAttribute(string name, CarPartAttribType type)
		{
			//this.Attributes.Add(new CustomAttribute(name, type));
		}

		/// <summary>
		/// Compares two <see cref="RealCarPart"/> and checks whether the equal.
		/// </summary>
		/// <param name="other"><see cref="RealCarPart"/> to compare this instance to.</param>
		/// <returns>True if this instance equals another instance passed; false otherwise.</returns>
		public override bool Equals(object other)
		{
			if (other is RealCarPart part)
			{

				if (part.PartLabel.BinHash() != this.PartLabel.BinHash()) return false;
				if (part.Attributes.Count != this.Attributes.Count) return false;

				var thislist = new List<CPAttribute>(this.Attributes);
				var otherlist = new List<CPAttribute>(part.Attributes);

				thislist.Sort((x, y) => x.Key.CompareTo(y.Key));
				otherlist.Sort((x, y) => x.Key.CompareTo(y.Key));

				for (int loop = 0; loop < this.Length; ++loop)
				{

					if (!thislist[loop].Equals(otherlist[loop])) return false;

				}

				bool result = true;
				result &= this.CarPartGroupID == part.CarPartGroupID;
				result &= this.DebugName == part.DebugName;
				result &= this.UpgradeGroupID == part.UpgradeGroupID;
				result &= this.UpgradeStyle == part.UpgradeStyle;
				result &= this.BrandLabel == part.BrandLabel;
				result &= this.GeometryLodA == part.GeometryLodA;
				result &= this.GeometryLodB == part.GeometryLodB;
				result &= this.GeometryLodC == part.GeometryLodC;
				result &= this.GeometryLodD == part.GeometryLodD;
				return result;

			}
			else return false;
		}

		/// <summary>
		/// Creates a plain copy of the objects that contains same values.
		/// </summary>
		/// <returns>Exact plain copy of the object.</returns>
		public override SubPart PlainCopy()
		{
			var result = new RealCarPart(this.Length)
			{
				BrandLabel = this.BrandLabel,
				CarPartGroupID = this.CarPartGroupID,
				DebugName = this.DebugName,
				GeometryLodA = this.GeometryLodA,
				GeometryLodB = this.GeometryLodB,
				GeometryLodC = this.GeometryLodC,
				GeometryLodD = this.GeometryLodD,
				PartLabel = this.PartLabel,
				UpgradeGroupID = this.UpgradeGroupID,
				UpgradeStyle = this.UpgradeStyle,
			};

			foreach (var attrib in this.Attributes)
			{

				result.Attributes.Add((CPAttribute)attrib.PlainCopy());

			}

			return result;
		}

		/// <summary>
		/// Clones values of another <see cref="SubPart"/>.
		/// </summary>
		/// <param name="other"><see cref="SubPart"/> to clone.</param>
		public override void CloneValuesFrom(SubPart other)
		{
			if (other is RealCarPart part)
			{

				this.CarPartGroupID = part.CarPartGroupID;
				this.DebugName = part.DebugName;
				this.BrandLabel = part.BrandLabel;
				this.PartLabel = part.PartLabel;
				this.UpgradeGroupID = part.UpgradeGroupID;
				this.UpgradeStyle = part.UpgradeStyle;
				this.GeometryLodA = part.GeometryLodA;
				this.GeometryLodB = part.GeometryLodB;
				this.GeometryLodC = part.GeometryLodC;
				this.GeometryLodD = part.GeometryLodD;
				this.Attributes.Capacity = part.Attributes.Capacity;

				foreach (var attrib in part.Attributes)
				{

					this.Attributes.Add((CPAttribute)attrib.PlainCopy());

				}

			}
		}
	}
}
