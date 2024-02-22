using System;
using System.Numerics;
using System.Text;

class test
{

    static Random random = new Random();
    static void Main()
    {
        // Получение большого простого p
        BigInteger p = GenerateRandomPrime();

        // Получение большого простого q, не равного p
        BigInteger q = GenerateRandomPrime(p);

        // Вычисление n 
        BigInteger n = p * q;

        // Вычисление phi
        BigInteger phi = (p - 1) * (q - 1);

        // Генерация нечетного e, взаимно простого с phi
        BigInteger e = GenerateExponent(phi);

        // d = e ^ (-1) mod n
        BigInteger d = ModInverse(e, phi);

        /*
            Блок тестов
        */

        string originalMessage = "Hello, World!";
        Console.WriteLine("Original Message: " + originalMessage);

        // Преобразуем сообщение в числовое представление
        BigInteger plaintext = StringToBigInteger(originalMessage);

        // Шифруем сообщение с использованием открытого ключа (n, e)
        BigInteger ciphertext = Encrypt(plaintext, n, e);
        Console.WriteLine("Encrypted Message: " + ciphertext);

        // Дешифруем сообщение с использованием секретного ключа (n, d)
        BigInteger decryptedText = Decrypt(ciphertext, n, d);

        // Преобразуем числовое представление обратно в текст
        string decryptedMessage = BigIntegerToString(decryptedText);
        Console.WriteLine("Decrypted Message: " + decryptedMessage);

        Console.WriteLine("Success comparison: " + String.Compare(originalMessage, decryptedMessage));

        // Шифруем цифровую подпись 
        BigInteger signature = Encrypt(plaintext, n, d);
        Console.WriteLine("\nSignature: " + signature);

        // Дешифруем цифровую подпись
        BigInteger encrypt = Decrypt(signature, n, e);
        Console.WriteLine("Decrypt signature: " + encrypt);

        Console.WriteLine("Success comparison: " + String.Compare(plaintext.ToString(), encrypt.ToString()));

        // Вносим ошибку в подпись
        Console.WriteLine("\nError test");
        encrypt = Decrypt(signature + 1, n, e);
        Console.WriteLine("Error signature: " + (signature + 1));
        Console.WriteLine("Decrypt signature: " + encrypt);

        Console.WriteLine("Success comparison: " + String.Compare(plaintext.ToString(), encrypt.ToString()));
    }

    /// <summary>
    /// Генерация большого простого числа 
    /// </summary>
    /// <returns>Большое простое число</returns>
    static BigInteger GenerateRandomPrime() => GenerateRandomPrime(2);

    /// <summary>
    /// Генерация большого простого числа 
    /// </summary>
    /// <param name="p">Число, которому не должно быть равно сгенерированное</param>
    /// <returns>Большое простое число, не равное p</returns>
    static BigInteger GenerateRandomPrime(BigInteger p)
    {
        // Длинна числа в битах
        int bitLength = 256; 
        // Сгенерировать случайное большое число
        BigInteger randomNum = BigIntegerGenRandom(3, (BigInteger)Math.Pow(2, bitLength));

        // Повторять, пока не найдется простое число
        // Для проверки используется тест Миллера-Рабина с 5 иттерациями 
        // Используется вероятностный тест определения на простоту, т.к. классические методы определения слишком медленнные
        while (!IsProbablePrime(randomNum, 5) || p == randomNum)  
            randomNum = BigIntegerGenRandom(3, (BigInteger)Math.Pow(2, bitLength));

        return randomNum;
    }

