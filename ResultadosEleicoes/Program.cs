//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="DevConn Software House">
//     Copyright (c) DevConn Software House and contributors. All rights reserved.
//     Licensed under the MIT license.
// </copyright>
//-----------------------------------------------------------------------

namespace ResultadosEleicoes
{
    using System;
    using System.Data;
    using System.IO.Compression;
    using System.Reflection;
    using System.Text;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Support.UI;
    using ResultadosEleicoes.Utils;

    /// <summary>
    /// Classe Program
    /// </summary>
    public static class Program
    {
        #region Campos
        /// <summary>
        /// Controle para a propriedade DiretorioAplicacao
        /// </summary>
        private static DirectoryInfo? diretorioAplicacao;

        /// <summary>
        /// Controle para a propriedade DiretorioBu
        /// </summary>
        private static DirectoryInfo? diretorioBu;
        #endregion

        #region Propriedades
        /// <summary>
        /// Obtém DiretorioAplicacao
        /// </summary>
        private static DirectoryInfo DiretorioAplicacao
        {
            get
            {
                Program.diretorioAplicacao ??= new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty);
                Program.diretorioAplicacao.Refresh();
                if (!Program.diretorioAplicacao.Exists)
                {
                    Program.diretorioAplicacao.Create();
                }

                return Program.diretorioAplicacao;
            }
        }

        /// <summary>
        /// Obtém DiretorioBu
        /// </summary>
        private static DirectoryInfo DiretorioBu
        {
            get
            {
                Program.diretorioBu ??= new DirectoryInfo(Path.Combine(Program.DiretorioAplicacao.FullName, "Bu"));
                Program.diretorioBu.Refresh();
                if (!Program.diretorioBu.Exists)
                {
                    Program.diretorioBu.Create();
                }

                return Program.diretorioBu;
            }
        }
        #endregion

        #region Métodos
        #region Públicos
        /// <summary>
        /// Método Main
        /// </summary>
        /// <param name="args">Parâmetro args</param>
        public static void Main()
        {
            try
            {
                // Status
                Console.WriteLine("---------------------------------------------------------------");
                Console.WriteLine("Sistema de Análise de Resultados (SAR) - DevConn Software House");
                Console.WriteLine("---------------------------------------------------------------");

                // Executar download dos arquivos
                string[] relacaoDiretoriosEstados = Directory.GetDirectories(Program.DiretorioBu.FullName);
                var relacaoDescricaoUf = Enum.GetValues(typeof(Enumeradores.UnidadeFederacaoEnum))
                    .Cast<Enumeradores.UnidadeFederacaoEnum>()
                    .Select(x => Enumeradores.ObterDescricao(x));
                if (!relacaoDiretoriosEstados.Any())
                {
                    // Status
                    Console.WriteLine("Obtendo os arquivos de BU do site 'https://cdn.tse.jus.br/'");

                    // Executar
                    IList<Task> listaDownloads = new List<Task>();
                    using IWebDriver driver = Program.ObterDriverPadrao();
                    WebDriverWait driverWaiter = new(driver, TimeSpan.FromSeconds(180)) { PollingInterval = TimeSpan.FromSeconds(1) };
                    foreach (string[] relacaoDescricaoUfParticionado in relacaoDescricaoUf.Chunk(8))
                    {
                        foreach (string descricaoUf in relacaoDescricaoUfParticionado)
                        {
                            try
                            {
                                listaDownloads.Add(Task.Run(() => driver.Navigate().GoToUrl($"https://cdn.tse.jus.br/estatistica/sead/eleicoes/eleicoes2022/buweb/bweb_2t_{descricaoUf}_311020221535.zip")));
                            }
                            catch
                            {
                                continue;
                            }
                        }

                        Task.WaitAll(listaDownloads.ToArray());
                        listaDownloads.Clear();
                        driverWaiter.Until(x => !Directory.GetFiles(Program.DiretorioBu.FullName).Any(x => x.EndsWith(".crdownload") || x.EndsWith(".tmp")));
                    }
                }

                // Extrair arquivos
                foreach (string descricaoUf in relacaoDescricaoUf)
                {
                    FileInfo zipInfo = new(Path.Combine(Program.DiretorioBu.FullName, $"bweb_2t_{descricaoUf}_311020221535.zip"));
                    if (!zipInfo.Exists)
                    {
                        continue;
                    }

                    Console.WriteLine($"Extraindo o arquivo {zipInfo.Name}");
                    string diretorioUf = Path.Combine(Program.DiretorioBu.FullName, descricaoUf);
                    ZipFile.ExtractToDirectory(zipInfo.FullName, diretorioUf);

                    // Deletar arquivos não importantes
                    zipInfo.Delete();
                    Directory.GetFiles(diretorioUf).Where(x => !x.EndsWith(".csv")).ToList().ForEach(x => File.Delete(x));
                }

                // Obter dados
                IList<DataTable> relacaoTabelas = new List<DataTable>();
                foreach (string descricaoUf in relacaoDescricaoUf)
                {
                    FileInfo arquivoAvaliar = new(Path.Combine(Program.DiretorioBu.FullName, descricaoUf, $"bweb_2t_{descricaoUf}_311020221535.csv"));
                    if (!arquivoAvaliar.Exists)
                    {
                        continue;
                    }

                    Console.WriteLine($"Obtendo dados do arquivo ./{descricaoUf}/{arquivoAvaliar.Name}");
                    DataTable? tabelaIncluir = Program.ObterDataTableFromCsv(descricaoUf, arquivoAvaliar.FullName);
                    if (tabelaIncluir == null)
                    {
                        continue;
                    }

                    relacaoTabelas.Add(tabelaIncluir);
                }

                // Executar Analisador
                Analisador analisador = new(relacaoTabelas);
                Console.WriteLine("Total de Votos (Geral): {0}", analisador.ObterTotalVotos());
                Console.WriteLine("Total de Votos (PT): {0}", analisador.ObterTotalVotos(null, new[] { "13" }, new[] { Enumeradores.CargoEnum.Presidente }));
                Console.WriteLine("Total de Votos (PL): {0}", analisador.ObterTotalVotos(null, new[] { "22" }, new[] { Enumeradores.CargoEnum.Presidente }));
                Console.WriteLine("Total de Votos (Brancos): {0}", analisador.ObterTotalVotos(null, new[] { "95" }, new[] { Enumeradores.CargoEnum.Presidente }));
                Console.WriteLine("Total de Votos (Nulos): {0}", analisador.ObterTotalVotos(null, new[] { "96" }, new[] { Enumeradores.CargoEnum.Presidente }));
                Console.ReadKey();
            }
            catch (Exception exp)
            {
                Console.WriteLine(Program.ObterMensagemCompleta(exp, true));
                Environment.Exit(-1);
            }
        }
        #endregion

