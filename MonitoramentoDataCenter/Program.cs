using System;
using System.Collections.Generic;
using System.Threading;

namespace MonitoramentoDataCenter
{
    // ============================================
    // Padrão OBSERVER aplicado em um Data Center
    // ============================================
    // Temos um sistema que simula servidores sendo monitorados,
    // e vários "observadores" (como equipes, logs e dashboards)
    // sendo notificados quando as métricas mudam.
    // ============================================

    // ------------------ Interfaces ------------------

    // interface de quem observa (Observer)
    public interface IObservadorMetricas
    {
        void Atualizar(string nomeServidor, string metrica, double valor, string status);
        string ObterNome();
    }

    // interface de quem é observado (Subject)
    public interface IServidorObservavel
    {
        void RegistrarObservador(IObservadorMetricas obs);
        void RemoverObservador(IObservadorMetricas obs);
        void NotificarObservadores(string metrica, double valor, string status);
    }

    // ------------------ Enums e classes auxiliares ------------------

    public enum StatusMetrica
    {
        Normal,
        Aviso,
        Critico,
        Emergencia
    }

    public class Metrica
    {
        public string Nome { get; set; }
        public double ValorAtual { get; set; }
        public double LimiteAviso { get; set; }
        public double LimiteCritico { get; set; }
        public double LimiteEmergencia { get; set; }
        public string Unidade { get; set; }

        public StatusMetrica ObterStatus()
        {
            if (ValorAtual >= LimiteEmergencia) return StatusMetrica.Emergencia;
            if (ValorAtual >= LimiteCritico) return StatusMetrica.Critico;
            if (ValorAtual >= LimiteAviso) return StatusMetrica.Aviso;
            return StatusMetrica.Normal;
        }
    }

    // ------------------ Classe Subject (Servidor) ------------------

    public class ServidorMonitorado : IServidorObservavel
    {
        private List<IObservadorMetricas> _observadores = new();
        private Dictionary<string, Metrica> _metricas;

        public string NomeServidor { get; private set; }
        public string Localizacao { get; private set; }

        public ServidorMonitorado(string nome, string local)
        {
            NomeServidor = nome;
            Localizacao = local;
            InicializarMetricas();
        }

        private void InicializarMetricas()
        {
            _metricas = new Dictionary<string, Metrica>
            {
                ["Temperatura"] = new Metrica { Nome = "Temperatura", ValorAtual = 25, LimiteAviso = 60, LimiteCritico = 75, LimiteEmergencia = 85, Unidade = "°C" },
                ["CPU"] = new Metrica { Nome = "Uso de CPU", ValorAtual = 30, LimiteAviso = 70, LimiteCritico = 85, LimiteEmergencia = 95, Unidade = "%" },
                ["Memoria"] = new Metrica { Nome = "Uso de Memória", ValorAtual = 40, LimiteAviso = 75, LimiteCritico = 88, LimiteEmergencia = 95, Unidade = "%" },
                ["Disco"] = new Metrica { Nome = "Uso de Disco", ValorAtual = 50, LimiteAviso = 80, LimiteCritico = 90, LimiteEmergencia = 97, Unidade = "%" }
            };
        }

        public void RegistrarObservador(IObservadorMetricas obs)
        {
            if (!_observadores.Contains(obs))
            {
                _observadores.Add(obs);
                Console.WriteLine($"[OK] Observador '{obs.ObterNome()}' adicionado ao {NomeServidor}");
            }
        }

        public void RemoverObservador(IObservadorMetricas obs)
        {
            if (_observadores.Remove(obs))
                Console.WriteLine($"[REMOVIDO] Observador '{obs.ObterNome()}' removido.");
        }

        public void NotificarObservadores(string metrica, double valor, string status)
        {
            foreach (var obs in _observadores)
                obs.Atualizar(NomeServidor, metrica, valor, status);
        }

