using System;

namespace BoletoNet
{
    public class DetalheSegmentoERetornoCNAB240
    {
        /// <summary>
        /// Codigo fornecido pelo Banco Central para identificacao do Banco que esta recebendo ou enviando o arquivo, com o qual se firmou o contrato de prestacao de servicos.
        /// </summary>
        public int CodigoBanco { get; private set; }

        /// <summary>
        /// numero sequencial para identificar univocamente um lote de servico. Criado e controlado pelo responsavel pela geracao magnetica dos dados contidos no arquivo.
        /// Preencher com '0001' para o primeiro lote do arquivo. Para os demais: numero do lote anterior acrescido de 1. O numero nao podera ser repetido dentro do arquivo.
        /// Se registro for Header do Arquivo preencher com '0000'
        /// Se registro for Trailer do Arquivo preencher com '9999'
        /// </summary>
        public int LoteServico { get; private set; }

        /// <summary>
        /// Codigo adotado pela FEBRABAN para identificar o tipo de registro.
        /// Dominio:
        /// '0' = Header de Arquivo
        /// '1' = Header de Lote
        /// '2' = Registros Iniciais do Lote
        /// '3' = Detalhe
        /// '4' = Registros Finais do Lote
        /// '5' = Trailer de Lote
        /// '9' = Trailer de Arquivo
        /// </summary>
        public string TipoRegistro { get; private set; }

        /// <summary>
        /// numero adotado e controlado pelo responsavel pela geracao magnetica dos dados contidos no arquivo, para identificar a sequencia de registros encaminhados no lote.
        /// Deve ser inicializado sempre em '1', em cada novo lote.
        /// </summary>
        public int NumeroRegistro { get; private set; }

        /// <summary>
        /// Codigo adotado pela FEBRABAN para identificar o segmento do registro.
        /// </summary>
        public string Segmento { get; private set; }

        /// <summary>
        /// Texto de observacoes destinado para uso exclusivo da FEBRABAN.
        /// Preencher com Brancos.
        /// </summary>
        public string UsoExclusivoFebrabanCnab { get; private set; }

        /// <summary>
        /// Codigo que identifica o tipo de inscricao da Empresa ou Pessoa Fisica perante uma Instituicao governamental.Dominio:
        /// '0' = Isento / nao Informado
        /// '1' = CPF
        /// '2' = CGC / CNPJ
        /// '3' = PIS / PASEP
        /// '9' = Outros
        /// Preenchimento deste campo e obrigatorio para DOC e TED (Forma de Lancamento = 03, 41, 43)
        /// Para pagamento para o SIAPE com credito em conta, o CPF devera ser do 1o titular.
        /// </summary>
        public TipoInscricao TipoInscricaoCliente { get; private set; }

        /// <summary>
        /// numero de inscricao da Empresa ou Pessoa Fisica perante uma Instituicao governamental.
        /// </summary>
        public long NumeroInscricaoCliente { get; private set; }

        /// <summary>
        /// Codigo adotado pelo Banco para identificar o Contrato entre este e a Empresa Cliente.
        /// </summary>
        public string CodigoConvenioBanco { get; private set; }

        /// <summary>
        /// Codigo adotado pelo Banco responsavel pela conta, para identificar a qual unidade esta vinculada a conta corrente.
        /// </summary>
        public int AgenciaMantenedoraConta { get; private set; }

        /// <summary>
        /// Codigo adotado pelo Banco responsavel pela conta corrente, para verificacao da autenticidade do Codigo da Agencia.
        /// </summary>
        public string DigitoVerificadorAgencia { get; private set; }

        /// <summary>
        /// numero adotado pelo Banco, para identificar univocamente a conta corrente utilizada pelo Cliente.
        /// </summary>
        public long NumeroContaCorrente { get; private set; }

        /// <summary>
        /// Codigo adotado pelo responsavel pela conta corrente, para verificacao da autenticidade do numero da Conta Corrente.
        /// Para os Bancos que se utilizam de duas posicoes para o Digito Verificador do numero da Conta Corrente, preencher este campo com a 1o posicao deste Digito.
        /// Exemplo :
        /// numero C/C = 45981-36
        /// Neste caso -> Digito Verificador da Conta = 3
        /// </summary>
        public string DigitoVerificadorConta { get; private set; }

        /// <summary>
        /// Codigo adotado pelo Banco responsavel pela conta corrente, para verificacao da autenticidade do par Codigo da Agencia / numero da Conta Corrente.
        /// Para os Bancos que se utilizam de duas posicoes para o Digito Verificador do numero da Conta Corrente, preencher este campo com a 2o posicao deste Digito.
        /// Exemplo :
        /// numero C/C = 45981-36
        /// Neste caso -> Digito Verificador da Ag/Conta = 6
        /// </summary>
        public string DigitoVerificadorAgenciaConta { get; private set; }

        /// <summary>
        /// Nome que identifica a pessoa, fisica ou juridica, a qual se quer fazer referencia.
        /// </summary>
        public string NomeEmpresa { get; private set; }