        #region Privados
        /// <summary>
        /// Obter o driver padrão
        /// </summary>
        /// <returns>O driver padrão</returns>
        private static ChromeDriver ObterDriverPadrao()
        {
            ChromeOptions opcoes = new();
            opcoes.AddUserProfilePreference("download.default_directory", Program.DiretorioBu.FullName);
            opcoes.AddUserProfilePreference("download.directory_upgrade", true);
            opcoes.AddUserProfilePreference("download.prompt_for_download", false);
            return new ChromeDriver(opcoes);
        }

        /// <summary>
        /// Obter tabela de dados do arquivo CSV
        /// </summary>
        /// <param name="descricaoUf">Descrição da UF</param>
        /// <param name="path">Path do arquivo</param>
        /// <returns>Tabela de dados do arquivo</returns>
        private static DataTable? ObterDataTableFromCsv(string descricaoUf, string path)
        {
            // Registrar codificação
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // Obter colunas
            DataTable tabelaRetorno = new() { TableName = descricaoUf };
            using StreamReader reader = new(path, Encoding.GetEncoding(1252));
            string[]? colunas = reader.ReadLine()?.Split(';');
            if (colunas == null)
            {
                return null;
            }

            foreach (string coluna in colunas)
            {
                tabelaRetorno.Columns.Add(coluna.Replace("\"", string.Empty));
            }

            // Obter linhas
            while (!reader.EndOfStream)
            {
                string[]? linhas = reader.ReadLine()?.Split(';')?.Select(x => x.Replace("\"", string.Empty)).ToArray();
                if (linhas == null)
                {
                    continue;
                }

                DataRow dataRow = tabelaRetorno.NewRow();
                for (int i = 0; i < colunas.Length; i++)
                {
                    dataRow[i] = linhas[i];
                }

                tabelaRetorno.Rows.Add(dataRow);
            }

            // Retorno
            return tabelaRetorno;
        }

        /// <summary>
        /// Retorna a mensagem completa da exceção
        /// </summary>
        /// <param name="exception">Parâmetro exception</param>
        /// <param name="stackTrace">Parâmetro stackTrace</param>
        /// <returns>Mensagem do erro</returns>
        private static string ObterMensagemCompleta(Exception exception, bool stackTrace)
        {
            // Validar
            if (exception == null)
            {
                return "Mensagem de erro não identificada.";
            }

            // Mensagem
            string mensagem = exception.Message;
            while (exception.InnerException != null)
            {
                mensagem += string.Format("{0}{1}", Environment.NewLine, exception.InnerException.Message);
                exception = exception.InnerException;
            }

            // Sem StackTrace
            if (!stackTrace)
            {
                return mensagem;
            }

            // Com StackTrace
            return string.Format(
                "[ERRO]{0}{1}{0}{0}[StackTrace]{0}{2}", Environment.NewLine, mensagem, Program.ObterStackTraceCompleto(exception));
        }

        /// <summary>
        /// Retorna o StackTrace completo da exceção
        /// </summary>
        /// <param name="exception">Parâmetro exception</param>
        /// <returns>StackTrace do erro</returns>
        private static string ObterStackTraceCompleto(Exception exception)
        {
            // Parâmetros
            if (exception == null)
            {
                return "StackTrace não identificada.";
            }

            // Obter StackTrace
            string stackTrace = exception.StackTrace ?? string.Empty;
            while (exception.InnerException != null)
            {
                stackTrace += string.Format("{0}{1}", Environment.NewLine, exception.InnerException.StackTrace);
                exception = exception.InnerException;
            }

            return stackTrace;
        }
        #endregion
        #endregion
    }
}