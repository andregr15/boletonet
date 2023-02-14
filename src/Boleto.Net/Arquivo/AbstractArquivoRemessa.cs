using System;
using System.IO;

namespace BoletoNet
{
    /// <summary>
    /// Classe responsavel por representar (de forma abstrata) o modelo de Arquivo para Remessa
    /// </summary>
    public abstract class AbstractArquivoRemessa: IArquivoRemessa
    {
        /// <summary>
        /// Definicao do evento
        /// </summary>
        public event EventHandler<LinhaDeArquivoGeradaArgs> LinhaDeArquivoGerada;

        #region Variaveis
        private readonly IArquivoRemessa _arquivoRemessa;
        #endregion

        #region Construtores
        /// <summary>
        /// Cria uma nova instancia abstrata de Arquivo para Remessa
        /// </summary>
        protected AbstractArquivoRemessa()
        {
        }

        /// <summary>
        /// Cria uma nova instancia abstrata de Arquivo para Remessa
        /// </summary>
        /// <param name="tipoArquivo">Tipo de Arquivo de Remesssa que deve ser gerado</param>
        public AbstractArquivoRemessa(TipoArquivo tipoArquivo)
        {
            switch (tipoArquivo)
            {
                case TipoArquivo.CNAB240:
                    _arquivoRemessa = new ArquivoRemessaCNAB240();
                    _arquivoRemessa.LinhaDeArquivoGerada += new EventHandler<LinhaDeArquivoGeradaArgs>(_arquivoRemessa_LinhaDeArquivoGerada);
                    break;
                case TipoArquivo.CNAB240EmModoTeste:
                    _arquivoRemessa = new ArquivoRemessaCNAB240(){ ModoTeste = true };
                    _arquivoRemessa.LinhaDeArquivoGerada += new EventHandler<LinhaDeArquivoGeradaArgs>(_arquivoRemessa_LinhaDeArquivoGerada);
                    break;
                case TipoArquivo.CNAB400:
                    _arquivoRemessa = new ArquivoRemessaCNAB400();
                    _arquivoRemessa.LinhaDeArquivoGerada += new EventHandler<LinhaDeArquivoGeradaArgs>(_arquivoRemessa_LinhaDeArquivoGerada);
                    break;
                default:
                    throw new NotImplementedException("Arquivo não implementado.");
            }

        }

        /// <summary>
        /// Dispara evento quando a linha for gerada
        /// </summary>
        /// <param name="sender">Objeto que disparou evento</param>
        /// <param name="e">Argumentos</param>
        void _arquivoRemessa_LinhaDeArquivoGerada(object sender, LinhaDeArquivoGeradaArgs e)
        {
            OnLinhaGerada(e.Boleto, e.Linha, e.TipoLinha);
        }
        #endregion

        #region Propriedades
        /// <summary>
        /// Numero do convenio
        /// <remarks>
        /// Apenas alguns bancos trabalham com esse conceito.
        /// </remarks>
        /// </summary>
        public virtual string NumeroConvenio { get; set; }

        /// <summary>
        /// Numero do arquivo de remessa
        /// </summary>
        public virtual int NumeroArquivoRemessa { get; set; }

        /// <summary>
        /// Boletos
        /// <remarks>
        /// Lista com os boletos para geracao do arquivo
        /// </remarks>
        /// </summary>
        public virtual Boletos Boletos { get; protected set; }

        /// <summary>
        /// Cedente
        /// <remarks>
        /// Dados do Cedente
        /// </remarks>
        /// </summary>
        public virtual Cedente Cedente { get; protected set; }

        /// <summary>
        /// Dados do Banco
        /// </summary>
        public virtual IBanco Banco { get; protected set; }

        /// <summary>
        /// Tipo do Arquivo
        /// <remarks>
        /// CNAB240 / CNAB400 / CBR643 / Outro
        /// </remarks>
        /// </summary>
        public virtual TipoArquivo TipoArquivo { get; protected set; }
        #endregion

        #region Metodos
        /// <summary>
        /// Metodo que fara a verificacao se a classe esta devidamente implementada para a geracao da Remessa
        /// </summary>
        /// <param name="numeroConvenio">Numero do Convenio</param>
        /// <param name="banco">Banco</param>
        /// <param name="cedente">Dados do Cedente</param>
        /// <param name="boletos">Lista com Boletos para geracao da remessa</param>
        /// <param name="numeroArquivoRemessa">Numero do arquivo da remessa</param>
        /// <param name="mensagem">Mensagem</param>
        /// <returns></returns>
        public virtual bool ValidarArquivoRemessa(string numeroConvenio, IBanco banco, Cedente cedente, Boletos boletos, int numeroArquivoRemessa, out string mensagem)
        {
            try
            {
                return _arquivoRemessa.ValidarArquivoRemessa(numeroConvenio, banco, cedente, boletos, numeroArquivoRemessa, out mensagem);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Gera o arquivo de remessa
        /// </summary>
        /// <param name="numeroConvenio">Numero do Convenio</param>
        /// <param name="banco">Banco</param>
        /// <param name="cedente">Dados do Cedente</param>
        /// <param name="boletos">Lista com Boletos para geracao da remessa</param>
        /// <param name="arquivo">Arquivo (Stream / File)</param>
        /// <param name="numeroArquivoRemessa">Numero do arquivo da remessa</param>
        public virtual void GerarArquivoRemessa(string numeroConvenio, IBanco banco, Cedente cedente, Boletos boletos, Stream arquivo, int numeroArquivoRemessa)
        {
            this.Banco = banco;
            this.Cedente = cedente;
            this.Boletos = boletos;
            this.NumeroConvenio = numeroConvenio;
            this.NumeroArquivoRemessa = numeroArquivoRemessa;
            _arquivoRemessa.GerarArquivoRemessa(numeroConvenio, banco, cedente, boletos, arquivo, numeroArquivoRemessa);
        }
        #endregion

        #region Disparadores de Eventos
        /// <summary>
        /// evebto disparado a cada linha gerada no arquivo
        /// </summary>
        /// <param name="boleto">Dados do Boleto</param>
        /// <param name="linha">Linha</param>
        /// <param name="tipoLinha">Tipo da Linha do arquivo</param>
        public virtual void OnLinhaGerada(Boleto boleto, string linha, EnumTipodeLinha tipoLinha)
        {
            try
            {
                if (this.LinhaDeArquivoGerada != null)
                    this.LinhaDeArquivoGerada(this, new LinhaDeArquivoGeradaArgs(boleto, linha, tipoLinha));
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar evento.", ex);
            }
        }
        #endregion
    }
}
