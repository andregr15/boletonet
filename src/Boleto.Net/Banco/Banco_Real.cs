using System;
using System.ComponentModel;
using System.Threading;
using System.Web.UI;
using BoletoNet;
using BoletoNet.Util;
using System.Text;

[assembly: WebResource("BoletoNet.Imagens.356.jpg", "image/jpg")]

namespace BoletoNet
{

    /// <author>  
    /// Eduardo Frare
    /// </author>    
    internal class Banco_Real : AbstractBanco, IBanco
    {
        private string _dacNossoNumero = string.Empty;
        private int _dacContaCorrente = 0;
        private int _dacBoleto = 0;

        /// <summary>
        /// Classe responsavel em criar os campos do Banco Banco_Real.
        /// </summary>
        internal Banco_Real()
        {
            this.Codigo = 356;
            this.Digito = "5";
            this.Nome = "Banco Real";
        }

        public override void ValidaBoleto(Boleto boleto)
        {
            if (boleto.Carteira != "57")
                throw new NotImplementedException("Carteira nao implementada. Carteiras implementadas 57.");

            //Formata o tamanho do numero da Agencia
            if (boleto.Cedente.ContaBancaria.Agencia.Length < 4)
                throw new Exception("numero da Agencia inválido");

            //Formata o tamanho do numero da conta corrente
            if (boleto.Cedente.ContaBancaria.Conta.Length < 7)
                boleto.Cedente.ContaBancaria.Conta = Utils.FormatCode(boleto.Cedente.ContaBancaria.Conta, 7);

            //Formata o tamanho do numero de nosso numero
            if (boleto.NossoNumero.Length < 13)
                boleto.NossoNumero = Utils.FormatCode(boleto.NossoNumero, 13);

            // Calcula o DAC do Nosso numero
            _dacNossoNumero = CalcularDigitoNossoNumero(boleto);

            // Calcula o DAC da Conta Corrente
            _dacContaCorrente = Mod10(boleto.Cedente.ContaBancaria.Agencia + boleto.Cedente.ContaBancaria.Conta);
            boleto.Cedente.ContaBancaria.DigitoConta = _dacContaCorrente.ToString();

            //Atribui o nome do banco ao local de pagamento
            boleto.LocalPagamento += Nome;

            //Verifica se o nosso numero e valido
            if (Utils.ToInt64(boleto.NossoNumero) == 0)
                throw new NotImplementedException("Nosso numero inválido");

            //Verifica se data do processamento e valida
			//if (boleto.DataProcessamento.ToString("dd/MM/yyyy") == "01/01/0001")
			if (boleto.DataProcessamento == DateTime.MinValue) // diegomodolo (diego.ribeiro@nectarnet.com.br)
                boleto.DataProcessamento = DateTime.Now;

            //Verifica se data do documento e valida
			//if (boleto.DataDocumento.ToString("dd/MM/yyyy") == "01/01/0001")
			if (boleto.DataDocumento == DateTime.MinValue) // diegomodolo (diego.ribeiro@nectarnet.com.br)
                boleto.DataDocumento = DateTime.Now;

            FormataCodigoBarra(boleto);
            FormataLinhaDigitavel(boleto);
            FormataNossoNumero(boleto);
        }

        public override void FormataNossoNumero(Boleto boleto)
        {
            //throw new NotImplementedException("Função do fomata nosso numero nao implementada.");
        }

        public override void FormataNumeroDocumento(Boleto boleto)
        {
            throw new NotImplementedException("Função do fomata numero do documento nao implementada.");
        }

        ///<summary>
        /// Campo Livre
        ///    20 a 23 - 4 - Agencia Cedente
        ///    24 a 30 - 7 - Conta
        ///    31 a 31 - 1 - Digito da Conta
        ///    32 a 44 - 13 - numero do Nosso numero
        ///</summary>
        public string CampoLivre(Boleto boleto)
        {
            return boleto.Cedente.ContaBancaria.Agencia + boleto.Cedente.ContaBancaria.Conta + Mod10(boleto.NossoNumero + boleto.Cedente.ContaBancaria.Agencia + boleto.Cedente.ContaBancaria.Conta) + boleto.NossoNumero;
        }


