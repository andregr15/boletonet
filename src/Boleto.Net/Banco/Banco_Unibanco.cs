using System;
using System.Collections.Generic;
using System.Web.UI;
using BoletoNet;
using BoletoNet.Util;

[assembly: WebResource("BoletoNet.Imagens.409.jpg", "image/jpg")]

namespace BoletoNet
{ 
    /// <author>  
    /// Marlon Oliveira (marlonoliveira@nextconsultoria.com.br)
    /// </author>    
    internal class Banco_Unibanco : AbstractBanco, IBanco
    {
        private string _dacNossoNumero = string.Empty;
        private int _dacBoleto = 0;

        /// <summary>
        /// Classe responsavel em criar os campos do banco Unibanco.
        /// </summary>
        internal Banco_Unibanco()
        {
            this.Codigo = 409;
            this.Digito = "2";
            this.Nome = "Unibanco";
        }
    
        #region IBanco Members

        /// <summary>
        /// A linha digitavel sera composta por cinco campos:
        ///      1o campo
        ///          composto pelo codigo de Banco, codigo da moeda, as cinco primeiras posicoes do campo 
        ///          livre e o Digito verificador deste campo;
        ///      2o campo
        ///          composto pelas posicoes 6a a 15a do campo livre e o Digito verificador deste campo;
        ///      3o campo
        ///          composto pelas posicoes 16a a 25a do campo livre e o Digito verificador deste campo;
        ///      4o campo
        ///          composto pelo Digito verificador do codigo de barras, ou seja, a 5a posicao do codigo de 
        ///          barras;
        ///      5o campo
        ///          Composto pelo fator de vencimento com 4(quatro) caracteres e o valor do documento com 10(dez) caracteres, sem separadores e sem edicao.
        /// 
        /// </summary>
        public override void FormataLinhaDigitavel(Boleto boleto)
        {
            //BBBMC.CCCCD1 CCCCC.CCCCCD2 CCCCC.CCCCCD3 D4 FFFFVVVVVVVVVV

            #region Campo 1

                string Grupo1 = string.Empty;

                string BBB = boleto.CodigoBarra.Codigo.Substring(0, 3);
                string M = boleto.CodigoBarra.Codigo.Substring(3, 1);
                string CCCCC = boleto.CodigoBarra.Codigo.Substring(19, 5);
                string D1 = Banco_Unibanco.Mod10(BBB + M + CCCCC).ToString() ;

                Grupo1 = string.Format("{0}{1}{2}.{3}{4} ", BBB, M, CCCCC.Substring(0, 1), CCCCC.Substring(1, 4), D1);


            #endregion Campo 1

            #region Campo 2

                string Grupo2 = string.Empty;

                string CCCCCCCCCC2 = boleto.CodigoBarra.Codigo.Substring(24, 10);
                string D2 = Banco_Unibanco.Mod10(CCCCCCCCCC2).ToString();

                Grupo2 = string.Format("{0}.{1}{2} ", CCCCCCCCCC2.Substring(0, 5), CCCCCCCCCC2.Substring(5, 5), D2);

            #endregion Campo 2

            #region Campo 3

                string Grupo3 = string.Empty;

                string CCCCCCCCCC3 = boleto.CodigoBarra.Codigo.Substring(34, 10);
                string D3 = Banco_Unibanco.Mod10(CCCCCCCCCC3).ToString();

                Grupo3 = string.Format("{0}.{1}{2} ", CCCCCCCCCC3.Substring(0, 5), CCCCCCCCCC3.Substring(5, 5), D3);


            #endregion Campo 3

            #region Campo 4

                string Grupo4 = string.Empty;

                string D4 = _dacBoleto.ToString();

                Grupo4 = string.Format("{0} ", D4);

            #endregion Campo 4

            #region Campo 5

                string Grupo5 = string.Empty;

                long FFFF = FatorVencimento(boleto) ;

                string VVVVVVVVVV = boleto.ValorBoleto.ToString("f").Replace(",", "").Replace(".", "");
                VVVVVVVVVV = Utils.FormatCode(VVVVVVVVVV, 10);

                if (Utils.ToInt64(VVVVVVVVVV) == 0)
                    VVVVVVVVVV = "000";

                Grupo5 = string.Format("{0}{1}", FFFF, VVVVVVVVVV);

            #endregion Campo 5

            boleto.CodigoBarra.LinhaDigitavel = Grupo1 + Grupo2 + Grupo3 + Grupo4 + Grupo5;

        }

