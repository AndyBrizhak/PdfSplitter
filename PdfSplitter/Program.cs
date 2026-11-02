using System;
using System.IO;
using PdfSharp;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace PdfSplitterInteractive
{
    class Program
    {
        static void Main()
        {
            // Явное подтверждение использования бесплатной лицензии AGPL
            PdfSharpLicense.SetAgplLicense();

            Console.WriteLine("=== Утилита для извлечения страниц из PDF ===");

            string directoryPath = string.Empty;
            string sourceFilePath = string.Empty;
            PdfDocument inputDocument = null;

            // 1. Ввод пути к директории (папке)
            while (true)
            {
                Console.Write("\nВведите путь к папке с файлом: ");
                directoryPath = Console.ReadLine()?.Trim(' ', '"');

                if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
                {
                    if (!AskYesNo("Ошибка: Указанная папка не найдена. Повторить попытку? (y/n): ")) return;
                    continue;
                }
                break;
            }

            // 2. Ввод названия исходного файла
            while (true)
            {
                Console.Write("\nВведите название исходного PDF-файла: ");
                string fileName = Console.ReadLine()?.Trim(' ', '"');

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    if (!AskYesNo("Ошибка: Имя файла не может быть пустым. Повторить попытку? (y/n): ")) return;
                    continue;
                }

                if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    fileName += ".pdf";
                }

                sourceFilePath = Path.Combine(directoryPath, fileName);

                if (!File.Exists(sourceFilePath))
                {
                    if (!AskYesNo($"Ошибка: Файл '{fileName}' не найден. Повторить попытку? (y/n): ")) return;
                    continue;
                }

                try
                {
                    inputDocument = PdfReader.Open(sourceFilePath, PdfDocumentOpenMode.Import);
                    Console.WriteLine($"\nФайл успешно открыт. Всего страниц в документе: {inputDocument.PageCount}");
                    break;
                }
                catch (Exception ex)
                {
                    if (!AskYesNo($"Ошибка при открытии PDF: {ex.Message}. Повторить попытку? (y/n): ")) return;
                }
            }

            // 3. Основной цикл извлечения фрагментов из открытого файла
            try
            {
                while (true)
                {
                    int startPage = 0;
                    while (true)
                    {
                        Console.Write("\nВведите номер начальной страницы для извлечения: ");
                        string input = Console.ReadLine()?.Trim();

                        if (int.TryParse(input, out startPage) && startPage >= 1 && startPage <= inputDocument.PageCount)
                        {
                            break;
                        }
                        if (!AskYesNo($"Ошибка: Введите корректное число от 1 до {inputDocument.PageCount}. Повторить попытку? (y/n): ")) return;
                    }

                    int endPage = 0;
                    while (true)
                    {
                        Console.Write($"\nВведите номер конечной страницы для извлечения (от {startPage} до {inputDocument.PageCount}): ");
                        string input = Console.ReadLine()?.Trim();

                        if (int.TryParse(input, out endPage) && endPage >= startPage && endPage <= inputDocument.PageCount)
                        {
                            break;
                        }
                        if (!AskYesNo($"Ошибка: Введите число от {startPage} до {inputDocument.PageCount}. Повторить попытку? (y/n): ")) return;
                    }

                    string outputFileName = string.Empty;
                    while (true)
                    {
                        Console.Write("\nВведите название для сохраняемого фрагмента: ");
                        outputFileName = Console.ReadLine()?.Trim(' ', '"');

                        if (string.IsNullOrWhiteSpace(outputFileName))
                        {
                            if (!AskYesNo("Ошибка: Имя файла не может быть пустым. Повторить попытку? (y/n): ")) return;
                            continue;
                        }

                        if (outputFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                        {
                            if (!AskYesNo("Ошибка: Имя содержит недопустимые символы. Повторить попытку? (y/n): ")) return;
                            continue;
                        }

                        if (!outputFileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                        {
                            outputFileName += ".pdf";
                        }

                        string potentialOutputPath = Path.Combine(directoryPath, outputFileName);
                        if (string.Equals(sourceFilePath, potentialOutputPath, StringComparison.OrdinalIgnoreCase))
                        {
                            if (!AskYesNo("Ошибка: Имя выходного файла совпадает с исходным. Повторить попытку? (y/n): ")) return;
                            continue;
                        }
                        break;
                    }

                    // Сохранение текущего фрагмента
                    try
                    {
                        string outputPath = Path.Combine(directoryPath, outputFileName);
                        using PdfDocument outputDocument = new PdfDocument();

                        for (int idx = startPage - 1; idx < endPage; idx++)
                        {
                            outputDocument.AddPage(inputDocument.Pages[idx]);
                        }

                        outputDocument.Save(outputPath);
                        Console.WriteLine($"\nУспех! Фрагмент сохранен по пути: {outputPath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"\nПроизошла ошибка при сохранении PDF: {ex.Message}");
                    }

                    // Запрос на продолжение работы с этим же файлом
                    if (!AskYesNo("\nХотите вырезать еще один фрагмент из этого же файла? (y/n): "))
                    {
                        break;
                    }
                }
            }
            finally
            {
                // Документ освобождается только когда пользователь окончательно завершает работу
                inputDocument?.Dispose();
            }

            Console.WriteLine("\nРабота завершена. Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        // Вспомогательный метод для диалогов
        static bool AskYesNo(string message)
        {
            Console.Write(message);
            while (true)
            {
                string answer = Console.ReadLine()?.Trim().ToLower();

                if (answer == "y") return true;
                if (answer == "n") return false;

                Console.Write("Пожалуйста, введите 'y' для продолжения или 'n' для выхода: ");
            }
        }
    }
}
