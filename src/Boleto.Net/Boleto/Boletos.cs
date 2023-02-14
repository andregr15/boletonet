using System;
using System.Collections.Generic;

namespace BoletoNet
{
    public class Boletos : List<Boleto>
    {

        # region Variaveis

	    # endregion

        # region Propriedades

	    public Banco Banco { get; set; }

	    public ContaBancaria ContaBancaria { get; set; }

	    public Cedente Cedente { get; set; }

	    # endregion

        # region Metodos

        /// <summary>
        /// Verifica se ja existe o arquivo relativo a remessa, caso nao exista é criado um arquivo ".rem".
        /// 
        /// O padrao dos arquivos de Remessa e Retorno, obedece as regras estabelecidas pelo C.N.A.B. (Centro Nacional
        /// de Automação Bancaria) e devera ser gravado contendo:
        /// Registro Header : Primeiro registro do Arquivo contendo a identificacao da empresa
        /// Registro Detalhe : Registro contendo as informacoes de Pagamentos :
        /// - Inclusao de compromissos
        /// - alteracao de Compromissos
        /// - Pagamentos Efetuados
        /// - Bloqueios / Desbloqueios
        /// Registro Trailer : último registro indicando finalização do Arquivo
        /// Caracteres obrigatorios = 0D 0A (Final de Registro) 0D 0A 1A (Final de Arquivo)
        /// </summary>

        private new void Add(Boleto item)
        {
            if (item.Banco == null)
                throw new Exception("Boleto nao possui Banco.");

            if (item.ContaBancaria == null)
                throw new Exception("Boleto nao possui conta bancária.");

            if (item.Cedente == null)
                throw new Exception("Boleto nao possui cedente.");

            item.Valida();
            this.Add(item);
        }

        # endregion

    }
}