        public void AtualizarMetrica(string nomeMetrica, double novoValor)
        {
            if (!_metricas.ContainsKey(nomeMetrica)) return;

            var m = _metricas[nomeMetrica];
            m.ValorAtual = novoValor;

            var status = m.ObterStatus();
            Console.WriteLine($"\n[{DateTime.Now:HH:mm:ss}] [{NomeServidor}] {m.Nome}: {novoValor:F1}{m.Unidade}");
            NotificarObservadores(m.Nome, novoValor, status.ToString());
        }

        public void ExibirStatus()
        {
            Console.WriteLine($"\n=== STATUS DO SERVIDOR: {NomeServidor} ===");
            Console.WriteLine($"Localização: {Localizacao}");

            foreach (var metrica in _metricas.Values)
            {
                string icone = metrica.ObterStatus() switch
                {
                    StatusMetrica.Normal => "✓",
                    StatusMetrica.Aviso => "⚠",
                    StatusMetrica.Critico => "❌",
                    StatusMetrica.Emergencia => "🔥",
                    _ => "?"
                };

                Console.WriteLine($"  {icone} {metrica.Nome}: {metrica.ValorAtual:F1}{metrica.Unidade} ({metrica.ObterStatus()})");
            }
        }
    }

    // ------------------ Observadores ------------------

    public class EquipeInfraestrutura : IObservadorMetricas
    {
        private string _nome;
        private List<string> _contatos = new() { "+55 11 98765-4321", "infra@datacenter.com" };

        public EquipeInfraestrutura(string nome) => _nome = nome;

        public void Atualizar(string nomeServidor, string metrica, double valor, string status)
        {
            if (status == "Critico" || status == "Emergencia")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n🚨 [{_nome}] ALERTA RECEBIDO!");
                Console.WriteLine($"Servidor: {nomeServidor} | {metrica}: {valor:F1} ({status})");
                Console.WriteLine($"Contatos notificados: {string.Join(", ", _contatos)}");
                Console.ResetColor();
            }
        }

