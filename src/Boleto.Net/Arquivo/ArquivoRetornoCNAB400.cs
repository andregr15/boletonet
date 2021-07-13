using BoletoNet.Enums;
using System;
using System.Collections.Generic;
using System.IO;

namespace BoletoNet
{
    public class ArquivoRetornoCNAB400 : AbstractArquivoRetorno, IArquivoRetorno
    {

        private HeaderRetorno _headerRetorno = new HeaderRetorno();
        private List<DetalheRetorno> _listaDetalhe = new List<DetalheRetorno>();

        public List<DetalheRetorno> ListaDetalhe
        {
            get { return _listaDetalhe; }
            set { _listaDetalhe = value; }
        }

        public HeaderRetorno HeaderRetorno
        {
            get { return _headerRetorno; }
            set { _headerRetorno = value; }
        }

        #region Construtores

        public ArquivoRetornoCNAB400()
        {
            this.TipoArquivo = TipoArquivo.CNAB400;
        }

        #endregion

        #region M�todos de inst�ncia

        public override void LerArquivoRetorno(IBanco banco, Stream arquivo)
        {
            try
            {
                StreamReader stream = new StreamReader(arquivo, System.Text.Encoding.UTF8);
                // Identifica��o do registro detalhe
                List<string> IdsRegistroDetalhe = new List<string>();

                // Lendo o arquivo
                string linha = stream.ReadLine();
                this.HeaderRetorno = banco.LerHeaderRetornoCNAB400(linha);

                // Pr�xima linha (DETALHE)
                linha = stream.ReadLine();

                //tem arquivo de retorno que possui somente cabe�alho
                if (linha != null)
                {
                    switch (banco.Codigo)
                    {
                        // 85 - CECRED - C�digo de registro detalhe 7 para CECRED
                        case (int)Bancos.CECRED:
                            IdsRegistroDetalhe.Add("7");
                            break;
                        // 1 - Banco do Brasil- C�digo de registro detalhe 7 para conv�nios com 7 posi��es, e detalhe 1 para conv�nios com 6 posi��es(colocado as duas, pois n�o interferem em cada tipo de arquivo)
                        case (int)Bancos.BancoBrasil:
                            IdsRegistroDetalhe.Add("1");//Para conv�nios de 6 posi��es
                            IdsRegistroDetalhe.Add("7");//Para conv�nios de 7 posi��es
                            break;
                        default:
                            IdsRegistroDetalhe.Add("1");
                            break;
                    }

                    while (IdsRegistroDetalhe.Contains(DetalheRetorno.PrimeiroCaracter(linha)))
                    {
                        DetalheRetorno detalhe = banco.LerDetalheRetornoCNAB400(linha);
                        ListaDetalhe.Add(detalhe);
                        //OnLinhaLida(detalhe, linha);
                        linha = stream.ReadLine();
                    }

                    //TODO: Tratar Triller.

                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao ler arquivo.", ex);
            }
        }

        #endregion
    }
}
