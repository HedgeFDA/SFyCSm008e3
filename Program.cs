using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace SFyCSm008e3;

internal class Program
{
    /// <summary>
    /// Функция выполняющее преобразования строки в число.
    /// </summary>
    /// <param name="stringValue"></param>
    /// <param name="integerValue"></param>
    /// <returns></returns>
    static bool IsCorrectIntegerValue(string stringValue, out int integerValue) => int.TryParse(stringValue, out integerValue);

    /// <summary>
    /// Функция реализующая запрос ввода у пользоватеся строкового значения
    /// </summary>
    /// <param name="message">Текст запроса ввода строки к пользователю</param>
    /// <returns></returns>
    static string InputStringValue(string message)
    {
        Console.Write(message);

        string? value = Console.ReadLine();

        return value == null ? "" : value;
    }

    /// <summary>
    /// Функция реализующая запрос ввода у пользоватеся числового значения, выполнеяет контроль что введенное значение является числом,
    /// а также при необходимости контроль за тем, чтоб введенное значение было больше 0
    /// </summary>
    /// <param name="message">Текст запроса ввода числа к пользователю</param>
    /// <param name="needCheckZero">Признак необходимости контролировать чтоб введенное знеачение было больше 0</param>
    /// <returns></returns>
    static int InputIntegerValue(string message, bool needCheckZero = false)
    {
        int value = 0;

        while (true)
        {
            bool isCorrectIntegerValue = IsCorrectIntegerValue(InputStringValue(message), out value);

            if (!isCorrectIntegerValue)
                Console.WriteLine("   необходимо ввести числовое значение");
            else if (needCheckZero & value <= 0)
                Console.WriteLine("   введенное значение должно быть больше 0");
            else break;
        }

        return value;
    }

    /// <summary>
    /// Процедура выводящяя информацию об ошибке в консоли.
    /// </summary>
    /// <param name="message">Текст сообщения который будет добавлен к сообщению после заголовка "Ошибка: "</param>
    static void ErrorMessage(string message) => Console.WriteLine("\nОшибка: {0}", message);

    /// <summary>
    /// Метод удаляющий все вложенные файлы старше даты deadline
    /// Возвращает 
    /// </summary>
    /// <param name="folderPath">Путь к каталогу в котором следует удалить файлы и рекурсивно обработать вложенные каталоги</param>
    /// <param name="deadline">Граница (дата), файлы с датой и времеменм последнего доступа старше которой будут удалены</param>
    /// <returns>Общий размер (в байтах) удаленных файлов.</returns>
    static long ClearFiles(string folderPath, DateTime deadline)
    {
        long result = 0;

        foreach (string file in Directory.GetFiles(folderPath))
        {
            FileInfo fileInfo = new FileInfo(file);

            // Файл моложе границы удаления, пропускаем
            if (fileInfo.LastAccessTime >= deadline) continue;

            try
            {
                long fileSize = fileInfo.Length;

                fileInfo.Delete();

                result += fileSize;
            }
            catch (Exception ex)
            {
                ErrorMessage($"Не удалось удалить файл \"{file}\" по причине: {ex.Message}");

                continue;
            }
        }

        return result;
    }

    /// <summary>
    /// Рекурсивный метод удаляющий все вложенные файлы и каталоги старше даты deadline
    /// </summary>
    /// <param name="folderPath">Путь к каталогу в котором следует удалить файлы и рекурсивно обработать вложенные каталоги</param>
    /// <param name="deadline">Граница (дата), файлы с датой и времеменм последнего доступа старше которой будут удалены</param>
    /// <returns>Общий размер (в байтах) удаленных файлов (включая удаленные файлы во вложенных каталогах).</returns>
    static long ClearFolder(string folderPath, DateTime deadline)
    {
        long result = 0;

        // Удаляем вложенные файлы в текущем каталоге
        result += ClearFiles(folderPath, deadline);

        foreach (string subFolderPath in Directory.GetDirectories(folderPath))
        {
            // Продолжение рекурсии очистки
            result += ClearFolder(subFolderPath, deadline);

            DirectoryInfo dirInfo = new DirectoryInfo(subFolderPath);

            // Остались вложенные файлы или каталоги, не можем удалять, пропускаем
            if (dirInfo.GetFiles().Length > 0 || dirInfo.GetDirectories().Length > 0) continue;

            try
            {
                // Удаляем вложенный каталог 
                dirInfo.Delete();
            }
            catch (Exception ex)
            {
                ErrorMessage($"Не удалось удалить каталог \"{subFolderPath}\" по причине: {ex.Message}");

                continue;
            }
        }

        return result;
    }

