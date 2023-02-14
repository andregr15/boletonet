namespace BoletoNet
{
    /// <summary>
    /// Representa o endereço do Cedente ou Sacado com todas as informacoes necessárias.
    /// </summary>
    public class Endereco
    {        
        private string _cep;

        /// <summary>
        /// Define o Logradouro
        /// <remarks>
        /// Exemplo: Rua, Av., Travessa...
        /// </remarks>
        /// </summary>        
        public string Logradouro
        {
            get
            {
                return End;
            }
        }

        /// <summary>
        /// Define o endereço completo
        /// <remarks>
        /// Exemplo: Barão do Amazonas
        /// </remarks>
        /// </summary>
        public string End { get; set; }

        /// <summary>
        /// Define o numero do endereço
        /// <remarks>
        /// Exemplo: 1025.
        /// </remarks>
        /// </summary>
        public string Numero { get; set; }

        /// <summary>
        /// Define o complemento
        /// <remarks>
        /// Exemplo: Ap, Apartamento, Bloco, Casa, etc.
        /// </remarks>
        /// </summary>
        public string Complemento { get; set; }

        /// <summary>
        /// Define o bairro
        /// <remarks>
        /// Exemplo: Centro.
        /// </remarks>
        /// </summary>
        public string Bairro { get; set; }

        /// <summary>
        /// Define o nome da Cidade
        /// <remarks>
        /// Exemplo: São Paulo
        /// </remarks>
        /// </summary>
        public string Cidade { get; set;}

        /// <summary>
        /// Define o Estado (UF)
        /// <remarks>
        /// Exemplo:
        /// SP - São Paulo
        /// SC - Santa Catarina
        /// *Utilizar apenas a sigla (UF)
        /// </remarks>
        /// </summary>
        public string UF { get; set;}

        /// <summary>
        /// Define o numero do CEP
        /// <remarks>
        /// O numero do CEP sera formatado automaticamente para remover pontos e traços
        /// </remarks>
        /// </summary>
        public string CEP
        {
            get
            {
                return _cep;
            }
            //Flavio(fhlviana@hotmail.com) - o metodo "Set" acontece menos vezes do que o get, por estimativa. Sendo assim, armazenar
            //sem o "." e o "-" faz com que o codigo tenda a executar os dois Replace uma vez só.
            //Consistência para evitar NullPointerException. (MarcielTorres)
            set
            {
                this._cep = !string.IsNullOrEmpty(value) ? value.Replace(".", "").Replace("-", "") : string.Empty;
            }
        }

        /// <summary>
        /// Define o E-Mail
        /// <remarks>
        /// Campo opcional, porem se informado nao há consistências para edereços de e-mails validos
        /// </remarks>
        /// </summary>
        public string Email { get; set; }

        public string EndComNumero {
            get {
                if (!string.IsNullOrEmpty(End) && !string.IsNullOrEmpty(Numero))
                    return string.Format("{0}, {1}", End.Trim(), Numero.Trim());

                return End;
            }
        }

        public string EndComNumeroEComplemento
        {
            get
            {
                var endComNumeroEComplemento = End;

                if (!string.IsNullOrEmpty(Numero))
                    endComNumeroEComplemento += " " + Numero;

                if (!string.IsNullOrEmpty(Complemento))
                    endComNumeroEComplemento += " " + Complemento;

                return endComNumeroEComplemento;
            }
        }
    }
}
