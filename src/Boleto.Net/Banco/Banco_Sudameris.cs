using System;
using System.Web.UI;
using BoletoNet.Util;

[assembly: WebResource("BoletoNet.Imagens.347.jpg", "image/jpg")]

namespace BoletoNet
{
    /// <author>  
    /// Eduardo Frare
    /// Stiven 
    /// Diogo(Allmatech)
    /// Miamoto(Allmatech)
    /// Rodrigo(Allmatech)
    /// </author>    

    ///<summary>
    /// Classe referente ao Banco Banco_Sudameris
    ///</summary>
    internal class Banco_Sudameris : AbstractBanco, IBanco
    {
        private int _dacDigitaoCobranca = 0;
        private int _dacBoleto = 0;

        internal Banco_Sudameris()
        {
            this.Codigo = 347;
            this.Digito = "6";
            this.Nome = "Banco_Sudameris";
        }

        #region IBanco Members

        public override void ValidaBoleto(Boleto boleto)
        {
            //Verifica se o nosso numero e valido
            if (Utils.ToInt32(boleto.NossoNumero) == 0)
                throw new NotImplementedException("Nosso numero inválido");

            //O numero da conta corrente sao 7 Digitos
            if (boleto.Cedente.ContaBancaria.Conta.Length != 7)
                throw new Exception("O numero da conta corrente do cedente sao 7 numeros.");

            //Verifica se o tamanho para o NossoNumero
            // 7 para Cobranca registrada
            // 13 para Cobranca sem registro
            boleto.NossoNumero = Utils.FormatCode(boleto.NossoNumero, 13);

            // Calcula o digitão de Cobranca DAC (Nosso numero/Agencia/Conta Corrente)
            _dacDigitaoCobranca = Mod10(boleto.NossoNumero + boleto.Cedente.ContaBancaria.Agencia + boleto.Cedente.ContaBancaria.Conta);

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

            FormataCodigoBarra(boleto);
            FormataLinhaDigitavel(boleto);
            FormataNumeroDocumento(boleto);
        }

        public override void FormataLinhaDigitavel(Boleto boleto)
        {
            #region Definições
            /* AAABC.CCCDDX.DDDDD.DEFFFY.FGGGG.GGHHHZ.K.UUUUVVVVVVVVVV
              * ------------------------------------------------------
              * Campo 1
              * AAABC.CCCDX
              * AAA - Codigo do Banco
              * B   - Moeda
              * CCCC - Agencia
              * D  - 1 primeiro numero da Conta Corrente
              * X   - DAC Campo 1 (AAABC.CCDD) Mod10
              * 
              * Campo 2
              * DDDDD.DEFFFY
              * DDDDD.D - Restante do Conta Corrente
              * E       - DAC (Nosso numero/Agencia/Conta)
              * FFF     - Três primeiros do Nosso numero
              * Y       - DAC Campo 2 (DDDDD.DEFFF) Mod10
              * 
              * Campo 3
              * FFFFF.FFFFZ
              * FFFFFFFFF- Restante Nosso numero
              * Z       - DAC Campo 3
              * 
              * Campo 4
              * K       - DAC Codigo de Barras
              * 
              * Campo 5
              * UUUUVVVVVVVVVV
              * UUUU       - Fator Vencimento
              * VVVVVVVVVV - Valor do Titulo 
              */
            #endregion Definições

            string numeroDocumento = Utils.FormatCode(boleto.NumeroDocumento.ToString(), 7);
            string codigoCedente = Utils.FormatCode(boleto.Cedente.Codigo.ToString(), 5);

            string AAA = Utils.FormatCode(Codigo.ToString(), 3);
            string B = boleto.Moeda.ToString();
            string CCCC = boleto.Cedente.ContaBancaria.Agencia;
            string D = boleto.Cedente.ContaBancaria.Conta.Substring(0, 1);
            string X = Mod10(AAA + B + CCCC + D).ToString();

            string DDDDDD = boleto.Cedente.ContaBancaria.Conta.Substring(1, 6);

            string K = string.Format(" {0} ", _dacDigitaoCobranca);

            string UUUU = FatorVencimento(boleto).ToString();
            string VVVVVVVVVV = boleto.ValorBoleto.ToString("f").Replace(",", "").Replace(".", "");

            string C1 = string.Empty;
            string C2 = string.Empty;
            string C3 = string.Empty;
            string C4 = string.Empty;

            #region AAABC.CCDDX

            C1 = string.Format("{0}{1}{2}.", AAA, B, CCCC.Substring(0, 1));
            C1 += string.Format("{0}{1}{2} ", CCCC.Substring(1, 3), D, X);

            #endregion AAABC.CCDDX

            #region UUUUVVVVVVVVVV

            VVVVVVVVVV = Utils.FormatCode(VVVVVVVVVV, 10);
            C4 = UUUU + VVVVVVVVVV;

            #endregion UUUUVVVVVVVVVV

            #region DDDDD.DEFFFY

            string E = _dacDigitaoCobranca.ToString();
            string FFF = boleto.NossoNumero.Substring(0, 3);
            string Y = Mod10(DDDDDD + E + FFF).ToString();

            C2 = string.Format("{0}.", DDDDDD.Substring(0, 5));
            C2 += string.Format("{0}{1}{2}{3} ", DDDDDD.Substring(5, 1), E, FFF, Y);

            #endregion DDDDD.DEFFFY

            #region FFFFF.FFFFFZ

            string FFFFFFFFF = boleto.NossoNumero.Substring(3, 10);
            string Z = Mod10(FFFFFFFFF).ToString();

            C3 = string.Format("{0}.{1}{2}", FFFFFFFFF.Substring(0, 5), FFFFFFFFF.Substring(5, 5), Z);

            #endregion FGGGG.GGHHHZ

            boleto.CodigoBarra.LinhaDigitavel = C1 + C2 + C3 + K + C4;
        }

        public void FormataNossoNumero()
        {
        }

        public override void FormataNumeroDocumento(Boleto boleto)
        {
            boleto.NumeroDocumento = Utils.FormatCode(boleto.NossoNumero, 13);
        }

        public override void FormataCodigoBarra(Boleto boleto)
        {
            // Codigo de Barras
            //Codigo do Banco/Moeda/ DAC /Fator Vencimento/Valor/Agencia/Conta + Digitão/Nosso numero

            string valorBoleto = boleto.ValorBoleto.ToString("f").Replace(",", "").Replace(".", "");
            valorBoleto = Utils.FormatCode(valorBoleto, 10);

            boleto.CodigoBarra.Codigo =
                    string.Format("{0}{1}{2}{3}{4}{5}{6}{7}", Codigo, boleto.Moeda,
                                  FatorVencimento(boleto), valorBoleto, boleto.Cedente.ContaBancaria.Agencia,
                                  boleto.Cedente.ContaBancaria.Conta, _dacDigitaoCobranca, boleto.NossoNumero);


            _dacBoleto = Mod11(boleto.CodigoBarra.Codigo, 9, 0);

            boleto.CodigoBarra.Codigo = Strings.Left(boleto.CodigoBarra.Codigo, 4) + _dacBoleto + Strings.Right(boleto.CodigoBarra.Codigo, 39);
        }

        public override string GerarHeaderRemessa(string numeroConvenio, Cedente cedente, TipoArquivo tipoArquivo, int numeroArquivoRemessa, Boleto boletos)
        {
            throw new NotImplementedException("Função nao implementada.");
        }
        #endregion IBanco Members


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