    /// <summary>
    /// Рекурсивный метод расчета размера каталога по вложенным файлам и каталогам
    /// </summary>
    /// <param name="folderPath">Путь к каталогу размер которого необходмо рассчитать</param>
    /// <returns>Общий размер (в байтах) файлов находящихся в каталоге (включая находящиеся файлы во вложенных каталогах).</returns>
    static long CalculateFolderSize(string folderPath)
    {
        long result = 0;

        foreach (string file in Directory.GetFiles(folderPath))
        {
            FileInfo fileInfo = new FileInfo(file);

            result += fileInfo.Length;
        }

        foreach (string subFolderPath in Directory.GetDirectories(folderPath))
        {
            // Продолжение рекурсии расчета
            result += CalculateFolderSize(subFolderPath);
        }

        return result;
    }
    
    /// <summary>
    /// Процедура реализующая основной алгоритм работы программы по удалению файлов и каталогов
    /// Включающая получение необходимых вводных значений от пользователя
    /// </summary>
    /// <param name="folderPath">Путь к каталогу в котором следует удалить файлы и рекурсивно обработать вложенные каталоги</param>
    static void PerformClearing(string folderPath = "")
    {
        while (folderPath == "" || !Directory.Exists(folderPath))
        {
            if (folderPath != "") Console.WriteLine("Каталог \"{0}\" не найден", folderPath);

            folderPath = InputStringValue("Укажите путь к каталогу размер которого необходимо рассчитать (для отмены введите пустое значение): ");

            if (string.IsNullOrEmpty(folderPath))
                return;
        }

        // Определим границу удаления (признания файлов устаревшими)
        DateTime deadline = DateTime.Now.AddMinutes(-InputIntegerValue("Укажите срок устаревания вложенных файлов и каталогов для очистки (в минутах): ", true));

        // Подтвердим удаление всех вложенных файлов и каталогов
        while (true)
        {
            string answer = InputStringValue($"\nВложенные файлы и каталоги с последней датой изменения старше \"{deadline}\" в каталоге \"{folderPath}\" будут удалены.\nПродолжить?\n    пустое значение - отменить\n    yes - продолжить\nВаше решение: ");

            if (answer == "yes")
                break;

            if (string.IsNullOrEmpty(answer))
                return;

            Console.WriteLine("    ответ должен быть yes или пустое значение.");
        }

        Console.WriteLine("\nВыполняем очистку каталога \"{0}\"...", folderPath);

        long size = 0;

        try
        {
            // Запуск рекурсии расчета размера
            size = CalculateFolderSize(folderPath);
        }
        catch (Exception ex)
        {
            ErrorMessage(ex.Message);

            return;
        }

        Console.WriteLine("Исходный размер: {0} байт", size);

        try
        {
            // Запуск рекурсии очистки
            size = ClearFolder(folderPath, deadline);
        }
        catch (Exception ex)
        {
            ErrorMessage(ex.Message);

            return;
        }

        Console.WriteLine("Освобождено: {0} байт", size);

        try
        {
            // Запуск рекурсии расчета размера
            size = CalculateFolderSize(folderPath);
        }
        catch (Exception ex)
        {
            ErrorMessage(ex.Message);

            return;
        }

        Console.WriteLine("Текущий размер: {0} байт", size);

    }

    /// <summary>
    /// Главная точка входа приложения
    /// </summary>
    /// <param name="args">Аргументы командной строки при запуске приложения.</param>    
    static void Main(string[] args)
    {
        string folderPath = "";

        if (args.Length > 0)
            folderPath = args[0];

        PerformClearing(folderPath);

        Console.WriteLine("\nВыполнение программы завершено.");
    }
}