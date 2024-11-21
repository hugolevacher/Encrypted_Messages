using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using System.Security.Cryptography;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace SecretMessages
{
    public partial class SecretForm : Form
    {
        public SecretForm()
        {
            InitializeComponent();
            button.Text = "Encrypt";
        }

        private void button_Click(object sender, EventArgs e)
        {
            string message = txtMessage.Text;
            string key = txtKey.Text;
            string result;

            if(message!="" && key != "")
            {
                if (rbEncrypt.Checked)
                {
                    result = Encrypt(message, key);
                    txtResult.Text = result;
                }
                else
                {
                    try
                    {
                        result = Decrypt(message, key);
                        txtResult.Text = result;
                    }
                    catch
                    {
                        MessageBox.Show("Message is undecryptable.\nIncorrect key or message.", "Undecryptable", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Message and key are required.", "Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        




        private static int KeyLength = 32; // 256-bit key
        private static int SaltLength = 16; // 128-bit salt

        // Encrypts the message with the given key
        private string Encrypt(string message, string key)
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

        private string Decrypt(string message, string key)
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

        private void rbEncrypt_CheckedChanged(object sender, EventArgs e)
        {
            if (rbEncrypt.Checked)
            {
                button.Text = "Encrypt";
                txtMessage.Text = "";
                txtKey.Text = "";
                txtResult.Text = "";
            }
            else
            {
                button.Text = "Decrypt";
                txtMessage.Text = "";
                txtKey.Text = "";
                txtResult.Text = "";
            }
        }
    }
}
