using BoletoNet.Excecoes;
using BoletoNet.Util;
using System;
using System.Globalization;
using System.Text;
using System.Web.UI;

[assembly: WebResource("BoletoNet.Imagens.341.jpg", "image/jpg")]
namespace BoletoNet
{
    /// <summary>
    /// Classe referente ao banco Ita�
    /// </summary>
    internal class Banco_Itau : AbstractBanco, IBanco
    {

        #region Vari�veis

        private int _dacBoleto = 0;
        private int _dacNossoNumero = 0;

        #endregion

        #region Construtores

        internal Banco_Itau()
        {
            try
            {
                this.Codigo = 341;
                this.Digito = "7";
                this.Nome = "Ita�";
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao instanciar objeto.", ex);
            }
        }

        #endregion

        #region M�todos de Inst�ncia

        /// <summary>
        /// Valida��es particulares do banco Ita�
        /// </summary>
        public override void ValidaBoleto(Boleto boleto)
        {
            try
            {
                //Carteiras v�lidas
                int[] cv = new int[] { 175, 176, 178, 109, 198, 107, 122, 142, 143, 196, 126, 131, 146, 150, 169, 121, 112, 104 };//MarcielTorres - adicionado a carteira 112
                bool valida = false;

                foreach (int c in cv)
                    if (Utils.ToString(boleto.Carteira) == Utils.ToString(c))
                        valida = true;

                if (!valida)
                {
                    StringBuilder carteirasImplementadas = new StringBuilder(100);

                    carteirasImplementadas.Append(". Carteiras implementadas: ");
                    foreach (int c in cv)
                    {
                        carteirasImplementadas.AppendFormat(" {0}", c);
                    }
                    throw new NotImplementedException("Carteira n�o implementada: " + boleto.Carteira + carteirasImplementadas.ToString());
                }

                //Verifica se o NossoNumero � um inteiro v�lido.
                int intNossoNumero;
                if (!Int32.TryParse(boleto.NossoNumero, out intNossoNumero))
                    throw new NotImplementedException("Nosso n�mero para a carteira " + boleto.Carteira + " inv�lido.");

                //Verifica se o tamanho para o NossoNumero s�o 8 d�gitos
                if (boleto.NossoNumero.Length > 8)
                    throw new NotImplementedException("A quantidade de d�gitos do nosso n�mero para a carteira "
                        + boleto.Carteira + ", s�o 8 n�meros.");

                if (boleto.NossoNumero.Length < 8)
                    boleto.NossoNumero = Utils.FormatCode(boleto.NossoNumero, 8);

                //� obrigat�rio o preenchimento do n�mero do documento
                if (boleto.Carteira == "106" || boleto.Carteira == "107" || boleto.Carteira == "122" || boleto.Carteira == "142" || boleto.Carteira == "143" || boleto.Carteira == "195" || boleto.Carteira == "196" || boleto.Carteira == "198")
                {
                    if (Utils.ToInt32(boleto.NumeroDocumento) == 0)
                        throw new NotImplementedException("O n�mero do documento n�o pode ser igual a zero.");
                }

                //Formato o n�mero do documento 
                boleto.NumeroDocumento = boleto.NumeroDocumento.Replace("/", string.Empty);
                if (Utils.ToInt32(boleto.NumeroDocumento) > 0)
                {
                    boleto.NumeroDocumento = Utils.ToInt32(boleto.NumeroDocumento).ToString();
                    boleto.NumeroDocumento = Utils.FormatCode(boleto.NumeroDocumento, 7);
                    
                }
                    


                // Calcula o DAC da Conta Corrente
                boleto.Cedente.ContaBancaria.DigitoConta = Mod10(boleto.Cedente.ContaBancaria.Agencia + boleto.Cedente.ContaBancaria.Conta).ToString();

                // Calcula o DAC do Nosso N�mero a maioria das carteiras
                // agencia/conta/carteira/nosso numero
                if (boleto.Carteira == "104" || boleto.Carteira == "112")
                    _dacNossoNumero = Mod10(boleto.Cedente.ContaBancaria.Agencia + boleto.Cedente.ContaBancaria.Conta + boleto.Cedente.ContaBancaria.DigitoConta + boleto.Carteira + boleto.NossoNumero);
                else if (boleto.Carteira != "126" && boleto.Carteira != "131"
                    && boleto.Carteira != "146" && boleto.Carteira != "150"
                    && boleto.Carteira != "168")
                    _dacNossoNumero = Mod10(boleto.Cedente.ContaBancaria.Agencia + boleto.Cedente.ContaBancaria.Conta + boleto.Carteira + boleto.NossoNumero);
                else
                    // Excess�o 126 - 131 - 146 - 150 - 168
                    // carteira/nosso numero
                    _dacNossoNumero = Mod10(boleto.Carteira + boleto.NossoNumero);

                boleto.DigitoNossoNumero = _dacNossoNumero.ToString();

                //Atribui o nome do banco ao local de pagamento
                if (string.IsNullOrEmpty(boleto.LocalPagamento))
                    boleto.LocalPagamento = "PAG�VEL PREFERENCIALMENTE NAS AG�NCIAS DO ITA�";
                else if (boleto.LocalPagamento == "At� o vencimento, preferencialmente no ")
                    boleto.LocalPagamento += Nome;

                //Verifica se o nosso n�mero � v�lido
                if (Utils.ToInt64(boleto.NossoNumero) == 0)
                throw new NotImplementedException("Nosso n�mero inv�lido");

                //Verifica se data do processamento � valida
                //if (boleto.DataProcessamento.ToString("dd/MM/yyyy") == "01/01/0001")
                if (boleto.DataProcessamento == DateTime.MinValue) // diegomodolo (diego.ribeiro@nectarnet.com.br)
                    boleto.DataProcessamento = DateTime.Now;

                //Verifica se data do documento � valida
                //if (boleto.DataDocumento.ToString("dd/MM/yyyy") == "01/01/0001")
                if (boleto.DataDocumento == DateTime.MinValue) // diegomodolo (diego.ribeiro@nectarnet.com.br)
                    boleto.DataDocumento = DateTime.Now;

                boleto.FormataCampos();
            }
            catch (Exception e)
            {
                throw new Exception("Erro ao validar boletos.", e);
            }
        }

        # endregion

        # region M�todos de formata��o do boleto

