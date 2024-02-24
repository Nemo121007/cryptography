using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Reflection.Metadata;
using System.Xml;

class HybridCryptosystem
{
    static void Main()
    {
        string str = "";

        while (str == "")
        {
            Console.WriteLine("\nВыберете операцию: \n" +
                              "1. Шифрование \n" +
                              "2. Дешифрование\n" +
                              "3. Выход\n");
            str = Console.ReadLine();
            int a;
            try
            {
                a = Convert.ToInt32(str);

            }
            catch
            {
                Console.WriteLine("Ошибка ввода. Введите номер операции\n");
                str = "";
                continue;
            }
            if (a == 1)
            {
                Encpyption();
                str = "";
            }
            else
            if (a == 2)
            {
                Decpyption();
                str = "";
            }
            else
            if (a == 3)
            {
                break;
            }
            else
            {
                Console.WriteLine("Ошибка ввода. Введите номер операции\n");
                str = "";
            }

        }

    }

    static void Encpyption()
    {
        string path = "";
        string fileContent = "";

        // Получение файла для шифрования
        while (path == "")
        {
            Console.WriteLine("Введите название файла и путь до него для шифрования:");
            path = Console.ReadLine();

            /////////////////////////////////////// Тест!
            path = "example.txt";
            /////////////////////////////////////// Тест!

            // Проверяем, существует ли файл
            if (File.Exists(path))
            {
                // Считываем содержимое файла
                fileContent = File.ReadAllText(path);
                Console.WriteLine("Считывание прошло успешно");
            }
            else
                Console.WriteLine("Файл не существует.");
        }

        // Генерация ключей для симметричного шифрования
        byte[] AESKey = GetRandomAesKey();
        byte[] AESIV = GetRandomAesIV();

        Console.WriteLine("Симметричный ключ сгенерирован");

        // Шифрование документа симметричным ключом
        byte[] encryptedDocument = EncryptDocument(fileContent, AESKey, AESIV);
        File.WriteAllBytes("encryptedDocument.txt", encryptedDocument);

        Console.WriteLine("Файл зашифрован. Зашифрованный файл: encryptedDocument.txt");

        byte[] encryptedAESKey;
        byte[] encryptedIVKey;
        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
        {
            // Сохранение открытого сессионного ключа в файл
            string publicKeyRSA = rsa.ToXmlString(false);
            File.WriteAllText("publicKeyRSA.xml", publicKeyRSA);
            Console.WriteLine("Открытый ключ RSA записан в publicKeyRSA.xml");

            // Сохранение закрытого сессионного ключа в файл
            string privateKeyRSA = rsa.ToXmlString(true);
            File.WriteAllText("privateKeyRSA.xml", privateKeyRSA);
            Console.WriteLine("Закрытый ключ RSA записан в privateKeyRSA.xml");

            // Шифрование симметричного ключа сессионным ключом
            encryptedAESKey = rsa.Encrypt(AESKey, false);
            encryptedIVKey = rsa.Encrypt(AESIV, false);
            // Запись зашифрованного симметричного ключа в файл
            WriteXML("AESKey", encryptedAESKey, "IVKey", encryptedIVKey, "encryptedAESKey.xml");
            Console.WriteLine("Зашифрованный симметричный ключ записан в encryptedAESKey.xml");

            // Генерация цифровой подписи к зашифрованному файлу
            byte[] digitalSignature = rsa.SignData(encryptedDocument, new SHA256CryptoServiceProvider());
            // Запись цифровой подписи в файл
            File.WriteAllBytes("digitalSignatureFromEncryptedDocument.txt", digitalSignature);
            Console.WriteLine("Добавлена цифровая подпись к зашифрованному документу в digitalSignatureFromEncryptedDocument.txt");
        }
    }

