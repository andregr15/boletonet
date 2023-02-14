namespace BoletoNet
{
    public enum TipoOcorrenciaRemessa
    {
        EntradaDeTitulos = 01, //Entrada de titulos
        PedidoDeBaixa = 02, //Pedido de Baixa
        ProtestoParaFinsFalimentares = 03, //Protesto para Fins Falimentares
        ConcessaoDeAbatimento = 04, //Concessao de Abatimento
        CancelamentoDeAbatimento = 05, //Cancelamento de Abatimento
        AlteracaoDeVencimento = 06, //alteracao de Vencimento
        ConcessaoDeDesconto = 07, //Concessao de Desconto
        CancelamentoDeDesconto = 08, //Cancelamento de Desconto
        Protestar = 09, //Protestar
        SustarProtestoBaixarTitulo = 10, //Sustar Protesto e Baixar Titulo
        SustarProtestoManterEmCarteira = 11, //Sustar Protesto e Manter em Carteira
        AlteracaoDeJurosDeMora = 12, //alteracao de Juros de Mora
        DispensarCobrancaDeJurosDeMora = 13, //Dispensar Cobranca de Juros de Mora
        AlteracaoDeValorPercentualDeMulta = 14, //alteracao de Valor/Percentual de Multa
        DispensarCobrancaDeMulta = 15, //Dispensar Cobranca de Multa
        AlteracaoDeValorDataDeDesconto = 16, //alteracao de Valor/Data de Desconto
        NaoConcederDesconto = 17, //nao conceder Desconto
        AlteracaoDoValorDeAbatimento = 18, //alteracao do Valor de Abatimento
        PrazoLimiteDeRecebimentoAlterar = 19, //Prazo Limite de Recebimento - Alterar
        PrazoLimiteDeRecebimentoDispensar = 20, //Prazo Limite de Recebimento - Dispensar
        AlterarNumeroDoTituloDadoPeloBeneficiario = 21, //Alterar numero do titulo dado pelo Beneficiário
        AlterarNumeroControleDoParticipante = 22, //Alterar numero controle do Participante
        AlterarDadosDoPagador = 23, //Alterar dados do Pagador
        AlterarDadosDoSacadorAvalista = 24, //Alterar dados do Sacador/Avalista
        RecusaDaAlegacaoDoPagador = 30, //Recusa da Alegação do Pagador
        AlteracaoDeOutrosDados = 31, //alteracao de Outros Dados
        AlteracaoDosDadosDoRateioDeCredito = 33, //alteracao dos Dados do Rateio de Credito
        PedidoDeCancelamentoDosDadosDoRateioDeCredito = 34, //Pedido de Cancelamento dos Dados do Rateio de Credito
        PedidoDeDesagendamentoDoDebitoAutomatico = 35, //Pedido de Desagendamento do Debito Automáico
        AlteracaoDeCarteira = 40, //alteracao de Carteira
        Cancelarprotesto = 41, //Cancelar protesto
        AlteracaoDeEspecieDeTitulo = 42, //alteracao de Especie de Titulo
        TransferenciaDeCarteiraModalidadeDeCobranca = 43, //Transferencia de carteira/modalidade de Cobranca
        AlteracaoDeContratoDeCobranca = 44, //alteracao de contrato de Cobranca
        NegativacaoSemProtesto = 45, //Negativação Sem Protesto
        SolicitacaoDeBaixaDeTituloNegativadoSemProtesto = 46, //Solicitação de Baixa de Titulo Negativado Sem Protesto
        AlteracaoDoValorNominalDoTitulo = 47, //alteracao do Valor Nominal do Titulo
    }
}