        /// <summary>
        /// Calcula o digito do Nosso Numero
        /// </summary>
        public string CalcularDigitoNossoNumero(Boleto boleto)
        {
            int dig;

            dig = Mod10(boleto.NossoNumero.Substring(0, 9) + boleto.Cedente.ContaBancaria.Agencia + boleto.Cedente.ContaBancaria.Conta);

            return dig.ToString();

            //throw new NotImplementedException("Função do calcular digito do nosso numero nao implementada.");
        }

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
        ///          composto pelo valor nominal do documento com supressao de Zeros e sem edicao.
        ///          Quando se tratar de valor zerado, a representacao devera ser 000 (tres Zeros).
        /// </summary>
        public override void FormataLinhaDigitavel(Boleto boleto)
        {

            //AAABC.CCCCX DDDDD.DDDDDY EEEEE.EEEEEZ K VVVVVVVVVVVVVV

            string LD = string.Empty; //Linha Digitavel

            #region Campo 1

            //Campo 1
            string AAA = Utils.FormatCode(boleto.Banco.Codigo.ToString(), 3);
            string B = boleto.Moeda.ToString();
            string CCCCC = CampoLivre(boleto).Substring(0, 5);
            string X = Mod10(AAA + B + CCCCC).ToString();

            LD = string.Format("{0}{1}{2}.", AAA, B, CCCCC.Substring(0, 1));
            LD += string.Format("{0}{1} ", CCCCC.Substring(1, 4), X);

            #endregion Campo 1

            #region Campo 2
            string DDDDDD = CampoLivre(boleto).Substring(5, 10);
            string Y = Mod10(DDDDDD).ToString();

            LD += string.Format("{0}.", DDDDDD.Substring(0, 5));
            LD += string.Format("{0}{1} ", DDDDDD.Substring(5, 5), Y);
            #endregion Campo 2


            #region Campo 3
            string EEEEE = CampoLivre(boleto).Substring(15, 10);
            string Z = Mod10(EEEEE).ToString();

            LD += string.Format("{0}.", EEEEE.Substring(0, 5));
            LD += string.Format("{0}{1} ", EEEEE.Substring(5, 5), Z);

            #endregion Campo 3

            #region Campo 4

            string K = _dacBoleto.ToString();

            LD += string.Format(" {0} ", K);

            #endregion Campo 4

            #region Campo 5
            string VVVVVVVVVVVVVV;
            if (boleto.ValorBoleto != 0)
            {
                VVVVVVVVVVVVVV = boleto.ValorBoleto.ToString("f").Replace(",", "").Replace(".", "");
                VVVVVVVVVVVVVV = FatorVencimento(boleto) + Utils.FormatCode(VVVVVVVVVVVVVV, 10);
            }
            else
                VVVVVVVVVVVVVV = "000";

            LD += VVVVVVVVVVVVVV;
            #endregion Campo 5


            boleto.CodigoBarra.LinhaDigitavel = LD;

        }

        /// <summary>
        ///	O codigo de barra para Cobranca contem 44 posicoes dispostas da seguinte forma:
        ///    01 a 03 - 3 - Identificacao  do  Banco
        ///    04 a 04 - 1 - Codigo da Moeda
        ///    05 a 05 - 1 - Digito verificador do Codigo de Barras
        ///    06 a 19 - 14 - Valor
        ///    20 a 44 - 25 - Campo Livre
        /// </summary>
        public override void FormataCodigoBarra(Boleto boleto)
        {
            string valorBoleto = boleto.ValorBoleto.ToString("f").Replace(",", "").Replace(".", "");
            valorBoleto = Utils.FormatCode(valorBoleto, 14);

            boleto.CodigoBarra.Codigo = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
                    Codigo,
                    boleto.Moeda,
                    FatorVencimento(boleto),
                    valorBoleto.Substring(4, 10),
                    boleto.Cedente.ContaBancaria.Agencia,
                    boleto.Cedente.ContaBancaria.Conta,
                    Mod10(boleto.NossoNumero + boleto.Cedente.ContaBancaria.Agencia + boleto.Cedente.ContaBancaria.Conta),
                    boleto.NossoNumero
            );

            _dacBoleto = Mod11(Strings.Left(boleto.CodigoBarra.Codigo, 4) + Strings.Right(boleto.CodigoBarra.Codigo, 39), 9, 0);

            boleto.CodigoBarra.Codigo = Strings.Left(boleto.CodigoBarra.Codigo, 4) + _dacBoleto + Strings.Right(boleto.CodigoBarra.Codigo, 39);

        }

        public override string GerarHeaderRemessa(string numeroConvenio, Cedente cedente, TipoArquivo tipoArquivo, int numeroArquivoRemessa, Boleto boletos)
        {
            throw new NotImplementedException("Função nao implementada.");
        }

        public override string GerarDetalheRemessa(Boleto boleto, int numeroRegistro, TipoArquivo tipoArquivo)
        {
            throw new NotImplementedException("Função nao implementada.");
        }

        public override string GerarHeaderRemessa(string numeroConvenio, Cedente cedente, TipoArquivo tipoArquivo, int numeroArquivoRemessa)
        {
            throw new NotImplementedException("Função nao implementada.");
        }

        public override string GerarTrailerRemessa(int numeroRegistro, TipoArquivo tipoArquivo, Cedente cedente, decimal vltitulostotal)
        {
            throw new NotImplementedException("Função nao implementada.");
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
