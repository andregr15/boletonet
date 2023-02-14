namespace BoletoNet
{
    /// <summary>
    /// Classe para representacao de Conta Bancaria
    /// </summary>
    public class ContaBancaria
    {
        #region Constructors
        /// <summary>
        /// Cria uma nova instancia de conta bancária
        /// </summary>
        public ContaBancaria()
        {
        }

        /// <summary>
        /// Cria uma nova instancia de conta bancária
        /// </summary>
        /// <param name="agencia">numero da Agencia</param>
        /// <param name="conta">numero da Conta</param>
        public ContaBancaria(string agencia, string conta)
        {
            this.Agencia = agencia;
            this.Conta = conta;
        }

        /// <summary>
        /// Cria uma nova instancia de conta bancária
        /// </summary>
        /// <param name="agencia">numero da Agencia</param>
        /// <param name="digitoAgencia">Digito da Agencia</param>
        /// <param name="conta">numero da Conta</param>
        /// <param name="digitoConta">Digito da Conta</param>
        public ContaBancaria(string agencia, string digitoAgencia, string conta, string digitoConta)
        {
            this.Agencia = agencia;
            this.DigitoAgencia = digitoAgencia;
            this.Conta = conta;
            this.DigitoConta = digitoConta;
        }

        /// <summary>
        /// Cria uma nova instancia de conta bancária
        /// </summary>
        /// <param name="agencia">numero da Agencia</param>
        /// <param name="digitoAgencia">Digito da Agencia</param>
        /// <param name="conta">numero da Conta</param>
        /// <param name="digitoConta">Digito da Conta</param>
        /// <param name="operacaoConta">Operação da Conta</param>
        public ContaBancaria(string agencia, string digitoAgencia, string conta, string digitoConta, string operacaoConta)
        {
            this.Agencia = agencia;
            this.DigitoAgencia = digitoAgencia;
            this.Conta = conta;
            this.DigitoConta = digitoConta;
            this.OperacaConta = operacaoConta;
        }
        #endregion Constructors

        #region Properties
        /// <summary>
        /// Retorna o numero da Agencia.
        /// <remarks>
        /// Completar com zeros a esquerda quando necessario
        /// </remarks>
        /// </summary>
        public string Agencia {get; set;}

        /// <summary>
        /// Digito da Agencia
        /// </summary>
        public string DigitoAgencia { get; set;}

        /// <summary>
        /// numero da Conta Corrente
        /// </summary>
        public string Conta {get; set;}

        /// <summary>
        /// Digito da Conta Corrente
        /// </summary>
        public string DigitoConta { get; set; }
        
        /// <summary>
        /// Opreração da Conta Corrente
        /// </summary>
        public string OperacaConta { get; set; }
        #endregion Properties
    }
}
