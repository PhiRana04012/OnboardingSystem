using ClosedXML.Excel;
using OnboardingSystem.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OnboardingSystem.Services;

public class ReportExportService : IReportExportService
{
    public byte[] ExportOnboardingProgressToExcel(OnboardingProgressReportDto report)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Прогресс онбординга");

        ws.Cell(1, 1).Value = "Отчет о прогрессе онбординга";
        ws.Range(1, 1, 1, 4).Merge().Style.Font.SetBold().Font.FontSize = 14;

        ws.Cell(3, 1).Value = "ФИО"; ws.Cell(3, 2).Value = report.FullName;
        ws.Cell(4, 1).Value = "Email"; ws.Cell(4, 2).Value = report.Email;
        ws.Cell(5, 1).Value = "Подразделение"; ws.Cell(5, 2).Value = report.DepartmentName;
        ws.Cell(6, 1).Value = "Наставник"; ws.Cell(6, 2).Value = report.MentorName ?? "—";
        ws.Cell(7, 1).Value = "Статус"; ws.Cell(7, 2).Value = report.OnboardingStatus;
        ws.Cell(8, 1).Value = "Прогресс"; ws.Cell(8, 2).Value = $"{Math.Round(report.ProgressPercentage)}%";

        ws.Range("A3:A8").Style.Font.SetBold();

        int row = 10;
        ws.Cell(row, 1).Value = "Модуль";
        ws.Cell(row, 2).Value = "Обязательный";
        ws.Cell(row, 3).Value = "Статус";
        ws.Cell(row, 4).Value = "Попыток";
        ws.Cell(row, 5).Value = "Результат";
        ws.Cell(row, 6).Value = "Дата завершения";
        ws.Range($"A{row}:F{row}").Style.Font.SetBold().Fill.BackgroundColor = XLColor.LightGray;

        row++;
        foreach (var m in report.ModuleStatuses)
        {
            ws.Cell(row, 1).Value = m.ModuleTitle;
            ws.Cell(row, 2).Value = m.IsMandatory ? "Да" : "Нет";
            ws.Cell(row, 3).Value = m.Status;
            ws.Cell(row, 4).Value = m.AttemptsCount;
            ws.Cell(row, 5).Value = m.BestScore.HasValue ? $"{Math.Round(m.BestScore.Value)}%" : "—";
            ws.Cell(row, 6).Value = m.CompletionDate?.ToString("dd.MM.yyyy") ?? "—";
            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportOnboardingProgressToPdf(OnboardingProgressReportDto report)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Text("Отчет о прогрессе онбординга").SemiBold().FontSize(18).FontColor(Colors.Blue.Darken2);

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(x =>
                {
                    x.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c => { c.RelativeColumn(1); c.RelativeColumn(2); });
                        t.Cell().Text("ФИО:").SemiBold(); t.Cell().Text(report.FullName);
                        t.Cell().Text("Подразделение:").SemiBold(); t.Cell().Text(report.DepartmentName);
                        t.Cell().Text("Наставник:").SemiBold(); t.Cell().Text(report.MentorName ?? "—");
                        t.Cell().Text("Статус:").SemiBold(); t.Cell().Text(report.OnboardingStatus);
                        t.Cell().Text("Прогресс:").SemiBold(); t.Cell().Text($"{Math.Round(report.ProgressPercentage)}%");
                    });

