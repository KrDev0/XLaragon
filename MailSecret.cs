using System;
using System.Globalization;
using System.Text;

namespace Laragon
{
    /// <summary>
    /// The mail password as the original stores it in <c>usr\laragon.ini</c>: not in plain text,
    /// but as two hex digits per byte of a simple XOR stream. Both halves were read out of
    /// laragon.exe this round.
    ///
    /// <para>Encode — <c>FUN_100095e40</c> (0x100095e40), reached from the password box's change
    /// handler <c>FUN_1000897a0</c> (0x1000897a0), which writes the result to
    /// <c>[preferences] MailSenderPassword</c> through <c>FUN_100096350</c> (0x100096350).
    /// Every cipher byte is written with <c>IntToHex(byte, 2)</c>, so the value is upper-case hex
    /// and twice as long as the password:</para>
    /// <code>
    /// key = 0x1f4b
    /// for each byte b:  c = b XOR (key >> 8);  key = ((c + key) * 0x302d - 0x4cc0) and 0xffff
    /// </code>
    ///
    /// <para>Decode — <c>FUN_100095fa0</c> (0x100095fa0), reached from
    /// <c>FUN_100095b10</c> (0x100095b10) when the Preferences window loads: read two hex digits,
    /// XOR them with the key's high byte, and advance the key with the cipher byte. The same walk
    /// in the other direction, so one function serves both.</para>
    ///
    /// <para>This is obfuscation, not encryption: it keeps the password out of a shoulder-surfer's
    /// way, and the original makes no claim beyond that. It is copied exactly so a
    /// <c>laragon.ini</c> written by the original opens here and one written here opens there.</para>
    /// </summary>
    public static class MailSecret
    {
        private const int Seed = 0x1f4b;
        private const int Step = 0x302d;
        private const int Bias = 0x4cc0;

        /// <summary>The text as it goes into the ini file (empty in, empty out).</summary>
        public static string Encode(string plain)
        {
            if (string.IsNullOrEmpty(plain))
            {
                return string.Empty;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(plain);
            StringBuilder hex = new StringBuilder(bytes.Length * 2);
            int key = Seed;
            foreach (byte b in bytes)
            {
                int c = b ^ ((key >> 8) & 0xff);
                key = Next(key, c);
                hex.Append(c.ToString("X2", CultureInfo.InvariantCulture));
            }

            return hex.ToString();
        }

        /// <summary>
        /// The password back out of a stored value. False when the value is not the original's
        /// encoding at all — an odd number of characters, something that is not hex, or bytes
        /// that do not come out as readable text. The rebuild's own earlier versions wrote the
        /// password in plain text under <c>[mailsender] Password</c>, and such a value must be
        /// shown as it stands rather than mangled.
        /// </summary>
        public static bool TryDecode(string stored, out string plain)
        {
            plain = string.Empty;
            if (string.IsNullOrEmpty(stored))
            {
                return true;
            }

            string text = stored.Trim();
            if (text.Length % 2 != 0)
            {
                return false;
            }

            byte[] cipher = new byte[text.Length / 2];
            for (int i = 0; i < cipher.Length; i++)
            {
                if (!byte.TryParse(text.Substring(i * 2, 2), NumberStyles.HexNumber,
                                   CultureInfo.InvariantCulture, out cipher[i]))
                {
                    return false;
                }
            }

            int key = Seed;
            for (int i = 0; i < cipher.Length; i++)
            {
                int c = cipher[i];
                cipher[i] = (byte)(c ^ ((key >> 8) & 0xff));
                key = Next(key, c);
            }

            string decoded = Encoding.UTF8.GetString(cipher);
            foreach (char ch in decoded)
            {
                if (ch < 0x20 || ch > 0x7e)
                {
                    return false;
                }
            }

            plain = decoded;
            return true;
        }

        /// <summary>The stored value for a text, or the text itself when it cannot be decoded.</summary>
        public static string Decode(string stored)
        {
            return TryDecode(stored, out string plain) ? plain : stored;
        }

        private static int Next(int key, int cipherByte)
        {
            long next = ((long)cipherByte + key) * Step - Bias;
            return (int)(next & 0xffff);
        }
    }
}
