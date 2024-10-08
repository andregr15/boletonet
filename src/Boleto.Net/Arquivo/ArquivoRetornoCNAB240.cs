using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace BoletoNet
{
    public class ArquivoRetornoCNAB240 : AbstractArquivoRetorno, IArquivoRetorno
    {
        private readonly Stream _streamArquivo;
        //private string _caminhoArquivo;
        private List<DetalheRetornoCNAB240> _listaDetalhes = new List<DetalheRetornoCNAB240>();

        #region Propriedades
        //public string CaminhoArquivo
        //{
        //    get { return _caminhoArquivo; }
        //}
        public Stream StreamArquivo
        {
            get { return _streamArquivo; }
        }
        public List<DetalheRetornoCNAB240> ListaDetalhes
        {
            get { return _listaDetalhes; }
            set { _listaDetalhes = value; }
        }
        #endregion Propriedades
        
        #region Construtores

        public ArquivoRetornoCNAB240()
		{
            this.TipoArquivo = TipoArquivo.CNAB240;
        }

        public ArquivoRetornoCNAB240(Stream streamArquivo)
        {
            this.TipoArquivo = TipoArquivo.CNAB240;
            _streamArquivo = streamArquivo;
        }

        public ArquivoRetornoCNAB240(string caminhoArquivo)
        {
            this.TipoArquivo = TipoArquivo.CNAB240;

            _streamArquivo = new StreamReader(caminhoArquivo).BaseStream;
        }
        #endregion

        #region Metodos de instancia

        public void LerArquivoRetorno(IBanco banco)
        {
            LerArquivoRetorno(banco, StreamArquivo);
        }

        public override void LerArquivoRetorno(IBanco banco, Stream arquivo)
        {
            try
             {
                StreamReader stream = new StreamReader(arquivo, System.Text.Encoding.UTF8);
                string linha = "";
                var cnpjEmpresa = "";

                while ((linha = stream.ReadLine()) != null)
                {
                    if (!String.IsNullOrEmpty(linha))
                    {

                        DetalheRetornoCNAB240 detalheRetorno = new DetalheRetornoCNAB240();

                        switch (linha.Substring(7, 1))
                        {
                            case "0": //Header de arquivo
                                cnpjEmpresa = linha.Substring(18, 14);
                                OnLinhaLida(null, linha, EnumTipodeLinhaLida.HeaderDeArquivo);
                                break;
                            case "1": //Header de lote
                                OnLinhaLida(null, linha, EnumTipodeLinhaLida.HeaderDeLote);
                                break;
                            case "3": //Detalhe
                                if (linha.Substring(13, 1) == "W")
                                {
                                    OnLinhaLida(detalheRetorno, linha, EnumTipodeLinhaLida.DetalheSegmentoW);
                                    detalheRetorno.SegmentoW.LerDetalheSegmentoWRetornoCNAB240(linha);
                                }
                                else if (linha.Substring(13, 1) == "E")
                                {
                                    OnLinhaLida(detalheRetorno, linha, EnumTipodeLinhaLida.DetalheSegmentoE);
                                    detalheRetorno.SegmentoE = new DetalheSegmentoERetornoCNAB240();
                                    detalheRetorno.SegmentoE.LerDetalheSegmentoERetornoCNAB240(linha);
                                }
                                else if (linha.Substring(13, 1) == "Y")
                                {
                                    break;
                                }
                                else if (linha.Substring(13, 1) == "T")
                                {
                                    //Ira ler o Segmento T e em sequencia o Segmento U
                                    detalheRetorno.SegmentoT = banco.LerDetalheSegmentoTRetornoCNAB240(linha);
                                    detalheRetorno.SegmentoT.NumeroInscricao = cnpjEmpresa;
                                    linha = stream.ReadLine();
                                    detalheRetorno.SegmentoU = banco.LerDetalheSegmentoURetornoCNAB240(linha);

                                    OnLinhaLida(detalheRetorno, linha, EnumTipodeLinhaLida.DetalheSegmentoU);                                    
                                }
                                ListaDetalhes.Add(detalheRetorno);
                                break;
                            case "5": //Trailler de lote
                                OnLinhaLida(null, linha, EnumTipodeLinhaLida.TraillerDeLote);
                                break;
                            case "9": //Trailler de arquivo
                                OnLinhaLida(null, linha, EnumTipodeLinhaLida.TraillerDeArquivo);
                                break;
                        }

                    }

                }
                stream.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao ler arquivo.", ex);
            }
        }

        public List<DetalheRetorno> RetornoModeloGenerico()
        {
            var retornos = new List<DetalheRetorno>();

            foreach (var det in ListaDetalhes)
            {
                retornos.Add(SetDetalheRetorno(det));
            }

            return retornos;
        }

        private DetalheRetorno SetDetalheRetorno(DetalheRetornoCNAB240 det)
        {
            return new DetalheRetorno
            {
                CodigoInscricao = det.SegmentoT.TipoInscricao,
                NumeroInscricao = det.SegmentoT.NumeroInscricao,

                Agencia = det.SegmentoT.Agencia,
                Conta = det.SegmentoT.Conta,
                DACConta = Int32.Parse(det.SegmentoT.DigitoConta),
                NossoNumero = det.SegmentoT.NossoNumero.Substring(0, 9),
                DACNossoNumero = det.SegmentoT.NossoNumero.Substring(9, 1),

                Carteira = det.SegmentoT.CodigoCarteira.ToString(),
                CodigoOcorrencia = Int32.Parse(det.SegmentoU.CodigoOcorrenciaSacado),
                
                DataOcorrencia = det.SegmentoU.DataOcorrencia,
                DataLiquidacao = det.SegmentoU.DataOcorrencia,
                NumeroDocumento = det.SegmentoT.NumeroDocumento,
                DataVencimento = det.SegmentoT.DataVencimento,
                
                ValorTitulo = det.SegmentoT.ValorTitulo,
                
                DataCredito = det.SegmentoU.DataCredito,

                ValorDespesa = det.SegmentoT.ValorTarifas,
                OutrasDespesas = det.SegmentoU.ValorOutrasDespesas,
                Abatimentos = det.SegmentoU.ValorAbatimentoConcedido,
                Descontos = det.SegmentoU.ValorDescontoConcedido,
                ValorPago = det.SegmentoU.ValorPagoPeloSacado,
                JurosMora = det.SegmentoU.JurosMultaEncargos,
                IdentificacaoTitulo = det.SegmentoT.NumeroDocumento,
                OutrosCreditos = det.SegmentoU.ValorOutrosCreditos
            };
        }

        #endregion
    }
}
