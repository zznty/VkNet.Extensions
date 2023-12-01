// This file was generated by a tool; you should avoid making direct changes.
// Consider using 'partial classes' to extend these types
// Input: checkin.proto

using System.ComponentModel;
using ProtoBuf;

// ReSharper disable InconsistentNaming

namespace VkNet.Extensions.Auth.Models.Google;

[ProtoContract]
public class AndroidCheckinResponse : IExtensible
{
	private ulong? __pbn__AndroidId;

	private string __pbn__Digest;

	private IExtension __pbn__extensionData;

	private bool? __pbn__MarketOk;

	private ulong? __pbn__SecurityToken;

	private bool? __pbn__SettingsDiff;

	private long? __pbn__TimeMsec;

	private string __pbn__VersionInfo;

	[ProtoMember(1, Name = @"stats_ok", IsRequired = true)]
	public bool StatsOk { get; set; }

	[ProtoMember(3, Name = @"time_msec")]
	public long TimeMsec
	{
		get => __pbn__TimeMsec.GetValueOrDefault();
		set => __pbn__TimeMsec = value;
	}

	[ProtoMember(4, Name = @"digest")]
	[DefaultValue("")]
	public string Digest
	{
		get => __pbn__Digest ?? "";
		set => __pbn__Digest = value;
	}

	[ProtoMember(9, Name = @"settings_diff")]
	public bool SettingsDiff
	{
		get => __pbn__SettingsDiff.GetValueOrDefault();
		set => __pbn__SettingsDiff = value;
	}

	[ProtoMember(10, Name = @"delete_setting")]
	public List<string> DeleteSettings { get; } = new();

	[ProtoMember(5, Name = @"setting")]
	public List<GservicesSetting> Settings { get; } = new();

	[ProtoMember(6, Name = @"market_ok")]
	public bool MarketOk
	{
		get => __pbn__MarketOk.GetValueOrDefault();
		set => __pbn__MarketOk = value;
	}

	[ProtoMember(7, Name = @"android_id", DataFormat = DataFormat.FixedSize)]
	public ulong AndroidId
	{
		get => __pbn__AndroidId.GetValueOrDefault();
		set => __pbn__AndroidId = value;
	}

	[ProtoMember(8, Name = @"security_token", DataFormat = DataFormat.FixedSize)]
	public ulong SecurityToken
	{
		get => __pbn__SecurityToken.GetValueOrDefault();
		set => __pbn__SecurityToken = value;
	}

	[ProtoMember(11, Name = @"version_info")]
	[DefaultValue("")]
	public string VersionInfo
	{
		get => __pbn__VersionInfo ?? "";
		set => __pbn__VersionInfo = value;
	}

	IExtension IExtensible.GetExtensionObject(bool createIfMissing)
	{
		return Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);
	}

	public bool ShouldSerializeTimeMsec()
	{
		return __pbn__TimeMsec != null;
	}

	public void ResetTimeMsec()
	{
		__pbn__TimeMsec = null;
	}

	public bool ShouldSerializeDigest()
	{
		return __pbn__Digest != null;
	}

	public void ResetDigest()
	{
		__pbn__Digest = null;
	}

	public bool ShouldSerializeSettingsDiff()
	{
		return __pbn__SettingsDiff != null;
	}

	public void ResetSettingsDiff()
	{
		__pbn__SettingsDiff = null;
	}

	public bool ShouldSerializeMarketOk()
	{
		return __pbn__MarketOk != null;
	}

	public void ResetMarketOk()
	{
		__pbn__MarketOk = null;
	}

	public bool ShouldSerializeAndroidId()
	{
		return __pbn__AndroidId != null;
	}

	public void ResetAndroidId()
	{
		__pbn__AndroidId = null;
	}

	public bool ShouldSerializeSecurityToken()
	{
		return __pbn__SecurityToken != null;
	}

	public void ResetSecurityToken()
	{
		__pbn__SecurityToken = null;
	}

	public bool ShouldSerializeVersionInfo()
	{
		return __pbn__VersionInfo != null;
	}

	public void ResetVersionInfo()
	{
		__pbn__VersionInfo = null;
	}
}