using BoletoNet.EDI.Banco;
using BoletoNet.Excecoes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;

[assembly: WebResource("BoletoNet.Imagens.748.jpg", "image/jpg")]
namespace BoletoNet
{
    /// <Author>
    /// Samuel Schmidt - Sicredi Nordeste RS / Felipe Eduardo - RS
    /// </Author>
    internal class Banco_Sicredi : AbstractBanco, IBanco
    {
        private static readonly Dictionary<int, string> carteirasDisponiveis = new Dictionary<int, string>() {
            { 1, "Com Registro" },
            { 3, "Sem Registro" }
        };

        private HeaderRetorno header;

        /// <author>
        /// Classe responsavel em criar os campos do Banco Sicredi.
        /// </author>
        internal Banco_Sicredi()
        {
            this.Codigo = 748;
            this.Digito = "X";
            this.Nome = "Banco Sicredi";
        }

        public override void ValidaBoleto(Boleto boleto)
        {
            //Formata o tamanho do numero da Agencia
            if (boleto.Cedente.ContaBancaria.Agencia.Length < 4)
                boleto.Cedente.ContaBancaria.Agencia = Utils.FormatCode(boleto.Cedente.ContaBancaria.Agencia, 4);

            //Formata o tamanho do numero da conta corrente
            if (boleto.Cedente.ContaBancaria.Conta.Length < 5)
                boleto.Cedente.ContaBancaria.Conta = Utils.FormatCode(boleto.Cedente.ContaBancaria.Conta, 5);

            //Atribui o nome do banco ao local de pagamento
            if (boleto.LocalPagamento == "Até o vencimento, preferencialmente no ")
                boleto.LocalPagamento += Nome;
            else boleto.LocalPagamento = "PAGÁVEL PREFERENCIALMENTE NAS COOPERATIVAS DE CRÉDITO DO SICREDI";

            //Verifica se data do processamento e valida
            if (boleto.DataProcessamento == DateTime.MinValue) // diegomodolo (diego.ribeiro@nectarnet.com.br)
                boleto.DataProcessamento = DateTime.Now;

            //Verifica se data do documento e valida
            if (boleto.DataDocumento == DateTime.MinValue) // diegomodolo (diego.ribeiro@nectarnet.com.br)
                boleto.DataDocumento = DateTime.Now;

            string infoFormatoCodigoCedente = "formato AAAAPPCCCCC, onde: AAAA = numero da Agencia, PP = Posto do beneficiário, CCCCC = Codigo do beneficiário";

            var codigoCedente = Utils.FormatCode(boleto.Cedente.Codigo, 11);

            if (string.IsNullOrEmpty(codigoCedente))
                throw new BoletoNetException("Codigo do cedente deve ser informado, " + infoFormatoCodigoCedente);

            var conta = boleto.Cedente.ContaBancaria.Conta;
            if (boleto.Cedente.ContaBancaria != null &&
                (!codigoCedente.StartsWith(boleto.Cedente.ContaBancaria.Agencia) ||
                 !(codigoCedente.EndsWith(conta) || codigoCedente.EndsWith(conta.Substring(0, conta.Length - 1)))))
                //throw new BoletoNetException("Codigo do cedente deve estar no " + infoFormatoCodigoCedente);
                boleto.Cedente.Codigo = string.Format("{0}{1}{2}", boleto.Cedente.ContaBancaria.Agencia, boleto.Cedente.ContaBancaria.OperacaConta, Utils.Right((boleto.Cedente.Codigo), 5, '0', true));

            if (string.IsNullOrEmpty(boleto.Carteira))
                throw new BoletoNetException("Tipo de carteira é obrigatorio. " + ObterInformacoesCarteirasDisponiveis());

            if (!CarteiraValida(boleto.Carteira))
                throw new BoletoNetException("Carteira informada é inválida. Informe " + ObterInformacoesCarteirasDisponiveis());

            //Verifica se o nosso numero e valido
            boleto.NossoNumero = boleto.NossoNumero.PadLeft(5, '0');
            boleto.NossoNumero = "2" + boleto.NossoNumero;
            var Length_NN = boleto.NossoNumero.Length;
            switch (Length_NN)
            {
                case 9:
                    boleto.NossoNumero = boleto.NossoNumero.Substring(0, Length_NN - 1);
                    boleto.DigitoNossoNumero = DigNossoNumeroSicredi(boleto);
                    boleto.NossoNumero += boleto.DigitoNossoNumero;
                    break;
                case 8:
                    boleto.DigitoNossoNumero = DigNossoNumeroSicredi(boleto);
                    boleto.NossoNumero += boleto.DigitoNossoNumero;
                    break;
                case 6:
                    boleto.NossoNumero = DateTime.Now.ToString("yy") + boleto.NossoNumero;
                    boleto.DigitoNossoNumero = DigNossoNumeroSicredi(boleto);
                    boleto.NossoNumero += boleto.DigitoNossoNumero;
                    break;
                default:
                    throw new NotImplementedException("Nosso numero inválido");
            }

            FormataCodigoBarra(boleto);
            if (boleto.CodigoBarra.Codigo.Length != 44)
                throw new BoletoNetException("Codigo de barras é inválido");

            FormataLinhaDigitavel(boleto);

            string nossoNumero = boleto.NossoNumero;

            if (nossoNumero == null || nossoNumero.Length != 9)
            {
                throw new Exception("Erro ao tentar formatar nosso numero, verifique o tamanho do campo");
            }

            try
            {
                boleto.NossoNumero = string.Format("{0}/{1}-{2}", nossoNumero.Substring(0, 2), nossoNumero.Substring(2, 6), nossoNumero.Substring(8));
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao formatar nosso numero", ex);
            }
        }

        private string ObterInformacoesCarteirasDisponiveis()
        {
            return string.Join(", ", carteirasDisponiveis.Select(o => string.Format("“{0}” – {1}", o.Key, o.Value)));
        }

        private bool CarteiraValida(string carteira)
        {
            int tipoCarteira;
            if (int.TryParse(carteira, out tipoCarteira))
            {
                return carteirasDisponiveis.ContainsKey(tipoCarteira);
            }
            return false;
        }

        public override void FormataNossoNumero(Boleto boleto)
        {
            boleto.NossoNumero = boleto.NossoNumero.PadLeft(5, '0');
            boleto.NossoNumero = "2" + boleto.NossoNumero;
            var Length_NN = boleto.NossoNumero.Length;
            switch (Length_NN)
            {
                case 9:
                    boleto.NossoNumero = boleto.NossoNumero.Substring(0, Length_NN - 1);
                    boleto.DigitoNossoNumero = DigNossoNumeroSicredi(boleto, true);
                    boleto.NossoNumero += boleto.DigitoNossoNumero;
                    break;
                case 8:
                    boleto.DigitoNossoNumero = DigNossoNumeroSicredi(boleto, true);
                    boleto.NossoNumero += boleto.DigitoNossoNumero;
                    break;
                case 6:
                    boleto.NossoNumero = DateTime.Now.ToString("yy") + boleto.NossoNumero;
                    boleto.DigitoNossoNumero = DigNossoNumeroSicredi(boleto, true);
                    boleto.NossoNumero += boleto.DigitoNossoNumero;
                    break;
                default:
                    throw new NotImplementedException("Nosso numero inválido");
            }

            string nossoNumero = boleto.NossoNumero;

            if (nossoNumero == null || nossoNumero.Length != 9)
            {
                throw new Exception("Erro ao tentar formatar nosso numero, verifique o tamanho do campo");
            }

            try
            {
                boleto.NossoNumero = string.Format("{0}/{1}-{2}", nossoNumero.Substring(0, 2), nossoNumero.Substring(2, 6), nossoNumero.Substring(8));
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao formatar nosso numero", ex);
            }
        }

        public override void FormataNumeroDocumento(Boleto boleto)
        {
            throw new NotImplementedException("Função do fomata numero do documento nao implementada.");
        }
        public override void FormataLinhaDigitavel(Boleto boleto)
        {
            //041M2.1AAAd1  CCCCC.CCNNNd2  NNNNN.041XXd3  V FFFF9999999999

            string campo1 = "7489" + boleto.CodigoBarra.Codigo.Substring(19, 5);
            int d1 = Mod10Sicredi(campo1);
            campo1 = FormataCampoLD(campo1) + d1.ToString();

            string campo2 = boleto.CodigoBarra.Codigo.Substring(24, 10);
            int d2 = Mod10Sicredi(campo2);
            campo2 = FormataCampoLD(campo2) + d2.ToString();

            string campo3 = boleto.CodigoBarra.Codigo.Substring(34, 10);
            int d3 = Mod10Sicredi(campo3);
            campo3 = FormataCampoLD(campo3) + d3.ToString();

            string campo4 = boleto.CodigoBarra.Codigo.Substring(4, 1);

            string campo5 = boleto.CodigoBarra.Codigo.Substring(5, 14);

            boleto.CodigoBarra.LinhaDigitavel = campo1 + "  " + campo2 + "  " + campo3 + "  " + campo4 + "  " + campo5;
        }
        private string FormataCampoLD(string campo)
        {
            return string.Format("{0}.{1}", campo.Substring(0, 5), campo.Substring(5));
        }

        public override void FormataCodigoBarra(Boleto boleto)
        {
            string valorBoleto = boleto.ValorBoleto.ToString("f").Replace(",", "").Replace(".", "");
            valorBoleto = Utils.FormatCode(valorBoleto, 10);

            var codigoCobranca = 1; //Codigo de Cobranca com registro
            string cmp_livre =
                codigoCobranca +
                boleto.Carteira +
                Utils.FormatCode(boleto.NossoNumero, 9) +
                Utils.FormatCode(boleto.Cedente.Codigo, 11) + "10";

            string dv_cmpLivre = digSicredi(cmp_livre).ToString();

            var codigoTemp = GerarCodigoDeBarras(boleto, valorBoleto, cmp_livre, dv_cmpLivre);

            boleto.CodigoBarra.CampoLivre = cmp_livre;
            boleto.CodigoBarra.FatorVencimento = FatorVencimento(boleto);
            boleto.CodigoBarra.Moeda = 9;
            boleto.CodigoBarra.ValorDocumento = valorBoleto;

            int _dacBoleto = digSicredi(codigoTemp);

            if (_dacBoleto == 0 || _dacBoleto > 9)
                _dacBoleto = 1;

            boleto.CodigoBarra.Codigo = GerarCodigoDeBarras(boleto, valorBoleto, cmp_livre, dv_cmpLivre, _dacBoleto);
        }

        private string GerarCodigoDeBarras(Boleto boleto, string valorBoleto, string cmp_livre, string dv_cmpLivre, int? dv_geral = null)
        {
            return string.Format("{0}{1}{2}{3}{4}{5}{6}",
                Utils.FormatCode(Codigo.ToString(), 3),
                boleto.Moeda,
                dv_geral.HasValue ? dv_geral.Value.ToString() : string.Empty,
                FatorVencimento(boleto),
                valorBoleto,
                cmp_livre,
                dv_cmpLivre);
        }

        //public bool RegistroByCarteira(Boleto boleto)
        //{
        //    bool valida = false;
        //    if (boleto.Carteira == "112"
        //        || boleto.Carteira == "115"
        //        || boleto.Carteira == "104"
        //        || boleto.Carteira == "147"
        //        || boleto.Carteira == "188"
        //        || boleto.Carteira == "108"
        //        || boleto.Carteira == "109"
        //        || boleto.Carteira == "150"
        //        || boleto.Carteira == "121")
        //        valida = true;
        //    return valida;
        //}