        /// <summary>
        /// Texto de observacoes destinado para uso exclusivo da FEBRABAN.
        /// Preencher com Brancos.
        /// </summary>
        public string   UsoExclusivoFebrabanCnab2 { get; private set; }

        /// <summary>
        /// Identifica se o Lancamento incide sobre valores disponiveis ou bloqueados, possibilitando a recomposicao das posicoes dos saldos.
        /// Dominio:
        /// 'DPV' = TIPO DISPONIVEL
        /// Lancamento ocorrido em Saldo Disponivel
        /// 'SCR' = TIPO VINCULADO
        /// Lancamento ocorrido em Saldo Disponivel ou Vinculado (a criterio de cada banco), porem pendente de liberacao por regras internas do banco
        /// 'SSR' = TIPO BLOQUEADO
        /// Lancamento ocorrido em Saldo Bloqueado
        /// 'CDS' = COMPOSICAO DE DIVERSOS SALDOS
        /// Lancamento ocorrido em diversos saldos
        /// A condicao de recurso Disponivel, Vinculado ou Bloqueado para os codigos, SCR, SSR e CDS a criterio de cada banco.
        /// </summary>
        public string NaturezaLancamento { get; private set; }

        /// <summary>
        /// Codigo adotado pela FEBRABAN para identificar a padronizacao a ser utilizada no complemento.
        /// Dominio:
        /// '00' = Sem Informacao do Complemento do Lancamento
        /// '01' = Identificacao da Origem do Lancamento
        /// </summary>
        public TipoComplementoLancamento TipoComplementoLancamento { get; private set; }

        /// <summary>
        /// Texto de informacoes complementares ao Lancamento.
        /// Para Tipo do Complemento = 01, o campo complemento tera o seguinte formato:
        /// Banco Origem Lancamento 114 116 3 Num
        /// Agencia Origem Lancamento 117 121 5 Num
        /// Uso Exclusivo FEBRABAN/ CNAB 122 133 12 Alfa preencher com brancos
        /// </summary>
        public string ComplementoLancamento { get; private set; }

        /// <summary>
        /// Codigo adotado pela FEBRABAN para identificacao de Lancamentos desobrigados de recolhimento do CPMF.
        /// Dominio:
        /// 'S' = Isento
        /// 'N' = nao Isento
        /// </summary>
        public IsencaoCpmf IdentificacaoIsencaoCpmf { get; private set; }

        /// <summary>
        /// Data de efetivacao do Lancamento.
        /// Utilizar o formato DDMMAAAA, onde:
        /// DD = dia
        /// MM = mes
        /// AAAA = ano
        /// </summary>
        public DateTime? DataContabil { get; private set; }

        /// <summary>
        /// Data de ocorrencia dos fatos, itens, componentes do extrato bancario.
        /// Utilizar o formato DDMMAAAA, onde:
        /// DD = dia
        /// MM = mes
        /// AAAA = ano
        /// </summary>
        public DateTime DataLancamento { get; private set; }

        /// <summary>
        /// Valor do Lancamento efetuado, expresso em moeda corrente.
        /// </summary>
        public decimal ValorLancamento { get; private set; }

        /// <summary>
        /// Codigo adotado pela FEBRABAN para caracterizar o item que esta sendo representado no extrato bancario.
        /// Dominio:
        /// 'D' = Debito
        /// 'C' = Credito
        /// </summary>
        public TipoLancamento TipoLancamento { get; private set; }

        /// <summary>
        /// Codigo adotado pela FEBRABAN, para identificar a categoria padrao do Lancamento, para conciliacao entre Bancos.
        /// Dominio:
        /// Debitos:
        /// '101' = Cheques
        /// '102' = Encargos
        /// '103' = Estornos
        /// '104' = Lancamento Avisado
        /// '105' = Tarifas
        /// '106' = Aplicacao
        /// '107' = Emprestimo / Financiamento
        /// '108' = Cambio
        /// '109' = CPMF
        /// '110' = IOF
        /// '111' = Imposto de Renda
        /// '112' = Pagamento Fornecedores
        /// '113' = Pagamentos Salario
        /// '114' = Saque Eletronico
        /// '115' = Acoes
        /// '117' = Transferencia entre Contas
        /// '118' = Devolucao da Compensacao
        /// '119' = Devolucao de Cheque Depositado
        /// '120' = Transferencia Interbancaria (DOC, TED)
        /// '121' = Antecipacao a Fornecedores
        /// '122' = OC / AEROPS
        /// Creditos:
        /// '201' = Depositos
        /// '202' = Liquido de Cobranca
        /// '203' = Devolucao de Cheques
        /// '204' = Estornos
        /// '205' = Lancamento Avisado
        /// '206' = Resgate de Aplicacao
        /// '207' = Emprestimo / Financiamento
        /// '208' = Cambio
        /// '209' = Transferencia Interbancaria (DOC, TED)
        /// '210' = Acoes
        /// '211' = Dividendos
        /// '212' = Seguro
        /// '213' = Transferencia entre Contas
        /// '214' = Depositos Especiais
        /// '215' = Devolucao da Compensacao
        /// '216' = OCT
        /// '217' = Pagamentos Fornecedores
        /// '218' = Pagamentos Diversos
        /// '219' = Pagamentos Salarios
        /// </summary>
        public CategoriaLancamento CategoriaLancamento { get; private set; }

