using System;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

class DiffieHellman
{
    static Random random = new Random();
    // Разрядность (в битах) переменных, использвемых для кодирования
    static int numberDigits = 16;
    static void Main()
    {
        // Генерация большого простого числа p
        BigInteger p = GenerateRandomPrime();

        // Генерация примитивного элемента g, являющегося первообразным корнем
        BigInteger g = GeneratePrimitiveRoot(p);

        BigInteger publicKeyX, x;
        do
        {
            // Генерация закрытого ключа x для пользователя
            x = BigIntegerGenRandom(g, p);

            // Вычисление открытого ключей для остальных пользователей
            publicKeyX = Power(g, x, p);
        }
        while (publicKeyX == 1);

        BigInteger publicKeyY, y;
        do
        {
            // Генерация закрытого ключа y для пользователя
            y = BigIntegerGenRandom(g, p);

            // Вычисление открытого ключей для остальных пользователей
            publicKeyY = Power(g, y, p);
        }
        while (publicKeyY == 1);

        // Обмен открытыми ключами между пользователями (обычно по сети)
        // В реальном приложении это будет происходить между двумя сторонами

        // Вычисление общего ключа для каждого пользователя
        BigInteger sharedKeyX = Power(publicKeyY, x, p);
        BigInteger sharedKeyY = Power(publicKeyX, y, p);
        // Вывод общих ключей
        Console.WriteLine($"Общий ключ для пользователя X: {sharedKeyX}");
        Console.WriteLine($"Общий ключ для пользователя Y: {sharedKeyY}");
    }

    /// <summary>
    /// Генерация большого простого числа 
    /// </summary>
    /// <returns>Большое простое число, не равное p</returns>
    static BigInteger GenerateRandomPrime()
    {
        // Длинна числа в битах
        int bitLength = numberDigits;
        // Сгенерировать случайное большое число
        BigInteger randomNum = BigIntegerGenRandom(2, (BigInteger)Math.Pow(2, bitLength));

        // Повторять, пока не найдется простое число
        // Для проверки используется тест Миллера-Рабина с 5 иттерациями 
        // Используется вероятностный тест определения на простоту, т.к. классические методы определения слишком медленнные
        while (!IsProbablePrime(randomNum, 5))
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

    /// <summary>
    /// Генерация примитивного элемента, являющегося первообразным корнем указанного числа
    /// </summary>
    /// <param name="prime">Число, для которого подбирается примитивный корень</param>
    /// <returns></returns>
    static BigInteger GeneratePrimitiveRoot(BigInteger prime)
    {
        BigInteger candidate = BigIntegerGenRandom(2, (BigInteger)Math.Pow(2, numberDigits));
        while (true)
        {
            if (IsPrimitiveRoot(candidate, prime))
            {
                return candidate;
            }
            else
                candidate = BigIntegerGenRandom(2, (BigInteger)Math.Pow(2, numberDigits));
        }
    }

    /// <summary>
    /// Проверка на явление примитивного корня
    /// </summary>
    /// <param name="g">Потенциальный примитивный корень</param>
    /// <param name="p">Число, для которого подбирается примитивный корень</param>
    /// <returns>True - если g - примитивный корень p, иначе False</returns>
    static bool IsPrimitiveRoot(BigInteger g, BigInteger p)
    {
        BigInteger eulerPhi = p - 1;
        BigInteger m = eulerPhi;

        // Проверка, что g и eulerPhi взаимно просты
        if (BigInteger.GreatestCommonDivisor(g, eulerPhi) != 1)
        {
            return false;
        }

        // Вычисление значения x методом Шенкса
        HashSet<BigInteger> powers = new HashSet<BigInteger>();

        for (BigInteger i = 1, val = 1; i <= m; i++)
        {
            val = (val * g) % p;
            powers.Add(val);
        }

        return powers.Count == (int)m; // g является первообразным корнем, если все степени от 1 до eulerPhi - 1 различны
    }
}