        #region Metodos de Geracao do Arquivo de Remessa
        public override string GerarDetalheRemessa(Boleto boleto, int numeroRegistro, TipoArquivo tipoArquivo)
        {
            try
            {
                string _detalhe = " ";

                //base.GerarDetalheRemessa(boleto, numeroRegistro, tipoArquivo);

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
                throw new Exception("Erro durante a geracao do DETALHE arquivo de REMESSA.", ex);
            }
        }
        public override string GerarHeaderRemessa(string numeroConvenio, Cedente cedente, TipoArquivo tipoArquivo, int numeroArquivoRemessa, Boleto boletos)
        {
            throw new NotImplementedException("Função nao implementada.");
        }
        public string GerarDetalheRemessaCNAB240(Boleto boleto, int numeroRegistro, TipoArquivo tipoArquivo)
        {
            try
            {
                string detalhe = Utils.FormatCode(Codigo.ToString(), "0", 3, true);
                detalhe += Utils.FormatCode("", "0", 4, true);
                detalhe += "3";
                detalhe += Utils.FormatCode(numeroRegistro.ToString(), 5);
                detalhe += "P 01";
                detalhe += Utils.FormatCode(boleto.Cedente.ContaBancaria.Agencia, 5);
                detalhe += "0";
                detalhe += Utils.FormatCode(boleto.Cedente.ContaBancaria.Conta, 12);
                detalhe += boleto.Cedente.ContaBancaria.DigitoConta;
                detalhe += " ";
                detalhe += Utils.FormatCode(boleto.NossoNumero.Replace("/", "").Replace("-", ""), 20);
                detalhe += "1";
                detalhe += (Convert.ToInt16(boleto.Carteira) == 1 ? "1" : "2");
                detalhe += "122";
                detalhe += Utils.FormatCode(boleto.NumeroDocumento, 15);
                detalhe += boleto.DataVencimento.ToString("ddMMyyyy");
                string valorBoleto = boleto.ValorBoleto.ToString("f").Replace(",", "").Replace(".", "");
                valorBoleto = Utils.FormatCode(valorBoleto, 13);
                detalhe += valorBoleto;
                detalhe += "00000 99A";
                detalhe += boleto.DataDocumento.ToString("ddMMyyyy");
                detalhe += "200000000";
                valorBoleto = boleto.JurosMora.ToString("f").Replace(",", "").Replace(".", "");
                valorBoleto = Utils.FormatCode(valorBoleto, 13);
                detalhe += valorBoleto;
                detalhe += "1";
                detalhe += boleto.DataDesconto.ToString("ddMMyyyy");
                valorBoleto = boleto.ValorDesconto.ToString("f").Replace(",", "").Replace(".", "");
                valorBoleto = Utils.FormatCode(valorBoleto, 13);
                detalhe += valorBoleto;
                detalhe += Utils.FormatCode("", 26);
                detalhe += Utils.FormatCode("", " ", 25);
                detalhe += "0001060090000000000 ";

                detalhe = Utils.SubstituiCaracteresEspeciais(detalhe);
                return detalhe;
            }
            catch (Exception e)
            {
                throw new Exception("Erro ao gerar DETALHE do arquivo CNAB240.", e);
            }
        }

        public override string GerarHeaderRemessa(Cedente cedente, TipoArquivo tipoArquivo, int numeroArquivoRemessa)
        {
            return GerarHeaderRemessa("0", cedente, tipoArquivo, numeroArquivoRemessa);
        }

        public override string GerarHeaderRemessa(string numeroConvenio, Cedente cedente, TipoArquivo tipoArquivo, int numeroArquivoRemessa)
        {
            try
            {
                string _header = " ";

                base.GerarHeaderRemessa("0", cedente, tipoArquivo, numeroArquivoRemessa);

                switch (tipoArquivo)
                {

                    case TipoArquivo.CNAB240:
                        _header = GerarHeaderRemessaCNAB240(cedente, numeroArquivoRemessa);
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
                throw new Exception("Erro durante a geracao do HEADER do arquivo de REMESSA.", ex);
            }
        }

        private string GerarHeaderLoteRemessaCNAB240(Cedente cedente, int numeroArquivoRemessa)
        {
            try
            {
                string header = "748"; //Posição 001 a 003   Código do Sicoob na Compensação: "756"
                header += "0001"; //Posição 004 a 007  Tipo de Registro: "1"
                header += "1";    //Posição 008        Tipo de Operação: "R"
                header += "R";    //Posição 009        Tipo de Serviço: "01"
                header += "01";   //Posição 010 a 011  Uso Exclusivo FEBRABAN/CNAB: Brancos
                header += new string(' ', 2);   //Posição 012 a 013  Nº da Versão do Layout do Lote: "040"
                header += "040";  //Posição 014 a 016     Uso Exclusivo FEBRABAN/CNAB: Brancos
                header += new string(' ', 1);    //Posição 017           Uso Exclusivo FEBRABAN/CNAB: Brancos
                header += (cedente.CPFCNPJ.Length == 11 ? "1" : "2");  //Posição 018        1=CPF    2=CGC/CNPJ
                header += Utils.FormatCode(cedente.CPFCNPJ, "0", 15, true); //Posição 019 a 033   Número de Inscrição da Empresa
                header += Utils.FormatCode(" ", " ", 20, true); //Posição 034 a 053     Código do Convênio no Sicoob: Brancos
                header += Utils.FormatCode(cedente.ContaBancaria.Agencia, "0", 5, true);//Posição 054 a 058     Prefixo da Cooperativa: vide planilha "Capa" deste arquivo
                header += " ";//Posição 059 a 059
                header += Utils.FormatCode(cedente.ContaBancaria.Conta, "0", 12, true);   //Posição 060 a 071
                header += Utils.FormatCode(cedente.ContaBancaria.DigitoConta, "0", 1, true);  //Posição 072 a 72
                header += new string(' ', 1); //Posição 073     Dígito Verificador da Ag/Conta: Brancos
                header += Utils.FormatCode(cedente.Nome.Length > 30 ? cedente.Nome.Substring(0, 30) : cedente.Nome, " ", 30);  //Posição 074 a 103      Nome do Banco: SICOOB
                header += Utils.FormatCode("", " ", 40);   // Posição 104 a 143 Informação 1			
                header += Utils.FormatCode("", " ", 40);   // Posição 144 a 183 Informação 2
                header += Utils.FormatCode(numeroArquivoRemessa.ToString(), "0", 8, true);    // Número da remessa
                header += DateTime.Now.ToString("ddMMyyyy");       //Posição 192 a 199       Data de Gravação Remessa/Retorno
                header += Utils.FormatCode("", "0", 8, true);       //Posição 200 a 207      Data do Crédito: "00000000"
                header += new string(' ', 33);   // Uso Exclusivo FEBRABAN/CNAB: Brancos
                header = Utils.SubstituiCaracteresEspeciais(header);
                return header;
            }
            catch (Exception e)
            {
                throw new Exception("Erro ao gerar HEADER DO LOTE do arquivo de remessa.", e);
            }
        }

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
                        // nao tem no CNAB 400 header = GerarHeaderLoteRemessaCNAB400(0, cedente, numeroArquivoRemessa);
                        break;
                    case TipoArquivo.Outro:
                        throw new Exception("Tipo de arquivo inexistente.");
                }