        /// <summary>
        /// Codigo adotado por cada Banco para identificar o descritivo do Lancamento. Observar que no Extrato de Conta Corrente para Conciliacao Bancaria este campo possui 4 caracteres, enquanto no Extrato para Gestao de Caixa ele possui 5 caracteres.
        /// </summary>
        public string CodigoHistorico { get; private set; }

        /// <summary>
        /// Texto descritivo do historico do Lancamento do extrato bancario.
        /// </summary>
        public string HistoricoLancamento { get; private set; }

        /// <summary>
        /// numero que identifica o documento que gerou o Lancamento. Para uso na conciliacao automatica de Conta Corrente, o numero do documento nao pode ser maior que 6 posicoes numericas. O complemento esta limitado de acordo com as restricoes de cada banco.
        /// </summary>
        public string NumeroDocumentoComplemento { get; private set; }

        public void LerDetalheSegmentoERetornoCNAB240(string registro)
        {
            try
            {
                if (registro.Substring(13, 1) != "E")
                    throw new Exception("Registro inválido. O detalhe nao possui as caracterasticas do segmento E.");

                CodigoBanco = LeitorLinhaPosicao.ExtrairInt32DaPosicao(registro, 1, 3);
                LoteServico = LeitorLinhaPosicao.ExtrairInt32DaPosicao(registro, 4, 7);
                TipoRegistro = LeitorLinhaPosicao.ExtrairDaPosicao(registro, 8, 8);
                NumeroRegistro = LeitorLinhaPosicao.ExtrairInt32DaPosicao(registro, 9, 13);
                Segmento = LeitorLinhaPosicao.ExtrairDaPosicao(registro, 14, 14);
                UsoExclusivoFebrabanCnab = LeitorLinhaPosicao.ExtrairDaPosicao(registro, 15, 17);
                TipoInscricaoCliente = (TipoInscricao) LeitorLinhaPosicao.ExtrairInt32DaPosicao(registro, 18, 18);
                NumeroInscricaoCliente = LeitorLinhaPosicao.ExtrairInt64DaPosicao(registro, 19, 32);
                CodigoConvenioBanco = LeitorLinhaPosicao.ExtrairDaPosicao(registro, 33, 52);
                AgenciaMantenedoraConta = LeitorLinhaPosicao.ExtrairInt32DaPosicao(registro, 53, 57);
                DigitoVerificadorAgencia = LeitorLinhaPosicao.ExtrairDaPosicao(registro, 58, 58);
                NumeroContaCorrente = LeitorLinhaPosicao.ExtrairInt64DaPosicao(registro, 59, 70);
                DigitoVerificadorConta = LeitorLinhaPosicao.ExtrairDaPosicao(registro, 71, 71);
                DigitoVerificadorAgenciaConta = LeitorLinhaPosicao.ExtrairDaPosicao(registro, 72, 72);
                NomeEmpresa = LeitorLinhaPosicao.ExtrairDaPosicao(registro, 73, 102);
                UsoExclusivoFebrabanCnab2 = LeitorLinhaPosicao.ExtrairDaPosicao(registro, 103, 108);
                NaturezaLancamento = LeitorLinhaPosicao.ExtrairDaPosicao(registro, 109, 111);
                TipoComplementoLancamento = (TipoComplementoLancamento) LeitorLinhaPosicao.ExtrairInt32OpcionalDaPosicao(registro, 112, 113).GetValueOrDefault();
                ComplementoLancamento = LeitorLinhaPosicao.ExtrairDaPosicao(registro, 114, 133);
                IdentificacaoIsencaoCpmf = (IsencaoCpmf)LeitorLinhaPosicao.ExtrairDaPosicao(registro, 134, 134)[0];
                DataContabil = LeitorLinhaPosicao.ExtrairDataOpcionalDaPosicao(registro, 135, 142);
                DataLancamento = LeitorLinhaPosicao.ExtrairDataDaPosicao(registro, 143, 150);
                ValorLancamento = decimal.Parse(LeitorLinhaPosicao.ExtrairDaPosicao(registro, 151, 168))/100m;
                TipoLancamento = (TipoLancamento) LeitorLinhaPosicao.ExtrairDaPosicao(registro, 169, 169)[0];
                CategoriaLancamento = (CategoriaLancamento) LeitorLinhaPosicao.ExtrairInt32DaPosicao(registro, 170, 172);
                CodigoHistorico = LeitorLinhaPosicao.ExtrairDaPosicao(registro, 173, 176);
                HistoricoLancamento = LeitorLinhaPosicao.ExtrairDaPosicao(registro, 177, 201);
                NumeroDocumentoComplemento = LeitorLinhaPosicao.ExtrairDaPosicao(registro, 202, 240);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao processar arquivo de RETORNO - SEGMENTO E.", ex);
            }
        }
    }
}
