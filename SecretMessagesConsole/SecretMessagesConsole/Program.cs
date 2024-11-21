using System;
using System.Security.Cryptography;

namespace SecretMessagesConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                string choice="";
                while (true)
                {
                    Console.WriteLine("Do you want to (E)ncrypt, (D)ecrypt a message or (C)lose the console? Enter 'E', 'D', or 'C'.");
                    choice = Console.ReadLine().Trim().ToUpper();
                    Console.WriteLine();
                    if (choice == "E" || choice == "D" || choice == "C") { break; }
                    else
                    {
                        Console.WriteLine("Invalid choice.");
                        continue;
                    }
                }
                if (choice == "C")
                {
                    break;
                }

                string message="";
                while (true)
                {
                    Console.WriteLine("Enter the message:");
                    message = Console.ReadLine();
                    Console.WriteLine();
                    if (message != "") { break; }
                    else
                    {
                        Console.WriteLine("Message is required.");
                        continue;
                    }
                }

                string key = "";
                while (true)
                {
                    Console.WriteLine("Enter the key:");
                    key = Console.ReadLine();
                    Console.WriteLine();
                    if (key != "") { break; }
                    else
                    {
                        Console.WriteLine("Key is required.");
                        continue;
                    }
                }

                string result = "";
                if (choice == "E")
                {
                    result = Encrypt(message, key);
                    Console.WriteLine("Result:\n" + result);
                    Console.WriteLine();
                }
                else if (choice == "D")
                {
                    try
                    {
                        result = Decrypt(message, key);
                        Console.WriteLine("Result:\n" + result);
                        Console.WriteLine();
                    }
                    catch
                    {
                        Console.WriteLine("Message is undecryptable.");
                        Console.WriteLine();
                    }
                }
            }
        }

        private static int KeyLength = 32; // 256-bit key
        private static int SaltLength = 16; // 128-bit salt

        // Encrypts the message with the given key
        static string Encrypt(string message, string key)
        {
            // Generate a random salt
            byte[] salt = GenerateSalt(SaltLength);
            using (Aes aes = Aes.Create())
            {
                // Derive the encryption key from the key and salt
                aes.Key = DeriveKey(key, salt, KeyLength);
                aes.GenerateIV(); // Generate a random IV

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    // Write the salt and IV to the output stream
                    ms.Write(salt, 0, salt.Length);
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(message); // Write the plaintext to the crypto stream
                    }

                    // Return the encrypted data as a base64 encoded string
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        static string Decrypt(string message, string key)
        {
            // Decode the base64 encoded message to a byte array
            byte[] buffer = Convert.FromBase64String(message);

            using (Aes aes = Aes.Create())
            {
                // Extract the salt and IV from the buffer
                byte[] salt = new byte[SaltLength];
                Array.Copy(buffer, 0, salt, 0, salt.Length);

                byte[] iv = new byte[aes.BlockSize / 8];
                Array.Copy(buffer, salt.Length, iv, 0, iv.Length);

                // Derive the decryption key from the key and salt
                aes.Key = DeriveKey(key, salt, KeyLength);

                using (var decryptor = aes.CreateDecryptor(aes.Key, iv))
                using (var ms = new MemoryStream(buffer, salt.Length + iv.Length, buffer.Length - salt.Length - iv.Length))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    // Read and return the decrypted plaintext
                    return sr.ReadToEnd();
                }
            }
        }

        // Generates a random salt of the specified length
        private static byte[] GenerateSalt(int length)
        {
            byte[] salt = new byte[length];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        // Derives a cryptographic key from the given key and salt
        private static byte[] DeriveKey(string key, byte[] salt, int keyLength)
        {
            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(key, salt, 10000))
            {
                return rfc2898DeriveBytes.GetBytes(keyLength);
            }
        }
    }
}