    /// <summary>
    /// Тест Миллера-Рабина
    /// </summary>
    /// <param name="n">Проверяемое число</param>
    /// <param name="k">Количество иттераций</param>
    /// <returns>True - число предположительно простое, иначе False</returns>
    static bool IsProbablePrime(BigInteger n, int k)
    {
        if (n == 2 || n == 3)
            return true;
        if (n < 2 || n % 2 == 0)
            return false;

        BigInteger d = n - 1;
        int s = 0;

        while (d % 2 == 0)
        {
            d /= 2;
            s++;
        }

        for (int i = 0; i < k; i++)
        {
            BigInteger a = BigIntegerGenRandom(2, n - 2);
            BigInteger x = BigInteger.ModPow(a, d, n);

            if (x == 1 || x == n - 1)
                continue;

            for (int j = 0; j < s - 1; j++)
            {
                x = BigInteger.ModPow(x, 2, n);
                if (x == n - 1)
                    break;
            }

            if (x != n - 1)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Генерация большого случайного числа
    /// </summary>
    /// <param name="minValue">Минимальное допустимое число</param>
    /// <param name="maxValue">Максимальное допустимое число</param>
    /// <returns>Случайное число из диапазона</returns>
    static BigInteger BigIntegerGenRandom(BigInteger minValue, BigInteger maxValue)
    {
        int byteLength = (maxValue - minValue).ToByteArray().Length;
        byte[] randomBytes = new byte[byteLength];
        random.NextBytes(randomBytes);

        BigInteger randomNum = new BigInteger(randomBytes);
        randomNum = randomNum % (maxValue - minValue) + minValue;

        return randomNum;
    }

    /// <summary>
    /// Генерация случайной экспоненты
    /// </summary>
    /// <param name="phi">Взаимно-простое заданное число</param>
    /// <returns></returns>
    static BigInteger GenerateExponent(BigInteger phi)
    {
        BigInteger x, y;

        int e = random.Next(3, 50);
        while (e % 2 == 0)
            e = random.Next(3, 50);

        // Находим НОД через расширенный алгоритм Евклида
        BigInteger gcd;
        ExtendedEuclideanAlgorithm((BigInteger)e, phi, out gcd, out x, out y);
        
        // Если НОД отличен от 1, то числа не взаимно-простые. Переходим к следующему нечётному e
        while (gcd != 1)
        {
            e += 2;
            ExtendedEuclideanAlgorithm((BigInteger)e, phi, out gcd, out x, out y);
        };

        return (BigInteger)e;
    }

    /// <summary>
    /// Расширенный алгоритм Евклида
    /// </summary>
    /// <param name="a">Первое число</param>
    /// <param name="b">Второе число</param>
    /// <param name="gcd">НОД</param>
    /// <param name="x">Коэффициент x Безу</param>
    /// <param name="y">Коэффициент y Безу</param>
    static void ExtendedEuclideanAlgorithm(BigInteger a, BigInteger b, out BigInteger gcd, out BigInteger x, out BigInteger y)
    {
        if (a == 0)
        {
            gcd = b;
            x = 0;
            y = 1;
        }
        else
        {
            ExtendedEuclideanAlgorithm(b % a, a, out gcd, out x, out y);
            BigInteger temp = y - (b / a) * x;
            y = x;
            x = temp;
        }
    }

    /// <summary>
    /// Нахождение мультипликативного обратного элемента a ^ (-1) mod m
    /// </summary>
    /// <param name="a">Коэффициент a</param>
    /// <param name="m">Коэффициент m</param>
    /// <returns>мультипликативный обратный элемент a ^ (-1) mod m</returns>
    static BigInteger ModInverse(BigInteger a, BigInteger m)
    {
        BigInteger m0 = m;
        BigInteger x0 = 0;
        BigInteger x1 = 1;

        if (m == 1)
            return 0;

        while (a > 1)
        {
            // q - частное от деления a на m
            BigInteger q = a / m;
            BigInteger t = m;

            // m - остаток от деления a на m
            m = a % m;
            a = t;
            t = x0;

            // Обновляем x0 и x1
            x0 = x1 - q * x0;
            x1 = t;
        }

        // Убеждаемся, что x1 остается положительным
        if (x1 < 0)
            x1 += m0;

        return x1;
    }

    /// <summary>
    /// Преобразование строки в последовательность байт BigInteger
    /// </summary>
    /// <param name="str">Переводимая строка</param>
    /// <returns>Последовательность байт, представленная BigInteger типом данных</returns>
    static BigInteger StringToBigInteger(string str)
    {
        // Преобразование строки в числовое представление (ASCII коды символов)
        byte[] bytes = Encoding.UTF8.GetBytes(str);
        return new BigInteger(bytes);
    }

    /// <summary>
    /// Преобразование последовательности байт BigInteger в строку
    /// </summary>
    /// <param name="num">последовательность байт BigInteger</param>
    /// <returns>Строка</returns>
    static string BigIntegerToString(BigInteger num)
    {
        // Преобразование числового представления в строку (ASCII коды символов)
        byte[] bytes = num.ToByteArray();
        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// Шифрование алгоритмом RSA
    /// </summary>
    /// <param name="plaintext">Шифруемое число</param>
    /// <param name="n">Второе число открытого ключа</param>
    /// <param name="e">Первое число открытого ключа</param>
    /// <returns>Зашифрованное число</returns>
    static BigInteger Encrypt(BigInteger plaintext, BigInteger n, BigInteger e)
    {
        // Шифрование: ciphertext = plaintext^e mod n
        return Power(plaintext, e, n);
    }

    /// <summary>
    /// Дешифрование алгоритмом RSA
    /// </summary>
    /// <param name="ciphertext"></param>
    /// <param name="n">Второе число закрытого ключа</param>
    /// <param name="d">Первое число закрытого ключа</param>
    /// <returns>Дешифрованное число</returns>
    static BigInteger Decrypt(BigInteger ciphertext, BigInteger n, BigInteger d)
    {
        // Дешифрование: plaintext = ciphertext^d mod n
        return Power(ciphertext, d, n);
    }

    /// <summary>
    /// Возведение числа в степень по модулю
    /// </summary>
    /// <param name="baseNumber">Число</param>
    /// <param name="exponent">Степень</param>
    /// <param name="modulo">Модуль</param>
    /// <returns>Число в степени</returns>
    static BigInteger Power(BigInteger baseNumber, BigInteger exponent, BigInteger modulo)
    {
        // Для ускорения используется алгоритм быстрого возведения в степень (Exponentiation by Squaring)
        BigInteger result = 1;
        baseNumber %= modulo;

        while (exponent > 0)
        {
            if (exponent % 2 == 1)
            {
                result = (result * baseNumber) % modulo;
            }

            exponent >>= 1;
            baseNumber = (baseNumber * baseNumber) % modulo;
        }

        return result;
    }
}