        public string ObterNome() => _nome;
    }

    public class CentroOperacoesNOC : IObservadorMetricas
    {
        private int _idTicket = 1000;

        public void Atualizar(string nomeServidor, string metrica, double valor, string status)
        {
            if (status == "Normal") return;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n🎫 [NOC] Ticket criado #{_idTicket}");
            Console.WriteLine($"Servidor: {nomeServidor} | {metrica}: {valor:F1} | Prioridade: {ObterPrioridade(status)}");
            _idTicket++;
            Console.ResetColor();
        }

        private string ObterPrioridade(string status) => status switch
        {
            "Aviso" => "Baixa",
            "Critico" => "Alta",
            "Emergencia" => "Urgente",
            _ => "Normal"
        };

        public string ObterNome() => "Centro de Operações NOC";
    }

    public class SistemaLogging : IObservadorMetricas
    {
        private string _path = "C:\\Logs\\DataCenter\\";

        public void Atualizar(string nomeServidor, string metrica, double valor, string status)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"\n [LOGGING] Registrando evento...");
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {nomeServidor} - {metrica}: {valor:F1} ({status})");
            Console.WriteLine($"Arquivo destino: {_path}{nomeServidor}_{DateTime.Now:yyyyMMdd}.log");
            Console.ResetColor();
        }

        public string ObterNome() => "Sistema de Logging";
    }

    public class DashboardTempoReal : IObservadorMetricas
    {
        private Dictionary<string, Dictionary<string, double>> _dados = new();

        public void Atualizar(string nomeServidor, string metrica, double valor, string status)
        {
            if (!_dados.ContainsKey(nomeServidor))
                _dados[nomeServidor] = new();

            _dados[nomeServidor][metrica] = valor;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n [DASHBOARD] Atualizando {metrica} do {nomeServidor}");
            Console.WriteLine($"Progresso: {ObterBarra(valor)}");
            Console.ResetColor();
        }

        private string ObterBarra(double valor)
        {
            int qtd = (int)(valor / 10);
            string barra = new string('█', Math.Min(qtd, 10));
            return $"{barra.PadRight(10, '░')} {valor:F1}%";
        }

        public string ObterNome() => "Dashboard Tempo Real";
    }

    public class SistemaEmail : IObservadorMetricas
    {
        private List<string> _destinatarios;

        public SistemaEmail(List<string> destinatarios) => _destinatarios = destinatarios;

        public void Atualizar(string nomeServidor, string metrica, double valor, string status)
        {
            if (status == "Critico" || status == "Emergencia")
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"\n [EMAIL] Enviando alerta...");
                Console.WriteLine($"Para: {string.Join(", ", _destinatarios)}");
                Console.WriteLine($"Assunto: [ALERTA {status}] {nomeServidor} - {metrica}");
                Console.ResetColor();
            }
        }

        public string ObterNome() => "Sistema de Email";
    }

    // ------------------ Simulador ------------------

    public class SimuladorMetricas
    {
        private Random _rand = new();

        public void Simular(ServidorMonitorado servidor, int cenario)
        {
            switch (cenario)
            {
                case 1:
                    Console.WriteLine("\n CENÁRIO 1: Operação normal");
                    servidor.AtualizarMetrica("CPU", 55);
                    servidor.AtualizarMetrica("Temperatura", 45);
                    servidor.AtualizarMetrica("Memoria", 60);
                    break;

                case 2:
                    Console.WriteLine("\n CENÁRIO 2: Carga moderada (Avisos)");
                    servidor.AtualizarMetrica("CPU", 73);
                    servidor.AtualizarMetrica("Memoria", 78);
                    servidor.AtualizarMetrica("Temperatura", 65);
                    break;

                case 3:
                    Console.WriteLine("\n CENÁRIO 3: Situação crítica");
                    servidor.AtualizarMetrica("CPU", 89);
                    servidor.AtualizarMetrica("Memoria", 91);
                    servidor.AtualizarMetrica("Temperatura", 80);
                    break;

                case 4:
                    Console.WriteLine("\n CENÁRIO 4: Emergência!");
                    servidor.AtualizarMetrica("CPU", 97);
                    servidor.AtualizarMetrica("Memoria", 96);
                    servidor.AtualizarMetrica("Temperatura", 88);
                    servidor.AtualizarMetrica("Disco", 98);
                    break;
            }
        }
    }

    // ------------------ Programa principal ------------------

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("==========================================");
            Console.WriteLine(" SISTEMA DE MONITORAMENTO DE DATA CENTER ");
            Console.WriteLine(" Usando o padrão OBSERVER");
            Console.WriteLine("==========================================\n");

            var servidor = new ServidorMonitorado("SRV-APP-001", "Rack A12 - Data Center SP");

            //criando e registrando observadores
            var infra = new EquipeInfraestrutura("Equipe Infra 24/7");
            var noc = new CentroOperacoesNOC();
            var log = new SistemaLogging();
            var dash = new DashboardTempoReal();
            var email = new SistemaEmail(new List<string> { "gerente@datacenter.com", "coordenador@datacenter.com" });

            servidor.RegistrarObservador(infra);
            servidor.RegistrarObservador(noc);
            servidor.RegistrarObservador(log);
            servidor.RegistrarObservador(dash);
            servidor.RegistrarObservador(email);

            servidor.ExibirStatus();

            var sim = new SimuladorMetricas();

            Thread.Sleep(2000); sim.Simular(servidor, 1);
            Thread.Sleep(3000); sim.Simular(servidor, 2);
            Thread.Sleep(3000); sim.Simular(servidor, 3);
            Thread.Sleep(3000); sim.Simular(servidor, 4);

            Console.WriteLine("\n--- Fim da simulação ---");
            servidor.ExibirStatus();

            Console.WriteLine("\nPressione qualquer tecla para sair...");
            Console.ReadKey();
        }
    }
}