        public override void FormataCodigoBarra(Boleto boleto)
        {
            try
            {
                // C�digo de Barras
                //banco & moeda & fator & valor & carteira & nossonumero & dac_nossonumero & agencia & conta & dac_conta & "000"

                string valorBoleto = boleto.ValorBoleto.ToString("f").Replace(",", "").Replace(".", "");
                valorBoleto = Utils.FormatCode(valorBoleto, 10);

                string numeroDocumento = Utils.FormatCode(boleto.NumeroDocumento.ToString(), 7);
                string codigoCedente = Utils.FormatCode(boleto.Cedente.Codigo.ToString(), 5);

                if (boleto.Carteira == "175" || boleto.Carteira == "176" || boleto.Carteira == "178" || boleto.Carteira == "109" || boleto.Carteira == "121" || boleto.Carteira == "112" || boleto.Carteira == "104")//MarcielTorres - adicionado a carteira 112
                {
                    boleto.CodigoBarra.Codigo =
                        string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}000", Codigo, boleto.Moeda,
                                      FatorVencimento(boleto), valorBoleto, boleto.Carteira,
                                      boleto.NossoNumero, _dacNossoNumero, boleto.Cedente.ContaBancaria.Agencia,//Flavio(fhlviana@hotmail.com) => Cedente.ContaBancaria.Agencia --> boleto.Cedente.ContaBancaria.Agencia
                                      Utils.FormatCode(boleto.Cedente.ContaBancaria.Conta, 5), boleto.Cedente.ContaBancaria.DigitoConta);//Flavio(fhlviana@hotmail.com) => Cedente.ContaBancaria.DigitoConta --> boleto.Cedente.ContaBancaria.DigitoConta
                }
                else if (boleto.Carteira == "198" || boleto.Carteira == "107"
                         || boleto.Carteira == "122" || boleto.Carteira == "142"
                         || boleto.Carteira == "143" || boleto.Carteira == "196")
                {
                    boleto.CodigoBarra.Codigo = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}0", Codigo, boleto.Moeda,
                        FatorVencimento(boleto), valorBoleto, boleto.Carteira,
                        boleto.NossoNumero, numeroDocumento, codigoCedente,
                        Mod10(boleto.Carteira + boleto.NossoNumero + numeroDocumento + codigoCedente));
                }

                _dacBoleto = Mod11(boleto.CodigoBarra.Codigo, 9, 0);

                boleto.CodigoBarra.Codigo = Strings.Left(boleto.CodigoBarra.Codigo, 4) + _dacBoleto + Strings.Right(boleto.CodigoBarra.Codigo, 39);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao formatar c�digo de barras.", ex);
            }
        }

        public override void FormataLinhaDigitavel(Boleto boleto)
        {
            try
            {
                string numeroDocumento = Utils.FormatCode(boleto.NumeroDocumento.ToString(), 7);
                string codigoCedente = Utils.FormatCode(boleto.Cedente.Codigo.ToString(), 5);
                string agencia = Utils.FormatCode(boleto.Cedente.ContaBancaria.Agencia, 4);

                string AAA = Utils.FormatCode(Codigo.ToString(), 3);
                string B = boleto.Moeda.ToString();
                string CCC = boleto.Carteira.ToString();
                string DD = boleto.NossoNumero.Substring(0, 2);
                string X = Mod10(AAA + B + CCC + DD).ToString();
                string LD = string.Empty; //Linha Digit�vel

                string DDDDDD = boleto.NossoNumero.Substring(2, 6);

                string K = string.Format(" {0} ", _dacBoleto);

                string UUUU = FatorVencimento(boleto).ToString();
                string VVVVVVVVVV = boleto.ValorBoleto.ToString("f").Replace(",", "").Replace(".", "");

                string C1 = string.Empty;
                string C2 = string.Empty;
                string C3 = string.Empty;
                string C5 = string.Empty;

                #region AAABC.CCDDX

                C1 = string.Format("{0}{1}{2}.", AAA, B, CCC.Substring(0, 1));
                C1 += string.Format("{0}{1}{2} ", CCC.Substring(1, 2), DD, X);

                #endregion AAABC.CCDDX

                #region UUUUVVVVVVVVVV

                VVVVVVVVVV = Utils.FormatCode(VVVVVVVVVV, 10);
                C5 = UUUU + VVVVVVVVVV;

                #endregion UUUUVVVVVVVVVV

                if (boleto.Carteira == "175" || boleto.Carteira == "176" || boleto.Carteira == "178" || boleto.Carteira == "109" || boleto.Carteira == "121" || boleto.Carteira == "112" || boleto.Carteira == "104")//MarcielTorres - adicionado a carteira 112
                {
                    #region Defini��es
                    /* AAABC.CCDDX.DDDDD.DEFFFY.FGGGG.GGHHHZ.K.UUUUVVVVVVVVVV
              * ------------------------------------------------------
              * Campo 1
              * AAABC.CCDDX
              * AAA - C�digo do Banco
              * B   - Moeda
              * CCC - Carteira
              * DD  - 2 primeiros n�meros Nosso N�mero
              * X   - DAC Campo 1 (AAABC.CCDD) Mod10
              * 
              * Campo 2
              * DDDDD.DEFFFY
              * DDDDD.D - Restante Nosso N�mero
              * E       - DAC (Ag�ncia/Conta/Carteira/Nosso N�mero)
              * FFF     - Tr�s primeiros da ag�ncia
              * Y       - DAC Campo 2 (DDDDD.DEFFF) Mod10
              * 
              * Campo 3
              * FGGGG.GGHHHZ
              * F       - Restante da Ag�ncia
              * GGGG.GG - N�mero Conta Corrente + DAC
              * HHH     - Zeros (N�o utilizado)
              * Z       - DAC Campo 3
              * 
              * Campo 4
              * K       - DAC C�digo de Barras
              * 
              * Campo 5
              * UUUUVVVVVVVVVV
              * UUUU       - Fator Vencimento
              * VVVVVVVVVV - Valor do T�tulo 
              */
                    #endregion Defini��es

                    #region DDDDD.DEFFFY

                    string E = _dacNossoNumero.ToString();
                    string FFF = agencia.Substring(0, 3);
                    string Y = Mod10(DDDDDD + E + FFF).ToString();

                    C2 = string.Format("{0}.", DDDDDD.Substring(0, 5));
                    C2 += string.Format("{0}{1}{2}{3} ", DDDDDD.Substring(5, 1), E, FFF, Y);

                    #endregion DDDDD.DEFFFY

                    #region FGGGG.GGHHHZ

                    string F = agencia.Substring(3, 1);
                    string GGGGGG = boleto.Cedente.ContaBancaria.Conta + boleto.Cedente.ContaBancaria.DigitoConta;
                    string HHH = "000";
                    string Z = Mod10(F + GGGGGG + HHH).ToString();

                    C3 = string.Format("{0}{1}.{2}{3}{4}", F, GGGGGG.Substring(0, 4), GGGGGG.Substring(4, 2), HHH, Z);

                    #endregion FGGGG.GGHHHZ
                }
                else if (boleto.Carteira == "198" || boleto.Carteira == "107"
                     || boleto.Carteira == "122" || boleto.Carteira == "142"
                     || boleto.Carteira == "143" || boleto.Carteira == "196")
                {
                    #region Defini��es
                    /* AAABC.CCDDX.DDDDD.DEEEEY.EEEFF.FFFGHZ.K.UUUUVVVVVVVVVV
              * ------------------------------------------------------
              * Campo 1 - AAABC.CCDDX
              * AAA - C�digo do Banco
              * B   - Moeda
              * CCC - Carteira
              * DD  - 2 primeiros n�meros Nosso N�mero
              * X   - DAC Campo 1 (AAABC.CCDD) Mod10
              * 
              * Campo 2 - DDDDD.DEEEEY
              * DDDDD.D - Restante Nosso N�mero
              * EEEE    - 4 primeiros numeros do n�mero do documento
              * Y       - DAC Campo 2 (DDDDD.DEEEEY) Mod10
              * 
              * Campo 3 - EEEFF.FFFGHZ
              * EEE     - Restante do n�mero do documento
              * FFFFF   - C�digo do Cliente
              * G       - DAC (Carteira/Nosso Numero(sem DAC)/Numero Documento/Codigo Cliente)
              * H       - zero
              * Z       - DAC Campo 3
              * 
              * Campo 4 - K
              * K       - DAC C�digo de Barras
              * 
              * Campo 5 - UUUUVVVVVVVVVV
              * UUUU       - Fator Vencimento
              * VVVVVVVVVV - Valor do T�tulo 
              */
                    #endregion Defini��es

                    #region DDDDD.DEEEEY

                    string EEEE = numeroDocumento.Substring(0, 4);
                    string Y = Mod10(DDDDDD + EEEE).ToString();

                    C2 = string.Format("{0}.", DDDDDD.Substring(0, 5));
                    C2 += string.Format("{0}{1}{2} ", DDDDDD.Substring(5, 1), EEEE, Y);

                    #endregion DDDDD.DEEEEY

                    #region EEEFF.FFFGHZ

                    string EEE = numeroDocumento.Substring(4, 3);
                    string FFFFF = codigoCedente;
                    string G = Mod10(boleto.Carteira + boleto.NossoNumero + numeroDocumento + codigoCedente).ToString();
                    string H = "0";
                    string Z = Mod10(EEE + FFFFF + G + H).ToString();
                    C3 = string.Format("{0}{1}.{2}{3}{4}{5}", EEE, FFFFF.Substring(0, 2), FFFFF.Substring(2, 3), G, H, Z);

                    #endregion EEEFF.FFFGHZ
                }
                else if (boleto.Carteira == "126" || boleto.Carteira == "131" || boleto.Carteira == "146" || boleto.Carteira == "150" || boleto.Carteira == "168")
                {
                    throw new NotImplementedException("Fun��o n�o implementada.");
                }

                boleto.CodigoBarra.LinhaDigitavel = C1 + C2 + C3 + K + C5;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao formatar linha digit�vel.", ex);
            }
        }

        public override void FormataNossoNumero(Boleto boleto)
        {
            try
            {
                boleto.NossoNumero = string.Format("{0}/{1}-{2}", boleto.Carteira, boleto.NossoNumero, _dacNossoNumero);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao formatar nosso n�mero", ex);
            }
        }

        public override void FormataNumeroDocumento(Boleto boleto)
        {
            try
            {
                boleto.NumeroDocumento = string.Format("{0}-{1}", boleto.NumeroDocumento, Mod10(boleto.NumeroDocumento));
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao formatar n�mero do documento.", ex);
            }
        }

        /// <summary>
        /// Verifica o tipo de ocorr�ncia para o arquivo remessa
        /// </summary>
        public string Ocorrencia(string codigo)
        {
            switch (codigo)
            {
                case "02":
                    return "02-Entrada Confirmada";
                case "03":
                    return "03-Entrada Rejeitada";
                case "04":
                    return "04-Altera��o de Dados-Nova entrada ou Altera��o/Exclus�o de dados acatada";
                case "05":
                    return "05-Altera��o de dados-Baixa";
                case "06":
                    return "06-Liquida��o normal";
                case "08":
                    return "08-Liquida��o em cart�rio";
                case "09":
                    return "09-Baixa simples";
                case "10":
                    return "10-Baixa por ter sido liquidado";
                case "11":
                    return "11-Em Ser (S� no retorno mensal)";
                case "12":
                    return "12-Abatimento Concedido";
                case "13":
                    return "13-Abatimento Cancelado";
                case "14":
                    return "14-Vencimento Alterado";
                case "15":
                    return "15-Baixas rejeitadas";
                case "16":
                    return "16-Instru��es rejeitadas";
                case "17":
                    return "17-Altera��o/Exclus�o de dados rejeitados";
                case "18":
                    return "18-Cobran�a contratual-Instru��es/Altera��es rejeitadas/pendentes";
                case "19":
                    return "19-Confirma Recebimento Instru��o de Protesto";
                case "20":
                    return "20-Confirma Recebimento Instru��o Susta��o de Protesto/Tarifa";
                case "21":
                    return "21-Confirma Recebimento Instru��o de N�o Protestar";
                case "23":
                    return "23-T�tulo enviado a Cart�rio/Tarifa";
                case "24":
                    return "24-Instru��o de Protesto Rejeitada/Sustada/Pendente";
                case "25":
                    return "25-Alega��es do Sacado";
                case "26":
                    return "26-Tarifa de Aviso de Cobran�a";
                case "27":
                    return "27-Tarifa de Extrato Posi��o";
                case "28":
                    return "28-Tarifa de Rela��o das Liquida��es";
                case "29":
                    return "29-Tarifa de Manuten��o de T�tulos Vencidos";
                case "30":
                    return "30-D�bito Mensal de Tarifas (Para Entradas e Baixas)";
                case "32":
                    return "32-Baixa por ter sido Protestado";
                case "33":
                    return "33-Custas de Protesto";
                case "34":
                    return "34-Custas de Susta��o";
                case "35":
                    return "35-Custas de Cart�rio Distribuidor";
                case "36":
                    return "36-Custas de Edital";
                case "37":
                    return "37-Tarifa de Emiss�o de Boleto/Tarifa de Envio de Duplicata";
                case "38":
                    return "38-Tarifa de Instru��o";
                case "39":
                    return "39-Tarifa de Ocorr�ncias";
                case "40":
                    return "40-Tarifa Mensal de Emiss�o de Boleto/Tarifa Mensal de Envio de Duplicata";
                case "41":
                    return "41-D�bito Mensal de Tarifas-Extrato de Posi��o(B4EP/B4OX)";
                case "42":
                    return "42-D�bito Mensal de Tarifas-Outras Instru��es";
                case "43":
                    return "43-D�bito Mensal de Tarifas-Manuten��o de T�tulos Vencidos";
                case "44":
                    return "44-D�bito Mensal de Tarifas-Outras Ocorr�ncias";
                case "45":
                    return "45-D�bito Mensal de Tarifas-Protesto";
                case "46":
                    return "56-D�bito Mensal de Tarifas-Susta��o de Protesto";
                case "47":
                    return "47-Baixa com Transfer�ncia para Protesto";
                case "48":
                    return "48-Custas de Susta��o Judicial";
                case "51":
                    return "51-Tarifa Mensal Ref a Entradas Bancos Correspondentes na Carteira";
                case "52":
                    return "52-Tarifa Mensal Baixas na Carteira";
                case "53":
                    return "53-Tarifa Mensal Baixas em Bancos Correspondentes na Carteira";
                case "54":
                    return "54-Tarifa Mensal de Liquida��es na Carteira";
                case "55":
                    return "55-Tarifa Mensal de Liquida��es em Bancos Correspondentes na Carteira";
                case "56":
                    return "56-Custas de Irregularidade";
                case "57":
                    return "57-Instru��o Cancelada";
                case "59":
                    return "59-Baixa por Cr�dito em C/C Atrav�s do SISPAG";
                case "60":
                    return "60-Entrada Rejeitada Carn�";
                case "61":
                    return "61-Tarifa Emiss�o Aviso de Movimenta��o de T�tulos";
                case "62":
                    return "62-D�bito Mensal de Tarifa-Aviso de Movimenta��o de T�tulos";
                case "63":
                    return "63-T�tulo Sustado Judicialmente";
                case "64":
                    return "64-Entrada Confirmada com Rateio de Cr�dito";
                case "69":
                    return "69-Cheque Devolvido";
                case "71":
                    return "71-Entrada Registrada-Aguardando Avalia��o";
                case "72":
                    return "72-Baixa por Cr�dito em C/C Atrav�s do SISPAG sem T�tulo Correspondente";
                case "73":
                    return "73-Confirma��o de Entrada na Cobran�a Simples-Entrada N�o Aceita na Cobran�a Contratual";
                case "76":
                    return "76-Cheque Compensado";
                default:
                    return "";
            }
        }

        /// <summary>
        /// Verifica o c�digo do motivo da rejei��o informada pelo banco
        /// </summary>
        public string MotivoRejeicao(string codigo)
        {
            switch (codigo)
            {
                case "03": return "03-AG. COBRADORA - CEP SEM ATENDIMENTO DE PROTESTO NO MOMENTO";
                case "04": return "04-ESTADO - SIGLA DO ESTADO INV�LIDA";
                case "05": return "05-DATA VENCIMENTO - PRAZO DA OPERA��O MENOR QUE PRAZO M�NIMO OU MAIOR QUE O M�XIMO";
                case "07": return "07-VALOR DO T�TULO - VALOR DO T�TULO MAIOR QUE 10.000.000,00";
                case "08": return "08-NOME DO PAGADOR - N�O INFORMADO OU DESLOCADO";
                case "09": return "09-AGENCIA/CONTA - AG�NCIA ENCERRADA";
                case "10": return "10-LOGRADOURO - N�O INFORMADO OU DESLOCADO";
                case "11": return "11-CEP - CEP N�O NUM�RICO OU CEP INV�LIDO";
                case "12": return "12-SACADOR / AVALISTA - NOME N�O INFORMADO OU DESLOCADO (BANCOS CORRESPONDENTES)";
                case "13": return "13-ESTADO/CEP - CEP INCOMPAT�VEL COM A SIGLA DO ESTADO";
                case "14": return "14-NOSSO N�MERO - NOSSO N�MERO J� REGISTRADO NO CADASTRO DO BANCO OU FORA DA FAIXA";
                case "15": return "15-NOSSO N�MERO - NOSSO N�MERO EM DUPLICIDADE NO MESMO MOVIMENTO";
                case "18": return "18-DATA DE ENTRADA - DATA DE ENTRADA INV�LIDA PARA OPERAR COM ESTA CARTEIRA";
                case "19": return "19-OCORR�NCIA - OCORR�NCIA INV�LIDA";
                case "21": return "21-AG. COBRADORA - CARTEIRA N�O ACEITA DEPOSIT�RIA CORRESPONDENTE ESTADO DA AG�NCIA DIFERENTE DO ESTADO DO PAGADOR AG. COBRADORA N�O CONSTA NO CADASTRO OU ENCERRANDO";
                case "22": return "22-CARTEIRA - CARTEIRA N�O PERMITIDA (NECESS�RIO CADASTRAR FAIXA LIVRE)";
                case "26": return "26-AG�NCIA/CONTA - AG�NCIA/CONTA N�O LIBERADA PARA OPERAR COM COBRAN�A";
                case "27": return "27-CNPJ INAPTO - CNPJ DO BENEFICI�RIO INAPTO DEVOLU��O DE T�TULO EM GARANTIA";
                case "29": return "29-C�DIGO EMPRESA - CATEGORIA DA CONTA INV�LIDA";
                case "30": return "30-ENTRADA BLOQUEADA - ENTRADAS BLOQUEADAS, CONTA SUSPENSA EM COBRAN�A";
                case "31": return "31-AG�NCIA/CONTA - CONTA N�O TEM PERMISS�O PARA PROTESTAR (CONTATE SEU GERENTE)";
                case "35": return "35-VALOR DO IOF - IOF MAIOR QUE 5%";
                case "36": return "36-QTDADE DE MOEDA - QUANTIDADE DE MOEDA INCOMPAT�VEL COM VALOR DO T�TULO";
                case "37": return "37-CNPJ/CPF DO PAGADOR - N�O NUM�RICO OU IGUAL A ZEROS";
                case "42": return "42-NOSSO N�MERO - NOSSO N�MERO FORA DE FAIXA";
                case "52": return "52-AG. COBRADORA - EMPRESA N�O ACEITA BANCO CORRESPONDENTE";
                case "53": return "53-AG. COBRADORA - EMPRESA N�O ACEITA BANCO CORRESPONDENTE - COBRAN�A MENSAGEM";
                case "54": return "54-DATA DE VENCTO - BANCO CORRESPONDENTE - T�TULO COM VENCIMENTO INFERIOR A 15 DIAS";
                case "55": return "55-DEP/BCO CORRESP - CEP N�O PERTENCE � DEPOSIT�RIA INFORMADA";
                case "56": return "56-DT VENCTO/BCO CORRESP - VENCTO SUPERIOR A 180 DIAS DA DATA DE ENTRADA";
                case "57": return "57-DATA DE VENCTO - CEP S� DEPOSIT�RIA BCO DO BRASIL COM VENCTO INFERIOR A 8 DIAS";
                case "60": return "60-ABATIMENTO - VALOR DO ABATIMENTO INV�LIDO";
                case "61": return "61-JUROS DE MORA - JUROS DE MORA MAIOR QUE O PERMITIDO";
                case "62": return "62-DESCONTO - VALOR DO DESCONTO MAIOR QUE VALOR DO T�TULO";
                case "63": return "63-DESCONTO DE ANTECIPA��O - VALOR DA IMPORT�NCIA POR DIA DE DESCONTO (IDD) N�O PERMITIDO";
                case "64": return "64-DATA DE EMISS�O - DATA DE EMISS�O DO T�TULO INV�LIDA";
                case "65": return "65-TAXA FINANCTO - TAXA INV�LIDA (VENDOR)";
                case "66": return "66-DATA DE VENCTO - INVALIDA/FORA DE PRAZO DE OPERA��O (M�NIMO OU M�XIMO)";
                case "67": return "67-VALOR/QTIDADE - VALOR DO T�TULO/QUANTIDADE DE MOEDA INV�LIDO";
                case "68": return "68-CARTEIRA - CARTEIRA INV�LIDA OU N�O CADASTRADA NO INTERC�MBIO DA COBRAN�A";
                case "69": return "69-CARTEIRA - CARTEIRA INV�LIDA PARA T�TULOS COM RATEIO DE CR�DITO";
                case "70": return "70-AG�NCIA/CONTA - BENEFICI�RIO N�O CADASTRADO PARA FAZER RATEIO DE CR�DITO";
                case "78": return "78-AG�NCIA/CONTA - DUPLICIDADE DE AG�NCIA/CONTA BENEFICI�RIA DO RATEIO DE CR�DITO";
                case "80": return "80-AG�NCIA/CONTA - QUANTIDADE DE CONTAS BENEFICI�RIAS DO RATEIO MAIOR DO QUE O PERMITIDO (M�XIMO DE 30 CONTAS POR T�TULO)";
                case "81": return "81-AG�NCIA/CONTA - CONTA PARA RATEIO DE CR�DITO INV�LIDA / N�O PERTENCE AO ITA�";
                case "82": return "82-DESCONTO/ABATI-MENTO - DESCONTO/ABATIMENTO N�O PERMITIDO PARA T�TULOS COM RATEIO DE CR�DITO";
                case "83": return "83-VALOR DO T�TULO - VALOR DO T�TULO MENOR QUE A SOMA DOS VALORES ESTIPULADOS PARA RATEIO";
                case "84": return "84-AG�NCIA/CONTA - AG�NCIA/CONTA BENEFICI�RIA DO RATEIO � A CENTRALIZADORA DE CR�DITO DO BENEFICI�RIO";
                case "85": return "85-AG�NCIA/CONTA - AG�NCIA/CONTA DO BENEFICI�RIO � CONTRATUAL / RATEIO DE CR�DITO N�O PERMITIDO";
                case "86": return "86-TIPO DE VALOR - C�DIGO DO TIPO DE VALOR INV�LIDO / N�O PREVISTO PARA T�TULOS COM RATEIO DE CR�DITO";
                case "87": return "87-AG�NCIA/CONTA - REGISTRO TIPO 4 SEM INFORMA��O DE AG�NCIAS/CONTAS BENEFICI�RIAS DO RATEIO";
                case "90": return "90-NRO DA LINHA - COBRAN�A MENSAGEM - N�MERO DA LINHA DA MENSAGEM INV�LIDO OU QUANTIDADE DE LINHAS EXCEDIDAS";
                case "97": return "97-SEM MENSAGEM - COBRAN�A MENSAGEM SEM MENSAGEM (S� DE CAMPOS FIXOS), POR�M COM REGISTRO DO TIPO 7 OU 8";
                case "98": return "98-FLASH INV�LIDO - REGISTRO MENSAGEM SEM FLASH CADASTRADO OU FLASH INFORMADO DIFERENTE DO CADASTRADO";
                case "99": return "99-FLASH INV�LIDO - CONTA DE COBRAN�A COM FLASH CADASTRADO E SEM REGISTRO DE MENSAGEM CORRESPONDENTE";
                default:
                    return string.Empty;
            }
        }

        # endregion

        # region M�todos de gera��o do arquivo remessa

        # region HEADER
        public override string GerarHeaderRemessa(string numeroConvenio, Cedente cendente, TipoArquivo tipoArquivo, int numeroArquivoRemessa, Boleto boletos)
        {
            throw new NotImplementedException("Fun��o n�o implementada.");
        }
        public override string GerarHeaderRemessa(Cedente cedente, TipoArquivo tipoArquivo, int numeroArquivoRemessa)
        {
            return GerarHeaderRemessa("0", cedente, tipoArquivo, numeroArquivoRemessa);
        }
        /// <summary>
        /// HEADER do arquivo CNAB
        /// Gera o HEADER do arquivo remessa de acordo com o lay-out informado
        /// </summary>
        public override string GerarHeaderRemessa(string numeroConvenio, Cedente cedente, TipoArquivo tipoArquivo, int numeroArquivoRemessa)
        {
            try
            {
                string _header = " ";

                base.GerarHeaderRemessa("0", cedente, tipoArquivo, numeroArquivoRemessa);

                switch (tipoArquivo)
                {

                    case TipoArquivo.CNAB240:
                        _header = GerarHeaderRemessaCNAB240(cedente);
                        break;
                    case TipoArquivo.CNAB400:
                        _header = GerarHeaderRemessaCNAB400(0, cedente, numeroArquivoRemessa);
                        break;
                    case TipoArquivo.Outro:
                        throw new Exception("Tipo de arquivo inexistente.");
                }

                return _header;

            }
            catch (Exception ex)
            {
                throw new Exception("Erro durante a gera��o do HEADER do arquivo de REMESSA.", ex);
            }
        }

        /// <summary>
        ///POS INI/FINAL	DESCRI��O	                   A/N	TAM	DEC	CONTE�DO	NOTAS
        ///--------------------------------------------------------------------------------
        ///001 - 003	C�digo do Banco na compensa��o	    N	003		341	
        ///004 - 007	Lote de servi�o	                    N	004		0000	1 
        ///008 - 008	Registro Hearder de Arquivo         N	001		0	2
        ///009 - 017	Reservado (uso Banco)	            A	009		Brancos	  
        ///018 - 018	Tipo de inscri��o da empresa	    N	001		1 = CPF,  2 = CNPJ 	
        ///019 � 032	N� de inscri��o da empresa	        N	014		Nota 1
        ///033 � 045	C�digo do Conv�nio no Banco   	    A	013	    Nota 2 
        ///046 - 052	Reservado (uso Banco)	            A	007		Brancos	
        ///053 - 053	Complemento de Registro             N	001     0			
        ///054 - 057	Ag�ncia Referente Conv�nio Ass.     N	004     Nota 1
        ///058 - 058    Complemento de Registro             A   001     Brancos
        ///059 - 065    Complemento de Registro             N   007     Brancos
        ///066 - 070    N�mero da C/C do Cliente            N   005     Nota 1
        ///071 - 071    Complemento de Registro             A   001     Brancos
        ///072 - 072    DAC da Ag�ncia/Conta                N   001     Nota 1
        ///073 - 102    Nome da Empresa                     A   030     Nome da Empresa
        ///103 - 132	Nome do Banco	                    A	030		Banco Ita� 	
        ///133 - 142	Reservado (uso Banco)	            A	010		Brancos	
        ///143 - 143	C�digo remessa 	                    N	001		1 = Remessa 	
        ///144 - 151	Data de gera��o do arquivo	        N	008		DDMMAAAA	
        ///152 - 157	Hora de gera��o do arquivo          N	006		HHMMSS
        ///158 - 163	N� seq�encial do arquivo 	        N	006	    Nota 3
        ///164 - 166	N� da vers�o do layout do arquivo	N	003		040
        ///167 - 171    Densidaded de Grava��o do arquivo   N   005     00000
        ///172 - 240	Reservado (uso Banco)	            A	069		Brancos	
        /// </summary>
        public string GerarHeaderRemessaCNAB240(Cedente cedente)
        {
            try
            {
                string header = "341";
                header += "0001";
                header += "0";
                header += Utils.FormatCode("", " ", 9);
                header += (cedente.CPFCNPJ.Length == 11 ? "1" : "2");
                header += Utils.FormatCode(cedente.CPFCNPJ, "0", 14, true);
                header += Utils.FormatCode("", " ", 20);
                header += "0";
                header += Utils.FormatCode(cedente.ContaBancaria.Agencia, " ", 4, true);
                header += " ";
                header += "0000000";
                header += Utils.FormatCode(cedente.ContaBancaria.Conta, "0", 5, true);
                header += " ";
                header += Utils.FormatCode(String.IsNullOrEmpty(cedente.ContaBancaria.DigitoConta) ? " " : cedente.ContaBancaria.DigitoConta, " ", 1, true);
                header += Utils.FitStringLength(cedente.Nome, 30, 30, ' ', 0, true, true, false);                
                header += Utils.FormatCode("BANCO ITAU SA", " ", 30);
                header += Utils.FormatCode("", " ", 10);
                header += "1";
                header += DateTime.Now.ToString("ddMMyyyyHHmmss");
                header += Utils.FormatCode("", "0", 6, true);
                header += "040";
                header += "00000";
                header += Utils.FormatCode("", " ", 54);
                header += "000";
                header += Utils.FormatCode("", " ", 12);
                header = Utils.SubstituiCaracteresEspeciais(header);
                return header;

            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar HEADER do arquivo de remessa do CNAB240.", ex);
            }
        }

        public string GerarHeaderRemessaCNAB400(int numeroConvenio, Cedente cedente, int numeroArquivoRemessa)
        {
            try
            {
                string complemento = new string(' ', 294);
                string _header;

                _header = "01REMESSA01COBRANCA       ";
                _header += Utils.FitStringLength(cedente.ContaBancaria.Agencia, 4, 4, '0', 0, true, true, true);
                _header += "00";
                _header += Utils.FitStringLength(cedente.ContaBancaria.Conta, 5, 5, '0', 0, true, true, true);
                _header += Utils.FitStringLength(cedente.ContaBancaria.DigitoConta, 1, 1, ' ', 0, true, true, false);
                _header += "        ";
                _header += Utils.FitStringLength(cedente.Nome, 30, 30, ' ', 0, true, true, false).ToUpper();
                _header += "341";
                _header += "BANCO ITAU SA  ";
                _header += DateTime.Now.ToString("ddMMyy");
                _header += complemento;
                _header += "000001";

                _header = Utils.SubstituiCaracteresEspeciais(_header);

                return _header;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar HEADER do arquivo de remessa do CNAB400.", ex);
            }
        }

        # endregion

        # region Header do Lote

        public override string GerarHeaderLoteRemessa(string numeroConvenio, Cedente cedente, int numeroArquivoRemessa, TipoArquivo tipoArquivo)
        {
            try
            {
                string header = " ";

                switch (tipoArquivo)
                {

                    case TipoArquivo.CNAB240:
                        header = GerarHeaderLoteRemessaCNAB240(cedente, numeroArquivoRemessa);
                        break;
                    case TipoArquivo.CNAB400:
                        header = GerarHeaderLoteRemessaCNAB400(0, cedente, numeroArquivoRemessa);
                        break;
                    case TipoArquivo.Outro:
                        throw new Exception("Tipo de arquivo inexistente.");
                }

                return header;

            }
            catch (Exception ex)
            {
                throw new Exception("Erro durante a gera��o do HEADER DO LOTE do arquivo de REMESSA.", ex);
            }
        }

        /// <summary>
        ///POS INI/FINAL	DESCRI��O	                   A/N	TAM	DEC	CONTE�DO	NOTAS
        ///--------------------------------------------------------------------------------
        ///001 - 003	C�digo do Banco na compensa��o	    N	003		341	
        ///004 - 007	Lote de servi�o	                    N	004		Nota 5 
        ///008 - 008	Registro Hearder de Lote            N	001     1
        ///009 - 009	Tipo de Opera��o                    A	001		D
        ///010 - 011	Tipo de servi�o             	    N	002		05
        ///012 � 013	Forma de Lan�amento                 N	002		50
        ///014 � 016	N�mero da vers�o do Layout   	    A	003	    030
        ///017 - 017	Complemento de Registro             A	001		Brancos	
        ///019 � 032	N� de inscri��o da empresa	        N	014		Nota 1
        ///033 � 045	C�digo do Conv�nio no Banco   	    A	013	    Nota 2
        ///018 - 018	Tipo de inscri��o da empresa	    N	001		1 = CPF,  2 = CNPJ
        ///046 - 052	Reservado (uso Banco)	            A	007		Brancos	
        ///053 - 053	Complemento de Registro             N	001     0			
        ///054 - 057	Ag�ncia Referente Conv�nio Ass.     N	004     Nota 1
        ///058 - 058    Complemento de Registro             A   001     Brancos
        ///059 - 065    Complemento de Registro             N   007     0000000
        ///066 - 070    N�mero da C/C do Cliente            N   005     Nota 1
        ///071 - 071    Complemento de Registro             A   001     Brancos
        ///072 - 072    DAC da Ag�ncia/Conta                N   001     Nota 1
        ///073 - 102    Nome da Empresa                     A   030     ENIX...
        ///103 - 142	Complemento de Registro             A	040		Brancos
        ///143 - 172	Endere�o da Empresa                 A	030		Nome da rua, Av., P�a, etc.
        ///173 - 177	N�mero do local                     N	005		N�mero do Local da Empresa
        ///178 - 192	Complemento                         A	015		Casa, Apto., Sala, etc.
        ///193 - 212	Nome da Cidade                      A	020	    Sao Paulo
        ///213 - 220	CEP                             	N	008		CEP
        ///221 - 222    Sigla do Estado                     A   002     SP
        ///223 - 230	Complemento de Registro             A	008		Brancos
        ///231 - 240    C�d. Ocr. para Retorno              A   010     Brancos
        /// </summary>

        private string GerarHeaderLoteRemessaCNAB240(Cedente cedente, int numeroArquivoRemessa)
        {
            try
            {
                string header = Utils.FormatCode(Codigo.ToString(), "0", 3, true);
                header += "0001";
                header += "1";
                header += "R";
                header += "01";
                header += "00";
                header += "030";
                header += " ";
                header += (cedente.CPFCNPJ.Length == 11 ? "1" : "2");
                header += Utils.FormatCode(cedente.CPFCNPJ, "0", 15, true);
                header += Utils.FormatCode("", " ", 20);
                header += "0";
                header += Utils.FormatCode(cedente.ContaBancaria.Agencia, "0", 4, true);
                header += " ";
                header += Utils.FormatCode("", "0", 7);
                header += Utils.FormatCode(cedente.ContaBancaria.Conta, "0", 5, true);
                header += " ";
                header += Utils.FormatCode(String.IsNullOrEmpty(cedente.ContaBancaria.DigitoConta) ? " " : cedente.ContaBancaria.DigitoConta, " ", 1, true);
                header += Utils.FitStringLength(cedente.Nome, 30, 30, ' ', 0, true, true, false);
                header += Utils.FormatCode("", " ", 80);
                header += Utils.FormatCode("", "0", 8, true);
                header += DateTime.Now.ToString("ddMMyyyy");
                header += DateTime.Now.ToString("ddMMyyyy");
                header += Utils.FormatCode("", " ", 33);
                header = Utils.SubstituiCaracteresEspeciais(header);
                return header;
            }
            catch (Exception e)
            {
                throw new Exception("Erro ao gerar HEADER DO LOTE do arquivo de remessa.", e);

            }
        }

        private string GerarHeaderLoteRemessaCNAB400(int numeroConvenio, Cedente cedente, int numeroArquivoRemessa)
        {
            throw new Exception("Fun��o n�o implementada.");
        }


        public override string GerarDetalheSegmentoPRemessa(Boleto boleto, int numeroRegistro, string numeroConvenio)
        {
            try
            {                
                string _segmentoP;
                _segmentoP = "341";
                _segmentoP += "0001";
                _segmentoP += "3";
                _segmentoP += Utils.FitStringLength(numeroRegistro.ToString(), 5, 5, '0', 0, true, true, true);
                _segmentoP += "P";
                _segmentoP += " ";
                _segmentoP += ObterCodigoDaOcorrencia(boleto);
                _segmentoP += "0";
                _segmentoP += Utils.FitStringLength(boleto.Cedente.ContaBancaria.Agencia, 4, 4, '0', 0, true, true, true);
                _segmentoP += " ";
                _segmentoP += "0000000";
                _segmentoP += Utils.FitStringLength(boleto.Cedente.ContaBancaria.Conta, 5, 5, '0', 0, true, true, true);
                _segmentoP += " ";
                _segmentoP += Utils.FormatCode(String.IsNullOrEmpty(boleto.Cedente.ContaBancaria.DigitoConta) ? " " : boleto.Cedente.ContaBancaria.DigitoConta, " ", 1, true);
                _segmentoP += Utils.FitStringLength(boleto.Carteira, 3, 3, '0', 0, true, true, true);
                _segmentoP += Utils.FitStringLength(boleto.NossoNumero, 8, 8, '0', 0, true, true, true);
                _segmentoP += Utils.FitStringLength(boleto.DigitoNossoNumero, 1, 1, '0', 1, true, true, true);
                _segmentoP += "        ";
                _segmentoP += "00000";
                _segmentoP += Utils.FitStringLength(boleto.NumeroDocumento, 10, 10, ' ', 0, true, true, false);
                _segmentoP += "     ";
                _segmentoP += Utils.FitStringLength(boleto.DataVencimento.ToString("ddMMyyyy"), 8, 8, ' ', 0, true, true, false);
                _segmentoP += Utils.FitStringLength(boleto.ValorBoleto.ApenasNumeros(), 15, 15, '0', 0, true, true, true);
                _segmentoP += "00000";
                _segmentoP += " ";
                _segmentoP += "01";
                _segmentoP += "A";
                _segmentoP += Utils.FitStringLength(boleto.DataDocumento.ToString("ddMMyyyy"), 8, 8, ' ', 0, true, true, false);
                _segmentoP += "0";
                _segmentoP += Utils.FitStringLength(boleto.DataJurosMora.ToString("ddMMyyyy"), 8, 8, ' ', 0, true, true, false);
                _segmentoP += Utils.FitStringLength(boleto.JurosMora.ApenasNumeros(), 15, 15, '0', 0, true, true, true);
                _segmentoP += "0";
                _segmentoP += Utils.FitStringLength(boleto.DataVencimento.ToString("ddMMyyyy"), 8, 8, ' ', 0, true, true, false);
                _segmentoP += Utils.FitStringLength("0", 15, 15, '0', 0, true, true, true);
                _segmentoP += Utils.FitStringLength("0", 15, 15, '0', 0, true, true, true);
                _segmentoP += Utils.FitStringLength("0", 15, 15, '0', 0, true, true, true);
                _segmentoP += Utils.FitStringLength(boleto.NumeroDocumento, 25, 25, ' ', 0, true, true, false);
                _segmentoP += "0";
                _segmentoP += "00";
                _segmentoP += "0";
                _segmentoP += "00";
                _segmentoP += "0000000000000";
                _segmentoP += " ";

                _segmentoP = Utils.SubstituiCaracteresEspeciais(_segmentoP);

                return _segmentoP;

            }
            catch (Exception ex)
            {
                throw new Exception("Erro durante a gera��o do SEGMENTO P DO DETALHE do arquivo de REMESSA.", ex);
            }
        }
        public override string GerarDetalheSegmentoQRemessa(Boleto boleto, int numeroRegistro, TipoArquivo tipoArquivo)
        {
            try
            {
                string _zeros16 = new string('0', 16);
                string _brancos10 = new string(' ', 10);
                string _brancos28 = new string(' ', 28);
                string _brancos40 = new string(' ', 40);

                string _segmentoQ;

                _segmentoQ = "341";
                _segmentoQ += "0001";
                _segmentoQ += "3";
                _segmentoQ += Utils.FitStringLength(numeroRegistro.ToString(), 5, 5, '0', 0, true, true, true);
                _segmentoQ += "Q";
                _segmentoQ += " ";

                _segmentoQ += ObterCodigoDaOcorrencia(boleto);
                if (boleto.Sacado.CPFCNPJ.Length <= 11)
                    _segmentoQ += "1";
                else
                    _segmentoQ += "2";

                _segmentoQ += Utils.FitStringLength(boleto.Sacado.CPFCNPJ, 15, 15, '0', 0, true, true, true);
                _segmentoQ += Utils.FitStringLength(boleto.Sacado.Nome.TrimStart(' '), 30, 30, ' ', 0, true, true, false).ToUpper();
                _segmentoQ += "          ";
                _segmentoQ += Utils.FitStringLength(boleto.Sacado.Endereco.End.TrimStart(' '), 40, 40, ' ', 0, true, true, false).ToUpper();
                _segmentoQ += Utils.FitStringLength(boleto.Sacado.Endereco.Bairro.TrimStart(' '), 15, 15, ' ', 0, true, true, false).ToUpper();
                _segmentoQ += Utils.FitStringLength(boleto.Sacado.Endereco.CEP, 8, 8, ' ', 0, true, true, false).ToUpper(); ;
                _segmentoQ += Utils.FitStringLength(boleto.Sacado.Endereco.Cidade.TrimStart(' '), 15, 15, ' ', 0, true, true, false).ToUpper();
                _segmentoQ += Utils.FitStringLength(boleto.Sacado.Endereco.UF, 2, 2, ' ', 0, true, true, false).ToUpper();

                if (boleto.Cedente.CPFCNPJ.Length <= 11)
                    _segmentoQ += "1";
                else
                    _segmentoQ += "2";

                _segmentoQ += Utils.FitStringLength(boleto.Cedente.CPFCNPJ, 15, 15, '0', 0, true, true, true);
                _segmentoQ += Utils.FitStringLength(boleto.Cedente.Nome.TrimStart(' '), 30, 30, ' ', 0, true, true, false).ToUpper();
                _segmentoQ += _brancos10;
                _segmentoQ += "000";
                _segmentoQ += _brancos28;

                _segmentoQ = Utils.SubstituiCaracteresEspeciais(_segmentoQ);

                return _segmentoQ;

            }
            catch (Exception ex)
            {
                throw new Exception("Erro durante a gera��o do SEGMENTO Q DO DETALHE do arquivo de REMESSA.", ex);
            }
        }
        public override string GerarDetalheSegmentoRRemessa(Boleto boleto, int numeroRegistro, TipoArquivo tipoArquivo)
        {
            try
            {
                string _brancos110 = new string(' ', 110);
                string _brancos9 = new string(' ', 9);

                string _segmentoR;

                _segmentoR = "341";
                _segmentoR += "0001";
                _segmentoR += "3";
                _segmentoR += Utils.FitStringLength(numeroRegistro.ToString(), 5, 5, '0', 0, true, true, true);
                _segmentoR += "R ";
                _segmentoR += ObterCodigoDaOcorrencia(boleto);
                
                //Suelton - 18/12/2018 - Implementa��o do 2 desconto por antecipa��o
                if (boleto.DataDescontoAntecipacao2.HasValue && boleto.ValorDescontoAntecipacao2.HasValue)
                {
                    _segmentoR += "1" + //'1' = Valor Fixo At� a Data Informada
                        Utils.FitStringLength(boleto.DataDescontoAntecipacao2.Value.ToString("ddMMyyyy"), 8, 8, '0', 0, true, true, false) +
                        Utils.FitStringLength(boleto.ValorDescontoAntecipacao2.ApenasNumeros(), 15, 15, '0', 0, true, true, true);
                }
                else
                {
                    // Desconto 2
                    _segmentoR += "000000000000000000000000"; //24 zeros
                }

                //Suelton - 18/12/2018 - Implementa��o do 3 desconto por antecipa��o
                if (boleto.DataDescontoAntecipacao3.HasValue && boleto.ValorDescontoAntecipacao3.HasValue)
                {
                    _segmentoR += "1" + //'1' = Valor Fixo At� a Data Informada
                        Utils.FitStringLength(boleto.DataDescontoAntecipacao3.Value.ToString("ddMMyyyy"), 8, 8, '0', 0, true, true, false) +
                        Utils.FitStringLength(boleto.ValorDescontoAntecipacao3.ApenasNumeros(), 15, 15, '0', 0, true, true, true);
                }
                else
                {
                    // Desconto 3
                    _segmentoR += "000000000000000000000000"; //24 zeros
                }

                if (boleto.PercMulta > 0)
                {
                    // C�digo da multa 2 - percentual
                    _segmentoR += "2";
                }
                else if (boleto.ValorMulta > 0)
                {
                    // C�digo da multa 1 - valor fixo
                    _segmentoR += "1";
                }
                else
                {
                    // C�digo da multa 0 - sem multa
                    _segmentoR += "0";
                }

                _segmentoR += Utils.FitStringLength(boleto.DataMulta.ToString("ddMMyyyy"), 8, 8, '0', 0, true, true, false);
                _segmentoR += Utils.FitStringLength(boleto.ValorMulta.ApenasNumeros(), 15, 15, '0', 0, true, true, true);
                _segmentoR += _brancos110;
                _segmentoR += "0000000000000000"; //16 zeros
                _segmentoR += " "; //1 branco
                _segmentoR += "000000000000"; //12 zeros
                _segmentoR += "  "; //2 brancos
                _segmentoR += "0"; //1 zero
                _segmentoR += _brancos9;

                _segmentoR = Utils.SubstituiCaracteresEspeciais(_segmentoR);

                return _segmentoR;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro durante a gera��o do SEGMENTO R DO DETALHE do arquivo de REMESSA.", ex);
            }
        }




        #endregion

        # region DETALHE

        /// <summary>
        /// DETALHE do arquivo CNAB
        /// Gera o DETALHE do arquivo remessa de acordo com o lay-out informado
        /// </summary>
        public override string GerarDetalheRemessa(Boleto boleto, int numeroRegistro, TipoArquivo tipoArquivo)
        {
            try
            {
                string _detalhe = " ";

                base.GerarDetalheRemessa(boleto, numeroRegistro, tipoArquivo);

                switch (tipoArquivo)
                {
                    case TipoArquivo.CNAB240:
                        _detalhe = GerarDetalheRemessaCNAB240(boleto, numeroRegistro, tipoArquivo);
                        break;
                    case TipoArquivo.CNAB400:
                        _detalhe = GerarDetalheRemessaCNAB400(boleto, numeroRegistro, tipoArquivo);
                        break;
                    case TipoArquivo.Outro:
                        throw new Exception("Tipo de arquivo inexistente.");
                }

                return _detalhe;

            }
            catch (Exception ex)
            {
                throw new Exception("Erro durante a gera��o do DETALHE arquivo de REMESSA.", ex);
            }
        }

        /// <summary>
        ///POS INI/FINAL	DESCRI��O	                   A/N	TAM	DEC	CONTE�DO	NOTAS
        ///--------------------------------------------------------------------------------
        ///001 - 003	C�digo do Banco na compensa��o	    N	003		341	
        ///004 - 007	Lote de servi�o	                    N	004		Nota 5 
        ///008 - 008	Registro Detalhe de Lote            N	001     3
        ///009 - 013	N�mero Sequencial Registro Lote     N	005		Nota 6
        ///014 - 014	C�digo Segmento Reg. Detalhe   	    A	001		A
        ///015 � 017	C�digo da Instru��o p/ Movimento    N	003		Nota 7
        ///018 - 020	C�digo da C�mara de Compensa��o     N	003	    000
        ///021 - 023	C�digo do Banco                     N	003	    341
        ///024 � 024	Complemento de Registros	        N	001		0
        ///025 � 028	N�mero Agencia Debitada       	    N	004	    
        ///029 - 029	Complemento de Registros            A	001		Brancos
        ///030 - 036	Complemento de Registros            N	007		0000000
        ///037 - 041	N�mero da Conta Debitada            N	005     
        ///042 - 042	Complemento de Registros            A	001     Brancos
        ///043 - 043    D�gito Verificador da AG/Conta      N   001     
        ///044 - 073    Nome do Debitado                    A   030     
        ///074 - 088    Nr. do Docum. Atribu�do p/ Empresa  A   015     Nota 8
        ///089 - 093    Complemento de Registros            A   005     Brancos
        ///094 - 101    Data para o Lan�amento do D�bito    N   008     DDMMAAAA
        ///102 - 104    Tipo da Moeda                       A   005     Nota 9
        ///105 - 119	Quantidade da Moeda ou IOF          N	015		Nota 10
        ///120 - 134	Valor do Lan�amento p/ D�bito       N	015		Nota 10
        ///135 - 154	Complemento de Registros            A	020		Brancos
        ///155 - 162	Complemento de Registros            A	008		Brancos
        ///163 - 177	Complemento de Registros            N	015	    Brancos
        ///178 - 179	Tipo do Encargo por dia de Atraso 	N	002		Nota 12
        ///180 - 196    Valor do Encargo p/ dia de Atraso   N   017     Nota 12
        ///197 - 212	Info. Compl. p/ Hist�rico C/C       A	016		Nota 13
        ///213 - 216    Complemento de Registros            A   004     Brancos
        ///217 - 230    No. de Insc. do Debitado(CPF/CNPJ)  N   014     
        ///231 - 240    C�d. Ocr. para Retorno              A   010     Brancos
        /// </summary>
        public string GerarDetalheRemessaCNAB240(Boleto boleto, int numeroRegistro, TipoArquivo tipoArquivo)
        {
            try
            {
                string detalhe = Utils.FormatCode(Codigo.ToString(), "0", 3, true);
                detalhe += Utils.FormatCode("", "0", 4, true);
                detalhe += "3";
                detalhe += Utils.FormatCode("", "0", 5, true);
                detalhe += "A";
                detalhe += Utils.FormatCode("", "0", 3, true);
                detalhe += "000";
                detalhe += Utils.FormatCode(Codigo.ToString(), "0", 3, true);
                detalhe += "0";
                detalhe += Utils.FormatCode("", "0", 4, true);
                detalhe += " ";
                detalhe += Utils.FormatCode("", "0", 7);
                detalhe += Utils.FormatCode("", "4", 5, true);
                detalhe += " ";
                detalhe += Utils.FormatCode("", "0", 1);
                detalhe += Utils.FitStringLength(boleto.Sacado.Nome, 30, 30, ' ', 0, true, true, false);
                detalhe += Utils.FormatCode(boleto.NossoNumero, " ", 15);
                detalhe += Utils.FormatCode("", " ", 5);
                detalhe += DateTime.Now.ToString("ddMMyyyy");
                detalhe += Utils.FormatCode("", " ", 3);
                detalhe += Utils.FormatCode("", "0", 15, true);
                detalhe += Utils.FormatCode("", "0", 15, true);
                detalhe += Utils.FormatCode("", " ", 20);
                detalhe += Utils.FormatCode("", " ", 8);
                detalhe += Utils.FormatCode("", " ", 15);
                detalhe += Utils.FormatCode("", "0", 2, true);
                detalhe += Utils.FormatCode("", "0", 17, true);
                detalhe += Utils.FormatCode("", " ", 16);
                detalhe += Utils.FormatCode("", " ", 4);
                detalhe += Utils.FormatCode(boleto.Cedente.CPFCNPJ, "0", 14, true);
                detalhe += Utils.FormatCode("", " ", 10);
                detalhe = Utils.SubstituiCaracteresEspeciais(detalhe);
                return detalhe;
            }
            catch (Exception e)
            {
                throw new Exception("Erro ao gerar DETALHE do arquivo CNAB240.", e);
            }
        }

        public string GerarDetalheRemessaCNAB400(Boleto boleto, int numeroRegistro, TipoArquivo tipoArquivo)
        {
            try
            {
                base.GerarDetalheRemessa(boleto, numeroRegistro, tipoArquivo);

                // USO DO BANCO - Identifica��o da opera��o no Banco (posi��o 87 a 107)
                string identificaOperacaoBanco = new string(' ', 21);
                string usoBanco = new string(' ', 10);
                string nrDocumento = new string(' ', 25);
                string _detalhe;

                _detalhe = "1";

                // Tipo de inscri��o da empresa

                // Normalmente definem o tipo (CPF/CNPJ) e o n�mero de inscri��o do cedente. 
                // Se o t�tulo for negociado, dever�o ser utilizados para indicar o CNPJ/CPF do sacador 
                // (cedente original), uma vez que os cart�rios exigem essa informa��o para efetiva��o 
                // dos protestos. Para este fim, tamb�m poder� ser utilizado o registro tipo �5�.
                // 01 - CPF DO CEDENTE
                // 02 - CNPJ DO CEDENTE
                // 03 - CPF DO SACADOR
                // 04 - CNPJ DO SACADOR
                // O arquivo gerado pelo aplicativo do Banco ITA�, sempre atriubuiu 04 para o tipo de inscri��o da empresa

                if (boleto.Cedente.CPFCNPJ.Length <= 11)
                    _detalhe += "01";
                else
                    _detalhe += "02";
                _detalhe += Utils.FitStringLength(boleto.Cedente.CPFCNPJ.ToString(), 14, 14, '0', 0, true, true, true);
                _detalhe += Utils.FitStringLength(boleto.Cedente.ContaBancaria.Agencia.ToString(), 4, 4, '0', 0, true, true, true);
                _detalhe += "00";
                _detalhe += Utils.FitStringLength(boleto.Cedente.ContaBancaria.Conta.ToString(), 5, 5, '0', 0, true, true, true);
                _detalhe += Utils.FitStringLength(boleto.Cedente.ContaBancaria.DigitoConta.ToString(), 1, 1, ' ', 0, true, true, false);
                _detalhe += "    "; // Complemento do registro - 4 posi��es em branco

                // C�digo da instru��o/alega��o a ser cancelada

                // Deve ser preenchido na remessa somente quando utilizados, na posi��o 109-110, os c�digos de ocorr�ncia 35 � 
                // Cancelamento de Instru��o e 38 � Cedente n�o concorda com alega��o do sacado. Para os demais c�digos de 
                // ocorr�ncia este campo dever� ser preenchido com zeros. 
                //Obs.: No arquivo retorno ser� informado o mesmo c�digo da instru��o cancelada, e para o cancelamento de alega��o 
                // de sacado n�o h� retorno da informa��o.

                // Por enquanto o objetivo � apenas gerar o arquivo de remessa e n�o utilizar o arquivo para enviar instru��es
                // para t�tulos que j� est�o no banco, portanto o campo ser� preenchido com zeros.
                _detalhe += "0000";

                _detalhe += Utils.FitStringLength(boleto.NumeroControle ?? boleto.NumeroDocumento, 25, 25, ' ', 0, true, true, false); //Identifica��o do t�tulo na empresa
                _detalhe += Utils.FitStringLength(boleto.NossoNumero, 8, 8, '0', 0, true, true, true);
                // Quantidade de moeda vari�vel - Preencher com zeros se a moeda for REAL
                // O manual do Banco ITA� n�o diz como preencher caso a moeda n�o seja o REAL
                if (boleto.Moeda == 9)
                    _detalhe += "0000000000000";

                _detalhe += Utils.FitStringLength(boleto.Carteira, 3, 3, '0', 0, true, true, true);
                _detalhe += Utils.FitStringLength(identificaOperacaoBanco, 21, 21, ' ', 0, true, true, true);
                // C�digo da carteira
                if (boleto.Moeda == 9)
                    _detalhe += "I"; //O c�digo da carteira s� muda para dois tipos, quando a cobran�a for em d�lar

                _detalhe += ObterCodigoDaOcorrencia(boleto);

                _detalhe += Utils.FitStringLength(boleto.NumeroDocumento, 10, 10, ' ', 0, true, true, false);
                _detalhe += boleto.DataVencimento.ToString("ddMMyy");
                _detalhe += Utils.FitStringLength(boleto.ValorBoleto.ApenasNumeros(), 13, 13, '0', 0, true, true, true);
                _detalhe += "341";
                _detalhe += "00000"; // Ag�ncia onde o t�tulo ser� cobrado - no arquivo de remessa, preencher com ZEROS

                _detalhe += Utils.FitStringLength(EspecieDocumento.ValidaCodigo(boleto.EspecieDocumento).ToString(), 2, 2, '0', 0, true, true, true);
                _detalhe += Utils.FitStringLength(boleto.Aceite, 1, 1, ' ', 0, true, true, false); // Identifica��o de t�tulo, Aceito ou N�o aceito

                //A data informada neste campo deve ser a mesma data de emiss�o do t�tulo de cr�dito 
                //(Duplicata de Servi�o / Duplicata Mercantil / Nota Fiscal, etc), que deu origem a esta Cobran�a. 
                //Existindo diverg�ncia, na exist�ncia de protesto, a documenta��o poder� n�o ser aceita pelo Cart�rio.
                _detalhe += boleto.DataDocumento.ToString("ddMMyy");

                switch (boleto.Instrucoes.Count)
                {
                    case 0:
                        _detalhe += "0000"; // J�ferson (jefhtavares) o banco n�o estava aceitando esses campos em Branco
                        break;
                    case 1:
                        _detalhe += Utils.FitStringLength(boleto.Instrucoes[0].Codigo.ToString(), 2, 2, '0', 0, true, true, true);
                        _detalhe += "00"; // J�ferson (jefhtavares) o banco n�o estava aceitando esses campos em Branco
                        break;
                    default:
                        _detalhe += Utils.FitStringLength(boleto.Instrucoes[0].Codigo.ToString(), 2, 2, '0', 0, true, true, true);
                        _detalhe += Utils.FitStringLength(boleto.Instrucoes[1].Codigo.ToString(), 2, 2, '0', 0, true, true, true);
                        break;
                }
                                
                _detalhe += Utils.FitStringLength(boleto.JurosMora.ApenasNumeros(), 13, 13, '0', 0, true, true, true);

                // Data limite para desconto
                _detalhe += boleto.DataDesconto == DateTime.MinValue ? boleto.DataVencimento.ToString("ddMMyy") : boleto.DataDesconto.ToString("ddMMyy");
                _detalhe += Utils.FitStringLength(boleto.ValorDesconto.ApenasNumeros(), 13, 13, '0', 0, true, true, true);
                _detalhe += "0000000000000"; // Valor do IOF
                _detalhe += "0000000000000"; // Valor do Abatimento

                if (boleto.Sacado.CPFCNPJ.Length <= 11)
                    _detalhe += "01";  // CPF
                else
                    _detalhe += "02"; // CNPJ

                _detalhe += Utils.FitStringLength(boleto.Sacado.CPFCNPJ, 14, 14, '0', 0, true, true, true).ToUpper();
                _detalhe += Utils.FitStringLength(boleto.Sacado.Nome.TrimStart(' '), 30, 30, ' ', 0, true, true, false);
                _detalhe += usoBanco;
                _detalhe += Utils.FitStringLength(boleto.Sacado.Endereco.EndComNumeroEComplemento.TrimStart(' '), 40, 40, ' ', 0, true, true, false).ToUpper();
                _detalhe += Utils.FitStringLength(boleto.Sacado.Endereco.Bairro.TrimStart(' '), 12, 12, ' ', 0, true, true, false).ToUpper();
                _detalhe += Utils.FitStringLength(boleto.Sacado.Endereco.CEP, 8, 8, ' ', 0, true, true, false).ToUpper();
                ;
                _detalhe += Utils.FitStringLength(boleto.Sacado.Endereco.Cidade, 15, 15, ' ', 0, true, true, false).ToUpper();
                _detalhe += Utils.FitStringLength(boleto.Sacado.Endereco.UF, 2, 2, ' ', 0, true, true, false).ToUpper();
                
                // SACADOR/AVALISTA
                // Normalmente deve ser preenchido com o nome do sacador/avalista. Alternativamente este campo poder� 
                // ter dois outros usos:
                // a) 2o e 3o descontos: para de operar com mais de um desconto(depende de cadastramento pr�vio do 
                // indicador 19.0 pelo Banco Ita�, conforme item 5)
                // b) Mensagens ao sacado: se utilizados as instru��es 93 ou 94 (Nota 11), transcrever a mensagem desejada

                /* Su�lton - 18/12/2018 - 2 e 3 desconto por antecipa��o
                   Posi��o 352 a 353 : Brancos
                   Posi��o 354 a 359 : Data do 2� desconto (DDMMAA)
                   Posi��o 360 a 372 : Valor do 2� desconto
                   Posi��o 373 a 378 : Data do 3� desconto (DDMMAA)
                   Posi��o 379 a 391 : Valor do 3� desconto
                   Posi��o 392 a 394 : Brancos */

                if (boleto.DataDescontoAntecipacao2.HasValue || boleto.DataDescontoAntecipacao3.HasValue)
                {
                    _detalhe += "  "; //352 - 353
                    if (boleto.DataDescontoAntecipacao2.HasValue)
                    {
                        _detalhe += boleto.DataDescontoAntecipacao2.Value.ToString("ddMMyy") + 
                            Utils.FitStringLength(boleto.ValorDescontoAntecipacao2.Value.ApenasNumeros(), 13, 13, '0', 0, true, true, true);
                    }
                    else
                    {
                        _detalhe += "0000000000000000000";
                    }

                    if (boleto.DataDescontoAntecipacao3.HasValue)
                    {
                        _detalhe += boleto.DataDescontoAntecipacao3.Value.ToString("ddMMyy") + 
                            Utils.FitStringLength(boleto.ValorDescontoAntecipacao3.Value.ApenasNumeros(), 13, 13, '0', 0, true, true, true) + "00";
                    }
                    else
                    {
                        _detalhe += "0000000000000000000";
                    }

                    _detalhe += "   ";
                }
                else
                {
                    _detalhe += Utils.FitStringLength((boleto.Avalista == null ? "" : boleto.Avalista.Nome), 30, 30, ' ', 0, true, true, false).ToUpper();
                    _detalhe += "    "; // Complemento do registro
                    _detalhe += boleto.DataVencimento.ToString("ddMMyy"); //Data de Mora

                    if (boleto.Instrucoes.Count > 0)
                    {
                        for (int i = 0; i < boleto.Instrucoes.Count; i++)
                        {
                            if (boleto.Instrucoes[i].Codigo == (int)EnumInstrucoes_Itau.Protestar ||
                                boleto.Instrucoes[i].Codigo == (int)EnumInstrucoes_Itau.ProtestarAposNDiasCorridos ||
                                boleto.Instrucoes[i].Codigo == (int)EnumInstrucoes_Itau.ProtestarAposNDiasUteis)
                            {
                                _detalhe += boleto.Instrucoes[i].QuantidadeDias.ToString("00");
                                break;
                            }
                            else if (i == boleto.Instrucoes.Count - 1)
                                _detalhe += "00";
                        }
                    }
                    else
                    {
                        _detalhe += "00";
                    }

                    _detalhe += " "; // Complemento do registro
                }               
                
                
                _detalhe += Utils.FitStringLength(numeroRegistro.ToString(), 6, 6, '0', 0, true, true, true);

                _detalhe = Utils.SubstituiCaracteresEspeciais(_detalhe);

                return _detalhe;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar DETALHE do arquivo CNAB400.", ex);
            }
        }

        public string GerarRegistroDetalhe2(Boleto boleto, int numeroRegistro)
        {
            var dataMulta = boleto.DataMulta == DateTime.MinValue ? boleto.DataVencimento : boleto.DataMulta;
            StringBuilder detalhe = new StringBuilder();
            detalhe.Append("2");                                        // 001
            detalhe.Append("2");                                        // 002 VALOR EM PERCENTUAL
            detalhe.Append(dataMulta.ToString("ddMMyyyy"));             // 003-010
            detalhe.Append(Utils.FitStringLength(Convert.ToInt32(boleto.PercMulta * 100).ToString(), 13, 13, '0', 0, true, true, true)); // 011-023
            detalhe.Append(new string(' ', 371));                       // 024 a 394
            detalhe.Append(Utils.FitStringLength(numeroRegistro.ToString(), 6, 6, '0', 0, true, true, true)); // 395 a 400
            //Retorno
            return Utils.SubstituiCaracteresEspeciais(detalhe.ToString());
        }

        # endregion DETALHE

        # region TRAILER CNAB240

        /// <summary>
        ///POS INI/FINAL	DESCRI��O	                   A/N	TAM	DEC	CONTE�DO	NOTAS
        ///--------------------------------------------------------------------------------
        ///001 - 003	C�digo do Banco na compensa��o	    N	003		341	
        ///004 - 007	Lote de servi�o	                    N	004		Nota 5 
        ///008 - 008	Registro Trailer de Lote            N	001     5
        ///009 - 017	Complemento de Registros            A	009     Brancos
        ///018 - 023    Qtd. Registros do Lote              N   006     Nota 15     
        ///024 - 041    Soma Valor dos D�bitos do Lote      N   018     Nota 14     
        ///042 - 059    Soma Qtd. de Moedas do Lote         N   018     Nota 14
        ///060 - 230    Complemento de Registros            A   171     Brancos
        ///231 - 240    C�d. Ocr. para Retorno              A   010     Brancos
        /// </summary>

        public override string GerarTrailerLoteRemessa(int numeroRegistro)
        {
            try
            {
                string header = Utils.FormatCode(Codigo.ToString(), "0", 3, true);                      // c�digo do banco na compensa��o - 001-003 9(03) - 341
                header += "0001";                                                                       // Lote de Servi�o - 004-007 9(04) - Nota 1
                header += "5";                                                                          // Tipo de Registro - 008-008 9(01) - 5
                header += Utils.FormatCode("", " ", 9);                                                 // Complemento de Registro - 009-017 X(09) - Brancos
                header += Utils.FormatCode(numeroRegistro.ToString(), "0", 6, true);                    // Quantidade de Registros do Lote - 018-023 9(06) - Nota 26

                // Totaliza��o da Cobran�a Simples
                header += Utils.FormatCode("", "0", 6);                                                 // Quantidade de T�tulos em Cobran�a Simples - 024-029 9(06) - Nota 24
                header += Utils.FormatCode("", "0", 17);                                                // Valor Total dos T�tulos em Carteiras - 030-046 9(15)V9(02) - Nota 24

                //Totaliza��o cobran�a vinculada
                header += Utils.FormatCode("", "0", 6);                                                 // Qtde de titulos em cobran�a vinculada - 047-052 9(06) - Nota 24
                header += Utils.FormatCode("", "0", 17);                                                // Valor total t�tulos em cobran�a vinculada - 053-069 9(15)V9(02) - Nota 24

                header += Utils.FormatCode("", "0", 46);                                                // Complemento de Registro - 070-115 X(08) - Zeros
                header += Utils.FormatCode("", " ", 8);                                                 // Refer�ncia do Aviso banc�rio - 116-123 X(08) - Nota 25
                header += Utils.FormatCode("", " ", 117);                                               // Complemento de Registro - 124-240 X(117) - Brancos

                return header;
            }
            catch (Exception e)
            {
                throw new Exception("Erro ao gerar Trailer de Lote do arquivo de remessa.", e);
            }

            #region Informa��es geradas de forma inconsistente
            //suelton@gmail.com - 04/01/2017 - Gerando informa��es inconsistentes
            //try
            //{
            //    string trailer = Utils.FormatCode(Codigo.ToString(), "0", 3, true);
            //    trailer += Utils.FormatCode("", "0", 4, true);
            //    trailer += "5";
            //    trailer += Utils.FormatCode("", " ", 9);
            //    trailer += Utils.FormatCode("", "0", 6, true);
            //    trailer += Utils.FormatCode("", "0", 18, true);
            //    trailer += Utils.FormatCode("", "0", 18, true);
            //    trailer += Utils.FormatCode("", " ", 171);
            //    trailer += Utils.FormatCode("", " ", 10);
            //    trailer = Utils.SubstituiCaracteresEspeciais(trailer);

            //    return trailer;
            //}
            //catch (Exception e)
            //{
            //    throw new Exception("Erro durante a gera��o do registro TRAILER do LOTE de REMESSA.", e);
            //} 
            #endregion
        }

        /// <summary>
        ///POS INI/FINAL	DESCRI��O	                   A/N	TAM	DEC	CONTE�DO	NOTAS
        ///--------------------------------------------------------------------------------
        ///001 - 003	C�digo do Banco na compensa��o	    N	003		341	
        ///004 - 007	Lote de servi�o	                    N	004		9999 
        ///008 - 008	Registro Trailer de Arquivo         N	001     9
        ///009 - 017	Complemento de Registros            A	009     Brancos
        ///018 - 023    Qtd. Lotes do Arquivo               N   006     Nota 15     
        ///024 - 029    Qtd. Registros do Arquivo           N   006     Nota 15     
        ///030 - 240    Complemento de Registros            A   211     Brancos
        /// </summary>

        public override string GerarTrailerArquivoRemessa(int numeroRegistro)
        {
            try
            {
                string header = Utils.FormatCode(Codigo.ToString(), "0", 3, true);                      // c�digo do banco na compensa��o - 001-003 (03) - 341
                header += "9999";                                                                       // Lote de Servi�o - 004-007 9(04) - '9999'
                header += "9";                                                                          // Tipo de Registro - 008-008 9(1) - '9'
                header += Utils.FormatCode("", " ", 9);                                                 // Complemento de Registro - 009-017 X(09) - Brancos
                header += "000001";                                                                     // Quantidade de Lotes do Arquivo - 018-023 9(06) - Nota 26
                header += Utils.FormatCode(numeroRegistro.ToString(), "0", 6, true);                    // Quantidade de Registros do Arquivo - 024-029 9(06) - Nota 26
                header += Utils.FormatCode("", "0", 6);                                                 // Complemento de Registro - 030-035 9(06)
                header += Utils.FormatCode("", " ", 205);                                               // Complemento de Registro - 036-240 X(205) - Brancos

                return header;
            }
            catch (Exception e)
            {
                throw new Exception("Erro ao gerar Trailer de arquivo de remessa.", e);
            }           
        }
        #endregion

        # region TRAILER CNAB400

        /// <summary>
        /// TRAILER do arquivo CNAB
        /// Gera o TRAILER do arquivo remessa de acordo com o lay-out informado
        /// </summary>
        public override string GerarTrailerRemessa(int numeroRegistro, TipoArquivo tipoArquivo, Cedente cedente, decimal vltitulostotal)
        {
            try
            {
                string _trailer = " ";

                base.GerarTrailerRemessa(numeroRegistro, tipoArquivo, cedente, vltitulostotal);

                switch (tipoArquivo)
                {
                    case TipoArquivo.CNAB240:
                        _trailer = GerarTrailerRemessa240();
                        break;
                    case TipoArquivo.CNAB400:
                        _trailer = GerarTrailerRemessa400(numeroRegistro);
                        break;
                    case TipoArquivo.Outro:
                        throw new Exception("Tipo de arquivo inexistente.");
                }

                return _trailer;

            }
            catch (Exception ex)
            {
                throw new Exception("", ex);
            }
        }

        public string GerarTrailerRemessa240()
        {
            throw new NotImplementedException("Fun��o n�o implementada.");
        }

        public string GerarTrailerRemessa400(int numeroRegistro)
        {
            try
            {
                string complemento = new string(' ', 393);
                string _trailer;

                _trailer = "9";
                _trailer += complemento;
                _trailer += Utils.FitStringLength(numeroRegistro.ToString(), 6, 6, '0', 0, true, true, true); // N�mero sequencial do registro no arquivo.

                _trailer = Utils.SubstituiCaracteresEspeciais(_trailer);

                return _trailer;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro durante a gera��o do registro TRAILER do arquivo de REMESSA.", ex);
            }
        }

        # endregion

        /// <summary>
        /// DETALHE do arquivo CNAB
        /// Gera o DETALHE do arquivo remessa de acordo com o lay-out informado
        /// </summary>

        public override string GerarMensagemVariavelRemessa(Boleto boleto, ref int numeroRegistro, TipoArquivo tipoArquivo)
        {
            try
            {
                string _detalhe = "";

                switch (tipoArquivo)
                {
                    case TipoArquivo.CNAB240:
                        throw new Exception("Mensagem Variavel nao existe para o tipo CNAB 240.");
                    case TipoArquivo.CNAB400:
                        //Comentado por diego.dariolli pois o registro tipo 2 do ita� � somente referente � multa
                        //_detalhe = GerarMensagemVariavelRemessaCNAB400(boleto, ref numeroRegistro, tipoArquivo);
                        _detalhe = string.Empty;
                        break;
                    case TipoArquivo.Outro:
                        throw new Exception("Tipo de arquivo inexistente.");
                }

                return _detalhe;

            }
            catch (Exception ex)
            {
                throw new Exception("Erro durante a gera��o do DETALHE arquivo de REMESSA.", ex);
            }
        }

        public string GerarMensagemVariavelRemessaCNAB400(Boleto boleto, ref int numeroRegistro, TipoArquivo tipoArquivo)
        {
            try
            {
                string _registroOpcional = "";
                //detalhe                           (tamanho,tipo) A= Alfanumerico, N= Numerico
                _registroOpcional = "2"; //Identifica��o do Registro         (1, N)

                //Mensagem 1 (80, A)
                if (boleto.Instrucoes != null && boleto.Instrucoes.Count > 0)
                    _registroOpcional += boleto.Instrucoes[0].Descricao.PadRight(80, ' ').Substring(0, 80);
                else
                    _registroOpcional += new string(' ', 80);

                //Mensagem 2 (80, A)
                if (boleto.Instrucoes != null && boleto.Instrucoes.Count > 1)
                    _registroOpcional += boleto.Instrucoes[1].Descricao.PadRight(80, ' ').Substring(0, 80);
                else
                    _registroOpcional += new string(' ', 80);

                //Mensagem 3 (80, A)
                if (boleto.Instrucoes != null && boleto.Instrucoes.Count > 2)
                    _registroOpcional += boleto.Instrucoes[2].Descricao.PadRight(80, ' ').Substring(0, 80);
                else
                    _registroOpcional += new string(' ', 80);

                //Mensagem 4 (80, A)
                if (boleto.Instrucoes != null && boleto.Instrucoes.Count > 3)
                    _registroOpcional += boleto.Instrucoes[3].Descricao.PadRight(80, ' ').Substring(0, 80);
                else
                    _registroOpcional += new string(' ', 80);

                _registroOpcional += new string(' ', 6); //Data limite para concess�o de Desconto 2 (6, N) DDMMAA
                _registroOpcional += new string(' ', 13);//Valor do Desconto (13, N) 
                _registroOpcional += new string(' ', 6);//Data limite para concess�o de Desconto 3 (6, N) DDMMAA
                _registroOpcional += new string(' ', 13);//Valor do Desconto (13, N)
                _registroOpcional += new string(' ', 7);//Reserva (7, A)
                _registroOpcional += Utils.FitStringLength(boleto.Carteira, 3, 3, '0', 0, true, true, true); //Carteira (3, N)
                _registroOpcional += Utils.FitStringLength(boleto.Cedente.ContaBancaria.Agencia, 5, 5, '0', 0, true, true, true); //Ag�ncia (5, N) 
                _registroOpcional += Utils.FitStringLength(boleto.Cedente.ContaBancaria.Conta, 7, 7, '0', 0, true, true, true); //Conta Corrente (7, N)
                _registroOpcional += Utils.FitStringLength(boleto.Cedente.ContaBancaria.DigitoConta, 1, 1, '0', 0, true, true, true); //D�gito C/C (1, A)
                _registroOpcional += Utils.FitStringLength(boleto.NossoNumero, 11, 11, '0', 0, true, true, true); //Nosso N�mero (11, N)
                _registroOpcional += Utils.FitStringLength("0", 1, 1, '0', 0, true, true, true); //DAC Nosso N�mero (1, A)

                //N� Seq�encial do Registro (06, N)
                _registroOpcional += Utils.FitStringLength(numeroRegistro.ToString(), 6, 6, '0', 0, true, true, true);

                _registroOpcional = Utils.SubstituiCaracteresEspeciais(_registroOpcional);

                return _registroOpcional;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar REGISTRO OPCIONAL do arquivo CNAB400.", ex);
            }
        }
        #endregion

        #region M�todos de processamento do arquivo retorno CNAB400

        public override DetalheRetorno LerDetalheRetornoCNAB400(string registro)
        {
            try
            {
                int dataOcorrencia = Utils.ToInt32(registro.Substring(110, 6));
                int dataVencimento = Utils.ToInt32(registro.Substring(146, 6));
                int dataCredito = Utils.ToInt32(registro.Substring(295, 6));

                DetalheRetorno detalhe = new DetalheRetorno(registro);

                detalhe.CodigoInscricao = Utils.ToInt32(registro.Substring(1, 2));
                detalhe.NumeroInscricao = registro.Substring(3, 14);
                detalhe.Agencia = Utils.ToInt32(registro.Substring(17, 4));
                detalhe.Conta = Utils.ToInt32(registro.Substring(23, 5));
                detalhe.DACConta = Utils.ToInt32(registro.Substring(28, 1));
                detalhe.UsoEmpresa = registro.Substring(37, 25);
                //
                detalhe.Carteira = registro.Substring(82, 1); // adicionado por Heric Souza em 20/06/16
                detalhe.NossoNumeroComDV = registro.Substring(85, 9);
                detalhe.NossoNumero = registro.Substring(85, 8); //Sem o DV
                detalhe.DACNossoNumero = registro.Substring(93, 1); //DV
                //
                //detalhe.Carteira = registro.Substring(107, 1); // comentado por Heric Souza em 20 / 06 / 16
                detalhe.CodigoOcorrencia = Utils.ToInt32(registro.Substring(108, 2));

                //Descri��o da ocorr�ncia
                detalhe.DescricaoOcorrencia = this.Ocorrencia(registro.Substring(108, 2));

                detalhe.DataOcorrencia = Utils.ToDateTime(dataOcorrencia.ToString("##-##-##"));
                detalhe.NumeroDocumento = registro.Substring(116, 10);
                //
                detalhe.DataVencimento = Utils.ToDateTime(dataVencimento.ToString("##-##-##"));
                decimal valorTitulo = Convert.ToInt64(registro.Substring(152, 13));
                detalhe.ValorTitulo = valorTitulo / 100;
                detalhe.CodigoBanco = Utils.ToInt32(registro.Substring(165, 3));
                detalhe.BancoCobrador = Utils.ToInt32(registro.Substring(165, 3));
                detalhe.AgenciaCobradora = Utils.ToInt32(registro.Substring(168, 4));
                detalhe.Especie = Utils.ToInt32(registro.Substring(173, 2));
                decimal tarifaCobranca = Convert.ToUInt64(registro.Substring(175, 13));
                detalhe.TarifaCobranca = tarifaCobranca / 100;
                // 26 brancos
                decimal iof = Convert.ToUInt64(registro.Substring(214, 13));
                detalhe.IOF = iof / 100;
                decimal valorAbatimento = !String.IsNullOrWhiteSpace(registro.Substring(227, 13)) ? Convert.ToUInt64(registro.Substring(227, 13)) : 0;
                detalhe.ValorAbatimento = valorAbatimento / 100;

                decimal valorDescontos = Convert.ToUInt64(registro.Substring(240, 13));
                detalhe.Descontos = valorDescontos / 100;

                decimal valorPrincipal = Convert.ToUInt64(registro.Substring(253, 13));
                detalhe.ValorPrincipal = valorPrincipal / 100;

                decimal jurosMora = Convert.ToUInt64(registro.Substring(266, 13));
                detalhe.JurosMora = jurosMora / 100;
                // 293 - 3 brancos
                detalhe.DataCredito = Utils.ToDateTime(dataCredito.ToString("##-##-##"));
                detalhe.InstrucaoCancelada = Utils.ToInt32(registro.Substring(301, 4));
                // 306 - 6 brancos
                // 311 - 13 zeros
                detalhe.NomeSacado = registro.Substring(324, 30);
                // 354 - 23 brancos
                detalhe.Erros = registro.Substring(377, 8);

                if (!string.IsNullOrWhiteSpace(detalhe.Erros))
                {
                    string detalheErro = detalhe.Erros;

                    var motivo1 = MotivoRejeicao(detalhe.Erros.Substring(0, 2));
                    var motivo2 = MotivoRejeicao(detalhe.Erros.Substring(2, 2));
                    var motivo3 = MotivoRejeicao(detalhe.Erros.Substring(4, 2));

                    if (!string.IsNullOrWhiteSpace(motivo1))
                        detalheErro += " - " + motivo1;

                    if (!string.IsNullOrWhiteSpace(motivo2))
                        detalheErro += " / " + motivo2;

                    if (!string.IsNullOrWhiteSpace(motivo3))
                        detalheErro += " / " + motivo3;

                    detalhe.Erros = detalheErro;
                }

                // 377 - Registros rejeitados ou alega��o do sacado
                // 386 - 7 brancos

                detalhe.CodigoLiquidacao = registro.Substring(392, 2);
                detalhe.NumeroSequencial = Utils.ToInt32(registro.Substring(394, 6));
                detalhe.ValorPago = detalhe.ValorPrincipal;

                // A correspond�ncia de Valor Pago no RETORNO ITA� � o Valor Principal (Valor lan�ado em Conta Corrente - Conforme Manual)
                // A determina��o se D�bito ou Cr�dito dever� ser feita nos aplicativos por se tratar de personaliza��o.
                // Para isso, considerar o C�digo da Ocorr�ncia e tratar de acordo com suas necessidades.
                // Alterado por jsoda em 04/06/2012
                //
                //// Valor principal � d�bito ou cr�dito ?
                //if ( (detalhe.ValorTitulo < detalhe.TarifaCobranca) ||
                //     ((detalhe.ValorTitulo - detalhe.Descontos) < detalhe.TarifaCobranca)
                //    )
                //{
                //    detalhe.ValorPrincipal *= -1; // Para d�bito coloca valor negativo
                //}


                //// Valor Pago � a soma do Valor Principal (Valor que entra na conta) + Tarifa de Cobran�a
                //detalhe.ValorPago = detalhe.ValorPrincipal + detalhe.TarifaCobranca;

                return detalhe;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao ler detalhe do arquivo de RETORNO / CNAB 400.", ex);
            }
        }

        public override HeaderRetorno LerHeaderRetornoCNAB400(string registro)
        {
            try
            {
                HeaderRetorno header = new HeaderRetorno(registro);
                header.TipoRegistro = Utils.ToInt32(registro.Substring(000, 1));
                header.CodigoRetorno = Utils.ToInt32(registro.Substring(001, 1));
                header.LiteralRetorno = registro.Substring(002, 7);
                header.CodigoServico = Utils.ToInt32(registro.Substring(009, 2));
                header.LiteralServico = registro.Substring(011, 15);
                header.Agencia = Utils.ToInt32(registro.Substring(026, 4));
                header.ComplementoRegistro1 = Utils.ToInt32(registro.Substring(030, 2));
                header.Conta = Utils.ToInt32(registro.Substring(032, 5));
                header.DACConta = Utils.ToInt32(registro.Substring(037, 1));
                header.ComplementoRegistro2 = registro.Substring(038, 8);
                header.NomeEmpresa = registro.Substring(046, 30);
                header.CodigoBanco = Utils.ToInt32(registro.Substring(076, 3));
                header.NomeBanco = registro.Substring(079, 15);
                header.DataGeracao = Utils.ToDateTime(Utils.ToInt32(registro.Substring(094, 6)).ToString("##-##-##"));
                header.Densidade = Utils.ToInt32(registro.Substring(100, 5));
                header.UnidadeDensidade = registro.Substring(105, 3);
                header.NumeroSequencialArquivoRetorno = Utils.ToInt32(registro.Substring(108, 5));
                header.DataCredito = Utils.ToDateTime(Utils.ToInt32(registro.Substring(113, 6)).ToString("##-##-##"));
                header.ComplementoRegistro3 = registro.Substring(119, 275);
                header.NumeroSequencial = Utils.ToInt32(registro.Substring(394, 6));
                return header;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao ler header do arquivo de RETORNO / CNAB 400.", ex);
            }
        }
        #endregion

        #region ::. Arquivo de Retorno CNAB240 .::
        public override DetalheSegmentoTRetornoCNAB240 LerDetalheSegmentoTRetornoCNAB240(string registro)
        {
            try
            {
                DetalheSegmentoTRetornoCNAB240 detalhe = new DetalheSegmentoTRetornoCNAB240(registro);

                if (registro.Substring(13, 1) != "T")
                    throw new Exception("Registro inv�lido. O detalhe n�o possu� as caracter�sticas do segmento T.");

                detalhe.CodigoBanco = Convert.ToInt32(registro.Substring(0, 3));
                detalhe.idCodigoMovimento = Convert.ToInt32(registro.Substring(15, 2));
                detalhe.Agencia = Convert.ToInt32(registro.Substring(18, 4));
                detalhe.DigitoAgencia = "0";
                detalhe.Conta = Convert.ToInt32(registro.Substring(30, 5));
                detalhe.DigitoConta = registro.Substring(36, 1);
                detalhe.NossoNumero = registro.Substring(40, 9);
                detalhe.CodigoCarteira = Convert.ToInt32(registro.Substring(37, 3));
                detalhe.NumeroDocumento = registro.Substring(58, 10);
                int dataVencimento = Convert.ToInt32(registro.Substring(73, 8));

                if (dataVencimento != 0) //Quando vem somente tarifas sobre (ex. Ocorrencia 54 - TARIFA MENSAL DE LIQUIDA��ES NA CARTEIRA), este campo vem 0
                    detalhe.DataVencimento = Convert.ToDateTime(dataVencimento.ToString("##-##-####"));
                else
                    detalhe.DataVencimento = DateTime.Now.Date;

                decimal valorTitulo = Convert.ToInt64(registro.Substring(81, 15));
                detalhe.ValorTitulo = valorTitulo / 100;
                detalhe.IdentificacaoTituloEmpresa = registro.Substring(105, 25);
                detalhe.TipoInscricao = Convert.ToInt32(registro.Substring(132, 1));
                detalhe.NumeroInscricao = registro.Substring(133, 15);
                detalhe.NomeSacado = registro.Substring(148, 30);
                decimal valorTarifas = Convert.ToUInt64(registro.Substring(198, 15));
                detalhe.ValorTarifas = valorTarifas / 100;

                return detalhe;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao processar arquivo de RETORNO - SEGMENTO T.", ex);
            }


        }

        public override DetalheSegmentoURetornoCNAB240 LerDetalheSegmentoURetornoCNAB240(string registro)
        {
            try
            {
                DetalheSegmentoURetornoCNAB240 detalhe = new DetalheSegmentoURetornoCNAB240(registro);

                if (registro.Substring(13, 1) != "U")
                    throw new Exception("Registro inv�lido. O detalhe n�o possu� as caracter�sticas do segmento U.");

                detalhe.CodigoOcorrenciaSacado = registro.Substring(15, 2);
                int DataCredito = Convert.ToInt32(registro.Substring(145, 8));
                detalhe.DataCredito = (DataCredito > 0) ? Convert.ToDateTime(DataCredito.ToString("##-##-####")) : new DateTime();
                int DataOcorrencia = Convert.ToInt32(registro.Substring(137, 8));
                detalhe.DataOcorrencia = (DataOcorrencia > 0) ? Convert.ToDateTime(DataOcorrencia.ToString("##-##-####")) : new DateTime();
                int DataOcorrenciaSacado = Convert.ToInt32(registro.Substring(157, 8));
                if (DataOcorrenciaSacado > 0)
                    detalhe.DataOcorrenciaSacado = Convert.ToDateTime(DataOcorrenciaSacado.ToString("##-##-####"));
                else
                    detalhe.DataOcorrenciaSacado = DateTime.Now;

                decimal JurosMultaEncargos = Convert.ToUInt64(registro.Substring(17, 15));
                detalhe.JurosMultaEncargos = JurosMultaEncargos / 100;
                decimal ValorDescontoConcedido = Convert.ToUInt64(registro.Substring(32, 15));
                detalhe.ValorDescontoConcedido = ValorDescontoConcedido / 100;
                decimal ValorAbatimentoConcedido = Convert.ToUInt64(registro.Substring(47, 15));
                detalhe.ValorAbatimentoConcedido = ValorAbatimentoConcedido / 100;
                decimal ValorIOFRecolhido = Convert.ToUInt64(registro.Substring(62, 15));
                detalhe.ValorIOFRecolhido = ValorIOFRecolhido / 100;
                decimal ValorPagoPeloSacado = Convert.ToUInt64(registro.Substring(77, 15));
                detalhe.ValorPagoPeloSacado = ValorPagoPeloSacado / 100;
                decimal ValorLiquidoASerCreditado = Convert.ToUInt64(registro.Substring(92, 15));
                detalhe.ValorLiquidoASerCreditado = ValorLiquidoASerCreditado / 100;
                decimal ValorOutrasDespesas = Convert.ToUInt64(registro.Substring(107, 15));
                detalhe.ValorOutrasDespesas = ValorOutrasDespesas / 100;

                return detalhe;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao processar arquivo de RETORNO - SEGMENTO U.", ex);
            }


        }
        #endregion

        /// <summary>
        /// Efetua as Valida��es dentro da classe Boleto, para garantir a gera��o da remessa
        /// </summary>
        public override bool ValidarRemessa(TipoArquivo tipoArquivo, string numeroConvenio, IBanco banco, Cedente cedente, Boletos boletos, int numeroArquivoRemessa, out string mensagem)
        {
            bool vRetorno = true;
            string vMsg = string.Empty;
            ////IMPLEMENTACAO PENDENTE...
            mensagem = vMsg;
            return vRetorno;
        }

        public override long ObterNossoNumeroSemConvenioOuDigitoVerificador(long convenio, string nossoNumero)
        {
            //Ita� n�o utiliza DV ou conv�nio com o nosso n�mero.
            long numero;
            if (long.TryParse(nossoNumero, out numero))
                return numero;
            throw new NossoNumeroInvalidoException();
        }

        public override string GerarNomeRemessa(Cedente cedente, string cidadeBanco, int remessa)
        {
            return $"REM_{cedente.ContaBancaria.Agencia}{cidadeBanco}_TER_{cedente.Codigo}{cedente.DigitoCedente}_{remessa.ToString(CultureInfo.InvariantCulture).PadLeft(6, '0')}_C400.txt";
        }
    }
}
