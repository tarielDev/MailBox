using System.Security.Cryptography;

namespace KeyGen
{
    internal class Program
    {
        public static void GenerateKeysAndSave(string publicKeyPath, string privateKeyPath)
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                var publicKey = rsa.ToXmlString(false);

                var privateKey = rsa.ToXmlString(true);

                File.WriteAllText(publicKeyPath, publicKey);
                File.WriteAllText(privateKeyPath, privateKey);
            }
        }

        public static void Main()
        {
            string publicKeyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + @"Rsa\public_key.pem");
            string privateKeyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + @"Rsa\private_key.pem");

            GenerateKeysAndSave(publicKeyPath, privateKeyPath);

            Console.WriteLine("RSA ключи сгенерированы.");
        }
    }
}
