// This file was generated by a tool; you should avoid making direct changes.
// Consider using 'partial classes' to extend these types
// Input: checkin.proto

using ProtoBuf;

// ReSharper disable InconsistentNaming

namespace VkNet.Extensions.Auth.Models.Google;

[ProtoContract]
public class GservicesSetting : IExtensible
{
	private IExtension __pbn__extensionData;

	[ProtoMember(1, Name = @"name", IsRequired = true)]
	public byte[] Name { get; set; }

	[ProtoMember(2, Name = @"value", IsRequired = true)]
	public byte[] Value { get; set; }

	IExtension IExtensible.GetExtensionObject(bool createIfMissing)
	{
		return Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);
	}
}