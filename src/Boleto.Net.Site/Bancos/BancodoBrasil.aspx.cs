using System;
using BoletoNet;

public partial class Bancos_BancodoBrasil : System.Web.UI.Page
{
    void Page_PreInit(object sender, EventArgs e)
    {
        if (IsPostBack)
            MasterPageFile = "~/MasterPrint.master";
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        DateTime vencimento = DateTime.Now.AddDays(5);

        #region Exemplo Carteira 16, com nosso numero de 11 posicoes
        /*
         * Nesse exemplo utilizamos a carteira 16 e o nosso numero no máximo de 11 posicoes.
         * nao é necessário informar o numero do Convenio e nem o tipo da modalidade.
         * O nosso numero tem que ter no máximo 11 posicoes.
         */

        Cedente c = new Cedente("00.000.000/0000-00", "Empresa de Atacado", "1234", "1", "123456", "1");
        Boleto b = new Boleto(vencimento, 0.01m, "16", "09876543210", c);

        #endregion Exemplo Carteira 16, com nosso numero de 11 posicoes

        #region Exemplo Carteira 16, Convenio de 6 posicoes e tipo modalidade 21
        /*
         * Nesse exemplo utilizamos a carteira 16 e o numero do Convenio de 6 posicoes.
         * é obrigatorio informar o tipo da modalidade 21.
         * O nosso numero tem que ter no máximo 10 posicoes.
         */

        //Cedente c = new Cedente("00.000.000/0000-00", "Empresa de Atacado", "1234", "1", "123456", "1");
        //c.Convenio = 123456;

        //Boleto b = new Boleto(vencimento, 0.01, "16", "09876543210", c);
        //b.TipoModalidade = "21";
        #endregion Exemplo Carteira 16, Convenio de 6 posicoes e tipo modalidade 21

        #region Exemplo Carteira 18, com nosso numero de 11 posicoes
        /*
         * Nesse exemplo utilizamos a carteira 18 e o nosso numero no máximo de 11 posicoes.
         * nao é necessário informar o numero do Convenio e nem o tipo da modalidade.
         * O nosso numero tem que ter no máximo 11 posicoes.
         */

        //Cedente c = new Cedente("00.000.000/0000-00", "Empresa de Atacado", "1234", "1", "123456", "1");
        //Boleto b = new Boleto(vencimento, 0.01, "18", "09876543210", c);

        #endregion Exemplo Carteira 18, com nosso numero de 11 posicoes

        #region Exemplo Carteira 18, Convenio de 6 posicoes e tipo modalidade 21
        /*
         * Nesse exemplo utilizamos a carteira 18 e o numero do Convenio de 6 posicoes.
         * é obrigatorio informar o tipo da modalidade 21.
         * O nosso numero tem que ter no máximo 10 posicoes.
         */

        //Cedente c = new Cedente("00.000.000/0000-00", "Empresa de Atacado", "1234", "1", "123456", "1");
        //c.Convenio = 123456;
        //Boleto b = new Boleto(vencimento, 0.01, "18", "09876543210", c);
        //b.TipoModalidade = "21";
        #endregion Exemplo Carteira 18, Convenio de 6 posicoes e tipo modalidade 21


        b.NumeroDocumento = "12415487";

        b.Sacado = new Sacado("000.000.000-00", "Nome do seu Cliente ");
        b.Sacado.Endereco.End = "Endereço do seu Cliente ";
        b.Sacado.Endereco.Bairro = "Bairro";
        b.Sacado.Endereco.Cidade = "Cidade";
        b.Sacado.Endereco.CEP = "00000000";
        b.Sacado.Endereco.UF = "UF";     

        //Adiciona as instruções ao boleto
        #region Instrucoes
        //Protestar
        Instrucao_BancoBrasil item = new Instrucao_BancoBrasil(9, 5);
        b.Instrucoes.Add(item);
        //ImportanciaporDiaDesconto
        item = new Instrucao_BancoBrasil(30, 0);
        b.Instrucoes.Add(item);
        //ProtestarAposNDiasCorridos
        item = new Instrucao_BancoBrasil(81, 15);
        b.Instrucoes.Add(item);
        #endregion Instrucoes

        boletoBancario.Boleto = b;
        boletoBancario.Boleto.Valida();

        boletoBancario.AjustaTamanhoFonte(12, tamanhoFonteInstrucaoImpressao:14);

        boletoBancario.MostrarComprovanteEntrega = (Request.Url.Query == "?show");
        boletoBancario.OcultarInstrucoes = true;
        boletoBancario.FormatoPropaganda = (Request.Url.Query == "?formatopropaganda");
        boletoBancario.ImagemPropaganda = GetBase64StringForImage(Server.MapPath("/prop.jpg"));

        var bytes = boletoBancario.MontaBytesPDF();
        Response.Clear();
        Response.ContentType = "application/pdf";
        Response.AddHeader("Content-Disposition", "attachment;filename=\"FileName.pdf\"");
        Response.BinaryWrite(bytes);
        Response.Flush();
        Response.End();
    }

    protected static string GetBase64StringForImage(string imgPath)
    {
        byte[] imageBytes = System.IO.File.ReadAllBytes(imgPath);
        string base64String = Convert.ToBase64String(imageBytes);
        return base64String;
    }
}
