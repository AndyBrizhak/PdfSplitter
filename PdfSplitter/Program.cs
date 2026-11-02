using PdfSharp;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.IO;
using System.Reflection.PortableExecutable;

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
                Console.Write("\nВведите путь к папке с файлом (например, D:\\Temp\\Anatomy): ");
                directoryPath = Console.ReadLine()?.Trim(' ', '"');

                if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
                {
                    if (!AskToContinue("Ошибка: Указанная папка не найдена.")) return;
                    continue;
                }
                break;
            }

            // 2. Ввод названия исходного файла
            while (true)
            {
                Console.Write("\nВведите название исходного PDF-файла (например, Anatomy или Anatomy.pdf): ");
                string fileName = Console.ReadLine()?.Trim(' ', '"');

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    if (!AskToContinue("Ошибка: Имя файла не может быть пустым.")) return;
                    continue;
                }

                if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    fileName += ".pdf";
                }

                sourceFilePath = Path.Combine(directoryPath, fileName);

                if (!File.Exists(sourceFilePath))
                {
                    if (!AskToContinue($"Ошибка: Файл '{fileName}' не найден в папке '{directoryPath}'.")) return;
                    continue;
                }

                try
                {
                    inputDocument = PdfReader.Open(sourceFilePath, PdfDocumentOpenMode.Import);
                    Console.WriteLine($"Файл успешно открыт. Всего страниц: {inputDocument.PageCount}");
                    break;
                }
                catch (Exception ex)
                {
                    if (!AskToContinue($"Ошибка при открытии PDF: {ex.Message}")) return;
                }
            }

            // 3. Ввод начальной страницы
            int startPage = 0;
            while (true)
            {
                Console.Write("\nВведите номер начальной страницы для извлечения: ");
                string input = Console.ReadLine()?.Trim();

                if (int.TryParse(input, out startPage) && startPage >= 1 && startPage <= inputDocument.PageCount)
                {
                    break;
                }

                if (!AskToContinue($"Ошибка: Введите корректное число от 1 до {inputDocument.PageCount}.")) return;
            }

            // 4. Ввод конечной страницы
            int endPage = 0;
            while (true)
            {
                Console.Write($"\nВведите номер конечной страницы для извлечения (от {startPage} до {inputDocument.PageCount}): ");
                string input = Console.ReadLine()?.Trim();

                if (int.TryParse(input, out endPage) && endPage >= startPage && endPage <= inputDocument.PageCount)
                {
                    break;
                }

                if (!AskToContinue($"Ошибка: Введите корректное число от {startPage} до {inputDocument.PageCount}.")) return;
            }

            // 5. Ввод названия выходного фрагмента
            string outputFileName = string.Empty;
            while (true)
            {
                Console.Write("\nВведите название для сохраняемого фрагмента (например, part1): ");
                outputFileName = Console.ReadLine()?.Trim(' ', '"');

                if (string.IsNullOrWhiteSpace(outputFileName))
                {
                    if (!AskToContinue("Ошибка: Имя файла не может быть пустым.")) return;
                    continue;
                }

                if (outputFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                {
                    if (!AskToContinue("Ошибка: Имя файла содержит недопустимые символы.")) return;
                    continue;
                }

                if (!outputFileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    outputFileName += ".pdf";
                }

                string potentialOutputPath = Path.Combine(directoryPath, outputFileName);
                if (string.Equals(sourceFilePath, potentialOutputPath, StringComparison.OrdinalIgnoreCase))
                {
                    if (!AskToContinue("Ошибка: Имя выходного файла совпадает с исходным. Выберите другое имя.")) return;
                    continue;
                }

                break;
            }

            // 6. Извлечение страниц и сохранение в ту же папку
            try
            {
                string outputPath = Path.Combine(directoryPath, outputFileName);

                using PdfDocument outputDocument = new PdfDocument();

                for (int idx = startPage - 1; idx < endPage; idx++)
                {
                    outputDocument.AddPage(inputDocument.Pages[idx]);
                }

                outputDocument.Save(outputPath);
                Console.WriteLine($"\nУспех! Фрагмент сохранен по пути:");
                Console.WriteLine(outputPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nПроизошла ошибка при сохранении PDF: {ex.Message}");
            }
            finally
            {
                inputDocument?.Dispose();
            }

            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        static bool AskToContinue(string errorMessage)
        {
            Console.WriteLine(errorMessage);
            while (true)
            {
                Console.Write("Повторить попытку? (y/n): ");
                string answer = Console.ReadLine()?.Trim().ToLower();

                if (answer == "y") return true;
                if (answer == "n") return false;

                Console.WriteLine("Пожалуйста, введите 'y' для продолжения или 'n' для выхода.");
            }
        }
    }
}