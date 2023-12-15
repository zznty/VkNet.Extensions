using System.Security.Cryptography;

namespace VkNet.Extensions.Auth.Utils;

internal static class RandomString
{
	private const string Chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_-";

	public static string Generate(int length) => RandomNumberGenerator.GetString(Chars, length);
}