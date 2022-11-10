//-----------------------------------------------------------------------
// <copyright file="Enumeradores.cs" company="DevConn Software House">
//     Copyright (c) DevConn Software House and contributors. All rights reserved.
//     Licensed under the MIT license.
// </copyright>
//-----------------------------------------------------------------------

namespace ResultadosEleicoes.Utils
{
    using System;
    using System.ComponentModel;
    using System.Linq;

    /// <summary>
    /// Classe Enumeradores
    /// </summary>
    public static class Enumeradores
    {
        #region Enumeradores
        /// <summary>
        /// Enumerador UnidadeFederacaoEnum
        /// </summary>
        public enum UnidadeFederacaoEnum
        {
            /// <summary>
            /// AC - Acre
            /// </summary>
            [Description("AC")]
            AC = 0,

            /// <summary>
            /// AL - Alagoas
            /// </summary>
            [Description("AL")]
            AL = 1,

            /// <summary>
            /// AP - Amapá
            /// </summary>
            [Description("AP")]
            AP = 2,

            /// <summary>
            /// AM - Amazonas
            /// </summary>
            [Description("AM")]
            AM = 3,

            /// <summary>
            /// BA - Bahia
            /// </summary>
            [Description("BA")]
            BA = 4,

            /// <summary>
            /// CE - Ceará
            /// </summary>
            [Description("CE")]
            CE = 5,

            /// <summary>
            /// DF - Distrito Federal
            /// </summary>
            [Description("DF")]
            DF = 6,

            /// <summary>
            /// GO - Goiás
            /// </summary>
            [Description("GO")]
            GO = 7,

            /// <summary>
            /// ES - Espírito Santo
            /// </summary>
            [Description("ES")]
            ES = 8,

            /// <summary>
            /// MA - Maranhão
            /// </summary>
            [Description("MA")]
            MA = 9,

            /// <summary>
            /// MT - Mato Grosso
            /// </summary>
            [Description("MT")]
            MT = 10,

            /// <summary>
            /// MS - Mato Grosso do Sul
            /// </summary>
            [Description("MS")]
            MS = 11,

            /// <summary>
            /// MG - Minas Gerais
            /// </summary>
            [Description("MG")]
            MG = 12,

            /// <summary>
            /// PA - Pará
            /// </summary>
            [Description("PA")]
            PA = 13,

            /// <summary>
            /// PB - Paraíba
            /// </summary>
            [Description("PB")]
            PB = 14,

            /// <summary>
            /// PR - Paraná
            /// </summary>
            [Description("PR")]
            PR = 15,

            /// <summary>
            /// PE - Pernambuco
            /// </summary>
            [Description("PE")]
            PE = 16,

            /// <summary>
            /// PI - Piauí
            /// </summary>
            [Description("PI")]
            PI = 17,

            /// <summary>
            /// RJ - Rio de Janeiro
            /// </summary>
            [Description("RJ")]
            RJ = 18,

            /// <summary>
            /// RN - Rio Grande do Norte
            /// </summary>
            [Description("RN")]
            RN = 19,

            /// <summary>
            /// RS - Rio Grande do Sul
            /// </summary>
            [Description("RS")]
            RS = 20,

            /// <summary>
            /// RO - Rondônia
            /// </summary>
            [Description("RO")]
            RO = 21,

            /// <summary>
            /// RR - Roraima
            /// </summary>
            [Description("RR")]
            RR = 22,

            /// <summary>
            /// SP - São Paulo
            /// </summary>
            [Description("SP")]
            SP = 23,

            /// <summary>
            /// SC - Santa Catarina
            /// </summary>
            [Description("SC")]
            SC = 24,

            /// <summary>
            /// SE - Sergipe
            /// </summary>
            [Description("SE")]
            SE = 25,

            /// <summary>
            /// TO - Tocantins
            /// </summary>
            [Description("TO")]
            TO = 26
        }

        /// <summary>
        /// Enumerador CargoEnum
        /// </summary>
        public enum CargoEnum
        {
            /// <summary>
            /// Cargo - Presidente
            /// </summary>
            [Description("Presidente")]
            Presidente = 1,

            /// <summary>
            /// Cargo - Governador
            /// </summary>
            [Description("Governador")]
            Governador = 3
        }
        #endregion

        #region Métodos
        /// <summary>
        /// Obter a descrição de um enumerador
        /// </summary>
        /// <param name="enumerador">Enumerador a ser considerado</param>
        /// <returns>A descrição do enumerador ou seu nome, caso a descrição não exista</returns>
        public static string ObterDescricao(Enum enumerador)
        {
            // Validar
            if (enumerador == null)
            {
                return string.Empty;
            }

            // Obter descrição
            try
            {
                var descricaoAtributos = enumerador
                    .GetType()?
                    .GetField(enumerador.ToString())?
                    .GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .Cast<DescriptionAttribute>();
                if (descricaoAtributos?.Any() ?? false)
                {
                    return descricaoAtributos.FirstOrDefault()?.Description ?? string.Empty;
                }

                // Retorno
                return enumerador.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }
        #endregion
    }
}
