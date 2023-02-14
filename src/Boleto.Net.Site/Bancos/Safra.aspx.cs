using System;
using BoletoNet;

public partial class Bancos_Safra : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        DateTime vencimento = DateTime.Now.AddDays(5);

        Cedente c = new Cedente("00.000.000/0000-00", "Empresa de Atacado", "0542", "5413000");

        c.Codigo = "13000";

        Boleto b = new Boleto(vencimento, 1642, "198", "02592082835", c);
        b.NumeroDocumento = "1008073";

        b.Sacado = new Sacado("000.000.000-00", "Nome do seu Cliente ");
        b.Sacado.Endereco.End = "Endereço do seu Cliente ";
        b.Sacado.Endereco.Bairro = "Bairro";
        b.Sacado.Endereco.Cidade = "Cidade";
        b.Sacado.Endereco.CEP = "00000000";
        b.Sacado.Endereco.UF = "UF";

        //b.Instrucoes.Add("nao Receber após o vencimento");
        //b.Instrucoes.Add("Após o Vencimento pague somente no Bradesco");
        //b.Instrucoes.Add("Instrucao 2");
        //b.Instrucoes.Add("Instrucao 3");

        Instrucao_Safra instrucao = new Instrucao_Safra();
        instrucao.Descricao = "Instrucao 1";

        b.Instrucoes.Add(instrucao);


        boletoBancario.Boleto = b;
        boletoBancario.Boleto.Valida();

        boletoBancario.MostrarComprovanteEntrega = (Request.Url.Query == "?show");
    }
}
