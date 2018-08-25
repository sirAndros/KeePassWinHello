using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using KeePass.App.Configuration;
using KeePassLib.Cryptography.Cipher;
using KeePassLib.Keys;
using KeePassLib.Security;
using KeePassLib.Utility;

namespace KeePassWinHello
{
	public static class Extensions
	{
		/// <summary>A CompositeKey extension method that combines keys.</summary>
		/// <remarks>Keep in sync with CompositeKey.CreateRawCompositeKey32().</remarks>
		/// <param name="key">The CompositeKey to act on.</param>
		/// <returns>A ProtectedBinary with the combined keys.</returns>
		public static ProtectedBinary CombineKeys(this CompositeKey key)
		{
			var dataList = new List<byte[]>();
			int dataLength = 0;
			foreach (var pKey in key.UserKeys)
			{
				var b = pKey.KeyData;
				if (b != null)
				{
					var keyData = b.ReadData();
					dataList.Add(keyData);
					dataLength += keyData.Length;
				}
			}

			var allData = new byte[dataLength];
			int p = 0;
			foreach (var pbData in dataList)
			{
				Array.Copy(pbData, 0, allData, p, pbData.Length);
				p += pbData.Length;
				MemUtil.ZeroByteArray(pbData);
			}

			var pb = new ProtectedBinary(true, allData);
			MemUtil.ZeroByteArray(allData);
			return pb;
		}

		/// <summary>
		/// Encrypts the contained data with the specific <see cref="CtrBlockCipher"/>.
		/// </summary>
		/// <param name="pb">The data to encrypt.</param>
		/// <param name="cipher">The cipher to use.</param>
		/// <returns>A new <see cref="ProtectedBinary"/> which contains the encrypted data.</returns>
		public static ProtectedBinary Encrypt(this ProtectedBinary pb, CtrBlockCipher cipher)
		{
			var data = pb.ReadData();

			cipher.Encrypt(data, 0, data.Length);

			var result = new ProtectedBinary(true, data);

			MemUtil.ZeroByteArray(data);

			return result;
		}

		/// <summary>
		/// Decrypts the contained data with the specific <see cref="CtrBlockCipher"/>.
		/// </summary>
		/// <param name="pb">The data to decrypt.</param>
		/// <param name="cipher">The cipher to use.</param>
		/// <returns>A new <see cref="ProtectedBinary"/> which contains the decrypted data.</returns>
		public static ProtectedBinary Decrypt(this ProtectedBinary pb, CtrBlockCipher cipher)
		{
			var data = pb.ReadData();

			cipher.Decrypt(data, 0, data.Length);

			var result = new ProtectedBinary(true, data);

			MemUtil.ZeroByteArray(data);

			return result;
		}

		/// <summary>A ProtectedString extension method that gets the characters.</summary>
		/// <param name="ps">The ProtectedString to act on.</param>
		/// <returns>An array of characters.</returns>
		public static char[] GetChars(this ProtectedString ps)
		{
			var pb = ps.ReadUtf8();

			var chars = StrUtil.Utf8.GetChars(pb);

			MemUtil.ZeroByteArray(pb);

			return chars;
		}

		/// <summary>A char[] extension method that converts the chars to a protected string.</summary>
		/// <param name="chars">The chars to act on.</param>
		/// <returns>The given data converted to a ProtectedString.</returns>
		public static ProtectedString ToProtectedString(this char[] chars)
		{
			return chars.ToProtectedString(0, chars.Length);
		}

		/// <summary>A char[] extension method that converts this object to a protected string.</summary>
		/// <param name="chars">The chars to act on.</param>
		/// <param name="charIndex">Zero-based index of the first character.</param>
		/// <param name="charCount">Number of characters.</param>
		/// <returns>The given data converted to a ProtectedString.</returns>
		public static ProtectedString ToProtectedString(this char[] chars, int charIndex, int charCount)
		{
			var pb = StrUtil.Utf8.GetBytes(chars, charIndex, charCount);
			var ps = new ProtectedString(true, pb);
			MemUtil.ZeroByteArray(pb);
			return ps;
		}
	}
}
