namespace Freedom.Extensions
{
    public static class BinaryExtensions
    {
        public static string ToHexString(this byte[] bytes)
        {
            if (bytes == null)
                return null;

            char[] result = new char[bytes.Length * 2];

            for (int i = 0; i < bytes.Length; i++)
            {
                // This is some voodoo bit-manipulation magic here.
                // but basically the function:  55 + b + (((b - 10) >> 31) & -7) 
                // will give the ASCII value of '0' through '9', if b is between 0 and 9;
                // and the ASCII value of 'A' through 'F', if b is between 10 and 15

                int b = bytes[i] >> 4;  // b = the high nibble from 0-15
                result[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));

                b = bytes[i] & 0xF;     // b = the low nibble from 0-15
                result[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
            }

            return new string(result);
        }
    }
}