                    x.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    x.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2); c.RelativeColumn(); c.RelativeColumn();
                            c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn();
                        });

                        t.Header(h =>
                        {
                            h.Cell().Text("Модуль").SemiBold();
                            h.Cell().Text("Обязат.").SemiBold();
                            h.Cell().Text("Статус").SemiBold();
                            h.Cell().Text("Попыток").SemiBold();
                            h.Cell().Text("Счет").SemiBold();
                            h.Cell().Text("Дата").SemiBold();
                        });

                        foreach (var m in report.ModuleStatuses)
                        {
                            t.Cell().Text(m.ModuleTitle);
                            t.Cell().Text(m.IsMandatory ? "Да" : "Нет");
                            t.Cell().Text(m.Status);
                            t.Cell().Text(m.AttemptsCount.ToString());
                            t.Cell().Text(m.BestScore.HasValue ? $"{Math.Round(m.BestScore.Value)}%" : "—");
                            t.Cell().Text(m.CompletionDate?.ToString("dd.MM.yyyy") ?? "—");
                        }
                    });
                });

                page.Footer().AlignCenter().Text(x => { x.CurrentPageNumber(); x.Span(" / "); x.TotalPages(); });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] ExportTestResultsToExcel(List<TestResultsReportDto> report)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Результаты тестов");

        ws.Cell(1, 1).Value = "Отчет о результатах тестов";
        ws.Range(1, 1, 1, 4).Merge().Style.Font.SetBold().Font.FontSize = 14;

        int row = 3;
        ws.Cell(row, 1).Value = "Сотрудник";
        ws.Cell(row, 2).Value = "Модуль";
        ws.Cell(row, 3).Value = "Дата";
        ws.Cell(row, 4).Value = "Попытка";
        ws.Cell(row, 5).Value = "Счет";
        ws.Cell(row, 6).Value = "Статус";
        ws.Range($"A{row}:F{row}").Style.Font.SetBold().Fill.BackgroundColor = XLColor.LightGray;

        row++;
        foreach (var t in report)
        {
            ws.Cell(row, 1).Value = t.FullName;
            ws.Cell(row, 2).Value = t.ModuleTitle;
            ws.Cell(row, 3).Value = t.AttemptDate.ToString("dd.MM.yyyy HH:mm");
            ws.Cell(row, 4).Value = t.AttemptNumber;
            ws.Cell(row, 5).Value = $"{Math.Round(t.Score)}%";
            ws.Cell(row, 6).Value = t.IsPassed ? "Сдано" : "Не сдано";
            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportTestResultsToPdf(List<TestResultsReportDto> report)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Text("Результаты тестов").SemiBold().FontSize(18).FontColor(Colors.Blue.Darken2);

                page.Content().PaddingVertical(1, Unit.Centimetre).Table(t =>
                {
                    t.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn();
                        c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn();
                    });

                    t.Header(h =>
                    {
                        h.Cell().Text("Сотрудник").SemiBold();
                        h.Cell().Text("Модуль").SemiBold();
                        h.Cell().Text("Дата").SemiBold();
                        h.Cell().Text("Попытка").SemiBold();
                        h.Cell().Text("Счет").SemiBold();
                        h.Cell().Text("Статус").SemiBold();
                    });

                    foreach (var m in report)
                    {
                        t.Cell().Text(m.FullName);
                        t.Cell().Text(m.ModuleTitle);
                        t.Cell().Text(m.AttemptDate.ToString("dd.MM.yyyy"));
                        t.Cell().Text(m.AttemptNumber.ToString());
                        t.Cell().Text($"{Math.Round(m.Score)}%");
                        t.Cell().Text(m.IsPassed ? "Сдано" : "Не сдано");
                    }
                });

                page.Footer().AlignCenter().Text(x => { x.CurrentPageNumber(); x.Span(" / "); x.TotalPages(); });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] ExportDepartmentReportToExcel(DepartmentReportDto report)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Отчет по подразделению");

        ws.Cell(1, 1).Value = $"Отчет по подразделению: {report.DepartmentName}";
        ws.Range(1, 1, 1, 4).Merge().Style.Font.SetBold().Font.FontSize = 14;

        ws.Cell(3, 1).Value = "Всего сотрудников"; ws.Cell(3, 2).Value = report.TotalUsers;
        ws.Cell(4, 1).Value = "В процессе"; ws.Cell(4, 2).Value = report.UsersInProgress;
        ws.Cell(5, 1).Value = "Завершено"; ws.Cell(5, 2).Value = report.UsersCompleted;
        ws.Cell(6, 1).Value = "Средний прогресс"; ws.Cell(6, 2).Value = $"{Math.Round(report.AverageProgressPercentage)}%";

        ws.Range("A3:A6").Style.Font.SetBold();

        int row = 8;
        ws.Cell(row, 1).Value = "Сотрудник";
        ws.Cell(row, 2).Value = "Статус";
        ws.Cell(row, 3).Value = "Прогресс";
        ws.Cell(row, 4).Value = "Дата завершения";
        ws.Range($"A{row}:D{row}").Style.Font.SetBold().Fill.BackgroundColor = XLColor.LightGray;

        row++;
        foreach (var u in report.Users)
        {
            ws.Cell(row, 1).Value = u.FullName;
            ws.Cell(row, 2).Value = u.OnboardingStatus;
            ws.Cell(row, 3).Value = $"{Math.Round(u.ProgressPercentage)}%";
            ws.Cell(row, 4).Value = u.CompletionDate?.ToString("dd.MM.yyyy") ?? "—";
            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportDepartmentReportToPdf(DepartmentReportDto report)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Text($"Отчет по подразделению: {report.DepartmentName}").SemiBold().FontSize(18).FontColor(Colors.Blue.Darken2);

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(x =>
                {
                    x.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c => { c.RelativeColumn(1); c.RelativeColumn(2); });
                        t.Cell().Text("Всего сотрудников:").SemiBold(); t.Cell().Text(report.TotalUsers.ToString());
                        t.Cell().Text("В процессе:").SemiBold(); t.Cell().Text(report.UsersInProgress.ToString());
                        t.Cell().Text("Завершено:").SemiBold(); t.Cell().Text(report.UsersCompleted.ToString());
                        t.Cell().Text("Средний прогресс:").SemiBold(); t.Cell().Text($"{Math.Round(report.AverageProgressPercentage)}%");
                    });

                    x.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    x.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn();
                        });

                        t.Header(h =>
                        {
                            h.Cell().Text("Сотрудник").SemiBold();
                            h.Cell().Text("Статус").SemiBold();
                            h.Cell().Text("Прогресс").SemiBold();
                            h.Cell().Text("Дата").SemiBold();
                        });

                        foreach (var u in report.Users)
                        {
                            t.Cell().Text(u.FullName);
                            t.Cell().Text(u.OnboardingStatus);
                            t.Cell().Text($"{Math.Round(u.ProgressPercentage)}%");
                            t.Cell().Text(u.CompletionDate?.ToString("dd.MM.yyyy") ?? "—");
                        }
                    });
                });

                page.Footer().AlignCenter().Text(x => { x.CurrentPageNumber(); x.Span(" / "); x.TotalPages(); });
            });
        });

        return document.GeneratePdf();
    }
}