    static void Decpyption()
    {
        string path = "";
        byte[] digitalSignature = null;
        // Получение цифровой подписи 
        while (path == "")
        {
            Console.WriteLine("Введите название подписи и путь до неё:");
            path = Console.ReadLine();

            /////////////////////////////////////// Тест!
            path = "digitalSignatureFromEncryptedDocument.txt";
            /////////////////////////////////////// Тест!

            // Проверяем, существует ли файл
            if (File.Exists(path))
            {
                // Считываем содержимое файла
                digitalSignature = File.ReadAllBytes(path);
                Console.WriteLine("Считывание прошло успешно");
            }
            else
                Console.WriteLine("Файл не существует.");
        }

        path = "";
        byte[] fileContent = null;
        // Получение зашифрованного файла
        while (path == "")
        {
            Console.WriteLine("\nВведите название файла и путь до него для дешифрования:");
            path = Console.ReadLine();

            /////////////////////////////////////// Тест!
            path = "encryptedDocument.txt";
            /////////////////////////////////////// Тест!

            // Проверяем, существует ли файл
            if (File.Exists(path))
            {
                // Считываем содержимое файла
                fileContent = File.ReadAllBytes(path);
                Console.WriteLine("Считывание прошло успешно");
            }
            else
                Console.WriteLine("Файл не существует.");
        }

        path = "";
        string publicRSAKeyPath = null;
        // Получение адреса открытого сессионного ключа 
        while (path == "")
        {
            Console.WriteLine("\nВведите название открытого сессионного ключа и путь до него:");
            path = Console.ReadLine();

            /////////////////////////////////////// Тест!
            path = "publicKeyRSA.xml";
            /////////////////////////////////////// Тест!

            // Проверяем, существует ли файл
            if (File.Exists(path))
            {
                Console.WriteLine("Считывание прошло успешно");
                publicRSAKeyPath = path;
            }
            else
                Console.WriteLine("Файл не существует.");
        }

        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
        {
            // Загрузка открытого ключа
            string publicKeyXmlFromFile = File.ReadAllText(publicRSAKeyPath);
            rsa.FromXmlString(publicKeyXmlFromFile);

            // Проверка цифровой подписи
            bool verify = rsa.VerifyData(fileContent, new SHA256CryptoServiceProvider(), digitalSignature);

            if (verify)
                Console.WriteLine("Подпись подтверждена");
            else
                Console.WriteLine("Подпись недействительна");
        }

        path = "";
        byte[] encryptedAESKey = null;
        byte[] encryptedIVKey = null;
        // Получение зашифрованного симметричного ключа
        while (path == "")
        {
            Console.WriteLine("\nВведите название зашифрованного симметричного ключа и путь до него:\n");
            path = Console.ReadLine();

            /////////////////////////////////////// Тест!
            path = "encryptedAESKey.xml";
            /////////////////////////////////////// Тест!

            // Проверяем, существует ли файл
            if (File.Exists(path))
            {
                // Считываем содержимое файла
                encryptedAESKey = File.ReadAllBytes(path);
                Console.WriteLine("Считывание прошло успешно");

                var a = ReadXML(path, "AESKey", "IVKey");

                // Выбираем кодировку (например, UTF-8)
                Encoding encoding = Encoding.UTF8;

                encryptedAESKey = a.Item1;
                encryptedIVKey = a.Item2;

                Console.WriteLine("Ключ считан");
            }
            else
                Console.WriteLine("Файл не существует.");
        }

        path = "";
        string privateRSAKeyPath = "";
        // Получение закрытого сессионного ключа
        while (path == "")
        {
            Console.WriteLine("\nВведите название закрытого сессионного ключа и путь до него:");
            path = Console.ReadLine();

            /////////////////////////////////////// Тест!
            path = "privateKeyRSA.xml";
            /////////////////////////////////////// Тест!

            // Проверяем, существует ли файл
            if (File.Exists(path))
            {
                Console.WriteLine("Считывание прошло успешно");
                privateRSAKeyPath = path;
            }
            else
                Console.WriteLine("Файл не существует.");
        }


        byte[] AESKey;
        byte[] IVKey;
        // Расшифровка симметричного ключа
        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
        {
            // Загрузка приватного ключа из файла
            string privateKeyXmlFromFile = File.ReadAllText(privateRSAKeyPath);
            rsa.FromXmlString(privateKeyXmlFromFile);

            AESKey = rsa.Decrypt(encryptedAESKey, false);
            IVKey = rsa.Decrypt(encryptedIVKey, false);

            Console.WriteLine("Симметричный ключ расшифрован");
        }

        // Расшифровка текста симметричным ключом
        string decryptText = DecryptDocument(fileContent, AESKey, IVKey);
        Console.WriteLine("Тест дешифрован");

        File.WriteAllText("DecryptedDocument.txt", decryptText);
        Console.WriteLine("Тест записан в DecryptedDocument.txt");
    }

