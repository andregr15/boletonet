namespace BoletoNet
{
    using System;
    #region EnumInstrucoes_Credisan enum
    public enum EnumInstrucoes_Credisan
    {
        AusenciaDeInstrucoes = 0,

        CobrarJuros = 1,

        Protestar3DiasUteis = 3,

        Protestar4DiasUteis = 4,

        Protestar5DiasUteis = 5,

        NaoProtestar = 7,

        Protestar10DiasUteis = 10,

        Protestar15DiasUteis = 15,

        Protestar20DiasUteis = 20,

        ConcederDescontoApenasAteDataEstipulada = 22,

        DevolverApos15DiasVencido = 42,

        DevolverApos30DiasVencido = 43
    }
    #endregion
    
    public class Instrucao_Credisan : AbstractInstrucao, IInstrucao
	{
		#region Construtores

		public Instrucao_Credisan()
		{
			try
			{
				this.Banco = new Banco(756);
			}
			catch (Exception ex)
			{
				throw new Exception("Erro ao carregar objeto", ex);
			}
		}

		public Instrucao_Credisan(int codigo)
		{
			this.carregar(codigo, 0, 0);
		}

		public Instrucao_Credisan(int codigo, int nrDias)
		{
			this.carregar(codigo, nrDias, (double)0.0);
		}

		public Instrucao_Credisan(int codigo, double percentualMultaDia)
		{
			this.carregar(codigo, 0, percentualMultaDia);
		}

        public Instrucao_Credisan(int codigo, int nrDias, double percentualMultaDia)
		{
			this.carregar(codigo, nrDias, percentualMultaDia);
		}

		#endregion

		#region Metodos Privados

        private void carregar(int idInstrucao, int nrDias, double percentualMultaDia)
        {
            try
            {
                this.Banco = new Banco_Banrisul();
                this.Valida();

                switch ((EnumInstrucoes_Credisan)idInstrucao)
                {
                    case EnumInstrucoes_Credisan.AusenciaDeInstrucoes:
                        break;
                    case EnumInstrucoes_Credisan.CobrarJuros:
                        this.Codigo = (int)EnumInstrucoes_Credisan.CobrarJuros;
                        this.Descricao = "Cobrar Juros";
                        break;
                    case EnumInstrucoes_Credisan.Protestar3DiasUteis:
                        this.Codigo = (int)EnumInstrucoes_Credisan.Protestar3DiasUteis;
                        this.Descricao = "Protestar 3 dias úteis após vencimento";
                        break;
                    case EnumInstrucoes_Credisan.Protestar4DiasUteis:
                        this.Codigo = (int)EnumInstrucoes_Credisan.Protestar4DiasUteis;
                        this.Descricao = "Protestar 4 dias úteis após vencimento";
                        break;
                    case EnumInstrucoes_Credisan.Protestar5DiasUteis:
                        this.Codigo = (int)EnumInstrucoes_Credisan.Protestar5DiasUteis;
                        this.Descricao = "Protestar 5 dias úteis após vencimento";
                        break;
                    case EnumInstrucoes_Credisan.NaoProtestar:
                        this.Codigo = (int)EnumInstrucoes_Credisan.NaoProtestar;
                        this.Descricao = "nao protestar";
                        break;
                    case EnumInstrucoes_Credisan.Protestar10DiasUteis:
                        this.Codigo = (int)EnumInstrucoes_Credisan.Protestar10DiasUteis;
                        this.Descricao = "Protestar 10 dias úteis após vencimento";
                        break;
                    case EnumInstrucoes_Credisan.Protestar15DiasUteis:
                        this.Codigo = (int)EnumInstrucoes_Credisan.Protestar15DiasUteis;
                        this.Descricao = "Protestar 15 dias úteis após vencimento";
                        break;
                    case EnumInstrucoes_Credisan.Protestar20DiasUteis:
                        this.Codigo = (int)EnumInstrucoes_Credisan.Protestar20DiasUteis;
                        this.Descricao = "Protestar 20 dias úteis após vencimento";
                        break;
                    case EnumInstrucoes_Credisan.ConcederDescontoApenasAteDataEstipulada:
                        this.Codigo = (int)EnumInstrucoes_Credisan.ConcederDescontoApenasAteDataEstipulada;
                        this.Descricao = "Conceder desconto só até a data estipulada";
                        break;
                    case EnumInstrucoes_Credisan.DevolverApos15DiasVencido:
                        this.Codigo = (int)EnumInstrucoes_Credisan.DevolverApos15DiasVencido;
                        this.Descricao = "Devolver após 15 dias vencido";
                        break;
                    case EnumInstrucoes_Credisan.DevolverApos30DiasVencido:
                        this.Codigo = (int)EnumInstrucoes_Credisan.DevolverApos30DiasVencido;
                        this.Descricao = "Devolver após 30 dias vencido";
                        break;
                    default:
                        this.Codigo = 0;
                        this.Descricao = " (Selecione) ";
                        break;

                }

                this.QuantidadeDias = nrDias;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao carregar objeto", ex);
            }
        }

		public override void Valida()
		{
	        base.Valida();
		}

		#endregion
	}
}