                return header;

            }
            catch (Exception ex)
            {
                throw new Exception("Erro durante a geracao do HEADER DO LOTE do arquivo de REMESSA.", ex);
            }
        }

        public string GerarHeaderRemessaCNAB240(Cedente cedente, int numeroArquivoRemessa)
        {
            try
            {
                string header = "748"; // 001 - 003
                header += "0000"; // 004 - 007
                header += "0"; // 008 - 008
                header += Utils.FormatCode("", " ", 9); //009 - 017
                header += (cedente.CPFCNPJ.Length == 11 ? "1" : "2"); // 018 - 018
                header += Utils.FormatCode(cedente.CPFCNPJ, "0", 14, true); // 019 - 032
                header += Utils.FormatCode(cedente.Convenio.ToString(), " ", 20); // 033 - 052
                header += Utils.FormatCode(cedente.ContaBancaria.Agencia, "0", 5, true); // 053 - 057
                header += " "; // 058 - 058
                header += Utils.FormatCode(cedente.ContaBancaria.Conta, "0", 12, true); // 059 - 070
                header += cedente.ContaBancaria.DigitoConta; // 071 - 071
                header += " "; // 072 - 072
                header += Utils.FormatCode(cedente.Nome, " ", 30); // 073 - 102
                header += Utils.FormatCode("SICREDI", " ", 30); // 103 - 132
                header += Utils.FormatCode("", " ", 10); // 133 - 042
                header += "1"; // 143 - 143
                header += DateTime.Now.ToString("ddMMyyyyHHmmss"); // 144 - 151 | 152 - 157
                header += Utils.FormatCode(numeroArquivoRemessa.ToString(), "0", 6); // 158 - 163
                header += "081"; // 164 - 166
                header += "01600"; // 167 - 171
                header += Utils.FormatCode("", " ", 69); // 172 - 240
                header = Utils.SubstituiCaracteresEspeciais(header);
                return header;

            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar HEADER do arquivo de remessa do CNAB240.", ex);
            }
        }

        public override string GerarTrailerLoteRemessa(int numeroRegistro)
        {
            try
            {
                string trailer = Utils.FormatCode(Codigo.ToString(), "0", 3, true); //Código do banco
                trailer += "0001"; //Utils.FormatCode("1", "0", 4, true); //Posição Uso 4 a 7  -  Número Lote - Sequencial
                trailer += "5";
                trailer += Utils.FormatCode("", " ", 9);  //Posição Uso 9 a 19    Exclusivo FEBRABAN/CNAB: Brancos
                trailer += Utils.FormatCode(numeroRegistro.ToString(), "0", 6, true);
                trailer += Utils.FormatCode("", "0", 6, true);
                trailer += Utils.FormatCode("", "0", 17, true);
                trailer += Utils.FormatCode("", "0", 6, true);
                trailer += Utils.FormatCode("", "0", 17, true);
                trailer += Utils.FormatCode("", "0", 6, true);
                trailer += Utils.FormatCode("", "0", 17, true);
                trailer += Utils.FormatCode("", "0", 6, true);
                trailer += Utils.FormatCode("", "0", 17, true);
                trailer += Utils.FormatCode("", " ", 8, true);
                trailer += Utils.FormatCode("", " ", 117);
                trailer = Utils.SubstituiCaracteresEspeciais(trailer);

                return trailer;
            }
            catch (Exception e)
            {
                throw new Exception("Erro durante a geração do registro TRAILER do LOTE de REMESSA.", e);
            }
        }

        public override string GerarTrailerRemessa(int numeroRegistro, TipoArquivo tipoArquivo, Cedente cedente, decimal vltitulostotal)
        {
            try
            {
                string _trailer = " ";

                switch (tipoArquivo)
                {
                    case TipoArquivo.CNAB240:
                        _trailer = GerarTrailerRemessa240(numeroRegistro);
                        break;
                    case TipoArquivo.CNAB400:
                        _trailer = GerarTrailerRemessa400(numeroRegistro, cedente);
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

        public string GerarTrailerRemessa240(int numeroRegistro)
        {
            try
            {
                string complemento = new string(' ', 205);
                string _trailer;

                _trailer = "74899999";
                _trailer += Utils.FormatCode("", " ", 9);
                _trailer += Utils.FormatCode("", 6);
                _trailer += Utils.FormatCode(numeroRegistro.ToString(), 6);
                _trailer += Utils.FormatCode("", 6);
                _trailer += complemento;

                _trailer = Utils.SubstituiCaracteresEspeciais(_trailer);

                return _trailer;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro durante a geracao do registro TRAILER do arquivo de REMESSA.", ex);
            }
        }

        public override string GerarTrailerArquivoRemessa(int numeroRegistro)
        {
            try
            {
                //Código do Banco na compensação ==> 001 - 003
                string trailer = Utils.FormatCode(Codigo.ToString(), "0", 3, true);

                //Numero do lote remessa ==> 004 - 007
                trailer += "9999";

                //Tipo de registro ==> 008 - 008
                trailer += "9";

                //Reservado (uso Banco) ==> 009 - 017
                trailer += Utils.FormatCode("", " ", 9);

                //Quantidade de lotes do arquivo ==> 018 - 023
                trailer += Utils.FormatCode("1", "0", 6, true);

                //Quantidade de registros do arquivo ==> 024 - 029
                trailer += Utils.FormatCode(numeroRegistro.ToString(), "0", 6, true);

                //Quantidade de registros do arquivo ==> 030 - 035
                trailer += Utils.FormatCode("", "0", 6, true);

                //Reservado (uso Banco) ==> 036 - 240
                trailer += Utils.FormatCode("", " ", 205);

                trailer = Utils.SubstituiCaracteresEspeciais(trailer);

                return trailer;
            }
            catch (Exception e)
            {
                throw new Exception("Erro durante a geração do registro TRAILER do ARQUIVO de REMESSA.", e);
            }
        }

        public override string GerarDetalheSegmentoPRemessa(Boleto boleto, int numeroRegistro, string numeroConvenio)
        {
            try
            {
                string detalhe = Utils.FormatCode(Codigo.ToString(), 3); //Posição 001 a 003   Código do Sicoob na Compensação: "756"
                detalhe += "0001"; //Utils.FormatCode(boleto.Cedente.NumeroSequencial.ToString(), "0", 4, true); //Posição 004 a 007   Número Sequencial
                detalhe += "3"; //Posição 008   Tipo de Registro: "3"
                detalhe += Utils.FormatCode(numeroRegistro.ToString(), "0", 5, true); //Posição 009 a 013   Número Sequencial
                detalhe += "P"; //Posição 014 Cód. Segmento do Registro Detalhe: "P"
                detalhe += " ";  //Posição 015 Uso Exclusivo FEBRABAN/CNAB: Brancos
                detalhe += Utils.FormatCode(boleto.Remessa.CodigoOcorrencia, 2); //Posição 016 a 017       '01'  =  Entrada de Títulos
                detalhe += Utils.FormatCode(boleto.Cedente.ContaBancaria.Agencia, 5); //Posição 018 a 022     Prefixo da Cooperativa: vide planilha "Capa" deste arquivo
                detalhe += " ";  //Posição 023  Dígito Verificador do Prefixo: vide planilha "Capa" deste arquivo
                detalhe += Utils.FormatCode(boleto.Cedente.ContaBancaria.Conta, 12); //Posição 024 a 035 Conta Corrente: vide planilha "Capa" deste arquivo
                detalhe += Utils.FormatCode(boleto.Cedente.ContaBancaria.DigitoConta, 1);  //Posição 036  Dígito Verificador da Conta: vide planilha "Capa" deste arquivo
                detalhe += " ";  //Posição 037 Dígito Verificador da Ag/Conta: Brancos


                //"Nosso Número:
                //    - Se emissão a cargo do Sicoob(vide planilha ""Capa"" deste arquivo):
                //NumTitulo - 10 posições(1 a 10) = Preencher com zeros
                //Parcela - 02 posições(11 a 12) - ""01"" se parcela única
                //Modalidade - 02 posições(13 a 14) - vide planilha ""Capa"" deste arquivo
                //Tipo Formulário -01 posição(15 a 15):
                //""1"" - auto - copiativo
                //""3"" - auto - envelopável
                //""4"" - A4 sem envelopamento
                //""6"" - A4 sem envelopamento 3 vias
                //    Em branco - 05 posições(16 a 20)
                //- Se emissão a cargo do Beneficiário(vide planilha ""Capa"" deste arquivo):
                //NumTitulo - 10 posições(1 a 10): Vide planilha ""02.Especificações do Boleto"" deste arquivo item 3.13
                //Parcela - 02 posições(11 a 12) - ""01"" se parcela única
                //Modalidade - 02 posições(13 a 14) - vide planilha ""Capa"" deste arquivo
                //Tipo Formulário -01 posição(15 a 15):
                //""1"" - auto - copiativo
                //""3"" - auto - envelopável
                //""4"" - A4 sem envelopamento
                //""6"" - A4 sem envelopamento 3 vias
                //    Em branco - 05 posições(16 a 20)"

                this.FormataNossoNumero(boleto);
                boleto.NossoNumero = boleto.NossoNumero.Replace("/", string.Empty).Replace("-", string.Empty);

                var nossoNumero = boleto.NossoNumero.PadRight(20, ' ');

                detalhe += nossoNumero;  //Posição 038 a 057 Nosso Número

                //TODO - verify code - imported from master repo
                //detalhe += Utils.FormatCode(NossoNumeroFormatado(boleto), 20);  //Posição 038 a 057 Nosso Número

                detalhe += "1";  //Posição 058 Código da Carteira: vide planilha "Capa" deste arquivo
                detalhe += "1";  //Posição 059 Forma de Cadastr. do Título no Banco: "0"
                detalhe += "1";  //Posição 060 Tipo de Documento: Brancos
                detalhe += "2";  //Posição 061 "Identificação da Emissão do Boleto: 1=Sicredi Emite 2=Beneficiário Emite TODO:Deivid
                detalhe += "2";  //Posição 062 "Identificação da distribuição do Boleto: 1=Sicredi Emite 2=Beneficiário Emite TODO:Deivid
                detalhe += Utils.FormatCode(boleto.NumeroDocumento, " ", 15); //Posição 063 a 075 Número do documento de cobrança. TODO:Deivid
                detalhe += Utils.FormatCode(boleto.DataVencimento.ToString("ddMMyyyy"), 8);

                string valorBoleto = boleto.ValorBoleto.ToString("f").Replace(",", "").Replace(".", "");

                valorBoleto = Utils.FormatCode(valorBoleto, 15);
                detalhe += valorBoleto; //Posição 86 a 100   Valor Nominal do Título

                detalhe += "00000";//Posição 101 a 105     Agência Encarregada da Cobrança: "00000"
                detalhe += new string(' ', 1);  //Posição 106  Dígito Verificador da Agência: Brancos
                detalhe += Utils.FormatCode(boleto.EspecieDocumento.Codigo, 2);  //Posição 107 a 108   Espécie do título
                detalhe += Utils.FormatCode(boleto.Aceite, 1);  //Posição 109 Identificação do título Aceito/Não Aceito  TODO:Deivid
                detalhe += Utils.FormatCode(boleto.DataProcessamento.ToString("ddMMyyyy"), 8);   //Posição 110 a 117   Data Emissão do Título

                detalhe += boleto.JurosMora > 0 ? "2" : "3"; //Posição 118  - "Código do Juros de Mora: '3' = Isento '1' = Valor por Dia '2' = Taxa Mensal"
                detalhe += Utils.FormatCode(boleto.JurosMora > 0 ? boleto.DataVencimento.ToString("ddMMyyyy") : "0", 8);  //Posição 119 a 126  - Data do Juros de Mora: preencher com a Data de Vencimento do Título

                valorBoleto = boleto.PercJurosMora.ToString("f").Replace(",", "").Replace(".", "");
                valorBoleto = Utils.FormatCode(valorBoleto, 15);
                detalhe += valorBoleto;  //Posição 127 a 141  - "Juros de Mora por Dia/Taxa ao Mês Valor = R$ ao dia Taxa = % ao mês Ex: 0000000000220 = 2,20 %; Ex: 0000000001040 = 10,40 % "

                detalhe += boleto.ValorDesconto > 0 ? "1" : "0"; //Posição 142  - Código do desconto
                detalhe += Utils.FormatCode(boleto.ValorDesconto > 0 ? boleto.DataDesconto.ToString("ddMMyyyy") : "0", 8); //Posição 143 a 150  - Data do Desconto 1
                valorBoleto = boleto.ValorDesconto.ToString("f").Replace(",", "").Replace(".", "");
                valorBoleto = Utils.FormatCode(valorBoleto, 15);  //Posição 151 a 165  - Valor/Percentual a ser Concedido
                detalhe += valorBoleto;
                detalhe += Utils.FormatCode(boleto.IOF.ToString(), 15);//Posição 166 a 180   -  Valor do IOF a ser Recolhido
                detalhe += Utils.FormatCode(boleto.Abatimento.ToString(), 15);//Posição 181 a 195   - Valor do Abatimento
                detalhe += Utils.FormatCode(boleto.NumeroDocumento, 25); //Posição 196 a 220  - Identificação do título

                detalhe += boleto.Instrucoes[0].Codigo.ToString() == "7" ? "3" : "1"; //Posição 221  - Código do protesto
                #region Instruções
                string vInstrucao1 = boleto.Instrucoes[0].Codigo.ToString();//"00"; //2ª instrução (2, N) Caso Queira colocar um cod de uma instrução. ver no Manual caso nao coloca 00

                #endregion
                detalhe += Utils.FormatCode(boleto.Instrucoes[0].QuantidadeDias.ToString(), 2);  //Posição 222 a 223  - Número de Dias Corridos para Protesto

                //TODO verify code
                // detalhe += Utils.FormatCode(boleto.CodJurosMora, "2", 1); //Posição 118  - Código do juros mora. 2 = Padrao % Mes
                // detalhe += Utils.FormatCode(boleto.DataJurosMora > DateTime.MinValue ? boleto.DataJurosMora.ToString("ddMMyyyy") : "".PadLeft(8, '0'), 8);  //Posição 119 a 126  - Data do Juros de Mora: preencher com a Data de Vencimento do Título
                // detalhe += Utils.FormatCode(boleto.CodJurosMora == "0" ? "".PadLeft(15, '0') : (boleto.CodJurosMora == "1" ? boleto.JurosMora.ToString("f").Replace(",", "").Replace(".", "") : boleto.PercJurosMora.ToString("f").Replace(",", "").Replace(".", "")), 15);   //Posição 127 a 141  - Data do Juros de Mora: preencher com a Data de Vencimento do Título

                // if (boleto.DataDesconto > DateTime.MinValue)
                // {
                //     detalhe += "1"; //Posição 118  - Código do desconto
                //     detalhe += Utils.FormatCode(boleto.DataDesconto.ToString("ddMMyyyy"), 8); //Posição 143 a 150  - Data do Desconto 1
                //     detalhe += Utils.FormatCode(boleto.ValorDesconto.ToString("f").Replace(",", "").Replace(".", ""), 15);
                // }
                // else
                // {
                //     detalhe += "0"; //Posição 118  - Código do desconto - Sem Desconto
                //     detalhe += Utils.FormatCode("", "0", 8, true); ; //Posição 143 a 150  - Data do Desconto
                //     detalhe += Utils.FormatCode("", "0", 15, true);
                // }

                // detalhe += Utils.FormatCode(boleto.IOF.ToString(), 15);//Posição 166 a 180   -  Valor do IOF a ser Recolhido
                // detalhe += Utils.FormatCode(boleto.Abatimento.ToString(), 15);//Posição 181 a 195   - Valor do Abatimento
                // detalhe += Utils.FormatCode(boleto.NumeroDocumento, " ", 25); //Posição 196 a 220  - Identificação do título
                // detalhe += "3"; //Posição 221  - Código do protesto 3 = Nao Protestar

                // #region Instruções

                // string vInstrucao1 = "00"; //2ª instrução (2, N) Caso Queira colocar um cod de uma instrução. ver no Manual caso nao coloca 00
                // foreach (IInstrucao instrucao in boleto.Instrucoes)
                // {
                //     switch ((EnumInstrucoes_Sicoob)instrucao.Codigo)
                //     {
                //         case EnumInstrucoes_Sicoob.CobrarJuros:
                //             vInstrucao1 = Utils.FitStringLength(instrucao.QuantidadeDias.ToString(), 2, 2, '0', 0, true, true, true);
                //             break;
                //     }
                // }

                // #endregion

                // detalhe += Utils.FormatCode(vInstrucao1, 2);  //Posição 222 a 223  - Código do protesto

                detalhe += Utils.FormatCode("1", 1);     //Posição 224  - Código para Baixa/Devolução: "0"
                detalhe += "000";//detalhe += Utils.FormatCode(" ", 3); Posição 225 A 227  - Número de Dias para Baixa/Devolução: Brancos
                detalhe += Utils.FormatCode(boleto.Moeda.ToString(), "0", 2, true); //Posição 228 A 229  - Código da Moeda
                detalhe += Utils.FormatCode("", "0", 10, true); //Posição 230 A 239    -  Nº do Contrato da Operação de Créd.: "0000000000"
                detalhe += " ";
                detalhe = Utils.SubstituiCaracteresEspeciais(detalhe);
                return detalhe;
            }
            catch (Exception e)
            {
                throw new Exception("Erro durante a geração do SEGMENTO P DO DETALHE do arquivo de REMESSA.", e);
            }
        }

        public override string GerarDetalheSegmentoQRemessa(Boleto boleto, int numeroRegistro, TipoArquivo tipoArquivo)
        {
            try
            {
                string detalhe = Utils.FormatCode(Codigo.ToString(), "0", 3, true); //Posição 001 a 003   Código do Sicoob na Compensação: "756"
                detalhe += "0001"; //Utils.FormatCode(boleto.Cedente.NumeroSequencial.ToString(), "0", 4, true); //Posição 004 a 007   Número Sequencial
                detalhe += "3"; //Posição 008   Tipo de Registro: "3"
                detalhe += Utils.FormatCode(numeroRegistro.ToString(), "0", 5, true); //Posição 009 a 013   Número Sequencial
                detalhe += "Q"; //Posição 014 Cód. Segmento do Registro Detalhe: "P"
                detalhe += " ";  //Posição 015 Uso Exclusivo FEBRABAN/CNAB: Brancos
                detalhe += "01"; //Posição 016 a 017       '01'  =  Entrada de Títulos
                detalhe += (boleto.Sacado.CPFCNPJ.Length == 11 ? "1" : "2");  //Posição 018        1=CPF    2=CGC/CNPJ
                detalhe += Utils.FormatCode(boleto.Sacado.CPFCNPJ, "0", 15, true); //Posição 019 a 033   Número de Inscrição da Empresa
                detalhe += Utils.FormatCode(boleto.Sacado.Nome, " ", 40);  //Posição 034 a 73      Nome
                detalhe += Utils.FormatCode(boleto.Sacado.Endereco.End, " ", 40);  //Posição 074 a 113      Endereço
                detalhe += Utils.FormatCode(boleto.Sacado.Endereco.Bairro, " ", 15);                     // Bairro 
                detalhe += Utils.FormatCode(boleto.Sacado.Endereco.CEP, 8);    //CEP (5, N) + Sufixo do CEP (3, N) Total (8, N)
                detalhe += Utils.FormatCode(boleto.Sacado.Endereco.Cidade, " ", 15);                     // Cidade 
                detalhe += boleto.Sacado.Endereco.UF;                                                  // Unidade da Federação
                //detalhe += (boleto.Cedente.CPFCNPJ.Length == 11 ? "1" : "2");                             // Tipo de Inscrição Sacador avalista
                //detalhe += Utils.FormatCode(boleto.Cedente.CPFCNPJ, "0", 15, true);                             // Número de Inscrição / Sacador avalista
                //detalhe += Utils.FormatCode(boleto.Cedente.Nome, " ", 40);                                // Nome / Sacador avalista
                detalhe += "0";                             // Tipo de Inscrição Sacador avalista
                detalhe += Utils.FormatCode("", "0", 15, true);                             // Número de Inscrição / Sacador avalista
                detalhe += Utils.FormatCode("", " ", 40);   
                detalhe += "000";                                                                         // Código Bco. Corresp. na Compensação
                detalhe += Utils.FormatCode("", " ", 20);                                                 //213 - Nosso N° no Banco Correspondente "1323739"
                detalhe += Utils.FormatCode("", " ", 8);                                                  // Uso Exclusivo FEBRABAN/CNAB
                detalhe = Utils.SubstituiCaracteresEspeciais(detalhe).ToUpper();
                return detalhe;
            }
            catch (Exception e)
            {
                throw new Exception("Erro durante a geração do SEGMENTO Q DO DETALHE do arquivo de REMESSA.", e);
            }
        }

        public override string GerarDetalheSegmentoRRemessa(Boleto boleto, int numeroRegistro, TipoArquivo tipoArquivo)
        {
            try
            {
                string detalhe = Utils.FormatCode(Codigo.ToString(), 3); //Posição 001 a 003   Código do Sicoob na Compensação: "756"
                detalhe += "0001"; //Utils.FormatCode(boleto.Cedente.NumeroSequencial.ToString(), "0", 4, true); //Posição 004 a 007   Número Sequencial
                detalhe += "3"; //Posição 008   Tipo de Registro: "3"
                detalhe += Utils.FormatCode(numeroRegistro.ToString(), "0", 5, true); //Posição 009 a 013   Número Sequencial
                detalhe += "R"; //Posição 014 Cód. Segmento do Registro Detalhe: "R"
                detalhe += " ";  //Posição 015 Uso Exclusivo FEBRABAN/CNAB: Brancos
                detalhe += "01"; //Posição 016 a 017       '01'  =  Entrada de Títulos

                detalhe += "0"; //Posição 18  - Código do desconto 2
                detalhe += Utils.FormatCode("0", 8);//Utils.FormatCode(boleto.DataDesconto.ToString("ddMMyyyy"), 8); //Posição 019 - 026  - Data do Desconto 2
                var valorBoleto = "0";//boleto.ValorDesconto.ToString("f").Replace(",", "").Replace(".", "");
                valorBoleto = Utils.FormatCode(valorBoleto, 15);  //Posição 027 - 041  - Valor/Percentual a ser Concedido 2
                detalhe += valorBoleto; //Posição 027 - 041  Valor/Percentual a ser Concedido

                detalhe += "0"; //Posição 42  - Código do desconto 3
                detalhe += Utils.FormatCode("0", 8);//Utils.FormatCode(boleto.DataDesconto.ToString("ddMMyyyy"), 8); //Posição 43 - 50  - Data do Desconto 3
                valorBoleto = "0";//boleto.ValorDesconto.ToString("f").Replace(",", "").Replace(".", "");
                valorBoleto = Utils.FormatCode(valorBoleto, 15);  //Posição 51 - 65  - Valor/Percentual a ser Concedido 3
                detalhe += valorBoleto; //Posição  51 - 65 Valor/Percentual a ser Concedido

                detalhe += boleto.PercMulta > 0 ? "2" : "0"; //Posição 66 - "Código da Multa: '0' = Isento '1' = Valor Fixo '2' = Percentual"
                detalhe += Utils.FormatCode(boleto.PercMulta > 0 ? boleto.DataMulta.ToString("ddMMyyyy") : "0", 8);  //Posição 67-74  - Data do Juros de Mora: preencher com a Data de Vencimento do Título

                valorBoleto = boleto.PercMulta.ToString("f").Replace(",", "").Replace(".", "");
                valorBoleto = Utils.FormatCode(valorBoleto, 15);
                detalhe += valorBoleto;   //Posição 75 - 89 - Valor/Percentual a Ser Aplicado Ex: 0000000000220 = 2,20 %; Ex: 0000000001040 = 10,40 %

                detalhe += Utils.FormatCode("0", 10); //Posição 90 a 99 Informação ao Pagador: Brancos
                detalhe += Utils.FormatCode("0", 40); //Posição 100 a 139 Informação ao Pagador: Brancos
                detalhe += Utils.FormatCode("0", 40); //Posição 140 a 179 Informação ao Pagador: Brancos
                detalhe += Utils.FormatCode("0", 20); //Posição 180 a 199 Uso Exclusivo FEBRABAN/CNAB: Brancos
                detalhe += Utils.FormatCode("0", "0", 8, true);  //Posição 200 a 207  Cód. Ocor. do Pagador: "00000000"
                detalhe += Utils.FormatCode("0", "0", 3, true);  //Posição 208 a 210  Cód. do Banco na Conta do Débito: "000"
                detalhe += Utils.FormatCode("0", "0", 5, true);  //Posição 211 a 215  Código da Agência do Débito: "00000"
                detalhe += " "; //Posição 216 Dígito Verificador da Agência: Brancos
                detalhe += Utils.FormatCode("0", "0", 12, true);  //Posição 217 a 228  Conta Corrente para Débito: "000000000000"
                detalhe += " "; //Posição 229  Verificador da Conta: Brancos
                detalhe += " "; //Posição 230  Verificador Ag/Conta: Brancos
                detalhe += "0"; //Posição 231  Aviso para Débito Automático: "0"
                detalhe += Utils.FormatCode("", " ", 9); //Posição 232 - 240 Uso Exclusivo FEBRABAN/CNAB: Brancos

                //TODO verify code
                // if (boleto.DataOutrosDescontos > DateTime.MinValue)
                // {
                //     detalhe += "1"; //Posição 18  - Código do desconto 2
                //     detalhe += Utils.FormatCode(boleto.DataOutrosDescontos.ToString("ddMMyyyy"), 8); //Posição 19 a 26  - Data do Desconto 2
                //     detalhe += Utils.FormatCode(boleto.OutrosDescontos.ToString("f").Replace(",", "").Replace(".", ""), 15);  //Posição 27 a 41  - Valor do Desconto 2
                // }
                // else
                // {
                //     detalhe += "0"; //Posição 18  - Código do desconto 2
                //     detalhe += Utils.FormatCode("", "0", 8, true); //Posição 19 a 26  - Data do Desconto 2
                //     detalhe += Utils.FormatCode("", "0", 15, true);  //Posição 27 a 41  - Valor do Desconto 2
                // }

                // detalhe += "0"; //Posição 42  - Código da desconto 3
                // detalhe += Utils.FormatCode("", "0", 8, true);
                // detalhe += Utils.FormatCode("", "0", 15, true);

                // if (boleto.PercMulta > 0)
                // {
                //     // Código da multa 2 - percentual
                //     detalhe += "2";
                //     detalhe += Utils.FormatCode(boleto.DataMulta.ToString("ddMMyyyy"), 8);  //Posição 119 a 126  - Data do Juros de Mora: preencher com a Data de Vencimento do Título
                //     detalhe += Utils.FitStringLength(boleto.PercMulta.ApenasNumeros(), 15, 15, '0', 0, true, true, true);
                // }
                // else if (boleto.ValorMulta > 0)
                // {
                //     // Código da multa 1 - valor fixo
                //     detalhe += "1";
                //     detalhe += Utils.FormatCode(boleto.DataMulta.ToString("ddMMyyyy"), 8);  //Posição 119 a 126  - Data do Juros de Mora: preencher com a Data de Vencimento do Título
                //     detalhe += Utils.FitStringLength(boleto.ValorMulta.ApenasNumeros(), 15, 15, '0', 0, true, true, true);
                // }
                // else
                // {
                //     // Código da multa 0 - sem multa
                //     detalhe += "0";
                //     detalhe += Utils.FormatCode("", "0", 8); //Posição 119 a 126  - Data do Juros de Mora: preencher com a Data de Vencimento do Título
                //     detalhe += Utils.FitStringLength("0", 15, 15, '0', 0, true, true, true);
                // }

                // detalhe += Utils.FormatCode(""," ", 10); //Posição 90 a 99 Informação ao Pagador: Brancos
                // detalhe += Utils.FormatCode(""," ", 40); //Posição 100 a 139 Informação ao Pagador: Brancos
                // detalhe += Utils.FormatCode(""," ", 40); //Posição 140 a 179 Informação ao Pagador: Brancos
                // detalhe += Utils.FormatCode(""," ", 20); //Posição 180 a 199 Uso Exclusivo FEBRABAN/CNAB: Brancos
                // detalhe += Utils.FormatCode("", "0", 8, true);  //Posição 200 a 207  Cód. Ocor. do Pagador: "00000000"
                // detalhe += Utils.FormatCode("", "0", 3, true);  //Posição 208 a 210  Cód. do Banco na Conta do Débito: "000"
                // detalhe += Utils.FormatCode("", "0", 5, true);  //Posição 211 a 215  Código da Agência do Débito: "00000"
                // detalhe += " "; //Posição 216 Dígito Verificador da Agência: Brancos
                // detalhe += Utils.FormatCode("", "0", 12, true);  //Posição 217 a 228  Conta Corrente para Débito: "000000000000"
                // detalhe += " "; //Posição 229  Verificador da Conta: Brancos
                // detalhe += " "; //Posição 230  Verificador Ag/Conta: Brancos
                // detalhe += "0"; //Posição 231  Aviso para Débito Automático: "0"
                // detalhe += Utils.FormatCode(""," ", 9); //Posição Uso Exclusivo FEBRABAN/CNAB: Brancos
                detalhe = Utils.SubstituiCaracteresEspeciais(detalhe);
                return detalhe;
            }
            catch (Exception e)
            {
                throw new Exception("Erro durante a geração do SEGMENTO R DO DETALHE do arquivo de REMESSA.", e);
            }
        }

        #endregion

        #region Metodos de Leitura do Arquivo de Retorno
        
        public override DetalheSegmentoTRetornoCNAB240 LerDetalheSegmentoTRetornoCNAB240(string registro)
        {
            try
            {
                DetalheSegmentoTRetornoCNAB240 detalhe = new DetalheSegmentoTRetornoCNAB240(registro);

                if (registro.Substring(13, 1) != "T")
                    throw new Exception("Registro inválido. O detalhe não possuí as características do segmento T.");

                detalhe.CodigoBanco = Convert.ToInt32(registro.Substring(0, 3));
                detalhe.idCodigoMovimento = Convert.ToInt32(registro.Substring(15, 2));
                detalhe.Agencia = Convert.ToInt32(registro.Substring(17, 5));
                detalhe.DigitoAgencia = registro.Substring(22, 1);
                detalhe.Conta = Convert.ToInt32(registro.Substring(23, 12));
                detalhe.DigitoConta = registro.Substring(35, 1);
                detalhe.NossoNumero = registro.Substring(37, 10);

                detalhe.NossoNumero = detalhe.NossoNumero.Substring(3, 5);
                detalhe.NossoNumero = detalhe.NossoNumero.Insert(detalhe.NossoNumero.Length, "0").PadLeft(10, '0');

                detalhe.CodigoCarteira = Convert.ToInt32(registro.Substring(57, 1));
                //detalhe.NumeroDocumento = registro.Substring(58, 15);
                //sera utilizado para agrupar os registros
                detalhe.NumeroDocumento = registro.Substring(37, 10);
                int dataVencimento = Convert.ToInt32(registro.Substring(73, 8));
                detalhe.DataVencimento = Convert.ToDateTime(dataVencimento.ToString("##-##-####"));
                decimal valorTitulo = Convert.ToInt64(registro.Substring(81, 15));
                detalhe.ValorTitulo = valorTitulo / 100;
                detalhe.IdentificacaoTituloEmpresa = registro.Substring(105, 25);
                detalhe.TipoInscricao = Convert.ToInt32(registro.Substring(132, 1));
                detalhe.NumeroInscricao = registro.Substring(133, 15);
                detalhe.NomeSacado = registro.Substring(148, 40);
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

                int aux;
                long aux1;

                if (registro.Substring(13, 1) != "U")
                    throw new Exception("Registro inválido. O detalhe não possuí as características do segmento U.");

                detalhe.CodigoOcorrenciaSacado = registro.Substring(15, 2);

                int.TryParse(registro.Substring(137, 8), out aux);
                int DataOcorrencia = aux;
                if (DataOcorrencia > 0)
                    detalhe.DataOcorrencia = Convert.ToDateTime(DataOcorrencia.ToString("##-##-####"));

                //TODO verify code
                // int DataCredito = Convert.ToInt32(registro.Substring(145, 8));
                // detalhe.DataCredito = (DataCredito > 0) ? Convert.ToDateTime(DataCredito.ToString("##-##-####")) : new DateTime();
                // int DataOcorrencia = Convert.ToInt32(registro.Substring(137, 8));
                // detalhe.DataOcorrencia = (DataOcorrencia > 0) ? Convert.ToDateTime(DataOcorrencia.ToString("##-##-####")) : new DateTime();

                int.TryParse(registro.Substring(157, 8), out aux);
                int DataOcorrenciaSacado = aux;
                if (DataOcorrenciaSacado > 0)
                    detalhe.DataOcorrenciaSacado = Convert.ToDateTime(DataOcorrenciaSacado.ToString("##-##-####"));
                else
                    detalhe.DataOcorrenciaSacado = DateTime.Now;

                int.TryParse(registro.Substring(145, 8), out aux);
                int DataCredito = aux;
                if (DataCredito > 0)
                    detalhe.DataCredito = Convert.ToDateTime(DataCredito.ToString("##-##-####"));
                else
                    detalhe.DataCredito = detalhe.DataOcorrencia;

                long.TryParse(registro.Substring(17, 15), out aux1);
                decimal JurosMultaEncargos = aux1;
                detalhe.JurosMultaEncargos = JurosMultaEncargos / 100;

                long.TryParse(registro.Substring(32, 15), out aux1);
                decimal ValorDescontoConcedido = aux1;
                detalhe.ValorDescontoConcedido = ValorDescontoConcedido / 100;

                long.TryParse(registro.Substring(47, 15), out aux1);
                decimal ValorAbatimentoConcedido = aux1;
                detalhe.ValorAbatimentoConcedido = ValorAbatimentoConcedido / 100;

                long.TryParse(registro.Substring(62, 15), out aux1);
                decimal ValorIOFRecolhido = aux1;
                detalhe.ValorIOFRecolhido = ValorIOFRecolhido / 100;

                long.TryParse(registro.Substring(77, 15), out aux1);
                decimal ValorPagoPeloSacado = aux1;
                detalhe.ValorPagoPeloSacado = ValorPagoPeloSacado / 100;

                long.TryParse(registro.Substring(92, 15), out aux1);
                decimal ValorLiquidoASerCreditado = aux1;
                detalhe.ValorLiquidoASerCreditado = ValorLiquidoASerCreditado / 100;

                long.TryParse(registro.Substring(107, 15), out aux1);
                decimal ValorOutrasDespesas = aux1;
                detalhe.ValorOutrasDespesas = ValorOutrasDespesas / 100;

                long.TryParse(registro.Substring(122, 15), out aux1);
                decimal ValorOutrosCreditos = aux1;
                detalhe.ValorOutrosCreditos = ValorOutrosCreditos / 100;

                return detalhe;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao processar arquivo de RETORNO - SEGMENTO U.", ex);
            }
        }

        #endregion Metodos de Leitura do Arquivo de Retorno

        public int Mod10Sicredi(string seq)
        {
            /* Variaveis
             * -------------
             * d - Digito
             * s - Soma
             * p - Peso
             * b - Base
             * r - Resto
             */

            int d, s = 0, p = 2, b = 2, r;

            for (int i = seq.Length - 1; i >= 0; i--)
            {

                r = (Convert.ToInt32(seq.Substring(i, 1)) * p);
                if (r > 9)
                    r = SomaDezena(r);
                s = s + r;
                if (p < b)
                    p++;
                else
                    p--;
            }

            d = Multiplo10(s);
            return d;
        }

        public int SomaDezena(int dezena)
        {
            string d = dezena.ToString();
            int d1 = Convert.ToInt32(d.Substring(0, 1));
            int d2 = Convert.ToInt32(d.Substring(1));
            return d1 + d2;
        }

        public int digSicredi(string seq)
        {
            /* Variaveis
             * -------------
             * d - Digito
             * s - Soma
             * p - Peso
             * b - Base
             * r - Resto
             */

            int d, s = 0, p = 2, b = 9;

            for (int i = seq.Length - 1; i >= 0; i--)
            {
                s = s + (Convert.ToInt32(seq.Substring(i, 1)) * p);
                if (p < b)
                    p = p + 1;
                else
                    p = 2;
            }

            d = 11 - (s % 11);
            if (d > 9)
                d = 0;
            return d;
        }

        public string DigNossoNumeroSicredi(Boleto boleto, bool arquivoRemessa = false)
        {
            //Adicionado por diego.dariolli pois ao gerar remessa o Digito saíra errado pois faltava Agencia e posto no codigo do cedente
            string codigoCedente = ""; //codigo do beneficiário aaaappccccc
            if (arquivoRemessa)
            {
                if (string.IsNullOrEmpty(boleto.Cedente.ContaBancaria.OperacaConta))
                    throw new Exception("O codigo do posto beneficiário nao foi informado.");

                codigoCedente = string.Concat(boleto.Cedente.ContaBancaria.Agencia, boleto.Cedente.ContaBancaria.OperacaConta, Utils.Right((boleto.Cedente.Codigo), 5, '0', true)); 
            }
            else
                codigoCedente = boleto.Cedente.Codigo;

            string nossoNumero = boleto.NossoNumero; //ano atual (yy), indicador de geracao do nosso numero (b) e o numero sequencial do beneficiário (nnnnn);

            string seq = string.Concat(codigoCedente, nossoNumero); // = aaaappcccccyybnnnnn
            /* Variaveis
             * -------------
             * d - Digito
             * s - Soma
             * p - Peso
             * b - Base
             * r - Resto
             */

            int d, s = 0, p = 2, b = 9;
            //Atribui os pesos de {2..9}
            for (int i = seq.Length - 1; i >= 0; i--)
            {
                s = s + (Convert.ToInt32(seq.Substring(i, 1)) * p);
                if (p < b)
                    p = p + 1;
                else
                    p = 2;
            }
            d = 11 - (s % 11);//Calcula o Modulo 11;
            if (d > 9)
                d = 0;

            return d.ToString();
        }


        /// <summary>
        /// Efetua as Validacoes dentro da classe Boleto, para garantir a geracao da remessa
        /// </summary>
        public override bool ValidarRemessa(TipoArquivo tipoArquivo, string numeroConvenio, IBanco banco, Cedente cedente, Boletos boletos, int numeroArquivoRemessa, out string mensagem)
        {
            bool vRetorno = true;
            string vMsg = string.Empty;
            //            
            switch (tipoArquivo)
            {
                case TipoArquivo.CNAB240:
                    //vRetorno = ValidarRemessaCNAB240(numeroConvenio, banco, cedente, boletos, numeroArquivoRemessa, out vMsg);
                    break;
                case TipoArquivo.CNAB400:
                    vRetorno = ValidarRemessaCNAB400(numeroConvenio, banco, cedente, boletos, numeroArquivoRemessa, out vMsg);
                    break;
                case TipoArquivo.Outro:
                    throw new Exception("Tipo de arquivo inexistente.");
            }
            //
            mensagem = vMsg;
            return vRetorno;
        }


        #region CNAB 400 - sidneiklein
        public bool ValidarRemessaCNAB400(string numeroConvenio, IBanco banco, Cedente cedente, Boletos boletos, int numeroArquivoRemessa, out string mensagem)
        {
            bool vRetorno = true;
            string vMsg = string.Empty;
            //
            #region Pre Validacoes
            if (banco == null)
            {
                vMsg += String.Concat("Remessa: O Banco é Obrigatório!", Environment.NewLine);
                vRetorno = false;
            }
            if (cedente == null)
            {
                vMsg += String.Concat("Remessa: O Cedente/Beneficiário é Obrigatório!", Environment.NewLine);
                vRetorno = false;
            }
            if (boletos == null || boletos.Count.Equals(0))
            {
                vMsg += String.Concat("Remessa: Deverá existir ao menos 1 boleto para geracao da remessa!", Environment.NewLine);
                vRetorno = false;
            }
            #endregion
            //
            foreach (Boleto boleto in boletos)
            {
                #region Validacao de cada boleto
                if (boleto.Remessa == null)
                {
                    vMsg += String.Concat("Boleto: ", boleto.NumeroDocumento, "; Remessa: Informe as diretrizes de remessa!", Environment.NewLine);
                    vRetorno = false;
                }
                else
                {
                    #region Validacoes da Remessa que deverao estar preenchidas quando SICREDI
                    //Comentado porque ainda esta fixado em 01
                    //if (String.IsNullOrEmpty(boleto.Remessa.CodigoOcorrencia))
                    //{
                    //    vMsg += String.Concat("Boleto: ", boleto.NumeroDocumento, "; Remessa: Informe o Codigo de Ocorrencia!", Environment.NewLine);
                    //    vRetorno = false;
                    //}
                    if (String.IsNullOrEmpty(boleto.NumeroDocumento))
                    {
                        vMsg += String.Concat("Boleto: ", boleto.NumeroDocumento, "; Remessa: Informe um numero de Documento!", Environment.NewLine);
                        vRetorno = false;
                    }
                    else if (String.IsNullOrEmpty(boleto.Remessa.TipoDocumento))
                    {
                        // Para o Sicredi, defini o Tipo de Documento sendo: 
                        //       A = 'A' - SICREDI com Registro
                        //      C1 = 'C' - SICREDI sem Registro Impressao Completa pelo Sicredi
                        //      C2 = 'C' - SICREDI sem Registro Pedido de bloquetos pre-impressos
                        // ** Isso porque sao tratados 3 leiautes de escrita diferentes para o Detail da remessa;

                        vMsg += String.Concat("Boleto: ", boleto.NumeroDocumento, "; Remessa: Informe o Tipo Documento!", Environment.NewLine);
                        vRetorno = false;
                    }
                    else if (!boleto.Remessa.TipoDocumento.Equals("A") && !boleto.Remessa.TipoDocumento.Equals("C1") && !boleto.Remessa.TipoDocumento.Equals("C2"))
                    {
                        vMsg += String.Concat("Boleto: ", boleto.NumeroDocumento, "; Remessa: Tipo de Documento Invalido! Deveráo ser: A = SICREDI com Registro; C1 = SICREDI sem Registro Impressao Completa pelo Sicredi;  C2 = SICREDI sem Registro Pedido de bloquetos pre-impressos", Environment.NewLine);
                        vRetorno = false;
                    }
                    //else if (boleto.Remessa.TipoDocumento.Equals("06") && !String.IsNullOrEmpty(boleto.NossoNumero))
                    //{
                    //    //Para o "Remessa.TipoDocumento = "06", nao podera ter NossoNumero Gerado!
                    //    vMsg += String.Concat("Boleto: ", boleto.NumeroDocumento, "; nao pode existir NossoNumero para o Tipo Documento '06 - Cobranca escritural'!", Environment.NewLine);
                    //    vRetorno = false;
                    //}
                    else if (!boleto.EspecieDocumento.Codigo.Equals("A") && //A - Duplicata Mercantil por Indicacao
                             !boleto.EspecieDocumento.Codigo.Equals("B") && //B - Duplicata Rural;
                             !boleto.EspecieDocumento.Codigo.Equals("C") && //C - Nota Promissória;
                             !boleto.EspecieDocumento.Codigo.Equals("D") && //D - Nota Promissória Rural;
                             !boleto.EspecieDocumento.Codigo.Equals("E") && //E - Nota de Seguros;
                             !boleto.EspecieDocumento.Codigo.Equals("F") && //G - Recibo;

                             !boleto.EspecieDocumento.Codigo.Equals("H") && //H - Letra de Cambio;
                             !boleto.EspecieDocumento.Codigo.Equals("I") && //I - Nota de Debito;
                             !boleto.EspecieDocumento.Codigo.Equals("J") && //J - Duplicata de Servico por Indicacao;
                             !boleto.EspecieDocumento.Codigo.Equals("O") && //O - Boleto Proposta
                             !boleto.EspecieDocumento.Codigo.Equals("K") //K - Outros.
                            )
                    {
                        vMsg += String.Concat("Boleto: ", boleto.NumeroDocumento, "; Remessa: Informe o Codigo da EspecieDocumento! São Aceitas:{A,B,C,D,E,F,H,I,J,O,K}", Environment.NewLine);
                        vRetorno = false;
                    }
                    else if (!boleto.Sacado.CPFCNPJ.Length.Equals(11) && !boleto.Sacado.CPFCNPJ.Length.Equals(14))
                    {
                        vMsg += String.Concat("Boleto: ", boleto.NumeroDocumento, "; Remessa: Cpf/Cnpj diferente de 11/14 caracteres!", Environment.NewLine);
                        vRetorno = false;
                    }
                    else if (!boleto.NossoNumero.Length.Equals(8))
                    {
                        //sidnei.klein: Segundo definição recebida pelo Sicredi-RS, o Nosso numero sempre tera somente 8 caracteres sem o DV que esta no boleto.DigitoNossoNumero
                        vMsg += String.Concat("Boleto: ", boleto.NumeroDocumento, "; Remessa: O Nosso numero diferente de 8 caracteres!", Environment.NewLine);
                        vRetorno = false;
                    }
                    else if (!boleto.TipoImpressao.Equals("A") && !boleto.TipoImpressao.Equals("B"))
                    {
                        vMsg += String.Concat("Boleto: ", boleto.NumeroDocumento, "; Tipo de Impressao deve conter A - Normal ou B - Carnê", Environment.NewLine);
                        vRetorno = false;
                    }
                    #endregion
                }
                #endregion
            }
            //
            mensagem = vMsg;
            return vRetorno;
        }
        public string GerarHeaderRemessaCNAB400(int numeroConvenio, Cedente cedente, int numeroArquivoRemessa)
        {
            try
            {
                TRegistroEDI reg = new TRegistroEDI();
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0001, 001, 0, "0", ' '));                             //001-001
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0002, 001, 0, "1", ' '));                             //002-002
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0003, 007, 0, "REMESSA", ' '));                       //003-009
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0010, 002, 0, "01", ' '));                            //010-011
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0012, 015, 0, "COBRANCA", ' '));                      //012-026
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0027, 005, 0, cedente.ContaBancaria.Conta, ' '));     //027-031
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0032, 014, 0, cedente.CPFCNPJ, ' '));                 //032-045
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0046, 031, 0, "", ' '));                              //046-076
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0077, 003, 0, "748", ' '));                           //077-079
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0080, 015, 0, "SICREDI", ' '));                       //080-094
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediDataAAAAMMDD_________, 0095, 008, 0, DateTime.Now, ' '));                    //095-102
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0103, 008, 0, "", ' '));                              //103-110
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0111, 007, 0, numeroArquivoRemessa.ToString(), '0')); //111-117
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0118, 273, 0, "", ' '));                              //118-390
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0391, 004, 0, "2.00", ' '));                          //391-394
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0395, 006, 0, "000001", ' '));                        //395-400
                //
                reg.CodificarLinha();
                //
                string vLinha = reg.LinhaRegistro;
                string _header = Utils.SubstituiCaracteresEspeciais(vLinha);
                //
                return _header;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar HEADER do arquivo de remessa do CNAB400.", ex);
            }
        }

        public string GerarDetalheRemessaCNAB400(Boleto boleto, int numeroRegistro, TipoArquivo tipoArquivo)
        {
            base.GerarDetalheRemessa(boleto, numeroRegistro, tipoArquivo);
            return GerarDetalheRemessaCNAB400_A(boleto, numeroRegistro, tipoArquivo);
        }
        public string GerarDetalheRemessaCNAB400_A(Boleto boleto, int numeroRegistro, TipoArquivo tipoArquivo)
        {
            try
            {
                TRegistroEDI reg = new TRegistroEDI();
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0001, 001, 0, "1", ' '));                                       //001-001
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0002, 001, 0, "A", ' '));                                       //002-002  'A' - SICREDI com Registro
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0003, 001, 0, "A", ' '));                                       //003-003  'A' - Simples
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0004, 001, 0, boleto.TipoImpressao, ' '));                                       //004-004  'A' - Normal
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0005, 012, 0, string.Empty, ' '));                              //005-016
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0017, 001, 0, "A", ' '));                                       //017-017  Tipo de moeda: 'A' - REAL
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0018, 001, 0, "A", ' '));                                       //018-018  Tipo de desconto: 'A' - VALOR
                #region Codigo de Juros
                string CodJuros = "A";
                decimal ValorOuPercJuros = 0;
                if (boleto.JurosMora > 0)
                {
                    CodJuros = "A";
                    ValorOuPercJuros = boleto.JurosMora;
                }
                else if (boleto.PercJurosMora > 0)
                {
                    CodJuros = "B";
                    ValorOuPercJuros = boleto.PercJurosMora;
                }
                #endregion
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0019, 001, 0, CodJuros, ' '));                                  //019-019  Tipo de juros: 'A' - VALOR / 'B' PERCENTUAL
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0020, 028, 0, string.Empty, ' '));                              //020-047
                #region Nosso numero + DV
                string NossoNumero = boleto.NossoNumero.Replace("/", "").Replace("-", ""); // AA/BXXXXX-D
                string vAuxNossoNumeroComDV = NossoNumero;
                if (string.IsNullOrEmpty(boleto.DigitoNossoNumero) || NossoNumero.Length < 9)
                {
                    boleto.DigitoNossoNumero = DigNossoNumeroSicredi(boleto, true);
                    vAuxNossoNumeroComDV = NossoNumero + boleto.DigitoNossoNumero;
                }
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0048, 009, 0, vAuxNossoNumeroComDV, '0'));                      //048-056
                #endregion
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0057, 006, 0, string.Empty, ' '));                              //057-062
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediDataAAAAMMDD_________, 0063, 008, 0, boleto.DataProcessamento, ' '));                  //063-070
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0071, 001, 0, string.Empty, ' '));                              //071-071
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0072, 001, 0, "N", ' '));                                       //072-072 'N' - nao Postar e remeter para o beneficiário
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0073, 001, 0, string.Empty, ' '));                              //073-073
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0074, 001, 0, "B", ' '));                                       //074-074 'B' - Impressao é feita pelo Beneficiário
                if (boleto.TipoImpressao.Equals("A"))
                {
                    reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0075, 002, 0, 0, '0'));                                      //075-076
                    reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0077, 002, 0, 0, '0'));                                      //077-078
                }
                else if (boleto.TipoImpressao.Equals("B"))
                {
                    reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0075, 002, 0, boleto.NumeroParcela, '0'));                   //075-076
                    reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0077, 002, 0, boleto.TotalParcela, '0'));                    //077-078
                }
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0079, 004, 0, string.Empty, ' '));                              //079-082
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0083, 010, 2, boleto.ValorDescontoAntecipacao, '0'));           //083-092
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0093, 004, 2, boleto.PercMulta, '0'));                          //093-096
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0097, 012, 0, string.Empty, ' '));                              //097-108
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0109, 002, 0, ObterCodigoDaOcorrencia(boleto), ' '));           //109-110 01 - Cadastro de titulo;
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0111, 010, 0, boleto.NumeroDocumento, ' '));                    //111-120
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediDataDDMMAA___________, 0121, 006, 0, boleto.DataVencimento, ' '));                     //121-126
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0127, 013, 2, boleto.ValorBoleto, '0'));                        //127-139
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0140, 009, 0, string.Empty, ' '));                              //140-148
                #region Especie de documento
                //Adota Duplicata Mercantil p/ Indicacao como padrao.
                var especieDoc = boleto.EspecieDocumento ?? new EspecieDocumento_Sicredi("A");
                #endregion
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0149, 001, 0, especieDoc.Codigo, ' '));                         //149-149
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0150, 001, 0, boleto.Aceite, ' '));                             //150-150
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediDataDDMMAA___________, 0151, 006, 0, boleto.DataProcessamento, ' '));                  //151-156
                #region Instrucoes
                string vInstrucao1 = "00"; //1o instrução (2, N) Caso Queira colocar um cod de uma instrução. ver no Manual caso nao coloca 00
                string vInstrucao2 = "00"; //2o instrução (2, N) Caso Queira colocar um cod de uma instrução. ver no Manual caso nao coloca 00
                foreach (IInstrucao instrucao in boleto.Instrucoes)
                {
                    switch ((EnumInstrucoes_Sicredi)instrucao.Codigo)
                    {
                        case EnumInstrucoes_Sicredi.AlteracaoOutrosDados_CancelamentoProtestoAutomatico:
                            vInstrucao1 = "00";
                            vInstrucao2 = "00";
                            break;
                        case EnumInstrucoes_Sicredi.PedidoProtesto:
                            vInstrucao1 = "06"; //Indicar o codigo "06" - (Protesto)
                            vInstrucao2 = Utils.FitStringLength(instrucao.QuantidadeDias.ToString(), 2, 2, '0', 0, true, true, true);
                            break;
                    }
                }
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0157, 002, 0, vInstrucao1, '0'));                               //157-158
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0159, 002, 0, vInstrucao2, '0'));                               //159-160
                #endregion               
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0161, 013, 2, ValorOuPercJuros, '0'));                          //161-173 Valor/% de juros por dia de atraso
                #region DataDesconto
                string vDataDesconto = "000000";
                if (!boleto.DataDesconto.Equals(DateTime.MinValue))
                    vDataDesconto = boleto.DataDesconto.ToString("ddMMyy");
                #endregion
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0174, 006, 0, vDataDesconto, '0'));                             //174-179
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0180, 013, 2, boleto.ValorDesconto, '0'));                      //180-192
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0193, 013, 0, 0, '0'));                                         //193-205
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0206, 013, 2, boleto.Abatimento, '0'));                         //206-218
                #region Regra Tipo de Inscricao Sacado
                string vCpfCnpjSac = "0";
                if (boleto.Sacado.CPFCNPJ.Length.Equals(11)) vCpfCnpjSac = "1"; //Cpf e sempre 11;
                else if (boleto.Sacado.CPFCNPJ.Length.Equals(14)) vCpfCnpjSac = "2"; //Cnpj e sempre 14;
                #endregion
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0219, 001, 0, vCpfCnpjSac, '0'));                               //219-219
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0220, 001, 0, "0", '0'));                                       //220-220
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0221, 014, 0, boleto.Sacado.CPFCNPJ, '0'));                     //221-234
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0235, 040, 0, boleto.Sacado.Nome.ToUpper(), ' '));              //235-274
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0275, 040, 0, boleto.Sacado.Endereco.EndComNumeroEComplemento.ToUpper(), ' '));      //275-314
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0315, 005, 0, 0, '0'));                                         //315-319
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0320, 006, 0, 0, '0'));                                         //320-325
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0326, 001, 0, string.Empty, ' '));                              //326-326
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0327, 008, 0, boleto.Sacado.Endereco.CEP, '0'));                //327-334
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0335, 005, 1, 0, '0'));                                         //335-339
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0340, 014, 0, string.Empty, ' '));                              //340-353
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0354, 041, 0, string.Empty, ' '));                              //354-394
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0395, 006, 0, numeroRegistro, '0'));                            //395-400
                //
                reg.CodificarLinha();
                //
                string _detalhe = Utils.SubstituiCaracteresEspeciais(reg.LinhaRegistro);
                //
                return _detalhe;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar DETALHE do arquivo CNAB400.", ex);
            }
        }

        public string GerarTrailerRemessa400(int numeroRegistro, Cedente cedente)
        {
            try
            {
                TRegistroEDI reg = new TRegistroEDI();
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0001, 001, 0, "9", ' '));                         //001-001
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0002, 001, 0, "1", ' '));                         //002-002
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0003, 003, 0, "748", ' '));                       //003-006
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0006, 005, 0, cedente.ContaBancaria.Conta, ' ')); //006-010
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0011, 384, 0, string.Empty, ' '));                //011-394
                reg.CamposEDI.Add(new TCampoRegistroEDI(TTiposDadoEDI.ediNumericoSemSeparador_, 0395, 006, 0, numeroRegistro, '0'));              //395-400
                //
                reg.CodificarLinha();
                //
                string vLinha = reg.LinhaRegistro;
                string _trailer = Utils.SubstituiCaracteresEspeciais(vLinha);
                //
                return _trailer;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro durante a geracao do registro TRAILER do arquivo de REMESSA.", ex);
            }
        }

        private string LerMotivoRejeicao(string codigorejeicao)
        {
            var rejeicao = String.Empty;

            if (codigorejeicao.Length >= 2)
            {
                #region LISTA DE MOTIVOS
                List<String> ocorrencias = new List<string>();

                ocorrencias.Add("01-Codigo do banco inválido");
                ocorrencias.Add("02-Codigo do registro detalhe inválido");
                ocorrencias.Add("03-Codigo da ocorrencia inválido");
                ocorrencias.Add("04-Codigo de ocorrencia nao permitida para a carteira");
                ocorrencias.Add("05-Codigo de ocorrencia nao numérico");
                ocorrencias.Add("07-Cooperativa/Agencia/conta/Digito inválidos");
                ocorrencias.Add("08-Nosso numero inválido");
                ocorrencias.Add("09-Nosso numero duplicado");
                ocorrencias.Add("10-Carteira inválida");
                ocorrencias.Add("14-Titulo protestado");
                ocorrencias.Add("15-Cooperativa/carteira/Agencia/conta/nosso numero inválidos");
                ocorrencias.Add("16-Data de vencimento inválida");
                ocorrencias.Add("17-Data de vencimento anterior à data de emissao");
                ocorrencias.Add("18-Vencimento fora do prazo de operacao");
                ocorrencias.Add("20-Valor do titulo inválido");
                ocorrencias.Add("21-Especie do titulo inválida");
                ocorrencias.Add("22-Especie nao permitida para a carteira");
                ocorrencias.Add("24-Data de emissao inválida");
                ocorrencias.Add("29-Valor do desconto maior/igual ao valor do titulo");
                ocorrencias.Add("31-Concessao de desconto - existe desconto anterior");
                ocorrencias.Add("33-Valor do abatimento inválido");
                ocorrencias.Add("34-Valor do abatimento maior/igual ao valor do titulo");
                ocorrencias.Add("36-Concessao de abatimento - existe abatimento anterior");
                ocorrencias.Add("38-Prazo para protesto inválido");
                ocorrencias.Add("39-Pedido para protesto nao permitido para o titulo");
                ocorrencias.Add("40-Titulo com ordem de protesto emitida");
                ocorrencias.Add("41-Pedido cancelamento/sustação sem instrução de protesto");
                ocorrencias.Add("44-Cooperativa de credito/Agencia beneficiária nao prevista");
                ocorrencias.Add("45-Nome do pagador inválido");
                ocorrencias.Add("46-Tipo/numero de inscricao do pagador inválidos");
                ocorrencias.Add("47-Endereço do pagador nao informado");
                ocorrencias.Add("48-CEP irregular");
                ocorrencias.Add("49-numero de Inscricao do pagador/avalista inválido");
                ocorrencias.Add("50-Pagador/avalista nao informado");
                ocorrencias.Add("60-Movimento para titulo nao cadastrado");
                ocorrencias.Add("63-Entrada para titulo ja cadastrado");
                ocorrencias.Add("A -Aceito");
                ocorrencias.Add("A1-Praça do pagador nao cadastrada.");
                ocorrencias.Add("A2-Tipo de Cobranca do titulo divergente com a praça do pagador.");
                ocorrencias.Add("A3-Cooperativa/Agencia depositária divergente: atualiza o cadastro de praças da Coop./Agencia beneficiária");
                ocorrencias.Add("A4-Beneficiário nao cadastrado ou possui CGC/CIC inválido");
                ocorrencias.Add("A5-Pagador nao cadastrado");
                ocorrencias.Add("A6-Data da instrução/ocorrencia inválida");
                ocorrencias.Add("A7-Ocorrencia nao pode ser comandada");
                ocorrencias.Add("A8-Recebimento da liquidacao fora da rede Sicredi - via compensação eletrônica");
                ocorrencias.Add("B4-Tipo de moeda inválido");
                ocorrencias.Add("B5-Tipo de desconto/juros inválido");
                ocorrencias.Add("B6-Mensagem padrao nao cadastrada");
                ocorrencias.Add("B7-Seu numero inválido");
                ocorrencias.Add("B8-Percentual de multa inválido");
                ocorrencias.Add("B9-Valor ou percentual de juros inválido");
                ocorrencias.Add("C1-Data limite para concessao de desconto inválida");
                ocorrencias.Add("C2-Aceite do titulo inválido");
                ocorrencias.Add("C3-Campo alterado na instrução “31 – alteração de outros dados” inválido");
                ocorrencias.Add("C4-Titulo ainda nao foi confirmado pela centralizadora");
                ocorrencias.Add("C5-Titulo rejeitado pela centralizadora");
                ocorrencias.Add("C6-Titulo ja liquidado");
                ocorrencias.Add("C7-Titulo ja baixado");
                ocorrencias.Add("C8-Existe mesma instrução pendente de confirmação para este titulo");
                ocorrencias.Add("C9-Instrucao previa de concessao de abatimento nao existe ou nao confirmada");
                ocorrencias.Add("D -Desprezado");
                ocorrencias.Add("D1-Titulo dentro do prazo de vencimento (em dia);");
                ocorrencias.Add("D2-Especie de documento nao permite protesto de titulo");
                ocorrencias.Add("D3-Titulo possui instrução de baixa pendente de confirmação");
                ocorrencias.Add("D4-Quantidade de mensagens padrao excede o limite permitido");
                ocorrencias.Add("D5-Quantidade inválida no pedido de boletos pre-impressos da Cobranca sem registro");
                ocorrencias.Add("D6-Tipo de impressao inválida para Cobranca sem registro");
                ocorrencias.Add("D7-Cidade ou Estado do pagador nao informado");
                ocorrencias.Add("D8-Sequencia para composicao do nosso numero do ano atual esgotada");
                ocorrencias.Add("D9-Registro mensagem para titulo nao cadastrado");
                ocorrencias.Add("E2-Registro complementar ao cadastro do titulo da Cobranca com e sem registro nao cadastrado");
                ocorrencias.Add("E3-Tipo de postagem inválido, diferente de S, N e branco");
                ocorrencias.Add("E4-Pedido de boletos pre-impressos");
                ocorrencias.Add("E5-Confirmação/rejeição para pedidos de boletos nao cadastrado");
                ocorrencias.Add("E6-Pagador/avalista nao cadastrado");
                ocorrencias.Add("E7-Informacao para atualização do valor do titulo para protesto inválido");
                ocorrencias.Add("E8-Tipo de impressao inválido, diferente de A, B e branco");
                ocorrencias.Add("E9-Codigo do pagador do titulo divergente com o codigo da cooperativa de credito");
                ocorrencias.Add("F1-Liquidado no sistema do cliente");
                ocorrencias.Add("F2-Baixado no sistema do cliente");
                ocorrencias.Add("F3-Instrucao inválida, este titulo esta caucionado/descontado");
                ocorrencias.Add("F4-Instrucao fixa com caracteres inválidos");
                ocorrencias.Add("F6-Nosso numero / numero da parcela fora de sequencia - total de parcelas inválido");
                ocorrencias.Add("F7-Falta de comprovante de prestacao de servico");
                ocorrencias.Add("F8-Nome do beneficiário incompleto / incorreto.");
                ocorrencias.Add("F9-CNPJ / CPF incompatível com o nome do pagador / Sacador Avalista");
                ocorrencias.Add("G1-CNPJ / CPF do pagador Incompatível com a espécie");
                ocorrencias.Add("G2-Titulo aceito: sem a assinatura do pagador");
                ocorrencias.Add("G3-Titulo aceito: rasurado ou rasgado");
                ocorrencias.Add("G4-Titulo aceito: falta titulo (cooperativa/ag. beneficiária devera enviá-lo);");
                ocorrencias.Add("G5-Praça de pagamento incompatível com o endereço");
                ocorrencias.Add("G6-Titulo aceito: sem endosso ou beneficiário irregular");
                ocorrencias.Add("G7-Titulo aceito: valor por extenso diferente do valor numérico");
                ocorrencias.Add("G8-Saldo maior que o valor do titulo");
                ocorrencias.Add("G9-Tipo de endosso inválido");
                ocorrencias.Add("H1-Nome do pagador incompleto / Incorreto");
                ocorrencias.Add("H2-Sustação judicial");
                ocorrencias.Add("H3-Pagador nao encontrado");
                ocorrencias.Add("H4-alteracao de carteira");
                ocorrencias.Add("H5-Recebimento de liquidacao fora da rede Sicredi - VLB Inferior - Via Compensacao");
                ocorrencias.Add("H6-Recebimento de liquidacao fora da rede Sicredi - VLB Superior - Via Compensacao");
                ocorrencias.Add("H7-Especie de documento necessita beneficiário ou avalista PJ");
                ocorrencias.Add("H8-Recebimento de liquidacao fora da rede Sicredi - Contingência Via Compe");
                ocorrencias.Add("H9-Dados do titulo nao conferem com disquete");
                ocorrencias.Add("I1-Pagador e Sacador Avalista sao a mesma pessoa");
                ocorrencias.Add("I2-Aguardar um dia útil após o vencimento para protestar");
                ocorrencias.Add("I3-Data do vencimento rasurada");
                ocorrencias.Add("I4-Vencimento - extenso nao confere com numero");
                ocorrencias.Add("I5-Falta data de vencimento no titulo");
                ocorrencias.Add("I6-DM/DMI sem comprovante autenticado ou declaração");
                ocorrencias.Add("I7-Comprovante ilegível para conferência e microfilmagem");
                ocorrencias.Add("I8-Nome solicitado nao confere com emitente ou pagador");
                ocorrencias.Add("I9-Confirmar se sao 2 emitentes. Se sim, indicar os dados dos 2");
                ocorrencias.Add("J1-Endereço do pagador igual ao do pagador ou do portador");
                ocorrencias.Add("J2-Endereço do apresentante incompleto ou nao informado");
                ocorrencias.Add("J3-Rua/numero inexistente no endereço");
                ocorrencias.Add("J4-Falta endosso do favorecido para o apresentante");
                ocorrencias.Add("J5-Data da emissao rasurada");
                ocorrencias.Add("J6-Falta assinatura do pagador no titulo");
                ocorrencias.Add("J7-Nome do apresentante nao informado/incompleto/incorreto");
                ocorrencias.Add("J8-Erro de preenchimento do titulo");
                ocorrencias.Add("J9-Titulo com direito de regresso vencido");
                ocorrencias.Add("K1-Titulo apresentado em duplicidade");
                ocorrencias.Add("K2-Titulo ja protestado");
                ocorrencias.Add("K3-Letra de cambio vencida - falta aceite do pagador");
                ocorrencias.Add("K4-Falta declaração de saldo assinada no titulo");
                ocorrencias.Add("K5-Contrato de cambio - Falta conta gráfica");
                ocorrencias.Add("K6-Ausência do documento físico");
                ocorrencias.Add("K7-Pagador falecido");
                ocorrencias.Add("K8-Pagador apresentou quitação do titulo");
                ocorrencias.Add("K9-Titulo de outra jurisdição territorial");
                ocorrencias.Add("L1-Titulo com emissao anterior a concordata do pagador");
                ocorrencias.Add("L2-Pagador consta na lista de falência");
                ocorrencias.Add("L3-Apresentante nao aceita publicacão de edital");
                ocorrencias.Add("L4-Dados do Pagador em Branco ou inválido");
                ocorrencias.Add("L5-Codigo do Pagador na Agencia beneficiária esta duplicado");
                ocorrencias.Add("M1-Reconhecimento da dívida pelo pagador");
                ocorrencias.Add("M2-nao reconhecimento da dívida pelo pagador");
                ocorrencias.Add("M3-Inclusao de desconto 2 e desconto 3 inválida");
                ocorrencias.Add("X0-Pago com cheque");
                ocorrencias.Add("X1-Regularização centralizadora - Rede Sicredi");
                ocorrencias.Add("X2-Regularização centralizadora - Compensacao");
                ocorrencias.Add("X3-Regularização centralizadora - Banco correspondente");
                ocorrencias.Add("X4-Regularização centralizadora - VLB Inferior - via compensação");
                ocorrencias.Add("X5-Regularização centralizadora - VLB Superior - via compensação");
                ocorrencias.Add("X6-Pago com cheque - bloqueado 24 horas");
                ocorrencias.Add("X7-Pago com cheque - bloqueado 48 horas");
                ocorrencias.Add("X8-Pago com cheque - bloqueado 72 horas");
                ocorrencias.Add("X9-Pago com cheque - bloqueado 96 horas");
                ocorrencias.Add("XA-Pago com cheque - bloqueado 120 horas");
                ocorrencias.Add("XB-Pago com cheque - bloqueado 144 horas");
                #endregion

                var ocorrencia = (from s in ocorrencias where s.Substring(0, 2) == codigorejeicao.Substring(0, 2) select s).FirstOrDefault();

                if (ocorrencia != null)
                    rejeicao = ocorrencia;
            }

            return rejeicao;
        }

        // 7.3 Tabela de Motivos da Ocorrencia "28 - Tarifas" Maio 2020 v1.6
        private string LerMotivoRejeicaoTarifas(string codigorejeicao) {
            var rejeicao = String.Empty;

            if (codigorejeicao.Length >= 2) {
                #region LISTA DE MOTIVOS
                List<String> ocorrencias = new List<string>();

                ocorrencias.Add("03-Tarifa de sustação");
                ocorrencias.Add("04-Tarifa de protesto");
                ocorrencias.Add("08-Tarifa de custas de protesto");
                ocorrencias.Add("A9-Tarifa de manuteção de titulo vencido");
                ocorrencias.Add("B1-Tarifa de baixa da carteira");
                ocorrencias.Add("B3-Tarifa de registro de entrada do titulo");
                ocorrencias.Add("F5-Tarifa de entrada na rede Sicredi");
                ocorrencias.Add("S4-Tarifa de Inclusao Negativação");
                ocorrencias.Add("S5-Tarifa de Exclusao Negativação");
                #endregion

                var ocorrencia = (from s in ocorrencias where s.Substring(0, 2) == codigorejeicao.Substring(0, 2) select s).FirstOrDefault();

                if (ocorrencia != null)
                    rejeicao = ocorrencia;
            }

            return rejeicao;
        }

        public override DetalheRetorno LerDetalheRetornoCNAB400(string registro)
        {
            try
            {
                TRegistroEDI_Sicredi_Retorno reg = new TRegistroEDI_Sicredi_Retorno();
                //
                reg.LinhaRegistro = registro;
                reg.DecodificarLinha();

                //Passa para o detalhe as propriedades de reg;
                DetalheRetorno detalhe = new DetalheRetorno(registro);
                //
                detalhe.IdentificacaoDoRegistro = Utils.ToInt32(reg.IdentificacaoRegDetalhe);
                //Filler1
                //TipoCobranca
                //CodigoPagadorAgenciaBeneficiario
                detalhe.NomeSacado = reg.CodigoPagadorJuntoAssociado;
                //BoletoDDA
                //Filler2
                #region NossoNumeroSicredi
                detalhe.NossoNumeroComDV = reg.NossoNumeroSicredi;
                detalhe.NossoNumero = reg.NossoNumeroSicredi.Substring(0, reg.NossoNumeroSicredi.Length - 1); //Nosso numero sem o DV!
                detalhe.DACNossoNumero = reg.NossoNumeroSicredi.Substring(reg.NossoNumeroSicredi.Length - 1); //DV do Nosso Numero
                #endregion
                //Filler3
                detalhe.CodigoOcorrencia = Utils.ToInt32(reg.Ocorrencia);
                int dataOcorrencia = Utils.ToInt32(reg.DataOcorrencia);
                detalhe.DataOcorrencia = Utils.ToDateTime(dataOcorrencia.ToString("##-##-##"));

                //Descrição da ocorrencia
                detalhe.DescricaoOcorrencia = new CodigoMovimento(748, detalhe.CodigoOcorrencia).Descricao;

                detalhe.NumeroDocumento = reg.SeuNumero;
                //Filler4
                if (!String.IsNullOrEmpty(reg.DataVencimento))
                {
                    int dataVencimento = Utils.ToInt32(reg.DataVencimento);
                    detalhe.DataVencimento = Utils.ToDateTime(dataVencimento.ToString("##-##-##"));
                }
                decimal valorTitulo = Convert.ToInt64(reg.ValorTitulo);
                detalhe.ValorTitulo = valorTitulo / 100;
                //Filler5
                //Despesas de Cobranca para os Codigos de Ocorrencia (Valor Despesa)
                if (!String.IsNullOrEmpty(reg.DespesasCobranca))
                {
                    decimal valorDespesa = Convert.ToUInt64(reg.DespesasCobranca);
                    detalhe.ValorDespesa = valorDespesa / 100;
                }
                //Outras despesas Custas de Protesto (Valor Outras Despesas)
                if (!String.IsNullOrEmpty(reg.DespesasCustasProtesto))
                {
                    decimal valorOutrasDespesas = Convert.ToUInt64(reg.DespesasCustasProtesto);
                    detalhe.ValorOutrasDespesas = valorOutrasDespesas / 100;
                }
                //Filler6
                //Abatimento Concedido sobre o Titulo (Valor Abatimento Concedido)
                decimal valorAbatimento = Convert.ToUInt64(reg.AbatimentoConcedido);
                detalhe.ValorAbatimento = valorAbatimento / 100;
                //Desconto Concedido (Valor Desconto Concedido)
                decimal valorDesconto = Convert.ToUInt64(reg.DescontoConcedido);
                detalhe.Descontos = valorDesconto / 100;
                //Valor Pago
                decimal valorPago = Convert.ToUInt64(reg.ValorEfetivamentePago);
                detalhe.ValorPago = valorPago / 100;
                //Juros Mora
                decimal jurosMora = Convert.ToUInt64(reg.JurosMora);
                detalhe.JurosMora = jurosMora / 100;
                //Filler7
                //SomenteOcorrencia19
                //Filler8
                detalhe.MotivoCodigoOcorrencia = reg.MotivoOcorrencia;
                int dataCredito = Utils.ToInt32(reg.DataPrevistaLancamentoContaCorrente);
                detalhe.DataCredito = Utils.ToDateTime(dataCredito.ToString("####-##-##"));
                //Filler9
                detalhe.NumeroSequencial = Utils.ToInt32(reg.NumeroSequencialRegistro);
                //
                #region NAO RETORNADOS PELO SICREDI
                //detalhe.Especie = reg.TipoDocumento; //Verificar Especie de Documentos...
                detalhe.OutrosCreditos = 0;
                detalhe.OrigemPagamento = String.Empty;
                detalhe.MotivoCodigoOcorrencia = reg.MotivoOcorrencia;
                //
                detalhe.IOF = 0;
                //Motivos das Rejeições para os Codigos de Ocorrencia
                if (detalhe.CodigoOcorrencia == 28) {
                    detalhe.MotivosRejeicao = LerMotivoRejeicaoTarifas(detalhe.MotivoCodigoOcorrencia);
                } else {
                    detalhe.MotivosRejeicao = LerMotivoRejeicao(detalhe.MotivoCodigoOcorrencia);
                }
                
                //numero do Cartório
                detalhe.NumeroCartorio = 0;
                //numero do Protocolo
                detalhe.NumeroProtocolo = string.Empty;

                detalhe.CodigoInscricao = 0;
                detalhe.NumeroInscricao = string.Empty;
                detalhe.Agencia = 0;
                detalhe.Conta = header.Conta;
                detalhe.DACConta = header.DACConta;

                detalhe.NumeroControle = string.Empty;
                detalhe.IdentificacaoTitulo = string.Empty;
                //Banco Cobrador
                detalhe.CodigoBanco = 0;
                //Agencia Cobradora
                detalhe.AgenciaCobradora = 0;
                #endregion
                //
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
                header = new HeaderRetorno(registro);
                header.TipoRegistro = Utils.ToInt32(registro.Substring(000, 1));
                header.CodigoRetorno = Utils.ToInt32(registro.Substring(001, 1));
                header.LiteralRetorno = registro.Substring(002, 7);
                header.CodigoServico = Utils.ToInt32(registro.Substring(009, 2));
                header.LiteralServico = registro.Substring(011, 15);
                string _conta = registro.Substring(026, 5);
                header.Conta = Utils.ToInt32(_conta.Substring(0, _conta.Length - 1));
                header.DACConta = Utils.ToInt32(_conta.Substring(_conta.Length - 1));
                header.ComplementoRegistro2 = registro.Substring(031, 14);
                header.CodigoBanco = Utils.ToInt32(registro.Substring(076, 3));
                header.NomeBanco = registro.Substring(079, 15);
                header.DataGeracao = Utils.ToDateTime(Utils.ToInt32(registro.Substring(094, 8)).ToString("##-##-##"));
                header.NumeroSequencialArquivoRetorno = Utils.ToInt32(registro.Substring(110, 7));
                header.Versao = registro.Substring(390, 5);
                header.NumeroSequencial = Utils.ToInt32(registro.Substring(394, 6));



                return header;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao ler header do arquivo de RETORNO / CNAB 400.", ex);
            }
        }

        #endregion

        public override long ObterNossoNumeroSemConvenioOuDigitoVerificador(long convenio, string nossoNumero)
        {
            long num;
            if (nossoNumero.Length >= 8 && long.TryParse(nossoNumero.Substring(0, 8), out num))
            {
                return num;
            }
            throw new BoletoNetException("Nosso numero é inválido!");
        }

        public override string GerarNomeRemessa(Cedente cedente, string cidadeBanco, int remessa)
        {
            var month = DateTime.Now.Month;
            string mes;
            switch (month)
            {
                case 10:
                    mes = "O";
                    break;
                case 11:
                    mes = "N";
                    break;
                case 12:
                    mes = "D";
                    break;
                default:
                    mes = month.ToString();
                    break;
            }


            return $"{cedente.ContaBancaria.Conta}{mes}{DateTime.Now.ToString("dd")}.crm";
        }
    }
}
