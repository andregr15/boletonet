using System;

namespace BoletoNet
{
    #region Enumerado

    public enum EnumEspecieDocumento_Credisan
    {
        DuplicataMercantil = 1,
        NotaPromissoria = 2,
        NotaSeguro = 3,
        Recibo = 5,
        DuplicataRural = 6,
        LetraCambio = 8,
        Warrant = 9,
        Cheque = 10,
        DuplicataServico = 12,
        NotaDebito = 13,
        TriplicataMercantil = 14,
        TriplicataServico = 15,
        Fatura = 18,
        ApoliceSeguro = 20,
        MensalidadeEscolar = 21,
        ParcelaConsorcio = 22,
        Outros = 99,
    }

    #endregion

    public class EspecieDocumento_Credisan : AbstractEspecieDocumento, IEspecieDocumento
    {
        #region Construtores

        public EspecieDocumento_Credisan()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao carregar objeto", ex);
            }
        }

        public EspecieDocumento_Credisan(string codigo)
        {
            try
            {
                this.carregar(codigo);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao carregar objeto", ex);
            }
        }

        #endregion

        #region Metodos Privados

        public string getCodigoEspecieByEnum(EnumEspecieDocumento_Credisan especie)
        {
            return Convert.ToInt32(especie).ToString("00");
        }

        public EnumEspecieDocumento_Credisan getEnumEspecieByCodigo(string codigo)
        {
            return (EnumEspecieDocumento_Credisan)Convert.ToInt32(codigo);
        }

        private void carregar(string idCodigo)
        {
            try
            {
                this.Banco = new Banco_Credisan();

                switch (getEnumEspecieByCodigo(idCodigo))
                {
                    case EnumEspecieDocumento_Credisan.DuplicataMercantil:
                        this.Codigo = getCodigoEspecieByEnum(EnumEspecieDocumento_Credisan.DuplicataMercantil);
                        this.Especie = "Duplicata mercantil";
                        this.Sigla = "DM";
                        break;
                    case EnumEspecieDocumento_Credisan.NotaPromissoria:
                        this.Codigo = getCodigoEspecieByEnum(EnumEspecieDocumento_Credisan.NotaPromissoria);
                        this.Especie = "Nota promissória";
                        this.Sigla = "NP";
                        break;
                    case EnumEspecieDocumento_Credisan.NotaSeguro:
                        this.Codigo = getCodigoEspecieByEnum(EnumEspecieDocumento_Credisan.NotaSeguro);
                        this.Especie = "Nota de seguro";
                        this.Sigla = "NS";
                        break;
                    case EnumEspecieDocumento_Credisan.Recibo:
                        this.Codigo = getCodigoEspecieByEnum(EnumEspecieDocumento_Credisan.Recibo);
                        this.Especie = "Recibo";
                        this.Sigla = "RC";
                        break;
                    case EnumEspecieDocumento_Credisan.DuplicataRural:
                        this.Codigo = getCodigoEspecieByEnum(EnumEspecieDocumento_Credisan.DuplicataRural);
                        this.Especie = "Duplicata Rural";
                        this.Sigla = "DR";
                        break;
                    case EnumEspecieDocumento_Credisan.LetraCambio:
                        this.Codigo = getCodigoEspecieByEnum(EnumEspecieDocumento_Credisan.LetraCambio);
                        this.Sigla = "LC";
                        this.Especie = "Letra de Câmbio";
                        break;
                    case EnumEspecieDocumento_Credisan.Warrant:
                        this.Codigo = getCodigoEspecieByEnum(EnumEspecieDocumento_Credisan.Warrant);
                        this.Sigla = "WR";
                        this.Especie = "Warrant";
                        break;
                    case EnumEspecieDocumento_Credisan.Cheque:
                        this.Codigo = getCodigoEspecieByEnum(EnumEspecieDocumento_Credisan.Cheque);
                        this.Sigla = "CH";
                        this.Especie = "Cheque";
                        break;
                    case EnumEspecieDocumento_Credisan.DuplicataServico:
                        this.Codigo = getCodigoEspecieByEnum(EnumEspecieDocumento_Credisan.DuplicataServico);
                        this.Sigla = "DS";
                        this.Especie = "Duplicata de serviço";
                        break;
                    case EnumEspecieDocumento_Credisan.NotaDebito:
                        this.Codigo = getCodigoEspecieByEnum(EnumEspecieDocumento_Credisan.NotaDebito);
                        this.Sigla = "ND";
                        this.Especie = "Nota de débito";
                        break;
                    case EnumEspecieDocumento_Credisan.TriplicataMercantil:
                        this.Codigo = getCodigoEspecieByEnum(EnumEspecieDocumento_Credisan.TriplicataMercantil);
                        this.Sigla = "TP";
                        this.Especie = "Triplicata Mercantil";
                        break;
                    case EnumEspecieDocumento_Credisan.TriplicataServico:
                        this.Codigo = getCodigoEspecieByEnum(EnumEspecieDocumento_Credisan.TriplicataServico);
                        this.Sigla = "TS";
                        this.Especie = "Triplicata de Serviço";
                        break;
                    case EnumEspecieDocumento_Credisan.Fatura:
                        this.Codigo = getCodigoEspecieByEnum(EnumEspecieDocumento_Credisan.Fatura);
                        this.Sigla = "FT";
                        this.Especie = "Fatura";
                        break;
                    case EnumEspecieDocumento_Credisan.ApoliceSeguro:
                        this.Codigo = getCodigoEspecieByEnum(EnumEspecieDocumento_Credisan.ApoliceSeguro);
                        this.Sigla = "AP";
                        this.Especie = "Apólice de Seguro";
                        break;
                    case EnumEspecieDocumento_Credisan.MensalidadeEscolar:
                        this.Codigo = getCodigoEspecieByEnum(EnumEspecieDocumento_Credisan.MensalidadeEscolar);
                        this.Sigla = "ME";
                        this.Especie = "Mensalidade Escolar";
                        break;
                    case EnumEspecieDocumento_Credisan.ParcelaConsorcio:
                        this.Codigo = getCodigoEspecieByEnum(EnumEspecieDocumento_Credisan.ParcelaConsorcio);
                        this.Sigla = "PC";
                        this.Especie = "Parcela de Consórcio";
                        break;
                    case EnumEspecieDocumento_Credisan.Outros:
                        this.Codigo = getCodigoEspecieByEnum(EnumEspecieDocumento_Credisan.Outros);
                        this.Especie = "Outros";
                        this.Sigla = "OU";
                        break;
                    default:
                        this.Codigo = "0";
                        this.Especie = "( Selecione )";
                        this.Sigla = "";
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao carregar objeto", ex);
            }
        }

        public static EspeciesDocumento CarregaTodas()
        {
            try
            {
                var alEspeciesDocumento = new EspeciesDocumento();

                var obj = new EspecieDocumento_Credisan();

                foreach (var item in Enum.GetValues(typeof (EnumEspecieDocumento_Credisan)))
                {
                    obj = new EspecieDocumento_Credisan(obj.getCodigoEspecieByEnum((EnumEspecieDocumento_Credisan)item));
                    alEspeciesDocumento.Add(obj);
                }

                return alEspeciesDocumento;

            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao listar objetos", ex);
            }
        }

        public override IEspecieDocumento DuplicataMercantil()
        {
            return new EspecieDocumento_Credisan(getCodigoEspecieByEnum(EnumEspecieDocumento_Credisan.DuplicataMercantil));
        }

        #endregion
    }
}