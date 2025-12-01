using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace patrimonioDB.Features.PdfFuncionario
{
    /// <summary>
    /// Service para gerar PDF com detalhes dos funcionários
    /// Usa a biblioteca QuestPDF
    /// </summary>
    public class PdfService
    {
        private readonly DocumentoRepository _documentoRepository;

        public PdfService()
        {
            _documentoRepository = new DocumentoRepository();
        }

        /// <summary>
        /// Gera um PDF com a lista de funcionários
        /// </summary>
        /// <param name="funcionarios">Lista de funcionários detalhados</param>
        /// <param name="caminhoArquivo">Caminho completo onde salvar o PDF</param>
        public void GerarPdfFuncionarios(List<FuncionarioDetalhado> funcionarios, string caminhoArquivo)
        {
            try
            {
                // Configurar licença comunitária do QuestPDF (gratuita)
                QuestPDF.Settings.License = LicenseType.Community;

                // Criar documento PDF
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        // Configurações da página
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        // Cabeçalho
                        page.Header()
                            .Text("Relatório de Funcionários")
                            .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                        // Conteúdo
                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(column =>
                            {
                                column.Spacing(5);

                                // Informações do relatório
                                column.Item().Text($"Data de geração: {DateTime.Now:dd/MM/yyyy HH:mm}");
                                column.Item().Text($"Total de funcionários: {funcionarios.Count}");
                                column.Item().PaddingTop(0.5f, Unit.Centimetre);

                                // Tabela com os dados
                                column.Item().Table(table =>
                                {
                                    // Definir colunas
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(40);   // ID
                                        columns.RelativeColumn(3);     // Nome
                                        columns.RelativeColumn(2);     // CPF
                                        columns.RelativeColumn(2);     // Cargo
                                        columns.RelativeColumn(2);     // Setor
                                        columns.RelativeColumn(1.5f);  // Salário
                                    });

                                    // Cabeçalho da tabela
                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).Text("ID").Bold();
                                        header.Cell().Element(CellStyle).Text("Nome").Bold();
                                        header.Cell().Element(CellStyle).Text("CPF").Bold();
                                        header.Cell().Element(CellStyle).Text("Cargo").Bold();
                                        header.Cell().Element(CellStyle).Text("Setor").Bold();
                                        header.Cell().Element(CellStyle).Text("Salário").Bold();

                                        // Estilo do cabeçalho
                                        IContainer CellStyle(IContainer container)
                                        {
                                            return container
                                                .DefaultTextStyle(x => x.SemiBold())
                                                .PaddingVertical(5)
                                                .BorderBottom(1)
                                                .BorderColor(Colors.Black);
                                        }
                                    });

                                    // Linhas da tabela
                                    foreach (var func in funcionarios)
                                    {
                                        table.Cell().Element(CellStyle).Text(func.ID_Pessoa.ToString());
                                        table.Cell().Element(CellStyle).Text(func.Nome_Funcionario);
                                        table.Cell().Element(CellStyle).Text(func.CPF);
                                        table.Cell().Element(CellStyle).Text(func.CargoOuSemCargo);
                                        table.Cell().Element(CellStyle).Text(func.SetorOuSemSetor);
                                        table.Cell().Element(CellStyle).Text(func.SalarioFormatado);

                                        // Estilo das células
                                        IContainer CellStyle(IContainer container)
                                        {
                                            return container
                                                .BorderBottom(1)
                                                .BorderColor(Colors.Grey.Lighten2)
                                                .PaddingVertical(5);
                                        }
                                    }
                                });
                            });

                        // Rodapé
                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Página ");
                                x.CurrentPageNumber();
                                x.Span(" de ");
                                x.TotalPages();
                            });
                    });
                })
                .GeneratePdf(caminhoArquivo);

                Debug.WriteLine($"✓ PDF gerado com sucesso: {caminhoArquivo}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERRO ao gerar PDF: {ex.Message}");
                throw new Exception($"Erro ao gerar PDF: {ex.Message}");
            }
        }

        /// <summary>
        /// ✅ NOVO: Gera PDF e salva no banco de dados
        /// </summary>
        public async Task<int> GerarESalvarPdfAsync(List<FuncionarioDetalhado> funcionarios, string nomeArquivo, bool porSetor = false)
        {
            try
            {
                // Gerar PDF em memória (temporário)
                string tempPath = Path.GetTempFileName();
      
                if (porSetor)
                {
                     GerarPdfPorSetor(funcionarios, tempPath);
                }
                else
                {
                     GerarPdfFuncionarios(funcionarios, tempPath);
                }

                // Salvar no banco de dados
                int documentoId = await _documentoRepository.SalvarPdfAsync(nomeArquivo, tempPath);

                // Deletar arquivo temporário
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                Debug.WriteLine($"✓ PDF salvo no banco com ID: {documentoId}");
                return documentoId;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERRO ao gerar e salvar PDF: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gera um PDF detalhado com informações agrupadas por setor
        /// </summary>
        public void GerarPdfPorSetor(List<FuncionarioDetalhado> funcionarios, string caminhoArquivo)
        {
            try
            {
                QuestPDF.Settings.License = LicenseType.Community;

                // Agrupar por setor
                var porSetor = new Dictionary<string, List<FuncionarioDetalhado>>();
                foreach (var func in funcionarios)
                {
                    string setor = func.SetorOuSemSetor;
                    if (!porSetor.ContainsKey(setor))
                    {
                        porSetor[setor] = new List<FuncionarioDetalhado>();
                    }
                    porSetor[setor].Add(func);
                }

                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header()
                            .Text("Relatório de Funcionários por Setor")
                            .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(column =>
                            {
                                column.Spacing(10);

                                column.Item().Text($"Data: {DateTime.Now:dd/MM/yyyy HH:mm}");
                                column.Item().Text($"Total de funcionários: {funcionarios.Count}");

                                // Para cada setor
                                foreach (var grupo in porSetor)
                                {
                                    column.Item().PaddingTop(0.5f, Unit.Centimetre);
                                    column.Item().Text($"SETOR: {grupo.Key}")
                                        .Bold().FontSize(14).FontColor(Colors.Blue.Darken1);
                                    
                                    column.Item().Text($"Total: {grupo.Value.Count} funcionário(s)")
                                        .FontSize(10).Italic();

                                    // Lista de funcionários do setor
                                    foreach (var func in grupo.Value)
                                    {
                                        column.Item().PaddingLeft(0.5f, Unit.Centimetre).Text(text =>
                                        {
                                            text.Span("• ").Bold();
                                            text.Span($"{func.Nome_Funcionario} ");
                                            text.Span($"({func.CargoOuSemCargo}) - {func.SalarioFormatado}");
                                        });
                                    }
                                }
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Página ");
                                x.CurrentPageNumber();
                            });
                    });
                })
                .GeneratePdf(caminhoArquivo);

                Debug.WriteLine($"✓ PDF por setor gerado: {caminhoArquivo}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERRO ao gerar PDF: {ex.Message}");
                throw new Exception($"Erro ao gerar PDF: {ex.Message}");
            }
        }
    }
}