        /// <summary>
        /// 
        ///   *******
        /// 
        ///	O codigo de barra para Cobranca contem 44 posicoes dispostas da seguinte forma:
        ///    01 a 03 -  3 - 409 fixo - Codigo do banco
        ///    04 a 04 -  1 - 9 fixo - Codigo da moeda (R$)
        ///    05 a 05 -  1 - Digito verificador do codigo de barras
        ///    06 a 09 -  4 - Fator de vencimento
        ///    10 a 19 - 10 - Valor
        ///    20 a 44 - 25 - Campo livre
        /// 
        ///   *******
        /// 
        /// </summary>
        public override void FormataCodigoBarra(Boleto boleto)
        {
            string valorBoleto = boleto.ValorBoleto.ToString("f").Replace(",", "").Replace(".", "");
            valorBoleto = Utils.FormatCode(valorBoleto, 10);

            boleto.CodigoBarra.Codigo = string.Format("{0}{1}{2}{3}{4}", Codigo.ToString(), boleto.Moeda,
                    FatorVencimento(boleto), valorBoleto, FormataCampoLivre(boleto));

            _dacBoleto = Banco_Unibanco.Mod11(boleto.CodigoBarra.Codigo);

            boleto.CodigoBarra.Codigo = Strings.Left(boleto.CodigoBarra.Codigo, 4) + _dacBoleto + Strings.Right(boleto.CodigoBarra.Codigo, 39);
        }

        ///<summary>
        /// Campo Livre
        ///    20 a 20 -  1 - 5 fixo - tipo de Cobranca (CVT Cobranca sem registro - 7744-5)
        ///    21 a 26 -  6 - Codigo do cedente
        ///    27 a 27 -  1 - Digito verificador do codigo do cedente
        ///    28 a 29 -  2 - 00 fixo - vago
        ///    30 a 43 - 14 - Nosso numero
        ///    44 a 44	- 1 - Digito do nosso numero
        ///
        /// PS: Cálculo do Digito do codigo de barras: usar rotina de módulo 11
        ///</summary>
        public string FormataCampoLivre(Boleto boleto)
        {
            string codigoCedente = boleto.Cedente.Codigo.ToString();
            codigoCedente = Utils.FormatCode(codigoCedente, 6);

            string FormataCampoLivre = string.Format("{0}{1}{2}{3}{4}{5}",
                "5", codigoCedente, Banco_Unibanco.Mod11(codigoCedente),
                "00", boleto.NossoNumero, Banco_Unibanco.Mod11(boleto.NossoNumero, true));

            return FormataCampoLivre;
        }

        public override void FormataNumeroDocumento(Boleto boleto)
        {
            throw new NotImplementedException("Função ainda nao implementada.");
        }

        public override void FormataNossoNumero(Boleto boleto)
        {
            boleto.NossoNumero = string.Format("{0}-{1}", boleto.NossoNumero, Banco_Unibanco.Mod11(boleto.NossoNumero,true));
        }

