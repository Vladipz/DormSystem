using System;
using System.Globalization;

using Inspections.API.Entities;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Inspections.API.Services
{
    public sealed class PdfReportService : IPdfReportService
    {
        private readonly string _generatedOn = DateTime.UtcNow.ToString("g", CultureInfo.InvariantCulture);

        public PdfReportService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerateSimpleReport(Inspection inspection)
        {
            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Size(PageSizes.A4);

                    page.Header().Element(h => h
                        .PaddingBottom(10)
                        .Text($"Inspection Report – {inspection.Name}")
                        .FontSize(20)
                        .SemiBold());

                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Type: {inspection.Type}");
                        col.Item().Text($"Start: {inspection.StartDate:g}");
                        col.Item().Text($"Status: {inspection.Status}");
                        col.Item().PaddingVertical(10);
                        col.Item().Element(BuildRoomTable(inspection, false));

                        // Додаємо новий блок, який буде містити підписи
                        // Це допоможе запобігти розриву між сторінками
                        col.Item().PaddingTop(50); // Додаємо великий відступ для переходу на нову сторінку, якщо потрібно
                        col.Item().Element(ComposeSimpleSignatureSection);
                    });

                    page.Footer().AlignCenter().Text(_generatedOn).FontSize(9);
                });
            });

            return doc.GeneratePdf();

            void ComposeSimpleSignatureSection(IContainer container)
            {
                container.Column(col =>
                {
                    // Додаємо заголовок для секції підпису
                    col.Item().PaddingBottom(10).Text("Signatures").SemiBold();

                    // Додаємо рядки для підпису
                    col.Item().Row(row =>
                    {
                        // Інспектор
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Inspector:");
                            c.Item().PaddingTop(20).LineHorizontal(1);
                        });

                        // Відступ між підписами
                        row.ConstantItem(30);

                        // Представник гуртожитку
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Dormitory Representative:");
                            c.Item().PaddingTop(20).LineHorizontal(1);
                        });
                    });

                    // Додаємо дату підпису
                    col.Item().PaddingTop(20).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Date:");
                            c.Item().PaddingTop(20).LineHorizontal(1);
                        });

                        // Заповнюємо решту простору
                        row.RelativeItem(3);
                    });
                });
            }
        }


        public byte[] GenerateFancyReport(Inspection inspection)
        {
            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(25);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.Grey.Lighten4);

                    page.Header().Element(ComposeHeader);

                    page.Content().Padding(10).Column(col =>
                    {
                        col.Item().Text($"Type: {inspection.Type}");
                        col.Item().Text($"Start: {inspection.StartDate:g}");
                        col.Item().Text($"Status: {inspection.Status}");
                        col.Item().PaddingVertical(8);
                        col.Item().Element(BuildFancyRoomGrid(inspection));

                        // Додаємо секцію для підпису
                        col.Item().PaddingTop(30).Element(ComposeSignatureSection);
                    });

                    page.Footer().AlignRight().Text(t =>
                    {
                        t.Span("Generated on ").FontSize(9);
                        t.Span(_generatedOn).FontSize(9).SemiBold();
                    });
                });
            });

            return doc.GeneratePdf();

            void ComposeHeader(IContainer container)
            {
                container.Padding(8)
                         .AlignCenter()
                         .Text($"🏠 Dormitory Inspection Report – {inspection.Name}")
                         .FontSize(22)
                         .SemiBold();
            }

            void ComposeSignatureSection(IContainer container)
            {
                container.Column(col =>
                {
                    // Додаємо заголовок для секції підпису
                    col.Item().PaddingBottom(5).Text("Inspection Confirmation").FontSize(12).SemiBold();

                    // Додаємо рядки для підпису
                    col.Item().Row(row =>
                    {
                        // Інспектор
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Inspector").FontSize(10).SemiBold();
                            c.Item().PaddingTop(20).LineHorizontal(1).LineColor(Colors.Black);
                            c.Item().PaddingTop(5).Text("Full Name & Signature").FontSize(8).FontColor(Colors.Grey.Medium);
                        });

                        // Відступ між підписами
                        row.ConstantItem(40);

                        // Представник гуртожитку
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Dormitory Representative").FontSize(10).SemiBold();
                            c.Item().PaddingTop(20).LineHorizontal(1).LineColor(Colors.Black);
                            c.Item().PaddingTop(5).Text("Full Name & Signature").FontSize(8).FontColor(Colors.Grey.Medium);
                        });
                    });

                    // Додаємо дату підпису
                    col.Item().PaddingTop(20).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Date").FontSize(10).SemiBold();
                            c.Item().PaddingTop(20).LineHorizontal(1).LineColor(Colors.Black);
                            c.Item().PaddingTop(5).Text("DD/MM/YYYY").FontSize(8).FontColor(Colors.Grey.Medium);
                        });

                        // Заповнюємо решту простору
                        row.RelativeItem(3);
                    });
                });
            }

            void ComposeKeyValue(IContainer root, string key, string value)
            {
                root.Row(r =>
                {
                    r.RelativeItem(1).Text(key).SemiBold();
                    r.RelativeItem(3).Text(value);
                });
            }
        }

        private static Action<IContainer> BuildRoomTable(Inspection inspection, bool fancy)
            => table =>
        {
            table.Table(t =>
            {
                t.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(2);
                    c.RelativeColumn(1);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(3);
                });

                t.Header(h =>
                {
                    void HeaderCell(IContainer c, string text) =>
                        c.Background(Colors.Grey.Lighten2)
                         .PaddingVertical(4)
                         .DefaultTextStyle(s => s.SemiBold())
                         .Text(text);

                    h.Cell().Element(c => HeaderCell(c, "Room"));
                    h.Cell().Element(c => HeaderCell(c, "Floor"));
                    h.Cell().Element(c => HeaderCell(c, "Building"));
                    h.Cell().Element(c => HeaderCell(c, "Status"));
                    h.Cell().Element(c => HeaderCell(c, "Comment"));
                });

                bool alt = false;
                foreach (var r in inspection.Rooms.OrderBy(r => r.RoomNumber, StringComparer.Ordinal))
                {
                    var background = fancy && alt ? Colors.Grey.Lighten4 : Colors.White;
                    alt = !alt;

                    t.Cell().Element(cell => cell.Background(background).Text(r.RoomNumber));
                    t.Cell().Element(cell => cell.Background(background).Text(r.Floor));
                    t.Cell().Element(cell => cell.Background(background).Text(r.Building));
                    t.Cell().Element(cell => cell.Background(background).Text(StatusDisplay(r.Status)));
                    t.Cell().Element(cell => cell.Background(background).Text(string.IsNullOrWhiteSpace(r.Comment) ? "-" : r.Comment));
                }

                string StatusDisplay(RoomInspectionStatus s) => fancy ? s switch
                {
                    RoomInspectionStatus.Confirmed => "✔ Confirmed",
                    RoomInspectionStatus.NotConfirmed => "✖ Not Confirmed",
                    RoomInspectionStatus.NoAccess => "🚪 No Access",
                    _ => s.ToString()
                } : s.ToString();
            });
        };

        private static Action<IContainer> BuildFancyRoomGrid(Inspection inspection)
            => container =>
        {
            container.Column(col =>
            {
                col.Spacing(10);

                foreach (var r in inspection.Rooms.OrderBy(x => x.RoomNumber, StringComparer.Ordinal))
                {
                    // Визначаємо основний колір картки
                    string cardBackground = Colors.White;
                    string commentBackground = GetStatusLightColor(r.Status);
                    string commentTextColor = GetStatusTextColor(r.Status);

                    col.Item()
                        .Background(cardBackground)
                        .Padding(15)
                        .Column(grid =>
                        {
                            // Заголовок з номером кімнати
                            grid.Item().Row(row =>
                            {
                                row.RelativeItem().Column(titleCol =>
                                {
                                    titleCol.Item().Text($"Room {r.RoomNumber}")
                                        .FontSize(14)
                                        .Bold();
                                    titleCol.Item().Text($"Floor {r.Floor}, Building {r.Building}")
                                        .FontSize(11)
                                        .FontColor(Colors.Grey.Medium);
                                });

                                // Статус справа у вигляді бейджа
                                row.AutoItem().AlignRight().AlignMiddle()
                                    .Background(GetStatusColor(r.Status))
                                    .Padding(5)
                                    .PaddingLeft(8)
                                    .PaddingRight(8)
                                    .Text(StatusDisplay(r.Status))
                                    .FontColor(GetStatusTextColor(r.Status))
                                    .FontSize(10);
                            });

                            // Коментар, якщо є
                            if (!string.IsNullOrWhiteSpace(r.Comment))
                            {
                                grid.Item().PaddingTop(10).Column(commentCol =>
                                {
                                    // Додаємо padding до контейнера, а не до тексту
                                    commentCol.Item().PaddingBottom(3).Text("Comment:")
                                        .FontSize(11)
                                        .Bold()
                                        .FontColor(commentTextColor);

                                    commentCol.Item().Background(commentBackground)
                                        .Padding(8)
                                        .Text(r.Comment)
                                        .FontColor(commentTextColor)
                                        .FontSize(11);
                                });
                            }
                        });
                }
            });

            static string StatusDisplay(RoomInspectionStatus s) => s switch
            {
                RoomInspectionStatus.Confirmed => "Confirmed",
                RoomInspectionStatus.NotConfirmed => "Not Confirmed",
                RoomInspectionStatus.NoAccess => "No Access",
                _ => s.ToString()
            };

            static string GetStatusColor(RoomInspectionStatus s) => s switch
            {
                RoomInspectionStatus.Confirmed => Colors.Green.Lighten5,
                RoomInspectionStatus.NotConfirmed => Colors.Red.Lighten5,
                RoomInspectionStatus.NoAccess => Colors.Yellow.Lighten5,
                _ => Colors.Grey.Lighten5
            };

            static string GetStatusLightColor(RoomInspectionStatus s) => s switch
            {
                RoomInspectionStatus.Confirmed => Colors.Green.Lighten5,
                RoomInspectionStatus.NotConfirmed => Colors.Red.Lighten5,
                RoomInspectionStatus.NoAccess => Colors.Yellow.Lighten5,
                _ => Colors.Grey.Lighten5
            };

            static string GetStatusTextColor(RoomInspectionStatus s) => s switch
            {
                RoomInspectionStatus.Confirmed => Colors.Green.Darken2,
                RoomInspectionStatus.NotConfirmed => Colors.Red.Darken1,
                RoomInspectionStatus.NoAccess => Colors.Orange.Darken2,
                _ => Colors.Grey.Darken2
            };
        };
    }
}
