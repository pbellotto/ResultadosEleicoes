//-----------------------------------------------------------------------
// <copyright file="Analisador.cs" company="DevConn Software House">
//     Copyright (c) DevConn Software House and contributors. All rights reserved.
//     Licensed under the MIT license.
// </copyright>
//-----------------------------------------------------------------------

namespace ResultadosEleicoes
{
    using System.Data;
    using ResultadosEleicoes.Utils;

    /// <summary>
    /// Classe Analisador
    /// </summary>
    public sealed class Analisador
    {
        #region Constantes
        /// <summary>
        /// Constante CD_CARGO_PERGUNTA
        /// </summary>
        private const string CD_CARGO_PERGUNTA = "CD_CARGO_PERGUNTA";

        /// <summary>
        /// Constante NR_VOTAVEL
        /// </summary>
        private const string NR_VOTAVEL = "NR_VOTAVEL";

        /// <summary>
        /// Constante QT_VOTOS
        /// </summary>
        private const string QT_VOTOS = "QT_VOTOS";
        #endregion

        #region Construtor
        /// <summary>
        /// Inicia uma nova instância da classe <see cref="Analisador"/>
        /// </summary>
        /// <param name="relacaoTabelas"></param>
        public Analisador(IList<DataTable> relacaoTabelas)
        {
            this.RelacaoTabelas = relacaoTabelas ?? throw new Exception("A relação de tabelas deve ser informada");
            this.RelacaoTabelasUfLookup = relacaoTabelas.ToLookup(x => x.TableName);
        }
        #endregion

        #region Propriedades
        #region Públicos
        /// <summary>
        /// Obtém RelacaoTabelas
        /// </summary>
        public IList<DataTable> RelacaoTabelas { get; }
        #endregion

        #region Públicos
        /// <summary>
        /// Obtém RelacaoTabelasUfLookup
        /// </summary>
        private ILookup<string, DataTable> RelacaoTabelasUfLookup { get; }
        #endregion
        #endregion

        #region Métodos
        #region Públicos
        /// <summary>
        /// Obter o total de votos
        /// </summary>
        /// <returns>O total de votos</returns>
        public int ObterTotalVotos()
        {
            return this.ObterTotalVotos(null, Array.Empty<string>(), Array.Empty<Enumeradores.CargoEnum>());
        }

        /// <summary>
        /// Obter o total de votos
        /// </summary>
        /// <param name="unidadeFederacao">Unidade da Federação</param>
        /// <returns>O total de votos</returns>
        public int ObterTotalVotos(Enumeradores.UnidadeFederacaoEnum unidadeFederacao)
        {
            return this.ObterTotalVotos(unidadeFederacao, Array.Empty<string>(), Array.Empty<Enumeradores.CargoEnum>());
        }

        /// <summary>
        /// Obter o total de votos
        /// </summary>
        /// <param name="unidadeFederacao">Unidade da Federação</param>
        /// <param name="numerosPartidos">Relação de números dos partidos</param>
        /// <returns>O total de votos</returns>
        public int ObterTotalVotos(Enumeradores.UnidadeFederacaoEnum unidadeFederacao, string[] numerosPartidos)
        {
            return this.ObterTotalVotos(unidadeFederacao, numerosPartidos, Array.Empty<Enumeradores.CargoEnum>());
        }

        /// <summary>
        /// Obter o total de votos
        /// </summary>
        /// <param name="unidadeFederacao">Unidade da Federação</param>
        /// <param name="numerosPartidos">Relação de números dos partidos</param>
        /// <returns>O total de votos</returns>
        public int ObterTotalVotos(Enumeradores.UnidadeFederacaoEnum? unidadeFederacao, string[] numerosPartidos, Enumeradores.CargoEnum[] cargos)
        {
            // Obter total de votos
            var gruposUf = this.ObterLinhasPorUf(unidadeFederacao)?.AsQueryable();
            if (numerosPartidos.Any())
            {
                gruposUf = gruposUf?
                    .GroupBy(x => x.Field<string>(Analisador.NR_VOTAVEL))
                    .Where(x => numerosPartidos.Any(y => y == x.Key))
                    .SelectMany(x => x.ToList());
            }

            if (cargos.Any())
            {
                gruposUf = gruposUf?
                    .GroupBy(x => Convert.ToInt32(x.Field<string>(Analisador.CD_CARGO_PERGUNTA)))
                    .Where(x => cargos.Any(y => (int)y == x.Key))
                    .SelectMany(x => x.ToList());
            }

            return gruposUf?.Sum(x => Convert.ToInt32(x.Field<string>(Analisador.QT_VOTOS))) ?? 0;
        }
        #endregion

        #region Privados
        /// <summary>
        /// Obter agrupamento por coluna NR_VOTAVEL
        /// </summary>
        /// <param name="unidadeFederacao">Unidade da Federação</param>
        /// <returns>O agrupamento por número do votável</returns>
        private IEnumerable<DataRow>? ObterLinhasPorUf(Enumeradores.UnidadeFederacaoEnum? unidadeFederacao)
        {
            if (!unidadeFederacao.HasValue)
            {
                return this.RelacaoTabelas.SelectMany(x => x.AsEnumerable());
            }

            return this.RelacaoTabelasUfLookup[Enumeradores.ObterDescricao(unidadeFederacao)].FirstOrDefault()?.AsEnumerable();
        }
        #endregion
        #endregion
    }
}