    /// <summary>
    /// Генерация случайного симметричного ключа (компонента Key)
    /// </summary>
    /// <returns>Случайный симметричный ключ (компонента Key)</returns>
    static byte[] GetRandomAesKey()
    {
        using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
        {
            aesAlg.GenerateKey();
            return aesAlg.Key;
        }
    }

    /// <summary>
    /// Генерация случайного симметричного ключа (компонента IV)
    /// </summary>
    /// <returns>Случайный симметричный ключ (компонента IV)</returns>
    static byte[] GetRandomAesIV()
    {
        using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
        {
            aesAlg.GenerateIV();
            return aesAlg.IV;
        }
    }

    /// <summary>
    /// Шифрование документа симметричным шифром AES
    /// </summary>
    /// <param name="document">Документ</param>
    /// <param name="key">Ключ (Компонента Key)</param>
    /// <param name="IV">Ключ (Компонента IV)</param>
    /// <returns>Зашифрованный документ</returns>
    static byte[] EncryptDocument(string document, byte[] key, byte[] IV)
    {
        using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
        {
            aesAlg.Key = key;
            aesAlg.IV = IV;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor();
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(document);
                    }
                }
                return msEncrypt.ToArray();
            }
        }
    }

    /// <summary>
    /// Дешифрование документа симметричным шифром AES
    /// </summary>
    /// <param name="document">Документ</param>
    /// <param name="key">Ключ (Компонента Key)</param>
    /// <param name="IV">Ключ (Компонента IV)</param>
    /// <returns>Дешифрованный документ</returns>
    static string DecryptDocument(byte[] cipherText, byte[] key, byte[] IV)
    {
        using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
        {
            aesAlg.Key = key;
            aesAlg.IV = IV; // Use the same IV that was used for encryption

            ICryptoTransform decryptor = aesAlg.CreateDecryptor();
            MemoryStream msDecrypt = new MemoryStream(cipherText);
            CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            StreamReader srDecrypt = new StreamReader(csDecrypt);

            return srDecrypt.ReadToEnd();
        }
    }

    /// <summary>
    /// Создание XML докумета
    /// </summary>
    /// <param name="nameA">Первый тег</param>
    /// <param name="a">Первая запись</param>
    /// <param name="nameB">Второй тег</param>
    /// <param name="b">Вторая запись</param>
    /// <param name="nameFile">Имя файла, куда будет записан документ</param>
    static void WriteXML(string nameA, byte[] a, string nameB, byte[] b, string nameFile)
    {
        try
        {
            // Создаем новый XML-документ
            XmlDocument xmlDoc = new XmlDocument();

            // Создаем корневой элемент
            XmlElement rootElement = xmlDoc.CreateElement("Root");

            // Создаем элементы данных и добавляем их в корневой элемент
            XmlElement element1 = xmlDoc.CreateElement(nameA);
            element1.InnerText = Convert.ToBase64String(a);
            rootElement.AppendChild(element1);

            XmlElement element2 = xmlDoc.CreateElement(nameB);
            element2.InnerText = Convert.ToBase64String(b);
            rootElement.AppendChild(element2);

            // Добавляем корневой элемент в документ
            xmlDoc.AppendChild(rootElement);

            // Сохраняем XML-документ в файл
            xmlDoc.Save(nameFile);

            Console.WriteLine("Данные успешно записаны в XML-файл.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Произошла ошибка: {ex.Message}");
        }
    }

    /// <summary>
    /// Чтение XML документа
    /// </summary>
    /// <param name="path">Путь до докумета</param>
    /// <param name="elementA">Первый тег</param>
    /// <param name="elementB">Второй тег</param>
    /// <returns>Кортеж (значение по первому тегу, значение по второму тегу)</returns>
    static (byte[], byte[]) ReadXML(string path, string elementA, string elementB)
    {
        try
        {
            // Создаем объект XmlDocument
            XmlDocument xmlDoc = new XmlDocument();

            // Загружаем XML-документ из файла
            xmlDoc.Load(path);

            // Получаем корневой элемент
            XmlElement rootElement = xmlDoc.DocumentElement;

            // Читаем данные из элементов
            byte[] element1Value = Convert.FromBase64String(rootElement.SelectSingleNode(elementA)?.InnerText);
            byte[] element2Value = Convert.FromBase64String(rootElement.SelectSingleNode(elementB)?.InnerText);

            return (element1Value, element2Value);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Произошла ошибка: {ex.Message}");
        }
        return (null, null);
    }
}