        public override string GerarHeaderRemessa(string numeroConvenio, Cedente cedente, TipoArquivo tipoArquivo, int numeroArquivoRemessa, Boleto boletos)
        {
            throw new NotImplementedException("Função nao implementada.");
        }
        public override void ValidaBoleto(Boleto boleto)
        {
            //Verifica se o nosso numero e valido
            //Verifica se o tamanho para o NossoNumero sao 12 Digitos
            if (Utils.ToString(boleto.NossoNumero) == string.Empty)
                throw new NotImplementedException("Nosso numero inválido");
            else if (Convert.ToInt32(boleto.NossoNumero).ToString().Length > 14)
                throw new NotImplementedException("A quantidade de Digitos do nosso numero sao 14 numeros.");
            else if (Convert.ToInt32(boleto.NossoNumero).ToString().Length < 14)
                boleto.NossoNumero = Utils.FormatCode(boleto.NossoNumero, 14);

            //Verificar se a Agencia esta correta
            if (boleto.Cedente.ContaBancaria.Agencia.Length > 4)
                throw new NotImplementedException("A quantidade de Digitos da Agencia " + boleto.Cedente.ContaBancaria.Agencia + ", sao de 4 numeros.");
            else if (boleto.Cedente.ContaBancaria.Agencia.Length < 4)
                boleto.Cedente.ContaBancaria.Agencia = Utils.FormatCode(boleto.Cedente.ContaBancaria.Agencia, 4);

           //Verificar se a Conta esta correta
            if (boleto.Cedente.ContaBancaria.Conta.Length > 6)
                throw new NotImplementedException("A quantidade de Digitos da Conta " + boleto.Cedente.ContaBancaria.Conta + ", sao de 6 numeros.");
            else if (boleto.Cedente.ContaBancaria.Conta.Length < 6)
                boleto.Cedente.ContaBancaria.Conta = Utils.FormatCode(boleto.Cedente.ContaBancaria.Conta, 6);

            //Atribui o nome do banco ao local de pagamento
            boleto.LocalPagamento += Nome;

            //Verifica se data do processamento e valida
			//if (boleto.DataProcessamento.ToString("dd/MM/yyyy") == "01/01/0001")
			if (boleto.DataProcessamento == DateTime.MinValue) // diegomodolo (diego.ribeiro@nectarnet.com.br)
                boleto.DataProcessamento = DateTime.Now;

            //Verifica se data do documento e valida
			//if (boleto.DataDocumento.ToString("dd/MM/yyyy") == "01/01/0001")
			if (boleto.DataDocumento == DateTime.MinValue) // diegomodolo (diego.ribeiro@nectarnet.com.br)
                boleto.DataDocumento = DateTime.Now;

            if (boleto.Carteira != "20")
                throw new NotImplementedException("Carteira nao implementada: " + boleto.Carteira + ". Utilize a carteira 20.");

            //Boleto.QuantidadeMoeda = 0;

            FormataCodigoBarra(boleto);
            FormataLinhaDigitavel(boleto);
            FormataNossoNumero(boleto);
        }
        #endregion IBanco Members

        internal new static int Mod10(string seq)
        {
            int Digito, Soma = 0, Peso = 2, res;

            for (int i = seq.Length; i > 0; i--)
            {
                res = (Convert.ToInt32(Strings.Mid(seq, i, 1)) * Peso);

                if (res > 9)
                    res = (res - 9);

                Soma += res;

                if (Peso == 2)
                    Peso = 1;
                else
                    Peso = Peso + 1;
            }

            Digito = ((10 - (Soma % 10)) % 10);

            return Digito;
        }

        internal new static int Mod11(string seq)
        {
            bool superDigito = false;

            return Mod11(seq, superDigito);
        }

        internal static int Mod11(string seq, bool superDigito)
        {
            int Digito, Soma = 0, Peso = 2;

            for (int i = seq.Length; i > 0; i--)
            {
                Soma += (Convert.ToInt32(Strings.Mid(seq, i, 1)) * Peso);

                if (Peso == 9)
                    Peso = 2;
                else
                    Peso = Peso + 1;
            }

            Digito = ((Soma * 10) % 11);

            if ((superDigito) && (Digito == 10))
                Digito = 0;
            else if ((Digito == 0) || (Digito == 10))
                Digito = 1;

            return Digito;
        }


        /// <summary>
        /// Efetua as Validacoes dentro da classe Boleto, para garantir a geracao da remessa
        /// </summary>
        public override bool ValidarRemessa(TipoArquivo tipoArquivo, string numeroConvenio, IBanco banco, Cedente cedente, Boletos boletos, int numeroArquivoRemessa, out string mensagem)
        {
            bool vRetorno = true;
            string vMsg = string.Empty;
            ////IMPLEMENTACAO PENDENTE...
            mensagem = vMsg;
            return vRetorno;
        }

    }
}
